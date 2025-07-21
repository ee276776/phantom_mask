namespace PhantomMaskAPI.Models.DTOs
{
    public class PharmacyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public string OpeningHours { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MaskCount { get; set; }
    }
}
