namespace MediaManager.Net;

using System.Collections.ObjectModel;
using CsQuery;
using Examples.Common;
using Examples.Linq;
using Examples.Net;
using MediaManager.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

internal static class Top
{
    private const int WriteCount = 50;

    internal static async Task DownloadMetadataAsync(
        IEnumerable<string> urls, string jsonPath, Func<int, bool>? @continue = null, 
        int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= Video.IOMaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        string jsonText;
        ConcurrentDictionary<string, TopMetadata[]> allSummaries = File.Exists(jsonPath)
            ? new(await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(jsonPath, cancellationToken))
            : new();

        await urls.ParallelForEachAsync(
            async (url, index, token) => await DownloadMetadataAsync(url, jsonPath, allSummaries, index + 1, @continue, log, token), 
            degreeOfParallelism, 
            cancellationToken);
        await JsonHelper.SerializeToFileAsync(allSummaries, jsonPath, cancellationToken);
    }

    internal static async Task DownloadMetadataAsync(string url, string jsonPath, Func<int, bool>? @continue = null, Action<string>? log = null, CancellationToken cancellationToken = default) =>
        await DownloadMetadataAsync(new[] { url }, jsonPath, @continue, 1, log, cancellationToken);

    private static async Task DownloadMetadataAsync(
        string url, string jsonPath, ConcurrentDictionary<string, TopMetadata[]> allSummaries, int partitionIndex, Func<int, bool>? @continue = null, 
        Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        @continue ??= _ => true;

        try
        {
            using IWebDriver webDriver = WebDriverHelper.Start(partitionIndex, true);
            webDriver.Url = url;
            new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.Id("pager_links")));
            webDriver.Url = url;
            IWebElement pager = new WebDriverWait(webDriver, WebDriverHelper.DefaultDomWait).Until(driver => driver.FindElement(By.Id("pager_links")));
            int pageIndex = 1;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!@continue(pageIndex))
                {
                    break;
                }

                await Task.Delay(WebDriverHelper.DefaultDomWait, cancellationToken);
                log($"{partitionIndex}:{pageIndex} Start {webDriver.Url}");

                CQ page = webDriver.PageSource;
                page
                    .Find("table.lista2t tr.lista2")
                    .Select(row =>
                    {
                        CQ cells = row.Cq().Children();
                        string[] texts = cells.Eq(1).Text().Trim().Split("  ", StringSplitOptions.RemoveEmptyEntries).Where(text => text.IsNotNullOrWhiteSpace()).ToArray();
                        string title = texts[0].Trim();
                        CQ links = cells.Eq(1).Find("a");
                        string baseUrl = new Uri(webDriver.Url).GetLeftPart(UriPartial.Authority);
                        string link = $"{baseUrl}{links[0].GetAttribute("href")}";
                        string imdbId = links.Length > 1
                            ? links[1].GetAttribute("href").Replace("/torrents.php?imdb=", string.Empty).Trim()
                            : string.Empty;

                        string[] genres = [];
                        string imdbRating = string.Empty;
                        if (texts.Length > 1)
                        {
                            string[] descriptions = texts[1].Trim().Split(" IMDB: ");
                            if (descriptions.Length > 0)
                            {
                                genres = descriptions[0].Split(", ").Select(genre => genre.Trim()).ToArray();
                            }

                            if (descriptions.Length > 1)
                            {
                                imdbRating = descriptions[1].Replace("/10", string.Empty).Trim();
                            }
                        }

                        string image = links[0].GetAttribute("onmouseover")?.Replace(@"return overlib('<img src=\'", string.Empty).Replace(@"\' border=0>')", string.Empty) ?? string.Empty;
                        int seed = int.TryParse(cells.Eq(4).Text().Trim(), out int seedValue) ? seedValue : -1;
                        int leech = int.TryParse(cells.Eq(5).Text().Trim(), out int leechValue) ? leechValue : -1;
                        return new TopMetadata(link, title, imdbId, imdbRating, genres, image, cells.Eq(2).Text().Trim(), cells.Eq(3).Text().Trim(), seed, leech, cells.Eq(7).Text().Trim());
                    })
                    .ForEach(summary =>
                    {
                        lock (AddSummaryLock)
                        {
                            allSummaries[summary.ImdbId] = allSummaries.ContainsKey(summary.ImdbId)
                                ? allSummaries[summary.ImdbId].Where(existing => !existing.Title.EqualsIgnoreCase(summary.Title)).Append(summary).ToArray()
                                : [summary];
                        }
                    });

                log($"{partitionIndex}:{pageIndex} End {webDriver.Url}");
                if (pageIndex++ % WriteCount == 0)
                {
                    JsonHelper.SerializeToFile(allSummaries, jsonPath, WriteJsonLock);
                }
            } while (webDriver.HasNextPage(ref pager, log));
        }
        catch (Exception exception)
        {
            log(exception.ToString());
        }
        finally
        {
            JsonHelper.SerializeToFile(allSummaries, jsonPath, WriteJsonLock);
        }
    }

    private static readonly object WriteJsonLock = new();

    private static readonly object AddSummaryLock = new();

    private static bool HasNextPage(this IWebDriver webDriver, ref IWebElement pager, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ReadOnlyCollection<IWebElement> nextPage = pager.FindElements(By.CssSelector("a[title='next page']"));
        if (nextPage.Count <= 0)
        {
            return false;
        }

        webDriver.Url = nextPage[0].GetAttribute("href");
        try
        {
            pager = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.Id("pager_links")));
            return true;
        }
        catch (NoSuchElementException exception)
        {
            log(exception.ToString());
            return false;
        }
    }
}