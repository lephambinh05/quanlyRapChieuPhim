# Hướng dẫn cấu hình Email SMTP cho chức năng Quên mật khẩu

## 1. Cấu hình Gmail SMTP

### Bước 1: Tạo App Password cho Gmail
1. Đăng nhập vào tài khoản Google của bạn
2. Vào **Google Account Settings** > **Security**
3. Bật **2-Step Verification** nếu chưa bật
4. Tạo **App Password**:
   - Chọn **App passwords**
   - Chọn **Mail** và **Other (Custom name)**
   - Đặt tên: "Cinema Management System"
   - Copy mật khẩu được tạo ra

### Bước 2: Cập nhật appsettings.json
Thay đổi thông tin email trong file `appsettings.json`:

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

**Lưu ý:** 
- Thay `your-email@gmail.com` bằng email Gmail của bạn
- Thay `your-app-password` bằng App Password đã tạo ở bước 1

## 2. Cấu hình Email khác

### Outlook/Hotmail
```json
{
  "Email": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "Username": "your-email@outlook.com",
    "Password": "your-password",
    "EnableSsl": true,
    "FromEmail": "your-email@outlook.com",
    "FromName": "Cinema Management System"
  }
}
```

### Yahoo Mail
```json
{
  "Email": {
    "SmtpServer": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "Username": "your-email@yahoo.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "your-email@yahoo.com",
    "FromName": "Cinema Management System"
  }
}
```

## 3. Test cấu hình

Sau khi cấu hình xong, bạn có thể test bằng cách:

1. Chạy ứng dụng
2. Vào trang đăng nhập
3. Click "Quên mật khẩu?"
4. Nhập email đã đăng ký trong hệ thống
5. Kiểm tra hộp thư email

## 4. Troubleshooting

### Lỗi thường gặp:

1. **"Authentication failed"**
   - Kiểm tra lại Username và Password
   - Đảm bảo đã sử dụng App Password cho Gmail

2. **"Connection timeout"**
   - Kiểm tra kết nối internet
   - Kiểm tra SmtpServer và SmtpPort

3. **"SSL/TLS error"**
   - Đảm bảo EnableSsl = true
   - Kiểm tra SmtpPort (587 cho TLS, 465 cho SSL)

### Debug:
- Kiểm tra file `error_log.txt` trong thư mục gốc của ứng dụng
- Log sẽ ghi lại các lỗi email với prefix `[EMAIL_ERROR]`

## 5. Bảo mật

- Không commit file `appsettings.json` chứa thông tin email thật lên Git
- Sử dụng User Secrets hoặc Environment Variables cho production
- Thay đổi App Password định kỳ
- Không chia sẻ App Password với người khác

## 6. Production Deployment

Cho môi trường production, nên sử dụng:

1. **Environment Variables:**
```bash
set Email__Username=your-email@gmail.com
set Email__Password=your-app-password
```

2. **Azure Key Vault** hoặc **AWS Secrets Manager**

3. **Dedicated Email Service** như SendGrid, Mailgun, Amazon SES 