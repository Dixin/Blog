namespace MediaManager.Net;

using Examples.Common;
using Examples.Linq;
using MediaManager.IO;
using Microsoft.Playwright;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Threading;

internal static class PageHelper
{
    internal static readonly TimeSpan DefaultManualWait = TimeSpan.FromSeconds(30);

    internal static readonly TimeSpan DefaultNetworkWait = TimeSpan.FromSeconds(1);

    internal static readonly TimeSpan DefaultPageWait = TimeSpan.FromSeconds(60);

    internal static readonly TimeSpan DefaultDomWait = TimeSpan.FromMilliseconds(100);

    internal static async Task<string> GetStringAsync(this IPage page, string url, PageGotoOptions? options = null, CancellationToken cancellationToken = default) =>
        await Retry.FixedIntervalAsync(
            async () =>
            {
                PageGotoOptions pageGotoOptions = options is null ? new PageGotoOptions() : new PageGotoOptions(options);
                IResponse? response = await page.GotoAsync(url, pageGotoOptions);
                Debug.Assert(response is not null && response.Ok);
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.WaitForLoadStateAsync(LoadState.Load);
                //await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions() { Timeout = pageGotoOptions.Timeout });
                await Task.Delay(DefaultNetworkWait, cancellationToken);
                if (await page.IsBlockedAsync())
                {
                    throw new InvalidOperationException(url);
                }

                string html = await page.ContentAsync();
                Debug.Assert(html.IsNotNullOrWhiteSpace());
                return html;
            },
            cancellationToken: cancellationToken);

    internal static async Task RefreshAsync(this IPage page, PageReloadOptions? options = null, CancellationToken cancellationToken = default) =>
        await Retry.FixedIntervalAsync(
            async () =>
            {
                IResponse? response = await page.ReloadAsync(options);
                Debug.Assert(response is not null && response.Ok);
                await Task.Delay(DefaultNetworkWait, cancellationToken);
                Debug.Assert(!await page.IsBlockedAsync());
            },
            cancellationToken: cancellationToken);

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
            async route => await route.AbortAsync());
    }

    internal static async Task<bool> IsBlockedAsync(this IPage page)
    {
        string text = await page.TextContentAsync("body") ?? string.Empty;
        return text.IsNullOrWhiteSpace()
            || text.ContainsIgnoreCase("JavaScript is disabled")
            || text.ContainsIgnoreCase("need to verify that you're not a robot")
            || text.ContainsIgnoreCase("Enable JavaScript and then reload");
    }

    private static async Task WaitForHiddenAsync(Func<ILocator> locatorFactory, CancellationToken cancellationToken = default)
    {
        long startingTimestamp = Stopwatch.GetTimestamp();
        while (await locatorFactory().CountAsync() > 0)
        {
            if (Stopwatch.GetElapsedTime(startingTimestamp) > DefaultManualWait)
            {
                throw new TimeoutException("Waiting for prompt to be closed timed out.");
            }

            await Task.Delay(DefaultDomWait, cancellationToken);
        }
    }

    private static async Task WaitForVisibleAsync(Func<ILocator> locatorFactory, int locatorCount, CancellationToken cancellationToken = default)
    {
        if (locatorCount <= 0)
        {
            return;
        }

        long spoilerInitializingTimestamp = Stopwatch.GetTimestamp();
        while (await locatorFactory().CountAsync() != locatorCount)
        {
            if (Stopwatch.GetElapsedTime(spoilerInitializingTimestamp) > DefaultManualWait)
            {
                throw new TimeoutException("Waiting for Spoilers to be loaded timed out.");
            }

            await Task.Delay(DefaultDomWait, cancellationToken);
        }
    }

    private static async Task<int> ClickOrPressAsync(Func<ILocator> locatorFactory, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ILocator> locators = await locatorFactory().AllAsync();
        if (locators.Count > 0)
        {
            await locators.Reverse().ForEachAsync(
                async button =>
                {
                    try
                    {
                        await button.ClickAsync();
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        await button.PressAsync(" ");
                    }
                },
                cancellationToken);

            await WaitForHiddenAsync(locatorFactory, cancellationToken);
        }

        return locators.Count;
    }

    internal static async Task WaitForHiddenAsync(this IPage page, string selector, PageLocatorOptions? options = null, CancellationToken cancellationToken = default) =>
        await WaitForHiddenAsync(() => page.Locator(selector, options), cancellationToken);

    internal static async Task WaitForHiddenAsync(this IPage page, AriaRole ariaRole, PageGetByRoleOptions? options = null, CancellationToken cancellationToken = default) =>
        await WaitForHiddenAsync(() => page.GetByRole(ariaRole, options), cancellationToken);

    internal static async Task WaitForVisibleAsync(this IPage page, string selector, PageLocatorOptions? options = null, int locatorCount = 0, CancellationToken cancellationToken = default) =>
        await WaitForVisibleAsync(() => page.Locator(selector, options), locatorCount, cancellationToken);

    internal static async Task WaitForVisibleAsync(this IPage page, AriaRole ariaRole, PageGetByRoleOptions? options = null, int locatorCount = 0, CancellationToken cancellationToken = default) =>
        await WaitForVisibleAsync(() => page.GetByRole(ariaRole, options), locatorCount, cancellationToken);

    internal static async Task WaitForVisibleAsync(this IPage page, Regex text, PageGetByTextOptions? options = null, int locatorCount = 0, CancellationToken cancellationToken = default) =>
        await WaitForVisibleAsync(() => page.GetByText(text, options), locatorCount, cancellationToken);

    internal static async Task<int> ClickOrPressAsync(this IPage page, string selector, PageLocatorOptions? options = null, CancellationToken cancellationToken = default) =>
        await ClickOrPressAsync(() => page.Locator(selector, options), cancellationToken);

    internal static async Task<int> ClickOrPressAsync(this IPage page, AriaRole ariaRole, PageGetByRoleOptions? options = null, CancellationToken cancellationToken = default) =>
        await ClickOrPressAsync(() => page.GetByRole(ariaRole, options), cancellationToken);
}