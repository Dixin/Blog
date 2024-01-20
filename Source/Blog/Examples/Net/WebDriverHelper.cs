namespace Examples.Net;

using System.Runtime.Versioning;
using Examples.Common;
using Examples.Diagnostics;
using Examples.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

public static class WebDriverHelper
{
    public static readonly TimeSpan DefaultManualWait = TimeSpan.FromSeconds(200);

    public static readonly TimeSpan DefaultNetworkWait = TimeSpan.FromSeconds(1);

    public static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

    private const string TempDirectory = @"D:\Temp";

    private const string ProfilePrefix = "Selenium Profile";

    private const bool IsEdgeDefault = true;

    public static IWebDriver Start(int index, bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "") =>
        IsEdgeDefault
         ? StartChromium<EdgeOptions>(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"), isLoadingAll, keepWindow, keepExisting, cleanProfile, downloadDirectory)
         : StartChromium<ChromeOptions>(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"), isLoadingAll, keepWindow, keepExisting, cleanProfile, downloadDirectory);

    public static IWebDriver Start(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "") =>
        IsEdgeDefault
            ? StartChromium<EdgeOptions>(profile, isLoadingAll, keepWindow, keepExisting, cleanProfile, downloadDirectory)
            : StartChromium<ChromeOptions>(profile, isLoadingAll, keepWindow, keepExisting, cleanProfile, downloadDirectory);

    private static IWebDriver StartChromium<TOptions>(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "")
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
            options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.cookies", 2);
            //options.AddUserProfilePreference("profile.managed_default_content_settings.javascript", 1);
            //options.AddUserProfilePreference("profile.managed_default_content_settings.plugins", 1);
            options.AddUserProfilePreference("profile.managed_default_content_settings.popups", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.geolocation", 2);
            options.AddUserProfilePreference("profile.managed_default_content_settings.media_stream", 2);

            """
            cookies
            images
            plugins
            popups
            geolocation
            notifications
            auto_select_certificate
            fullscreen
            mouselock
            media_stream
            media_stream_mic
            media_stream_camera
            protocol_handlers
            ppapi_broker
            automatic_downloads
            midi_sysex
            push_messaging
            ssl_cert_decisions
            metro_switch_to_desktop
            protected_media_identifier
            app_banner
            site_engagement
            durable_storage
            """
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ForEach(option => options.AddUserProfilePreference($"profile.default_content_setting_values.{option}", 2));
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

    public static IWebDriver StartFirefox(string profile = "", bool isLoadingAll = false, bool keepWindow = false, bool keepExisting = false, bool cleanProfile = false, string downloadDirectory = "")
    {
        FirefoxOptions options = new();
        //options.AddUserProfilePreference("download.default_directory", downloadDirectory.IfNullOrWhiteSpace(TempDirectory));
        //options.AddUserProfilePreference("disable-popup-blocking", "true");
        //options.AddUserProfilePreference("download.prompt_for_download", "false");
        //options.AddUserProfilePreference("download.directory_upgrade", "true");
        if (isLoadingAll)
        {
            //options.AddUserProfilePreference("profile.default_content_setting_values.cookies", 1);
            //options.AddUserProfilePreference("profile.cookie_controls_mode", 0);
        }
        else
        {
            options.AddAdditionalFirefoxOption("permissions.default.stylesheet", 2);
            options.AddAdditionalFirefoxOption("permissions.default.image", 2);
            options.AddAdditionalFirefoxOption("dom.ipc.plugins.enabled.libflashplayer.so", "false");
        }

        if (!keepExisting)
        {
            DisposeAllFirefox();
        }

        if (profile.IsNullOrWhiteSpace())
        {
            profile = Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(FirefoxDriver)}");
        }

        if (cleanProfile && Directory.Exists(profile))
        {
            DirectoryHelper.Recycle(profile);
        }

        options.AddArguments($"user-data-dir={profile}");
        FirefoxDriver webDriver = new(options);

        if (!keepWindow)
        {
            webDriver.Manage().Window.Minimize();
        }

        return webDriver;
    }

    public static void DisposeAll()
    {
        if (IsEdgeDefault)
        {
            DisposeAllEdge();

        }
        else
        {
            DisposeAllChrome();
        }
    }

    [SupportedOSPlatform("windows")]
    private static void DisposeAllEdge()
    {
        ProcessHelper.TryKillAll("msedgedriver.exe");
        Win32ProcessHelper.TryKillAll("msedge.exe", " --test-type=webdriver ");
    }

    private static void DisposeAllChrome()
    {
        ProcessHelper.TryKillAll("chromedriver.exe");
        ProcessHelper.TryKillAll("chrome.exe");
    }

    public static void DisposeAllFirefox()
    {
        ProcessHelper.TryKillAll("firefoxdriver.exe");
        ProcessHelper.TryKillAll("firefox.exe");
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
                catch (Exception innerException) when (innerException.IsNotCritical())
                {
                }

                webDriver = restart?.Invoke() ?? Start();
            }
        }

        throw lastException!;
    }
}