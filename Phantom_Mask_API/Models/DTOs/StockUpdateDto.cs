namespace PhantomMaskAPI.Models.DTOs
{
    public class StockUpdateDto
    {
        public string Operation { get; set; } = string.Empty; // "increase" or "decrease"
        public int Quantity { get; set; }
    }
}
