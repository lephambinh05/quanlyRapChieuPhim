@echo off
echo ========================================
echo    Cinema Management System
echo    Google OAuth Setup
echo ========================================
echo.

echo Checking if port 7158 is available...
netstat -an | findstr :7158
if %errorlevel% equ 0 (
    echo Port 7158 is already in use!
    echo Please stop the application using port 7158 first.
    pause
    exit /b 1
)

echo Port 7158 is available.
echo.
echo Starting application on https://localhost:7158
echo.
echo IMPORTANT: Make sure you have configured Google OAuth:
echo 1. Update ClientId and ClientSecret in appsettings.json
echo 2. Configure Google Cloud Console with:
echo    - Authorized JavaScript origins: https://localhost:7158
echo    - Authorized redirect URIs: https://localhost:7158/Auth/GoogleCallback
echo.
echo Press any key to start the application...
pause

dotnet run --urls "https://localhost:7158" 