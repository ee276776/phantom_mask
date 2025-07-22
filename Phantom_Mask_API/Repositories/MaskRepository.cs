using Microsoft.EntityFrameworkCore;
using PhantomMaskAPI.Data;
using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Repositories
{
    public class MaskRepository : BaseRepository<Mask>, IMaskRepository
    {
        public MaskRepository(PhantomMaskContext context) : base(context)
        {
        }

        public async Task<List<Mask>> GetMasksByPharmacyIdAsync(int pharmacyId)
        {
            return await _dbSet
                .Include(m => m.Pharmacy)
                .Where(m => m.PharmacyId == pharmacyId)
                .ToListAsync();
        }

        public async Task<bool> CheckStockingEnoughAsync(int maskID,int num)
        {
            var mask= await _dbSet.FirstOrDefaultAsync(n => n.Id == maskID);
            return mask.StockQuantity >= num;
        }

        public async Task<List<Mask>> UpsertMasksAsync(int pharmacyId, List<MaskUpsertDto> maskUpdates)
        {
            var result = new List<Mask>();

            foreach (var dto in maskUpdates)
            {
                var existing = await _context.Masks
                    .FirstOrDefaultAsync(m => m.PharmacyId == pharmacyId && m.Name == dto.Name);

                if (existing != null)
                {
                    // 更新現有口罩
                    existing.Price = dto.Price;
                    existing.StockQuantity = dto.StockQuantity;
                    result.Add(existing);
                }
                else
                {
                    // 新增新口罩
                    var newMask = new Mask
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        StockQuantity = dto.StockQuantity,
                        PharmacyId = pharmacyId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Masks.Add(newMask);
                    result.Add(newMask);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }



        public async Task<List<Mask>> SearchMasksAsync(string searchTerm)
        {
            return await _dbSet
                .Include(m => m.Pharmacy)
                .Where(m => m.Name.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }

        public async Task<List<Mask>> GetMasksInPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Include(m => m.Pharmacy)
                .Where(m => m.Price >= minPrice && m.Price <= maxPrice)
                .ToListAsync();
        }

        public async Task<Mask?> GetMaskWithPharmacyAsync(int maskId)
        {
            return await _dbSet
                .Include(m => m.Pharmacy)
                .FirstOrDefaultAsync(m => m.Id == maskId);
        }

        public async Task<List<Mask>> GetMasksWithPharmacyAsync()
        {
            return await _dbSet
                .Include(m => m.Pharmacy)
                .ToListAsync();
        }

        public async Task UpdateMaskStockAsync(int maskId, int newStock)
        {
            var mask = await _dbSet.FindAsync(maskId);
            if (mask != null)
            {
                mask.StockQuantity = newStock;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Mask>> BulkUpdateMasksAsync(int pharmacyId, List<BulkMaskUpdateDto> maskUpdates)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var updatedMasks = new List<Mask>();

                foreach (var update in maskUpdates)
                {
                    var mask = await _dbSet.FindAsync(update.MaskId);
                    if (mask != null && mask.PharmacyId == pharmacyId)
                    {
                        mask.StockQuantity = update.NewStock;
                        if (update.NewPrice.HasValue)
                        {
                            mask.Price = update.NewPrice.Value;
                        }
                        updatedMasks.Add(mask);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return updatedMasks;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Mask>> GetLowStockMasksAsync(int threshold = 10)
        {
            return await _dbSet
                .Include(m => m.Pharmacy)
                .Where(m => m.StockQuantity <= threshold)
                .ToListAsync();
        }

        public async Task UpdateStockQuantityAsync(int maskId, int quantity)
        {
            var mask = await _dbSet.FindAsync(maskId);
            if (mask != null)
            {
                mask.StockQuantity = quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkUpdateMasksAsync(int pharmacyId, List<Mask> masks)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var mask in masks)
                {
                    mask.PharmacyId = pharmacyId;
                    mask.CreatedAt = DateTime.UtcNow;
                    
                    var existingMask = await _dbSet
                        .FirstOrDefaultAsync(m => m.Name == mask.Name && m.PharmacyId == pharmacyId);
                    
                    if (existingMask != null)
                    {
                        existingMask.Price = mask.Price;
                        existingMask.StockQuantity = mask.StockQuantity;
                    }
                    else
                    {
                        _dbSet.Add(mask);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
