@echo off
REM WallpaperChanger Uninstallation Script

echo ========================================
echo WallpaperChanger Uninstaller
echo ========================================
echo.

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires administrator privileges.
    echo Please right-click and select "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo This will remove WallpaperChanger from your system.
echo.
echo WARNING: This will delete:
echo - Application files from Program Files
echo - Desktop shortcuts
echo - Start Menu shortcuts
echo - Startup shortcuts
echo.
echo Your settings and image folders will NOT be deleted.
echo Settings location: %APPDATA%\WallpaperChanger
echo.

choice /C YN /M "Are you sure you want to uninstall"
if %errorLevel% neq 1 (
    echo.
    echo Uninstallation cancelled.
    pause
    exit /b 0
)

echo.
echo [1/5] Stopping WallpaperChanger if running...
tasklist /FI "IMAGENAME eq WallpaperChanger.exe" 2>NUL | find /I /N "WallpaperChanger.exe">NUL
if %errorLevel% equ 0 (
    taskkill /F /IM WallpaperChanger.exe >nul 2>&1
    echo Application stopped
    timeout /t 2 /nobreak >nul
) else (
    echo Application is not running
)
echo.

echo [2/5] Removing application files...
set "INSTALL_DIR=%ProgramFiles%\WallpaperChanger"
if exist "%INSTALL_DIR%" (
    rmdir /S /Q "%INSTALL_DIR%"
    echo Application directory removed
) else (
    echo Application directory not found
)
echo.

echo [3/5] Removing desktop shortcut...
if exist "%USERPROFILE%\Desktop\WallpaperChanger.lnk" (
    del /F /Q "%USERPROFILE%\Desktop\WallpaperChanger.lnk"
    echo Desktop shortcut removed
) else (
    echo Desktop shortcut not found
)
echo.

echo [4/5] Removing Start Menu shortcuts...
set "STARTMENU=%ProgramData%\Microsoft\Windows\Start Menu\Programs\WallpaperChanger"
if exist "%STARTMENU%" (
    rmdir /S /Q "%STARTMENU%"
    echo Start Menu shortcuts removed
) else (
    echo Start Menu shortcuts not found
)
echo.

echo [5/5] Removing startup shortcut...
set "STARTUP=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\WallpaperChanger.lnk"
if exist "%STARTUP%" (
    del /F /Q "%STARTUP%"
    echo Startup shortcut removed
) else (
    echo Startup shortcut not found
)
echo.

echo ========================================
echo Uninstallation completed!
echo ========================================
echo.
echo WallpaperChanger has been removed from your system.
echo.
echo Your personal settings are still available at:
echo %APPDATA%\WallpaperChanger
echo.
echo Would you like to delete your settings and data too?
choice /C YN /M "Delete settings"
if %errorLevel% equ 1 (
    if exist "%APPDATA%\WallpaperChanger" (
        rmdir /S /Q "%APPDATA%\WallpaperChanger"
        echo.
        echo Settings deleted successfully.
    )
) else (
    echo.
    echo Settings preserved.
    echo You can manually delete them later from:
    echo %APPDATA%\WallpaperChanger
)

echo.
echo Thank you for using WallpaperChanger!
echo.
pause
