// using PhantomMaskAPI.Models.Entities;
// using PhantomMaskAPI.Models.DTOs;

// namespace PhantomMaskAPI.Repositories
// {
//     public interface IPurchaseRepository : IBaseRepository<Purchase>
//     {
//         Task<List<Purchase>> GetPurchasesByUserAsync(string userName);
//         Task<List<Purchase>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate);
//         Task<List<Purchase>> GetPurchasesByPharmacyAsync(string pharmacyName);
//         Task<Purchase> CreatePurchaseAsync(BulkPurchaseItemDto purchaseItem, string userName);
//         Task<List<Purchase>> CreateBulkPurchasesAsync(List<BulkPurchaseItemDto> purchases, string userName);
//         Task<decimal> GetTotalSpentByUserAsync(string userName, DateTime startDate, DateTime endDate);
//     }
// }
