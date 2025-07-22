using PhantomMaskAPI.Models.Entities;

namespace PhantomMaskAPI.Interfaces
{
    public interface IPharmacyRepository : IBaseRepository<Pharmacy>
    {
        Task<List<Pharmacy>> GetPharmaciesWithMasksAsync();
        Task<Pharmacy?> GetPharmacyWithMasksAsync(int id);
        Task<List<Pharmacy>> SearchPharmaciesAsync(string searchTerm);
        Task<List<Pharmacy>> GetPharmaciesByStockCriteriaAsync(decimal minPrice, decimal maxPrice, int stockThreshold, string comparison);
        Task<List<Pharmacy>> GetPharmaciesWithAvailableStockAsync();
        Task UpdateBalanceByIdAsync(int pharmacyId, decimal newBalance);
    }
}
