using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PhantomMaskETL.Converters;

namespace PhantomMaskETL.Models
{
    public class User
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("cashBalance")]
        public decimal CashBalance { get; set; }

        [JsonPropertyName("purchaseHistories")]
        public List<PurchaseHistory> PurchaseHistories { get; set; } = new();
    }

    public class PurchaseHistory
    {
        [JsonPropertyName("pharmacyName")]
        public string? PharmacyName { get; set; }

        [JsonPropertyName("maskName")]
        public string? MaskName { get; set; }

        [JsonPropertyName("transactionAmount")]
        public decimal? TransactionAmount { get; set; }

        [JsonPropertyName("transactionQuantity")]
        public int? TransactionQuantity { get; set; }

        [JsonPropertyName("transactionDatetime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime TransactionDatetime { get; set; }
    }
}
