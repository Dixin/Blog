namespace Examples.Net
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Edge;

    internal static class WebDriverHelper
    {
        internal static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(200);

        internal static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

        private const string TempDirectory = @"D:\Temp";

        private const string ProfilePrefix = "Selenium Profile";

        internal static IWebDriver StartChrome(string profile = "")
        {
            if (string.IsNullOrWhiteSpace(profile))
            {
                profile = Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(ChromeDriver)}");
            }

            ChromeOptions options = new();
            options.AddArguments($"user-data-dir={profile}");
            ChromeDriver webDriver = new(options);
            return webDriver;
        }

        internal static IWebDriver StartChrome(int index) => StartChrome(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(ChromeDriver)} {index:00}"));

        internal static IWebDriver StartEdge(string profile = "")
        {
            if (string.IsNullOrWhiteSpace(profile))
            {
                profile = Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)}");
            }

            EdgeOptions options = new();
            options.AddArguments($"user-data-dir={profile}");
            EdgeDriver webDriver = new(options);
            return webDriver;
        }

        internal static IWebDriver StartEdge(int index) => StartEdge(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"));
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