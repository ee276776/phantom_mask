namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkPurchaseResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PurchaseDto> CompletedPurchases { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
