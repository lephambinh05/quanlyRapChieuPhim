# 🔧 Hướng dẫn cuối cùng để sửa lỗi redirect_uri_mismatch

## Vấn đề hiện tại:
- Ứng dụng gửi: `https://localhost:7158/signin-google` ✅
- Google Console có thể chưa được cập nhật đúng
- Cần xóa hoàn toàn và tạo lại OAuth Client ID

## Bước 1: Xóa OAuth Client ID cũ
1. Vào [Google Cloud Console](https://console.cloud.google.com/)
2. Chọn project của bạn
3. Vào **"APIs & Services"** → **"Credentials"**
4. **XÓA** OAuth 2.0 Client ID hiện tại (click vào trash icon)

## Bước 2: Tạo OAuth Client ID mới
1. Click **"Create Credentials"** → **"OAuth 2.0 Client IDs"**
2. Chọn **"Web application"**
3. Điền thông tin:
   - **Name**: `Cinema Management System`
   - **Authorized JavaScript origins**: 
     - `https://localhost:7158`
   - **Authorized redirect URIs**:
     - `https://localhost:7158/signin-google`
4. Click **"Create"**

## Bước 3: Lưu thông tin mới
Sau khi tạo xong, lưu ngay:
- **Client ID**: `123456789-abcdef.apps.googleusercontent.com`
- **Client Secret**: `GOCSPX-xxxxxxxxxxxxxxxxxxxxx`

## Bước 4: Cập nhật appsettings.json
Thay thế trong file `appsettings.json`:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "CLIENT_ID_MỚI",
      "ClientSecret": "CLIENT_SECRET_MỚI"
    }
  }
}
```

## Bước 5: Restart ứng dụng
1. Dừng ứng dụng: `Ctrl+C`
2. Chạy lại: `dotnet run`
3. Test: `https://localhost:7158/Auth/Login`

## Lưu ý quan trọng:
- **KHÔNG** thêm nhiều URI redirect
- **CHỈ** giữ lại `https://localhost:7158/signin-google`
- **Đợi 5-10 phút** sau khi tạo mới
- **Xóa cache browser** hoàn toàn

## Troubleshooting:
- Nếu vẫn lỗi: Kiểm tra lại Client ID và Secret
- Đảm bảo ứng dụng chạy trên `https://localhost:7158`
- Test với tab ẩn danh 