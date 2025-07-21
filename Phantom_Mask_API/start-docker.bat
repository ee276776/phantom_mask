@echo off
echo 🚀 Starting Phantom Mask API with Docker...
echo.

:: 檢查 Docker 是否運行
docker version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)

:: 停止並移除舊容器
echo 🧹 Cleaning up old containers...
docker-compose down -v

:: 構建並啟動服務
echo 🔨 Building and starting services...
docker-compose up --build -d

:: 等待服務啟動
echo ⏳ Waiting for services to start...
timeout /t 30 /nobreak >nul

:: 檢查服務狀態
echo 📊 Checking service status...
docker-compose ps

echo.
echo ✅ Phantom Mask API is now running!
echo 🌐 Swagger UI: http://localhost/swagger
echo 🗄️  Database: localhost:5432
echo.
echo 📝 To view logs: docker-compose logs -f phantom-mask-api
echo 🛑 To stop: docker-compose down
echo.
pause
