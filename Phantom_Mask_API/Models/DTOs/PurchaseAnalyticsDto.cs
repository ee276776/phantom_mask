namespace PhantomMaskAPI.Models.DTOs
{
    public class PurchaseAnalyticsDto
    {
        public int TotalPurchases { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string MostPopularMask { get; set; } = string.Empty;
        public string TopPharmacy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
