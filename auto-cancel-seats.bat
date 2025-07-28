@echo off
chcp 65001 >nul
REM Script tự động hủy ghế sau 5 giây
REM Sử dụng: auto-cancel-seats.bat

set BaseUrl=https://localhost:7158
set IntervalSeconds=5

echo === TỰ ĐỘNG HỦY GHẾ SAU 5 GIÂY ===
echo Base URL: %BaseUrl%
echo Interval: %IntervalSeconds% giây
echo Nhấn Ctrl+C để dừng
echo.

REM Tạo thư mục logs nếu chưa có
if not exist "logs" mkdir logs

REM Hàm gọi API hủy ghế
:call_cancel_api
echo [%date% %time%] Gọi API hủy ghế...
powershell -Command "try { $response = Invoke-RestMethod -Uri '%BaseUrl%/api/cron' -Method GET -TimeoutSec 60; Write-Host '[%date% %time%] Kết quả hủy ghế: Tổng đơn hủy='$response.totalCancelled', Tổng ghế nhả='$response.totalReleasedSeats; } catch { Write-Host '[%date% %time%] LỖI hủy ghế:' $_.Exception.Message }"

REM Lưu log vào file
echo [%date% %time%] Chạy hủy ghế tự động >> logs\cancel_seats_log.txt

REM Chờ 5 giây
echo [%date% %time%] Chờ %IntervalSeconds% giây...
timeout /t %IntervalSeconds% /nobreak >nul

REM Lặp lại
goto call_cancel_api 