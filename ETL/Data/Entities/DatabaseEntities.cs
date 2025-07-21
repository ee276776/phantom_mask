using System;
using System.Collections.Generic;

namespace PhantomMaskETL.Data.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PharmacyEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public string OpeningHours { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<MaskEntity> Masks { get; set; } = new();
    }

    public class MaskEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int PharmacyId { get; set; }
        public PharmacyEntity Pharmacy { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PurchaseEntity
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string MaskName { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
        public int TransactionQuantity { get; set; }
        public DateTime TransactionDatetime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
