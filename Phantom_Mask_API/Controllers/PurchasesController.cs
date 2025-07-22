using Microsoft.AspNetCore.Mvc;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(
            IPurchaseService purchaseService,
            ILogger<PurchasesController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        /// <summary>
        /// 處理用戶一次向多家藥局購買口罩的購買行為
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkPurchaseResultDto>> ProcessBulkPurchase(
            [FromBody] BulkPurchaseDto bulkPurchase)
        {
            try
            {
                if (string.IsNullOrEmpty(bulkPurchase.UserName))
                {
                    return BadRequest("用戶名稱不能為空");
                }

                if (!bulkPurchase.Purchases.Any())
                {
                    return BadRequest("購買項目不能為空");
                }

                var result = await _purchaseService.ProcessBulkPurchaseAsync(bulkPurchase);
                
                _logger.LogInformation($"🛒 用戶 {bulkPurchase.UserName} 批量購買結果: {(result.Success ? "成功" : "失敗")}");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用戶 {bulkPurchase.UserName} 批量購買時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 處理用戶一次向多家藥局購買口罩的購買行為
        /// </summary>
        [HttpPost("bulk2")]
        public async Task<ActionResult<BulkPurchaseResultDto>> ProcessBulkPurchase_(
            [FromBody] BulkPurchaseDto_ bulkPurchase)
        {
            try
            {
                if (bulkPurchase.UserId==null)
                {
                    return BadRequest("用戶ID不能為空");
                }

                if (!bulkPurchase.Purchases.Any())
                {
                    return BadRequest("購買項目不能為空");
                }

                var result = await _purchaseService.ProcessBulkPurchaseAsync_(bulkPurchase);

                _logger.LogInformation($"🛒 用戶 {bulkPurchase.UserId} 批量購買結果: {(result.Success ? "成功" : "失敗")}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用戶 {bulkPurchase.UserId} 批量購買時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }



        /// <summary>
        /// 取得用戶購買記錄
        /// </summary>
        [HttpGet("by-user/{userName}")]
        public async Task<ActionResult<List<PurchaseDto>>> GetUserPurchases(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    return BadRequest("用戶名稱不能為空");
                }

                var purchases = await _purchaseService.GetUserPurchasesAsync(userName);
                _logger.LogInformation($"📋 用戶 {userName} 有 {purchases.Count} 筆購買記錄");
                
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得用戶 {userName} 購買記錄時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 取得特定日期範圍內的購買記錄
        /// </summary>
        [HttpGet("by-date-range")]
        public async Task<ActionResult<List<PurchaseDto>>> GetPurchasesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("開始日期不能晚於結束日期");
                }

                var purchases = await _purchaseService.GetPurchasesByDateRangeAsync(startDate, endDate);
                _logger.LogInformation($"📅 日期範圍 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 有 {purchases.Count} 筆購買記錄");
                
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得日期範圍購買記錄時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 取得購買分析資料
        /// </summary>
        [HttpGet("analytics")]
        public async Task<ActionResult<PurchaseAnalyticsDto>> GetPurchaseAnalytics(
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
                _logger.LogInformation($"📊 生成購買分析報表: {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd}");
                
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成購買分析報表時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }
    }
}
