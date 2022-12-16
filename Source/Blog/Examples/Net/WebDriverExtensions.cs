namespace Examples.Net;

using OpenQA.Selenium;

public static class WebDriverExtensions
{
    public static async Task<string> GetStringAsync(this IWebDriver webDriver, string url, Action? wait = null)
    {
        webDriver.Url = url;
        await Task.Delay(WebDriverHelper.DefaultDomWait);
        wait?.Invoke();
        return webDriver.PageSource;
    }
}