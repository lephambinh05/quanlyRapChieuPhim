@echo off
chcp 65001 >nul
title Cinema Management - Khởi động ứng dụng

echo.
echo ========================================
echo    KHỞI ĐỘNG ỨNG DỤNG CINEMA MANAGEMENT
echo ========================================
echo.

echo 🚀 Đang khởi động ứng dụng...
echo.

REM Chạy ứng dụng và mở trình duyệt
powershell -ExecutionPolicy Bypass -File "start-app.ps1"

echo.
echo ✅ Hoàn thành!
pause 