using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Interfaces
{
    public interface IMaskService
    {
        //Task<List<MaskDto>> SearchMasksAsync(string searchTerm, int limit = 50);
        //Task<MaskDto?> GetMaskByIdAsync(int id);
        Task<MaskDto?> UpdateMaskStockAsync(int maskId, StockUpdateDto stockUpdate);
        //Task<List<MaskDto>> BulkUpdateMasksAsync(int pharmacyId, List<BulkMaskUpdateDto> maskUpdates);
        //Task<List<MaskDto>> GetLowStockMasksAsync(int threshold = 10);
        //Task<List<MaskDto>> GetMasksInPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<List<MaskDto>> UpsertMasksAsync(int pharmacyId, List<MaskUpsertDto> maskUpdates);
    }
}
