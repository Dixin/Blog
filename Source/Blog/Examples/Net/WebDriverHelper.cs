﻿namespace Examples.Net
{
    using System;
    using System.IO;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Edge;

    public static class WebDriverHelper
    {
        public static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(200);

        public static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

        private const string TempDirectory = @"D:\Temp";

        private const string ProfilePrefix = "Selenium Profile";

        public static IWebDriver StartChrome(string profile = "")
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

        public static IWebDriver StartChrome(int index) => StartChrome(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(ChromeDriver)} {index:00}"));

        public static IWebDriver StartEdge(string profile = "")
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

        public static IWebDriver StartEdge(int index) => StartEdge(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"));
    }
}