namespace PhantomMaskAPI.Models.DTOs
{
    public class TopSpenderDto
    {
        public string UserName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int TotalPurchases { get; set; }
        public DateTime FirstPurchase { get; set; }
        public DateTime LastPurchase { get; set; }
    }
}
