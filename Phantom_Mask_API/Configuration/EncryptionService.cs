using System.Security.Cryptography;
using System.Text;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Configuration
{
    public class EncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText, string key)
        {
            using var aes = Aes.Create();
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
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
            try
            {
                var fullCipher = Convert.FromBase64String(encryptedText);

                using var aes = Aes.Create();
                var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
                aes.Key = keyBytes;

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
            catch
            {
                // 如果解密失敗，嘗試 Base64 解碼（向後兼容）
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));
                }
                catch
                {
                    throw new InvalidOperationException("無法解密連接字串");
                }
            }
        }
    }
}
