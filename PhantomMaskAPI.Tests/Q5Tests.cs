using Microsoft.Extensions.Logging;
using Moq;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Services;

namespace PhantomMaskAPI.Tests
{
    public class Q5Tests
    {
        private readonly Mock<IPurchaseRepository> _mockPurchaseRepo;
        private readonly Mock<IMaskRepository> _mockMaskRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPharmacyRepository> _mockPharmacyRepo;
        private readonly Mock<ILogger<PurchaseService>> _mockLogger;
        private readonly PurchaseService _purchaseService;

        public Q5Tests()
        {
            _mockPurchaseRepo = new Mock<IPurchaseRepository>();
            _mockMaskRepo = new Mock<IMaskRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPharmacyRepo = new Mock<IPharmacyRepository>();
            _mockLogger = new Mock<ILogger<PurchaseService>>();

            _purchaseService = new PurchaseService(
                _mockPurchaseRepo.Object,
                _mockMaskRepo.Object,
                _mockUserRepo.Object,
                _mockPharmacyRepo.Object,
                _mockLogger.Object
            );
        }

        // --- Helper Methods for Test Data ---

        // 建立模擬用戶
        private User GetMockUser(int id, string name, decimal balance) =>
            new User { Id = id, Name = name, CashBalance = balance };

        // 建立模擬口罩
        private Mask GetMockMask(int id, string name, int stock, decimal price) =>
            new Mask { Id = id, Name = name, StockQuantity = stock, Price = price };

        // 建立模擬藥局
        private Pharmacy GetMockPharmacy(int id, string name, decimal balance) =>
            new Pharmacy { Id = id, Name = name, CashBalance = balance };

        // 建立模擬購買記錄 (由 CreatePurchaseAsync_ 返回的 Entity)
        private Purchase GetMockPurchaseEntity(
            int id, int userId, string userName, int pharmacyId, string pharmacyName, // userId 和 pharmacyId 只是輔助傳入，Purchase 實體本身沒有這些欄位
            int maskId, string maskName, int quantity, decimal amount, DateTime transactionTime) =>
            new Purchase
            {
                Id = id,
                // *** 這些屬性與您提供的 Purchase 類定義完全一致 ***
                UserName = userName,
                PharmacyName = pharmacyName,
                MaskName = maskName,
                TransactionQuantity = quantity,
                TransactionAmount = amount,
                TransactionDateTime = transactionTime,
                CreatedAt = transactionTime.AddSeconds(-5)
            };


        // --- Test Cases ---

        /// <summary>
        /// S1. 測試用戶不存在時，批量購買應直接失敗。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_UserDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var bulkPurchaseDto = new BulkPurchaseDto_ { UserId = 999, Purchases = new List<BulkPurchaseItemDto_>() };
            _mockUserRepo.Setup(repo => repo.UserExistsAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("用戶 999 不存在", result.Message);
            Assert.Contains("用戶 999 不存在", result.Errors);
            Assert.Empty(result.CompletedPurchases);
            Assert.Equal(0m, result.TotalAmount);
            _mockUserRepo.Verify(repo => repo.UserExistsAsync(999), Times.Once);
            // 驗證沒有其他 Repository 被呼叫
            _mockMaskRepo.VerifyNoOtherCalls();
            _mockPharmacyRepo.VerifyNoOtherCalls();
            _mockPurchaseRepo.VerifyNoOtherCalls();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("批量購買處理時發生錯誤") || v.ToString().Contains("批量購買結果")), // Ensure no successful log
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Never // Should not log success or general error
            );
        }

        /// <summary>
        /// S2. 測試用戶餘額不足以支付任何一個項目時，應直接失敗。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_UserCashBalanceInsufficient_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var maskId1 = 101;
            var pharmacyId1 = 201;
            var quantity1 = 2;
            var price1 = 50m; // Total for item1: 100

            var bulkPurchaseDto = new BulkPurchaseDto_
            {
                UserId = userId,
                Purchases = new List<BulkPurchaseItemDto_>
                {
                    new BulkPurchaseItemDto_ { MaskId = maskId1, PharmacyId = pharmacyId1, Quantity = quantity1 }
                }
            };

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, "TestUser", 90m)); // 餘額不足 (應付 100)
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskId1)).ReturnsAsync(GetMockMask(maskId1, "MaskA", 10, price1)); // 庫存充足

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("💰 買家餘額不足，僅有 90.00", result.Message);
            Assert.Contains("💰 買家餘額不足，僅有 90.00", result.Errors);
            Assert.Empty(result.CompletedPurchases);
            Assert.Equal(0m, result.TotalAmount);

            _mockUserRepo.Verify(repo => repo.UserExistsAsync(userId), Times.Once);
            _mockUserRepo.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockMaskRepo.Verify(repo => repo.GetByIdAsync(maskId1), Times.Once);
            // 驗證沒有進行任何購買或更新操作
            _mockPurchaseRepo.VerifyNoOtherCalls();
            _mockPharmacyRepo.VerifyNoOtherCalls();
            _mockUserRepo.Verify(repo => repo.UpdateUserBalanceByIdAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        }

        /// <summary>
        /// S3. 測試所有購買項目庫存不足，應返回失敗且無完成購買。
        /// </summary>
        /// <summary>
        /// S3. 測試所有購買項目庫存不足，應返回失敗且無完成購買。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_AllItemsInsufficientStock_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var maskId1 = 101;
            var pharmacyId1 = 201;
            var quantity1 = 5;
            var maskStock1 = 2; // 庫存不足

            var bulkPurchaseDto = new BulkPurchaseDto_
            {
                UserId = userId,
                Purchases = new List<BulkPurchaseItemDto_>
                {
                    new BulkPurchaseItemDto_ { MaskId = maskId1, PharmacyId = pharmacyId1, Quantity = quantity1 }
                }
            };

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, "TestUser", 500m)); // 餘額充足
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskId1)).ReturnsAsync(GetMockMask(maskId1, "MaskA", maskStock1, 50m)); // 庫存不足

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.False(result.Success); // 因為沒有任何成功購買
            Assert.Equal("所有購買都失敗了", result.Message);
            Assert.Contains($"❌ 藥局 {pharmacyId1} 的口罩 {maskId1} 庫存不足（剩餘: {maskStock1}, 需求: {quantity1}）", result.Errors);
            Assert.Empty(result.CompletedPurchases);
            Assert.Equal(0m, result.TotalAmount); // 沒有成功購買，所以總金額為 0

            
        }

        /// <summary>
        /// S4. 測試單一成功購買的完整流程 (Happy Path)。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_SingleSuccessfulPurchase_ReturnsSuccess()
        {
            // Arrange
            var userId = 1;
            var userName = "TestUser";
            var maskId = 101;
            var maskName = "MaskA";
            var maskStock = 10;
            var maskPrice = 50m;
            var pharmacyId = 201;
            var pharmacyName = "PharmacyX";
            var pharmacyBalance = 1000m;
            var quantity = 2;
            var purchaseAmount = quantity * maskPrice; // 100m
            var userBalance = 200m; // 足夠支付

            var bulkPurchaseDto = new BulkPurchaseDto_
            {
                UserId = userId,
                Purchases = new List<BulkPurchaseItemDto_>
                {
                    new BulkPurchaseItemDto_ { MaskId = maskId, PharmacyId = pharmacyId, Quantity = quantity }
                }
            };

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, userName, userBalance));
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskId)).ReturnsAsync(GetMockMask(maskId, maskName, maskStock, maskPrice));
            _mockPharmacyRepo.Setup(repo => repo.GetByIdAsync(pharmacyId)).ReturnsAsync(GetMockPharmacy(pharmacyId, pharmacyName, pharmacyBalance));

            var mockPurchaseEntity = GetMockPurchaseEntity(1, userId, userName, pharmacyId, pharmacyName, maskId, maskName, quantity, purchaseAmount, DateTime.Now);
            _mockPurchaseRepo.Setup(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskId && item.Quantity == quantity && item.PharmacyId == pharmacyId),
                userId
            )).ReturnsAsync(mockPurchaseEntity);

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"成功完成 1 筆購買，總金額: ${purchaseAmount:F2}", result.Message);
            Assert.Empty(result.Errors);
            Assert.Single(result.CompletedPurchases);
            Assert.Equal(purchaseAmount, result.TotalAmount);

            var completedPurchase = result.CompletedPurchases.First();
            Assert.Equal(mockPurchaseEntity.Id, completedPurchase.Id);
            Assert.Equal(mockPurchaseEntity.UserName, completedPurchase.UserName);
            Assert.Equal(mockPurchaseEntity.MaskName, completedPurchase.MaskName);
            Assert.Equal(mockPurchaseEntity.TransactionQuantity, completedPurchase.TransactionQuantity);
            Assert.Equal(mockPurchaseEntity.TransactionAmount, completedPurchase.TransactionAmount);

            // 驗證 Repository 方法被正確呼叫
            _mockUserRepo.Verify(repo => repo.UserExistsAsync(userId), Times.Once);
            _mockUserRepo.Verify(repo => repo.GetByIdAsync(userId), Times.Once); // 獲取用戶餘額
            _mockMaskRepo.Verify(repo => repo.GetByIdAsync(maskId), Times.Exactly(2)); // 預計算和實際處理階段各一次
            _mockPharmacyRepo.Verify(repo => repo.GetByIdAsync(pharmacyId), Times.Once);
            _mockPurchaseRepo.Verify(repo => repo.CreatePurchaseAsync_(It.IsAny<BulkPurchaseItemDto_>(), It.IsAny<int>()), Times.Once);

            // 驗證餘額和庫存更新
            _mockUserRepo.Verify(repo => repo.UpdateUserBalanceByIdAsync(userId, userBalance - purchaseAmount), Times.Once);
            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(pharmacyId, pharmacyBalance + purchaseAmount), Times.Once);
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(maskId, maskStock - quantity), Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"用戶ID {userId} 批量購買結果: 1/1 成功")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        /// <summary>
        /// S5. 測試批量購買，部分項目成功，部分項目因庫存不足失敗。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_PartialSuccess_SomeItemsInsufficientStock()
        {
            // Arrange
            var userId = 1;
            var userName = "TestUser";
            var userBalance = 500m;

            var maskA = GetMockMask(101, "MaskA", 5, 50m); // 充足 (購買 2 個)
            var maskB = GetMockMask(102, "MaskB", 3, 30m); // 不足 (購買 5 個)
            var maskC = GetMockMask(103, "MaskC", 10, 20m); // 充足 (購買 3 個)

            var pharmacyX = GetMockPharmacy(201, "PharmacyX", 1000m);
            var pharmacyY = GetMockPharmacy(202, "PharmacyY", 500m);

            var bulkPurchaseDto = new BulkPurchaseDto_
            {
                UserId = userId,
                Purchases = new List<BulkPurchaseItemDto_>
                {
                    new BulkPurchaseItemDto_ { MaskId = maskA.Id, PharmacyId = pharmacyX.Id, Quantity = 2 }, // 成功 (50*2 = 100)
                    new BulkPurchaseItemDto_ { MaskId = maskB.Id, PharmacyId = pharmacyY.Id, Quantity = 5 }, // 庫存不足 (3 < 5)
                    new BulkPurchaseItemDto_ { MaskId = maskC.Id, PharmacyId = pharmacyX.Id, Quantity = 3 }  // 成功 (20*3 = 60)
                }
            };

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, userName, userBalance));

            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskA.Id)).ReturnsAsync(maskA);
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskB.Id)).ReturnsAsync(maskB);
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskC.Id)).ReturnsAsync(maskC);

            _mockPharmacyRepo.Setup(repo => repo.GetByIdAsync(pharmacyX.Id)).ReturnsAsync(pharmacyX);
            _mockPharmacyRepo.Setup(repo => repo.GetByIdAsync(pharmacyY.Id)).ReturnsAsync(pharmacyY);

            // Mock successful purchases
            var purchaseA_Entity = GetMockPurchaseEntity(1001, userId, userName, pharmacyX.Id, pharmacyX.Name, maskA.Id, maskA.Name, 2, 100m, DateTime.Now);
            var purchaseC_Entity = GetMockPurchaseEntity(1002, userId, userName, pharmacyX.Id, pharmacyX.Name, maskC.Id, maskC.Name, 3, 60m, DateTime.Now.AddSeconds(1));

            _mockPurchaseRepo.Setup(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskA.Id), userId))
                .ReturnsAsync(purchaseA_Entity);
            _mockPurchaseRepo.Setup(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskC.Id), userId))
                .ReturnsAsync(purchaseC_Entity);

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.True(result.Success); // 至少有一個成功
            Assert.Equal($"成功完成 2 筆購買，總金額: ${160m:F2}", result.Message); // 100 + 60
            Assert.Equal(2, result.Errors.Count); // 預期有兩個錯誤訊息,預計算&實際操作階段各一次
            Assert.Contains($"❌ 藥局 {pharmacyY.Id} 的口罩 {maskB.Id} 庫存不足（剩餘: {maskB.StockQuantity}, 需求: 5）", result.Errors);
            Assert.Equal(2, result.CompletedPurchases.Count);
            Assert.Equal(160m, result.TotalAmount);

            // 驗證正確的 Repo 互動
            _mockUserRepo.Verify(repo => repo.GetByIdAsync(userId), Times.Once); // 餘額檢查
            _mockMaskRepo.Verify(repo => repo.GetByIdAsync(maskA.Id), Times.Exactly(2)); // 預計算和實際處理
            _mockMaskRepo.Verify(repo => repo.GetByIdAsync(maskB.Id), Times.Exactly(2)); // 預計算和實際處理
            _mockMaskRepo.Verify(repo => repo.GetByIdAsync(maskC.Id), Times.Exactly(2)); // 預計算和實際處理

            _mockPurchaseRepo.Verify(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskA.Id), userId), Times.Once);
            _mockPurchaseRepo.Verify(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskC.Id), userId), Times.Once);
            _mockPurchaseRepo.Verify(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskB.Id), userId), Times.Never); // 失敗的項目不應呼叫 CreatePurchaseAsync_

            // 驗證成功的更新
            _mockUserRepo.Verify(repo => repo.UpdateUserBalanceByIdAsync(userId, userBalance - 160m), Times.Once);
            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(pharmacyX.Id, pharmacyX.CashBalance + 100m), Times.Once); // MaskA
            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(pharmacyX.Id, pharmacyX.CashBalance + 60m), Times.Once); // MaskC
            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(pharmacyY.Id, It.IsAny<decimal>()), Times.Never); // MaskB 失敗，不應更新藥局餘額
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(maskA.Id, maskA.StockQuantity - 2), Times.Once);
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(maskC.Id, maskC.StockQuantity - 3), Times.Once);
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(maskB.Id, It.IsAny<int>()), Times.Never); // MaskB 失敗，不應更新庫存

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("庫存不足")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Exactly(2) //MaskB的庫存不足會記錄兩次：一次在預計算階段，一次在實際處理階段
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"用戶ID {userId} 批量購買結果: 2/3 成功")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once   //購買成功紀錄
            );
        }

        /// <summary>
        /// S6. 測試空購買清單的處理。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_EmptyPurchasesList_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var bulkPurchaseDto = new BulkPurchaseDto_ { UserId = userId, Purchases = new List<BulkPurchaseItemDto_>() };

            // **修正點：在這裡宣告並賦值 initialUserBalance**
            var initialUserBalance = 500m;

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            // **修正點：使用宣告的 initialUserBalance 變數**
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, "TestUser", initialUserBalance));

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.False(result.Success); // 沒有任何購買成功
            Assert.Equal("所有購買都失敗了", result.Message);
            Assert.Empty(result.Errors); // 沒有錯誤，只是沒有購買
            Assert.Empty(result.CompletedPurchases);
            Assert.Equal(0m, result.TotalAmount);

            _mockUserRepo.Verify(repo => repo.UserExistsAsync(userId), Times.Once);
            // **修正點：使用宣告的 initialUserBalance 變數**
            // 雖然沒有購買，但服務層還是會呼叫 UpdateUserBalanceByIdAsync 來更新餘額（餘額會是初始餘額 - 0）
            _mockUserRepo.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mockUserRepo.Verify(repo => repo.UpdateUserBalanceByIdAsync(userId, initialUserBalance), Times.Once);

            _mockMaskRepo.VerifyNoOtherCalls();
            _mockPharmacyRepo.VerifyNoOtherCalls();
        }

        /// <summary>
        /// S7. 測試在預計算階段，如果獲取 Mask 資訊時拋出異常。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_MaskGetByIdThrowsExceptionInPrecalculation_ContinuesProcessing()
        {
            // Arrange
            var userId = 1;
            var bulkPurchaseDto = new BulkPurchaseDto_
            {
                UserId = userId,
                Purchases = new List<BulkPurchaseItemDto_>
                {
                    new BulkPurchaseItemDto_ { MaskId = 101, PharmacyId = 201, Quantity = 2 }, // 會拋出異常
                    new BulkPurchaseItemDto_ { MaskId = 102, PharmacyId = 202, Quantity = 1 }  // 正常購買
                }
            };

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, "TestUser", 500m));

            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(101)).ThrowsAsync(new InvalidOperationException("Mask 101 資料庫錯誤"));
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(102)).ReturnsAsync(GetMockMask(102, "MaskB", 10, 50m)); // 正常

            _mockPharmacyRepo.Setup(repo => repo.GetByIdAsync(202)).ReturnsAsync(GetMockPharmacy(202, "PharmacyY", 500m));

            var mockPurchaseEntity = GetMockPurchaseEntity(1001, userId, "TestUser", 202, "PharmacyY", 102, "MaskB", 1, 50m, DateTime.Now);
            _mockPurchaseRepo.Setup(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == 102), userId))
                .ReturnsAsync(mockPurchaseEntity);

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.True(result.Success); // Mask 102 應該成功
            Assert.Equal("成功完成 1 筆購買，總金額: $50.00", result.Message);
            Assert.Equal(2, result.Errors.Count); // 預期有兩個錯誤訊息,預計算&實際操作階段各一次


            Assert.Contains( "購買失敗: Mask 101 資料庫錯誤", result.Errors);

            Assert.Single(result.CompletedPurchases);
            Assert.Equal(50m, result.TotalAmount);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"用戶ID {userId} 批量購買結果: 1/{bulkPurchaseDto.Purchases.Count} 成功")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once // 資訊日誌會被呼叫一次,成功的那次
            );
        }

        /// <summary>
        /// S8. 測試在實際購買階段，如果創建購買記錄時拋出異常。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_CreatePurchaseThrowsException_ContinuesProcessing()
        {
            // Arrange
            var userId = 1;
            var userName = "TestUser";
            var userBalance = 500m;

            var maskA = GetMockMask(101, "MaskA", 5, 50m); // 成功購買
            var maskB = GetMockMask(102, "MaskB", 10, 30m); // 創建購買時拋出異常

            var pharmacyX = GetMockPharmacy(201, "PharmacyX", 1000m);
            var pharmacyY = GetMockPharmacy(202, "PharmacyY", 500m);

            var bulkPurchaseDto = new BulkPurchaseDto_
            {
                UserId = userId,
                Purchases = new List<BulkPurchaseItemDto_>
                {
                    new BulkPurchaseItemDto_ { MaskId = maskA.Id, PharmacyId = pharmacyX.Id, Quantity = 2 },
                    new BulkPurchaseItemDto_ { MaskId = maskB.Id, PharmacyId = pharmacyY.Id, Quantity = 1 }
                }
            };

            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ReturnsAsync(true);
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(GetMockUser(userId, userName, userBalance));

            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskA.Id)).ReturnsAsync(maskA);
            _mockMaskRepo.Setup(repo => repo.GetByIdAsync(maskB.Id)).ReturnsAsync(maskB);

            _mockPharmacyRepo.Setup(repo => repo.GetByIdAsync(pharmacyX.Id)).ReturnsAsync(pharmacyX);
            _mockPharmacyRepo.Setup(repo => repo.GetByIdAsync(pharmacyY.Id)).ReturnsAsync(pharmacyY);

            var purchaseA_Entity = GetMockPurchaseEntity(1001, userId, userName, pharmacyX.Id, pharmacyX.Name, maskA.Id, maskA.Name, 2, 100m, DateTime.Now);
            _mockPurchaseRepo.Setup(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskA.Id), userId))
                .ReturnsAsync(purchaseA_Entity);

            _mockPurchaseRepo.Setup(repo => repo.CreatePurchaseAsync_(
                It.Is<BulkPurchaseItemDto_>(item => item.MaskId == maskB.Id), userId))
                .ThrowsAsync(new InvalidOperationException("購買記錄創建失敗"));

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.True(result.Success); // Mask A 應該成功
            Assert.Equal("成功完成 1 筆購買，總金額: $100.00", result.Message);
            Assert.Single(result.Errors);
            Assert.Contains("購買失敗: 購買記錄創建失敗", result.Errors);
            Assert.Single(result.CompletedPurchases);
            Assert.Equal(100m, result.TotalAmount);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("處理購買項目時發生錯誤")),
                    It.IsAny<InvalidOperationException>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once // 只有 Mask B 觸發錯誤日誌
            );

            // 驗證只有成功的購買更新了餘額和庫存
            _mockUserRepo.Verify(repo => repo.UpdateUserBalanceByIdAsync(userId, userBalance - 100m), Times.Once);
            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(pharmacyX.Id, pharmacyX.CashBalance + 100m), Times.Once);
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(maskA.Id, maskA.StockQuantity - 2), Times.Once);

            _mockPharmacyRepo.Verify(repo => repo.UpdateBalanceByIdAsync(pharmacyY.Id, It.IsAny<decimal>()), Times.Never);
            _mockMaskRepo.Verify(repo => repo.UpdateMaskStockAsync(maskB.Id, It.IsAny<int>()), Times.Never);
        }

        /// <summary>
        /// S9. 測試整個方法被捕獲的通用異常 (Try-Catch 包裹整個方法)。
        /// </summary>
        [Fact]
        public async Task ProcessBulkPurchaseAsync_GeneralExceptionCaught_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var bulkPurchaseDto = new BulkPurchaseDto_ { UserId = userId, Purchases = new List<BulkPurchaseItemDto_>() };

            // 模擬第一個 Repository 呼叫就拋出異常
            _mockUserRepo.Setup(repo => repo.UserExistsAsync(userId)).ThrowsAsync(new Exception("模擬一般錯誤"));

            // Act
            var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchaseDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("批量購買處理失敗", result.Message);
            Assert.Contains("模擬一般錯誤", result.Errors);
            Assert.Empty(result.CompletedPurchases);
            Assert.Equal(0m, result.TotalAmount);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("批量購買處理時發生錯誤")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }
    }
}