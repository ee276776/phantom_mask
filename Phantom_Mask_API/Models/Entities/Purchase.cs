namespace PhantomMaskAPI.Models.Entities
{
    public class Purchase
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string MaskName { get; set; } = string.Empty;
        public int TransactionQuantity { get; set; }
        public decimal TransactionAmount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
