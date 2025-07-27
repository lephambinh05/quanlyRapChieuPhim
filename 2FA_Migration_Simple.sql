-- =====================================================
-- MIGRATION 2FA - CHẠY TRONG SQL SERVER MANAGEMENT STUDIO
-- =====================================================

-- 1. Thêm các cột 2FA vào bảng TaiKhoan
ALTER TABLE TaiKhoan 
ADD TwoFactorSecret NVARCHAR(32) NULL,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorVerified BIT NOT NULL DEFAULT 0,
    BackupCodes NVARCHAR(200) NULL,
    TwoFactorSetupDate DATETIME NULL;

-- 2. Tạo bảng lưu trữ password reset tokens
CREATE TABLE PasswordResetTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL,
    Token NVARCHAR(100) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- 3. Tạo index cho bảng PasswordResetTokens
CREATE INDEX IX_PasswordResetTokens_Email ON PasswordResetTokens(Email);
CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token);

-- 4. Kiểm tra kết quả
SELECT 'Migration hoàn thành!' as Status;

-- 5. Kiểm tra cấu trúc bảng TaiKhoan sau khi cập nhật
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TaiKhoan' 
ORDER BY ORDINAL_POSITION;

-- 6. Kiểm tra bảng PasswordResetTokens
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PasswordResetTokens' 
ORDER BY ORDINAL_POSITION; 