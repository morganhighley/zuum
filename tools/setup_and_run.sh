#!/bin/bash

echo "========================================"
echo "Crestron SDK Documentation Scraper Setup"
echo "========================================"
echo ""

# Check if Python 3 is installed
if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python 3 is not installed."
    echo "Install it with: brew install python3"
    echo "Or download from: https://www.python.org/downloads/"
    exit 1
fi

echo "Python 3 found: $(python3 --version)"
echo ""

# Create virtual environment (optional but recommended)
if [ ! -d "venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv venv
fi

# Activate virtual environment
source venv/bin/activate

# Install playwright
echo "Installing Playwright..."
pip install playwright

# Install browser
echo ""
echo "Installing Chromium browser for automation..."
playwright install chromium

echo ""
echo "========================================"
echo "Setup complete! Starting the scraper..."
echo "========================================"
echo ""

# Run the scraper
python3 scrape_crestron_docs.py

echo ""
echo "Done! Check the 'crestron_docs' folder for saved pages."
