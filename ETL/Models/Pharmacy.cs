using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PhantomMaskETL.Models
{
    public class Pharmacy
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cashBalance")]
        public decimal CashBalance { get; set; }

        [JsonPropertyName("openingHours")]
        public string OpeningHours { get; set; } = string.Empty;

        [JsonPropertyName("masks")]
        public List<Mask> Masks { get; set; } = new();
    }

    public class Mask
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }
    }
}
