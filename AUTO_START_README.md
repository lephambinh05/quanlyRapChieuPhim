# HƯỚNG DẪN TỰ ĐỘNG MỞ TRÌNH DUYỆT KHI KHỞI ĐỘNG

## Tính năng đã thêm

### 1. Tự động mở trình duyệt trong code (Program.cs)
- **Chức năng**: Tự động mở trình duyệt khi ứng dụng khởi động (chỉ trong Development mode)
- **Cách hoạt động**: 
  - Chờ 2 giây sau khi ứng dụng khởi động
  - Tự động mở trình duyệt với URL của ứng dụng
  - Chỉ hoạt động trong môi trường Development

### 2. Script PowerShell (start-app.ps1)
- **Chức năng**: Khởi động ứng dụng và tự động mở trình duyệt
- **Tính năng**:
  - Kiểm tra xem ứng dụng đã chạy chưa
  - Khởi động ứng dụng nếu chưa chạy
  - Chờ ứng dụng khởi động (tối đa 30 giây)
  - Tự động mở trình duyệt khi ứng dụng sẵn sàng
  - Hiển thị thông tin các trang chính

### 3. File Batch (start-app.bat)
- **Chức năng**: Wrapper để chạy script PowerShell
- **Ưu điểm**: Dễ dàng double-click để chạy

## Cách sử dụng

### 1. Chạy thủ công với dotnet run
```bash
# Chạy ứng dụng bình thường
dotnet run

# Trình duyệt sẽ tự động mở sau 2 giây (chỉ trong Development)
```

### 2. Chạy với PowerShell script
```powershell
# Chạy với cấu hình mặc định
.\start-app.ps1

# Chạy không mở trình duyệt
.\start-app.ps1 -NoBrowser

# Chạy với URL tùy chỉnh
.\start-app.ps1 -BaseUrl "https://localhost:5039"
```

### 3. Chạy với file batch
```bash
# Double-click file start-app.bat
# Hoặc chạy từ command line
start-app.bat
```

## Các URL chính

Sau khi ứng dụng khởi động, bạn có thể truy cập:

- **🏠 Trang chủ**: `https://localhost:7158/`
- **📊 Quản lý**: `https://localhost:7158/QuanLy`
- **🎫 Bán vé**: `https://localhost:7158/BanVe`
- **👥 Khách hàng**: `https://localhost:7158/KhachHang`
- **🔐 Đăng nhập**: `https://localhost:7158/Auth/Login`

## Tính năng nâng cao

### 1. Kiểm tra ứng dụng đã chạy
Script sẽ kiểm tra xem ứng dụng đã chạy chưa trước khi khởi động mới.

### 2. Timeout và retry
- Timeout khởi động: 30 giây
- Chờ ổn định: 2 giây trước khi mở trình duyệt

### 3. Error handling
- Xử lý lỗi khởi động ứng dụng
- Xử lý lỗi mở trình duyệt
- Thông báo lỗi chi tiết

### 4. Monitoring
- Hiển thị tiến trình khởi động
- Thông báo trạng thái real-time
- Logging chi tiết

## Cấu hình

### 1. Thay đổi URL mặc định
Trong `start-app.ps1`:
```powershell
param(
    [string]$BaseUrl = "https://localhost:7158",  # Thay đổi ở đây
    [switch]$NoBrowser = $false
)
```

### 2. Thay đổi timeout
Trong `start-app.ps1`:
```powershell
$timeout = 30  # Thay đổi số giây chờ
```

### 3. Tắt auto-open trong code
Trong `Program.cs`, comment hoặc xóa đoạn code:
```csharp
// Tự động mở trình duyệt khi khởi động (chỉ trong Development)
if (app.Environment.IsDevelopment())
{
    // ... code auto-open
}
```

## Troubleshooting

### 1. Không mở được trình duyệt
- Kiểm tra trình duyệt mặc định
- Kiểm tra quyền truy cập
- Thử chạy với `-NoBrowser` và mở thủ công

### 2. Ứng dụng không khởi động
- Kiểm tra port có bị chiếm không
- Kiểm tra firewall
- Chạy `dotnet run` thủ công để xem lỗi

### 3. Script PowerShell không chạy
- Kiểm tra Execution Policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned`
- Chạy PowerShell với quyền Administrator

### 4. Lỗi SSL certificate
- Chạy: `dotnet dev-certs https --trust`
- Hoặc truy cập `http://localhost:5039` thay vì HTTPS

## Lưu ý quan trọng

### 1. Bảo mật
- Auto-open chỉ hoạt động trong Development mode
- Không nên bật trong Production

### 2. Performance
- Script chờ tối đa 30 giây để khởi động
- Có thể điều chỉnh theo hiệu suất máy

### 3. Compatibility
- Hoạt động trên Windows với PowerShell
- Có thể cần điều chỉnh cho Linux/Mac

## Ví dụ sử dụng

### Khởi động nhanh
```bash
# Cách 1: Double-click start-app.bat
# Cách 2: Chạy PowerShell script
.\start-app.ps1
# Cách 3: Chạy thủ công
dotnet run
```

### Khởi động với tùy chỉnh
```powershell
# Không mở trình duyệt
.\start-app.ps1 -NoBrowser

# URL tùy chỉnh
.\start-app.ps1 -BaseUrl "https://localhost:5039"
```

### Monitor quá trình
```powershell
# Xem log real-time
Get-Content logs\banking_log.txt -Wait

# Xem process
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}
```

## Kết luận

Hệ thống đã được tích hợp đầy đủ tính năng tự động mở trình duyệt:

✅ **Auto-open trong code**: Tự động mở khi khởi động ứng dụng  
✅ **Script PowerShell**: Khởi động và mở trình duyệt tự động  
✅ **File Batch**: Dễ dàng double-click để chạy  
✅ **Error handling**: Xử lý lỗi và timeout  
✅ **Monitoring**: Theo dõi quá trình khởi động  
✅ **Documentation**: Hướng dẫn chi tiết sử dụng  

Bây giờ bạn có thể khởi động ứng dụng một cách dễ dàng và trình duyệt sẽ tự động mở! 