using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Services;
using System;
namespace PhantomMaskAPI.Tests
{
    public class Q8Tests
    {
        private readonly Mock<IMaskRepository> _mockMaskRepository;
        private readonly Mock<IPharmacyRepository> _mockPharmacyRepository;
        private readonly Mock<IRelevanceService> _mockRelevanceService;
        private readonly Mock<ILogger<SearchService>> _mockLogger;
        private readonly SearchService _searchService;

        public Q8Tests()
        {
            _mockMaskRepository = new Mock<IMaskRepository>();
            _mockPharmacyRepository = new Mock<IPharmacyRepository>();
            _mockRelevanceService = new Mock<IRelevanceService>();
            _mockLogger = new Mock<ILogger<SearchService>>();

            _searchService = new SearchService(
                _mockPharmacyRepository.Object,
                _mockMaskRepository.Object,
                _mockRelevanceService.Object,
                _mockLogger.Object
            );
        }

        // 輔助方法來建立模擬的 Mask 和 Pharmacy 實體
        private Mask GetMockMask(int id, string name, int stock = 0, decimal price = 0) =>
            new Mask { Id = id, Name = name, StockQuantity = stock, Price = price };

        private Pharmacy GetMockPharmacy(int id, string name, decimal balance = 0) =>
            new Pharmacy { Id = id, Name = name, CashBalance = balance };

        // --- 測試案例 ---

        /// <summary>
        /// Q8.1: 測試查詢能匹配到口罩和藥局，且結果能正確排序和過濾。
        /// </summary>
        [Fact]
        public async Task SearchByRelevanceAsync_ReturnsMasksAndPharmaciesSortedByScore()
        {
            // Arrange
            string query = "N95";
            var masks = new List<Mask>
        {
            GetMockMask(1, "N95口罩"),
            GetMockMask(2, "醫療口罩")
        };
            var pharmacies = new List<Pharmacy>
        {
            GetMockPharmacy(101, "大安藥局"),
            GetMockPharmacy(102, "健康N95藥局")
        };

            _mockMaskRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(masks);
            _mockPharmacyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(pharmacies);

            // 設定 CalculateRelevanceScoreInternal 的返回值
            // Mask 1: N95口罩 -> 高分
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 1 && d.Type == "mask")))
                .Returns(0.9);
            // Mask 2: 醫療口罩 -> 中等分
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 2 && d.Type == "mask")))
                .Returns(0.5);
            // Pharmacy 101: 大安藥局 -> 較低分
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 101 && d.Type == "pharmacy")))
                .Returns(0.3);
            // Pharmacy 102: 健康N95藥局 -> 最高分
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 102 && d.Type == "pharmacy")))
                .Returns(1.0); // 最高分

            // Act
            var results = await _searchService.SearchByRelavanceAsync(query);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(4, results.Count); // 應該有4個結果，因為所有分數都>0

            // 驗證排序
            Assert.Equal(102, results[0].ID); // 健康N95藥局 (1.0)
            Assert.Equal("pharmacy", results[0].Type);
            Assert.Equal(1.0, results[0].RelevanceScore);

            Assert.Equal(1, results[1].ID); // N95口罩 (0.9)
            Assert.Equal("mask", results[1].Type);
            Assert.Equal(0.9, results[1].RelevanceScore);

            Assert.Equal(2, results[2].ID); // 醫療口罩 (0.5)
            Assert.Equal("mask", results[2].Type);
            Assert.Equal(0.5, results[2].RelevanceScore);

            Assert.Equal(101, results[3].ID); // 大安藥局 (0.3)
            Assert.Equal("pharmacy", results[3].Type);
            Assert.Equal(0.3, results[3].RelevanceScore);

            // 驗證 GetAllAsync 至少被呼叫一次 (因為每個 repository 都只有一種類型，所以會是 Once)
            _mockMaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockPharmacyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);

            // 驗證 CalculateRelevanceScoreInternal 被呼叫的次數 (mask 2個 + pharmacy 2個 = 4次)
            _mockRelevanceService.Verify(s => s.CalculateRelevanceScoreInternal(
                It.IsAny<string>(), It.IsAny<RelevanceDto>()), Times.Exactly(4));

            // 驗證沒有日誌被記錄
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }

        /// <summary>
        /// Q8.2: 測試分數為 0 的結果會被正確過濾掉。
        /// </summary>
        [Fact]
        public async Task SearchByRelevanceAsync_FiltersOutZeroScoreResults()
        {
            // Arrange
            string query = "不相關";
            var masks = new List<Mask>
        {
            GetMockMask(1, "N95口罩"), // 預計分數 > 0
            GetMockMask(2, "醫療口罩")  // 預計分數 = 0
        };
            var pharmacies = new List<Pharmacy>
        {
            GetMockPharmacy(101, "大安藥局"), // 預計分數 > 0
            GetMockPharmacy(102, "健康藥局")  // 預計分數 = 0
        };

            _mockMaskRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(masks);
            _mockPharmacyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(pharmacies);

            // 設定 CalculateRelevanceScoreInternal 的返回值
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 1 && d.Type == "mask")))
                .Returns(0.8);
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 2 && d.Type == "mask")))
                .Returns(0.0); // 應被過濾

            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 101 && d.Type == "pharmacy")))
                .Returns(0.6);
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 102 && d.Type == "pharmacy")))
                .Returns(0.0); // 應被過濾

            // Act
            var results = await _searchService.SearchByRelavanceAsync(query);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count); // 只有2個分數 > 0 的結果

            // 驗證只包含分數大於 0 的結果並正確排序
            Assert.Equal(1, results[0].ID); // N95口罩 (0.8)
            Assert.Equal("mask", results[0].Type);
            Assert.Equal(0.8, results[0].RelevanceScore);

            Assert.Equal(101, results[1].ID); // 大安藥局 (0.6)
            Assert.Equal("pharmacy", results[1].Type);
            Assert.Equal(0.6, results[1].RelevanceScore);

            _mockMaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockPharmacyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockRelevanceService.Verify(s => s.CalculateRelevanceScoreInternal(
                It.IsAny<string>(), It.IsAny<RelevanceDto>()), Times.Exactly(4)); // 總共計算了4次分數

            // 驗證沒有日誌被記錄
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }

        /// <summary>
        /// Q8.3: 測試空查詢字符串應返回空列表。
        /// </summary>
        [Fact]
        public async Task SearchByRelevanceAsync_EmptyQueryReturnsEmptyList()
        {
            // Arrange
            string query = string.Empty;
            var masks = new List<Mask> { GetMockMask(1, "N95口罩") };
            var pharmacies = new List<Pharmacy> { GetMockPharmacy(101, "大安藥局") };

            _mockMaskRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(masks);
            _mockPharmacyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(pharmacies);

            // 設定 CalculateRelevanceScoreInternal 對任何空查詢都返回 0
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.IsAny<RelevanceDto>()))
                .Returns(0.0);

            // Act
            var results = await _searchService.SearchByRelavanceAsync(query);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results); // 所有分數都為0，應被過濾

            _mockMaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockPharmacyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockRelevanceService.Verify(s => s.CalculateRelevanceScoreInternal(
                It.IsAny<string>(), It.IsAny<RelevanceDto>()), Times.Exactly(2)); // 兩個項目都計算了分數

            // 驗證沒有日誌被記錄
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }

        /// <summary>
        /// Q8.4: 測試沒有任何匹配結果時應返回空列表。
        /// </summary>
        [Fact]
        public async Task SearchByRelevanceAsync_NoMatchingResultsReturnsEmptyList()
        {
            // Arrange
            string query = "完全不相關的查詢";
            var masks = new List<Mask> { GetMockMask(1, "N95口罩") };
            var pharmacies = new List<Pharmacy> { GetMockPharmacy(101, "大安藥局") };

            _mockMaskRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(masks);
            _mockPharmacyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(pharmacies);

            // 設定 CalculateRelevanceScoreInternal 對所有查詢都返回 0
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.IsAny<RelevanceDto>()))
                .Returns(0.0);

            // Act
            var results = await _searchService.SearchByRelavanceAsync(query);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results); // 所有分數都為0，應被過濾

            _mockMaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockPharmacyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockRelevanceService.Verify(s => s.CalculateRelevanceScoreInternal(
                It.IsAny<string>(), It.IsAny<RelevanceDto>()), Times.Exactly(2));

            // 驗證沒有日誌被記錄
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }

        /// <summary>
        /// Q8.5: 測試當 MaskRepository 拋出異常時，應記錄錯誤並繼續處理 Pharmacy。
        /// </summary>
        [Fact]
        public async Task SearchByRelevanceAsync_MaskRepositoryThrowsException_ContinuesAndLogsError()
        {
            // Arrange
            string query = "測試";
            var masks = new List<Mask>(); // 不需要實際的口罩，因為會拋異常
            var pharmacies = new List<Pharmacy>
        {
            GetMockPharmacy(101, "大安藥局"), // 預計正常處理
        };

            _mockMaskRepository.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new InvalidOperationException("Mask 資料庫連線失敗"));
            _mockPharmacyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(pharmacies);

            // 設定 CalculateRelevanceScoreInternal 的返回值
            _mockRelevanceService.Setup(s => s.CalculateRelevanceScoreInternal(
                query, It.Is<RelevanceDto>(d => d.ID == 101 && d.Type == "pharmacy")))
                .Returns(0.7);

            // Act
            var results = await _searchService.SearchByRelavanceAsync(query);

            // Assert
            Assert.NotNull(results);
            Assert.Single(results); // 只有一個藥局結果

            // 驗證結果正確 (只有藥局)
            Assert.Equal(101, results[0].ID);
            Assert.Equal("pharmacy", results[0].Type);
            Assert.Equal(0.7, results[0].RelevanceScore);

            // 驗證 MaskRepository.GetAllAsync 被呼叫一次並拋出異常
            _mockMaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            // 驗證 PharmacyRepository.GetAllAsync 仍然被呼叫一次
            _mockPharmacyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            // 驗證 CalculateRelevanceScoreInternal 只為藥局被呼叫
            _mockRelevanceService.Verify(s => s.CalculateRelevanceScoreInternal(
                It.IsAny<string>(), It.IsAny<RelevanceDto>()), Times.Once);

            // 驗證錯誤日誌被記錄一次
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("搜尋口罩時發生錯誤.")),
                    It.IsAny<InvalidOperationException>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
            // 驗證沒有其他類型的日誌被記錄
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l != LogLevel.Error), // 非錯誤級別的日誌
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }


        /// <summary>
        /// Q8.6: 測試當 PharmacyRepository 拋出異常時，應記錄錯誤並返回已處理的 Mask 結果。
        /// </summary>
        [Fact]
        public async Task SearchByRelevanceAsync_PharmacyRepositoryThrowsException_ReturnsMaskResultsAndLogsError()
        {
            // Arrange
            string query = "測試";

            var masks = new List<Mask>
    {
        GetMockMask(1, "N95口罩") // 預期被正常處理
    };

            // mask 成功回傳
            _mockMaskRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(masks);

            // pharmacy 故意噴錯
            _mockPharmacyRepository
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new InvalidOperationException("Pharmacy 資料庫連線失敗"));

            // relevance 計算只為口罩設定
            _mockRelevanceService
                .Setup(s => s.CalculateRelevanceScoreInternal(
                    query,
                    It.Is<RelevanceDto>(d => d.ID == 1 && d.Type == "mask")))
                .Returns(0.9);

            // Act
            var results = await _searchService.SearchByRelavanceAsync(query);

            // Assert
            Assert.NotNull(results);
            Assert.Single(results); // 只有一筆口罩結果

            Assert.Equal(1, results[0].ID);
            Assert.Equal("mask", results[0].Type);
            Assert.Equal(0.9, results[0].RelevanceScore);

            // 驗證 mask 被呼叫一次
            _mockMaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            // 驗證 pharmacy 被呼叫一次（會拋異常）
            _mockPharmacyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);

            // 只有口罩會進到 relevance score
            _mockRelevanceService.Verify(s =>
                s.CalculateRelevanceScoreInternal(It.IsAny<string>(), It.IsAny<RelevanceDto>()),
                Times.Once);

            // 驗證 error log 有記錄到藥局錯誤
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("搜尋藥局時發生錯誤.")),
                    It.IsAny<InvalidOperationException>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );

            // 其他非錯誤級別的 log 不該出現
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l != LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }
    }
}