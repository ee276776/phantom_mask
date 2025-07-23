# PhantomMask API 文件

**版本：** v1  
**描述：** 口罩購買系統 API - 提供藥局、口罩和購買相關功能

---

## 目錄

- [Analytics 分析功能](#analytics-分析功能)
- [Masks 口罩管理](#masks-口罩管理)
- [Pharmacies 藥局管理](#pharmacies-藥局管理)
- [Purchases 購買管理](#purchases-購買管理)
- [Search 搜尋功能](#search-搜尋功能)
- [資料模型 (Schemas)](#資料模型-schemas)

---

## Analytics 分析功能

### 1. 取得消費最多的用戶排行榜

**[Q4] ※※ 顯示特定日期範圍內購買口罩花費最多的前 N 名用戶 ※※**

```http
GET /api/Analytics/top-spenders
```

**查詢參數：**
- `startDate` (string, datetime, 可選): 開始日期
- `endDate` (string, datetime, 可選): 結束日期
- `topN` (integer, 預設: 10): 取得前 N 名用戶

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

### 2. 取得購買趨勢分析

```http
GET /api/Analytics/purchase-trends
```

**查詢參數：**
- `startDate` (string, datetime, 可選): 開始日期
- `endDate` (string, datetime, 可選): 結束日期

**回應格式：**
```json
{
  "totalPurchases": 0,
  "totalRevenue": 0.0,
  "averageOrderValue": 0.0,
  "mostPopularMask": "string",
  "topPharmacy": "string",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-01T00:00:00Z"
}
```

---

## Masks 口罩管理

### 1. 搜尋口罩

```http
GET /api/Masks/search
```

**查詢參數：**
- `query` (string, 可選): 搜尋關鍵字
- `limit` (integer, 預設: 50): 限制回傳數量

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

### 2. 取得特定口罩資訊

```http
GET /api/Masks/{id}
```

**路徑參數：**
- `id` (integer, 必填): 口罩 ID

**回應格式：** 單一口罩物件 (同上)

### 3. 更新口罩庫存

**[Q6] ※※ 更新口罩庫存 - 透過增加或減少來更新現有口罩產品的庫存數量 ※※**

```http
PUT /api/Masks/{id}/stock
```

**路徑參數：**
- `id` (integer, 必填): 口罩 ID

**請求內容：**
```json
{
  "operation": "string",
  "quantity": 0
}
```

### 4. 根據價格範圍取得口罩

```http
GET /api/Masks/by-price-range
```

**查詢參數：**
- `minPrice` (number, 可選): 最低價格
- `maxPrice` (number, 可選): 最高價格

### 5. 取得低庫存口罩

```http
GET /api/Masks/low-stock
```

**查詢參數：**
- `threshold` (integer, 預設: 10): 庫存閾值

### 6. 批量新增或更新口罩

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

---

## Pharmacies 藥局管理

### 1. 取得藥局列表

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
    "maskCount": 0
  }
]
```

### 2. 取得特定藥局銷售的口罩

**[Q2] ※※ 列出特定藥局銷售的所有口罩，並可依名稱或價格排序 ※※**

```http
GET /api/Pharmacies/{id}/masks
```

**路徑參數：**
- `id` (integer, 必填): 藥局 ID

**查詢參數：**
- `sortBy` (string, 預設: "name"): 排序欄位 ("name" 或 "price")
- `sortOrder` (string, 預設: "asc"): 排序方向 ("asc" 或 "desc")

### 3. 根據庫存條件取得藥局

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

### 4. 取得特定藥局資訊

```http
GET /api/Pharmacies/{id}
```

**路徑參數：**
- `id` (integer, 必填): 藥局 ID

### 5. 批量更新藥局口罩

```http
POST /api/Pharmacies/{id}/masks/bulk
```

**路徑參數：**
- `id` (integer, 必填): 藥局 ID

**請求內容：**
```json
[
  {
    "maskId": 0,
    "newStock": 0,
    "newPrice": 0.0
  }
]
```

---

## Purchases 購買管理

### 1. 批量購買

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

### 2. 取得用戶購買記錄

```http
GET /api/Purchases/by-user/{userName}
```

**路徑參數：**
- `userName` (string, 必填): 用戶名稱

### 3. 取得特定日期範圍內的購買記錄

```http
GET /api/Purchases/by-date-range
```

**查詢參數：**
- `startDate` (string, datetime, 可選): 開始日期
- `endDate` (string, datetime, 可選): 結束日期

### 4. 取得購買分析資料

```http
GET /api/Purchases/analytics
```

**查詢參數：**
- `startDate` (string, datetime, 可選): 開始日期
- `endDate` (string, datetime, 可選): 結束日期

---

## Search 搜尋功能

### 1. 綜合搜尋

```http
GET /api/Search
```

**查詢參數：**
- `query` (string, 可選): 搜尋關鍵字
- `type` (string, 預設: "all"): 搜尋類型
- `limit` (integer, 預設: 50): 限制回傳數量

**回應格式：**
```json
{
  "pharmacies": [...],
  "masks": [...],
  "totalResults": 0,
  "searchTerm": "string"
}
```

### 2. 僅搜尋藥局

```http
GET /api/Search/pharmacies
```

**查詢參數：**
- `query` (string, 可選): 搜尋關鍵字
- `limit` (integer, 預設: 50): 限制回傳數量

### 3. 僅搜尋口罩

```http
GET /api/Search/masks
```

**查詢參數：**
- `query` (string, 可選): 搜尋關鍵字
- `limit` (integer, 預設: 50): 限制回傳數量

### 4. 相關性搜尋

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

---

## 資料模型 (Schemas)

### BulkMaskUpdateDto
```json
{
  "maskId": 0,
  "newStock": 0,
  "newPrice": 0.0
}
```

### BulkPurchaseDto
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

### BulkPurchaseResultDto
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
```json
{
  "name": "string",
  "price": 0.0,
  "stockQuantity": 0
}
```

### PharmacyDto
```json
{
  "id": 0,
  "name": "string",
  "cashBalance": 0.0,
  "openingHours": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "maskCount": 0
}
```

### PurchaseAnalyticsDto
```json
{
  "totalPurchases": 0,
  "totalRevenue": 0.0,
  "averageOrderValue": 0.0,
  "mostPopularMask": "string",
  "topPharmacy": "string",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-01T00:00:00Z"
}
```

### PurchaseDto
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
```json
{
  "id": 0,
  "name": "string",
  "type": "string",
  "relevanceScore": 0.0
}
```

### SearchResultDto
```json
{
  "pharmacies": [...],
  "masks": [...],
  "totalResults": 0,
  "searchTerm": "string"
}
```

### StockUpdateDto
```json
{
  "operation": "string",
  "quantity": 0
}
```

### TopSpenderDto
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

## 使用範例

### 搜尋口罩
```bash
curl -X GET "https://api.phantommask.com/api/Masks/search?query=N95&limit=10"
```

### 更新口罩庫存
```bash
curl -X PUT "https://api.phantommask.com/api/Masks/1/stock" \
  -H "Content-Type: application/json" \
  -d '{"operation":"add","quantity":100}'
```

### 批量購買
```bash
curl -X POST "https://api.phantommask.com/api/Purchases/bulk" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "purchases": [
      {
        "pharmacyId": 1,
        "maskId": 1,
        "quantity": 2
      }
    ]
  }'
```