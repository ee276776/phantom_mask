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
        /// [Q6] ※※ 更新口罩庫存 - 透過增加或減少來更新現有口罩產品的庫存數量 ※※
        /// </summary>
        /// <param name="id">要更新庫存的口罩產品 ID。</param>
        /// <param name="stockUpdate">包含操作類型（增加或減少）及數量的庫存更新資料。</param>
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
