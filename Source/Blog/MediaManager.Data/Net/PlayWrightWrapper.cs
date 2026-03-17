namespace MediaManager.Net;

using Examples.Common;
using Microsoft.Playwright;

internal class PlayWrightWrapper : IAsyncDisposable
{
    private readonly string initialUrl;

    private readonly string cookieFile;

    private IPlaywright? playwright;

    private IBrowser? browser;

    private IBrowserContext? context;

    private IPage? page;

    internal PlayWrightWrapper(string initialUrl = "", string cookieFile = "")
    {
        this.initialUrl = initialUrl;
        this.cookieFile = cookieFile;
    }

    internal ValueTask<IPage> PageAsync() => this.page == null ? this.InitializeAsync(this.initialUrl) : ValueTask.FromResult(this.page);

    private async ValueTask<IPage> InitializeAsync(string url = "")
    {
        this.playwright = await Playwright.CreateAsync();
        this.browser = await this.playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions() { Headless = false });
        this.context = await this.browser.NewContextAsync();
        if (this.cookieFile.IsNotNullOrWhiteSpace())
        {
            Cookie[] cookies = await JsonHelper.DeserializeFromFileAsync<Cookie[]>(this.cookieFile);
            await this.context.AddCookiesAsync(cookies);
        }

        this.page = await this.context.NewPageAsync();
        await this.page.AbortMediaAsync();
        if (this.initialUrl.IsNotNullOrWhiteSpace())
        {
            IResponse? response = await this.page.GotoAsync(url);
            Debug.Assert(response is not null && response.Ok);
        }

        return this.page;
    }

    public async ValueTask DisposeAsync()
    {
        try { }
        finally
        {
            if (this.page is not null)
            {
                await this.page.CloseAsync();
            }
        }

        try { }
        finally
        {
            if (this.context is not null)
            {
                await this.context.DisposeAsync();
            }
        }

        try { }
        finally
        {
            if (this.browser is not null)
            {
                await this.browser.DisposeAsync();
            }
        }

        try { }
        finally
        {
            this.playwright?.Dispose();
        }
    }

    internal async Task<IPage> RestartAsync(TimeSpan? wait = null, string initialUrl = "", CancellationToken cancellationToken = default)
    {
        await this.DisposeAsync();
        await Task.Delay(wait ??= PageHelper.DefaultErrorWait, cancellationToken);
        return await this.InitializeAsync(initialUrl);
    }

    internal async Task<string> GetUrlAsync() => (await this.PageAsync()).Url;
}