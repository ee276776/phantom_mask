using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhantomMaskETL.Converters;
using PhantomMaskETL.Data;
using PhantomMaskETL.Data.Entities;
using PhantomMaskETL.Models;

namespace PhantomMaskETL.Services
{
    public interface IETLService
    {
        Task ProcessAsync();
    }

    public class ETLService : IETLService
    {
        private readonly PhantomMaskContext _context;
        private readonly ILogger<ETLService> _logger;
        private const string DataPath = "/app/data";

        public ETLService(PhantomMaskContext context, ILogger<ETLService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessAsync()
        {
            try
            {
                _logger.LogInformation("開始 ETL 處理程序...");

                // Extract
                var users = await ExtractUsersAsync();
                var pharmacies = await ExtractPharmaciesAsync();

                _logger.LogInformation($"提取到 {users.Count} 位用戶，{pharmacies.Count} 家藥房");

                // Transform & Load
                await TransformAndLoadUsersAsync(users);
                await TransformAndLoadPharmaciesAsync(pharmacies);
                await TransformAndLoadPurchasesAsync(users);

                _logger.LogInformation("✅ ETL 處理完成！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ETL 處理失敗");
                throw;
            }
        }

        private async Task<List<User>> ExtractUsersAsync()
        {
            _logger.LogInformation("📥 正在提取用戶資料...");
            
            string filePath = Path.Combine(DataPath, "users.json");
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"找不到檔案：{filePath}");
                return new List<User>();
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateTimeConverter(), new NullableDateTimeConverter() }
            };
            
            var users = JsonSerializer.Deserialize<List<User>>(jsonContent, options) ?? new List<User>();
            
            // 過濾掉空的用戶名稱
            var validUsers = users.Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToList();
            
            _logger.LogInformation($"✅ 成功提取 {validUsers.Count} 位用戶資料");
            return validUsers;
        }

        private async Task<List<Pharmacy>> ExtractPharmaciesAsync()
        {
            _logger.LogInformation("📥 正在提取藥房資料...");
            
            string filePath = Path.Combine(DataPath, "pharmacies.json");
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"找不到檔案：{filePath}");
                return new List<Pharmacy>();
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateTimeConverter(), new NullableDateTimeConverter() }
            };
            
            var pharmacies = JsonSerializer.Deserialize<List<Pharmacy>>(jsonContent, options) ?? new List<Pharmacy>();
            
            // 過濾掉空的藥房名稱
            var validPharmacies = pharmacies.Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToList();
            
            _logger.LogInformation($"✅ 成功提取 {validPharmacies.Count} 家藥房資料");
            return validPharmacies;
        }

        private async Task TransformAndLoadUsersAsync(List<User> users)
        {
            _logger.LogInformation("🔄 處理用戶資料...");

            foreach (var user in users)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Name == user.Name);

                if (existingUser == null)
                {
                    _context.Users.Add(new UserEntity
                    {
                        Name = user.Name!,
                        CashBalance = user.CashBalance
                    });
                }
                else
                {
                    existingUser.CashBalance = user.CashBalance;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ 成功處理 {users.Count} 位用戶");
        }

        private async Task TransformAndLoadPharmaciesAsync(List<Pharmacy> pharmacies)
        {
            _logger.LogInformation("🔄 處理藥房資料...");

            foreach (var pharmacy in pharmacies)
            {
                var existingPharmacy = await _context.Pharmacies
                    .Include(p => p.Masks)
                    .FirstOrDefaultAsync(p => p.Name == pharmacy.Name);

                if (existingPharmacy == null)
                {
                    var pharmacyEntity = new PharmacyEntity
                    {
                        Name = pharmacy.Name,
                        CashBalance = pharmacy.CashBalance,
                        OpeningHours = pharmacy.OpeningHours
                    };

                    _context.Pharmacies.Add(pharmacyEntity);
                    await _context.SaveChangesAsync();

                    // 添加口罩資料
                    foreach (var mask in pharmacy.Masks.Where(m => !string.IsNullOrWhiteSpace(m.Name)))
                    {
                        _context.Masks.Add(new MaskEntity
                        {
                            Name = mask.Name,
                            Price = mask.Price,
                            StockQuantity = mask.StockQuantity,
                            PharmacyId = pharmacyEntity.Id
                        });
                    }
                }
                else
                {
                    // 更新現有藥房資料
                    existingPharmacy.CashBalance = pharmacy.CashBalance;
                    existingPharmacy.OpeningHours = pharmacy.OpeningHours;

                    // 清除舊口罩資料並重新載入
                    _context.Masks.RemoveRange(existingPharmacy.Masks);

                    foreach (var mask in pharmacy.Masks.Where(m => !string.IsNullOrWhiteSpace(m.Name)))
                    {
                        _context.Masks.Add(new MaskEntity
                        {
                            Name = mask.Name,
                            Price = mask.Price,
                            StockQuantity = mask.StockQuantity,
                            PharmacyId = existingPharmacy.Id
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ 成功處理 {pharmacies.Count} 家藥房");
        }

        private async Task TransformAndLoadPurchasesAsync(List<User> users)
        {
            _logger.LogInformation("🔄 處理購買記錄...");

            // 清除舊的購買記錄
            _context.Purchases.RemoveRange(_context.Purchases);
            await _context.SaveChangesAsync();

            int totalPurchases = 0;

            foreach (var user in users)
            {
                foreach (var purchase in user.PurchaseHistories.Where(p => 
                    !string.IsNullOrWhiteSpace(p.PharmacyName) && 
                    !string.IsNullOrWhiteSpace(p.MaskName)))
                {
                    // 確保 DateTime 為 UTC 格式
                    var transactionDateTime = purchase.TransactionDatetime.Kind == DateTimeKind.Utc 
                        ? purchase.TransactionDatetime 
                        : DateTime.SpecifyKind(purchase.TransactionDatetime, DateTimeKind.Utc);

                    _context.Purchases.Add(new PurchaseEntity
                    {
                        UserName = user.Name!,
                        PharmacyName = purchase.PharmacyName!,
                        MaskName = purchase.MaskName!,
                        TransactionAmount = purchase.TransactionAmount ?? 0,
                        TransactionQuantity = purchase.TransactionQuantity ?? 0,
                        TransactionDatetime = transactionDateTime
                    });

                    totalPurchases++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ 成功處理 {totalPurchases} 筆購買記錄");
        }
    }
}
