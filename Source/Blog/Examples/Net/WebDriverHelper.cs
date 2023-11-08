namespace Examples.Net;

using Examples.Common;
using Examples.Diagnostics;
using Examples.IO;
using Examples.Management;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;

public static class WebDriverHelper
{
    public static readonly TimeSpan DefaultManualWait = TimeSpan.FromSeconds(200);

    public static readonly TimeSpan DefaultNetworkWait = TimeSpan.FromSeconds(1);

    public static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

    private const string TempDirectory = @"D:\Temp";

    private const string ProfilePrefix = "Selenium Profile";

    private static readonly List<Win32Process> childProcesses = new();

    //public static IWebDriver StartChrome(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "") =>
    //    StartChromium<ChromeOptions>(profile, isLoadingAll, keepWindow, keepExisting, cleanProfile, downloadDirectory);

    //public static IWebDriver StartChrome(int index, bool isLoadingAll = false) =>
    //    StartChrome(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(ChromeDriver)} {index:00}"), isLoadingAll);

    //public static IWebDriver StartEdge(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "") =>
    //    StartChromium<EdgeOptions>(profile, isLoadingAll, keepWindow, keepExisting, cleanProfile, downloadDirectory);

    //public static IWebDriver StartEdge(int index, bool isLoadingAll = false) =>
    //    StartEdge(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"), isLoadingAll);

    public static IWebDriver Start(int index, bool isLoadingAll = false, bool keepExisting = false) =>
        Start(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"), isLoadingAll, keepExisting: keepExisting);

    public static IWebDriver Start(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool keepProfile = false, string downloadDirectory = "") =>
        StartChromium<ChromeOptions>(profile, isLoadingAll, keepWindow, keepExisting, keepProfile, downloadDirectory);

    public static IWebDriver StartChromium<TOptions>(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "")
        where TOptions : ChromiumOptions, new()
    {
        TOptions options = new();
        options.AddUserProfilePreference("download.default_directory", downloadDirectory.IfNullOrWhiteSpace(TempDirectory));
        options.AddUserProfilePreference("disable-popup-blocking", "true");
        options.AddUserProfilePreference("download.prompt_for_download", "false");
        options.AddUserProfilePreference("download.directory_upgrade", "true");
        if (isLoadingAll)
        {
            options.AddUserProfilePreference("profile.default_content_setting_values.cookies", 1);
            options.AddUserProfilePreference("profile.cookie_controls_mode", 0);
        }
        else
        {
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.cookies", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.javascript", 1);
            options.AddUserProfilePreference("profile.managed_default_content_settings.plugins", 1);
            options.AddUserProfilePreference("profile.managed_default_content_settings.popups", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.geolocation", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.media_stream", 2);
        }

        ChromiumDriver webDriver;
        switch (options)
        {
            case EdgeOptions edgeOptions:
                if (!keepExisting)
                {
                    DisposeAllEdge();
                }

                if (profile.IsNullOrWhiteSpace())
                {
                    profile = Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)}");
                }

                if (cleanProfile && Directory.Exists(profile))
                {
                    DirectoryHelper.Recycle(profile);
                }

                options.AddArguments($"user-data-dir={profile}");
                webDriver = new EdgeDriver(edgeOptions);

                break;

            case ChromeOptions chromeOptions:
                if (!keepExisting)
                {
                    DisposeAllChrome();
                }

                if (profile.IsNullOrWhiteSpace())
                {
                    profile = Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(ChromeDriver)}");
                }

                if (cleanProfile && Directory.Exists(profile))
                {
                    DirectoryHelper.Recycle(profile);
                }

                options.AddArguments($"user-data-dir={profile}");
                webDriver = new ChromeDriver(chromeOptions);
                break;

            default:
                throw new NotSupportedException(options.GetType().FullName);
        }

        if (!keepWindow)
        {
            webDriver.Manage().Window.Minimize();
        }

        return webDriver;
    }

    public static void DisposeAllEdge()
    {
        ProcessHelper.TryKillAll("msedgedriver.exe");
        Win32ProcessHelper.TryKillAll("msedge.exe", " --test-type=webdriver ");
    }

    public static void DisposeAllChrome()
    {
        ProcessHelper.TryKillAll("chromedriver.exe");
        ProcessHelper.TryKillAll("chrome.exe");
    }

    public static string GetString(ref IWebDriver webDriver, string url, int retryCount = 10, Func<IWebDriver>? restart = null, Action? wait = null)
    {
        webDriver.NotNull();

        Exception? lastException = null;
        for (int retry = 0; retry < retryCount; retry++)
        {
            try
            {
                webDriver.Url = url;
                Thread.Sleep(DefaultDomWait);
                wait?.Invoke();
                return webDriver.PageSource;
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                lastException = exception;
                try
                {
                    webDriver.Dispose();
                }
                finally
                {
                    webDriver = restart?.Invoke() ?? Start();
                }
            }
        }

        throw lastException!;
    }
}