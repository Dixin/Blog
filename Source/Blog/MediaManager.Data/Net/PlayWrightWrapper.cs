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

    internal ValueTask<IPage> PageAsync() => this.page == null ? this.InitializeAsync(this.initialUrl) : ValueTask.FromResult(this.page);

    private async ValueTask<IPage> InitializeAsync(string url = "")
    {
        this.playwright = await Playwright.CreateAsync();
        this.browser = await this.playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions() { Headless = false });
        this.page = await this.browser.NewPageAsync();
        //await this.page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>()
        //{
        //    ["cookie"] = "cf_clearance=z_dfCu_X81W8TP4HDFFbRj2rgHKtNpfHKE2nZjfHZkg-1769764272-1.2.1.1-RInEL_Pum2_VVmwamyLsc4d2tjOImlhmrHbrO90idsv7bfnOfjaLg.zdFXPmtLv7PfBe7y2tJz2MkMi18EwfwV68fZbIlr1I4XiryJhhNu7Cr1kkXtHFIvF0fLDRvcYJsi2YtN5lgs.2ej2GpqUyBn5C4yLu.DBj.L7CUuhS.3iJBp2TbQsycu7oIYOQ5nJOZfeIuF2U4Kt5PeA2CM2N_iAlM3..bLTpSwZb1V4oDgo"
        //});
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