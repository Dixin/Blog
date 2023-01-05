namespace Examples.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Yts
{
    private const int SaveFrequency = 100;

    internal static async Task DownloadSummariesAsync(string baseUrl, string jsonPath, Func<int, bool>? @continue = null, int index = 1, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        @continue ??= _ => true;
        Dictionary<string, YtsSummary> allSummaries = File.Exists(jsonPath)
            ? JsonSerializer.Deserialize<Dictionary<string, YtsSummary>>(await File.ReadAllTextAsync(jsonPath)) ?? throw new InvalidOperationException(jsonPath)
            : new();
        List<string> links = new();
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        for (; @continue(index); index++)
        {
            string url = $"{baseUrl}/browse-movies?page={index}";
            log($"Start {url}");
            string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url));
            CQ cq = new(html);
            if (cq[".browse-movie-wrap"].IsEmpty())
            {
                log($"! {url} is empty");
                break;
            }

            YtsSummary[] summaries = cq
                .Find(".browse-movie-wrap")
                .Select(dom =>
                {
                    CQ cqMovie = new(dom);
                    return new YtsSummary(
                        Link: cqMovie.Find(".browse-movie-title").Attr("href"),
                        Title: cqMovie.Find(".browse-movie-title").Text(),
                        ImdbRating: cqMovie.Find(".rating").Text().Replace(" / 10", string.Empty),
                        Genres: cqMovie.Find(@"h4[class!=""rating""]").Select(genre => genre.TextContent).ToArray(),
                        Year: int.TryParse(cqMovie.Find(".browse-movie-year").Text(), out int year) ? year : -1,
                        Image: cqMovie.Find(".img-responsive").Data<string>("cfsrc"));
                })
                .ToArray();

            summaries.ForEach(summary => allSummaries[summary.Link] = summary);
            summaries.ForEach(summary => links.Add(summary.Link));

            if (index % SaveFrequency == 0)
            {
                string jsonString = JsonSerializer.Serialize(allSummaries, new JsonSerializerOptions() { WriteIndented = true });
                await FileHelper.SaveAndReplaceAsync(jsonPath, jsonString, null, SaveJsonLock);
            }

            log($"End {url}");
        }

        links.OrderBy(link => link).ForEach(log);

        string finalJsonString = JsonSerializer.Serialize(allSummaries, new JsonSerializerOptions() { WriteIndented = true });
        await FileHelper.SaveAndReplaceAsync(jsonPath, finalJsonString, null, SaveJsonLock);
    }

    internal static async Task DownloadMetadataAsync(string baseUrl, string summaryJsonPath, string metadataJsonPath, Func<int, bool>? @continue = null, int index = 1, int degreeOfParallelism = 4, Action<string>? log = null)
    {
        await DownloadSummariesAsync(baseUrl, summaryJsonPath, @continue, index, log);
        await DownloadMetadataAsync(summaryJsonPath, metadataJsonPath, degreeOfParallelism, log);
    }

    internal static async Task DownloadMetadataAsync(string summaryJsonPath, string metadataJsonPath, int degreeOfParallelism = 4, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        
        string summaryJsonString = await File.ReadAllTextAsync(summaryJsonPath);
        Dictionary<string, YtsSummary> summaries = JsonSerializer.Deserialize<Dictionary<string, YtsSummary>>(summaryJsonString) ?? throw new InvalidOperationException(summaryJsonPath);

        ConcurrentDictionary<string, YtsMetadata[]> details = File.Exists(metadataJsonPath)
            ? new(JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(metadataJsonPath)) ?? throw new InvalidOperationException(metadataJsonPath))
            : new();
        Dictionary<string, YtsMetadata> existingMetadataByLink = details.Values.SelectMany(detailMetadata => detailMetadata).ToDictionary(detail => detail.Link, detail => detail);

        int count = 1;
        await summaries
            .Values
            .Where(summary => !existingMetadataByLink.ContainsKey(summary.Link))
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
                        async () => await webClient.GetStringAsync(summary.Link),
                        isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound });
                    CQ cq = new(html);
                    CQ info = cq.Find("#movie-info");
                    YtsMetadata detail = new(
                        Link: summary.Link,
                        Title: summary.Title,
                        ImdbId: info.Find("a.icon[title='IMDb Rating']").Attr("href").Replace("https://www.imdb.com/title/", string.Empty).Trim('/'),
                        ImdbRating: summary.ImdbRating,
                        Genres: summary.Genres,
                        Image: summary.Image,
                        Year: summary.Year,
                        Language: info.Find("h2 a span").Text().Trim().TrimStart('[').TrimEnd(']'),
                        Availabilities: info.Find("p.hidden-sm a[rel='nofollow']").ToDictionary(link => link.TextContent.Trim(), link => link.GetAttribute("href")));
                    lock (AddItemLock)
                    {
                        details[detail.ImdbId] = details.ContainsKey(detail.ImdbId)
                            ? details[detail.ImdbId].Where(item => !item.Link.EqualsIgnoreCase(detail.Link)).Append(detail).ToArray()
                            : new[] { detail };
                    }
                }
                catch (Exception exception)
                {
                    log($"{summary.Link} {exception}");
                }

                if (Interlocked.Increment(ref count) % SaveFrequency == 0)
                {
                    string jsonString = JsonSerializer.Serialize(details, new JsonSerializerOptions() { WriteIndented = true });
                    await FileHelper.SaveAndReplaceAsync(metadataJsonPath, jsonString, null, SaveJsonLock);
                }

                log($"End {index}:{summary.Link}");
            }, degreeOfParallelism);

        string jsonString = JsonSerializer.Serialize(details, new JsonSerializerOptions() { WriteIndented = true });
        await FileHelper.SaveAndReplaceAsync(metadataJsonPath, jsonString, null, SaveJsonLock);
    }

    private static readonly object SaveJsonLock = new();

    private static readonly object AddItemLock = new();

    internal static async Task SaveImdbSpecialTitles(string imdbBasicsPath, string jsonPath)
    {
        string[] specialTitles = File.ReadLines(imdbBasicsPath)
            .Skip(1)
            .Select(line => line.Split('\t'))
            .Where(line => !"0".EqualsOrdinal(line.ElementAtOrDefault(4)))
            .Select(line => line[0])
            .ToArray();
        string jsonString = JsonSerializer.Serialize(specialTitles, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, jsonString);
    }

    internal static async Task SaveYtsSpecialTitles(string directory, string jsonPath, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] titles = Directory
            .GetFiles(directory)
            .Select(file =>
            {
                string url = CQ.CreateDocumentFromFile(file)?.Find(@"a.icon[title=""IMDb Rating""]")?.Attr<string>("href")?.Replace("../../external.html?link=", string.Empty) ?? string.Empty;
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    try
                    {
                        string imdbId = Path.GetFileName(uri.LocalPath.TrimEnd('/'));
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
        string jsonString = JsonSerializer.Serialize(titles, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, jsonString);
    }
}