namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Preferred
{
    private const int WriteCount = 100;

    internal static async Task<Dictionary<string, PreferredSummary>> DownloadSummariesAsync(
        ISettings settings, Func<int, bool>? @continue = null, int index = 1, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        @continue ??= _ => true;
        Dictionary<string, PreferredSummary> allSummaries = await JsonHelper
            .DeserializeFromFileAsync<Dictionary<string, PreferredSummary>>(settings.MoviePreferredSummary, new(), cancellationToken);
        List<string> links = [];
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        for (; @continue(index); index++)
        {
            string url = $"{settings.MoviePreferredUrl}/browse-movies?page={index}";
            log($"Start {url}");
            string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, cancellationToken));
            CQ cq = new(html);
            if (cq[".browse-movie-wrap"].IsEmpty())
            {
                log($"! {url} is empty");
                break;
            }

            PreferredSummary[] summaries = cq
                .Find(".browse-movie-wrap")
                .Select(dom =>
                {
                    CQ cqMovie = new(dom);
                    return new PreferredSummary(
                        Link: cqMovie.Find(".browse-movie-title").Attr("href"),
                        Title: cqMovie.Find(".browse-movie-title").Text(),
                        ImdbRating: cqMovie.Find(".rating").Text().Replace(" / 10", string.Empty),
                        Genres: cqMovie.Find("""h4[class!="rating"]""").Select(genre => genre.TextContent).ToArray(),
                        Year: int.TryParse(cqMovie.Find(".browse-movie-year").Text(), out int year) ? year : -1,
                        Image: cqMovie.Find(".img-responsive").Data<string>("cfsrc"));
                })
                .ToArray();

            summaries.ForEach(summary => allSummaries[summary.Link] = summary);
            summaries.ForEach(summary => links.Add(summary.Link));

            if (index % WriteCount == 0)
            {
                await JsonHelper.SerializeToFileAsync(allSummaries, settings.MoviePreferredSummary, cancellationToken);
            }

            log($"End {url}");
        }

        links.OrderBy(link => link).ForEach(log);

        await JsonHelper.SerializeToFileAsync(allSummaries, settings.MoviePreferredSummary, cancellationToken);
        return allSummaries;
    }

    internal static async Task DownloadMetadataAsync(ISettings settings, Func<int, bool>? @continue = null, int index = 1, int? degreeOfParallelism = null, Action<string>? log = null)
    {
        degreeOfParallelism ??= Video.IOMaxDegreeOfParallelism;
        Dictionary<string, PreferredSummary> allSummaries = await DownloadSummariesAsync(settings, @continue, index, log);
        await DownloadMetadataAsync(settings, allSummaries, degreeOfParallelism, log);
    }

    internal static async Task DownloadMetadataAsync(ISettings settings, Dictionary<string, PreferredSummary>? summaries = null, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= Video.IOMaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        summaries ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredSummary>>(settings.MoviePreferredSummary, cancellationToken);

        ConcurrentDictionary<string, PreferredMetadata[]> details = await JsonHelper
            .DeserializeFromFileAsync<ConcurrentDictionary<string, PreferredMetadata[]>>(settings.MoviePreferredMetadata, new(), cancellationToken);
        HashSet<string> existingLinks = new(details.Values.SelectMany(detailMetadata => detailMetadata).Select(detail => detail.Link), StringComparer.OrdinalIgnoreCase);

        int count = 1;
        await summaries
            .Values
            .Where(summary => !existingLinks.Contains(summary.Link))
            .OrderBy(summary => summary.Link)
            .Do(summary => log(summary.Link))
            .ToArray()
            .ParallelForEachAsync(
                async (summary, index, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    log($"Start {index}:{summary.Link}");
                    using HttpClient webClient = new HttpClient().AddEdgeHeaders();
                    try
                    {
                        string html = await Retry.FixedIntervalAsync(async () => await webClient.GetStringAsync(summary.Link, token),
                            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound });
                        CQ cq = new(html);
                        CQ info = cq.Find("#movie-info");
                        PreferredMetadata detail = new(
                            Link: summary.Link,
                            Title: summary.Title,
                            ImdbId: info.Find("a.icon[title='IMDb Rating']").Attr("href").Replace("https://www.imdb.com/title/", string.Empty).Trim('/'),
                            ImdbRating: summary.ImdbRating,
                            Genres: summary.Genres,
                            Image: summary.Image,
                            Year: summary.Year,
                            Language: info.Find("h2 a span").Text().Trim().TrimStart('[').TrimEnd(']'),
                            Availabilities: info.Find("p.hidden-sm a[rel='nofollow']").ToDictionary(link => link.TextContent.Trim(), link => link.GetAttribute("href")));

                        details.AddOrUpdate(
                            detail.ImdbId,
                            key => [detail],
                            (key, group) => group
                                .Where(item => !item.Link.EqualsIgnoreCase(detail.Link))
                                .Append(detail)
                                .ToArray());
                    }
                    catch (Exception exception)
                    {
                        log($"{summary.Link} {exception}");
                    }

                    if (Interlocked.Increment(ref count) % WriteCount == 0)
                    {
                        JsonHelper.SerializeToFile(details, settings.MoviePreferredMetadata, WriteJsonLock);
                    }

                    log($"End {index}:{summary.Link}");
                },
                degreeOfParallelism,
                cancellationToken);

        JsonHelper.SerializeToFile(details, settings.MoviePreferredMetadata, WriteJsonLock);
    }

    private static readonly object WriteJsonLock = new();

    internal static async Task WriteImdbSpecialTitles(string imdbBasicsPath, string jsonPath)
    {
        string[] specialTitles = File.ReadLines(imdbBasicsPath)
            .Skip(1)
            .Select(line => line.Split('\t'))
            .Where(line => !"0".EqualsOrdinal(line.ElementAtOrDefault(4)))
            .Select(line => line[0])
            .ToArray();
        await JsonHelper.SerializeToFileAsync(specialTitles, jsonPath);
    }

    internal static async Task WritePreferredSpecialTitles(string directory, string jsonPath, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] titles = Directory
            .GetFiles(directory)
            .Select(file =>
            {
                string url = CQ.CreateDocumentFromFile(file)?.Find("""a.icon[title="IMDb Rating"]""")?.Attr<string>("href")?.Replace("../../external.html?link=", string.Empty) ?? string.Empty;
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    try
                    {
                        string imdbId = PathHelper.GetFileName(uri.LocalPath.TrimEnd('/'));
                        if (!imdbId.IsImdbId())
                        {
                            log($"Invalid IMDB Id: {file}, {url}.");
                        }

                        return imdbId;
                    }
                    catch
                    {
                        log($"Invalid IMDB Id: {file}, {url}.");
                        return string.Empty;
                    }
                }

                log($"Invalid IMDB Id: {file}, {url}.");
                return string.Empty;
            })
            .Where(imdbId => imdbId.IsNotNullOrWhiteSpace())
            .ToArray();
        await JsonHelper.SerializeToFileAsync(titles, jsonPath);
    }

    internal static async Task DownloadAllTorrentsAsync(ISettings settings, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        HashSet<string> existingTorrents = new(Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, TorrentHelper.TorrentSearchPattern), StringComparer.OrdinalIgnoreCase);

        ConcurrentDictionary<string, PreferredMetadata[]> preferredMetadata = await JsonHelper
            .DeserializeFromFileAsync<ConcurrentDictionary<string, PreferredMetadata[]>>(settings.MoviePreferredMetadata, new(), cancellationToken);
        PreferredMetadata[] allMetadata = preferredMetadata
            .SelectMany(group => group.Value)
            .ToArray();
        int length = allMetadata.Length;
        log($"Estimation {allMetadata.Length - existingTorrents.Count} files.");
        await allMetadata
            .ParallelForEachAsync(
                async (metadata, index, token) =>
                {
                    if (index % 100 == 0)
                    {
                        log($"{index * 100 / length}% - {index}/{length}");
                    }

                    await DownloadTorrentsAsync(settings, metadata, existingTorrents.Contains, isDryRun, log, token);
                },
                Environment.ProcessorCount,
                cancellationToken);
    }

    internal static async Task<bool> DownloadTorrentsAsync(
        ISettings settings, PreferredMetadata preferredMetadata, Func<string, bool>? skip = null, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        skip ??= File.Exists;

        (string Quality, string Link, PreferredMetadata Metadata)[] availabilities = preferredMetadata
            .Availabilities
            .Select(availability => (Quality: availability.Key, Link: availability.Value, Metadata: preferredMetadata))
            .ToArray();
        (string Quality, string Link, PreferredMetadata Metadata)[] videos = availabilities
            .Where(availability => availability.Quality.ContainsIgnoreCase("1080") && availability.Quality.ContainsIgnoreCase("Blu") && availability.Quality.ContainsIgnoreCase("265"))
            .ToArray();
        if (videos.IsEmpty())
        {
            videos = availabilities
                .Where(availability => availability.Quality.ContainsIgnoreCase("1080") && availability.Quality.ContainsIgnoreCase("Blu"))
                .ToArray();
        }

        if (videos.IsEmpty())
        {
            videos = availabilities
                .Where(availability => availability.Quality.ContainsIgnoreCase("1080") && availability.Quality.ContainsIgnoreCase("WEB") && availability.Quality.ContainsIgnoreCase("265"))
                .ToArray();
        }

        if (videos.IsEmpty())
        {
            videos = availabilities
                .Where(availability => availability.Quality.ContainsIgnoreCase("1080") && availability.Quality.ContainsIgnoreCase("WEB"))
                .ToArray();
        }

        if (videos.IsEmpty())
        {
            videos = availabilities
                .Where(availability => availability.Quality.ContainsIgnoreCase("720") && availability.Quality.ContainsIgnoreCase("Blu"))
                .ToArray();
        }

        if (videos.IsEmpty())
        {
            videos = availabilities
                .Where(availability => availability.Quality.ContainsIgnoreCase("720") && availability.Quality.ContainsIgnoreCase("WEB"))
                .ToArray();
        }

        if (videos.IsEmpty())
        {
            videos = availabilities
                .Where(availability => availability.Quality.ContainsIgnoreCase("480"))
                .ToArray();
        }

        if (videos.IsEmpty())
        {
            log($"!{preferredMetadata.ImdbId} has no valid availability: {string.Join("|", availabilities.Select(availability => availability.Quality))}.");
            return false;
        }

        //log($"{preferredVideos.First().ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{preferredVideos.First().Title} {preferredVideos.First().Link} {imdbMetadata.Link}");
        //log($"{imdbMetadata.Link}keywords");
        //log($"{imdbMetadata.Link}parentalguide");
        using HttpClient? httpClient = isDryRun ? null : new HttpClient().AddEdgeHeaders();
        bool isDownloaded = false;
        await videos.ForEachAsync(
            async (video, index, token) =>
            {
                //log($"{video.Key} | {video.Link} | {video.Metadata.Link}");
                string hash = video.Link.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last().Trim();
                string webUrl = video.Metadata.Link.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last().Trim();
                string quality = video.Quality.Replace(".", " ").FilterForFileSystem();
                string title = video.Metadata.Title.ReplaceOrdinal(" - ", " ").ReplaceOrdinal("-", " ").ReplaceOrdinal(".", " ").ReplaceIgnoreCase("：", " ").ReplaceOrdinal("  ", " ").FilterForFileSystem().Trim();
                string name = $"{preferredMetadata.ImdbId}.{hash}.{webUrl}.{quality}.{title}";
                string file = Path.Combine(settings.MovieMetadataCacheDirectory, $"{name}{TorrentHelper.TorrentExtension}");
                if (skip(file))
                {
                    return;
                }

                if (httpClient is null)
                {
                    log($"""
                         {name}
                         {file}

                         """);
                    return;
                }

                try
                {
                    await Retry.FixedIntervalAsync(
                            async () => await httpClient.GetFileAsync(video.Link, file, token),
                            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound });
                    isDownloaded = true;
                    log($"""
                         {name}
                         {file}

                         """);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                    log($"""
                         {name}
                         magnet:?xt=urn:btih:{hash}

                         """);
                }
            },
            cancellationToken);
        return isDownloaded;
    }
}