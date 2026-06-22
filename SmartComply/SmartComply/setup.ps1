#!/usr/bin/env pwsh

Write-Host "====================================" -ForegroundColor Green
Write-Host "    SmartComply Setup Script" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

Write-Host "1. Restoring NuGet packages..." -ForegroundColor Yellow
try {
    dotnet restore
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Failed to restore packages" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "2. Building the application..." -ForegroundColor Yellow
try {
    dotnet build
    Write-Host "✓ Build completed successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "3. Updating database..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "✓ Database updated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Database update failed" -ForegroundColor Red
    Write-Host "Note: Make sure Entity Framework tools are installed:" -ForegroundColor Yellow
    Write-Host "  dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "    Setup completed successfully!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "The application will be available at:" -ForegroundColor Cyan
Write-Host "  HTTPS: https://localhost:5001" -ForegroundColor White
Write-Host "  HTTP:  http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "Default login credentials:" -ForegroundColor Cyan
Write-Host "  Admin:   admin@smartcomply.com / Admin123!" -ForegroundColor White
Write-Host "  Auditor: auditor@smartcomply.com / Auditor123!" -ForegroundColor White
Write-Host "  Manager: manager@smartcomply.com / Manager123!" -ForegroundColor White
Write-Host ""

if ($IsWindows) {
    Read-Host "Press Enter to continue"
}
