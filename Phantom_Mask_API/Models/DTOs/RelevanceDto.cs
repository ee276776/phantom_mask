namespace PhantomMaskAPI.Models.DTOs
{
    /// <summary>
    /// 相關搜尋項目資料傳輸物件（DTO），包含 ID、名稱與類型。
    /// </summary>
    public class RelevanceDto
    {
        /// <summary>
        /// 項目唯一識別編號。
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 項目名稱。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 搜尋類型，可能值為 "mask" 或 "pharmacy"。
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// 相關搜尋結果資料傳輸物件（DTO），包含基本資訊與相關度分數。
    /// </summary>
    public class RelevanceResultDto
    {
        /// <summary>
        /// 項目唯一識別編號。
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 項目名稱。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 搜尋類型，可能值為 "mask" 或 "pharmacy"。
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 項目相關度分數，數值越高代表相關度越高。
        /// </summary>
        public double RelevanceScore { get; set; }
    }
}
