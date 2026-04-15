import os

import undetected_chromedriver as uc
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

if __name__ == "__main__":
    directory = r"D:\Files\Preferred"
    # options = webdriver.ChromeOptions()
    # options.add_experimental_option("prefs", {"profile.managed_default_content_settings.images": 2})
    # options.add_argument('--blink-settings=imagesEnabled=false')
    driver = uc.Chrome(headless=False, use_subprocess=False)
    wait_second = 30
    all_urls = []

    for index in range(1, 200):
        driver.get(f"https://yts.bz/browse-movies?page={index}")
        page_link_elements = WebDriverWait(driver, wait_second).until(
            EC.presence_of_all_elements_located((By.CSS_SELECTOR, ".browse-movie-link"))
        )
        page_urls = [element.get_attribute("href") for element in page_link_elements]
        all_urls.extend(page_urls)

        html = driver.page_source
        file_path = path = os.path.join(directory, f"Preferred.Summary.{index}.log")
        with open(file_path, "w", encoding="utf-8") as file:
            file.write(html)

    for url in all_urls:
        driver.get(url)
        element = WebDriverWait(driver, wait_second).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, "#mobile-search-input"))
        )
        html = driver.page_source
        name = url.split("/")[-1]
        file_path = os.path.join(directory, f"Preferred.Movie.{name}.log")
        with open(file_path, "w", encoding="utf-8") as file:
            file.write(html)

    driver.quit()