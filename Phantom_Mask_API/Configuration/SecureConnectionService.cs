using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Configuration
{
    public class SecureConnectionService
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
                // 嘗試取得加密的連接字串
                var encryptedConnectionString = _configuration.GetConnectionString("EncryptedDefaultConnection");
                var encryptionKey = _configuration["Security:EncryptionKey"];

                if (!string.IsNullOrEmpty(encryptedConnectionString) && !string.IsNullOrEmpty(encryptionKey))
                {
                    _logger.LogInformation("🔓 使用加密連接字串");
                    return _encryptionService.Decrypt(encryptedConnectionString, encryptionKey);
                }

                // 回退到普通連接字串
                var regularConnectionString = _configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(regularConnectionString))
                {
                    _logger.LogWarning("⚠️ 使用明文連接字串");
                    return regularConnectionString;
                }

                throw new InvalidOperationException("找不到資料庫連接字串");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 取得連接字串時發生錯誤");
                throw;
            }
        }
    }
}
