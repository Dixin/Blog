namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.Linq;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Linq;

internal static class Shared
{
    private static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 50);

    private const int WriteCount = 100;

    internal static async Task<SharedMetadata[]> DownloadMetadataAsync(ISettings settings, bool skipExisting = false, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        degreeOfParallelism ??= MaxDegreeOfParallelism;

        IDictionary<string, string> itemUrls = await DownloadListAsync(settings.MovieSharedUrl, degreeOfParallelism.Value, log, cancellationToken);
        return await DownloadItemsAsync(settings, itemUrls, degreeOfParallelism.Value, skipExisting, log, cancellationToken);
    }

    private static async Task<IDictionary<string, string>> DownloadListAsync(string indexUrl, int degreeOfParallelism, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        using HttpClient httpClient = new();
        CQ indexCQ = await httpClient.GetStringAsync(indexUrl, cancellationToken);
        CQ lastPageCQ = indexCQ.Find("#content > div > nav > a:last");
        string lastListUrl = lastPageCQ.Attr("href");
        int lastListIndex = int.Parse(lastListUrl.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last());
        log($"Total pages: {lastListIndex}.");
        ConcurrentDictionary<string, string> itemLinks = new(StringComparer.OrdinalIgnoreCase);
        await Enumerable
            .Range(1, lastListIndex)
            .Select(listIndex => $"http://hotxshare.com/page/{listIndex}/")
            .ParallelForEachAsync(
                async (listUrl, index, token) =>
                {
                    using HttpClient httpClient = new();
                    CQ listCQ = await Retry.FixedIntervalAsync(
                        async () => await httpClient.GetStringAsync(listUrl, token),
                        retryingHandler: (sender, args) => log($"{args.CurrentRetryCount} {listUrl}: {args.LastException.GetBaseException()}"), 
                        cancellationToken: token);
                    log(listUrl);
                    listCQ.Find("#content article")
                        .Select(article => (article.Id, Url: article.Cq().Find(" h2.entry-title a").Attr("href")))
                        .Do(item => Debug.Assert(item.Id.IsNotNullOrWhiteSpace() && item.Url.IsNotNullOrWhiteSpace()))
                        .ForEach(item => itemLinks[item.Url] = item.Id);
                },
                degreeOfParallelism,
                cancellationToken);
        return itemLinks;
    }

    private static async Task<SharedMetadata[]> DownloadItemsAsync(ISettings settings, IDictionary<string, string> itemUrls, int degreeOfParallelism, bool skipExisting = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        log($"Total items: {itemUrls.Count}.");
        ConcurrentDictionary<string, SharedMetadata> results = new(StringComparer.OrdinalIgnoreCase);
        IEnumerable<KeyValuePair<string, string>> downloadingUrls = itemUrls;
        if (skipExisting && File.Exists(settings.MovieSharedMetadata))
        {
            (await settings.LoadMovieSharedMetadataAsync(cancellationToken))
                .AsParallel()
                .Where(item => itemUrls.ContainsKey(item.Url))
                .ForAll(item => results[item.Url] = item);

            downloadingUrls = downloadingUrls.Where(item => !results.ContainsKey(item.Key));
        }

        Lock writeJsonLock = new();
        await downloadingUrls
            .ParallelForEachAsync(
                async (url, index, token) =>
                {
                    using HttpClient httpClient = new();
                    CQ itemCQ;
                    try
                    {
                        itemCQ = await Retry.FixedIntervalAsync(
                            async () => await httpClient.GetStringAsync(url.Key, token),
                            retryingHandler: (sender, args) => log($"{args.CurrentRetryCount} {url.Key}: {args.LastException.GetBaseException()}"), 
                            cancellationToken: token);
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log(exception.GetBaseException().ToString());
                        return;
                    }

                    log(url.Key);
                    CQ articleCQ = itemCQ.Find("article");
                    string title = articleCQ.Find("h1.entry-title").Text().Trim();
                    Debug.Assert(title.IsNotNullOrWhiteSpace());
                    string contentHtml = articleCQ.Find("div.entry-content").Html();
                    Debug.Assert(contentHtml.IsNotNullOrWhiteSpace());
                    string[] categories = articleCQ.Find("div.entry-meta span.bl_categ a").Select(linkDom => linkDom.TextContent.Trim()).ToArray();
                    string[] tags = articleCQ.Find("span.bl_posted a").Select(linkDom => linkDom.TextContent.Trim()).ToArray();
                    string commentsHtml = itemCQ.Find("#comments ol.commentlist").Html();
                    string[] imdbIds = new string[] { contentHtml, commentsHtml }
                        .SelectMany(html => ImdbMetadata.ImdbIdInLinkRegex().Matches(html))
                        .Select(match => match.Groups[1].Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Do(imdbId => Debug.Assert(imdbId.IsImdbId()))
                        .ToArray();
                    string[] downloads = articleCQ.Find("div.entry-content a")
                        .Where(linkDom => linkDom.Cq().Find("img").IsEmpty())
                        .Select(linkDom => linkDom.GetAttribute("href"))
                        .ToArray();
                    results[url.Key] = new SharedMetadata(url.Value, url.Key, title, contentHtml, categories, tags, downloads, imdbIds);

                    if (results.Count % WriteCount == 0)
                    {
                        SharedMetadata[] ordered = results.Values.OrderByDescending(item => item.Id).ToArray();
                        JsonHelper.SerializeToFile(ordered, settings.MovieSharedMetadata, ref writeJsonLock);
                    }
                },
                degreeOfParallelism,
                cancellationToken);
        SharedMetadata[] ordered = results.Values.OrderByDescending(item => item.Id).ToArray();
        await settings.WriteMovieSharedMetadataAsync(ordered, cancellationToken);
        return ordered;
    }
}
