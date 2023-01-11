namespace Examples.Net;

using System.Collections.ObjectModel;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

internal static class Rarbg
{
    private const int WriteCount = 50;

    internal static async Task DownloadMetadataAsync(IEnumerable<string> urls, string jsonPath, Func<int, bool>? @continue = null, int degreeOfParallelism = 4, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, RarbgMetadata[]> allSummaries = File.Exists(jsonPath)
            ? new(JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(jsonPath)) ?? throw new InvalidOperationException(jsonPath))
            : new();
        await urls.ParallelForEachAsync(async (url, index) => await DownloadMetadataAsync(url, jsonPath, allSummaries, index + 1, @continue, log), degreeOfParallelism);
        string jsonText = JsonSerializer.Serialize(allSummaries, new JsonSerializerOptions() { WriteIndented = true });
        FileHelper.WriteText(jsonPath, jsonText, null, WriteJsonLock);
    }

    internal static async Task DownloadMetadataAsync(string url, string jsonPath, Func<int, bool>? @continue = null, Action<string>? log = null) =>
        await DownloadMetadataAsync(new[] { url }, jsonPath, @continue, 1, log: log);

    private static async Task DownloadMetadataAsync(string url, string jsonPath, ConcurrentDictionary<string, RarbgMetadata[]> allSummaries, int partitionIndex, Func<int, bool>? @continue = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        @continue ??= _ => true;

        try
        {
            using IWebDriver webDriver = WebDriverHelper.StartEdge(partitionIndex, true);
            webDriver.Url = url;
            new WebDriverWait(webDriver, WebDriverHelper.DefaultWait).Until(e => e.FindElement(By.Id("pager_links")));
            webDriver.Url = url;
            IWebElement pager = new WebDriverWait(webDriver, WebDriverHelper.DefaultDomWait).Until(e => e.FindElement(By.Id("pager_links")));
            int pageIndex = 1;
            do
            {
                if (!@continue(pageIndex))
                {
                    break;
                }

                await Task.Delay(WebDriverHelper.DefaultDomWait);
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

                        string[] genres = Array.Empty<string>();
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
                        return new RarbgMetadata(link, title, imdbId, imdbRating, genres, image, cells.Eq(2).Text().Trim(), cells.Eq(3).Text().Trim(), seed, leech, cells.Eq(7).Text().Trim());
                    })
                    .ForEach(summary =>
                    {
                        lock (AddSummaryLock)
                        {
                            allSummaries[summary.ImdbId] = allSummaries.ContainsKey(summary.ImdbId)
                                ? allSummaries[summary.ImdbId].Where(existing => !existing.Title.EqualsIgnoreCase(summary.Title)).Append(summary).ToArray()
                                : new[] { summary };
                        }
                    });

                log($"{partitionIndex}:{pageIndex} End {webDriver.Url}");
                if (pageIndex++ % WriteCount == 0)
                {
                    string jsonText = JsonSerializer.Serialize(allSummaries, new JsonSerializerOptions() { WriteIndented = true });
                    FileHelper.WriteText(jsonPath, jsonText, null, WriteJsonLock);
                }
            } while (webDriver.HasNextPage(ref pager, log));

            webDriver.Close();
            webDriver.Quit();
        }
        catch (Exception exception)
        {
            log(exception.ToString());
        }
        finally
        {
            string jsonText = JsonSerializer.Serialize(allSummaries, new JsonSerializerOptions() { WriteIndented = true });
            FileHelper.WriteText(jsonPath, jsonText, null, WriteJsonLock);
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
            pager = new WebDriverWait(webDriver, WebDriverHelper.DefaultWait).Until(e => e.FindElement(By.Id("pager_links")));
            return true;
        }
        catch (NoSuchElementException exception)
        {
            log(exception.ToString());
            return false;
        }
    }
}