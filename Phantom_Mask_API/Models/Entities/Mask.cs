namespace PhantomMaskAPI.Models.Entities
{
    public class Mask
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int PharmacyId { get; set; }
        public DateTime CreatedAt { get; set; }

        // 導航屬性
        public virtual Pharmacy Pharmacy { get; set; } = null!;
    }
}
