namespace PhantomMaskAPI.Models.DTOs
{
    public class MaskCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
