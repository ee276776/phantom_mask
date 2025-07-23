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
        /// [Q1] ※※ 列出藥局，可選擇依特定時間和/或星期幾進行篩選  ※※
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
        /// [Q2] ※※ 列出特定藥局銷售的所有口罩，並可依名稱或價格排序 ※※
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

        

        /// <summary>
        /// [Q3] ※※ 列出所有在給定價格範圍內提供一定數量口罩產品的藥店 ※※
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
      
    }
}
