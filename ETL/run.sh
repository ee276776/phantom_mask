#!/bin/bash
echo "🎭 PhantomMask ETL 啟動腳本"
echo

echo "📋 檢查必要檔案..."
if [ ! -f "../users.json" ]; then
    echo "❌ 找不到 users.json 檔案"
    exit 1
fi
if [ ! -f "../pharmacies.json" ]; then
    echo "❌ 找不到 pharmacies.json 檔案"
    exit 1
fi

echo "✅ JSON 檔案檢查完成"
echo

echo "🚀 啟動 Docker Compose..."
docker-compose up --build
