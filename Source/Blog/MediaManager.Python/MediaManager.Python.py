import undetected_chromedriver as uc
if __name__ ==  '__main__':
    driver = uc.Chrome(headless=False,use_subprocess=False)
    driver.get('https://nowsecure.nl')
    driver.save_screenshot('nowsecure.png')