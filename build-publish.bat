@echo off
setlocal

echo ==========================================
echo GuardianShield - Local Build
echo ==========================================
echo.

dotnet restore GuardianShield.csproj

if %errorlevel% neq 0 (
    echo Restore failed.
    exit /b %errorlevel%
)

dotnet publish GuardianShield.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -o publish

if %errorlevel% neq 0 (
    echo Publish failed.
    exit /b %errorlevel%
)

echo.
echo ==========================================
echo Build completed successfully.
echo ==========================================
echo.
echo Published files:
echo %cd%\publish
echo.
echo To build the installer with Inno Setup:
echo.
echo ISCC.exe installer\GuardianShield.iss
echo.

pause
