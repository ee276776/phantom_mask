namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkPurchaseDto
    {
        public string UserName { get; set; } = string.Empty;
        public List<BulkPurchaseItemDto> Purchases { get; set; } = new();
    }
}
