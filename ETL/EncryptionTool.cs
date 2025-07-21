using System;

class Program
{
    // static void Main(string[] args)
    // {
    //     Console.WriteLine("🔐 PhantomMask 連線字串加密工具");
    //     Console.WriteLine("===================================");

    //     if (args.Length < 2)
    //     {
    //         Console.WriteLine("使用方式: dotnet run <連線字串> <加密金鑰>");
    //         Console.WriteLine("範例: dotnet run \"Host=database;Database=phantom_mask;Username=phantom_user;Password=SecurePass123!;Port=5432\" \"PhantomMask2025SecretKey\"");
    //         return;
    //     }

    //     var connectionString = args[0];
    //     var encryptionKey = args[1];
    //     var encryptionService = new PhantomMaskETL.Services.EncryptionService();

    //     try
    //     {
    //         var encryptedConnectionString = encryptionService.Encrypt(connectionString, encryptionKey);
            
    //         Console.WriteLine("🔐 原始連線字串:");
    //         // 遮蔽密碼部分顯示
    //         var maskedConnectionString = connectionString.Replace(GetPasswordFromConnectionString(connectionString), "****");
    //         Console.WriteLine(maskedConnectionString);
    //         Console.WriteLine();
            
    //         Console.WriteLine("🔒 加密後的連線字串:");
    //         Console.WriteLine(encryptedConnectionString);
    //         Console.WriteLine();
            
    //         Console.WriteLine("🔧 環境變數設定 (請加入到 docker-compose.yml):");
    //         Console.WriteLine($"- ConnectionStrings__EncryptedDefaultConnection={encryptedConnectionString}");
    //         Console.WriteLine($"- Security__EncryptionKey={encryptionKey}");
    //         Console.WriteLine();
            
    //         // 驗證加密解密是否正確
    //         var decrypted = encryptionService.Decrypt(encryptedConnectionString, encryptionKey);
    //         Console.WriteLine("✅ 驗證: " + (decrypted == connectionString ? "加密解密成功!" : "加密解密失敗!"));
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"❌ 加密失敗: {ex.Message}");
    //     }
    // }

    // static string GetPasswordFromConnectionString(string connectionString)
    // {
    //     var parts = connectionString.Split(';');
    //     foreach (var part in parts)
    //     {
    //         if (part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
    //         {
    //             return part.Split('=')[1];
    //         }
    //     }
    //     return "";
    // }
}
