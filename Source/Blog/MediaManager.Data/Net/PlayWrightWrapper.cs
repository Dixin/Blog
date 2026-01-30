namespace MediaManager.Net;

using Examples.Common;
using Microsoft.Playwright;

internal class PlayWrightWrapper : IAsyncDisposable
{
    private readonly string initialUrl;

    private IPlaywright? playwright;

    private IBrowser? browser;

    private IPage? page;

    internal PlayWrightWrapper(string initialUrl = "") => this.initialUrl = initialUrl;

    internal ValueTask<IPage> PageAsync() => this.page == null ? this.InitializeAsync() : ValueTask.FromResult(this.page);

    private async ValueTask<IPage> InitializeAsync()
    {
        this.playwright = await Playwright.CreateAsync();
        this.browser = await this.playwright.Chromium.LaunchAsync();
        this.page = await this.browser.NewPageAsync();
        await this.page.AbortMediaAsync();
        if (this.initialUrl.IsNotNullOrWhiteSpace())
        {
            IResponse? response = await this.page.GotoAsync(this.initialUrl);
            Debug.Assert(response is not null && response.Ok);
        }

        return this.page;
    }

    public async ValueTask DisposeAsync()
    {
        this.playwright?.Dispose();

        if (this.browser is not null)
        {
            await this.browser.DisposeAsync();
        }
    }

    internal async ValueTask Restart()
    {
        await this.DisposeAsync();
        await this.InitializeAsync();
    }

    internal async Task<string> GetStringAsync(string url, PageGotoOptions? options = null) =>
        await (await this.PageAsync()).GetStringAsync(url, options);

    internal async Task<string> GetUrlAsync() => (await this.PageAsync()).Url;
}