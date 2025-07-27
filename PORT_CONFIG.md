# Cấu hình Cổng - Cinema Management System

## Thông tin cổng hiện tại

### Development Environment
- **HTTPS**: `https://localhost:7158`
- **HTTP**: `http://localhost:5039`
- **Primary URL**: `https://localhost:7158`

### Google OAuth Configuration
- **Authorized JavaScript origins**: `https://localhost:7158`
- **Authorized redirect URIs**: `https://localhost:7158/Auth/GoogleCallback`

## Cách chạy ứng dụng

### Option 1: Sử dụng script (Recommended)
```bash
# Windows Batch
run-app.bat

# Windows PowerShell
.\run-app.ps1
```

### Option 2: Chạy trực tiếp
```bash
dotnet run --urls "https://localhost:7158"
```

### Option 3: Chạy mặc định
```bash
dotnet run
# Sẽ chạy trên https://localhost:7158 và http://localhost:5039
```

## Kiểm tra cổng

### Windows
```cmd
netstat -an | findstr :7158
```

### PowerShell
```powershell
Get-NetTCPConnection -LocalPort 7158
```

## Troubleshooting

### Lỗi "Port already in use"
1. Tìm process đang sử dụng cổng:
   ```cmd
   netstat -ano | findstr :7158
   ```
2. Kill process:
   ```cmd
   taskkill /PID <PID> /F
   ```

### Lỗi "Connection refused"
1. Kiểm tra firewall
2. Kiểm tra antivirus
3. Thử chạy với quyền Administrator

### Lỗi "SSL Certificate"
- Trong development, chấp nhận certificate không tin cậy
- Hoặc tạo development certificate:
  ```bash
  dotnet dev-certs https --trust
  ```

## URLs quan trọng

- **Login Page**: `https://localhost:7158/Auth/Login`
- **Google Callback**: `https://localhost:7158/Auth/GoogleCallback`
- **Home Page**: `https://localhost:7158/`

## Production Deployment

Khi deploy lên production, cần:
1. Cập nhật Google OAuth redirect URIs
2. Sử dụng domain thực tế
3. Cấu hình SSL certificate
4. Cập nhật appsettings.Production.json 