using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Interfaces
{
    public interface IPurchaseService
    {
        Task<List<PurchaseDto>> GetUserPurchasesAsync(string userName);
        Task<List<TopSpenderDto>> GetTopSpendersAsync(DateTime startDate, DateTime endDate, int topN);
        Task<BulkPurchaseResultDto> ProcessBulkPurchaseAsync(BulkPurchaseDto bulkPurchase);
        Task<BulkPurchaseResultDto> ProcessBulkPurchaseAsync_(BulkPurchaseDto_ bulkPurchase);

        Task<List<PurchaseDto>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PurchaseAnalyticsDto> GetPurchaseAnalyticsAsync(DateTime startDate, DateTime endDate);
    }
}
