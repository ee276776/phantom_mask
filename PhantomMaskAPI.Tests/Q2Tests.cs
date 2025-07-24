using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PhantomMaskAPI.Interfaces; // 假設此命名空間包含 IMaskRepository, IPharmacyService
using PhantomMaskAPI.Models.DTOs; // 假設此命名空間包含 MaskDto
using PhantomMaskAPI.Services; // 假設此命名空間包含 PharmacyService (其中有 GetPharmacyMasksAsync)
using PhantomMaskAPI.Models.Entities; // 假設此命名空間包含 Mask, Pharmacy

namespace PhantomMaskAPI.Tests
{
    public class Q2Tests
    {
        // 私有欄位，用於模擬物件和被測試的服務
        private readonly Mock<IMaskRepository> _mockMaskRepo;
        private readonly Mock<IPharmacyRepository> _mockPharmacyRepo; // 儘管 GetPharmacyMasksAsync 未直接使用，但 PharmacyService 建構子可能需要
        private readonly Mock<ILogger<PharmacyService>> _mockLogger; // PharmacyService 的日誌記錄器
        private readonly PharmacyService _pharmacyService; // 被測試的服務

       
        public Q2Tests()
        {
            // 初始化依賴項的模擬物件
            _mockMaskRepo = new Mock<IMaskRepository>();
            _mockPharmacyRepo = new Mock<IPharmacyRepository>(); 
            _mockLogger = new Mock<ILogger<PharmacyService>>();

            // 初始化被測試的服務，注入模擬的依賴項
         
            _pharmacyService = new PharmacyService(
                _mockPharmacyRepo.Object, 
                _mockMaskRepo.Object,
                _mockLogger.Object
            );
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 能否為給定藥局 ID 返回所有口罩。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_ValidId_ReturnsAllMasks()
        {
            // Arrange (準備)
            int pharmacyId = 1;
            var mockMasks = new List<Mask>
            {
                new Mask { Id = 101, Name = "N95口罩", Price = 50.0m, StockQuantity = 100, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 102, Name = "外科口罩", Price = 10.0m, StockQuantity = 200, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 103, Name = "布口罩", Price = 5.0m, StockQuantity = 50, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } }
            };

            // 設定模擬 Repository 的行為
            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(mockMasks);

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, null, null); // 不進行排序

            // Assert (斷言)
            Assert.NotNull(result); // 確認結果不是 null
            Assert.Equal(3, result.Count); // 確認返回了 3 個口罩
            Assert.Equal("N95口罩", result[0].Name); // 如果沒有排序，則檢查基於原始模擬資料的順序
            Assert.Equal("A藥局", result[0].PharmacyName); // 確保 PharmacyName 正確映射
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 能否正確按照名稱升序排序口罩。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_SortByNameAsc_ReturnsSortedMasks()
        {
            // Arrange (準備)
            int pharmacyId = 1;
            var mockMasks = new List<Mask>
            {
                new Mask { Id = 101, Name = "Z口罩", Price = 50.0m, StockQuantity = 100, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 102, Name = "A口罩", Price = 10.0m, StockQuantity = 200, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 103, Name = "M口罩", Price = 5.0m, StockQuantity = 50, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } }
            };

            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(mockMasks);

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, "name", "asc");

            // Assert (斷言)
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("A口罩", result[0].Name); // 檢查排序結果
            Assert.Equal("M口罩", result[1].Name);
            Assert.Equal("Z口罩", result[2].Name);
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 能否正確按照價格降序排序口罩。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_SortByPriceDesc_ReturnsSortedMasks()
        {
            // Arrange (準備)
            int pharmacyId = 1;
            var mockMasks = new List<Mask>
            {
                new Mask { Id = 101, Name = "口罩甲", Price = 50.0m, StockQuantity = 100, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 102, Name = "口罩乙", Price = 10.0m, StockQuantity = 200, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 103, Name = "口罩丙", Price = 5.0m, StockQuantity = 50, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } }
            };

            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(mockMasks);

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, "price", "desc");

            // Assert (斷言)
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("口罩甲", result[0].Name); // 價格 50
            Assert.Equal("口罩乙", result[1].Name); // 價格 10
            Assert.Equal("口罩丙", result[2].Name); // 價格 5
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 能否正確按照庫存數量升序排序口罩。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_SortByStockAsc_ReturnsSortedMasks()
        {
            // Arrange (準備)
            int pharmacyId = 1;
            var mockMasks = new List<Mask>
            {
                new Mask { Id = 101, Name = "口罩 A", Price = 10, StockQuantity = 100, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 102, Name = "口罩 B", Price = 20, StockQuantity = 50, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 103, Name = "口罩 C", Price = 30, StockQuantity = 200, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } }
            };

            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(mockMasks);

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, "stock", "asc");

            // Assert (斷言)
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("口罩 B", result[0].Name); // 庫存 50
            Assert.Equal("口罩 A", result[1].Name); // 庫存 100
            Assert.Equal("口罩 C", result[2].Name); // 庫存 200
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 在未找到任何口罩時是否返回空列表。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_NoMasksFound_ReturnsEmptyList()
        {
            // Arrange (準備)
            int pharmacyId = 99; // 一個沒有口罩的藥局 ID
            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(new List<Mask>()); // Repository 返回一個空列表

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, null, null);

            // Assert (斷言)
            Assert.NotNull(result); // 確認結果不是 null
            Assert.Empty(result); // 預期返回一個空列表
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 在映射到 MaskDto 時，是否能優雅地處理 Mask 的 Pharmacy 物件為 null 的情況
        /// (例如，PharmacyName 應該為 null 或空字串，而不是拋出 NullReferenceException)。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_WithNullPharmacy_HandlesGracefully()
        {
            // Arrange (準備)
            int pharmacyId = 1;
            var mockMasks = new List<Mask>
            {
                // 模擬 Mask 的 Pharmacy 導航屬性為 null 的情況
                new Mask { Id = 101, Name = "問題口罩", Price = 10.0m, StockQuantity = 10, PharmacyId = pharmacyId, Pharmacy = null }
            };

            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(mockMasks);

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, null, null);

            // Assert (斷言)
            Assert.NotNull(result);
            Assert.Single(result); // 確認只返回一個口罩
            Assert.Equal("問題口罩", result[0].Name);
            // 關鍵：斷言 PharmacyName 在沒有 NRE 的情況下被處理。
            // 根據 MaskDto 中 PharmacyName 屬性的類型和預設值，它應該是 null 或空字串。
            Assert.Null(result[0].PharmacyName); // 假設 MaskDto 中的 PharmacyName 是 string? (可為 null 的字串)
        }

        /// <summary>
        /// 測試 GetPharmacyMasksAsync 對於無效的 sortBy 值，是否會使用預設排序 (名稱升序)。
        /// </summary>
        [Fact]
        public async Task GetPharmacyMasksAsync_InvalidSortBy_DefaultsToNameAsc()
        {
            // Arrange (準備)
            int pharmacyId = 1;
            var mockMasks = new List<Mask>
            {
                new Mask { Id = 101, Name = "Z口罩", Price = 50.0m, StockQuantity = 100, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 102, Name = "A口罩", Price = 10.0m, StockQuantity = 200, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } },
                new Mask { Id = 103, Name = "M口罩", Price = 5.0m, StockQuantity = 50, PharmacyId = pharmacyId, Pharmacy = new Pharmacy { Id = pharmacyId, Name = "A藥局" } }
            };

            _mockMaskRepo.Setup(repo => repo.GetMasksByPharmacyIdAsync(pharmacyId))
                .ReturnsAsync(mockMasks);

            // Act (執行)
            var result = await _pharmacyService.GetPharmacyMasksAsync(pharmacyId, "invalid", "desc"); // 無效的 sortBy，但嘗試降序

            // Assert (斷言)
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            // 應按照名稱升序排序 (預設)
            Assert.Equal("A口罩", result[0].Name);
            Assert.Equal("M口罩", result[1].Name);
            Assert.Equal("Z口罩", result[2].Name);
        }

       
    }
}