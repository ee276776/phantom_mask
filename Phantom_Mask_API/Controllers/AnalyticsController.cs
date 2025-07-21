using Microsoft.AspNetCore.Mvc;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IPurchaseService purchaseService,
            ILogger<AnalyticsController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        /// <summary>
        /// 顯示特定日期範圍內購買口罩花費最多的前 N 名用戶
        /// </summary>
        [HttpGet("top-spenders")]
        public async Task<ActionResult<List<TopSpenderDto>>> GetTopSpenders(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int topN = 10)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("開始日期不能晚於結束日期");
                }

                if (topN <= 0 || topN > 100)
                {
                    return BadRequest("topN 必須介於 1-100 之間");
                }

                var topSpenders = await _purchaseService.GetTopSpendersAsync(startDate, endDate, topN);
                _logger.LogInformation($"🏆 取得前 {topN} 名消費用戶 ({startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd})");
                
                return Ok(topSpenders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得消費排行榜時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 取得購買趨勢分析
        /// </summary>
        [HttpGet("purchase-trends")]
        public async Task<ActionResult<PurchaseAnalyticsDto>> GetPurchaseTrends(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("開始日期不能晚於結束日期");
                }

                var analytics = await _purchaseService.GetPurchaseAnalyticsAsync(startDate, endDate);
                _logger.LogInformation($"📈 生成購買趨勢分析: {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd}");
                
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得購買趨勢分析時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }
    }
}
