#!/bin/bash
host="$1"
shift
cmd="$@"

echo "⏳ 等待 PostgreSQL 在 $host:5432 啟動..."

until nc -z "$host" 5432; do
  echo "🔄 PostgreSQL 尚未準備就緒 - 等待中..."
  sleep 2
done

echo "✅ PostgreSQL 已就緒 - 執行 ETL 程式"
exec $cmd
