@echo off
echo ====================================
echo    SmartComply Setup Script
echo ====================================
echo.

echo 1. Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

echo.
echo 2. Building the application...
dotnet build
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo 3. Updating database...
dotnet ef database update
if %errorlevel% neq 0 (
    echo ERROR: Database update failed
    pause
    exit /b 1
)

echo.
echo ====================================
echo    Setup completed successfully!
echo ====================================
echo.
echo To run the application:
echo   dotnet run
echo.
echo The application will be available at:
echo   HTTPS: https://localhost:5001
echo   HTTP:  http://localhost:5000
echo.
echo Default login credentials:
echo   Admin:   admin@smartcomply.com / Admin123!
echo   Auditor: auditor@smartcomply.com / Auditor123!
echo   Manager: manager@smartcomply.com / Manager123!
echo.
pause
