@echo off
REM WallpaperChanger Installation Script
REM This script helps with installation and setup

echo ========================================
echo WallpaperChanger Installation Script
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

echo [1/5] Checking system requirements...
echo.

REM Check Windows version
ver | findstr /i "10\.0\." > nul
if %errorLevel% neq 0 (
    echo WARNING: This application is designed for Windows 10/11
    echo Your system may not be supported.
    echo.
)

REM Check for .NET Runtime
echo [2/5] Checking for .NET 8.0 Runtime...
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET 8.0 Runtime not found!
    echo.
    echo Please install .NET 8.0 Runtime from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo After installation, run this script again.
    pause
    exit /b 1
) else (
    echo .NET Runtime found: 
    dotnet --version
    echo.
)

echo [3/5] Creating application directory...
set "INSTALL_DIR=%ProgramFiles%\WallpaperChanger"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
echo Installation directory: %INSTALL_DIR%
echo.

echo [4/5] Copying files...
if exist "WallpaperChanger.exe" (
    copy /Y "WallpaperChanger.exe" "%INSTALL_DIR%\" >nul
    echo WallpaperChanger.exe copied successfully
) else (
    echo ERROR: WallpaperChanger.exe not found in current directory
    echo Please make sure the executable is in the same folder as this script.
    pause
    exit /b 1
)

REM Copy additional files if they exist
if exist "README.md" copy /Y "README.md" "%INSTALL_DIR%\" >nul
if exist "LICENSE" copy /Y "LICENSE" "%INSTALL_DIR%\" >nul
echo.

echo [5/5] Creating shortcuts...

REM Create desktop shortcut
set "SHORTCUT=%USERPROFILE%\Desktop\WallpaperChanger.lnk"
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%SHORTCUT%'); $Shortcut.TargetPath = '%INSTALL_DIR%\WallpaperChanger.exe'; $Shortcut.Save()"
echo Desktop shortcut created
echo.

REM Create Start Menu shortcut
set "STARTMENU=%ProgramData%\Microsoft\Windows\Start Menu\Programs"
if not exist "%STARTMENU%\WallpaperChanger" mkdir "%STARTMENU%\WallpaperChanger"
set "STARTSHORTCUT=%STARTMENU%\WallpaperChanger\WallpaperChanger.lnk"
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTSHORTCUT%'); $Shortcut.TargetPath = '%INSTALL_DIR%\WallpaperChanger.exe'; $Shortcut.Save()"
echo Start Menu shortcut created
echo.

echo ========================================
echo Installation completed successfully!
echo ========================================
echo.
echo Installation location: %INSTALL_DIR%
echo Desktop shortcut: Created
echo Start Menu: Created
echo.
echo IMPORTANT:
echo - WallpaperChanger requires administrator privileges
echo - Right-click the shortcut and select "Run as administrator"
echo.
echo Optional: Add to startup
echo Would you like to add WallpaperChanger to Windows startup?
echo (It will start automatically when you log in)
echo.

choice /C YN /M "Add to startup"
if %errorLevel% equ 1 (
    set "STARTUP=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"
    powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTUP%\WallpaperChanger.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\WallpaperChanger.exe'; $Shortcut.Save()"
    echo.
    echo Startup shortcut created!
    echo WallpaperChanger will start automatically on login.
) else (
    echo.
    echo Skipped startup configuration.
    echo You can add it later by copying the shortcut to:
    echo %APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
)

echo.
echo Would you like to launch WallpaperChanger now?
choice /C YN /M "Launch now"
if %errorLevel% equ 1 (
    start "" "%INSTALL_DIR%\WallpaperChanger.exe"
)

echo.
echo Thank you for installing WallpaperChanger!
echo For help and documentation, visit the installation folder.
echo.
pause
