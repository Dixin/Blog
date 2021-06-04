namespace Examples.Net
{
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    internal static class ChromeDriverHelper
    {
        internal static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(100);

        internal static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

        internal static IWebDriver Start(string profile = @"D:\Temp\Chrome Profile")
        {
            ChromeOptions options = new();
            options.AddArguments($"user-data-dir={profile}");
            ChromeDriver webDriver = new(options);
            return webDriver;
        }
    }
}