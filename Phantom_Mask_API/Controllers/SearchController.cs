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
        /// [Q8] ※※ 執行相關性搜尋 ※※
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
