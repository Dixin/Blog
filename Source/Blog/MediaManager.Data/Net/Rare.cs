namespace Examples.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;

internal static class Rare
{
    private static readonly object WriteJsonLock = new();

    private const int WriteCount = 100;

    internal static async Task DownloadMetadataAsync(
        string indexUrl,
        string rareJsonPath, string x265JsonPath, string h264JsonPath, string preferredJsonPath, string h264720PJsonPath, string libraryJsonPath,
        string metadataDirectory, string cacheDirectory, 
        int degreeOfParallelism = 4, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        using HttpClient httpClient = new();
        string indexHtml = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(indexUrl));
        CQ indexCQ = indexHtml;
        string[] links = indexCQ
            .Find("#content li.wsp-post a")
            .Select(link => link.GetAttribute("href"))
            .ToArray();
        int linkCount = links.Length;
        log($"Total: {linkCount}");
        ConcurrentDictionary<string, RareMetadata> rareMetadata = new();
        await links.ParallelForEachAsync(async (link, index) =>
        {
            using HttpClient httpClient = new();
            try
            {
                string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(link));
                CQ rareCQ = html;
                CQ rareArticleCQ = rareCQ.Find("#content article");
                string title = rareArticleCQ.Find("h1").Text().Trim();
                log($"{Math.Round((decimal)index * 100 / linkCount)}% {index}/{linkCount} {title} {link}");
                rareMetadata[link] = new RareMetadata(title,
                    rareArticleCQ.Html());
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                log(exception.ToString());
            }

            if (index % WriteCount == 0)
            {
                string jsonString = JsonSerializer.Serialize(rareMetadata, new JsonSerializerOptions() { WriteIndented = true });
                FileHelper.WriteText(rareJsonPath, jsonString, null, WriteJsonLock);
            }
        }, degreeOfParallelism);

        string jsonString = JsonSerializer.Serialize(rareMetadata, new JsonSerializerOptions() { WriteIndented = true });
        FileHelper.WriteText(rareJsonPath, jsonString, null, WriteJsonLock);

        await PrintVersionsAsync(rareMetadata, libraryJsonPath, x265JsonPath, h264JsonPath, preferredJsonPath, h264720PJsonPath, metadataDirectory, cacheDirectory, log);
    }

    internal static async Task PrintVersionsAsync(IDictionary<string, RareMetadata> rareMetadata, string libraryJsonPath, string x265JsonPath, string h264JsonPath, string preferredJsonPath, string h264720PJsonPath, string metadataDirectory, string cacheDirectory, Action<string>? log = null, params string[] categories)
    {
        log ??= Logger.WriteLine;

        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;
        Dictionary<string, TopMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        Dictionary<string, TopMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
        Dictionary<string, PreferredMetadata[]> preferredMetadata = JsonSerializer.Deserialize<Dictionary<string, PreferredMetadata[]>>(await File.ReadAllTextAsync(preferredJsonPath))!;
        Dictionary<string, TopMetadata[]> h264720PMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264720PJsonPath))!;

        string[] cacheFiles = Directory.GetFiles(cacheDirectory);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory);
        (string Link, string Title, string Value, string[] Categories)[] imdbIds = rareMetadata
            .AsParallel()
            .SelectMany(rare => Regex
                .Matches(rare.Value.Content, @"imdb\.com/title/(tt[0-9]+)")
                .Where(match => match.Success)
                .Select(match => (rare.Key, rare.Value.Title, match.Groups[1].Value, Categories: new CQ(rare.Value.Content).Find("span.bl_categ a").Select(category => category.Cq().Text().Trim()).ToArray())))
            .Where(imdbId => imdbId.Value.IsNotNullOrWhiteSpace() && imdbId.Categories.Intersect(categories, StringComparer.OrdinalIgnoreCase).Any())
            .AsSequential()
            .Distinct(imdbId => imdbId.Value)
            .ToArray();
        int length = imdbIds.Length;
        using IWebDriver webDriver = WebDriverHelper.Start();
        await imdbIds
            .OrderBy(imdbId => imdbId.Value)
            .ForEachAsync(async (imdbId, index) =>
            {
                log($"{index * 100 / length}% - {index}/{length} - {imdbId}");
                if (libraryMetadata.TryGetValue(imdbId.Value, out Dictionary<string, VideoMetadata>? libraryVideos) && libraryVideos.Any())
                {
                    libraryVideos.ForEach(video => log($"- {video.Key} {video.Value.File}"));
                    log(string.Empty);
                    return;
                }

                try
                {
                    await Video.DownloadImdbMetadataAsync(imdbId.Value, @"D:\Files\Library\ImdbCache", @"D:\Files\Library\ImdbMetadata", cacheFiles, metadataFiles, webDriver, false, true, log);
                }
                catch (ArgumentOutOfRangeException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    if (imdbId.Value.StartsWithIgnoreCase("tt0"))
                    {
                        imdbId = imdbId with { Value = imdbId.Value.ReplaceIgnoreCase("tt0", "tt") };
                        await Video.DownloadImdbMetadataAsync(imdbId.Value, @"D:\Files\Library\ImdbCache", @"D:\Files\Library\ImdbMetadata", cacheFiles, metadataFiles, webDriver, false, true, log);
                    }
                }

                if (x265Metadata.TryGetValue(imdbId.Value, out TopMetadata[]? x265Videos))
                {
                    List<TopMetadata> excluded = new();
                    if (x265Videos.Length > 1)
                    {
                        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && x265Videos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                        {
                            excluded.AddRange(x265Videos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                            x265Videos = x265Videos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                        }

                        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".FRENCH.")) && x265Videos.Any(video => video.Title.ContainsIgnoreCase(".DUBBED.")))
                        {
                            excluded.AddRange(x265Videos.Where(video => video.Title.ContainsIgnoreCase(".DUBBED.")));
                            x265Videos = x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".DUBBED.")).ToArray();
                        }

                        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".EXTENDED.")) && x265Videos.Any(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")))
                        {
                            excluded.AddRange(x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")));
                            x265Videos = x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")).ToArray();
                        }

                        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".REMASTERED.")) && x265Videos.Any(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")))
                        {
                            excluded.AddRange(x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")));
                            x265Videos = x265Videos.Where(video => video.Title.ContainsIgnoreCase(".REMASTERED.")).ToArray();
                        }

                        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".PROPER.")) && x265Videos.Any(video => !video.Title.ContainsIgnoreCase(".PROPER.")))
                        {
                            excluded.AddRange(x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".PROPER.")));
                            x265Videos = x265Videos.Where(video => video.Title.ContainsIgnoreCase(".PROPER.")).ToArray();
                        }
                    }

                    log($"https://www.imdb.com/title/{imdbId.Value}/");
                    log($"https://www.imdb.com/title/{imdbId.Value}/parentalguide");
                    log($"{imdbId.Value} {imdbId.Title} {imdbId.Link}");
                    x265Videos.ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                    return;
                }

                if (h264Metadata.TryGetValue(imdbId.Value, out TopMetadata[]? h264Videos))
                {
                    if (h264Videos.Length > 1)
                    {
                        List<TopMetadata> excluded = new();
                        excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") && !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}")));
                        if (excluded.Count == h264Videos.Length)
                        {
                            excluded.Clear();
                        }
                        else
                        {
                            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") || video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}")).ToArray();
                        }

                        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && h264Videos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                        {
                            excluded.AddRange(h264Videos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                        }

                        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && h264Videos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                        {
                            excluded.AddRange(h264Videos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                        }

                        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".FRENCH.")) && h264Videos.Any(video => video.Title.ContainsIgnoreCase(".DUBBED.")))
                        {
                            excluded.AddRange(h264Videos.Where(video => video.Title.ContainsIgnoreCase(".DUBBED.")));
                            h264Videos = h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".DUBBED.")).ToArray();
                        }

                        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".EXTENDED.")) && h264Videos.Any(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")))
                        {
                            excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")));
                            h264Videos = h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")).ToArray();
                        }

                        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".REMASTERED.")) && h264Videos.Any(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")))
                        {
                            excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")));
                            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase(".REMASTERED.")).ToArray();
                        }

                        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".PROPER.")) && h264Videos.Any(video => !video.Title.ContainsIgnoreCase(".PROPER.")))
                        {
                            excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".PROPER.")));
                            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase(".PROPER.")).ToArray();
                        }
                    }

                    log($"https://www.imdb.com/title/{imdbId.Value}/");
                    log($"https://www.imdb.com/title/{imdbId.Value}/parentalguide");
                    log($"{imdbId.Value} {imdbId.Title} {imdbId.Link}");
                    h264Videos.ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                    return;
                }

                if (preferredMetadata.TryGetValue(imdbId.Value, out PreferredMetadata[]? preferredVideos))
                {
                    PreferredMetadata preferredVideo = preferredVideos.Single();
                    if (preferredVideo.Availabilities.Count > 1)
                    {
                        if (preferredVideo.Availabilities.Keys.Any(key => key.ContainsIgnoreCase("1080p")))
                        {
                            preferredVideo
                                .Availabilities.Keys
                                .Where(key => !key.ContainsIgnoreCase("1080p"))
                                .ToArray()
                                .ForEach(key => preferredVideo.Availabilities.Remove(key));
                        }

                        if (preferredVideo.Availabilities.Keys.Any(key => key.ContainsIgnoreCase("BluRay")) && preferredVideo.Availabilities.Keys.Any(key => !key.ContainsIgnoreCase("BluRay")))
                        {
                            preferredVideo
                                .Availabilities.Keys
                                .Where(key => !key.ContainsIgnoreCase("BluRay"))
                                .ToArray()
                                .ForEach(key => preferredVideo.Availabilities.Remove(key));
                        }
                    }

                    log($"https://www.imdb.com/title/{imdbId.Value}/");
                    log($"https://www.imdb.com/title/{imdbId.Value}/parentalguide");
                    log($"{imdbId.Value} {imdbId.Title} {imdbId.Link}");
                    preferredVideo.Availabilities.ForEach(availability => log($"{availability.Value} {preferredVideo.Title} {availability.Key}"));
                    log(string.Empty);
                    return;
                }

                if (h264720PMetadata.TryGetValue(imdbId.Value, out TopMetadata[]? h264720PVideos))
                {
                    if (h264720PVideos.Length > 1)
                    {
                        List<TopMetadata> excluded = new();
                        excluded.AddRange(h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") && !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}")));
                        if (excluded.Count == h264720PVideos.Length)
                        {
                            excluded.Clear();
                        }
                        else
                        {
                            h264720PVideos = h264720PVideos.Where(video => video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") || video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}")).ToArray();
                        }
                    }

                    log($"https://www.imdb.com/title/{imdbId.Value}/");
                    log($"https://www.imdb.com/title/{imdbId.Value}/parentalguide");
                    log($"{imdbId.Value} {imdbId.Title} {imdbId.Link}");
                    h264720PVideos.ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                }
            });
    }

    internal static async Task PrintVersionsAsync(string rareJsonPath, string libraryJsonPath, string x265JsonPath, string h264JsonPath, string preferredJsonPath, string h264720PJsonPath, string metadataDirectory, string cacheDirectory, Action<string>? log = null)
    {
        Dictionary<string, RareMetadata> rareMetadata = JsonSerializer.Deserialize<Dictionary<string, RareMetadata>>(await File.ReadAllTextAsync(rareJsonPath))!;
        await PrintVersionsAsync(rareMetadata, libraryJsonPath, x265JsonPath, h264JsonPath, preferredJsonPath, h264720PJsonPath, metadataDirectory, cacheDirectory, log);
    }
}