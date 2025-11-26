#!/usr/bin/env python3
"""
Crestron SDK Documentation Scraper

This script opens a browser, lets you log in manually, then crawls
and saves all documentation pages as HTML files.

Requirements:
    pip install playwright
    playwright install chromium

Usage:
    python scrape_crestron_docs.py
"""

import os
import re
import time
from urllib.parse import urljoin, urlparse
from playwright.sync_api import sync_playwright

# Configuration
START_URL = "https://sdkcon78221.crestron.com/sdk/Crestron_Certified_Drivers_SDK/Content/Topics/Home.htm"
OUTPUT_DIR = "crestron_docs"
BASE_DOMAIN = "sdkcon78221.crestron.com"

def sanitize_filename(url):
    """Convert URL to a safe filename."""
    parsed = urlparse(url)
    path = parsed.path.replace("/", "_").replace("\\", "_")
    if not path or path == "_":
        path = "index"
    # Remove leading underscore
    if path.startswith("_"):
        path = path[1:]
    # Ensure .html extension
    if not path.endswith(".html") and not path.endswith(".htm"):
        path += ".html"
    # Sanitize
    path = re.sub(r'[<>:"|?*]', '_', path)
    return path

def main():
    # Create output directory
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    visited_urls = set()
    urls_to_visit = [START_URL]

    with sync_playwright() as p:
        # Launch browser in headed mode so user can log in
        browser = p.chromium.launch(headless=False)
        context = browser.new_context()
        page = context.new_page()

        # Navigate to the starting URL
        print(f"Navigating to: {START_URL}")
        print("\n" + "="*60)
        print("PLEASE LOG IN TO THE WEBSITE IN THE BROWSER WINDOW")
        print("Once logged in and you see the documentation, press Enter here...")
        print("="*60 + "\n")

        page.goto(START_URL, wait_until="networkidle")

        # Wait for user to log in
        input("Press Enter after you've logged in and can see the documentation...")

        # Give the page a moment to fully load after login
        time.sleep(2)

        # Now start crawling
        pages_saved = 0

        while urls_to_visit:
            current_url = urls_to_visit.pop(0)

            if current_url in visited_urls:
                continue

            # Only process URLs from the same domain and SDK path
            parsed = urlparse(current_url)
            if parsed.netloc != BASE_DOMAIN:
                continue
            if "Crestron_Certified_Drivers_SDK" not in current_url:
                continue

            visited_urls.add(current_url)

            try:
                print(f"Fetching: {current_url}")
                page.goto(current_url, wait_until="networkidle", timeout=30000)
                time.sleep(0.5)  # Small delay to be respectful

                # Get page content
                content = page.content()

                # Save the page
                filename = sanitize_filename(current_url)
                filepath = os.path.join(OUTPUT_DIR, filename)

                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)

                pages_saved += 1
                print(f"  Saved: {filename} ({pages_saved} pages total)")

                # Find all links on the page
                links = page.evaluate("""
                    () => {
                        const links = [];
                        document.querySelectorAll('a[href]').forEach(a => {
                            links.push(a.href);
                        });
                        return links;
                    }
                """)

                # Add new links to visit
                for link in links:
                    if link not in visited_urls and link not in urls_to_visit:
                        # Filter to only SDK documentation pages
                        if BASE_DOMAIN in link and "Crestron_Certified_Drivers_SDK" in link:
                            # Skip anchors, javascript, etc.
                            if not link.startswith("javascript:") and "#" not in link:
                                urls_to_visit.append(link)

            except Exception as e:
                print(f"  Error fetching {current_url}: {e}")
                continue

        browser.close()

    print(f"\n{'='*60}")
    print(f"COMPLETE! Saved {pages_saved} pages to '{OUTPUT_DIR}/' folder")
    print(f"{'='*60}")

    # Create an index file
    index_path = os.path.join(OUTPUT_DIR, "_index.txt")
    with open(index_path, 'w', encoding='utf-8') as f:
        f.write("Saved Documentation Pages:\n")
        f.write("="*40 + "\n\n")
        for url in sorted(visited_urls):
            filename = sanitize_filename(url)
            f.write(f"{filename}\n  -> {url}\n\n")

    print(f"Index saved to: {index_path}")

if __name__ == "__main__":
    main()
