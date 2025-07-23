namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkPurchaseDto
    {
        public string UserName { get; set; } 
        public List<BulkPurchaseItemDto> Purchases { get; set; } = new();
    }

    /// <summary>
    /// 批次購買資料傳輸物件（DTO），包含使用者資訊及多筆購買項目。
    /// </summary>
    public class BulkPurchaseDto_
    {
        /// <summary>
        /// 購買使用者的唯一識別編號。
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 多筆購買項目清單。
        /// </summary>
        public List<BulkPurchaseItemDto_> Purchases { get; set; } = new();
    }
}
