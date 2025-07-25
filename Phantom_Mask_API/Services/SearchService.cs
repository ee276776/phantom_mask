using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Services
{
    public class SearchService : ISearchService
    {
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IMaskRepository _maskRepository;
        private readonly IRelevanceService _relevanceService;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IPharmacyRepository pharmacyRepository,
            IMaskRepository maskRepository,
            IRelevanceService relevanceService,
            ILogger<SearchService> logger)
        {
            _relevanceService = relevanceService;
            _pharmacyRepository = pharmacyRepository;
            _maskRepository = maskRepository;
            _logger = logger;
        }

        //public async Task<SearchResultDto> SearchAsync(string query, string type = "all", int limit = 50)
        //{
        //    var result = new SearchResultDto
        //    {
        //        SearchTerm = query,
        //        Pharmacies = new List<PharmacyDto>(),
        //        Masks = new List<MaskDto>()
        //    };

        //    try
        //    {
        //        if (type.ToLower() == "all" || type.ToLower() == "pharmacy")
        //        {
        //            result.Pharmacies = await SearchPharmaciesAsync(query, limit);
        //        }

        //        if (type.ToLower() == "all" || type.ToLower() == "mask")
        //        {
        //            result.Masks = await SearchMasksAsync(query, limit);
        //        }

        //        result.TotalResults = result.Pharmacies.Count + result.Masks.Count;

        //        _logger.LogInformation($"🔍 綜合搜尋 '{query}' (類型: {type}) 找到 {result.TotalResults} 個結果");
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"綜合搜尋 '{query}' 時發生錯誤");
        //        throw;
        //    }
        //}

        //public async Task<List<PharmacyDto>> SearchPharmaciesAsync(string query, int limit = 50)
        //{
        //    var pharmacies = await _pharmacyRepository.SearchPharmaciesAsync(query);

        //    var result = pharmacies.Take(limit).Select(p => new PharmacyDto
        //    {
        //        Id = p.Id,
        //        Name = p.Name,
        //        CashBalance = p.CashBalance,
        //        OpeningHours = p.OpeningHours,
        //        CreatedAt = p.CreatedAt,
        //        MaskTypeCount = p.Masks.Count,
        //        MaskTotalCount = p.Masks.Sum(m => m.StockQuantity)
        //    }).ToList();

        //    // 按相關性排序 - 精確匹配優先，然後是包含關鍵字的
        //    result = result.OrderByDescending(p =>
        //    {
        //        var name = p.Name.ToLower();
        //        var searchTerm = query.ToLower();

        //        if (name == searchTerm) return 100; // 精確匹配
        //        if (name.StartsWith(searchTerm)) return 80; // 開頭匹配
        //        if (name.Contains(searchTerm)) return 60; // 包含匹配
        //        return 0; // 其他
        //    }).ToList();

        //    _logger.LogInformation($"🏥 搜尋藥局 '{query}' 找到 {result.Count} 個結果");
        //    return result;
        //}

        //public async Task<List<MaskDto>> SearchMasksAsync(string query, int limit = 50)
        //{
        //    var masks = await _maskRepository.SearchMasksAsync(query);

        //    var result = masks.Take(limit).Select(m => new MaskDto
        //    {
        //        Id = m.Id,
        //        Name = m.Name,
        //        Price = m.Price,
        //        StockQuantity = m.StockQuantity,
        //        PharmacyId = m.PharmacyId,
        //        PharmacyName = m.Pharmacy.Name,
        //        CreatedAt = m.CreatedAt
        //    }).ToList();

        //    // 按相關性排序 - 精確匹配優先，然後是包含關鍵字的
        //    result = result.OrderByDescending(m =>
        //    {
        //        var name = m.Name.ToLower();
        //        var searchTerm = query.ToLower();

        //        if (name == searchTerm) return 100; // 精確匹配
        //        if (name.StartsWith(searchTerm)) return 80; // 開頭匹配
        //        if (name.Contains(searchTerm)) return 60; // 包含匹配
        //        return 0; // 其他
        //    }).ToList();

        //    _logger.LogInformation($"😷 搜尋口罩 '{query}' 找到 {result.Count} 個結果");
        //    return result;
        //}

        public async Task<List<RelevanceResultDto>> SearchByRelavanceAsync(string query)
        {

            var masks = await _maskRepository.GetAllAsync();
            var pharmacies = await _pharmacyRepository.GetAllAsync();

            var allResults = new List<RelevanceResultDto>();

            // 2. 處理口罩數據
            foreach (var mask in masks)
            {
                var searchItem = new RelevanceDto
                {
                    ID = mask.Id,
                    Name = mask.Name,
                    Type = "mask"

                };
                double score = _relevanceService.CalculateRelevanceScoreInternal(query, searchItem);
                allResults.Add(new RelevanceResultDto
                {
                    ID = mask.Id,
                    Type = "mask",
                    Name = mask.Name,
                    RelevanceScore = score
                });
            }

            // 3. 處理藥局數據
            foreach (var pharmacy in pharmacies)
            {
                var searchItem = new RelevanceDto
                {
                    ID = pharmacy.Id,
                    Name = pharmacy.Name,
                    Type = "pharmacy"
                };
                double score = _relevanceService.CalculateRelevanceScoreInternal(query, searchItem);
                allResults.Add(new RelevanceResultDto
                {
                    ID = pharmacy.Id,
                    Type = "pharmacy",
                    Name = pharmacy.Name,
                    RelevanceScore = score
                });
            }

            // 4. 過濾掉分數為 0 的結果，然後按相關性分數降序排序並回傳
            return allResults
                .Where(r => r.RelevanceScore > 0)
                .OrderByDescending(r => r.RelevanceScore)
                .ToList();


        }


    }
}
