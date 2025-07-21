-- 資料庫初始化腳本
-- 這個腳本會在 PostgreSQL 容器首次啟動時執行

-- 確保使用正確的資料庫
\c phantom_mask;

-- 創建使用者表
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    cashbalance DECIMAL(10,2) NOT NULL,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 創建藥房表
CREATE TABLE IF NOT EXISTS pharmacies (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    cashbalance DECIMAL(10,2) NOT NULL,
    openinghours VARCHAR(500) NOT NULL,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 創建口罩表
CREATE TABLE IF NOT EXISTS masks (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    stockquantity INTEGER NOT NULL,
    pharmacyid INTEGER NOT NULL,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pharmacyid) REFERENCES pharmacies(id) ON DELETE CASCADE
);

-- 創建購買記錄表
CREATE TABLE IF NOT EXISTS purchases (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    pharmacyname VARCHAR(255) NOT NULL,
    maskname VARCHAR(255) NOT NULL,
    transactionamount DECIMAL(10,2) NOT NULL,
    transactionquantity INTEGER NOT NULL,
    transactiondatetime TIMESTAMP WITH TIME ZONE NOT NULL,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 建立索引以提高查詢效能
CREATE INDEX IF NOT EXISTS ix_users_name ON users(name);
CREATE INDEX IF NOT EXISTS ix_pharmacies_name ON pharmacies(name);
CREATE INDEX IF NOT EXISTS ix_purchases_transactiondatetime ON purchases(transactiondatetime);
CREATE INDEX IF NOT EXISTS ix_purchases_username ON purchases(username);
CREATE INDEX IF NOT EXISTS ix_purchases_pharmacyname ON purchases(pharmacyname);

-- 顯示訊息
\echo '✅ 資料庫初始化完成！';
