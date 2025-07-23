using Microsoft.AspNetCore.Mvc;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasksController : ControllerBase
    {
        private readonly IMaskService _maskService;
        private readonly IMaskRepository _maskRepository;
        private readonly ILogger<MasksController> _logger;

        public MasksController(
            IMaskService maskService,
            IMaskRepository maskRepository,
            ILogger<MasksController> logger)
        {
            _maskRepository = maskRepository;
            _maskService = maskService;
            _logger = logger;
        }

        /// <summary>
        /// 搜尋口罩
        /// </summary>
        [HttpGet("search")]
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

                var masks = await _maskService.SearchMasksAsync(query, limit);
                _logger.LogInformation($"😷 搜尋 '{query}' 找到 {masks.Count} 個口罩");
                
                return Ok(masks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"搜尋口罩 '{query}' 時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 取得特定口罩資訊
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MaskDto>> GetMask(int id)
        {
            try
            {
                var mask = await _maskService.GetMaskByIdAsync(id);
                if (mask == null)
                {
                    return NotFound($"找不到 ID 為 {id} 的口罩");
                }

                return Ok(mask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得口罩 ID {id} 時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// [Q6] ※※ 更新口罩庫存 - 透過增加或減少來更新現有口罩產品的庫存數量 ※※
        /// </summary>
        [HttpPut("{id}/stock")]
        public async Task<ActionResult<MaskDto>> UpdateMaskStock(
            int id,
            [FromBody] StockUpdateDto stockUpdate)
        {
            try
            {
                if (stockUpdate.Quantity <= 0)
                {
                    return BadRequest("數量必須大於 0");
                }

                if (stockUpdate.Operation != "increase" && stockUpdate.Operation != "decrease")
                {
                    return BadRequest("操作必須是 'increase' 或 'decrease'");
                }

              
                
                var updatedMask = await _maskService.UpdateMaskStockAsync(id, stockUpdate);
                if (updatedMask == null)
                {
                    return NotFound($"找不到 ID 為 {id} 的口罩");
                }

                _logger.LogInformation($"📦 口罩 ID {id} 庫存已{(stockUpdate.Operation == "increase" ? "增加" : "減少")} {stockUpdate.Quantity}");
                return Ok(updatedMask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新口罩 ID {id} 庫存時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 取得價格範圍內的口罩
        /// </summary>
        [HttpGet("by-price-range")]
        public async Task<ActionResult<List<MaskDto>>> GetMasksByPriceRange(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                {
                    return BadRequest("價格範圍無效");
                }

                var masks = await _maskService.GetMasksInPriceRangeAsync(minPrice, maxPrice);
                _logger.LogInformation($"💰 價格範圍 {minPrice}-{maxPrice} 找到 {masks.Count} 個口罩");
                
                return Ok(masks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得價格範圍 {minPrice}-{maxPrice} 口罩時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 取得低庫存口罩
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<ActionResult<List<MaskDto>>> GetLowStockMasks(
            [FromQuery] int threshold = 10)
        {
            try
            {
                var masks = await _maskService.GetLowStockMasksAsync(threshold);
                _logger.LogInformation($"⚠️ 找到 {masks.Count} 個低庫存口罩（閾值: {threshold}）");
                
                return Ok(masks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得低庫存口罩時發生錯誤");
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// [Q7] ※※ 新增或更新多筆口罩資訊 (不含異動藥局現金餘額CashBalance) ※※
        /// </summary>
        /// <param name="pharmacyId">藥局 ID</param>
        /// <param name="maskDto">口罩資料列表</param>
        [HttpPost("upsert")]
        public async Task<ActionResult<List<MaskDto>>> UpsertMask(int pharmacyId, [FromBody] List<MaskUpsertDto> maskDto)
        {
            if (maskDto == null || !maskDto.Any())
                return BadRequest("請提供至少一筆口罩資料");

            try
            {
                var result = await _maskService.UpsertMasksAsync(pharmacyId, maskDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"新增或更新藥局 {pharmacyId} 的口罩資料時發生錯誤");
                return StatusCode(500, "伺服器錯誤，無法處理口罩資料");
            }
        }
    }
}
