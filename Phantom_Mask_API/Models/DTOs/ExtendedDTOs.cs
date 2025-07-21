// namespace PhantomMaskAPI.Models.DTOs
// {
//     /// <summary>
//     /// 口罩庫存更新 DTO
//     /// </summary>
//     public class MaskStockUpdateDto
//     {
//         /// <summary>
//         /// 操作類型：increase (增加) 或 decrease (減少)
//         /// </summary>
//         public string Operation { get; set; } = "increase";
        
//         /// <summary>
//         /// 數量
//         /// </summary>
//         public int Quantity { get; set; }
//     }

//     /// <summary>
//     /// 購買分析 DTO
//     /// </summary>
//     public class PurchaseAnalyticsDto
//     {
//         /// <summary>
//         /// 用戶名稱
//         /// </summary>
//         public string UserName { get; set; } = string.Empty;
        
//         /// <summary>
//         /// 總消費金額
//         /// </summary>
//         public decimal TotalAmount { get; set; }
        
//         /// <summary>
//         /// 購買次數
//         /// </summary>
//         public int PurchaseCount { get; set; }
        
//         /// <summary>
//         /// 平均單次消費
//         /// </summary>
//         public decimal AverageAmount { get; set; }
        
//         /// <summary>
//         /// 購買的口罩總數量
//         /// </summary>
//         public int TotalMaskCount { get; set; }
//     }

//     /// <summary>
//     /// 消費者排行 DTO
//     /// </summary>
//     public class TopSpenderDto
//     {
//         /// <summary>
//         /// 用戶名稱
//         /// </summary>
//         public string UserName { get; set; } = string.Empty;
        
//         /// <summary>
//         /// 總消費金額
//         /// </summary>
//         public decimal TotalSpent { get; set; }
        
//         /// <summary>
//         /// 購買次數
//         /// </summary>
//         public int PurchaseCount { get; set; }
        
//         /// <summary>
//         /// 排名
//         /// </summary>
//         public int Rank { get; set; }
//     }

//     /// <summary>
//     /// 搜尋結果 DTO
//     /// </summary>
//     public class SearchResultDto
//     {
//         /// <summary>
//         /// 搜尋關鍵字
//         /// </summary>
//         public string Query { get; set; } = string.Empty;
        
//         /// <summary>
//         /// 搜尋類型
//         /// </summary>
//         public string SearchType { get; set; } = string.Empty;
        
//         /// <summary>
//         /// 總結果數
//         /// </summary>
//         public int TotalResults { get; set; }
        
//         /// <summary>
//         /// 藥局結果
//         /// </summary>
//         public List<PharmacyDto> Pharmacies { get; set; } = new();
        
//         /// <summary>
//         /// 口罩結果
//         /// </summary>
//         public List<MaskDto> Masks { get; set; } = new();
//     }
// }
