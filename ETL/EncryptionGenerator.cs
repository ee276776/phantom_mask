using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class SimpleEncryption
{
    // static void Main()
    // {
    //     var connectionString = "Host=database;Database=phantom_mask;Username=phantom_user;Password=SecurePass123!;Port=5432";
    //     var encryptionKey = "PhantomMask2025SecretKey";
    //
    //     Console.WriteLine("🔐 PhantomMask 連線字串加密工具");
    //     Console.WriteLine("===================================");
    //
    //     try
    //     {
    //         var encrypted = Encrypt(connectionString, encryptionKey);
    //
    //         Console.WriteLine("🔐 原始連線字串:");
    //         Console.WriteLine("Host=database;Database=phantom_mask;Username=phantom_user;Password=****;Port=5432");
    //         Console.WriteLine();
    //
    //         Console.WriteLine("🔒 加密後的連線字串:");
    //         Console.WriteLine(encrypted);
    //         Console.WriteLine();
    //
    //         Console.WriteLine("請將以下環境變數加入到 docker-compose.yml:");
    //         Console.WriteLine($"- ConnectionStrings__EncryptedDefaultConnection={encrypted}");
    //         Console.WriteLine($"- Security__EncryptionKey={encryptionKey}");
    //         Console.WriteLine();
    //
    //         // 驗證
    //         var decrypted = Decrypt(encrypted, encryptionKey);
    //         Console.WriteLine("✅ 驗證: " + (decrypted == connectionString ? "加密解密成功!" : "加密解密失敗!"));
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"❌ 錯誤: {ex.Message}");
    //     }
    // }

    // static string Encrypt(string plainText, string key)
    // {
    //     using var aes = Aes.Create();
    //     var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
    //     aes.Key = keyBytes;
    //     aes.GenerateIV();

    //     using var encryptor = aes.CreateEncryptor();
    //     using var msEncrypt = new MemoryStream();
        
    //     msEncrypt.Write(aes.IV, 0, aes.IV.Length);
        
    //     using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
    //     using (var swEncrypt = new StreamWriter(csEncrypt))
    //     {
    //         swEncrypt.Write(plainText);
    //     }

    //     return Convert.ToBase64String(msEncrypt.ToArray());
    // }
    
    // static string Decrypt(string encryptedText, string key)
    // {
    //     var fullCipher = Convert.FromBase64String(encryptedText);

    //     using var aes = Aes.Create();
    //     var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
    //     aes.Key = keyBytes;

    //     var iv = new byte[aes.BlockSize / 8];
    //     var cipher = new byte[fullCipher.Length - iv.Length];

    //     Array.Copy(fullCipher, 0, iv, 0, iv.Length);
    //     Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

    //     aes.IV = iv;

    //     using var decryptor = aes.CreateDecryptor();
    //     using var msDecrypt = new MemoryStream(cipher);
    //     using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
    //     using var srDecrypt = new StreamReader(csDecrypt);

    //     return srDecrypt.ReadToEnd();
    // }
}
