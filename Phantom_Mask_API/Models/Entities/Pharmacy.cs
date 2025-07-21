namespace PhantomMaskAPI.Models.Entities
{
    public class Pharmacy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public string OpeningHours { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // 導航屬性
        public virtual ICollection<Mask> Masks { get; set; } = new List<Mask>();
    }
}
