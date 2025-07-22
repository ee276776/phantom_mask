namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkPurchaseDto
    {
        public string UserName { get; set; } 
        public List<BulkPurchaseItemDto> Purchases { get; set; } = new();
    }

    public class BulkPurchaseDto_
    {
        public int UserId { get; set; }
        public List<BulkPurchaseItemDto_> Purchases { get; set; } = new();
    }
}
