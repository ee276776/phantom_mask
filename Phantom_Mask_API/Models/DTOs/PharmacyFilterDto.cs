namespace PhantomMaskAPI.Models.DTOs
{
    public class PharmacyFilterDto
    {
        public string? SearchName { get; set; }
        public string? StartTime { get; set; } // 格式: "08:00" (24小時制)
        public string? EndTime { get; set; }   // 格式: "18:00" (24小時制)
        public int? DayOfWeek { get; set; }    // 1=Mon, 2=Tue, 3=Wed, 4=Thur, 5=Fri, 6=Sat, 7=Sun
    }
}
