namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkPurchaseItemDto
    {
        public string PharmacyName { get; set; } = string.Empty;
        public string MaskName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// 批次購買項目資料傳輸物件（DTO），代表單一藥局的單一口罩購買明細。
    /// </summary>
    public class BulkPurchaseItemDto_
    {
        /// <summary>
        /// 藥局唯一識別編號。
        /// </summary>
        public int PharmacyId { get; set; }

        /// <summary>
        /// 口罩唯一識別編號。
        /// </summary>
        public int MaskId { get; set; }

        /// <summary>
        /// 購買數量。
        /// </summary>
        public int Quantity { get; set; }
    }
}
