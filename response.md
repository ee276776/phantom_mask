
# PhantomMask API 文件

**版本：** v1  
**描述：** 口罩購買系統 API - 提供藥局、口罩和購買相關功能

---

## 目錄
- [完成度](#完成度)
- [核心功能 API](#核心功能-api)
- [資料模型 (Schemas)](#資料模型-schemas)


---
## 完成度
* [x] List pharmacies, optionally filtered by specific time and/or day of the week.  
  * Implemented at **GET /api/Pharmacies**
* [x] List all masks sold by a given pharmacy with an option to sort by name or price.  
  * Implemented at **GET /api/Pharmacies/{id}/masks**
* [x] List all pharmacies that offer a number of mask products within a given price range, where the count is above, below, or between given thresholds.  
  * Implemented at **GET /api/Pharmacies/by-stock**
* [x] Show the top N users who spent the most on masks during a specific date range.  
  * Implemented at **GET /api/Analytics/top-spenders**
* [x] Process a purchase where a user buys masks from multiple pharmacies at once.  
  * Implemented at **POST /api/Purchases/bulk**
* [x] Update the stock quantity of an existing mask product by increasing or decreasing it.  
  * Implemented at **PUT /api/Masks/{id}/stock**
* [x] Create or update multiple mask products for a pharmacy at once, including name, price, and stock quantity.  
  * Implemented at **POST /api/Masks/upsert**
* [x] Search for pharmacies or masks by name and rank the results by relevance to the search term.  
  * Implemented at **GET /api/Search/SearchByRelavance**

## 核心功能 API

### Q1 - 藥局列表查詢

**[Q1] ※※ 列出藥局，可選擇依特定時間和/或星期幾進行篩選 ※※**

```http
GET /api/Pharmacies
```

**查詢參數：**
- `searchName` (string, 可選): 藥局名稱篩選
- `startTime` (string, 可選): 營業開始時間篩選 (格式: "08:00", 24小時制) 
- `endTime` (string, 可選): 營業結束時間篩選 (格式: "18:00", 24小時制)
- `dayOfWeek` (integer, 可選): 星期篩選 (1=Mon, 2=Tue, 3=Wed, 4=Thur, 5=Fri, 6=Sat, 7=Sun)

**回應格式：**
```json
[
  {
    "id": 0,
    "name": "string",
    "cashBalance": 0.0,
    "openingHours": "string", 
    "createdAt": "2024-01-01T00:00:00Z",
    "maskTypeCount": 0,
    "maskTotalCount": 0
  }
]
```

**使用範例：**
```bash
# 查詢週一營業且名稱包含"康"的藥局
curl -X GET "https://api.phantommask.com/api/Pharmacies?searchName=康&dayOfWeek=1"

# 查詢早上8點到晚上6點營業的藥局
curl -X GET "https://api.phantommask.com/api/Pharmacies?startTime=08:00&endTime=18:00"
```

---

### Q2 - 特定藥局口罩列表

**[Q2] ※※ 列出特定藥局銷售的所有口罩，並可依名稱或價格排序 ※※**

```http
GET /api/Pharmacies/{id}/masks
```

**路徑參數：**
- `id` (integer, 必填): 藥局ID

**查詢參數：**
- `sortBy` (string, 預設: "name"): 排序欄位 ("name" 或 "price")
- `sortOrder` (string, 預設: "asc"): 排序方向 ("asc" 或 "desc")

**回應格式：**
```json
[
  {
    "id": 0,
    "name": "string",
    "price": 0.0,
    "stockQuantity": 0,
    "pharmacyId": 0,
    "pharmacyName": "string",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

**使用範例：**
```bash
# 取得藥局ID為1的所有口罩，依價格降序排列
curl -X GET "https://api.phantommask.com/api/Pharmacies/1/masks?sortBy=price&sortOrder=desc"
```

---

### Q3 - 依庫存條件查詢藥局

**[Q3] ※※ 列出所有在給定價格範圍內提供一定數量口罩產品的藥店 ※※**

```http
GET /api/Pharmacies/by-stock
```

**查詢參數：**
- `minPrice` (number, 可選): 最低價格
- `maxPrice` (number, 可選): 最高價格
- `minStockThreshold` (integer, 可選): 最小庫存閾值
- `maxStockThreshold` (integer, 可選): 最大庫存閾值
- `isInclusive` (boolean, 預設: false): 是否包含等於

**回應格式：** 藥局陣列 (同Q1)

**使用範例：**
```bash
# 查詢有價格在10-50元且庫存大於100個口罩的藥局
curl -X GET "https://api.phantommask.com/api/Pharmacies/by-stock?minPrice=10&maxPrice=50&minStockThreshold=100"
```

---

### Q4 - 消費排行榜

**[Q4] ※※ 顯示特定日期範圍內購買口罩花費最多的前 N 名用戶 ※※**

```http
GET /api/Analytics/top-spenders
```

**查詢參數：**
- `startDate` (string, datetime, 可選): 查詢起始日期（包含此日期）
- `endDate` (string, datetime, 可選): 查詢結束日期（包含此日期）
- `topN` (integer, 預設: 10): 要取得的前 N 名用戶數量

**回應格式：**
```json
[
  {
    "userName": "string",
    "totalSpent": 0.0,
    "totalPurchases": 0,
    "firstPurchase": "2024-01-01T00:00:00Z",
    "lastPurchase": "2024-01-01T00:00:00Z"
  }
]
```

**使用範例：**
```bash
# 查詢2024年1月消費前5名用戶
curl -X GET "https://api.phantommask.com/api/Analytics/top-spenders?startDate=2024-01-01T00:00:00Z&endDate=2024-01-31T23:59:59Z&topN=5"
```

---

### Q5 - 批量購買

**[Q5] ※※ 處理用戶一次向多家藥局購買口罩的購買行為 ※※**

```http
POST /api/Purchases/bulk
```

**請求內容：**
```json
{
  "userId": 0,
  "purchases": [
    {
      "pharmacyId": 0,
      "maskId": 0,
      "quantity": 0
    }
  ]
}
```

**回應格式：**
```json
{
  "success": true,
  "message": "string",
  "completedPurchases": [
    {
      "id": 0,
      "userName": "string",
      "pharmacyName": "string",
      "maskName": "string",
      "transactionQuantity": 0,
      "transactionAmount": 0.0,
      "transactionDateTime": "2024-01-01T00:00:00Z",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "errors": ["string"],
  "totalAmount": 0.0
}
```

**使用範例：**
```bash
# 用戶ID為1同時向兩家藥局購買口罩
curl -X POST "https://api.phantommask.com/api/Purchases/bulk" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "purchases": [
      {
        "pharmacyId": 1,
        "maskId": 1,
        "quantity": 5
      },
      {
        "pharmacyId": 2,
        "maskId": 3,
        "quantity": 3
      }
    ]
  }'
```

---

### Q6 - 更新口罩庫存

**[Q6] ※※ 更新口罩庫存 - 透過增加或減少來更新現有口罩產品的庫存數量 ※※**

```http
PUT /api/Masks/{id}/stock
```

**路徑參數：**
- `id` (integer, 必填): 要更新庫存的口罩產品 ID

**請求內容：**
```json
{
  "operation": "string",
  "quantity": 0
}
```

**回應格式：** 單一口罩物件 (同Q2回應中的單個項目)

**使用範例：**
```bash
# 增加口罩ID為1的庫存100個
curl -X PUT "https://api.phantommask.com/api/Masks/1/stock" \
  -H "Content-Type: application/json" \
  -d '{"operation":"increase","quantity":100}'

# 減少口罩ID為2的庫存50個
curl -X PUT "https://api.phantommask.com/api/Masks/2/stock" \
  -H "Content-Type: application/json" \
  -d '{"operation":"decrease","quantity":50}'
```

---

### Q7 - 批量新增或更新口罩

**[Q7] ※※ 新增或更新多筆口罩資訊 (不含異動藥局現金餘額CashBalance) ※※**

```http
POST /api/Masks/upsert
```

**查詢參數：**
- `pharmacyId` (integer, 可選): 藥局 ID

**請求內容：**
```json
[
  {
    "name": "string",
    "price": 0.0,
    "stockQuantity": 0
  }
]
```

**回應格式：** 口罩陣列 (同Q2)

**使用範例：**
```bash
# 為藥局ID為1批量新增/更新口罩資訊
curl -X POST "https://api.phantommask.com/api/Masks/upsert?pharmacyId=1" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "name": "N95口罩",
      "price": 15.0,
      "stockQuantity": 100
    },
    {
      "name": "醫療口罩",
      "price": 5.0,
      "stockQuantity": 200
    }
  ]'
```

---

### Q8 - 相關性搜尋

**[Q8] ※※ 執行相關性搜尋 ※※**

```http
GET /api/Search/SearchByRelavance
```

**查詢參數：**
- `query` (string, 必填): 搜尋關鍵字

**回應格式：**
```json
[
  {
    "id": 0,
    "name": "string",
    "type": "string",
    "relevanceScore": 0.0
  }
]
```

**使用範例：**
```bash
# 搜尋關鍵字"N95"的相關結果
curl -X GET "https://api.phantommask.com/api/Search/SearchByRelavance?query=N95"
```

---

## 資料模型 (Schemas)

### BulkPurchaseDto
**批次購買資料傳輸物件，包含使用者資訊及多筆購買項目**
```json
{
  "userId": 0,
  "purchases": [
    {
      "pharmacyId": 0,
      "maskId": 0,
      "quantity": 0
    }
  ]
}
```

### BulkPurchaseItemDto
**批次購買項目資料傳輸物件，代表單一藥局的單一口罩購買明細**
```json
{
  "pharmacyId": 0,
  "maskId": 0,
  "quantity": 0
}
```

### BulkPurchaseResultDto
**批量購買結果**
```json
{
  "success": true,
  "message": "string",
  "completedPurchases": [...],
  "errors": ["string"],
  "totalAmount": 0.0
}
```

### MaskDto
**口罩資料傳輸物件，包含口罩基本資訊及所屬藥局相關資訊**
```json
{
  "id": 0,
  "name": "string",
  "price": 0.0,
  "stockQuantity": 0,
  "pharmacyId": 0,
  "pharmacyName": "string",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### MaskUpsertDto
**口罩新增/更新資料**
```json
{
  "name": "string",
  "price": 0.0,
  "stockQuantity": 0
}
```

### PharmacyDto
**藥局資料傳輸物件，包含基本資訊與口罩庫存相關數量**
```json
{
  "id": 0,
  "name": "string",
  "cashBalance": 0.0,
  "openingHours": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "maskTypeCount": 0,
  "maskTotalCount": 0
}
```

### PurchaseDto
**購買紀錄資料傳輸物件，包含用戶購買口罩的詳細資訊**
```json
{
  "id": 0,
  "userName": "string",
  "pharmacyName": "string",
  "maskName": "string",
  "transactionQuantity": 0,
  "transactionAmount": 0.0,
  "transactionDateTime": "2024-01-01T00:00:00Z",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### RelevanceResultDto
**相關搜尋結果資料傳輸物件，包含基本資訊與相關度分數**
```json
{
  "id": 0,
  "name": "string",
  "type": "string",
  "relevanceScore": 0.0
}
```

### StockUpdateDto
**用於更新庫存的資料傳輸物件**
```json
{
  "operation": "string",
  "quantity": 0
}
```

### TopSpenderDto
**消費排行榜資料**
```json
{
  "userName": "string",
  "totalSpent": 0.0,
  "totalPurchases": 0,
  "firstPurchase": "2024-01-01T00:00:00Z",
  "lastPurchase": "2024-01-01T00:00:00Z"
}
```

---

## 狀態碼說明

- **200 OK**: 請求成功
- **400 Bad Request**: 請求參數錯誤
- **404 Not Found**: 資源不存在
- **500 Internal Server Error**: 伺服器內部錯誤

---

## 重要說明

1. **日期時間格式**: 所有日期時間參數使用 ISO 8601 格式 (例: `2024-01-01T00:00:00Z`)
2. **庫存操作**: StockUpdateDto 中的 `operation` 參數可為 `"increase"`（增加）或 `"decrease"`（減少）
3. **搜尋類型**: RelevanceResultDto 中的 `type` 參數值為 `"mask"` 或 `"pharmacy"`
4. **營業時間**: 時間格式使用 24 小時制，例如 `"08:00"` 或 `"18:00"`
5. **星期編號**: 1=週一, 2=週二, 3=週三, 4=週四, 5=週五, 6=週六, 7=週日