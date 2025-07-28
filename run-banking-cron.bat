@echo off
chcp 65001 >nul
REM Script chạy cron job check banking
REM Sử dụng: run-banking-cron.bat

set BaseUrl=https://localhost:7158
set IntervalSeconds=5

echo === BẮT ĐẦU CRON JOB CHECK BANKING ===
echo Base URL: %BaseUrl%
echo Interval: %IntervalSeconds% giây
echo Nhấn Ctrl+C để dừng
echo.

REM Tạo thư mục logs nếu chưa có
if not exist "logs" mkdir logs

REM Chạy lần đầu
echo [%date% %time%] Chạy lần đầu...
powershell -Command "try { $response = Invoke-RestMethod -Uri '%BaseUrl%/api/cron/run-banking-cron' -Method GET -TimeoutSec 60; Write-Host '[%date% %time%] Kết quả: Tổng giao dịch='$response.totalTransactions', Khớp='$response.matchedOrders', Cập nhật='$response.updatedOrders; } catch { Write-Host '[%date% %time%] LỖI:' $_.Exception.Message }"

REM Vòng lặp chính
:loop
echo.
echo [%date% %time%] Chờ %IntervalSeconds% giây...
timeout /t %IntervalSeconds% /nobreak >nul

echo [%date% %time%] Chạy cron job...
powershell -Command "try { $response = Invoke-RestMethod -Uri '%BaseUrl%/api/cron/run-banking-cron' -Method GET -TimeoutSec 60; Write-Host '[%date% %time%] Kết quả: Tổng giao dịch='$response.totalTransactions', Khớp='$response.matchedOrders', Cập nhật='$response.updatedOrders; } catch { Write-Host '[%date% %time%] LỖI:' $_.Exception.Message }"

REM Lấy lịch sử giao dịch mỗi 10 phút (600 giây)
set /a "currentSecond=%time:~6,2%"
set /a "currentMinute=%time:~3,2%"
set /a "totalSeconds=!currentMinute!*60+!currentSecond!"
set /a "remainder=!totalSeconds! %% 600"
if !remainder!==0 (
    echo [%date% %time%] Lấy lịch sử giao dịch...
    powershell -Command "try { $response = Invoke-RestMethod -Uri '%BaseUrl%/api/cron/banking-history' -Method GET -TimeoutSec 60; Write-Host '[%date% %time%] Lịch sử: Tổng giao dịch='$response.result.totalTransactions; } catch { Write-Host '[%date% %time%] LỖI lấy lịch sử:' $_.Exception.Message }"
)

goto loop 