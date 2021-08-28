namespace Examples.Net
{
    using System.Threading.Tasks;
    using OpenQA.Selenium;

    public static class WebDriverExtensions
    {
        public static async Task<string> DownloadStringAsync(this IWebDriver webDriver, string url)
        {
            webDriver.Url = url;
            await Task.Delay(WebDriverHelper.DefaultDomWait);
            return webDriver.PageSource;
        }
    }
}