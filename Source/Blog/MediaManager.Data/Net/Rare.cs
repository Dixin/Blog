namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.Linq;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Rare
{
    private const int WriteCount = 100;

    internal static async Task DownloadMetadataAsync(ISettings settings, string indexUrl, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= Video.IOMaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        using HttpClient httpClient = new();
        string indexHtml = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(indexUrl, cancellationToken), cancellationToken: cancellationToken);
        CQ indexCQ = indexHtml;
        string[] links = indexCQ
            .Find("#content li.wsp-post a")
            .Select(link => link.GetAttribute("href"))
            .ToArray();
        int linkCount = links.Length;
        log($"Total: {linkCount}");
        ConcurrentDictionary<string, RareMetadata> rareMetadata = new();
        object writeJsonLock = new();
        await links.ParallelForEachAsync(
            async (link, index, token) =>
            {
                token.ThrowIfCancellationRequested();
                using HttpClient httpClient = new();
                try
                {
                    string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(link, token), cancellationToken: token);
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
                    JsonHelper.SerializeToFile(rareMetadata, settings.MovieRareMetadata, ref writeJsonLock);
                }
            },
            degreeOfParallelism,
            cancellationToken);

        JsonHelper.SerializeToFile(rareMetadata, settings.MovieRareMetadata, ref writeJsonLock);

        await PrintVersionsAsync(settings, rareMetadata, log, cancellationToken);
    }

    internal static async Task PrintVersionsAsync(ISettings settings, IDictionary<string, RareMetadata> rareMetadata, Action<string>? log = null, CancellationToken cancellationToken = default, params string[] categories)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> libraryMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Metadata = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264720PMetadata = await settings.LoadMovieTopH264720PMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredMetadata>> preferredMetadata = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

        string[] cacheFiles = Directory.GetFiles(settings.MovieMetadataCacheDirectory);
        string[] metadataFiles = Directory.GetFiles(settings.MovieMetadataDirectory);
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
        using WebDriverWrapper webDriver = new();
        await imdbIds
            .OrderBy(imdbId => imdbId.Value)
            .ForEachAsync(async (imdbId, index) =>
            {
                log($"{index * 100 / length}% - {index}/{length} - {imdbId}");
                if (libraryMetadata.TryGetValue(imdbId.Value, out ConcurrentDictionary<string, VideoMetadata>? libraryVideos) && libraryVideos.Any())
                {
                    libraryVideos.ForEach(video => log($"- {video.Key} {video.Value.File}"));
                    log(string.Empty);
                    return;
                }

                try
                {
                    await Retry.FixedIntervalAsync(
                        async () => await Video.DownloadImdbMetadataAsync(imdbId.Value, settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, metadataFiles, cacheFiles, webDriver, overwrite: false, useCache: true, log: log, cancellationToken: cancellationToken),
                        isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.InternalServerError }, cancellationToken: cancellationToken);
                }
                catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
                {
                    log($"!!!{imdbId} {exception.ToString()}");
                    if (imdbId.Value.StartsWithIgnoreCase("tt0"))
                    {
                        imdbId = imdbId with { Value = imdbId.Value.ReplaceIgnoreCase("tt0", "tt") };
                        await Video.DownloadImdbMetadataAsync(imdbId.Value, @settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, metadataFiles, cacheFiles, webDriver, overwrite: false, useCache: true, log: log, cancellationToken: cancellationToken);
                    }
                }

                if (x265Metadata.TryGetValue(imdbId.Value, out TopMetadata[]? x265Videos))
                {
                    List<TopMetadata> excluded = [];
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
                        List<TopMetadata> excluded = [];
                        excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")));
                        if (excluded.Count == h264Videos.Length)
                        {
                            excluded.Clear();
                        }
                        else
                        {
                            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
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

                if (preferredMetadata.TryGetValue(imdbId.Value, out List<PreferredMetadata>? preferredVideos))
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
                        List<TopMetadata> excluded = [];
                        excluded.AddRange(h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")));
                        if (excluded.Count == h264720PVideos.Length)
                        {
                            excluded.Clear();
                        }
                        else
                        {
                            h264720PVideos = h264720PVideos.Where(video => video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                        }
                    }

                    log($"https://www.imdb.com/title/{imdbId.Value}/");
                    log($"https://www.imdb.com/title/{imdbId.Value}/parentalguide");
                    log($"{imdbId.Value} {imdbId.Title} {imdbId.Link}");
                    h264720PVideos.ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                }
            }, cancellationToken: cancellationToken);
    }

    internal static async Task PrintVersionsAsync(ISettings settings, Action<string>? log = null)
    {
        Dictionary<string, RareMetadata> rareMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, RareMetadata>>(settings.MovieRareMetadata);
        await PrintVersionsAsync(settings, rareMetadata, log);
    }
}