// namespace PhantomMaskAPI.Models
// {
//     public class User
//     {
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public decimal CashBalance { get; set; }
//         public DateTime CreatedAt { get; set; }
//     }

//     public class Pharmacy
//     {
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public decimal CashBalance { get; set; }
//         public string OpeningHours { get; set; } = string.Empty;
//         public DateTime CreatedAt { get; set; }

//         // 導航屬性
//         public virtual ICollection<Mask> Masks { get; set; } = new List<Mask>();
//     }

//     public class Mask
//     {
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public decimal Price { get; set; }
//         public int StockQuantity { get; set; }
//         public int PharmacyId { get; set; }
//         public DateTime CreatedAt { get; set; }

//         // 導航屬性
//         public virtual Pharmacy Pharmacy { get; set; } = null!;
//     }

//     public class Purchase
//     {
//         public int Id { get; set; }
//         public string UserName { get; set; } = string.Empty;
//         public string PharmacyName { get; set; } = string.Empty;
//         public string MaskName { get; set; } = string.Empty;
//         public int TransactionQuantity { get; set; }
//         public decimal TransactionAmount { get; set; }
//         public DateTime TransactionDateTime { get; set; }
//         public DateTime CreatedAt { get; set; }
//     }
// }
