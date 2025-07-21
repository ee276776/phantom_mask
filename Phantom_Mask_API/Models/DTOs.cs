// namespace PhantomMaskAPI.Models.DTOs
// {
//     // 藥局相關 DTOs
//     public class PharmacyDto
//     {
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public decimal CashBalance { get; set; }
//         public string OpeningHours { get; set; } = string.Empty;
//         public DateTime CreatedAt { get; set; }
//         public int MaskCount { get; set; }
//     }

    

//     // 口罩相關 DTOs
//     public class MaskDto
//     {
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public decimal Price { get; set; }
//         public int StockQuantity { get; set; }
//         public int PharmacyId { get; set; }
//         public string PharmacyName { get; set; } = string.Empty;
//         public DateTime CreatedAt { get; set; }
//     }

//     public class MaskCreateDto
//     {
//         public string Name { get; set; } = string.Empty;
//         public decimal Price { get; set; }
//         public int StockQuantity { get; set; }
//     }

//     public class MaskUpdateDto
//     {
//         public string? Name { get; set; }
//         public decimal? Price { get; set; }
//         public int? StockQuantity { get; set; }
//     }

//     public class BulkMaskDto
//     {
//         public List<MaskCreateDto> Masks { get; set; } = new();
//     }

//     // 查詢相關 DTOs
//     public class StockFilterDto
//     {
//         public decimal? MinPrice { get; set; }
//         public decimal? MaxPrice { get; set; }
//         public int StockThreshold { get; set; }
//         public string StockComparison { get; set; } = string.Empty; // "above", "below", "between"
//         public int? MinStock { get; set; }
//         public int? MaxStock { get; set; }
//     }

//     public class SearchDto
//     {
//         public string Query { get; set; } = string.Empty;
//         public string Type { get; set; } = "all"; // "mask", "pharmacy", "all"
//         public int Limit { get; set; } = 10;
//     }

//     public class SearchResultDto
//     {
//         public string Type { get; set; } = string.Empty; // "mask" or "pharmacy"
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public decimal? Price { get; set; }
//         public string? PharmacyName { get; set; }
//         public double RelevanceScore { get; set; }
//     }

//     // 購買相關 DTOs
//     public class PurchaseDto
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

//     public class BulkPurchaseDto
//     {
//         public string UserName { get; set; } = string.Empty;
//         public List<PurchaseItemDto> Purchases { get; set; } = new();
//     }

//     public class PurchaseItemDto
//     {
//         public int PharmacyId { get; set; }
//         public int MaskId { get; set; }
//         public int Quantity { get; set; }
//     }

//     // 分析相關 DTOs
//     public class TopSpenderDto
//     {
//         public string UserName { get; set; } = string.Empty;
//         public decimal TotalSpent { get; set; }
//         public int TotalPurchases { get; set; }
//         public DateTime FirstPurchase { get; set; }
//         public DateTime LastPurchase { get; set; }
//     }

//     public class AnalyticsFilterDto
//     {
//         public DateTime StartDate { get; set; }
//         public DateTime EndDate { get; set; }
//         public int TopN { get; set; } = 10;
//     }

//     // 通用回應 DTOs
//     public class ApiResponse<T>
//     {
//         public bool Success { get; set; } = true;
//         public string Message { get; set; } = string.Empty;
//         public T? Data { get; set; }
//         public List<string> Errors { get; set; } = new();
//     }

//     public class PaginatedResponse<T>
//     {
//         public List<T> Data { get; set; } = new();
//         public int TotalCount { get; set; }
//         public int Page { get; set; }
//         public int PageSize { get; set; }
//         public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
//     }
// }
