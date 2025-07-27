# Tính năng Đăng nhập Google

## Tổng quan

Đã thêm tính năng đăng nhập bằng Google vào hệ thống quản lý rạp chiếu phim. Người dùng có thể đăng nhập bằng tài khoản Google thay vì tạo tài khoản mới.

## Tính năng

### ✅ Đã hoàn thành

1. **Đăng nhập Google OAuth 2.0**
   - Tích hợp Google OAuth 2.0 authentication
   - Nút đăng nhập Google trên trang Login
   - Xử lý callback từ Google

2. **Tự động tạo tài khoản**
   - Tự động tạo tài khoản khách hàng khi đăng nhập Google lần đầu
   - Lấy thông tin email và tên từ Google
   - Gán role "Khách hàng" mặc định

3. **Đăng nhập tài khoản có sẵn**
   - Nếu email đã tồn tại, đăng nhập bình thường
   - Hỗ trợ tất cả các role (Quản lý, Nhân viên, Khách hàng)

4. **Bảo mật**
   - Sử dụng HTTPS
   - Lưu log đăng nhập
   - Xử lý lỗi an toàn

## Cách sử dụng

### Cho người dùng

1. Truy cập trang đăng nhập: `https://localhost:7158/Auth/Login`
2. Click nút "Đăng nhập bằng Google"
3. Chọn tài khoản Google
4. Cho phép truy cập thông tin cơ bản
5. Hệ thống sẽ tự động đăng nhập

### Cho developer

1. Cấu hình Google OAuth theo file `GOOGLE_OAUTH_SETUP.md`
2. Cập nhật `ClientId` và `ClientSecret` trong `appsettings.json`
3. Chạy ứng dụng: `dotnet run`
4. Truy cập: `https://localhost:7158`

## Cấu trúc code

### Controllers
- `AuthController.cs`: Xử lý Google OAuth
  - `GoogleLogin()`: Khởi tạo Google OAuth
  - `GoogleCallback()`: Xử lý callback từ Google
  - `LoginWithGoogleAccount()`: Đăng nhập tài khoản có sẵn
  - `CreateGoogleAccount()`: Tạo tài khoản mới

### Configuration
- `Program.cs`: Cấu hình authentication
- `appsettings.json`: Cấu hình Google OAuth
- `CinemaManagement.csproj`: Thêm packages cần thiết

### Views
- `Login.cshtml`: Thêm nút đăng nhập Google

## Logging

Tất cả hoạt động đăng nhập Google được log vào file `error_log.txt`:

```
[2025-01-XX HH:mm:ss] [GOOGLE_LOGIN] Email: user@gmail.com, Role: Khách hàng
[2025-01-XX HH:mm:ss] [GOOGLE_CREATE] Email: newuser@gmail.com, Name: User Name
[2025-01-XX HH:mm:ss] [GOOGLE_ERROR] Error message
```

## Troubleshooting

### Lỗi thường gặp

1. **"Invalid redirect URI"**
   - Kiểm tra cấu hình trong Google Cloud Console
   - Đảm bảo URL: `https://localhost:7158/Auth/GoogleCallback`
   - Sử dụng HTTPS, không phải HTTP

2. **"Client ID not found"**
   - Kiểm tra ClientId trong appsettings.json
   - Đảm bảo OAuth consent screen đã cấu hình

3. **"Access denied"**
   - Kiểm tra OAuth consent screen
   - Đảm bảo domain được thêm vào authorized domains

4. **"Connection refused"**
   - Đảm bảo ứng dụng chạy trên cổng 7158
   - Kiểm tra firewall và antivirus
   - Thử chạy: `dotnet run --urls "https://localhost:7158"`

### Debug

1. Kiểm tra log trong `error_log.txt`
2. Kiểm tra console browser (F12)
3. Kiểm tra Network tab trong Developer Tools
4. Kiểm tra cổng ứng dụng: `netstat -an | findstr 7158`

## Bảo mật

- ✅ Sử dụng HTTPS
- ✅ Validate email từ Google
- ✅ Logging an toàn
- ✅ Xử lý lỗi
- ✅ Không lưu thông tin nhạy cảm

## Tương lai

Có thể mở rộng thêm:
- Đăng nhập Facebook
- Đăng nhập Microsoft
- Two-factor authentication
- Social login cho nhân viên/quản lý 