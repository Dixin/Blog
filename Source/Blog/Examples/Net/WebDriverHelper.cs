namespace Examples.Net
{
    using System;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    internal static class WebDriverHelper
    {
        internal static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(200);

        internal static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

        internal static IWebDriver Start(string profile = @"D:\Temp\Chrome Profile")
        {
            ChromeOptions options = new();
            options.AddArguments($"user-data-dir={profile}");
            ChromeDriver webDriver = new(options);
            return webDriver;
        }

        internal static IWebDriver Start(int index) => Start(@$"D:\Temp\Chrome Profile {index}");
    }

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