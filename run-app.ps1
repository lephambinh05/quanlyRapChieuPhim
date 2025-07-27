# Cinema Management System - Google OAuth Setup
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Cinema Management System" -ForegroundColor Yellow
Write-Host "   Google OAuth Setup" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if port 7158 is available
Write-Host "Checking if port 7158 is available..." -ForegroundColor Green
$portCheck = Get-NetTCPConnection -LocalPort 7158 -ErrorAction SilentlyContinue

if ($portCheck) {
    Write-Host "Port 7158 is already in use!" -ForegroundColor Red
    Write-Host "Please stop the application using port 7158 first." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Port 7158 is available." -ForegroundColor Green
Write-Host ""

Write-Host "Starting application on https://localhost:7158" -ForegroundColor Green
Write-Host ""

Write-Host "IMPORTANT: Make sure you have configured Google OAuth:" -ForegroundColor Yellow
Write-Host "1. Update ClientId and ClientSecret in appsettings.json" -ForegroundColor White
Write-Host "2. Configure Google Cloud Console with:" -ForegroundColor White
Write-Host "   - Authorized JavaScript origins: https://localhost:7158" -ForegroundColor White
Write-Host "   - Authorized redirect URIs: https://localhost:7158/Auth/GoogleCallback" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to start the application"

# Run the application
dotnet run --urls "https://localhost:7158" 