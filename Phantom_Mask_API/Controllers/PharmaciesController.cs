using Microsoft.AspNetCore.Mvc;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PharmaciesController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly IMaskService _maskService;
        private readonly ILogger<PharmaciesController> _logger;

        public PharmaciesController(
            IPharmacyService pharmacyService,
            IMaskService maskService,
            ILogger<PharmaciesController> logger)
        {
            _pharmacyService = pharmacyService;
            _maskService = maskService;
            _logger = logger;
        }

        /// <summary>
        /// 列出藥局，可選擇依特定時間和/或星期幾進行篩選
        /// </summary>
        /// <param name="searchName">藥局名稱篩選</param>
        /// <param name="startTime">營業開始時間篩選 (格式: "08:00", 24小時制)</param>
        /// <param name="endTime">營業結束時間篩選 (格式: "18:00", 24小時制)</param>
        /// <param name="dayOfWeek">星期篩選 (1=Mon, 2=Tue, 3=Wed, 4=Thur, 5=Fri, 6=Sat, 7=Sun)</param>
        [HttpGet]
        public async Task<ActionResult<List<PharmacyDto>>> GetPharmacies(
            [FromQuery] string? searchName = null,
            [FromQuery] string? startTime = null,
            [FromQuery] string? endTime = null,
            [FromQuery] int? dayOfWeek = null)
        {
            try
            {
                // 驗證 dayOfWeek 範圍
                if (dayOfWeek.HasValue && (dayOfWeek < 1 || dayOfWeek > 7))
                {
                    return BadRequest("dayOfWeek 必須在 1-7 範圍內 (1=Mon, 2=Tue, 3=Wed, 4=Thur, 5=Fri, 6=Sat, 7=Sun)");
                }

                // 驗證時間格式
                if (!string.IsNullOrEmpty(startTime) && !IsValidTimeFormat(startTime))
                {
                    return BadRequest("startTime 格式錯誤，請使用 24 小時制格式 (例如: '08:00')");
                }

                if (!string.IsNullOrEmpty(endTime) && !IsValidTimeFormat(endTime))
                {
                    return BadRequest("endTime 格式錯誤，請使用 24 小時制格式 (例如: '18:00')");
                }

                var filter = new PharmacyFilterDto
                {
                    SearchName = searchName,
                    StartTime = startTime,
                    EndTime = endTime,
                    DayOfWeek = dayOfWeek
                };

                var pharmacies = await _pharmacyService.GetPharmaciesAsync(filter);
                _logger.LogInformation($"🏥 篩選條件找到 {pharmacies.Count} 間藥局");
                return Ok(pharmacies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得藥局列表時發生錯誤");
                return StatusCode(500, $"取得藥局列表時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 驗證時間格式是否正確 (HH:MM)
        /// </summary>
        private bool IsValidTimeFormat(string time)
        {
            if (string.IsNullOrEmpty(time)) return false;
            
            return TimeOnly.TryParseExact(time, "HH:mm", out _);
        }

        /// <summary>
        /// 列出特定藥局銷售的所有口罩，並可依名稱或價格排序
        /// </summary>
        /// <param name="id">藥局ID</param>
        /// <param name="sortBy">排序欄位 ("name" 或 "price")</param>
        /// <param name="sortOrder">排序方向 ("asc" 或 "desc")</param>
        [HttpGet("{id}/masks")]
        public async Task<ActionResult<List<MaskDto>>> GetPharmacyMasks(
            int id,
            [FromQuery] string sortBy = "name",
            [FromQuery] string sortOrder = "asc")
        {
            try
            {
                var masks = await _pharmacyService.GetPharmacyMasksAsync(id, sortBy, sortOrder);
                _logger.LogInformation($"💊 藥局 ID {id} 找到 {masks.Count} 個口罩");
                return Ok(masks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得藥局 ID {id} 口罩時發生錯誤");
                return StatusCode(500, $"取得口罩資料時發生錯誤: {ex.Message}");
            }
        }

        ///// <summary>
        ///// 列出所有在給定價格範圍內提供一定數量口罩產品的藥店
        ///// </summary>
        ///// <param name="minPrice">最低價格</param>
        ///// <param name="maxPrice">最高價格</param>
        ///// <param name="stockThreshold">庫存閾值</param>
        ///// <param name="stockComparison">比較方式 ("above", "below", "between")</param>
        //[HttpGet("by-stock")]
        //public async Task<ActionResult<List<PharmacyDto>>> GetPharmaciesByStock(
        //    [FromQuery] decimal minPrice,
        //    [FromQuery] decimal maxPrice,
        //    [FromQuery] int stockThreshold,
        //    [FromQuery] string stockComparison = "above")
        //{
        //    try
        //    {
        //        var pharmacies = await _pharmacyService.GetPharmaciesByStockCriteriaAsync_(
        //            minPrice, maxPrice, stockThreshold, stockComparison);
        //        _logger.LogInformation($"📊 股票篩選找到 {pharmacies.Count} 間藥局");
        //        return Ok(pharmacies);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "依庫存篩選藥局時發生錯誤");
        //        return StatusCode(500, $"篩選藥局時發生錯誤: {ex.Message}");
        //    }
        //}
        /// <summary>
        /// 列出所有在給定價格範圍內提供一定數量口罩產品的藥店
        /// </summary>
        /// <param name="minPrice">最低價格</param>
        /// <param name="maxPrice">最高價格</param>
        /// <param name="minStockThreshold">最小庫存閾值（可空）</param>
        /// <param name="maxStockThreshold">最大庫存閾值（可空）</param>
        /// <param name="isInclusive">是否包含等於（預設 false）</param>
        [HttpGet("by-stock")]
        public async Task<ActionResult<List<PharmacyDto>>> GetPharmaciesByStock(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice,
            [FromQuery] int? minStockThreshold = null,
            [FromQuery] int? maxStockThreshold = null,
            [FromQuery] bool isInclusive = false)
        {
            try
            {
                var pharmacies = await _pharmacyService.GetPharmaciesByStockCriteriaAsync(
                    minPrice, maxPrice, minStockThreshold, maxStockThreshold, isInclusive);

                _logger.LogInformation($"📊 庫存篩選找到 {pharmacies.Count} 間藥局");
                return Ok(pharmacies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "依庫存篩選藥局時發生錯誤");
                return StatusCode(500, $"篩選藥局時發生錯誤: {ex.Message}");
            }
        }
        /// <summary>
        /// 根據 ID 取得特定藥局
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PharmacyDto>> GetPharmacy(int id)
        {
            try
            {
                var pharmacy = await _pharmacyService.GetPharmacyByIdAsync(id);
                if (pharmacy == null)
                {
                    _logger.LogWarning($"⚠️ 找不到 ID 為 {id} 的藥局");
                    return NotFound($"找不到 ID 為 {id} 的藥局");
                }

                _logger.LogInformation($"🏥 找到藥局：{pharmacy.Name}");
                return Ok(pharmacy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得藥局 ID {id} 時發生錯誤");
                return StatusCode(500, $"取得藥局資料時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 一次為藥局創建或更新多個口罩產品，包括名稱、價格和庫存數量
        /// </summary>
        /// <param name="id">藥局ID</param>
        /// <param name="maskUpdates">口罩更新資料列表</param>
        [HttpPost("{id}/masks/bulk")]
        public async Task<ActionResult<List<MaskDto>>> BulkUpdateMasks(
            int id,
            [FromBody] List<BulkMaskUpdateDto> maskUpdates)
        {
            try
            {
                var result = await _maskService.BulkUpdateMasksAsync(id, maskUpdates);
                _logger.LogInformation($"💊 藥局 ID {id} 成功批量更新 {result.Count} 個口罩");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"藥局 ID {id} 批量更新口罩時發生錯誤");
                return StatusCode(500, $"批量更新口罩時發生錯誤: {ex.Message}");
            }
        }

        // /// <summary>
        // /// 新增藥局
        // /// </summary>
        // [HttpPost]
        // public async Task<ActionResult<PharmacyDto>> CreatePharmacy([FromBody] PharmacyDto pharmacyDto)
        // {
        //     try
        //     {
        //         var createdPharmacy = await _pharmacyService.CreatePharmacyAsync(pharmacyDto);
        //         _logger.LogInformation($"✅ 成功建立藥局：{createdPharmacy.Name}");
        //         return CreatedAtAction(nameof(GetPharmacy), 
        //             new { id = createdPharmacy.Id }, createdPharmacy);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"建立藥局 {pharmacyDto.Name} 時發生錯誤");
        //         return StatusCode(500, $"建立藥局時發生錯誤: {ex.Message}");
        //     }
        // }

        // /// <summary>
        // /// 更新藥局資訊
        // /// </summary>
        // [HttpPut("{id}")]
        // public async Task<ActionResult<PharmacyDto>> UpdatePharmacy(int id, [FromBody] PharmacyDto pharmacyDto)
        // {
        //     try
        //     {
        //         pharmacyDto.Id = id;
        //         var updatedPharmacy = await _pharmacyService.UpdatePharmacyAsync(pharmacyDto);
                
        //         if (updatedPharmacy == null)
        //         {
        //             _logger.LogWarning($"⚠️ 找不到 ID 為 {id} 的藥局進行更新");
        //             return NotFound($"找不到 ID 為 {id} 的藥局");
        //         }

        //         _logger.LogInformation($"� 成功更新藥局：{updatedPharmacy.Name}");
        //         return Ok(updatedPharmacy);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"更新藥局 ID {id} 時發生錯誤");
        //         return StatusCode(500, $"更新藥局時發生錯誤: {ex.Message}");
        //     }
        // }

        // /// <summary>
        // /// 刪除藥局
        // /// </summary>
        // [HttpDelete("{id}")]
        // public async Task<ActionResult> DeletePharmacy(int id)
        // {
        //     try
        //     {
        //         var result = await _pharmacyService.DeletePharmacyAsync(id);
        //         if (!result)
        //         {
        //             _logger.LogWarning($"⚠️ 找不到 ID 為 {id} 的藥局進行刪除");
        //             return NotFound($"找不到 ID 為 {id} 的藥局");
        //         }

        //         _logger.LogInformation($"�️ 成功刪除藥局 ID {id}");
        //         return NoContent();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"刪除藥局 ID {id} 時發生錯誤");
        //         return StatusCode(500, $"刪除藥局時發生錯誤: {ex.Message}");
        //     }
        // }
    }
}
