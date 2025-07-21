using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhantomMaskETL.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText, string key);
        string Decrypt(string encryptedText, string key);
    }

    public class EncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            using var aes = Aes.Create();
            
            // 使用 SHA256 來產生固定長度的金鑰
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.Key = keyBytes;
            
            // 產生隨機 IV
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            // 先寫入 IV
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string Decrypt(string encryptedText, string key)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullCipher = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            
            // 使用 SHA256 來產生固定長度的金鑰
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.Key = keyBytes;

            // 提取 IV（前 16 bytes）
            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
    }
}
