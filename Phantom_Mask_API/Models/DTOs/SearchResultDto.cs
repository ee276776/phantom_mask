namespace PhantomMaskAPI.Models.DTOs
{
    public class SearchResultDto
    {
        public List<PharmacyDto> Pharmacies { get; set; } = new();
        public List<MaskDto> Masks { get; set; } = new();
        public int TotalResults { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
