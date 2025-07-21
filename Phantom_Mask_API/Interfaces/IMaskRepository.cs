using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Interfaces
{
    public interface IMaskRepository : IBaseRepository<Mask>
    {
        Task<List<Mask>> GetMasksByPharmacyIdAsync(int pharmacyId);
        Task<List<Mask>> SearchMasksAsync(string searchTerm);
        Task<List<Mask>> GetMasksInPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<Mask?> GetMaskWithPharmacyAsync(int id);
        Task UpdateMaskStockAsync(int maskId, int newStock);
        Task<List<Mask>> BulkUpdateMasksAsync(int pharmacyId, List<BulkMaskUpdateDto> maskUpdates);
        Task<List<Mask>> GetLowStockMasksAsync(int threshold = 10);
    }
}
