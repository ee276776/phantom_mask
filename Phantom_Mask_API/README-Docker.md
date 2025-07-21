# Phantom Mask API - Docker 部署指南

## 🚀 快速啟動

1. **確保 Docker Desktop 正在運行**

2. **啟動 API 服務**
   ```bash
   # 方法 1：使用自動化腳本
   start-docker.bat
   
   # 方法 2：手動執行
   docker-compose up --build -d
   ```

3. **訪問 Swagger UI**
   - 打開瀏覽器，訪問：http://localhost/swagger
   - 所有 API 端點都會顯示在 Swagger 介面中

## 🌐 服務端點

- **Swagger UI**: http://localhost/swagger
- **API Base URL**: http://localhost/api
- **資料庫**: localhost:5432

## 📊 主要 API 端點

### 藥局管理
- `GET /api/pharmacies` - 取得所有藥局
- `GET /api/pharmacies/{id}` - 取得特定藥局
- `POST /api/pharmacies` - 新增藥局
- `PUT /api/pharmacies/{id}` - 更新藥局
- `DELETE /api/pharmacies/{id}` - 刪除藥局

### 口罩管理
- `GET /api/masks` - 取得所有口罩
- `POST /api/masks/bulk-update` - 批量更新口罩庫存

### 購買記錄
- `GET /api/purchases` - 取得購買記錄
- `POST /api/purchases/bulk` - 批量新增購買

### 分析功能
- `GET /api/analytics/top-spenders` - 熱門消費者
- `GET /api/analytics/purchase-analytics` - 購買分析

### 搜尋功能
- `GET /api/search/pharmacies` - 搜尋藥局
- `GET /api/search/users` - 搜尋使用者

## 🛠️ 管理命令

```bash
# 查看服務狀態
docker-compose ps

# 查看 API 日誌
docker-compose logs -f phantom-mask-api

# 查看資料庫日誌
docker-compose logs -f database

# 停止服務
stop-docker.bat
# 或
docker-compose down

# 完全重置（包含資料）
docker-compose down -v
```

## 🗄️ 資料庫資訊

- **主機**: localhost
- **端口**: 5432
- **資料庫**: phantom_mask
- **使用者**: phantom_user
- **密碼**: SecurePass123!

## 🔧 開發模式

如果需要開發時的熱重載，可以使用：

```bash
# 只啟動資料庫
docker-compose up database -d

# 在本地運行 API（需要修改連接字串）
dotnet run
```

## 🐛 故障排除

1. **端口被佔用**
   - 確認 80 和 5432 端口未被使用
   - 或修改 docker-compose.yml 中的端口映射

2. **Docker Desktop 未啟動**
   - 啟動 Docker Desktop 後再執行

3. **資料庫連接失敗**
   - 等待資料庫完全啟動（約 30 秒）
   - 檢查 docker-compose logs database

4. **API 無法訪問**
   - 確認防火牆設置
   - 檢查 docker-compose logs phantom-mask-api

## 📝 注意事項

- 首次啟動時會自動建立資料庫結構
- 預設會載入 ETL 初始化資料
- 連接字串已加密處理
- 支援 CORS 跨域請求
