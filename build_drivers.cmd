@echo off
REM Build script for Zuum H10X10-4K6G Matrix Switcher Drivers
REM This script builds the drivers using MSBuild without requiring Visual Studio IDE

echo ======================================
echo Building Zuum H10X10-4K6G Drivers
echo ======================================
echo.

REM Set MSBuild path (adjust if needed for your VS installation)
set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist %MSBUILD_PATH% (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
)
if not exist %MSBUILD_PATH% (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)
if not exist %MSBUILD_PATH% (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
)

echo Using MSBuild: %MSBUILD_PATH%
echo.

REM Build Serial Driver
echo Building Serial Driver...
%MSBUILD_PATH% "MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj" /p:Configuration=Release /t:Rebuild
if errorlevel 1 (
    echo ERROR: Serial driver build failed!
    goto :error
)
echo Serial driver built successfully.
echo.

REM Build IP Driver
echo Building IP Driver...
%MSBUILD_PATH% "MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP\MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.csproj" /p:Configuration=Release /t:Rebuild
if errorlevel 1 (
    echo ERROR: IP driver build failed!
    goto :error
)
echo IP driver built successfully.
echo.

REM Package Serial Driver
echo Packaging Serial Driver...
SDK\ManifestUtil\ManifestUtil.exe "MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.dll"
if errorlevel 1 (
    echo ERROR: Serial driver packaging failed!
    goto :error
)
echo Serial driver packaged successfully.
echo.

REM Package IP Driver
echo Packaging IP Driver...
SDK\ManifestUtil\ManifestUtil.exe "MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.dll"
if errorlevel 1 (
    echo ERROR: IP driver packaging failed!
    goto :error
)
echo IP driver packaged successfully.
echo.

echo ======================================
echo Build Complete!
echo ======================================
echo.
echo Output files:
echo   - MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.pkg
echo   - MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.pkg
echo.
pause
exit /b 0

:error
echo.
echo ======================================
echo Build Failed!
echo ======================================
echo.
pause
exit /b 1
