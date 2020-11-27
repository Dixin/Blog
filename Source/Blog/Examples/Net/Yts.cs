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

    public record YtsSummary(string Title, string Link, int Year, string Image, string Rating, string[] Tags);

    public record YtsDetail(string Title, string Link, int Year, string Image, string Rating, string[] Tags, string ImdbId, string Language, Dictionary<string, string> Availabilities)
        : YtsSummary(Title, Link, Year, Image, Rating, Tags);


    internal static class Yts
    {
        private const string BaseUrl = "https://yts.mx";

        private const int RetryCount = 10;

        private const int SaveFrequency = 500;

        private static readonly Action<string> Log = text => Trace.WriteLine(text);

        internal static async Task DownloadSummariesAsync(string jsonPath, int index = 1)
        {
            Dictionary<string, YtsSummary> allSummaries = File.Exists(jsonPath)
                ? JsonSerializer.Deserialize<Dictionary<string, YtsSummary>>(await File.ReadAllTextAsync(jsonPath)) ?? throw new InvalidOperationException(jsonPath)
                : new();
            using WebClient webClient = new();
            for (; ; index++)
            {
                string url = $"{BaseUrl}/browse-movies?page={index}";
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(url), RetryCount);
                Log($"Downloaded {url}");
                CQ cq = new(html);
                if (cq[".browse-movie-wrap"].IsEmpty())
                {
                    break;
                }

                YtsSummary[] summaries = cq
                    .Find(".browse-movie-wrap")
                    .Select(dom =>
                    {
                        CQ cqMovie = new(dom);
                        return new YtsSummary(
                            cqMovie.Find(".browse-movie-title").Text(),
                            cqMovie.Find(".browse-movie-title").Attr("href"),
                            int.TryParse(cqMovie.Find(".browse-movie-year").Text(), out int year) ? year : -1,
                            cqMovie.Find(".img-responsive").Data<string>("cfsrc"),
                            cqMovie.Find(".rating").Text().Replace(" / 10", string.Empty),
                            cqMovie.Find(@"h4[class!=""rating""]").Select(domTag => domTag.TextContent).ToArray());
                    })
                    .ToArray();
                if (summaries.All(summary => allSummaries.ContainsKey(summary.Link)))
                {
                    break;
                }

                summaries.ForEach(summary => allSummaries[summary.Link] = summary);

                if (index % SaveFrequency == 0)
                {
                    string jsonString = JsonSerializer.Serialize(allSummaries, new() { WriteIndented = true });
                    await File.WriteAllTextAsync(jsonPath, jsonString);
                }
            }

            string finalJsonString = JsonSerializer.Serialize(allSummaries, new() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, finalJsonString);
        }

        internal static async Task DownloadDetailsAsync(string summaryJsonPath, string detailJsonPath, int degreeOfParallelism = 4)
        {
            string summaryJsonString = await File.ReadAllTextAsync(summaryJsonPath);
            Dictionary<string, YtsSummary> summaries = JsonSerializer.Deserialize<Dictionary<string, YtsSummary>>(summaryJsonString) ?? throw new InvalidOperationException(summaryJsonPath);

            string detailJsonString = await File.ReadAllTextAsync(detailJsonPath);
            ConcurrentDictionary<string, YtsDetail[]> details = new(JsonSerializer.Deserialize<Dictionary<string, YtsDetail[]>>(detailJsonString) ?? throw new InvalidOperationException(detailJsonPath));
            Dictionary<string, YtsDetail> detailsByLink = details.Values.SelectMany(details => details).ToDictionary(detail => detail.Link, detail => detail);

            int count = 1;

            await summaries.Values.ParallelForEachAsync(async summary =>
            {
                if (detailsByLink.ContainsKey(summary.Link))
                {
                    return;
                }

                using WebClient webClient = new();
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(summary.Link), RetryCount);
                Log($"Downloaded {summary.Link}");
                CQ cq = new(html);
                CQ info = cq.Find("#movie-info");
                YtsDetail detail = new(
                    summary.Title,
                    summary.Link,
                    summary.Year,
                    summary.Image,
                    summary.Rating,
                    summary.Tags,
                    info.Find("a.icon[title='IMDb Rating']").Attr("href"),
                    info.Find("h2 a span").Text().Trim().TrimStart('[').TrimEnd(']'),
                    info.Find("p.hidden-sm a[rel='nofollow']").ToDictionary(link => link.TextContent.Trim(), link => link.GetAttribute("href")));
                lock (AddItemLock)
                {
                    details[detail.ImdbId] = details.ContainsKey(detail.ImdbId)
                        ? details[detail.ImdbId].Where(item => !string.Equals(item.Link, detail.Link, StringComparison.OrdinalIgnoreCase)).Append(detail).ToArray()
                        : new[] { detail };
                }

                if (Interlocked.Increment(ref count) % SaveFrequency == 0)
                {
                    SaveDetail(detailJsonPath, details);
                }
            }, degreeOfParallelism);

            SaveDetail(detailJsonPath, details);
        }

        private static readonly object SaveJsonLock = new();

        private static readonly object AddItemLock = new();

        private static void SaveDetail(string detailJsonPath, IDictionary<string, YtsDetail[]> allDetails)
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
