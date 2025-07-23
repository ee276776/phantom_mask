namespace PhantomMaskAPI.Models.DTOs
{
    /// <summary>
    /// 口罩資料傳輸物件（DTO），包含口罩基本資訊及所屬藥局相關資訊。
    /// </summary>
    public class MaskDto
    {
        /// <summary>
        /// 口罩唯一識別編號。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 口罩名稱。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 口罩價格。
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 庫存數量。
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// 所屬藥局的唯一識別編號。
        /// </summary>
        public int PharmacyId { get; set; }

        /// <summary>
        /// 所屬藥局名稱。
        /// </summary>
        public string PharmacyName { get; set; } = string.Empty;

        /// <summary>
        /// 口罩資料建立時間。
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
