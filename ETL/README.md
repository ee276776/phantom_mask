# 🎭 PhantomMask ETL 專案

這是一個 C# ETL (Extract, Transform, Load) 專案，用於處理口罩藥房的用戶和交易資料。

## 🏗️ 專案架構

```
ETL/
├── Models/                 # 資料模型
│   ├── User.cs            # 用戶模型
│   └── Pharmacy.cs        # 藥房模型
├── Data/                  # 資料存取層
│   ├── Entities/          # 資料庫實體
│   └── PhantomMaskContext.cs # Entity Framework DbContext
├── Services/              # 業務邏輯層
│   └── ETLService.cs      # ETL 主要服務
├── Program.cs             # 程式進入點
├── Dockerfile             # Docker 容器配置
├── docker-compose.yml     # Docker Compose 配置
├── init.sql              # 資料庫初始化腳本
└── wait-for-postgres.sh   # PostgreSQL 等待腳本
```

## 🚀 快速開始

### 方法1：使用 Docker Compose (推薦)

1. **啟動服務**
```bash
docker-compose up --build
```

2. **查看日誌**
```bash
docker-compose logs -f etl-app
```

3. **清理環境**
```bash
docker-compose down -v
```

### 方法2：本地開發

1. **安裝相依套件**
```bash
dotnet restore
```

2. **設定環境變數**
```bash
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=phantom_mask_db;Username=admin;Password=password123"
```

3. **執行程式**
```bash
dotnet run
```

## 📊 資料流程

1. **Extract (提取)**：從 JSON 檔案讀取用戶和藥房資料
2. **Transform (轉換)**：清理和驗證資料
3. **Load (載入)**：將資料儲存到 PostgreSQL 資料庫

## 🗄️ 資料庫結構

- **Users**：用戶資訊 (姓名、餘額)
- **Pharmacies**：藥房資訊 (名稱、餘額、營業時間)
- **Masks**：口罩資訊 (名稱、價格、庫存、所屬藥房)
- **Purchases**：購買記錄 (用戶、藥房、口罩、交易資訊)

## 🔧 技術堆疊

- **語言**：C# (.NET 8)
- **ORM**：Entity Framework Core
- **資料庫**：PostgreSQL 15
- **容器化**：Docker & Docker Compose
- **日誌**：Microsoft.Extensions.Logging

## 📝 注意事項

- JSON 檔案必須放在專案根目錄
- 確保 Docker 和 Docker Compose 已安裝
- PostgreSQL 預設埠號為 5432

## 🐳 Docker 指令

```bash
# 建立並啟動服務
docker-compose up --build

# 背景執行
docker-compose up -d

# 查看服務狀態
docker-compose ps

# 停止服務
docker-compose stop

# 移除容器和網路
docker-compose down

# 移除容器、網路和資料卷
docker-compose down -v
```

## 📈 監控和除錯

- **查看 ETL 應用程式日誌**：`docker-compose logs etl-app`
- **查看資料庫日誌**：`docker-compose logs database`
- **連接資料庫**：使用任何 PostgreSQL 客戶端連接 `localhost:5432`

---
Made with ❤️ for PhantomMask Project
