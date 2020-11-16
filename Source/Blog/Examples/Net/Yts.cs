namespace Examples.Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using CsQuery;
    using Examples.Linq;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    internal static class Yts
    {
        private const string BaseUrl = "https://yts.mx";

        private const string RootDirectory = "";

        private static readonly string DocumentsDirectory = Path.Combine(RootDirectory, "Documents");

        private static readonly string ListDirectory = Path.Combine(DocumentsDirectory, "List");

        private static readonly string ListFile = Path.Combine(DocumentsDirectory, "List.json");

        private static readonly string ItemsDirectory = Path.Combine(DocumentsDirectory, "Items");

        private const string HtmlExtension = ".htm";

        private const int RetryCount = 10;

        private static readonly Action<string> Log = text => Trace.WriteLine(text);

        internal static async Task DownloadMovieListAsync(int index = 1)
        {
            for (; ; index++)
            {
                using WebClient webClient = new();
                string url = $"{BaseUrl}/browse-movies?page={index}";
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(url),
                    RetryCount);
                Log($"Downloaded {url}");
                CQ cqHtml = new(html);
                if (cqHtml[".browse-movie-wrap"].IsEmpty())
                {
                    break;
                }

                string file = Path.Combine(ListDirectory, $"browse-movies-{index}{HtmlExtension}");
                await File.WriteAllTextAsync(file, html);
                Log($"Saved {file}");
            }
        }

        internal static async Task ConvertListAsync()
        {
            YtsMovieSummary[] movies = Directory
                .GetFiles(ListDirectory, $"*{HtmlExtension}")
                .Select(CQ.CreateDocumentFromFile)
                .SelectMany(cq => cq.Find(".browse-movie-wrap").Select(dom =>
                    {
                        CQ cqMovie = new(dom);
                        return new YtsMovieSummary(
                            cqMovie.Find(".browse-movie-title").Text(),
                            cqMovie.Find(".browse-movie-title").Attr("href"),
                            int.TryParse(cqMovie.Find(".browse-movie-year").Text(), out int year) ? year : -1,
                            cqMovie.Find(".img-responsive").Data<string>("cfsrc"),
                            cqMovie.Find(".rating").Text().Replace(" / 10", string.Empty),
                            cqMovie.Find(@"h4[class!=""rating""]").Select(domTag => domTag.TextContent).ToArray());
                    }))
                .OrderBy(movie => movie.Title)
                .ThenBy(movie => movie.Year)
                .ToArray();

            string json = JsonSerializer.Serialize(movies, new() { WriteIndented = true });
            await File.WriteAllTextAsync(ListFile, json);
        }

        internal static async Task DownloadItemsAsync()
        {
            string json = await File.ReadAllTextAsync(ListFile);
            YtsMovieSummary[] movies = JsonSerializer.Deserialize<YtsMovieSummary[]>(json)!;
            await movies.ParallelForEachAsync(async movie =>
                {
                    string file = Path.Combine(ItemsDirectory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}{HtmlExtension}");
                    if (!File.Exists(file))
                    {
                        using WebClient webClient = new();
                        string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(movie.Link), RetryCount);
                        Log($"Downloaded {movie.Link}");
                        await File.WriteAllTextAsync(file, html);
                        Log($"Saved {file}");
                    }
                });
        }

        internal static async Task MoveItems(string directory)
        {
            string json = await File.ReadAllTextAsync(ListFile);
            YtsMovieSummary[] movies = JsonSerializer.Deserialize<YtsMovieSummary[]>(json)!;
            movies.ForEach(movie =>
                {
                    string file = Path.Combine(ItemsDirectory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}{HtmlExtension}");
                    if (File.Exists(file))
                    {
                        string file3 = Path.Combine(directory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}.html");
                        if (File.Exists(file3))
                        {
                            File.Delete(file3);
                            return;
                        }
                        return;
                    }
                    string file2 = Path.Combine(ItemsDirectory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}{HtmlExtension}l");
                    if (File.Exists(file2))
                    {
                        string file3 = Path.Combine(directory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}.html");
                        if (File.Exists(file3))
                        {
                            File.Delete(file3);
                            return;
                        }
                        return;
                    }

                    Trace.WriteLine(movie.Link);
                });
        }

        internal static async Task SaveLinkList()
        {
            string json = await File.ReadAllTextAsync(ListFile);
            YtsMovieSummary[] movies = JsonSerializer.Deserialize<YtsMovieSummary[]>(json)!;
            IEnumerable<string> links = movies
                .Where(movie => !File.Exists(Path.Combine(ItemsDirectory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}{HtmlExtension}l")))
                .Where(movie => !File.Exists(Path.Combine(ItemsDirectory, $"{Path.GetFileName(new Uri(movie.Link).LocalPath)}{HtmlExtension}")))
                .Select(movie => movie.Link)
                .ToArray();
            string text = string.Join(Environment.NewLine, links);
            await File.WriteAllTextAsync(Path.Combine(DocumentsDirectory, "Links.txt"), text);
        }

        private static readonly string ImdbDirectory = Path.Combine(DocumentsDirectory, "Imdb");

        private const string DefaultContentRating = "-";

        private const string JsonExtension = ".json";

        internal static async Task DownloadImdbAsync()
        {
            string[] imdbFiles = Directory.GetFiles(ImdbDirectory, $"*{JsonExtension}").Select(file => Path.GetFileNameWithoutExtension(file)!).OrderBy(file => file).ToArray();
            await Directory
                .GetFiles(ItemsDirectory)
                .ForEachAsync(async file =>
                {
                    CQ cqPage = CQ.CreateDocumentFromFile(file);
                    string url = cqPage.Find(@"a.icon[title=""IMDb Rating""]").Attr<string>("href");
                    string imdbId = Path.GetFileName(new Uri(url).LocalPath.TrimEnd('/'));
                    if (imdbFiles.Any(existingFile => existingFile.Split(".").First().Equals(Path.GetFileNameWithoutExtension(file))))
                    {
                        return;
                    }

                    using WebClient webClient = new();
                    string imdbHtml = await webClient.DownloadStringTaskAsync(url);
                    CQ cqImdb = new(imdbHtml);
                    string imdbJson = cqImdb.Find(@"script[type=""application/ld+json""]").Text();
                    JsonDocument document = JsonDocument.Parse(imdbJson);
                    string contentRating = document.RootElement.TryGetProperty("contentRating", out JsonElement ratingElement)
                        ? ratingElement.GetString()!
                        : DefaultContentRating;
                    if (string.IsNullOrWhiteSpace(contentRating))
                    {
                        contentRating = DefaultContentRating;
                    }
                    string imdbFile = Path.Combine(ImdbDirectory, $"{Path.GetFileNameWithoutExtension(file)}.{imdbId}.{contentRating}{HtmlExtension}");
                    await File.WriteAllTextAsync(imdbFile, imdbHtml);
                    string jsonFile = Path.Combine(ImdbDirectory, $"{Path.GetFileNameWithoutExtension(file)}.{imdbId}.{contentRating}{JsonExtension}");
                    await File.WriteAllTextAsync(jsonFile, imdbJson);
                });
        }

        internal static async Task SaveImdbSpecialTitles()
        {
            string[] specialTitles = File.ReadLines(Path.Combine(ImdbDirectory, "title.basics.tsv"))
                .Skip(1)
                .Select(line => line.Split('\t'))
                .Where(line => !"0".Equals(line.ElementAtOrDefault(4), StringComparison.Ordinal))
                .Select(line => line[0])
                .ToArray();
            string json = JsonSerializer.Serialize(specialTitles, new() { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(DocumentsDirectory, $"ImdbSpecialTitles{JsonExtension}"), json);
        }

        internal static async Task SaveYtsSpecialTitles()
        {
            string[] ytsTitles = Directory
                .GetFiles(ItemsDirectory)
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
            string json = JsonSerializer.Serialize(ytsTitles, new() { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(DocumentsDirectory, $"YtsTitles{JsonExtension}"), json);
        }

        internal static async Task PrintSpecialTitles()
        {
            string jsonYts = await File.ReadAllTextAsync(Path.Combine(DocumentsDirectory, $"YtsTitles{JsonExtension}"));
            string[] yts = JsonSerializer.Deserialize<string[]>(jsonYts)!;
            string jsonImdb = await File.ReadAllTextAsync(Path.Combine(DocumentsDirectory, $"ImdbSpecialTitles{JsonExtension}"));
            string[] imdb = JsonSerializer.Deserialize<string[]>(jsonImdb)!.OrderBy(imdbid => imdbid).ToArray();
            yts
                .Where(imdbid => Array.BinarySearch(imdb, imdbid) >= 0)
                .ForEach(imdbid => Trace.WriteLine(imdbid));
        }
    }
}
