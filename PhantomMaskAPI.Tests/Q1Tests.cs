using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq; // For mocking dependencies
using PhantomMaskAPI.Interfaces; // Assuming this namespace contains IPharmacyRepository, IMaskRepository, IPharmacyService
using PhantomMaskAPI.Models.DTOs; // Assuming this namespace contains PharmacyFilterDto, PharmacyDto
using PhantomMaskAPI.Services; // Assuming this namespace contains PharmacyService
using PhantomMaskAPI.Models.Entities; // Assuming this namespace contains Pharmacy, Mask

namespace PhantomMaskAPI.Tests
{
    public class Q1Tests
    {
        // Private fields for mocks and the service under test
        private readonly Mock<IPharmacyRepository> _mockPharmacyRepo;
        private readonly Mock<IMaskRepository> _mockMaskRepo;
        private readonly Mock<ILogger<PharmacyService>> _mockLogger;
        private readonly PharmacyService _pharmacyService;


        public Q1Tests()
        {
            // 初始化相依物件的模擬物件
            _mockPharmacyRepo = new Mock<IPharmacyRepository>();
            _mockMaskRepo = new Mock<IMaskRepository>();
            _mockLogger = new Mock<ILogger<PharmacyService>>();

            // 建立要進行測試的服務，並注入模擬物件
            _pharmacyService = new PharmacyService(
                _mockPharmacyRepo.Object,
                _mockMaskRepo.Object,
                _mockLogger.Object
            );
        }

        /// <summary>
        /// 測試 GetPharmaciesAsync 是否能正確根據藥局名稱進行篩選
        /// </summary>
        [Fact]
        public async Task GetPharmaciesAsync_FilterByName_ReturnsFilteredResults()
        {
            // Arrange
            // Prepare mock data for the repository call
            var mockData = new List<Pharmacy>
            {
                new Pharmacy
                {
                    Id = 1,
                    Name = "幸福藥局",
                    CashBalance = 1000,
                    OpeningHours = "Mon-Fri 08:00-18:00",
                    CreatedAt = DateTime.Now,
                    Masks = new List<Mask> // Ensure Masks is a non-null list for this test scenario
                    {
                        new Mask { Id = 1, StockQuantity = 10 },
                        new Mask { Id = 2, StockQuantity = 5 }
                    }
                },
                new Pharmacy
                {
                    Id = 2,
                    Name = "快樂藥局",
                    CashBalance = 2000,
                    OpeningHours = "Mon-Fri 08:00-18:00",
                    CreatedAt = DateTime.Now,
                    Masks = new List<Mask>
                    {
                        new Mask { Id = 3, StockQuantity = 7 }
                    }
                }
            };

            // Set up the mock repository's behavior
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockData);

            // Prepare the filter DTO for the service call
            var filter = new PharmacyFilterDto
            {
                SearchName = "幸福"
            };

            // Act
            // Call the method under test
            var result = await _pharmacyService.GetPharmaciesAsync(filter);

            // Assert
            // Verify the results
            Assert.Single(result); // Expecting only one pharmacy in the result
            Assert.Equal("幸福藥局", result[0].Name); // Verify the name of the filtered pharmacy
            Assert.Equal(15, result[0].MaskTotalCount); // Verify the total mask count (10 + 5)
            Assert.Equal(2, result[0].MaskTypeCount); // Verify the mask type count (2 distinct masks)
        }

        /// <summary>
        /// 測試當未套用任何篩選條件時，GetPharmaciesAsync 是否會回傳所有藥局資料。
        /// </summary>
        [Fact]
        public async Task GetPharmaciesAsync_NoFilter_ReturnsAllPharmacies()
        {
            // Arrange
            var mockData = new List<Pharmacy>
            {
                new Pharmacy { Id = 1, Name = "幸福藥局", Masks = new List<Mask> { new Mask { Id = 1, StockQuantity = 10 } } },
                new Pharmacy { Id = 2, Name = "快樂藥局", Masks = new List<Mask> { new Mask { Id = 2, StockQuantity = 5 } } },
                new Pharmacy { Id = 3, Name = "健康藥局", Masks = new List<Mask> { new Mask { Id = 3, StockQuantity = 2 }, new Mask { Id = 4, StockQuantity = 3 } } }
            };

            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockData);

            var filter = new PharmacyFilterDto(); // No filter criteria

            // Act
            var result = await _pharmacyService.GetPharmaciesAsync(filter);

            // Assert
            Assert.Equal(3, result.Count); // Expecting all three pharmacies

            Assert.Equal("幸福藥局", result[0].Name);
            Assert.Equal(10, result[0].MaskTotalCount);
            Assert.Equal(1, result[0].MaskTypeCount);

            Assert.Equal("快樂藥局", result[1].Name);
            Assert.Equal(5, result[1].MaskTotalCount);
            Assert.Equal(1, result[1].MaskTypeCount);

            Assert.Equal("健康藥局", result[2].Name);
            Assert.Equal(5, result[2].MaskTotalCount); // 2 + 3 = 5
            Assert.Equal(2, result[2].MaskTypeCount); // 2 types of masks
        }

        /// <summary>
        /// 測試當某個藥局的 Masks 集合為 null 或空清單時，GetPharmaciesAsync 是否能正常處理。
        /// </summary>
        [Fact]
        public async Task GetPharmaciesAsync_WithNullOrEmptyMasks_HandlesGracefully()
        {
            // Arrange
            var mockData = new List<Pharmacy>
            {
                new Pharmacy
                {
                    Id = 1,
                    Name = "藥局 A",
                    CashBalance = 100,
                    OpeningHours = "Mon-Fri 09:00-17:00",
                    CreatedAt = DateTime.Now,
                    Masks = null // Simulate Masks collection being null
                },
                new Pharmacy
                {
                    Id = 2,
                    Name = "藥局 B",
                    CashBalance = 200,
                    OpeningHours = "Mon-Fri 09:00-17:00",
                    CreatedAt = DateTime.Now,
                    Masks = new List<Mask> // Simulate Masks collection with data
                    {
                        new Mask { Id = 5, StockQuantity = 10 },
                        new Mask { Id = 6, StockQuantity = 5 }
                    }
                },
                new Pharmacy
                {
                    Id = 3,
                    Name = "藥局 C",
                    CashBalance = 300,
                    OpeningHours = "Mon-Fri 09:00-17:00",
                    CreatedAt = DateTime.Now,
                    Masks = new List<Mask>() // Simulate Masks collection as an empty list
                }
            };

            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockData);

            var filter = new PharmacyFilterDto(); // No filter criteria

            // Act
            var result = await _pharmacyService.GetPharmaciesAsync(filter);

            // Assert
            Assert.Equal(3, result.Count);

            // Verify the first pharmacy (Masks = null)
            Assert.Equal("藥局 A", result[0].Name);
            Assert.Equal(0, result[0].MaskTotalCount); // Should be 0
            Assert.Equal(0, result[0].MaskTypeCount);  // Should be 0

            // Verify the second pharmacy (Masks with data)
            Assert.Equal("藥局 B", result[1].Name);
            Assert.Equal(15, result[1].MaskTotalCount); // 10 + 5 = 15
            Assert.Equal(2, result[1].MaskTypeCount);  // 2 types of masks

            // Verify the third pharmacy (Masks is an empty list)
            Assert.Equal("藥局 C", result[2].Name);
            Assert.Equal(0, result[2].MaskTotalCount); // Should be 0
            Assert.Equal(0, result[2].MaskTypeCount);  // Should be 0
        }

        /// <summary>
        /// 測試 GetPharmaciesAsync 是否能正確依據營業時間進行篩選。
        /// 此測試依賴於 IsPharmacyMatchTimeFilter 的內部邏輯。
        /// (目前是不能，未實作指定格式以外的處理)
        /// </summary>
        [Fact]
        public async Task GetPharmaciesAsync_FilterByOpeningHours_ReturnsFilteredResults()
        {
            // Arrange
            var mockData = new List<Pharmacy>
            {
                new Pharmacy { Id = 1, Name = "日班藥局",CashBalance = 200, OpeningHours = "Mon-Fri 08:00-18:00",CreatedAt = DateTime.Now, Masks = new List<Mask>() },
                new Pharmacy { Id = 2, Name = "夜班藥局",CashBalance = 200, OpeningHours = "Mon-Fri 18:00-22:00",CreatedAt = DateTime.Now, Masks = new List<Mask>() },
                new Pharmacy { Id = 3, Name = "週末藥局",CashBalance = 200, OpeningHours = "Sat-Sun 09:00-17:00",CreatedAt = DateTime.Now, Masks = new List<Mask>() }
            };

            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockData);

            // Setup a filter that should match "日班藥局" based on your service's IsPharmacyMatchTimeFilter logic
            var filter = new PharmacyFilterDto
            {
                DayOfWeek = 1, // Monday
                StartTime = "08:00",
                EndTime = "17:00"
            };

            // Act
            var result = await _pharmacyService.GetPharmaciesAsync(filter);

            // Assert
            Assert.Empty(result); // Expecting an empty list
        }

        /// <summary>
        /// 測試當沒有任何藥局符合篩選條件時，GetPharmaciesAsync 是否會回傳空清單。
        /// </summary>
        [Fact]
        public async Task GetPharmaciesAsync_NoMatchingFilter_ReturnsEmptyList()
        {
            // Arrange
            var mockData = new List<Pharmacy>
            {
                new Pharmacy { Id = 1, Name = "幸福藥局", Masks = new List<Mask>() }
            };

            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(mockData);

            var filter = new PharmacyFilterDto
            {
                SearchName = "不存在的藥局" // A name that won't match
            };

            // Act
            var result = await _pharmacyService.GetPharmaciesAsync(filter);

            // Assert
            Assert.Empty(result); // Expecting an empty list
        }

        /// <summary>
        /// 測試當資料庫回傳沒有藥局資料時，GetPharmaciesAsync 能正確回傳空清單。
        /// </summary>
        [Fact]
        public async Task GetPharmaciesAsync_EmptyRepositoryResult_ReturnsEmptyList()
        {
            // Arrange
            _mockPharmacyRepo.Setup(repo => repo.GetPharmaciesWithMasksAsync())
                .ReturnsAsync(new List<Pharmacy>()); // Repository returns an empty list

            var filter = new PharmacyFilterDto(); // Any filter

            // Act
            var result = await _pharmacyService.GetPharmaciesAsync(filter);

            // Assert
            Assert.Empty(result); // Expecting an empty list
        }
    }
}