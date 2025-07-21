using Microsoft.EntityFrameworkCore;
using PhantomMaskAPI.Data;
using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Repositories
{
    public class PharmacyRepository : BaseRepository<Pharmacy>, IPharmacyRepository
    {
        public PharmacyRepository(PhantomMaskContext context) : base(context)
        {
        }

        public async Task<List<Pharmacy>> GetPharmaciesWithMasksAsync()
        {
            return await _dbSet
                .Include(p => p.Masks)
                .ToListAsync();
        }

        public async Task<Pharmacy?> GetPharmacyWithMasksAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Masks)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Pharmacy>> SearchPharmaciesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()))
                .Include(p => p.Masks)
                .ToListAsync();
        }

        public async Task<List<Pharmacy>> GetPharmaciesByOpeningHoursAsync(string openingHours)
        {
            return await _dbSet
                .Where(p => p.OpeningHours.Contains(openingHours))
                .Include(p => p.Masks)
                .ToListAsync();
        }

        public async Task<List<Pharmacy>> GetPharmaciesByDayOfWeekAsync(string dayOfWeek)
        {
            return await _dbSet
                .Where(p => p.OpeningHours.ToLower().Contains(dayOfWeek.ToLower()))
                .Include(p => p.Masks)
                .ToListAsync();
        }

        public async Task<List<Pharmacy>> GetPharmaciesWithAvailableStockAsync()
        {
            return await _dbSet
                .Include(p => p.Masks)
                .Where(p => p.Masks.Any(m => m.StockQuantity > 0))
                .ToListAsync();
        }

        public async Task<List<Pharmacy>> GetPharmaciesByStockCriteriaAsync(decimal minPrice, decimal maxPrice, int stockThreshold, string comparison)
        {
            var query = _dbSet
                .Include(p => p.Masks)
                .Where(p => p.Masks.Any(m => m.Price >= minPrice && m.Price <= maxPrice));

            query = comparison.ToLower() switch
            {
                "above" => query.Where(p => p.Masks.Any(m => m.StockQuantity > stockThreshold)),
                "below" => query.Where(p => p.Masks.Any(m => m.StockQuantity < stockThreshold)),
                "between" => query.Where(p => p.Masks.Any(m => m.StockQuantity >= stockThreshold)),
                _ => query
            };

            return await query.ToListAsync();
        }
    }
}
