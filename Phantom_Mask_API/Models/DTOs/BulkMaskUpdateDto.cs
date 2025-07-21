namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkMaskUpdateDto
    {
        public int MaskId { get; set; }
        public int NewStock { get; set; }
        public decimal? NewPrice { get; set; }
    }
}
