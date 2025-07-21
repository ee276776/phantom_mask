@echo off
echo 🎭 PhantomMask ETL 啟動腳本
echo.

echo 📋 檢查必要檔案...
if not exist "../users.json" (
    echo ❌ 找不到 users.json 檔案
    goto :error
)
if not exist "../pharmacies.json" (
    echo ❌ 找不到 pharmacies.json 檔案  
    goto :error
)

echo ✅ JSON 檔案檢查完成

echo.
echo 🚀 啟動 Docker Compose...
docker-compose up --build

goto :eof

:error
echo.
echo 💡 請確保 users.json 和 pharmacies.json 檔案在上層目錄中
pause
