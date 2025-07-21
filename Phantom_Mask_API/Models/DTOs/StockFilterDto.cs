namespace PhantomMaskAPI.Models.DTOs
{
    public class StockFilterDto
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int StockThreshold { get; set; }
        public string StockComparison { get; set; } = string.Empty; // "above", "below", "between"
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
    }
}
