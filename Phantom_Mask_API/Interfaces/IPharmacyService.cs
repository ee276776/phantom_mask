using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Interfaces
{
    public interface IPharmacyService
    {
        Task<List<PharmacyDto>> GetPharmaciesAsync(PharmacyFilterDto filter);
        //Task<PharmacyDto?> GetPharmacyByIdAsync(int id);
        Task<List<MaskDto>> GetPharmacyMasksAsync(int pharmacyId, string? sortBy, string? sortOrder);
        //Task<List<PharmacyDto>> SearchPharmaciesAsync(string searchTerm);
        
        Task<List<PharmacyDto>> GetPharmaciesByStockCriteriaAsync(decimal minPrice, decimal maxPrice, int? minstockThreshold, int? maxstockThreshold , bool isInclusive);

    }
}
