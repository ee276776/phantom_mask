using PhantomMaskAPI.Models.DTOs;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.Entities;

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

            List<PharmacyDto> result = pharmacies.Any()?pharmacies.Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                CashBalance = p.CashBalance,
                OpeningHours = p.OpeningHours,
                CreatedAt = p.CreatedAt,
                MaskTypeCount = p.Masks?.Count ?? 0, // 如果 p.Masks 為 null，則 MaskTypeCount 為 0
                MaskTotalCount = p.Masks?.Sum(m => m.StockQuantity) ?? 0 // 如果 p.Masks 為 null，則 MaskTotalCount 為 0
            }).ToList(): new List<PharmacyDto>(); 

            _logger.LogInformation($"🏥 找到 {result.Count} 家藥局");
            return result;
        }

        //public async Task<PharmacyDto?> GetPharmacyByIdAsync(int id)
        //{
        //    var pharmacy = await _pharmacyRepository.GetPharmacyWithMasksAsync(id);
        //    if (pharmacy == null) return null;

        //    return new PharmacyDto
        //    {
        //        Id = pharmacy.Id,
        //        Name = pharmacy.Name,
        //        CashBalance = pharmacy.CashBalance,
        //        OpeningHours = pharmacy.OpeningHours,
        //        CreatedAt = pharmacy.CreatedAt,
        //        MaskTypeCount = pharmacy.Masks.Count,
        //        MaskTotalCount = pharmacy.Masks.Sum(m => m.StockQuantity)
        //    };
        //}

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
                PharmacyName = m.Pharmacy?.Name,
                CreatedAt = m.CreatedAt
            }).ToList();

            _logger.LogInformation($"😷 藥局 {pharmacyId} 有 {result.Count} 個口罩產品");
            return result;
        }

        //public async Task<List<PharmacyDto>> SearchPharmaciesAsync(string searchTerm)
        //{
        //    var pharmacies = await _pharmacyRepository.SearchPharmaciesAsync(searchTerm);

        //    var result = pharmacies.Select(p => new PharmacyDto
        //    {
        //        Id = p.Id,
        //        Name = p.Name,
        //        CashBalance = p.CashBalance,
        //        OpeningHours = p.OpeningHours,
        //        CreatedAt = p.CreatedAt,
        //        MaskTypeCount = p.Masks.Count,
        //        MaskTotalCount = p.Masks.Sum(m => m.StockQuantity)
        //    }).ToList();

        //    _logger.LogInformation($"� 搜尋 '{searchTerm}' 找到 {result.Count} 家藥局");
        //    return result;
        //}

        
        public async Task<List<PharmacyDto>> GetPharmaciesByStockCriteriaAsync(
            decimal minPrice,
            decimal maxPrice,
            int? minStockThreshold,
            int? maxStockThreshold,
            bool isInclusive=false)
        {
            var pharmacies = await _pharmacyRepository.GetPharmaciesWithMasksAsync();

            var filtered = pharmacies.Where(p =>
            {
                // 確保 p.Masks 不為 null
                var currentPharmacyMasks = p.Masks?? new List<Mask>();

                // *** 新增價格篩選邏輯 ***
                // 判斷該藥局是否有至少一個口罩的價格落在 minPrice 和 maxPrice 之間
                bool priceMatches = (minPrice == 0 && maxPrice == 0) || // 如果價格範圍是預設的0-0，則不進行價格篩選
                                    currentPharmacyMasks.Any(m => m.Price >= minPrice && m.Price <= maxPrice);

                // 如果該藥局連一個符合價格條件的口罩都沒有，則直接排除
                if (!priceMatches)
                {
                    return false;
                }

                // 口罩總數量篩選邏輯 (保持不變)
                int count = currentPharmacyMasks.Sum(m => m.StockQuantity);

                if (minStockThreshold.HasValue && maxStockThreshold.HasValue)
                {
                    return isInclusive
                        ? count >= minStockThreshold && count <= maxStockThreshold
                        : count > minStockThreshold && count < maxStockThreshold;
                }
                else if (minStockThreshold.HasValue)
                {
                    return isInclusive
                        ? count >= minStockThreshold
                        : count > minStockThreshold;
                }
                else if (maxStockThreshold.HasValue)
                {
                    return isInclusive
                        ? count <= maxStockThreshold
                        : count < maxStockThreshold;
                }

                return true; // 若庫存兩者都沒填，代表不過濾庫存數量
            });

            var result = filtered.Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                CashBalance = p.CashBalance,
                OpeningHours = p.OpeningHours,
                CreatedAt = p.CreatedAt,
                MaskTypeCount = p.Masks?.Count ?? 0, // 如果 p.Masks 為 null，則 MaskTypeCount 為 0
                MaskTotalCount = p.Masks?.Sum(m => m.StockQuantity) ?? 0 // 如果 p.Masks 為 null，則 MaskTotalCount 為 0
            }).ToList();

            _logger.LogInformation($"🏪 符合庫存條件的藥局: {result.Count} 家");
            return result;
        }

        /// <summary>
        /// 檢查藥局是否符合時間篩選條件
        /// </summary>
        private bool IsPharmacyMatchTimeFilter(string openingHours, int? dayOfWeek, string? startTime, string? endTime)
        {
            // 如果營業時間字串為空，則直接不符合條件
            if (string.IsNullOrEmpty(openingHours))
                return false;

            // 將數字轉換為星期名稱，用於匹配
            var dayNames = new[] { "", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }; // 注意: "Thur" 改為 "Thu" 更常見

            // 解析營業時間字串，預期格式如 "Mon 08:00 - 18:00, Tue 13:00 - 18:00"
            var daySchedules = openingHours.Split(',')
                                           .Select(s => s.Trim())
                                           .ToList();

            // 解析篩選時間（如果提供）
            TimeSpan filterStartTimeSpan = TimeSpan.MinValue; // 預設為最小值
            if (!string.IsNullOrEmpty(startTime))
            {
                if (!TimeSpan.TryParse(startTime, out filterStartTimeSpan))
                {
                    // 如果篩選開始時間格式不正確，則此篩選條件無效，可以視為不符合
                    // 這裡選擇直接返回 false，表示不符合此篩選條件
                    // 或者可以選擇日誌記錄警告並忽略該篩選時間，讓其他條件判斷
                    // 根據您的需求決定，這裡採嚴格模式
                    return false;
                }
            }

            TimeSpan filterEndTimeSpan = TimeSpan.MaxValue; // 預設為最大值
            if (!string.IsNullOrEmpty(endTime))
            {
                if (!TimeSpan.TryParse(endTime, out filterEndTimeSpan))
                {
                    // 如果篩選結束時間格式不正確，同上
                    return false;
                }
            }

            // 遍歷每個營業時間段
            foreach (var schedule in daySchedules)
            {
                var parts = schedule.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // 使用 RemoveEmptyEntries

                // 檢查基本格式：至少要有 "Day Time1 - Time2" (4 parts)
                // 例如 "Mon 08:00 - 18:00"
                if (parts.Length < 4 || !parts[2].Equals("-")) // parts[2] 應該是分隔符 '-'
                {
                    continue; // 格式不正確，跳過此營業時間段
                }

                var dayName = parts[0]; // "Mon"
                                        // timeParts[0] 是 "08:00", timeParts[1] 是 "18:00"
                var scheduleStartTimeStr = parts[1];
                var scheduleEndTimeStr = parts[3];

                // 1. 檢查星期是否符合
                bool dayMatchesCurrentSchedule = false;
                if (dayOfWeek.HasValue)
                {
                    // 轉換篩選的 dayOfWeek 為星期名稱，用於匹配
                    if (dayOfWeek.Value >= 1 && dayOfWeek.Value <= 7) // 確保 dayOfWeek 在有效範圍內
                    {
                        var targetDayName = dayNames[dayOfWeek.Value];
                        if (dayName.Equals(targetDayName, StringComparison.OrdinalIgnoreCase))
                        {
                            dayMatchesCurrentSchedule = true;
                        }
                        // 特殊處理 "Mon-Fri" 這種範圍
                        else if (dayName.Contains("-")) // 檢查是否為 "X-Y" 格式
                        {
                            var rangeDays = dayName.Split('-');
                            if (rangeDays.Length == 2)
                            {
                                int startIdx = Array.IndexOf(dayNames, rangeDays[0]);
                                int endIdx = Array.IndexOf(dayNames, rangeDays[1]);

                                if (startIdx != -1 && endIdx != -1 && dayOfWeek.Value >= startIdx && dayOfWeek.Value <= endIdx)
                                {
                                    dayMatchesCurrentSchedule = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 如果 dayOfWeek 沒有指定，則任何一天都符合星期的篩選條件
                    dayMatchesCurrentSchedule = true;
                }

                if (!dayMatchesCurrentSchedule)
                {
                    continue; // 如果當前排程的星期不符合篩選條件，則跳過
                }

                // 2. 解析排程的時間並檢查時間是否符合
                TimeSpan scheduleStartTimeSpan, scheduleEndTimeSpan;
                if (!TimeSpan.TryParse(scheduleStartTimeStr, out scheduleStartTimeSpan) ||
                    !TimeSpan.TryParse(scheduleEndTimeStr, out scheduleEndTimeSpan))
                {
                    // 如果營業時間的開始或結束時間格式不正確，跳過此排程
                    continue;
                }

                // 檢查時間區間是否有重疊
                // 篩選時間段：[filterStartTimeSpan, filterEndTimeSpan]
                // 藥局營業時間：[scheduleStartTimeSpan, scheduleEndTimeSpan]
                // 重疊條件：filterReqStartTime < scheduleEndTimeSpan && filterReqEndTime > scheduleStartTimeSpan
                bool timeOverlap = (filterStartTimeSpan < scheduleEndTimeSpan && filterEndTimeSpan > scheduleStartTimeSpan);

                // 如果這個排程的時間與篩選時間有重疊，並且星期也符合（或未指定篩選星期），則該藥局符合條件
                if (timeOverlap)
                {
                    return true; // 找到一個符合條件的營業時間段，立即返回 true
                }
            }

            // 遍歷所有營業時間段後，如果都沒有找到符合條件的，則返回 false
            return false;
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
