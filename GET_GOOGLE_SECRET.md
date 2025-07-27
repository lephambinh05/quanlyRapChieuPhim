# 🚀 Hướng dẫn nhanh: Lấy Google OAuth Client Secret

## 📋 Checklist nhanh:

### ✅ Bước 1: OAuth Consent Screen
- [ ] Vào Google Cloud Console → APIs & Services → OAuth consent screen
- [ ] Chọn **External** (không phải Internal)
- [ ] Điền App name: `Cinema Management System`
- [ ] Chọn scopes: `email`, `profile`, `openid`
- [ ] Thêm test users (nếu cần)

### ✅ Bước 2: Tạo Credentials
- [ ] Vào APIs & Services → Credentials
- [ ] Click "Create Credentials" → "OAuth 2.0 Client IDs"
- [ ] Chọn "Web application"
- [ ] Điền thông tin:
  - Name: `Cinema Management System`
  - Authorized JavaScript origins: `https://localhost:7158`
  - Authorized redirect URIs: `https://localhost:7158/Auth/GoogleCallback`

### ✅ Bước 3: Lấy Client Secret
- [ ] Sau khi tạo, Google hiển thị popup với:
  - **Client ID**: `123456789-abcdef.apps.googleusercontent.com`
  - **Client Secret**: `GOCSPX-xxxxxxxxxxxxxxxxxxxxx`
- [ ] **COPY NGAY** Client Secret (Google chỉ hiển thị 1 lần!)

### ✅ Bước 4: Cập nhật appsettings.json
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "123456789-abcdef.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-xxxxxxxxxxxxxxxxxxxxx"
    }
  }
}
```

## 🔍 Nếu quên Client Secret:

### Cách 1: Download JSON
1. Vào Credentials
2. Click vào OAuth 2.0 Client ID
3. Click "Download JSON"
4. Mở file JSON, tìm `client_secret`

### Cách 2: Show Secret
1. Vào Credentials
2. Click vào OAuth 2.0 Client ID
3. Click "Show" bên cạnh Client Secret

### Cách 3: Tạo mới (nếu không có cách nào khác)
1. Xóa OAuth 2.0 Client ID cũ
2. Tạo lại từ đầu
3. Copy Client Secret ngay lập tức

## ⚠️ Lưu ý quan trọng:

- **Client Secret chỉ hiển thị 1 lần** khi tạo
- **Không commit vào Git** - sử dụng User Secrets
- **Sử dụng HTTPS** cho Google OAuth
- **Test với email thật** để đảm bảo hoạt động

## 🚨 Troubleshooting nhanh:

| Lỗi | Giải pháp |
|-----|-----------|
| "Invalid redirect URI" | Kiểm tra URL: `https://localhost:7158/Auth/GoogleCallback` |
| "Client ID not found" | Kiểm tra ClientId trong appsettings.json |
| "Access denied" | Thêm email vào Test users |
| "App not verified" | Bình thường trong development |

## 🎯 Kết quả mong đợi:

Sau khi cấu hình xong, khi click "Đăng nhập bằng Google":
1. Chuyển đến trang Google
2. Chọn tài khoản
3. Cho phép truy cập
4. Quay lại ứng dụng và đăng nhập thành công 