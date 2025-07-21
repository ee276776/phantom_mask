using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Services
{
    public class MaskService : IMaskService
    {
        private readonly IMaskRepository _maskRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly ILogger<MaskService> _logger;

        public MaskService(
            IMaskRepository maskRepository,
            IPharmacyRepository pharmacyRepository,
            ILogger<MaskService> logger)
        {
            _maskRepository = maskRepository;
            _pharmacyRepository = pharmacyRepository;
            _logger = logger;
        }

        public async Task<List<MaskDto>> SearchMasksAsync(string searchTerm, int limit = 50)
        {
            var masks = await _maskRepository.SearchMasksAsync(searchTerm);
            
            var result = masks.Take(limit).Select(m => new MaskDto
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                StockQuantity = m.StockQuantity,
                PharmacyId = m.PharmacyId,
                PharmacyName = m.Pharmacy.Name,
                CreatedAt = m.CreatedAt
            }).ToList();

            _logger.LogInformation($"🔍 搜尋口罩 '{searchTerm}' 找到 {result.Count} 個結果");
            return result;
        }

        public async Task<MaskDto?> GetMaskByIdAsync(int maskId)
        {
            var mask = await _maskRepository.GetMaskWithPharmacyAsync(maskId);
            if (mask == null) return null;

            return new MaskDto
            {
                Id = mask.Id,
                Name = mask.Name,
                Price = mask.Price,
                StockQuantity = mask.StockQuantity,
                PharmacyId = mask.PharmacyId,
                PharmacyName = mask.Pharmacy.Name,
                CreatedAt = mask.CreatedAt
            };
        }

        public async Task<MaskDto?> UpdateMaskStockAsync(int maskId, StockUpdateDto stockUpdate)
        {
            var mask = await _maskRepository.GetByIdAsync(maskId);
            if (mask == null) throw new ArgumentException($"找不到口罩 ID: {maskId}");

            int newStock = stockUpdate.Operation.ToLower() switch
            {
                "increase" => mask.StockQuantity + stockUpdate.Quantity,
                "decrease" => Math.Max(0, mask.StockQuantity - stockUpdate.Quantity),
                _ => throw new ArgumentException("Operation 必須是 'increase' 或 'decrease'")
            };

            if (stockUpdate.Operation.ToLower() == "decrease" && mask.StockQuantity < stockUpdate.Quantity)
            {
                _logger.LogWarning($"⚠️ 口罩 {maskId} 庫存不足，無法減少 {stockUpdate.Quantity}");
                newStock = 0;
            }

            await _maskRepository.UpdateMaskStockAsync(maskId, newStock);

            _logger.LogInformation($"📦 口罩 {maskId} 庫存已{(stockUpdate.Operation == "increase" ? "增加" : "減少")} {stockUpdate.Quantity}，目前庫存: {newStock}");

            mask.StockQuantity = newStock;
            return new MaskDto
            {
                Id = mask.Id,
                Name = mask.Name,
                Price = mask.Price,
                StockQuantity = mask.StockQuantity,
                PharmacyId = mask.PharmacyId,
                PharmacyName = mask.Pharmacy?.Name ?? "",
                CreatedAt = mask.CreatedAt
            };
        }

        public async Task<List<MaskDto>> BulkUpdateMasksAsync(int pharmacyId, List<BulkMaskUpdateDto> maskUpdates)
        {
            var updatedMasks = await _maskRepository.BulkUpdateMasksAsync(pharmacyId, maskUpdates);
            
            var result = updatedMasks.Select(m => new MaskDto
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                StockQuantity = m.StockQuantity,
                PharmacyId = m.PharmacyId,
                PharmacyName = m.Pharmacy?.Name ?? "",
                CreatedAt = m.CreatedAt
            }).ToList();

            _logger.LogInformation($"💊 藥局 {pharmacyId} 批量更新了 {result.Count} 個口罩");
            return result;
        }

        public async Task<List<MaskDto>> GetLowStockMasksAsync(int threshold = 10)
        {
            var lowStockMasks = await _maskRepository.GetLowStockMasksAsync(threshold);
            
            var result = lowStockMasks.Select(m => new MaskDto
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                StockQuantity = m.StockQuantity,
                PharmacyId = m.PharmacyId,
                PharmacyName = m.Pharmacy.Name,
                CreatedAt = m.CreatedAt
            }).ToList();

            _logger.LogInformation($"⚠️ 找到 {result.Count} 個低庫存口罩 (低於 {threshold} 個)");
            return result;
        }

        public async Task<List<MaskDto>> GetMasksInPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var masks = await _maskRepository.GetMasksInPriceRangeAsync(minPrice, maxPrice);
            
            var result = masks.Select(m => new MaskDto
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                StockQuantity = m.StockQuantity,
                PharmacyId = m.PharmacyId,
                PharmacyName = m.Pharmacy.Name,
                CreatedAt = m.CreatedAt
            }).ToList();

            _logger.LogInformation($"💰 價格範圍 ${minPrice}-${maxPrice} 找到 {result.Count} 個口罩");
            return result;
        }
    }
}
