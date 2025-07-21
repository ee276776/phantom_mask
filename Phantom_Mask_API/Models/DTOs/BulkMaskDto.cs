namespace PhantomMaskAPI.Models.DTOs
{
    public class BulkMaskDto
    {
        public List<MaskCreateDto> Masks { get; set; } = new();
    }
}
