namespace PhantomMaskAPI.Models.DTOs
{
    /// <summary>
    /// 口罩庫存更新 DTO
    /// </summary>
    public class MaskStockUpdateDto
    {
        /// <summary>
        /// 操作類型：increase (增加) 或 decrease (減少)
        /// </summary>
        public string Operation { get; set; } = "increase";
        
        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }
    }
}
