// using PhantomMaskAPI.Models.Entities;

// namespace PhantomMaskAPI.Repositories
// {
//     public interface IMaskRepository : IBaseRepository<Mask>
//     {
//         Task<List<Mask>> GetMasksByPharmacyIdAsync(int pharmacyId);
//         Task<List<Mask>> SearchMasksAsync(string searchTerm);
//         Task<List<Mask>> GetMasksByPriceRangeAsync(decimal minPrice, decimal maxPrice);
//         Task<Mask?> GetMaskWithPharmacyAsync(int maskId);
//         Task<List<Mask>> GetMasksWithPharmacyAsync();
//         Task UpdateStockQuantityAsync(int maskId, int quantity);
//         Task BulkUpdateMasksAsync(int pharmacyId, List<Mask> masks);
//     }
// }
