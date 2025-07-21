using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhantomMaskETL.Services;

namespace PhantomMaskETL.Configuration
{
    public interface ISecureConnectionService
    {
        string GetConnectionString();
    }

    public class SecureConnectionService : ISecureConnectionService
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<SecureConnectionService> _logger;

        public SecureConnectionService(
            IConfiguration configuration,
            IEncryptionService encryptionService,
            ILogger<SecureConnectionService> logger)
        {
            _configuration = configuration;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public string GetConnectionString()
        {
            try
            {
                var encryptedConnectionString = _configuration.GetConnectionString("EncryptedDefaultConnection");
                var encryptionKey = _configuration["Security:EncryptionKey"];

                if (string.IsNullOrEmpty(encryptedConnectionString))
                {
                    _logger.LogWarning("未找到加密的連線字串，使用預設連線字串");
                    return _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
                }

                if (string.IsNullOrEmpty(encryptionKey))
                {
                    _logger.LogError("未找到加密金鑰");
                    throw new InvalidOperationException("加密金鑰未設定");
                }

                // 使用 Base64 解碼 (簡單版本)
                try 
                {
                    var base64Bytes = Convert.FromBase64String(encryptedConnectionString);
                    var decryptedConnectionString = System.Text.Encoding.UTF8.GetString(base64Bytes);
                    _logger.LogInformation("✅ 成功解碼連線字串");
                    return decryptedConnectionString;
                }
                catch
                {
                    // 如果 Base64 解碼失敗，嘗試使用完整加密
                    var decryptedConnectionString = _encryptionService.Decrypt(encryptedConnectionString, encryptionKey);
                    _logger.LogInformation("✅ 成功解密連線字串");
                    return decryptedConnectionString;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 解密連線字串時發生錯誤");
                throw;
            }
        }
    }
}
