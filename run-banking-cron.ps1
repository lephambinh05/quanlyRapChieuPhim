# Script chạy cron job check banking
# Sử dụng: .\run-banking-cron.ps1

param(
    [string]$BaseUrl = "https://localhost:7158",
    [int]$IntervalSeconds = 5
)

Write-Host "=== BẮT ĐẦU CRON JOB CHECK BANKING ===" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Interval: $IntervalSeconds giây" -ForegroundColor Yellow
Write-Host "Nhấn Ctrl+C để dừng" -ForegroundColor Red
Write-Host ""

# Tạo thư mục logs nếu chưa có
$logsDir = "logs"
if (!(Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir
}

# Hàm gọi API
function Invoke-BankingCron {
    try {
        $url = "$BaseUrl/api/cron/run-banking-cron"
        Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Gọi API: $url" -ForegroundColor Cyan
        
        $response = Invoke-RestMethod -Uri $url -Method GET -TimeoutSec 60
        $logMessage = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Kết quả: Tổng giao dịch=$($response.totalTransactions), Khớp=$($response.matchedOrders), Cập nhật=$($response.updatedOrders)"
        
        if ($response.updatedOrders -gt 0) {
            Write-Host $logMessage -ForegroundColor Green
        } else {
            Write-Host $logMessage -ForegroundColor Yellow
        }
        
        # Lưu log vào file
        $logMessage | Out-File -FilePath "$logsDir\cron_log.txt" -Append -Encoding UTF8
        
        return $true
    }
    catch {
        $errorMsg = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] LỖI: $($_.Exception.Message)"
        Write-Host $errorMsg -ForegroundColor Red
        $errorMsg | Out-File -FilePath "$logsDir\cron_log.txt" -Append -Encoding UTF8
        return $false
    }
}

# Hàm gọi API lịch sử giao dịch
function Get-BankingHistory {
    try {
        $url = "$BaseUrl/api/cron/banking-history"
        Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Lấy lịch sử giao dịch: $url" -ForegroundColor Cyan
        
        $response = Invoke-RestMethod -Uri $url -Method GET -TimeoutSec 60
        Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Lịch sử: Tổng giao dịch=$($response.result.totalTransactions)" -ForegroundColor Blue
        
        return $true
    }
    catch {
        $errorMsg = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] LỖI lấy lịch sử: $($_.Exception.Message)"
        Write-Host $errorMsg -ForegroundColor Red
        return $false
    }
}

# Chạy lần đầu
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Chạy lần đầu..." -ForegroundColor Green
Invoke-BankingCron

# Vòng lặp chính
while ($true) {
    try {
        Write-Host ""
        Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Chờ $IntervalSeconds giây..." -ForegroundColor Gray
        Start-Sleep -Seconds $IntervalSeconds
        
        Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Chạy cron job..." -ForegroundColor Green
        Invoke-BankingCron
        
        # Lấy lịch sử giao dịch mỗi 10 phút
        $currentMinute = (Get-Date).Minute
        if ($currentMinute % 10 -eq 0) {
            Get-BankingHistory
        }
    }
    catch {
        Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] LỖI VÒNG LẶP: $($_.Exception.Message)" -ForegroundColor Red
        Start-Sleep -Seconds 30
    }
} 