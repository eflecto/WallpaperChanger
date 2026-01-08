@echo off
REM Quick Build Script for WallpaperChanger
echo ========================================
echo WallpaperChanger - Quick Build Script
echo ========================================
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 or higher from:
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET Version:
dotnet --version
echo.

echo [1/4] Cleaning previous builds...
dotnet clean >nul 2>&1
echo Done.

echo [2/4] Restoring NuGet packages...
dotnet restore
if %errorLevel% neq 0 (
    echo ERROR: Failed to restore packages!
    pause
    exit /b 1
)

echo [3/4] Building Release version...
dotnet build -c Release
if %errorLevel% neq 0 (
    echo.
    echo ERROR: Build failed!
    echo Check the errors above and fix them.
    echo.
    pause
    exit /b 1
)

echo [4/4] Build successful!
echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Executable location:
echo bin\Release\net8.0-windows\WallpaperChanger.exe
echo.

choice /C YN /M "Would you like to create a standalone EXE (single file)"
if %errorLevel% equ 1 (
    echo.
    echo Creating standalone executable...
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    echo.
    echo Standalone EXE created:
    echo bin\Release\net8.0-windows\win-x64\publish\WallpaperChanger.exe
    echo.
    echo This file can run on any Windows PC without .NET installed!
)

echo.
choice /C YN /M "Open output folder"
if %errorLevel% equ 1 (
    explorer bin\Release\net8.0-windows\
)

echo.
echo To run the application:
echo 1. Navigate to bin\Release\net8.0-windows\
echo 2. Right-click WallpaperChanger.exe
echo 3. Select "Run as administrator"
echo.
pause
