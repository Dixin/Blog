namespace MediaManager.Net;

using System.Collections.ObjectModel;
using Examples.Common;
using Examples.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class WebDriverWrapper(Func<IWebDriver>? webDriverFactory = null, string initialUrl = "") : IDisposable
{
    internal const int NetworkDefaultRetryCount = 5;

    private IWebDriver? webDriver;

    private IWebDriver WebDriver
    {
        get
        {
            if (this.webDriver is null)
            {
                this.webDriver = webDriverFactory?.Invoke() ?? WebDriverHelper.Start();
                if (initialUrl.IsNotNullOrWhiteSpace())
                {
                    this.webDriver.Url = initialUrl;
                }
            }
            
            return this.webDriver;
        }
    }

    public void Restart()
    {
        try
        {
            this.webDriver?.Dispose();
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
        }

        this.webDriver = webDriverFactory?.Invoke() ?? WebDriverHelper.Start();
    }

    public void Dispose()
    {
        try
        {
            this.webDriver?.Dispose();
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
        }
    }

    public string GetString(string url, Action? wait = null, int retryCount = NetworkDefaultRetryCount)
    {
        Exception? lastException = null;
        for (int retry = 0; retry < retryCount; retry++)
        {
            try
            {
                this.WebDriver.Url = url;
                Thread.Sleep(WebDriverHelper.DefaultDomWait);
                wait?.Invoke();
                return this.WebDriver.PageSource;
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                lastException = exception;
                this.Restart();
            }
        }

        throw lastException!;
    }

    public async Task<string> GetStringAsync(string url, Action? wait = null, int retryCount = NetworkDefaultRetryCount, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;
        for (int retry = 0; retry < retryCount; retry++)
        {
            try
            {
                this.WebDriver.Url = url;
                await Task.Delay(WebDriverHelper.DefaultDomWait, cancellationToken);
                wait?.Invoke();
                return this.WebDriver.PageSource;
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                lastException = exception;
                this.Restart();
            }
        }

        throw lastException!;
    }

    public string Url
    {
        get => this.WebDriver.Url;
        set => this.WebDriver.Url = value;
    }

    public string PageSource => this.WebDriver.PageSource;

    public string Title => this.WebDriver.Title;

    public ReadOnlyCollection<IWebElement> FindElements(By by) => this.WebDriver.FindElements(by);

    public WebDriverWait Wait(TimeSpan timeout) => new(this.WebDriver, timeout);
}