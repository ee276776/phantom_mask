namespace PhantomMaskAPI.Models.DTOs
{
    /// <summary>
    /// 藥局資料傳輸物件（DTO），包含基本資訊與口罩庫存相關數量。
    /// </summary>
    public class PharmacyDto
    {
        /// <summary>
        /// 藥局唯一識別編號。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 藥局名稱。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 藥局現金餘額。
        /// </summary>
        public decimal CashBalance { get; set; }

        /// <summary>
        /// 營業時間描述。
        /// </summary>
        public string OpeningHours { get; set; } = string.Empty;

        /// <summary>
        /// 藥局建立時間。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 藥局中口罩的種類數量。
        /// </summary>
        public int MaskTypeCount { get; set; }

        /// <summary>
        /// 藥局中口罩的總數量（所有種類加總）。
        /// </summary>
        public int MaskTotalCount { get; set; }
    }
}
