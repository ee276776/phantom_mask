using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Services
{
    public class PharmacyService : IPharmacyService
    {
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IMaskRepository _maskRepository;
        private readonly ILogger<PharmacyService> _logger;

        public PharmacyService(
            IPharmacyRepository pharmacyRepository,
            IMaskRepository maskRepository,
            ILogger<PharmacyService> logger)
        {
            _pharmacyRepository = pharmacyRepository;
            _maskRepository = maskRepository;
            _logger = logger;
        }

        public async Task<List<PharmacyDto>> GetPharmaciesAsync(PharmacyFilterDto filter)
        {
            var pharmacies = await _pharmacyRepository.GetPharmaciesWithMasksAsync();

            // 應用篩選條件
            if (!string.IsNullOrEmpty(filter.SearchName))
            {
                pharmacies = pharmacies.Where(p => p.Name.ToLower().Contains(filter.SearchName.ToLower())).ToList();
            }

            // 根據 dayOfWeek, startTime, endTime 進行篩選
            if (filter.DayOfWeek.HasValue || !string.IsNullOrEmpty(filter.StartTime) || !string.IsNullOrEmpty(filter.EndTime))
            {
                pharmacies = pharmacies.Where(p => IsPharmacyMatchTimeFilter(p.OpeningHours, filter.DayOfWeek, filter.StartTime, filter.EndTime)).ToList();
            }

            var result = pharmacies.Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                CashBalance = p.CashBalance,
                OpeningHours = p.OpeningHours,
                CreatedAt = p.CreatedAt,
                MaskCount = p.Masks.Count
            }).ToList();

            _logger.LogInformation($"🏥 找到 {result.Count} 家藥局");
            return result;
        }

        public async Task<PharmacyDto?> GetPharmacyByIdAsync(int id)
        {
            var pharmacy = await _pharmacyRepository.GetPharmacyWithMasksAsync(id);
            if (pharmacy == null) return null;

            return new PharmacyDto
            {
                Id = pharmacy.Id,
                Name = pharmacy.Name,
                CashBalance = pharmacy.CashBalance,
                OpeningHours = pharmacy.OpeningHours,
                CreatedAt = pharmacy.CreatedAt,
                MaskCount = pharmacy.Masks.Count
            };
        }

        public async Task<List<MaskDto>> GetPharmacyMasksAsync(int pharmacyId, string? sortBy, string? sortOrder)
        {
            var masks = await _maskRepository.GetMasksByPharmacyIdAsync(pharmacyId);

            // 排序
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isAsc = string.IsNullOrEmpty(sortOrder) || sortOrder.ToLower() == "asc";
                
                masks = sortBy.ToLower() switch
                {
                    "name" => isAsc ? masks.OrderBy(m => m.Name).ToList() : masks.OrderByDescending(m => m.Name).ToList(),
                    "price" => isAsc ? masks.OrderBy(m => m.Price).ToList() : masks.OrderByDescending(m => m.Price).ToList(),
                    "stock" => isAsc ? masks.OrderBy(m => m.StockQuantity).ToList() : masks.OrderByDescending(m => m.StockQuantity).ToList(),
                    _ => masks.OrderBy(m => m.Name).ToList()
                };
            }

            var result = masks.Select(m => new MaskDto
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                StockQuantity = m.StockQuantity,
                PharmacyId = m.PharmacyId,
                PharmacyName = m.Pharmacy.Name,
                CreatedAt = m.CreatedAt
            }).ToList();

            _logger.LogInformation($"😷 藥局 {pharmacyId} 有 {result.Count} 個口罩產品");
            return result;
        }

        public async Task<List<PharmacyDto>> SearchPharmaciesAsync(string searchTerm)
        {
            var pharmacies = await _pharmacyRepository.SearchPharmaciesAsync(searchTerm);

            var result = pharmacies.Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                CashBalance = p.CashBalance,
                OpeningHours = p.OpeningHours,
                CreatedAt = p.CreatedAt,
                MaskCount = p.Masks.Count
            }).ToList();

            _logger.LogInformation($"� 搜尋 '{searchTerm}' 找到 {result.Count} 家藥局");
            return result;
        }

        public async Task<List<PharmacyDto>> GetPharmaciesByStockCriteriaAsync(decimal minPrice, decimal maxPrice, int stockThreshold, string comparison)
        {
            var pharmacies = await _pharmacyRepository.GetPharmaciesByStockCriteriaAsync(minPrice, maxPrice, stockThreshold, comparison);

            var result = pharmacies.Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                CashBalance = p.CashBalance,
                OpeningHours = p.OpeningHours,
                CreatedAt = p.CreatedAt,
                MaskCount = p.Masks.Count
            }).ToList();

            _logger.LogInformation($"🏪 符合庫存條件的藥局: {result.Count} 家");
            return result;
        }

        /// <summary>
        /// 檢查藥局是否符合時間篩選條件
        /// </summary>
        private bool IsPharmacyMatchTimeFilter(string openingHours, int? dayOfWeek, string? startTime, string? endTime)
        {
            if (string.IsNullOrEmpty(openingHours))
                return false;

            // 將數字轉換為星期名稱
            var dayNames = new[] { "", "Mon", "Tue", "Wed", "Thur", "Fri", "Sat", "Sun" };
            
            // 解析營業時間字串: "Mon 08:00 - 18:00, Tue 13:00 - 18:00, Wed 08:00 - 18:00, Thur 13:00 - 18:00, Fri 08:00 - 18:00"
            var daySchedules = openingHours.Split(',').Select(s => s.Trim()).ToList();
            
            foreach (var schedule in daySchedules)
            {
                var parts = schedule.Split(' ');
                if (parts.Length < 4) continue; // 格式不正確，跳過
                
                var dayName = parts[0]; // Mon, Tue, Wed...
                var timeRange = $"{parts[1]} {parts[2]} {parts[3]}"; // 08:00 - 18:00
                
                // 如果指定了 dayOfWeek，只檢查該天；如果沒指定，則檢查所有天
                if (dayOfWeek.HasValue)
                {
                    var targetDayName = dayNames[dayOfWeek.Value];
                    if (!dayName.Equals(targetDayName, StringComparison.OrdinalIgnoreCase))
                        continue; // 不是指定的星期，跳過
                }
                // 如果沒有指定 dayOfWeek，則檢查任何一天是否符合時間條件
                
                // 解析時間範圍: "08:00 - 18:00"
                var timeParts = timeRange.Split('-').Select(t => t.Trim()).ToArray();
                if (timeParts.Length != 2) continue;
                
                var scheduleStartTime = timeParts[0]; // "08:00"
                var scheduleEndTime = timeParts[1];   // "18:00"
                
                // 檢查時間條件
                bool timeMatches = true;
                
                // 檢查 startTime 條件 (如果藥局開始營業時間晚於指定時間，則不符合)
                if (!string.IsNullOrEmpty(startTime))
                {
                    if (CompareTime(scheduleStartTime, startTime) > 0) 
                        timeMatches = false;
                }
                
                // 檢查 endTime 條件 (如果藥局結束營業時間早於指定時間，則不符合)
                if (!string.IsNullOrEmpty(endTime) && timeMatches)
                {
                    if (CompareTime(scheduleEndTime, endTime) < 0) 
                        timeMatches = false;
                }
                
                // 如果時間條件都符合，則返回 true
                if (timeMatches)
                    return true;
            }
            
            return false; // 沒有找到符合條件的時間段
        }

        /// <summary>
        /// 比較兩個時間字串 (格式: "HH:mm")
        /// 返回值: < 0 表示 time1 < time2, 0 表示相等, > 0 表示 time1 > time2
        /// </summary>
        private int CompareTime(string time1, string time2)
        {
            if (TimeOnly.TryParseExact(time1, "HH:mm", out var t1) && 
                TimeOnly.TryParseExact(time2, "HH:mm", out var t2))
            {
                return t1.CompareTo(t2);
            }
            return 0; // 解析失敗時視為相等
        }
    }
}
