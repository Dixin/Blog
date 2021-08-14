namespace Examples.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using CsQuery;
    using Examples.Linq;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    internal static class Yts
    {
        private const string BaseUrl = "https://yts.mx";

        private const int RetryCount = 10;

        private const int SaveFrequency = 100;

        private static readonly Action<string> Log = text => Trace.WriteLine(text);

        internal static async Task DownloadSummariesAsync(string jsonPath, Func<int, bool>? @continue = null, int index = 1)
        {
            @continue ??= _ => true;
            Dictionary<string, YtsSummary> allSummaries = File.Exists(jsonPath)
                ? JsonSerializer.Deserialize<Dictionary<string, YtsSummary>>(await File.ReadAllTextAsync(jsonPath)) ?? throw new InvalidOperationException(jsonPath)
                : new();
            using WebClient webClient = new WebClient().AddChromeHeaders();
            for (; @continue(index); index++)
            {
                string url = $"{BaseUrl}/browse-movies?page={index}";
                Log($"Start {url}");
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(url), RetryCount);
                CQ cq = new(html);
                if (cq[".browse-movie-wrap"].IsEmpty())
                {
                    Log($"! {url} is empty");
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

                if (index % SaveFrequency == 0)
                {
                    string jsonString = JsonSerializer.Serialize(allSummaries, new() { WriteIndented = true });
                    await File.WriteAllTextAsync(jsonPath, jsonString);
                }

                Log($"End {url}");
            }

            string finalJsonString = JsonSerializer.Serialize(allSummaries, new() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, finalJsonString);
        }

        internal static async Task DownloadMetadataAsync(string summaryJsonPath, string metadataJsonPath, Func<int, bool>? @continue = null, int index = 1, int degreeOfParallelism = 4)
        {
            await DownloadSummariesAsync(summaryJsonPath, @continue, index);
            await DownloadMetadataAsync(summaryJsonPath, metadataJsonPath, degreeOfParallelism);
        }

        internal static async Task DownloadMetadataAsync(string summaryJsonPath, string metadataJsonPath, int degreeOfParallelism = 4)
        {
            string summaryJsonString = await File.ReadAllTextAsync(summaryJsonPath);
            Dictionary<string, YtsSummary> summaries = JsonSerializer.Deserialize<Dictionary<string, YtsSummary>>(summaryJsonString) ?? throw new InvalidOperationException(summaryJsonPath);

            ConcurrentDictionary<string, YtsMetadata[]> details = File.Exists(metadataJsonPath)
                ? new(JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(metadataJsonPath)) ?? throw new InvalidOperationException(metadataJsonPath))
                : new();
            Dictionary<string, YtsMetadata> existingMetadataByLink = details.Values.SelectMany(details => details).ToDictionary(detail => detail.Link, detail => detail);

            int count = 1;
            await summaries
                .Values
                .Where(summary => !existingMetadataByLink.ContainsKey(summary.Link))
                .ParallelForEachAsync(async (summary, index) =>
                {
                    Log($"Start {index}:{summary.Link}");
                    using WebClient webClient = new WebClient().AddChromeHeaders();
                    try
                    {
                        string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(summary.Link), RetryCount);
                        CQ cq = new(html);
                        CQ info = cq.Find("#movie-info");
                        YtsMetadata detail = new(
                            Link: summary.Link,
                            Title: summary.Title,
                            ImdbId: info.Find("a.icon[title='IMDb Rating']").Attr("href").Replace("https://www.imdb.com/title/", string.Empty).Trim('/'),
                            ImdbRating: summary.ImdbRating,
                            Genres: summary.Genres,
                            Year: summary.Year,
                            Image: summary.Image,
                            Language: info.Find("h2 a span").Text().Trim().TrimStart('[').TrimEnd(']'),
                            Availabilities: info.Find("p.hidden-sm a[rel='nofollow']").ToDictionary(link => link.TextContent.Trim(), link => link.GetAttribute("href")));
                        lock (AddItemLock)
                        {
                            details[detail.ImdbId] = details.ContainsKey(detail.ImdbId)
                                ? details[detail.ImdbId].Where(item => !string.Equals(item.Link, detail.Link, StringComparison.OrdinalIgnoreCase)).Append(detail).ToArray()
                                : new[] { detail };
                        }
                    }
                    catch (Exception exception)
                    {
                        Log($"{summary.Link} {exception}");
                    }

                    if (Interlocked.Increment(ref count) % SaveFrequency == 0)
                    {
                        SaveDetail(metadataJsonPath, details);
                    }

                    Log($"End {index}:{summary.Link}");
                }, degreeOfParallelism);

            SaveDetail(metadataJsonPath, details);
        }

        private static readonly object SaveJsonLock = new();

        private static readonly object AddItemLock = new();

        private static void SaveDetail(string detailJsonPath, IDictionary<string, YtsMetadata[]> allDetails)
        {
            string jsonString = JsonSerializer.Serialize(allDetails, new() { WriteIndented = true });
            lock (SaveJsonLock)
            {
                File.WriteAllText(detailJsonPath, jsonString);
            }
        }

        internal static async Task SaveImdbSpecialTitles(string imdbBasicsPath, string jsonPath)
        {
            string[] specialTitles = File.ReadLines(imdbBasicsPath)
                .Skip(1)
                .Select(line => line.Split('\t'))
                .Where(line => !"0".Equals(line.ElementAtOrDefault(4), StringComparison.Ordinal))
                .Select(line => line[0])
                .ToArray();
            string jsonString = JsonSerializer.Serialize(specialTitles, new() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, jsonString);
        }

        internal static async Task SaveYtsSpecialTitles(string directory, string jsonPath)
        {
            string[] titles = Directory
                .GetFiles(directory)
                .Select(file =>
                    {
                        string url = CQ.CreateDocumentFromFile(file)?.Find(@"a.icon[title=""IMDb Rating""]")?.Attr<string>("href")?.Replace("../../external.html?link=", string.Empty) ?? string.Empty;
                        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                        {
                            try
                            {
                                string imdbId = Path.GetFileName(uri?.LocalPath?.TrimEnd('/')) ?? string.Empty;
                                if (!Regex.IsMatch(imdbId, @"tt[0-9]+"))
                                {
                                    Trace.WriteLine($"Invalid IMDB Id: {file}, {url}.");
                                }

                                return imdbId;
                            }
                            catch
                            {
                                Trace.WriteLine($"Invalid IMDB Id: {file}, {url}.");
                                return string.Empty;
                            }
                        }
                        else
                        {
                            Trace.WriteLine($"Invalid IMDB Id: {file}, {url}.");
                            return string.Empty;
                        }
                    })
                .Where(imdbId => !string.IsNullOrWhiteSpace(imdbId))
                .ToArray();
            string jsonString = JsonSerializer.Serialize(titles, new() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, jsonString);
        }
    }
}
