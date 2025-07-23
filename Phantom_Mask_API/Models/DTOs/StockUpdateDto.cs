namespace PhantomMaskAPI.Models.DTOs
{
    /// <summary>
    /// 用於更新庫存的資料傳輸物件（DTO）。
    /// </summary>
    public class StockUpdateDto
    {
        /// <summary>
        /// 庫存操作類型，可為 "increase"（增加）或 "decrease"（減少）。
        /// </summary>
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// 庫存變動數量。
        /// </summary>
        public int Quantity { get; set; }
    }
}
