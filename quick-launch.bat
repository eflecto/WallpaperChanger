@echo off
REM One-Click Build and Launch Script
REM This creates a standalone EXE and runs it as administrator

echo ========================================
echo WallpaperChanger - Quick Launch
echo ========================================
echo.
echo This script will:
echo 1. Build standalone version
echo 2. Launch as administrator
echo.
pause

cd /d "%~dp0"

echo [1/3] Building standalone version...
echo This may take a minute...
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

if %errorLevel% neq 0 (
    echo.
    echo Build FAILED!
    echo.
    pause
    exit /b 1
)

echo.
echo [2/3] Build successful!
echo.

set "STANDALONE_EXE=bin\Release\net8.0-windows\win-x64\publish\WallpaperChanger.exe"

if not exist "%STANDALONE_EXE%" (
    echo ERROR: Standalone EXE not found!
    pause
    exit /b 1
)

echo [3/3] Launching as Administrator...
echo.
echo NOTE: Click "Yes" in the UAC prompt
echo.

timeout /t 2 /nobreak >nul

powershell -Command "Start-Process '%STANDALONE_EXE%' -Verb RunAs"

echo.
echo ========================================
echo Application should now be running!
echo ========================================
echo.
echo If the window doesn't appear:
echo - Check if it's behind other windows (Alt+Tab)
echo - Look in the taskbar
echo - Check antivirus settings
echo.
pause
