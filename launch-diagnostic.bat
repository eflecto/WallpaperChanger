@echo off
REM Diagnostic and Launch Script for WallpaperChanger
echo ========================================
echo WallpaperChanger - Diagnostic Launch
echo ========================================
echo.

set "EXE_PATH=bin\Release\net8.0-windows\WallpaperChanger.exe"

REM Check if executable exists
if not exist "%EXE_PATH%" (
    echo ERROR: WallpaperChanger.exe not found!
    echo Expected location: %EXE_PATH%
    echo.
    echo Please build the project first:
    echo   dotnet build -c Release
    echo.
    pause
    exit /b 1
)

echo [✓] Executable found: %EXE_PATH%
echo.

REM Check .NET Runtime
echo Checking .NET Runtime...
dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App"
if %errorLevel% neq 0 (
    echo.
    echo WARNING: Windows Desktop Runtime not found!
    echo Installing...
    echo.
)
echo.

echo ========================================
echo Launching WallpaperChanger...
echo ========================================
echo.
echo If the window doesn't appear, check:
echo 1. Windows Event Viewer for errors
echo 2. Try running as administrator
echo 3. Check antivirus settings
echo.

REM Try to launch normally first
echo [Attempt 1] Launching normally...
start "" "%EXE_PATH%"
timeout /t 3 /nobreak >nul

REM Check if process started
tasklist /FI "IMAGENAME eq WallpaperChanger.exe" 2>NUL | find /I /N "WallpaperChanger.exe">NUL
if %errorLevel% equ 0 (
    echo [✓] Application is running!
    echo.
    echo If you don't see the window, it might be behind other windows.
    echo Press Alt+Tab to find it.
    echo.
    pause
    exit /b 0
) else (
    echo [✗] Application did not start or crashed immediately.
    echo.
    echo [Attempt 2] Trying with error logging...
    echo.
    
    REM Create error log
    echo Running with error capture...
    "%EXE_PATH%" 2> error_log.txt
    
    if exist error_log.txt (
        echo.
        echo Error log created. Contents:
        echo ----------------------------------------
        type error_log.txt
        echo ----------------------------------------
        echo.
    )
    
    echo [Attempt 3] Trying as Administrator...
    echo.
    echo NOTE: A UAC prompt will appear. Click "Yes"
    timeout /t 2 /nobreak >nul
    
    powershell -Command "Start-Process '%EXE_PATH%' -Verb RunAs"
    
    timeout /t 3 /nobreak >nul
    
    tasklist /FI "IMAGENAME eq WallpaperChanger.exe" 2>NUL | find /I /N "WallpaperChanger.exe">NUL
    if %errorLevel% equ 0 (
        echo [✓] Application started as administrator!
    ) else (
        echo [✗] Application still did not start.
        echo.
        echo Troubleshooting steps:
        echo 1. Check Windows Event Viewer
        echo    - Open Event Viewer
        echo    - Windows Logs ^> Application
        echo    - Look for .NET Runtime errors
        echo.
        echo 2. Install .NET 8.0 Desktop Runtime:
        echo    https://dotnet.microsoft.com/download/dotnet/8.0
        echo.
        echo 3. Rebuild in Debug mode:
        echo    dotnet build -c Debug
        echo    Then run: bin\Debug\net8.0-windows\WallpaperChanger.exe
        echo.
        echo 4. Check antivirus/firewall settings
        echo.
    )
)

pause
