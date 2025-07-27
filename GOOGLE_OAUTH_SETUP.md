# Hướng dẫn cập nhật Google OAuth cho HTTP

## Bước 1: Cập nhật Google Cloud Console

### Vào Google Cloud Console:
1. Truy cập: https://console.cloud.google.com/
2. Chọn project của bạn
3. Vào **APIs & Services** > **Credentials**

### Chỉnh sửa OAuth 2.0 Client ID:
1. Click vào OAuth 2.0 Client ID hiện tại
2. Click **"Edit"** (biểu tượng bút chì)

### Cập nhật Authorized JavaScript origins:
- **XÓA**: `https://localhost:7158`
- **THÊM**: `http://localhost:5039`

### Cập nhật Authorized redirect URIs:
- **XÓA**: `https://localhost:7158/signin-google` (nếu có)
- **THÊM**: `http://localhost:5039/signin-google`

### Click "Save"

## Bước 2: Test ứng dụng
1. **Truy cập**: `http://localhost:5039/Auth/Login`
2. **Click**: "Đăng nhập bằng Google"
3. **Chọn**: Tài khoản Google của bạn

## Lưu ý:
- Sử dụng HTTP thay vì HTTPS để tránh vấn đề với secure cookies
- Port 5039 thay vì 7158
- Đợi vài phút sau khi cập nhật Google Console 