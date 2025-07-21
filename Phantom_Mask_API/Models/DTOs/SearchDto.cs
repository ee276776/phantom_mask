namespace PhantomMaskAPI.Models.DTOs
{
    public class SearchDto
    {
        public string Query { get; set; } = string.Empty;
        public string Type { get; set; } = "all"; // "mask", "pharmacy", "all"
        public int Limit { get; set; } = 10;
    }
}
