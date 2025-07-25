using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PhantomMaskAPI.Interfaces; // 假設此命名空間包含 IMaskRepository, IPharmacyRepository, IPharmacyService
using PhantomMaskAPI.Models.DTOs; // 假設此命名空間包含 PharmacyDto, MaskDto
using PhantomMaskAPI.Services; // 假設此命名空間包含 PharmacyService
using PhantomMaskAPI.Models.Entities; // 假設此命名空間包含 Pharmacy, Mask

namespace PhantomMaskAPI.Tests
{
    public class Q3Tests
    {
        // 私有欄位，用於模擬物件和被測試的服務
        private readonly Mock<IPharmacyRepository> _mockPharmacyRepo;
        private readonly Mock<IMaskRepository> _mockMaskRepo; // 儘管 Q3 服務未直接使用，但 PharmacyService 建構子可能需要
        private readonly Mock<ILogger<PharmacyService>> _mockLogger; // PharmacyService 的日誌記錄器
        private readonly PharmacyService _pharmacyService; // 被測試的服務

        /// <summary>
        /// 測試類別的建構子。在每個測試方法執行前都會運行。
        /// </summary>
        public Q3Tests()
        {
            // 初始化模擬物件
            _mockPharmacyRepo = new Mock<IPharmacyRepository>();
            _mockMaskRepo = new Mock<IMaskRepository>(); // 如果 PharmacyService 建構子需要，則初始化
            _mockLogger = new Mock<ILogger<PharmacyService>>();

            // 初始化被測試的服務，注入模擬的依賴項
            // 根據您的 PharmacyService 建構子調整
            _pharmacyService = new PharmacyService(
                _mockPharmacyRepo.Object,
                _mockMaskRepo.Object, // 傳遞 _mockMaskRepo.Object 如果 PharmacyService 建構子需要
                _mockLogger.Object
            );
        }

        // --- Helper Methods for Test Data ---
        private List<Pharmacy> GetMockPharmacies()
        {
            return new List<Pharmacy>
            {
                new Pharmacy
                {
                    Id = 1, Name = "藥局 A", CashBalance = 1000m, OpeningHours = "Mon-Fri 09:00-18:00", CreatedAt = DateTime.Now,
                    Masks = new List<Mask>
                    {
                        new Mask { Id = 1, Name = "N95", Price = 50.0m, StockQuantity = 50 },
                        new Mask { Id = 2, Name = "外科", Price = 10.0m, StockQuantity = 150 }
                    } // 2 種類型, 總庫存: 200, 價格範圍: [10, 50]
                },
                new Pharmacy
                {
                    Id = 2, Name = "藥局 B", CashBalance = 2000m, OpeningHours = "Mon-Sun 08:00-20:00", CreatedAt = DateTime.Now,
                    Masks = new List<Mask>
                    {
                        new Mask { Id = 3, Name = "N95", Price = 60.0m, StockQuantity = 10 },
                        new Mask { Id = 4, Name = "布", Price = 5.0m, StockQuantity = 30 }
                    } // 2 種類型, 總庫存: 40, 價格範圍: [5, 60]
                },
                new Pharmacy
                {
                    Id = 3, Name = "藥局 C", CashBalance = 3000m, OpeningHours = "Mon-Sat 10:00-19:00", CreatedAt = DateTime.Now,
                    Masks = new List<Mask>
                    {
                        new Mask { Id = 5, Name = "N95", Price = 55.0m, StockQuantity = 20 },
                        new Mask { Id = 6, Name = "外科", Price = 12.0m, StockQuantity = 80 },
                        new Mask { Id = 7, Name = "兒童", Price = 15.0m, StockQuantity = 120 }
                    } // 3 種類型, 總庫存: 220, 價格範圍: [12, 55]
                },
                new Pharmacy
                {
                    Id = 4, Name = "藥局 D", CashBalance = 500m, OpeningHours = "Mon-Fri 09:00-17:00", CreatedAt = DateTime.Now,
                    Masks = new List<Mask>
                    {
                        new Mask { Id = 8, Name = "醫用", Price = 8.0m, StockQuantity = 100 }
                    } // 1 種類型, 總庫存: 100, 價格範圍: [8, 8]
                },
                new Pharmacy
                {
                    Id = 5, Name = "藥局 E", CashBalance = 1500m, OpeningHours = "Mon-Fri 09:00-18:00", CreatedAt = DateTime.Now,
                    Masks = new List<Mask>() // 0 種類型, 總庫存: 0, 價格範圍: [] (無口罩)
                },
                 new Pharmacy
                {
                    Id = 6, Name = "藥局 F (無口罩物件)", CashBalance = 1500m, OpeningHours = "Mon-Fri 09:00-18:00", CreatedAt = DateTime.Now,
                    Masks = null // 0 種類型, 總庫存: 0 (經過服務端 null 處理後), 價格範圍: [] (無口罩)
                }
            };
        }

        // --- Test Cases ---

        /// <summary>
        /// 測試當同時指定 minStockThreshold 和 maxStockThreshold 且為包含式時，是否正確篩選藥局。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 >= 40 且 <= 100
        /// 預期：藥局 B (40), 藥局 D (100)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MinMaxInclusive_ReturnsFilteredResults()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int minStockThreshold = 40; // 總庫存 >= 40
            int maxStockThreshold = 100; // 總庫存 <= 100
            bool isInclusive = true;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // 藥局 B, D

            // 驗證特定藥局是否在結果中
            Assert.Contains(result, p => p.Name == "藥局 B");
            Assert.Contains(result, p => p.Name == "藥局 D");

            // 驗證 DTO 轉換的準確性
            var pharmacyB = result.First(p => p.Name == "藥局 B");
            Assert.Equal(2, pharmacyB.MaskTypeCount); // 種類數量不變
            Assert.Equal(40, pharmacyB.MaskTotalCount); // 總庫存數量
        }

        /// <summary>
        /// 測試當同時指定 minStockThreshold 和 maxStockThreshold 且為不包含式時，是否正確篩選藥局。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 > 100 且 < 220
        /// 預期：藥局 A (200)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MinMaxExclusive_ReturnsFilteredResults()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int minStockThreshold = 100; // 總庫存 > 100
            int maxStockThreshold = 220; // 總庫存 < 220
            bool isInclusive = false;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count); // 藥局 A

            Assert.Contains(result, p => p.Name == "藥局 A");
        }

        /// <summary>
        /// 測試只指定 minStockThreshold 且為包含式時，是否正確篩選藥局。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 >= 100
        /// 預期：藥局 A (200), 藥局 C (220), 藥局 D (100)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MinOnlyInclusive_ReturnsFilteredResults()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int minStockThreshold = 100; // 總庫存 >= 100
            int? maxStockThreshold = null;
            bool isInclusive = true;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // 藥局 A, C, D

            Assert.Contains(result, p => p.Name == "藥局 A");
            Assert.Contains(result, p => p.Name == "藥局 C");
            Assert.Contains(result, p => p.Name == "藥局 D");
        }

        /// <summary>
        /// 測試只指定 maxStockThreshold 且為不包含式時，是否正確篩選藥局。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 < 100
        /// 預期：藥局 B (40), 藥局 E (0), 藥局 F (0)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MaxOnlyExclusive_ReturnsFilteredResults()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int? minStockThreshold = null;
            int maxStockThreshold = 100; // 總庫存 < 100
            bool isInclusive = false;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // 藥局 B, E, F

            Assert.Contains(result, p => p.Name == "藥局 B");
            Assert.Contains(result, p => p.Name == "藥局 E");
            Assert.Contains(result, p => p.Name == "藥局 F (無口罩物件)");
        }

        /// <summary>
        /// 測試當 minStockThreshold 和 maxStockThreshold 相同且為包含式時。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 >= 40 且 <= 40 (即 == 40)
        /// 預期：藥局 B (40)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MinEqualsMaxInclusive_ReturnsCorrectResults()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int minStockThreshold = 40;
            int maxStockThreshold = 40;
            bool isInclusive = true;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count); // 藥局 B
            Assert.Contains(result, p => p.Name == "藥局 B");
        }

        /// <summary>
        /// 測試當 minStockThreshold 和 maxStockThreshold 相同且為不包含式時，應返回空列表。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 > 40 且 < 40 (不可能成立)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MinEqualsMaxExclusive_ReturnsEmptyList()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int minStockThreshold = 40;
            int maxStockThreshold = 40;
            bool isInclusive = false;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空
        }

        /// <summary>
        /// 測試當 minStockThreshold 大於 maxStockThreshold 時，應返回空列表。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 總庫存量 > 100 且 < 40 (不可能成立)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_MinGreaterThanMax_ReturnsEmptyList()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int minStockThreshold = 100;
            int maxStockThreshold = 40;
            bool isInclusive = true; // 包含或不包含都應為空

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空
        }

        /// <summary>
        /// 測試當 minStockThreshold 和 maxStockThreshold 都為 null 時，服務應返回所有藥局。
        /// 價格篩選: 不進行 (minPrice=0, maxPrice=0)
        /// 庫存篩選: 不進行
        /// 預期：所有藥局 (包含 Mask 為 null 的藥局 F，因為服務已做 null 處理)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_NoThresholds_ReturnsAllPharmacies()
        {
            // Arrange
            var mockPharmacies = GetMockPharmacies();
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockPharmacies);

            decimal minPrice = 0; // 不進行價格篩選
            decimal maxPrice = 0; // 不進行價格篩選
            int? minStockThreshold = null;
            int? maxStockThreshold = null;
            bool isInclusive = false; // 不相關

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            // 由於服務已修正 p.Masks?.Sum() ?? 0 的 null 處理，所有藥局都應被包含。
            Assert.Equal(6, result.Count); // 藥局 A, B, C, D, E, F

            Assert.Contains(result, p => p.Name == "藥局 A");
            Assert.Contains(result, p => p.Name == "藥局 B");
            Assert.Contains(result, p => p.Name == "藥局 C");
            Assert.Contains(result, p => p.Name == "藥局 D");
            Assert.Contains(result, p => p.Name == "藥局 E");
            Assert.Contains(result, p => p.Name == "藥局 F (無口罩物件)");

            // 特別檢查藥局 E (無口罩) 和 藥局 F (口罩物件為 null) 的 DTO 轉換
            var pharmacyE = result.First(p => p.Name == "藥局 E");
            Assert.Equal(0, pharmacyE.MaskTypeCount);
            Assert.Equal(0, pharmacyE.MaskTotalCount);

            var pharmacyF = result.First(p => p.Name == "藥局 F (無口罩物件)");
            Assert.Equal(0, pharmacyF.MaskTypeCount);
            Assert.Equal(0, pharmacyF.MaskTotalCount);
        }

        /// <summary>
        /// 測試當 Repository 拋出異常時，服務是否會將該異常向上傳播。
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("模擬資料庫連線錯誤");
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _pharmacyService.GetPharmaciesByStockCriteriaAsync(0, 0, null, null, false)); // 使用 0,0 避免價格過濾影響

            Assert.Equal(expectedException.Message, actualException.Message);

            // 驗證日誌記錄器：服務層沒有捕獲此異常，所以預期日誌記錄器不會在服務層被呼叫 LogError。
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never
            );
        }

        /// <summary>
        /// 這個測試案例特別設計來演示當程式碼的實際行為與測試的預期不符時，
        /// Assert.Equal 如何導致測試「失敗」。
        /// 
        /// 預期行為：服務會返回 6 個藥局 (因為我們設定了 6 個模擬資料，且服務已處理 null)。
        /// 測試斷言：我們錯誤地斷言服務應該只返回 5 個藥局。
        /// 結果：當執行這個測試時，它將會「失敗」，因為實際返回的 6 個藥局不等於預期的 5 個。
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_DemonstratesAssertEqualFailure_TestWillFail()
        {
            // Arrange (準備)
            var mockPharmacies = GetMockPharmacies();
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockPharmacies);

            // Act (執行)
            // 使用 minPrice=0, maxPrice=0 確保不進行價格篩選，專注於庫存篩選的數量不符
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(0, 0, null, null, false);

            // Assert (斷言)
            // 這裡故意斷言錯誤的預期值，以演示測試失敗
            // 預期：5，實際：6。 Assert.Equal 會失敗。
            Assert.Equal(5, result.Count);
        }

        // --- 新增的價格篩選測試案例 ---

        /// <summary>
        /// 測試價格範圍篩選，只依價格篩選，不考慮庫存。
        /// 價格篩選: minPrice = 10, maxPrice = 15
        /// 藥局 A (價格 [10, 50]), 藥局 B (價格 [5, 60]), 藥局 C (價格 [12, 15, 55]), 藥局 D (價格 [8]), 藥局 E (無口罩), 藥局 F (無口罩)
        /// 預期：藥局 A (有 10), 藥局 C (有 12, 15)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_FiltersByPriceRangeOnly_ReturnsCorrectPharmacies()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 10.0m;
            decimal maxPrice = 15.0m;
            int? minStockThreshold = null; // 不進行庫存篩選
            int? maxStockThreshold = null; // 不進行庫存篩選
            bool isInclusive = false;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // 藥局 A, C

            Assert.Contains(result, p => p.Name == "藥局 A");
            Assert.Contains(result, p => p.Name == "藥局 C");
            Assert.DoesNotContain(result, p => p.Name == "藥局 B"); // B 的口罩價格不在 10-15 之間
            Assert.DoesNotContain(result, p => p.Name == "藥局 D"); // D 的口罩價格不在 10-15 之間
            Assert.DoesNotContain(result, p => p.Name == "藥局 E"); // E 沒有口罩
            Assert.DoesNotContain(result, p => p.Name == "藥局 F (無口罩物件)"); // F 沒有口罩
        }

        /// <summary>
        /// 測試同時結合價格和庫存篩選。
        /// 價格篩選: minPrice = 50, maxPrice = 60
        /// 庫存篩選: 總庫存量 >= 40 且 <= 200 (包含式)
        /// 預期步驟：
        /// 1. 價格篩選後剩餘：藥局 A (有 50), 藥局 B (有 60), 藥局 C (有 55)
        /// 2. 再進行庫存篩選：
        ///    藥局 A (總庫存 200) -> 符合
        ///    藥局 B (總庫存 40) -> 符合
        ///    藥局 C (總庫存 220) -> 不符合 (>200)
        /// 最終預期：藥局 A, 藥局 B
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_FiltersByPriceAndStock_ReturnsCorrectPharmacies()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 50.0m;
            decimal maxPrice = 60.0m;
            int minStockThreshold = 40; // 總庫存 >= 40
            int maxStockThreshold = 200; // 總庫存 <= 200
            bool isInclusive = true;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // 藥局 A, B

            Assert.Contains(result, p => p.Name == "藥局 A");
            Assert.Contains(result, p => p.Name == "藥局 B");
            Assert.DoesNotContain(result, p => p.Name == "藥局 C");
        }

        /// <summary>
        /// 測試當價格範圍沒有任何藥局的口罩符合時，是否返回空列表。
        /// 價格篩選: minPrice = 100, maxPrice = 200 (沒有任何口罩在此價格範圍內)
        /// 庫存篩選: 不進行
        /// 預期：空列表
        /// </summary>
        [Fact]
        public async Task GetPharmaciesByStockCriteriaAsync_PriceRangeNoMatchingMasks_ReturnsEmptyList()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(GetMockPharmacies());

            decimal minPrice = 100.0m;
            decimal maxPrice = 200.0m;
            int? minStockThreshold = null;
            int? maxStockThreshold = null;
            bool isInclusive = false;

            // Act
            var result = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空
        }
    }
}