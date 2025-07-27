# 🔧 Hướng dẫn cập nhật Google Console để sửa lỗi redirect_uri_mismatch

## Vấn đề hiện tại:
- Ứng dụng gửi: `https://localhost:7158/signin-google`
- Google Console có: `http://localhost:5039/signin-google` và `https://localhost:7158/signin-google`
- Cần xóa URI cũ để tránh conflict

## Bước 1: Truy cập Google Cloud Console
1. Vào [Google Cloud Console](https://console.cloud.google.com/)
2. Chọn project của bạn
3. Vào **"APIs & Services"** → **"Credentials"**

## Bước 2: Chỉnh sửa OAuth 2.0 Client ID
1. Click vào **OAuth 2.0 Client ID** đã tạo
2. Trong phần **"Authorized redirect URIs"**:
   - **XÓA**: `http://localhost:5039/signin-google`
   - **CHỈ GIỮ LẠI**: `https://localhost:7158/signin-google`
3. Click **"Save"**

## Bước 3: Đợi cập nhật
- Google có thể mất **5-10 phút** để cập nhật
- Trong thời gian này, có thể vẫn gặp lỗi

## Bước 4: Test lại
1. Đợi 10 phút
2. Truy cập: `https://localhost:7158/Auth/Login`
3. Click "Đăng nhập bằng Google"
4. Chọn tài khoản `lephambinh05@gmail.com`

## Lưu ý quan trọng:
- **KHÔNG** thêm nhiều URI redirect
- **CHỈ** giữ lại URI đang sử dụng
- Đảm bảo **HTTPS** cho localhost:7158
- Đảm bảo **HTTP** cho localhost:5039 (nếu dùng)

## Troubleshooting:
- Nếu vẫn lỗi sau 10 phút: Xóa và tạo lại OAuth Client ID
- Kiểm tra log trong `error_log.txt` để xem URI thực tế
- Đảm bảo ứng dụng chạy trên đúng port 