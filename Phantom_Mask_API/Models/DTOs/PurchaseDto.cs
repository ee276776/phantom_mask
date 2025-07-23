namespace PhantomMaskAPI.Models.DTOs
{
    /// <summary>
    /// 購買紀錄資料傳輸物件（DTO），包含用戶購買口罩的詳細資訊。
    /// </summary>
    public class PurchaseDto
    {
        /// <summary>
        /// 購買紀錄唯一識別編號。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 購買用戶名稱。
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 購買口罩的藥局名稱。
        /// </summary>
        public string PharmacyName { get; set; } = string.Empty;

        /// <summary>
        /// 口罩名稱。
        /// </summary>
        public string MaskName { get; set; } = string.Empty;

        /// <summary>
        /// 交易數量。
        /// </summary>
        public int TransactionQuantity { get; set; }

        /// <summary>
        /// 交易金額。
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// 交易發生時間。
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// 紀錄建立時間。
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
