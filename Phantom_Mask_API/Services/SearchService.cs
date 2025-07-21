using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Services
{
    public class SearchService : ISearchService
    {
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IMaskRepository _maskRepository;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IPharmacyRepository pharmacyRepository,
            IMaskRepository maskRepository,
            ILogger<SearchService> logger)
        {
            _pharmacyRepository = pharmacyRepository;
            _maskRepository = maskRepository;
            _logger = logger;
        }

        public async Task<SearchResultDto> SearchAsync(string query, string type = "all", int limit = 50)
        {
            var result = new SearchResultDto
            {
                SearchTerm = query,
                Pharmacies = new List<PharmacyDto>(),
                Masks = new List<MaskDto>()
            };

            try
            {
                if (type.ToLower() == "all" || type.ToLower() == "pharmacy")
                {
                    result.Pharmacies = await SearchPharmaciesAsync(query, limit);
                }

                if (type.ToLower() == "all" || type.ToLower() == "mask")
                {
                    result.Masks = await SearchMasksAsync(query, limit);
                }

                result.TotalResults = result.Pharmacies.Count + result.Masks.Count;
                
                _logger.LogInformation($"🔍 綜合搜尋 '{query}' (類型: {type}) 找到 {result.TotalResults} 個結果");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"綜合搜尋 '{query}' 時發生錯誤");
                throw;
            }
        }

        public async Task<List<PharmacyDto>> SearchPharmaciesAsync(string query, int limit = 50)
        {
            var pharmacies = await _pharmacyRepository.SearchPharmaciesAsync(query);
            
            var result = pharmacies.Take(limit).Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                CashBalance = p.CashBalance,
                OpeningHours = p.OpeningHours,
                CreatedAt = p.CreatedAt,
                MaskCount = p.Masks.Count
            }).ToList();

            // 按相關性排序 - 精確匹配優先，然後是包含關鍵字的
            result = result.OrderByDescending(p => 
            {
                var name = p.Name.ToLower();
                var searchTerm = query.ToLower();
                
                if (name == searchTerm) return 100; // 精確匹配
                if (name.StartsWith(searchTerm)) return 80; // 開頭匹配
                if (name.Contains(searchTerm)) return 60; // 包含匹配
                return 0; // 其他
            }).ToList();

            _logger.LogInformation($"🏥 搜尋藥局 '{query}' 找到 {result.Count} 個結果");
            return result;
        }

        public async Task<List<MaskDto>> SearchMasksAsync(string query, int limit = 50)
        {
            var masks = await _maskRepository.SearchMasksAsync(query);
            
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

            // 按相關性排序 - 精確匹配優先，然後是包含關鍵字的
            result = result.OrderByDescending(m => 
            {
                var name = m.Name.ToLower();
                var searchTerm = query.ToLower();
                
                if (name == searchTerm) return 100; // 精確匹配
                if (name.StartsWith(searchTerm)) return 80; // 開頭匹配
                if (name.Contains(searchTerm)) return 60; // 包含匹配
                return 0; // 其他
            }).ToList();

            _logger.LogInformation($"😷 搜尋口罩 '{query}' 找到 {result.Count} 個結果");
            return result;
        }
    }
}
