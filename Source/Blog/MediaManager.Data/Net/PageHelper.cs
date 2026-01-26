namespace MediaManager.Net;

using Examples.Common;
using MediaManager.IO;
using Microsoft.Playwright;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class PageHelper
{
    internal static readonly TimeSpan DefaultManualWait = TimeSpan.FromSeconds(30);

    internal static readonly TimeSpan DefaultNetworkWait = TimeSpan.FromSeconds(1);

    internal static readonly TimeSpan DefaultPageWait = TimeSpan.FromSeconds(60);

    internal static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

    internal static async Task<string> GetStringAsync(this IPage page, string url, PageGotoOptions? options = null)
    {
	    return await Retry.FixedIntervalAsync(async () =>
	    {
		    PageGotoOptions pageGotoOptions = options is null ? new PageGotoOptions() : new PageGotoOptions(options);
		    //pageGotoOptions.Timeout = (float)DefaultPageWait.TotalMilliseconds;
		    IResponse? response = await page.GotoAsync(url, pageGotoOptions);
		    Debug.Assert(response is not null && response.Ok);
		    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		    await page.WaitForLoadStateAsync(LoadState.Load);
		    //await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions() { Timeout = pageGotoOptions.Timeout });
		    await Task.Delay(DefaultNetworkWait);
		    if (await page.IsBlockedAsync())
		    {
			    throw new InvalidOperationException(url);
		    }

		    string html = await page.ContentAsync();
		    Debug.Assert(html.IsNotNullOrWhiteSpace());
		    return html;
		});
    }

    internal static async Task RefreshAsync(this IPage page, PageReloadOptions? options = null)
    {
        PageReloadOptions pageReloadOptions = options is null ? new PageReloadOptions() : new PageReloadOptions(options);
        pageReloadOptions.Timeout = (float)DefaultPageWait.TotalMilliseconds;
        IResponse? response = await page.ReloadAsync(pageReloadOptions);
        Debug.Assert(response is not null && response.Ok);
        await Task.Delay(DefaultNetworkWait);
        Debug.Assert(!await page.IsBlockedAsync());
    }

    private static readonly string[] Media = ["jpg", "jpeg", "png", "gif", "webp", ".mp4", ".mov"];

    internal static async Task AbortMediaAsync(this IPage page, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        //await page.RouteAsync(
        //    "**/*",
        //    async route =>
        //    {
        //        if (route.Request.Url.ContainsIgnoreCase("cdn.prod.metrics.imdb.com")
        //            || route.Request.Url.ContainsIgnoreCase("caching.graphql.imdb.com"))
        //        {
        //            await route.ContinueAsync();
        //            return;
        //        }

        //        if (route.Request.ResourceType is "image" or "media" or "other"
        //            && Media.Any(media => route.Request.Url.ContainsIgnoreCase(media))
        //            //&& !route.Request.Url.ContainsIgnoreCase("media-amazon.com")
        //            )
        //        {
        //            await route.FulfillAsync(new RouteFulfillOptions() { Status = 200 });
        //            log($"Blocked {route.Request.ResourceType} {route.Request.Url}");
        //            return;
        //        }

        //        await route.ContinueAsync();
        //    });
        await page.RouteAsync(
            new Regex(@"\.(jpg|jpeg|png|gif|webp|mp4|mov)", RegexOptions.IgnoreCase),
            async route =>
            {
                await route.AbortAsync();
                //log($"Blocked {route.Request.ResourceType} {route.Request.Url}");
            });
    }

    internal static async Task<bool> IsBlockedAsync(this IPage page, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string text = await page.TextContentAsync("body") ?? string.Empty;
        return text.IsNullOrWhiteSpace()
            || text.ContainsIgnoreCase("JavaScript is disabled")
            || text.ContainsIgnoreCase("need to verify that you're not a robot")
            || text.ContainsIgnoreCase("Enable JavaScript and then reload");
    }
}