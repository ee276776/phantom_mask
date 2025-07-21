using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhantomMaskETL.Configuration;
using PhantomMaskETL.Data;
using PhantomMaskETL.Services;

namespace PhantomMaskETL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🎭 PhantomMask ETL 程式啟動中...");

            var host = CreateHostBuilder(args).Build();

            try
            {
                // 檢查資料庫連線
                using (var scope = host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<PhantomMaskContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    
                    logger.LogInformation("🔧 檢查資料庫連線...");
                    await context.Database.CanConnectAsync();
                    logger.LogInformation("✅ 資料庫連線正常");
                }

                // 執行 ETL
                var etlService = host.Services.GetRequiredService<IETLService>();
                await etlService.ProcessAsync();

                Console.WriteLine("🎉 ETL 程式執行完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 錯誤：{ex.Message}");
                Console.WriteLine($"詳細錯誤：{ex}");
                Environment.Exit(1);
            }

            // 只有在非容器環境下才等待用戶輸入
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
            {
                Console.WriteLine("按任意鍵結束...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("✅ ETL 處理程序在容器中完成");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    // 註冊加密服務
                    services.AddSingleton<IEncryptionService, EncryptionService>();
                    services.AddSingleton<ISecureConnectionService, SecureConnectionService>();

                    // 使用安全的連線字串服務
                    services.AddDbContext<PhantomMaskContext>((serviceProvider, options) =>
                    {
                        var secureConnectionService = serviceProvider.GetRequiredService<ISecureConnectionService>();
                        var connectionString = secureConnectionService.GetConnectionString();
                        options.UseNpgsql(connectionString);
                    });

                    services.AddScoped<IETLService, ETLService>();
                });
    }
}
