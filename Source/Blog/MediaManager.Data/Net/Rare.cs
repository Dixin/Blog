namespace Examples.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;

internal static class Rare
{
    private static readonly object SaveJsonLock = new();

    private const int SaveFrequency = 100;

    internal static async Task DownloadMetadataAsync(
        string indexUrl,
        string rareJsonPath, string x265JsonPath, string h264JsonPath, string ytsJsonPath, string h264720PJsonPath, string libraryJsonPath,
        Action<string> log,
        int degreeOfParallelism = 4)
    {
        using WebClient webClient = new();
        string indexHtml = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(indexUrl));
        CQ indexCQ = indexHtml;
        string[] links = indexCQ
            .Find("#content li.wsp-post a")
            .Select(link => link.GetAttribute("href"))
            .ToArray();
        int linkCount = links.Length;

        ConcurrentDictionary<string, RareMetadata> rareMetadata = new();
        await links.ParallelForEachAsync(async (link, index) =>
        {
            using WebClient webClient = new();
            try
            {
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(link));
                CQ rareCQ = html;
                CQ rareArticleCQ = rareCQ.Find("#content article");
                string title = rareArticleCQ.Find("h1").Text().Trim();
                log($"{Math.Round((decimal)index * 100 / linkCount)}% {index}/{linkCount} {title} {link}");
                rareMetadata[link] = new RareMetadata(title,
                    rareArticleCQ.Find("div.entry-content").Html());
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                log(exception.ToString());
            }

            if (index % SaveFrequency == 0)
            {
                string jsonString = JsonSerializer.Serialize(rareMetadata, new JsonSerializerOptions() { WriteIndented = true });
                await FileHelper.SaveAndReplaceAsync(rareJsonPath, jsonString, null, SaveJsonLock);
            }
        }, degreeOfParallelism);

        string jsonString = JsonSerializer.Serialize(rareMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await FileHelper.SaveAndReplaceAsync(rareJsonPath, jsonString, null, SaveJsonLock);

        await PrintVersions(rareMetadata, libraryJsonPath, x265JsonPath, h264JsonPath, ytsJsonPath, h264720PJsonPath, log);
    }

    internal static async Task PrintVersions(IDictionary<string, RareMetadata> rareMetadata, string libraryJsonPath, string x265JsonPath, string h264JsonPath, string ytsJsonPath, string h264720PJsonPath, Action<string> log)
    {
        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;
        Dictionary<string, RarbgMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        Dictionary<string, RarbgMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
        Dictionary<string, YtsMetadata[]> ytsMetadata = JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(ytsJsonPath))!;
        Dictionary<string, RarbgMetadata[]> h264720PMetadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264720PJsonPath))!;

        using IWebDriver webDriver = WebDriverHelper.StartEdge();
        await rareMetadata
            .SelectMany(rare => Regex
                .Matches(rare.Value.Content, @"imdb\.com/title/(tt[0-9]+)")
                .Where(match => match.Success)
                .Select(match => (Link: rare.Key, rare.Value.Title, match.Groups[1].Value)))
            .Distinct(imdbId => imdbId.Value)
            .ForEachAsync(async imdbId =>
            {

                if (libraryMetadata.TryGetValue(imdbId.Value, out Dictionary<string, VideoMetadata>? libraryVideos) && libraryVideos.Any())
                {
                    libraryVideos.ForEach(video => log($"- {video.Key} {video.Value.File}"));
                    log(string.Empty);
                    return;
                }

                await Video.DownloadImdbMetadataAsync(imdbId.Value, @"D:\Files\Library\ImdbCache", @"D:\Files\Library\ImdbMetadata", Directory.GetFiles(@"D:\Files\Library\ImdbCache"), Directory.GetFiles(@"D:\Files\Library\ImdbMetadata"), webDriver, false, true, log);

                if (x265Metadata.TryGetValue(imdbId.Value, out RarbgMetadata[]? x265Videos))
                {
                    List<RarbgMetadata> excluded = new();
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

                if (h264Metadata.TryGetValue(imdbId.Value, out RarbgMetadata[]? h264Videos))
                {
                    if (h264Videos.Length > 1)
                    {
                        List<RarbgMetadata> excluded = new();
                        excluded.AddRange(h264Videos.Where(video => !video.Title.EndsWith("-RARBG") && !video.Title.EndsWith("-VXT")));
                        if (excluded.Count == h264Videos.Length)
                        {
                            excluded.Clear();
                        }
                        else
                        {
                            h264Videos = h264Videos.Where(video => video.Title.EndsWith("-RARBG") || video.Title.EndsWith("-VXT")).ToArray();
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

                if (ytsMetadata.TryGetValue(imdbId.Value, out YtsMetadata[]? ytsVideos))
                {
                    YtsMetadata ytsVideo = ytsVideos.Single();
                    if (ytsVideo.Availabilities.Count > 1)
                    {
                        if (ytsVideo.Availabilities.Keys.Any(key => key.ContainsIgnoreCase("1080p")))
                        {
                            ytsVideo
                                .Availabilities.Keys
                                .Where(key => !key.ContainsIgnoreCase("1080p"))
                                .ToArray()
                                .ForEach(key => ytsVideo.Availabilities.Remove(key));
                        }

                        if (ytsVideo.Availabilities.Keys.Any(key => key.ContainsIgnoreCase("BluRay")) && ytsVideo.Availabilities.Keys.Any(key => !key.ContainsIgnoreCase("BluRay")))
                        {
                            ytsVideo
                                .Availabilities.Keys
                                .Where(key => !key.ContainsIgnoreCase("BluRay"))
                                .ToArray()
                                .ForEach(key => ytsVideo.Availabilities.Remove(key));
                        }
                    }

                    log($"https://www.imdb.com/title/{imdbId.Value}/");
                    log($"https://www.imdb.com/title/{imdbId.Value}/parentalguide");
                    log($"{imdbId.Value} {imdbId.Title} {imdbId.Link}");
                    ytsVideo.Availabilities.ForEach(availability => log($"{availability.Value} {ytsVideo.Title} {availability.Key}"));
                    log(string.Empty);
                    return;
                }

                if (h264720PMetadata.TryGetValue(imdbId.Value, out RarbgMetadata[]? h264720PVideos))
                {
                    if (h264720PVideos.Length > 1)
                    {
                        List<RarbgMetadata> excluded = new();
                        excluded.AddRange(h264720PVideos.Where(video => !video.Title.EndsWith("-RARBG") && !video.Title.EndsWith("-VXT")));
                        if (excluded.Count == h264720PVideos.Length)
                        {
                            excluded.Clear();
                        }
                        else
                        {
                            h264720PVideos = h264720PVideos.Where(video => video.Title.EndsWith("-RARBG") || video.Title.EndsWith("-VXT")).ToArray();
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

    internal static async Task PrintVersions(string rareJsonPath, string libraryJsonPath, string x265JsonPath, string h264JsonPath, string ytsJsonPath, string h264720PJsonPath, Action<string> log)
    {
        Dictionary<string, RareMetadata> rareMetadata = JsonSerializer.Deserialize<Dictionary<string, RareMetadata>>(await File.ReadAllTextAsync(rareJsonPath))!;
        await PrintVersions(rareMetadata, libraryJsonPath, x265JsonPath, h264JsonPath, ytsJsonPath, h264720PJsonPath, log);
    }
}