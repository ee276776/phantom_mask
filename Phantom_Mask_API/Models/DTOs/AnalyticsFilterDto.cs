namespace PhantomMaskAPI.Models.DTOs
{
    public class AnalyticsFilterDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TopN { get; set; } = 10;
    }
}
