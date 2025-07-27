# Hướng Dẫn Cấu Hình Dự Án Cinema Management

## Cấu Hình Google OAuth

Để sử dụng tính năng đăng nhập Google, bạn cần cấu hình Google OAuth:

### Bước 1: Tạo Google OAuth Credentials
1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project có sẵn
3. Vào "APIs & Services" > "Credentials"
4. Click "Create Credentials" > "OAuth 2.0 Client IDs"
5. Chọn "Web application"
6. Thêm Authorized redirect URIs: `https://localhost:5001/signin-google`
7. Lưu lại Client ID và Client Secret

### Bước 2: Cập Nhật File Cấu Hình
Thay thế các giá trị placeholder trong file `appsettings.Development.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET"
  }
}
```

## Cấu Hình Email

Để sử dụng tính năng gửi email, cấu hình SMTP:

### Bước 1: Tạo App Password cho Gmail
1. Vào Google Account Settings
2. Security > 2-Step Verification > App passwords
3. Tạo app password cho ứng dụng

### Bước 2: Cập Nhật Email Settings
Thay thế trong file `appsettings.Development.json`:

```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "EnableSsl": true,
  "FromEmail": "your-email@gmail.com",
  "FromName": "Cinema Management System"
}
```

## Cấu Hình Database

Cập nhật connection string trong file cấu hình:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;"
}
```

## Lưu Ý Bảo Mật

- **KHÔNG** commit các file chứa thông tin nhạy cảm lên Git
- Sử dụng biến môi trường hoặc file cấu hình riêng cho production
- Thay thế tất cả placeholder bằng giá trị thực tế trước khi chạy ứng dụng

## Chạy Ứng Dụng

Sau khi cấu hình xong:

```bash
dotnet run
```

Truy cập: https://localhost:5001 