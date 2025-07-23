namespace PhantomMaskAPI.Models.DTOs
{
    public class MaskUpsertDto
    {
        /// <summary>
        /// 欲新增/更新的口罩名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 欲新增/更新的口罩價格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 欲新增/更新的口罩庫存數量
        /// </summary>
        public int StockQuantity { get; set; }
    }
}
