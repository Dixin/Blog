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

    internal static async Task<Dictionary<string, PreferredSummary>> DownloadSummariesAsync(ISettings settings, Func<int, bool>? @continue = null, int index = 1, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        @continue ??= _ => true;
        Dictionary<string, PreferredSummary> allSummaries = File.Exists(settings.MoviePreferredSummary)
            ? await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredSummary>>(settings.MoviePreferredSummary)
            : new();
        List<string> links = [];
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        for (; @continue(index); index++)
        {
            string url = $"{settings.MoviePreferredUrl}/browse-movies?page={index}";
            log($"Start {url}");
            string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url));
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
                await JsonHelper.SerializeToFileAsync(allSummaries, settings.MoviePreferredSummary);
            }

            log($"End {url}");
        }

        links.OrderBy(link => link).ForEach(log);

        await JsonHelper.SerializeToFileAsync(allSummaries, settings.MoviePreferredSummary);
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

        ConcurrentDictionary<string, PreferredMetadata[]> details = File.Exists(settings.MoviePreferredMetadata)
            ? new(await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(settings.MoviePreferredMetadata, cancellationToken))
            : new();
        HashSet<string> existingLinks = new(details.Values.SelectMany(detailMetadata => detailMetadata).Select(detail => detail.Link), StringComparer.OrdinalIgnoreCase);

        int count = 1;
        await summaries
            .Values
            .Where(summary => !existingLinks.Contains(summary.Link))
            .OrderBy(summary => summary.Link)
            .Do(summary => log(summary.Link))
            .ToArray()
            .ParallelForEachAsync(async (summary, index) =>
            {
                log($"Start {index}:{summary.Link}");
                using HttpClient webClient = new HttpClient().AddEdgeHeaders();
                try
                {
                    string html = await Retry.FixedIntervalAsync(
                        async () => await webClient.GetStringAsync(summary.Link, cancellationToken),
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
                    lock (AddDetailLock)
                    {
                        details[detail.ImdbId] = details.ContainsKey(detail.ImdbId)
                            ? details[detail.ImdbId].Where(item => !item.Link.EqualsIgnoreCase(detail.Link)).Append(detail).ToArray()
                            : [detail];
                    }
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
            }, degreeOfParallelism, cancellationToken: cancellationToken);

        JsonHelper.SerializeToFile(details, settings.MoviePreferredMetadata, WriteJsonLock);
    }

    private static readonly object WriteJsonLock = new();

    private static readonly object AddDetailLock = new();

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
                        if (!Regex.IsMatch(imdbId, @"tt[0-9]+"))
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
}