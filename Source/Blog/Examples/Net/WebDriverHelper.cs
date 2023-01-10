namespace Examples.Net;

using Examples.Common;
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

    public static IWebDriver StartEdge(string profile = "", bool isLoadingAll = false, string downloadDirectory = "")
    {
        if (string.IsNullOrWhiteSpace(profile))
        {
            profile = Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)}");
        }

        EdgeOptions options = new();
        options.AddArguments($"user-data-dir={profile}");
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

        EdgeDriver webDriver = new(options);
        return webDriver;
    }

    public static IWebDriver StartEdge(int index, bool isLoadingAll = false) => 
        StartEdge(Path.Combine(TempDirectory, $"{ProfilePrefix} {nameof(EdgeDriver)} {index:00}"), isLoadingAll);
}