namespace Examples.IO;

using System.Text.Encodings.Web;
using System.Text.Unicode;
using Examples.Common;
using Examples.Linq;
using Examples.Net;
using OpenQA.Selenium;

internal static partial class Video
{
    internal static void BackupMetadata(string directory, string flag = DefaultBackupFlag, bool overwrite = false)
    {
        Directory
            .GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .ForEach(metadata => File.Copy(metadata, PathHelper.AddFilePostfix(metadata, $"{Delimiter}{flag}"), overwrite));
    }

    internal static void RestoreMetadata(string directory, string flag = DefaultBackupFlag)
    {
        Directory
            .GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .Where(nfo => nfo.EndsWithOrdinal($"{Delimiter}{flag}{XmlMetadataExtension}"))
            .Where(nfo => File.Exists(nfo.Replace($"{Delimiter}{flag}{XmlMetadataExtension}", XmlMetadataExtension)))
            .ForEach(nfo => FileHelper.Move(nfo, Path.Combine(Path.GetDirectoryName(nfo) ?? throw new InvalidOperationException(nfo), (Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException(nfo)).Replace($"{Delimiter}{flag}", string.Empty) + Path.GetExtension(nfo)), true));
    }

    internal static void DeleteFeaturettesMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string featurettes = Path.Combine(movie, Featurettes);
                if (Directory.Exists(featurettes))
                {
                    string[] metadataFiles = Directory.GetFiles(featurettes, XmlMetadataSearchPattern, SearchOption.AllDirectories);
                    metadataFiles
                        .Do(log)
                        .Where(_ => !isDryRun)
                        .ForEach(FileHelper.Delete);
                    // TODO.
                }
            });
    }

    private static void CreateSeasonEpisodeMetadata(string seasonDirectory, Func<string, string>? getTitle = null, Func<string, int, string>? getEpisode = null, Func<string, string>? getSeason = null, bool overwrite = false)
    {
        Directory.GetFiles(seasonDirectory, VideoSearchPattern, SearchOption.TopDirectoryOnly)
            .OrderBy(video => video)
            .ForEach((video, index) =>
            {
                string metadataPath = PathHelper.ReplaceExtension(video, XmlMetadataExtension);
                if (!overwrite && File.Exists(metadataPath))
                {
                    return;
                }

                XDocument metadata = XDocument.Parse("""
                    <?xml version="1.0" encoding="utf-8" standalone="yes"?>
                    <episodedetails>
                      <plot />
                      <outline />
                      <lockdata>false</lockdata>
                      <title></title>
                      <episode></episode>
                      <season></season>
                    </episodedetails>
                    """);
                string title = getTitle?.Invoke(video) ?? string.Empty;
                string episode = getEpisode?.Invoke(video, index) ?? string.Empty;
                string season = getSeason?.Invoke(video) ?? string.Empty;
                if (title.IsNullOrWhiteSpace() || episode.IsNullOrWhiteSpace() || season.IsNullOrWhiteSpace())
                {
                    Match match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"\.S([0-9]+)E([0-9]+)\.(.*)");
                    if (match.Success)
                    {
                        if (season.IsNullOrWhiteSpace())
                        {
                            season = match.Groups[1].Value.TrimStart('0');
                        }

                        if (episode.IsNullOrWhiteSpace())
                        {
                            episode = match.Groups[2].Value.TrimStart('0');
                        }

                        if (title.IsNullOrWhiteSpace())
                        {
                            title = match.Groups[3].Value.Replace("BluRay.", string.Empty).Replace("WEBRip.", string.Empty).Replace("1080p.", string.Empty).Replace("720p.", string.Empty).Replace("ffmpeg.", string.Empty);
                        }
                    }
                }

                if (season.IsNullOrWhiteSpace())
                {
                    season = "1";
                }

                if (episode.IsNullOrWhiteSpace())
                {
                    episode = (index + 1).ToString();
                }

                if (title.IsNullOrWhiteSpace())
                {
                    title = $"Episode {episode}";
                }

                metadata.Root!.Element("title")!.Value = title;
                metadata.Root!.Element("episode")!.Value = episode;
                metadata.Root!.Element("season")!.Value = season;
                metadata.Save(metadataPath);
            });
    }

    internal static void CreateTVEpisodeMetadata(string tvDirectory, Func<string, string>? getTitle = null, Func<string, int, string>? getEpisode = null, Func<string, string>? getSeason = null, bool overwrite = false)
    {
        Directory
            .EnumerateDirectories(tvDirectory)
            .Where(season => !Path.GetFileName(season).EqualsIgnoreCase(Featurettes))
            .OrderBy(season => season)
            .ForEach(season => CreateSeasonEpisodeMetadata(season, getTitle, getEpisode, getSeason, overwrite));
    }

    internal static void WriteRating(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string metadataPath = Directory.GetFiles(movie, XmlMetadataSearchPattern).First();
                XDocument metadata = XDocument.Load(metadataPath);
                XElement? ratingElement = metadata.Root!.Element("rating");
                if (Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata) && imdbMetadata.AggregateRating?.RatingValue is not null)
                {
                    // IMDB has rating.
                    string jsonRatingFormatted = imdbMetadata.AggregateRating.RatingValue.Replace(".0", string.Empty);
                    if ((ratingElement?.Value).EqualsOrdinal(jsonRatingFormatted))
                    {
                        return;
                    }

                    log($"{movie} rating {ratingElement?.Value} is set to {jsonRatingFormatted}.");
                    if (!isDryRun)
                    {
                        metadata.Root!.SetElementValue("rating", jsonRatingFormatted);
                        metadata.Save(metadataPath);
                    }
                }
                else
                {
                    // IMDB has no rating.
                    if (ratingElement is null)
                    {
                        return;
                    }

                    log($"{movie} rating {ratingElement.Value} is removed.");
                    if (!isDryRun)
                    {
                        ratingElement.Remove();
                        metadata.Save(metadataPath);
                    }
                }
            });
    }

    internal static void MoveMetadata(string directory, string cacheDirectory, string metadataDirectory, int level = 2)
    {
        string[] cacheFiles = Directory.GetFiles(cacheDirectory, ImdbCacheSearchPattern);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory, ImdbMetadataSearchPattern);

        EnumerateDirectories(directory, level).ForEach(movie =>
        {
            string[] files = Directory.GetFiles(movie);
            string jsonMetadata = files.FirstOrDefault(file => file.EndsWithIgnoreCase(ImdbMetadataExtension), string.Empty);
            if (jsonMetadata.IsNotNullOrWhiteSpace())
            {
                return;
            }

            string xmlMetadata = files.First(file => file.EndsWithIgnoreCase(XmlMetadataExtension));
            XDocument xmlDocument = XDocument.Load(xmlMetadata);
            string imdbId = xmlDocument.Root?.Element("imdbid")?.Value ?? throw new InvalidOperationException(xmlMetadata);
            cacheFiles
                .Where(file =>
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    return name.EqualsIgnoreCase(imdbId) || name.StartsWithIgnoreCase($"{imdbId}.");
                })
                .ForEach(file => FileHelper.MoveToDirectory(file, movie));

            metadataFiles
                .Where(file => Path.GetFileNameWithoutExtension(file).StartsWithIgnoreCase($"{imdbId}{SubtitleSeparator}"))
                .ForEach(file => FileHelper.MoveToDirectory(file, movie));
        });
    }

    private static async Task DownloadImdbMetadataAsync(string directory, IWebDriver? webDriver, bool overwrite = false, bool useCache = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        string[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
        string[] jsonFiles = files.Where(file => file.EndsWithIgnoreCase(ImdbMetadataExtension)).ToArray();
        //if (jsonFiles.Any())
        //{
        //    if (overwrite)
        //    {
        //        jsonFiles.ForEach(jsonFile =>
        //        {
        //            log($"Delete imdb metadata {jsonFile}.");
        //            File.Delete(jsonFile);
        //        });
        //        files = files.Except(jsonFiles).ToArray();
        //    }
        //    else
        //    {
        //        log($"Skip {directory}.");
        //        return;
        //    }
        //}

        string nfo = files.FirstOrDefault(file => file.EndsWithIgnoreCase(XmlMetadataExtension), string.Empty);
        if (nfo.IsNullOrWhiteSpace())
        {
            log($"!Missing metadata {directory}.");
            return;
        }

        XElement? root = XDocument.Load(nfo).Root;
        string imdbId = (root?.Element("imdbid") ?? root?.Element("imdb_id"))?.Value ?? NotExistingFlag;
        log($"Start {directory}");
        await DownloadImdbMetadataAsync(imdbId, directory, directory, files, jsonFiles, webDriver, overwrite, useCache, log);
    }

    internal static async Task DownloadImdbMetadataAsync(string imdbId, string cacheDirectory, string metadataDirectory, string[] cacheFiles, string[] metadataFiles, IWebDriver? webDriver, bool overwrite = false, bool useCache = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        string[] jsonFiles = metadataFiles.Where(file => file.EndsWithIgnoreCase(ImdbMetadataExtension)).ToArray();
        if (jsonFiles.Any(file => Path.GetFileName(file).StartsWithIgnoreCase(imdbId)))
        {
            if (overwrite)
            {
                jsonFiles.ForEach(jsonFile =>
                {
                    log($"Delete imdb metadata {jsonFile}.");
                    FileHelper.Delete(jsonFile);
                });
            }
            else
            {
                log($"Skip {imdbId}.");
                return;
            }
        }

        string imdbFile = Path.Combine(cacheDirectory, $"{imdbId}{ImdbCacheExtension}");
        string releaseFile = Path.Combine(cacheDirectory, $"{imdbId}.Release{ImdbCacheExtension}");
        string keywordsFile = Path.Combine(cacheDirectory, $"{imdbId}.Keywords{ImdbCacheExtension}");
        string advisoriesFile = Path.Combine(cacheDirectory, $"{imdbId}.Advisories{ImdbCacheExtension}");
        if (imdbId.EqualsOrdinal(NotExistingFlag))
        {
            await new string[] { imdbFile, releaseFile, keywordsFile, advisoriesFile, Path.Combine(cacheDirectory, $"{imdbId}{ImdbMetadataExtension}") }
                .Where(fileToWrite => !cacheFiles.Any(file => file.EqualsIgnoreCase(fileToWrite)) || overwrite)
                .ForEachAsync(async fileToWrite => await File.WriteAllTextAsync(Path.Combine(cacheDirectory, fileToWrite), string.Empty));
            return;
        }

        string parentImdbFile = Path.Combine(cacheDirectory, $"{imdbId}.Parent{ImdbCacheExtension}");
        string parentReleaseFile = Path.Combine(cacheDirectory, $"{imdbId}.Parent.Release{ImdbCacheExtension}");
        string parentKeywordsFile = Path.Combine(cacheDirectory, $"{imdbId}.Parent.Keywords{ImdbCacheExtension}");
        string parentAdvisoriesFile = Path.Combine(cacheDirectory, $"{imdbId}.Parent.Advisories{ImdbCacheExtension}");
        (
            ImdbMetadata imdbMetadata,

            string imdbUrl, string imdbHtml,
            string releaseUrl, string releaseHtml,
            string keywordsUrl, string keywordsHtml,
            string advisoriesUrl, string advisoriesHtml,

            string parentImdbUrl, string parentImdbHtml,
            string parentReleaseUrl, string parentReleaseHtml,
            string parentKeywordsUrl, string parentKeywordsHtml,
            string parentAdvisoriesUrl, string parentAdvisoriesHtml
        ) = await Imdb.DownloadAsync(
            imdbId,
            useCache ? imdbFile : string.Empty,
            useCache ? releaseFile : string.Empty,
            useCache ? keywordsFile : string.Empty,
            useCache ? advisoriesFile : string.Empty,
            useCache ? parentImdbFile : string.Empty,
            useCache ? parentReleaseFile : string.Empty,
            useCache ? parentKeywordsFile : string.Empty,
            useCache ? parentAdvisoriesFile : string.Empty,
            webDriver);
        Debug.Assert(imdbHtml.IsNotNullOrWhiteSpace());
        if (imdbMetadata.Regions.IsEmpty())
        {
            log($"!Location is missing for {imdbId}: {cacheDirectory}");
        }

        await new (string Url, string File, string Html)[]
            {
                (imdbUrl, imdbFile, imdbHtml),
                (releaseFile, releaseFile, releaseHtml),
                (keywordsUrl, keywordsFile, keywordsHtml),
                (advisoriesUrl, advisoriesFile, advisoriesHtml),

                (parentImdbUrl, parentImdbFile, parentImdbHtml),
                (parentReleaseUrl, parentReleaseFile, parentReleaseHtml),
                (parentKeywordsUrl, parentKeywordsFile, parentKeywordsHtml),
                (parentAdvisoriesUrl, parentAdvisoriesFile, parentAdvisoriesHtml),
            }
            .Where(data => data.Html.IsNotNullOrWhiteSpace() && !cacheFiles.Any(file => file.EqualsIgnoreCase(data.File)) || !useCache && overwrite)
            .ForEachAsync(async data =>
            {
                log($"Downloaded {data.Url} to {data.File}.");
                await File.WriteAllTextAsync(data.File, data.Html);
                log($"Saved to {data.File}.");
            });

        string jsonFile = Path.Combine(metadataDirectory, $"{imdbId}{SubtitleSeparator}{imdbMetadata.Year}{SubtitleSeparator}{string.Join(ImdbMetadataSeparator, imdbMetadata.Regions.Select(value => value.Replace(SubtitleSeparator, string.Empty)).Take(5))}{SubtitleSeparator}{string.Join(ImdbMetadataSeparator, imdbMetadata.Languages.Take(3).Select(value => value.Replace(SubtitleSeparator, string.Empty)))}{SubtitleSeparator}{string.Join(ImdbMetadataSeparator, imdbMetadata.Genres.Take(5).Select(value => value.Replace(SubtitleSeparator, string.Empty)))}{ImdbMetadataExtension}");
        log($"Merged {imdbUrl}, {releaseUrl}, {keywordsUrl}, {advisoriesUrl} to {jsonFile}.");
        string jsonContent = JsonSerializer.Serialize(
            imdbMetadata,
            new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
        await File.WriteAllTextAsync(jsonFile, jsonContent);
        log($"Saved to {jsonFile}.");
    }

    internal const string ImdbMetadataSeparator = ",";

    internal static async Task DownloadImdbMetadataAsync(
        (string directory, int level)[] directories, Func<VideoDirectoryInfo, bool> predicate,
        bool overwrite = false, bool useCache = false, bool useBrowser = false, int? degreeOfParallelism = null, Action<string>? log = null)
    {
        string[] movies = directories
            .SelectMany(directory => EnumerateDirectories(directory.directory, directory.level))
            .Where(directory => predicate(VideoDirectoryInfo.Parse(directory)))
            .ToArray();
        if (movies.Any())
        {
            using IWebDriver? webDriver = useBrowser ? WebDriverHelper.StartEdge() : null;
            if (webDriver is not null)
            {
                webDriver.Url = "https://www.imdb.com/";
            }

            await movies.ForEachAsync(async movie => await DownloadImdbMetadataAsync(movie, webDriver, overwrite, useCache, log));
        }
    }

    internal static async Task WriteLibraryMovieMetadata(string jsonPath, Action<string>? log = null, params string[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, Dictionary<string, VideoMetadata>> existingMetadata = File.Exists(jsonPath)
            ? JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(jsonPath))
            ?? throw new InvalidOperationException(jsonPath)
            : new();

        existingMetadata
            .Values
            .ForEach(group => group
                .Keys
                .ToArray()
                .Where(video => !File.Exists(Path.IsPathRooted(video) ? video : Path.Combine(Path.GetDirectoryName(jsonPath) ?? string.Empty, video)))
                .ForEach(video => group.Remove(video)));

        Dictionary<string, string> existingVideos = existingMetadata
            .Values
            .SelectMany(group => group.Keys)
            .ToDictionary(video => video, _ => string.Empty);

        Dictionary<string, Dictionary<string, VideoMetadata>> allVideoMetadata = directories
            .SelectMany(directory => Directory.GetFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .SelectMany(movieJson =>
            {
                string relativePath = Path.GetDirectoryName(jsonPath) ?? string.Empty;
                Imdb.TryLoad(movieJson, out ImdbMetadata? imdbMetadata);
                return Directory
                    .GetFiles(Path.GetDirectoryName(movieJson) ?? string.Empty, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(video => video.IsCommonVideo() && !video.IsDiskImage() && !existingVideos.ContainsKey(Path.GetRelativePath(relativePath, video)))
                    .Select(video =>
                    {
                        if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata, imdbMetadata, relativePath))
                        {
                            log($"!Fail: {video}");
                        }

                        return videoMetadata;
                    })
                    .NotNull();
            })
            .ToLookup(videoMetadata => videoMetadata.Imdb?.ImdbId ?? string.Empty, metadata => metadata)
            .ToDictionary(group => group.Key, group => group.ToDictionary(videoMetadata => videoMetadata.File, videoMetadata => videoMetadata));

        allVideoMetadata.ForEach(
            group =>
            {
                if (!existingMetadata.ContainsKey(group.Key))
                {
                    existingMetadata[group.Key] = new();
                }

                group.Value.ForEach(pair => existingMetadata[group.Key][pair.Key] = pair.Value);
            });

        existingMetadata
            .Keys
            .ToArray()
            .Where(imdbId => existingMetadata[imdbId].IsEmpty())
            .ForEach(imdbId => existingMetadata.Remove(imdbId));

        string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await FileHelper.WriteTextAsync(jsonPath, mergedVideoMetadataJson);
    }

    internal static async Task WriteExternalVideoMetadataAsync(string jsonPath, params string[] directories)
    {
        Dictionary<string, VideoMetadata> allVideoMetadata = directories
            .SelectMany(directory => Directory.GetFiles(directory, VideoSearchPattern, SearchOption.AllDirectories))
            .Select(video =>
            {
                string metadata = PathHelper.ReplaceExtension(video, XmlMetadataExtension);
                string imdbId = XDocument.Load(metadata).Root?.Element("imdbid")?.Value ?? throw new InvalidOperationException(video);
                if (TryReadVideoMetadata(video, out VideoMetadata? videoMetadata))
                {
                    return (ImdbId: imdbId, Value: videoMetadata);
                }

                throw new InvalidOperationException(video);
            })
            .Distinct(metadata => metadata.ImdbId)
            .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value);

        string mergedVideoMetadataJson = JsonSerializer.Serialize(allVideoMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
    }
}