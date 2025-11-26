@echo off
echo ========================================
echo Crestron SDK Documentation Scraper Setup
echo ========================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH.
    echo Please install Python from https://www.python.org/downloads/
    echo Make sure to check "Add Python to PATH" during installation.
    pause
    exit /b 1
)

echo Python found. Installing required packages...
echo.

REM Install playwright
pip install playwright

REM Install browser
echo.
echo Installing Chromium browser for automation...
playwright install chromium

echo.
echo ========================================
echo Setup complete! Starting the scraper...
echo ========================================
echo.

REM Run the scraper
python scrape_crestron_docs.py

echo.
pause
