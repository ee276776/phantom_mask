using Microsoft.AspNetCore.Mvc;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ISearchService searchService,
            ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// 按名稱搜尋藥局或口罩，並根據與搜尋字詞的相關性對結果進行排名
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<SearchResultDto>> Search(
            [FromQuery] string query,
            [FromQuery] string type = "all", // all, pharmacy, mask
            [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest("搜尋關鍵字不能為空");
                }

                if (limit <= 0 || limit > 100)
                {
                    return BadRequest("限制數量必須介於 1-100 之間");
                }

                if (!new[] { "all", "pharmacy", "mask" }.Contains(type.ToLower()))
                {
                    return BadRequest("類型必須是 'all', 'pharmacy' 或 'mask'");
                }

                var result = await _searchService.SearchAsync(query, type, limit);
                
                _logger.LogInformation($"🔍 搜尋 '{query}' (類型: {type}) 找到 {result.TotalResults} 個結果");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"搜尋 '{query}' 時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 僅搜尋藥局
        /// </summary>
        [HttpGet("pharmacies")]
        public async Task<ActionResult<List<PharmacyDto>>> SearchPharmacies(
            [FromQuery] string query,
            [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest("搜尋關鍵字不能為空");
                }

                var pharmacies = await _searchService.SearchPharmaciesAsync(query, limit);
                _logger.LogInformation($"🏥 搜尋藥局 '{query}' 找到 {pharmacies.Count} 個結果");
                
                return Ok(pharmacies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"搜尋藥局 '{query}' 時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 僅搜尋口罩
        /// </summary>
        [HttpGet("masks")]
        public async Task<ActionResult<List<MaskDto>>> SearchMasks(
            [FromQuery] string query,
            [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest("搜尋關鍵字不能為空");
                }

                var masks = await _searchService.SearchMasksAsync(query, limit);
                _logger.LogInformation($"😷 搜尋口罩 '{query}' 找到 {masks.Count} 個結果");
                
                return Ok(masks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"搜尋口罩 '{query}' 時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 執行相關性搜尋。
        /// </summary>
        /// <param name="query">搜尋關鍵字 (必填)。</param>
        [HttpGet("SearchByRelavance")] // 建議使用更具描述性的 Action 名稱
        public async Task<ActionResult<List<RelevanceResultDto>>> Search(
            [FromQuery] string query
           )
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest("搜尋關鍵字不能為空。");
                }

                // 調用服務層的搜尋方法
                // 將 Controller 的 type 和 limit 參數傳遞給服務層
                var result = await _searchService.SearchByRelavanceAsync(query);

                _logger.LogInformation($"🔍 搜尋 '{query}' 成功。");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"搜尋 '{query}' 時發生錯誤。");
                return StatusCode(500, "伺服器錯誤，請稍後再試。"); // 提供更友善的錯誤訊息
            }
        }
    }
}
