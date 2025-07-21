namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkPurchaseItemDto
    {
        public string PharmacyName { get; set; } = string.Empty;
        public string MaskName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
