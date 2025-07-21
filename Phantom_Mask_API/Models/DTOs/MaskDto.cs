namespace PhantomMaskAPI.Models.DTOs
{
    public class MaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int PharmacyId { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
