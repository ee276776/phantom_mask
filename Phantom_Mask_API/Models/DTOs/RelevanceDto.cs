namespace PhantomMaskAPI.Models.DTOs
{
    public class RelevanceDto
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 搜尋型態 mask, pharmacy
        /// </summary>
        public string Type { get; set; } 
    }
    public class RelevanceResultDto
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 搜尋型態 mask, pharmacy
        /// </summary>
        public string Type { get; set; }
        public double RelevanceScore { get; set; }
    }
}
