using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PhantomMaskAPI.Interfaces; // 假設此命名空間包含 IPurchaseRepository, IYourService (例如 IPurchaseService)
using PhantomMaskAPI.Models.DTOs; // 假設此命名空間包含 TopSpenderDto
using PhantomMaskAPI.Models.Entities; // 假設此命名空間包含 Purchase
using PhantomMaskAPI.Services; // 假設此命名空間包含 YourService (例如 PurchaseService)

namespace PhantomMaskAPI.Tests
{
    public class Q4Tests
    {
        private readonly Mock<IPurchaseRepository> _mockPurchaseRepo;
        private readonly Mock<IMaskRepository> _mockMaskRepo; // 新增
        private readonly Mock<IUserRepository> _mockUserRepo; // 新增
        private readonly Mock<IPharmacyRepository> _mockPharmacyRepo; // 新增
        private readonly Mock<ILogger<PurchaseService>> _mockLogger;
        private readonly PurchaseService _purchaseService;

        /// <summary>
        /// 測試類別的建構子。在每個測試方法執行前都會運行。
        /// </summary>
        public Q4Tests()
        {
            // 初始化所有模擬物件
            _mockPurchaseRepo = new Mock<IPurchaseRepository>();
            _mockMaskRepo = new Mock<IMaskRepository>();      // 初始化
            _mockUserRepo = new Mock<IUserRepository>();      // 初始化
            _mockPharmacyRepo = new Mock<IPharmacyRepository>(); // 初始化
            _mockLogger = new Mock<ILogger<PurchaseService>>();

            // 初始化被測試的服務，注入所有模擬的依賴項
            _purchaseService = new PurchaseService(
                _mockPurchaseRepo.Object,
                _mockMaskRepo.Object,
                _mockUserRepo.Object,
                _mockPharmacyRepo.Object,
                _mockLogger.Object
            );
        }

        // --- Helper Methods for Test Data ---
        private List<Purchase> GetMockPurchases()
        {
            // 確保所有 DateTimeKind 都是 Utc，以符合服務中的指定
            return new List<Purchase>
            {
                // User1 的購買記錄
                new Purchase { Id = 1, UserName = "User1", TransactionAmount = 100m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 5, 10, 0, 0), DateTimeKind.Utc) },
                new Purchase { Id = 2, UserName = "User1", TransactionAmount = 50m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 10, 11, 0, 0), DateTimeKind.Utc) },
                new Purchase { Id = 3, UserName = "User1", TransactionAmount = 75m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 15, 12, 0, 0), DateTimeKind.Utc) },
                // User2 的購買記錄
                new Purchase { Id = 4, UserName = "User2", TransactionAmount = 200m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 3, 9, 0, 0), DateTimeKind.Utc) },
                new Purchase { Id = 5, UserName = "User2", TransactionAmount = 20m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 8, 14, 0, 0), DateTimeKind.Utc) },
                // User3 的購買記錄 (與 User2 總花費相同，用於測試排序)
                new Purchase { Id = 6, UserName = "User3", TransactionAmount = 220m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 7, 16, 0, 0), DateTimeKind.Utc) },
                // User4 的購買記錄 (在範圍外)
                new Purchase { Id = 7, UserName = "User4", TransactionAmount = 300m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 2, 1, 8, 0, 0), DateTimeKind.Utc) },
                // User5 的購買記錄 (只有一筆)
                new Purchase { Id = 8, UserName = "User5", TransactionAmount = 10m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 20, 9, 0, 0), DateTimeKind.Utc) }
            };
        }

        // --- Test Cases ---

        /// <summary>
        /// 測試在指定日期範圍內，能正確獲取前 N 名消費用戶。
        /// 預期：User2 (220), User1 (225), User3 (220) -> top2 會是 User1, User2 (因為排序是 OrderByDescending)
        /// 註：如果 TotalSpent 相同，OrderByDescending 是穩定的，會保留原始順序 (在 GroupBy 和 Select 後的順序可能不確定，但 Take 會取前 N 個)。
        /// 通常，當 TotalSpent 相同時，會再加一個次要排序條件，例如 UserName。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_ValidDateRangeAndTopN_ReturnsCorrectTopSpenders()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            int topN = 3;

            // 模擬 GetPurchasesByDateRangeAsync 返回在範圍內的購買記錄
            // 排除 User4 的記錄 (日期在 2/1)
            var purchasesInRange = GetMockPurchases().Where(p => p.TransactionDateTime >= startDate.ToUniversalTime() && p.TransactionDateTime <= endDate.ToUniversalTime()).ToList();
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(purchasesInRange);

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // 應該返回 3 個用戶

            // 驗證排序和數據準確性
            Assert.Equal("User1", result[0].UserName); // User1: 100+50+75 = 225
            Assert.Equal(225m, result[0].TotalSpent);
            Assert.Equal(3, result[0].TotalPurchases);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 5, 10, 0, 0), DateTimeKind.Utc), result[0].FirstPurchase);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 15, 12, 0, 0), DateTimeKind.Utc), result[0].LastPurchase);

            Assert.Equal("User2", result[1].UserName); // User2: 200+20 = 220
            Assert.Equal(220m, result[1].TotalSpent);
            Assert.Equal(2, result[1].TotalPurchases);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 3, 9, 0, 0), DateTimeKind.Utc), result[1].FirstPurchase);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 8, 14, 0, 0), DateTimeKind.Utc), result[1].LastPurchase);

            Assert.Equal("User3", result[2].UserName); // User3: 220
            Assert.Equal(220m, result[2].TotalSpent);
            Assert.Equal(1, result[2].TotalPurchases);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 7, 16, 0, 0), DateTimeKind.Utc), result[2].FirstPurchase);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 7, 16, 0, 0), DateTimeKind.Utc), result[2].LastPurchase);

            // 驗證日誌呼叫
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"取得前 {topN} 名消費用戶")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        /// <summary>
        /// 測試當 topN 大於所有用戶數時，應返回所有用戶。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_TopNGreaterThanTotalUsers_ReturnsAllUsers()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            int topN = 10; // 大於實際用戶數 (4 個有購買的用戶: User1, User2, User3, User5)

            var purchasesInRange = GetMockPurchases().Where(p => p.TransactionDateTime >= startDate.ToUniversalTime() && p.TransactionDateTime <= endDate.ToUniversalTime()).ToList();
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(purchasesInRange);

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count); // 應該返回所有 4 個用戶 (User1, User2, User3, User5)
            // 驗證排序：User1 (225), User2 (220), User3 (220), User5 (10)
            Assert.Equal("User1", result[0].UserName);
            Assert.Equal("User2", result[1].UserName);
            Assert.Equal("User3", result[2].UserName);
            Assert.Equal("User5", result[3].UserName);
        }

        /// <summary>
        /// 測試在指定日期範圍內沒有任何購買記錄時，應返回空列表。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_NoPurchasesInDateRange_ReturnsEmptyList()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1); // 選擇一個沒有購買記錄的日期範圍
            var endDate = new DateTime(2026, 1, 31);
            int topN = 5;

            // 模擬 GetPurchasesByDateRangeAsync 返回空列表
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Purchase>());

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空列表
        }

        /// <summary>
        /// 測試當 topN 為 0 時，應返回空列表。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_TopNIsZero_ReturnsEmptyList()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            int topN = 0;

            var purchasesInRange = GetMockPurchases().Where(p => p.TransactionDateTime >= startDate.ToUniversalTime() && p.TransactionDateTime <= endDate.ToUniversalTime()).ToList();
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(purchasesInRange);

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空列表
        }

        /// <summary>
        /// 測試當 topN 為負數時，應返回空列表。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_TopNIsNegative_ReturnsEmptyList()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            int topN = -1;

            var purchasesInRange = GetMockPurchases().Where(p => p.TransactionDateTime >= startDate.ToUniversalTime() && p.TransactionDateTime <= endDate.ToUniversalTime()).ToList();
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(purchasesInRange);

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空列表
        }

        /// <summary>
        /// 測試當 Repository 拋出異常時，服務是否會將該異常向上傳播。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            int topN = 5;
            var expectedException = new InvalidOperationException("模擬資料庫連線錯誤");

            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _purchaseService.GetTopSpendersAsync(startDate, endDate, topN));

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
        /// 測試日期區間為一天，且購買時間恰好在邊界上。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_SingleDayRange_IncludesBoundaryPurchases()
        {
            var inputStartDate = new DateTime(2025, 1, 10);
            var inputEndDate = new DateTime(2025, 1, 10, 23, 59, 59);
            int topN = 5;

            var serviceCallStartDate = DateTime.SpecifyKind(inputStartDate, DateTimeKind.Utc);
            var serviceCallEndDate = DateTime.SpecifyKind(inputEndDate, DateTimeKind.Utc);

            var allPurchases = new List<Purchase>
            {
                new Purchase { Id = 1, UserName = "BoundaryUser", TransactionAmount = 100m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 10, 0, 0, 0), DateTimeKind.Utc) },
                new Purchase { Id = 2, UserName = "BoundaryUser", TransactionAmount = 50m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 10, 23, 59, 59), DateTimeKind.Utc) },
                new Purchase { Id = 3, UserName = "OtherUser", TransactionAmount = 200m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 9, 23, 59, 59), DateTimeKind.Utc) },
                new Purchase { Id = 4, UserName = "OtherUser", TransactionAmount = 300m, TransactionDateTime = DateTime.SpecifyKind(new DateTime(2025, 1, 11, 0, 0, 0), DateTimeKind.Utc) }
            };

            // *** 修正：確保 Mock Repository 的返回數據是根據 serviceCallStartDate/EndDate 篩選的 ***
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(
                It.Is<DateTime>(dt => dt == serviceCallStartDate), 
                It.Is<DateTime>(dt => dt == serviceCallEndDate)    
            ))
            .ReturnsAsync(allPurchases.Where(p => p.TransactionDateTime >= serviceCallStartDate && p.TransactionDateTime <= serviceCallEndDate).ToList());

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(inputStartDate, inputEndDate, topN);


            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // 只有 BoundaryUser

            var topSpender = result.First();
            Assert.Equal("BoundaryUser", topSpender.UserName);
            Assert.Equal(150m, topSpender.TotalSpent);
            Assert.Equal(2, topSpender.TotalPurchases);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 10, 0, 0, 0), DateTimeKind.Utc), topSpender.FirstPurchase);
            Assert.Equal(DateTime.SpecifyKind(new DateTime(2025, 1, 10, 23, 59, 59), DateTimeKind.Utc), topSpender.LastPurchase);
        }

        /// <summary>
        /// 測試開始日期晚於結束日期時，應返回空列表。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_StartDateGreaterThanEndDate_ReturnsEmptyList()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 31); // 晚於 endDate
            var endDate = new DateTime(2025, 1, 1);   // 早於 startDate
            int topN = 5;

            // *** 修正：精確模擬 Repository 在收到無效日期範圍時的行為 ***
            // 當服務傳遞 startDate > endDate 給 Repository 時，Repository 理應返回空列表
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(
                It.Is<DateTime>(s => s == startDate), // 驗證傳入的 startDate
                It.Is<DateTime>(e => e == endDate)    // 驗證傳入的 endDate
            ))
            .ReturnsAsync(new List<Purchase>()); // <<-- 核心修正：模擬 Repository 返回空列表

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // 預期為空列表，因為 Repository 返回了空列表
        }

        /// <summary>
        /// 測試當有多個用戶的總花費金額相同時，OrderByDescending 的穩定性。
        /// 期望：在總花費相同的情況下，依據 GroupBy 和 Select 後的順序 (通常是鍵的順序)
        /// User1: 225
        /// User2: 220
        /// User3: 220
        /// User5: 10
        /// 預期 User2 和 User3 的順序取決於 GroupBy 的內部實現，但因為我們沒有次要排序，
        /// 通常是它們在原始數據中的第一個出現的順序，或其 UserName 的字母順序等。
        /// 在這裡，為了測試穩定性，我們預期會是 User2 在 User3 之前，因為在 `GetMockPurchases` 中 User2 的購買記錄在 User3 之前。
        /// </summary>
        [Fact]
        public async Task GetTopSpendersAsync_SameTotalSpent_MaintainsConsistentOrder()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);
            int topN = 3; // 抓取前 3 名，會包含 User2 和 User3

            var purchasesInRange = GetMockPurchases().Where(p => p.TransactionDateTime >= startDate.ToUniversalTime() && p.TransactionDateTime <= endDate.ToUniversalTime()).ToList();
            _mockPurchaseRepo.Setup(repo => repo.GetPurchasesByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(purchasesInRange);

            // Act
            var result = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            // 驗證 User1 是第一名，然後 User2 和 User3 按照他們在原始數據中被處理的相對順序出現
            // (通常 GroupBy 是根據 Key 順序，所以這裡仍預期 User2 在 User3 前面)
            Assert.Equal("User1", result[0].UserName);
            Assert.Equal("User2", result[1].UserName); // TotalSpent = 220
            Assert.Equal("User3", result[2].UserName); // TotalSpent = 220

            Assert.Equal(225m, result[0].TotalSpent);
            Assert.Equal(220m, result[1].TotalSpent);
            Assert.Equal(220m, result[2].TotalSpent);
        }
    }
}