FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PhantomMaskETL.csproj", "."]
RUN dotnet restore "./PhantomMaskETL.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PhantomMaskETL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhantomMaskETL.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 安裝 netcat 用於等待 PostgreSQL 啟動
RUN apt-get update && apt-get install -y netcat-traditional && rm -rf /var/lib/apt/lists/*

# 複製等待腳本
COPY wait-for-postgres.sh /wait-for-postgres.sh
RUN chmod +x /wait-for-postgres.sh

# 創建資料目錄
RUN mkdir -p /app/data

ENTRYPOINT ["/wait-for-postgres.sh", "database", "dotnet", "PhantomMaskETL.dll"]
