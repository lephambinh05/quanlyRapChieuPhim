# HƯỚNG DẪN SỬ DỤNG CRON JOB CHECK BANKING

## Tổng quan
Hệ thống đã được tích hợp sẵn cron job để tự động check API bank và cập nhật trạng thái thanh toán cho các đơn hàng.

## Tính năng chính

### 1. Logic xử lý
- **Kiểm tra nội dung chuyển khoản**: Hệ thống sẽ kiểm tra xem nội dung giao dịch có chứa mã đơn hàng hay không
- **Không cần trùng hoàn toàn**: Chỉ cần nội dung chứa mã đơn hàng, không cần phải trùng 100%
- **Tự động cập nhật**: Khi tìm thấy giao dịch khớp, trạng thái đơn hàng sẽ được cập nhật thành "Đã thanh toán"

### 2. Logging chi tiết
- **Console logging**: In ra console để theo dõi real-time
- **File logging**: Lưu log vào file `logs/banking_log.txt`
- **Cron log**: Lưu log cron job vào file `logs/cron_log.txt`

## API Endpoints

### 1. Chạy cron job check banking
```
GET /api/cron/run-banking-cron
```
- Chạy cron job check banking một lần
- Trả về kết quả chi tiết

### 2. Check banking thủ công
```
GET /api/cron/banking-check
```
- Check banking mà không cập nhật database
- Dùng để test và debug

### 3. Lấy lịch sử giao dịch
```
GET /api/cron/banking-history
```
- Lấy toàn bộ lịch sử giao dịch từ API bank
- In ra console chi tiết từng giao dịch

### 4. Chạy tất cả cron jobs
```
GET /api/cron
```
- Chạy tất cả cron jobs (banking + hủy đơn quá hạn + nhả ghế)

## Cách sử dụng

### 1. Chạy thủ công qua API
```bash
# Chạy cron job check banking
curl -X GET "https://localhost:7001/api/cron/run-banking-cron"

# Check banking thủ công
curl -X GET "https://localhost:7001/api/cron/banking-check"

# Lấy lịch sử giao dịch
curl -X GET "https://localhost:7001/api/cron/banking-history"
```

### 2. Chạy tự động với PowerShell script
```powershell
# Chạy với cấu hình mặc định (2 phút/lần)
.\run-banking-cron.ps1

# Chạy với interval tùy chỉnh (5 phút/lần)
.\run-banking-cron.ps1 -IntervalMinutes 5

# Chạy với URL tùy chỉnh
.\run-banking-cron.ps1 -BaseUrl "https://your-domain.com" -IntervalMinutes 3
```

### 3. Chạy tự động với Windows Task Scheduler
1. Mở Task Scheduler
2. Tạo task mới
3. Cấu hình trigger mỗi 2 phút
4. Action: `powershell.exe -File "C:\path\to\run-banking-cron.ps1"`

## Cấu trúc log

### Banking log (`logs/banking_log.txt`)
```
[2024-01-15 10:30:00] === BẮT ĐẦU KIỂM TRA BANKING 2024-01-15 10:30:00 ===
[2024-01-15 10:30:00] Tìm thấy 2 đơn hàng chờ thanh toán:
[2024-01-15 10:30:00]   - HD001: Chờ chuyển khoản (tạo lúc 10:25:00)
[2024-01-15 10:30:00]   - HD002: Chờ chuyển khoản (tạo lúc 10:28:00)
[2024-01-15 10:30:00] Gọi API: https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123
[2024-01-15 10:30:01] Response Status: 200
[2024-01-15 10:30:01] Tổng số giao dịch: 5
[2024-01-15 10:30:01] === LỊCH SỬ GIAO DỊCH NGÂN HÀNG ===
[2024-01-15 10:30:01]   - ID: TX001
[2024-01-15 10:30:01]     Số tiền: 150,000 VND
[2024-01-15 10:30:01]     Nội dung: Chuyen tien HD001
[2024-01-15 10:30:01]     Ngày: 15/01/2024
[2024-01-15 10:30:01]     Loại: IN
[2024-01-15 10:30:01]     ---
[2024-01-15 10:30:01] Mã đơn hợp lệ: HD001, HD002
[2024-01-15 10:30:01] Giao dịch có chứa mã đơn: 1
[2024-01-15 10:30:01]   - TX001: 150,000 VND - Chuyen tien HD001 (ngày: 15/01/2024)
[2024-01-15 10:30:01] Kiểm tra đơn HD001...
[2024-01-15 10:30:01] ✓ Tìm thấy thanh toán cho đơn HD001: 150,000 VND
[2024-01-15 10:30:01] Nội dung giao dịch: Chuyen tien HD001
[2024-01-15 10:30:01] Cập nhật trạng thái từ 'Chờ chuyển khoản' thành 'Đã thanh toán'
[2024-01-15 10:30:01] Kiểm tra đơn HD002...
[2024-01-15 10:30:01] ✗ Chưa có thanh toán cho đơn HD002
[2024-01-15 10:30:01] Lưu thay đổi vào database...
[2024-01-15 10:30:01] ✓ Đã cập nhật 1 đơn hàng thành công
[2024-01-15 10:30:01] === KẾT THÚC KIỂM TRA BANKING 2024-01-15 10:30:01 ===
```

### Cron log (`logs/cron_log.txt`)
```
[2024-01-15 10:30:00] Kết quả: Tổng giao dịch=5, Khớp=1, Cập nhật=1
[2024-01-15 10:32:00] Kết quả: Tổng giao dịch=5, Khớp=0, Cập nhật=0
[2024-01-15 10:34:00] Kết quả: Tổng giao dịch=5, Khớp=0, Cập nhật=0
```

## Cấu hình

### 1. API URL
- URL mặc định: `https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123`
- Có thể thay đổi trong `Services/BankingService.cs`

### 2. Interval
- Mặc định: 2 phút/lần
- Có thể thay đổi trong PowerShell script

### 3. Timeout
- API timeout: 30 giây
- Cron job timeout: 60 giây

## Troubleshooting

### 1. Lỗi kết nối API
- Kiểm tra internet connection
- Kiểm tra API URL có đúng không
- Kiểm tra API key có hợp lệ không

### 2. Không cập nhật được đơn hàng
- Kiểm tra mã đơn hàng có đúng format không
- Kiểm tra nội dung giao dịch có chứa mã đơn không
- Kiểm tra log để xem chi tiết

### 3. Script PowerShell không chạy
- Kiểm tra Execution Policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned`
- Kiểm tra URL có đúng không
- Kiểm tra port có mở không

## Lưu ý quan trọng

1. **Bảo mật**: API key được hardcode trong code, nên thay đổi trong production
2. **Performance**: Cron job chạy mỗi 2 phút, có thể điều chỉnh theo nhu cầu
3. **Log rotation**: Log files có thể lớn, nên xóa định kỳ
4. **Monitoring**: Nên monitor log files để đảm bảo hệ thống hoạt động tốt
5. **Backup**: Nên backup database trước khi chạy cron job

## Ví dụ sử dụng

### Test thủ công
```bash
# 1. Tạo đơn hàng test
# 2. Chạy cron job
curl -X GET "https://localhost:7001/api/cron/run-banking-cron"
# 3. Kiểm tra log
tail -f logs/banking_log.txt
```

### Chạy tự động
```powershell
# Chạy script PowerShell
.\run-banking-cron.ps1 -IntervalMinutes 1
```

### Monitor real-time
```bash
# Xem log real-time
Get-Content logs/banking_log.txt -Wait
``` 