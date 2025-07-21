using System;
using PhantomMaskETL.Services;

namespace PhantomMaskETL.Tools
{
    /// <summary>
    /// 加密工具程式，用於產生加密的連線字串
    /// 使用方式: dotnet run --project EncryptionTool "你的連線字串" "你的加密金鑰"
    /// </summary>
    class EncryptionTool
    {
        // static void Main(string[] args)
        // {
        //     if (args.Length < 2)
        //     {
        //         Console.WriteLine("使用方式: EncryptionTool1234 <連線字串> <加密金鑰>");
        //         Console.WriteLine("範例: EncryptionTool \"Host=database;Database=phantom_mask;Username=phantom_user;Password=SecurePass123!;Port=5432\" \"MySecretKey123\"");
        //         return;
        //     }

        //     var connectionString = args[0];
        //     var encryptionKey = args[1];

        //     var encryptionService = new EncryptionService();

        //     try
        //     {
        //         var encryptedConnectionString = encryptionService.Encrypt(connectionString, encryptionKey);

        //         Console.WriteLine("🔐 原始連線字串:");
        //         Console.WriteLine(connectionString);
        //         Console.WriteLine();

        //         Console.WriteLine("🔒 加密後的連線字串:");
        //         Console.WriteLine(encryptedConnectionString);
        //         Console.WriteLine();

        //         Console.WriteLine("🔧 請將以下設定加入您的 appsettings.json 或環境變數:");
        //         Console.WriteLine($"\"ConnectionStrings:EncryptedDefaultConnection\": \"{encryptedConnectionString}\"");
        //         Console.WriteLine($"\"Security:EncryptionKey\": \"{encryptionKey}\"");
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
    }
}
