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
        /// [Q5] ※※ 處理用戶一次向多家藥局購買口罩的購買行為 ※※
        /// </summary>
        [HttpPost("bulk")]
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



      
    }
}
