import os
import logging

from joblib import Parallel, delayed
from selenium.webdriver.support.wait import D
import undetected_chromedriver as uc
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC


def download_summary(page, drivers, wait_second):
    driver_index = page % len(drivers)
    driver = drivers[driver_index]
    if driver is None:
        # options = webdriver.ChromeOptions()
        # options.add_argument(r"--user-data-dir=D:\Temp")
        # options.add_argument(f"--profile-directory=Profile {index}")
        driver = uc.Chrome(
            headless=False,
            use_subprocess=False,
            user_data_dir=f"D:\\Temp\\UndetectedProfile{driver_index}",
            driver_executable_path=f"D:\\Temp\\UndetectedExecutable{driver_index}\\undetected\\chromedriver-win32\\chromedriver.exe",
        )
        drivers[driver_index] = driver

    url = f"https://yts.bz/browse-movies?page={page}"
    logging.info(f"Downloading summary page {page} with URL: {url}")
    driver.get(url)
    page_link_elements = WebDriverWait(driver, wait_second).until(
        EC.presence_of_all_elements_located((By.CSS_SELECTOR, ".browse-movie-link"))
    )
    page_urls = [element.get_attribute("href") for element in page_link_elements]
    html = driver.page_source
    return (driver_index, html, page_urls)


def download_movie(index, url, drivers, wait_second):
    driver_index = index % len(drivers)
    driver = drivers[driver_index]
    if driver is None:
        driver = uc.Chrome(
            headless=False,
            use_subprocess=False,
            user_data_dir=f"D:\\Temp\\UndetectedProfile{driver_index}",
            driver_executable_path=f"D:\\Temp\\UndetectedExecutable{driver_index}\\undetected\\chromedriver-win32\\chromedriver.exe",
        )
        drivers[driver_index] = driver

    logging.info(f"Downloading movie page {index} with URL: {url}")
    driver.get(url)
    search_element = WebDriverWait(driver, wait_second).until(
        EC.presence_of_element_located((By.CSS_SELECTOR, "#mobile-search-input"))
    )
    html = driver.page_source
    name = url.split("/")[-1]
    return (name, html)


if __name__ == "__main__":
    directory = r"D:\Files\Preferred"
    parallelism = 6
    drivers = [None] * parallelism
    wait_second = 30
    all_urls = []
    last_page = 4

    summary_results = Parallel(n_jobs=parallelism, backend="threading")(
        delayed(download_summary)(index, drivers, wait_second)
        for index in range(1, last_page + 1)
    )

    for index, html, page_urls in summary_results:
        all_urls.extend(page_urls)

        file_path = path = os.path.join(directory, f"Preferred.Summary.{index}.log")
        with open(file_path, "w", encoding="utf-8") as file:
            file.write(html)

    movie_results = Parallel(n_jobs=parallelism, backend="threading")(
        delayed(download_movie)(index, link, drivers, wait_second)
        for index, link in enumerate(all_urls)
    )

    for name, html in movie_results:
        file_path = os.path.join(directory, f"Preferred.Movie.{name}.log")
        with open(file_path, "w", encoding="utf-8") as file:
            file.write(html)

    for driver in drivers:
        if driver is not None:
            driver.quit()