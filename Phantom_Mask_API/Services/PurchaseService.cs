using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IMaskRepository _maskRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IMaskRepository maskRepository,
            IUserRepository userRepository,
            IPharmacyRepository pharmacyRepository,
            ILogger<PurchaseService> logger)
        {
            _purchaseRepository = purchaseRepository;
            _maskRepository = maskRepository;
            _userRepository = userRepository;
            _purchaseRepository = purchaseRepository;
            _pharmacyRepository = pharmacyRepository;
            _logger = logger;
        }

        //public async Task<List<PurchaseDto>> GetUserPurchasesAsync(string userName)
        //{
        //    var purchases = await _purchaseRepository.GetPurchasesByUserAsync(userName);
            
        //    var result = purchases.Select(p => new PurchaseDto
        //    {
        //        Id = p.Id,
        //        UserName = p.UserName,
        //        PharmacyName = p.PharmacyName,
        //        MaskName = p.MaskName,
        //        TransactionQuantity = p.TransactionQuantity,
        //        TransactionAmount = p.TransactionAmount,
        //        TransactionDateTime = p.TransactionDateTime,
        //        CreatedAt = p.CreatedAt
        //    }).ToList();

        //    _logger.LogInformation($"📋 用戶 {userName} 有 {result.Count} 筆購買記錄");
        //    return result;
        //}

        public async Task<List<TopSpenderDto>> GetTopSpendersAsync(DateTime startDate, DateTime endDate, int topN)
        {
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var purchases = await _purchaseRepository.GetPurchasesByDateRangeAsync(startDate, endDate);
            
            var topSpenders = purchases
                .GroupBy(p => p.UserName)
                .Select(g => new TopSpenderDto
                {
                    UserName = g.Key,
                    TotalSpent = g.Sum(p => p.TransactionAmount),
                    TotalPurchases = g.Count(),
                    FirstPurchase = g.Min(p => p.TransactionDateTime),
                    LastPurchase = g.Max(p => p.TransactionDateTime)
                })
                .OrderByDescending(s => s.TotalSpent)
                .Take(topN)
                .ToList();

            _logger.LogInformation($"🏆 取得前 {topN} 名消費用戶，期間: {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd}");
            return topSpenders;
        }

        //public async Task<BulkPurchaseResultDto> ProcessBulkPurchaseAsync(BulkPurchaseDto bulkPurchase)
        //{
        //    var result = new BulkPurchaseResultDto
        //    {
        //        CompletedPurchases = new List<PurchaseDto>(),
        //        Errors = new List<string>()
        //    };

        //    try
        //    {
        //        // 驗證用戶是否存在
        //        var userExists = await _userRepository.UserExistsAsync(bulkPurchase.UserName);
        //        if (!userExists)
        //        {
        //            result.Success = false;
        //            result.Message = $"用戶 {bulkPurchase.UserName} 不存在";
        //            result.Errors.Add($"用戶 {bulkPurchase.UserName} 不存在");
        //            return result;
        //        }

        //        var totalAmount = 0m;
        //        var completedPurchases = new List<PurchaseDto>();

        //        foreach (var purchaseItem in bulkPurchase.Purchases)
        //        {
        //            try
        //            {
        //                // 這裡可以加入庫存檢查、價格驗證等邏輯
        //                var purchase = await _purchaseRepository.CreatePurchaseAsync(purchaseItem, bulkPurchase.UserName);
                        
        //                var purchaseDto = new PurchaseDto
        //                {
        //                    Id = purchase.Id,
        //                    UserName = purchase.UserName,
        //                    PharmacyName = purchase.PharmacyName,
        //                    MaskName = purchase.MaskName,
        //                    TransactionQuantity = purchase.TransactionQuantity,
        //                    TransactionAmount = purchase.TransactionAmount,
        //                    TransactionDateTime = purchase.TransactionDateTime,
        //                    CreatedAt = purchase.CreatedAt
        //                };

        //                completedPurchases.Add(purchaseDto);
        //                totalAmount += purchase.TransactionAmount;
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, $"處理購買項目時發生錯誤: {purchaseItem.MaskName} from {purchaseItem.PharmacyName}");
        //                result.Errors.Add($"購買 {purchaseItem.MaskName} 失敗: {ex.Message}");
        //            }
        //        }

        //        result.Success = completedPurchases.Count > 0;
        //        result.CompletedPurchases = completedPurchases;
        //        result.TotalAmount = totalAmount;
        //        result.Message = result.Success 
        //            ? $"成功完成 {completedPurchases.Count} 筆購買，總金額: ${totalAmount:F2}"
        //            : "所有購買都失敗了";

        //        _logger.LogInformation($"🛒 用戶 {bulkPurchase.UserName} 批量購買結果: {completedPurchases.Count}/{bulkPurchase.Purchases.Count} 成功");
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"批量購買處理時發生錯誤");
        //        result.Success = false;
        //        result.Message = "批量購買處理失敗";
        //        result.Errors.Add(ex.Message);
        //        return result;
        //    }
        //}

        public async Task<BulkPurchaseResultDto> ProcessBulkPurchaseAsync_(BulkPurchaseDto_ bulkPurchase)
        {
            var result = new BulkPurchaseResultDto
            {
                CompletedPurchases = new List<PurchaseDto>(),
                Errors = new List<string>()
            };

            try
            {
                // 驗證用戶是否存在
                var userExists = await _userRepository.UserExistsAsync(bulkPurchase.UserId);
                if (!userExists)
                {
                    result.Success = false;
                    result.Message = $"用戶 {bulkPurchase.UserId} 不存在";
                    result.Errors.Add($"用戶 {bulkPurchase.UserId} 不存在");
                    return result;
                }
                var PrecalculatedAmount = 0m;
                var totalAmount = 0m;
                var completedPurchases = new List<PurchaseDto>();

                foreach (var precalculate in bulkPurchase.Purchases)
                {
                    try
                    {
                        var mask = await _maskRepository.GetByIdAsync(precalculate.MaskId);
                        if (mask.StockQuantity < precalculate.Quantity)
                        {
                            var err = $"❌ 藥局 {precalculate.PharmacyId} 的口罩 {precalculate.MaskId} 庫存不足（剩餘: {mask.StockQuantity}, 需求: {precalculate.Quantity}）";
                            _logger.LogWarning(err);
                            result.Errors.Add(err);
                            continue; // 跳過這一筆，繼續處理下一筆
                        }
                        PrecalculatedAmount += mask.Price * precalculate.Quantity;


                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"處理購買項目時發生錯誤");
                        result.Errors.Add($"購買失敗: {ex.Message}");
                    }
                   
                }
                var user = await _userRepository.GetByIdAsync(bulkPurchase.UserId);
                if (user.CashBalance < PrecalculatedAmount)
                {
                    var err = $"💰 買家餘額不足，僅有 {user.CashBalance:F2}";
                    result.Success = false;
                    result.Message = err;
                    result.Errors.Add(err);
                    return result;
                }

                foreach (var purchaseItem in bulkPurchase.Purchases)
                {
                    try
                    {
                        var mask = await _maskRepository.GetByIdAsync( purchaseItem.MaskId);
                        var pharmacy = await _pharmacyRepository.GetByIdAsync(purchaseItem.PharmacyId);

                        if (mask.StockQuantity < purchaseItem.Quantity)
                        {
                            var err = $"❌ 藥局 {purchaseItem.PharmacyId} 的口罩 {purchaseItem.MaskId} 庫存不足（剩餘: {mask.StockQuantity}, 需求: {purchaseItem.Quantity}）";
                            _logger.LogWarning(err);
                            result.Errors.Add(err);
                            continue; // 跳過這一筆，繼續處理下一筆
                        }
                        
                        var purchase = await _purchaseRepository.CreatePurchaseAsync_(purchaseItem, bulkPurchase.UserId);

                        var purchaseDto = new PurchaseDto
                        {
                            Id = purchase.Id,
                            UserName = purchase.UserName,
                            PharmacyName = purchase.PharmacyName,
                            MaskName = purchase.MaskName,
                            TransactionQuantity = purchase.TransactionQuantity,
                            TransactionAmount = purchase.TransactionAmount,
                            TransactionDateTime = purchase.TransactionDateTime,
                            CreatedAt = purchase.CreatedAt
                        };

                        completedPurchases.Add(purchaseDto);
                        totalAmount += purchase.TransactionAmount;

                        await _pharmacyRepository.UpdateBalanceByIdAsync(purchaseItem.PharmacyId, pharmacy.CashBalance + purchase.TransactionAmount);
                        await _maskRepository.UpdateMaskStockAsync(purchaseItem.MaskId, mask.StockQuantity - purchaseItem.Quantity);
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"處理購買項目時發生錯誤");
                        result.Errors.Add($"購買失敗: {ex.Message}");
                    }
                }
                await _userRepository.UpdateUserBalanceByIdAsync(bulkPurchase.UserId, user.CashBalance - totalAmount);


                result.Success = completedPurchases.Count > 0;
                result.CompletedPurchases = completedPurchases;
                result.TotalAmount = totalAmount;
                result.Message = result.Success
                    ? $"成功完成 {completedPurchases.Count} 筆購買，總金額: ${totalAmount:F2}"
                    : "所有購買都失敗了";

                _logger.LogInformation($"🛒 用戶ID {bulkPurchase.UserId} 批量購買結果: {completedPurchases.Count}/{bulkPurchase.Purchases.Count} 成功");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量購買處理時發生錯誤");
                result.Success = false;
                result.Message = "批量購買處理失敗";
                result.Errors.Add(ex.Message);
                return result;
            }
        }







        //public async Task<List<PurchaseDto>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate)
        //{
        //    var purchases = await _purchaseRepository.GetPurchasesByDateRangeAsync(startDate, endDate);
            
        //    var result = purchases.Select(p => new PurchaseDto
        //    {
        //        Id = p.Id,
        //        UserName = p.UserName,
        //        PharmacyName = p.PharmacyName,
        //        MaskName = p.MaskName,
        //        TransactionQuantity = p.TransactionQuantity,
        //        TransactionAmount = p.TransactionAmount,
        //        TransactionDateTime = p.TransactionDateTime,
        //        CreatedAt = p.CreatedAt
        //    }).ToList();

        //    _logger.LogInformation($"📅 日期範圍 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 有 {result.Count} 筆購買記錄");
        //    return result;
        //}

        //public async Task<PurchaseAnalyticsDto> GetPurchaseAnalyticsAsync(DateTime startDate, DateTime endDate)
        //{
        //    var purchases = await _purchaseRepository.GetPurchasesByDateRangeAsync(startDate, endDate);
            
        //    var analytics = new PurchaseAnalyticsDto
        //    {
        //        StartDate = startDate,
        //        EndDate = endDate,
        //        TotalPurchases = purchases.Count,
        //        TotalRevenue = purchases.Sum(p => p.TransactionAmount),
        //        AverageOrderValue = purchases.Count > 0 ? purchases.Average(p => p.TransactionAmount) : 0,
        //        MostPopularMask = purchases.GroupBy(p => p.MaskName)
        //            .OrderByDescending(g => g.Count())
        //            .FirstOrDefault()?.Key ?? "無資料",
        //        TopPharmacy = purchases.GroupBy(p => p.PharmacyName)
        //            .OrderByDescending(g => g.Sum(p => p.TransactionAmount))
        //            .FirstOrDefault()?.Key ?? "無資料"
        //    };

        //    _logger.LogInformation($"📊 生成購買分析報表: {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd}");
        //    return analytics;
        //}
    }
}
