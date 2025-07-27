# Chức năng Quên mật khẩu - Hệ thống Quản lý Rạp Chiếu Phim

## Tổng quan

Chức năng quên mật khẩu cho phép người dùng đặt lại mật khẩu thông qua email khi quên mật khẩu đăng nhập.

## Tính năng chính

### 1. Giao diện người dùng
- **Trang quên mật khẩu**: `/Auth/ForgotPassword`
- **Trang đặt lại mật khẩu**: `/Auth/ResetPassword`
- **Link từ trang đăng nhập**: "Quên mật khẩu?"

### 2. Bảo mật
- Token có hiệu lực 30 phút
- Token chỉ sử dụng được 1 lần
- Không tiết lộ thông tin email có tồn tại hay không
- Mật khẩu mới phải có ít nhất 6 ký tự

### 3. Email template
- Giao diện HTML đẹp mắt
- Responsive design
- Thông tin bảo mật
- Link trực tiếp để đặt lại mật khẩu

## Cấu trúc code

### Models
- `ResetPasswordModel`: Model cho form quên mật khẩu
- `ResetPasswordConfirmModel`: Model cho form đặt lại mật khẩu
- `PasswordResetToken`: Model cho token reset

### Services
- `IEmailService` & `EmailService`: Gửi email qua SMTP
- `IPasswordResetService` & `PasswordResetService`: Quản lý token reset

### Controllers
- `AuthController`: Xử lý logic quên mật khẩu

### Views
- `ForgotPassword.cshtml`: Trang nhập email
- `ResetPassword.cshtml`: Trang đặt lại mật khẩu

## Luồng hoạt động

### 1. Yêu cầu đặt lại mật khẩu
```
User → ForgotPassword → Nhập email → Validate email → Tạo token → Gửi email
```

### 2. Đặt lại mật khẩu
```
User → Click link email → ResetPassword → Validate token → Nhập mật khẩu mới → Update database
```

## Cấu hình

### 1. Email Settings (appsettings.json)
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "your-email@gmail.com",
    "FromName": "Cinema Management System"
  }
}
```

### 2. Service Registration (Program.cs)
```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
```

## API Endpoints

### GET /Auth/ForgotPassword
- Hiển thị form nhập email

### POST /Auth/ForgotPassword
- Nhận email từ form
- Tạo token reset
- Gửi email với link reset

### GET /Auth/ResetPassword?email={email}&token={token}
- Validate token
- Hiển thị form đặt lại mật khẩu

### POST /Auth/ResetPassword
- Nhận mật khẩu mới
- Update database
- Mark token as used

## Bảo mật

### Token Management
- Token được tạo bằng `RNGCryptoServiceProvider`
- Token có thời gian hết hạn (30 phút)
- Token chỉ sử dụng được 1 lần
- Cleanup tự động cho token hết hạn

### Email Security
- Không tiết lộ thông tin email có tồn tại
- Sử dụng App Password cho Gmail
- SSL/TLS encryption
- Rate limiting (có thể thêm)

### Password Requirements
- Tối thiểu 6 ký tự
- Validation client-side và server-side
- Password strength indicator

## Logging

Tất cả lỗi được log vào file `error_log.txt`:
- `[EMAIL_ERROR]`: Lỗi gửi email
- `[FORGOT_PASSWORD_ERROR]`: Lỗi quên mật khẩu
- `[RESET_PASSWORD_ERROR]`: Lỗi đặt lại mật khẩu

## Testing

### Test Cases
1. **Email không tồn tại**: Hiển thị thông báo thành công (không tiết lộ)
2. **Email tồn tại**: Gửi email thành công
3. **Token hợp lệ**: Cho phép đặt lại mật khẩu
4. **Token hết hạn**: Hiển thị lỗi
5. **Token đã sử dụng**: Hiển thị lỗi
6. **Mật khẩu yếu**: Validation error

### Test Data
```sql
-- Tài khoản test
INSERT INTO TaiKhoans (MaTK, Email, MatKhau, Role, TrangThai) 
VALUES ('TK001', 'test@example.com', 'password123', 'Khách hàng', 'Hoạt động');
```

## Troubleshooting

### Lỗi thường gặp
1. **Email không gửi được**: Kiểm tra cấu hình SMTP
2. **Token không hợp lệ**: Kiểm tra thời gian hết hạn
3. **Link không hoạt động**: Kiểm tra URL generation

### Debug Commands
```bash
# Kiểm tra log
tail -f error_log.txt

# Test email service
dotnet run --environment Development
```

## Future Enhancements

### Tính năng có thể thêm
1. **Rate Limiting**: Giới hạn số lần gửi email
2. **Captcha**: Bảo vệ chống spam
3. **SMS Reset**: Gửi mã qua SMS
4. **Security Questions**: Câu hỏi bảo mật
5. **Audit Log**: Ghi log chi tiết hơn

### Performance Optimization
1. **Email Queue**: Sử dụng background service
2. **Caching**: Cache token trong Redis
3. **Async Processing**: Xử lý email bất đồng bộ

## Deployment

### Production Checklist
- [ ] Cấu hình email production
- [ ] SSL certificate
- [ ] Environment variables
- [ ] Monitoring và alerting
- [ ] Backup strategy
- [ ] Security audit

### Monitoring
- Email delivery rate
- Token usage statistics
- Error rates
- Response times 