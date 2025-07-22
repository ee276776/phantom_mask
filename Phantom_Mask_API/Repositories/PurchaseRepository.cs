using Microsoft.EntityFrameworkCore;
using PhantomMaskAPI.Data;
using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Repositories
{
    public class PurchaseRepository : BaseRepository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(PhantomMaskContext context) : base(context)
        {
        }

        public async Task<List<Purchase>> GetPurchasesByUserAsync(string userName)
        {
            return await _dbSet
                .Where(p => p.UserName == userName)
                .OrderByDescending(p => p.TransactionDateTime)
                .ToListAsync();
        }

        public async Task<List<Purchase>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.TransactionDateTime >= startDate && p.TransactionDateTime <= endDate)
                .OrderByDescending(p => p.TransactionDateTime)
                .ToListAsync();
        }

        public async Task<List<Purchase>> GetPurchasesByPharmacyAsync(string pharmacyName)
        {
            return await _dbSet
                .Where(p => p.PharmacyName == pharmacyName)
                .OrderByDescending(p => p.TransactionDateTime)
                .ToListAsync();
        }

        public async Task<Purchase> CreatePurchaseAsync(BulkPurchaseItemDto purchaseItem, string userName)
        {
            var purchase = new Purchase
            {
                UserName = userName,
                PharmacyName = purchaseItem.PharmacyName,
                MaskName = purchaseItem.MaskName,
                TransactionQuantity = purchaseItem.Quantity,
                TransactionAmount = purchaseItem.TotalAmount,
                TransactionDateTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(purchase);
            await _context.SaveChangesAsync();
            return purchase;
        }

        public async Task<Purchase> CreatePurchaseAsync_(BulkPurchaseItemDto_ purchaseItem, int userId)
        {
            var userName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();
            var PharmacyName = await _context.Pharmacies
                .Where(p => p.Id == purchaseItem.PharmacyId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync();
            var mask = await _context.Masks
                .Where(m => m.Id == purchaseItem.MaskId)
                .FirstOrDefaultAsync();

            var purchase = new Purchase
            {
                UserName = userName,
                PharmacyName = PharmacyName,
                MaskName = mask.Name,
                TransactionQuantity = purchaseItem.Quantity,
                TransactionAmount = purchaseItem.Quantity * mask.Price,
                TransactionDateTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(purchase);
            await _context.SaveChangesAsync();
            return purchase;
        }




        public async Task<List<Purchase>> CreateBulkPurchasesAsync(List<BulkPurchaseItemDto> purchases, string userName)
        {
            var purchaseEntities = new List<Purchase>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in purchases)
                {
                    var purchase = new Purchase
                    {
                        UserName = userName,
                        PharmacyName = item.PharmacyName,
                        MaskName = item.MaskName,
                        TransactionQuantity = item.Quantity,
                        TransactionAmount = item.TotalAmount,
                        TransactionDateTime = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbSet.Add(purchase);
                    purchaseEntities.Add(purchase);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return purchaseEntities;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> GetTotalSpentByUserAsync(string userName, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.UserName == userName && 
                           p.TransactionDateTime >= startDate && 
                           p.TransactionDateTime <= endDate)
                .SumAsync(p => p.TransactionAmount);
        }
    }
}
