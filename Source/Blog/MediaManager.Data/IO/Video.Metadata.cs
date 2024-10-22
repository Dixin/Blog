namespace MediaManager.IO;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using Examples.Text;
using MediaManager.Net;

internal static partial class Video
{
    internal static void BackupMetadata(string directory, string flag = DefaultBackupFlag, bool overwrite = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .Where(metadata => !PathHelper.GetFileNameWithoutExtension(metadata).EndsWithIgnoreCase($"{Delimiter}{flag}"))
            .Select(metadata => (Metadata: metadata, Backup: PathHelper.AddFilePostfix(metadata, $"{Delimiter}{flag}")))
            .Where(metadata => overwrite || !File.Exists(metadata.Backup))
            .Do(metadata => log(metadata.Backup))
            .ForEach(metadata => File.Copy(metadata.Metadata, metadata.Backup, overwrite));
    }

    internal static void RestoreMetadata(string directory, string flag = DefaultBackupFlag)
    {
        Directory
            .GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .Where(file => file.EndsWithOrdinal($"{Delimiter}{flag}{XmlMetadataExtension}"))
            .Where(file => File.Exists(file.Replace($"{Delimiter}{flag}{XmlMetadataExtension}", XmlMetadataExtension)))
            .ForEach(file => FileHelper.Move(file, Path.Combine(PathHelper.GetDirectoryName(file), $"{PathHelper.GetFileNameWithoutExtension(file).Replace($"{Delimiter}{flag}", string.Empty)}{PathHelper.GetExtension(file)}"), true));
    }

    internal static void DeleteFeaturettesMetadata(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
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
                    Match match = Regex.Match(PathHelper.GetFileNameWithoutExtension(video), @"\.S([0-9]+)E([0-9]+)\.(.*)");
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
                            title = match.Groups[3].Value.Replace("BluRay.", string.Empty).Replace("WEBRip.", string.Empty).Replace("1080p.", string.Empty).Replace("720p.", string.Empty).Replace($"{FfmpegHelper.Executable}{Delimiter}", string.Empty);
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
            .Where(season => !PathHelper.GetFileName(season).EqualsIgnoreCase(Featurettes))
            .OrderBy(season => season)
            .ForEach(season => CreateSeasonEpisodeMetadata(season, getTitle, getEpisode, getSeason, overwrite));
    }

    internal static void UpdateXmlRating(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata);
                string? jsonRatingFormatted = imdbMetadata?.AggregateRating?.RatingValue.Replace(".0", string.Empty).Trim();
                Directory
                    .GetFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly)
                    .ForEach(xmlMetadataFile =>
                    {
                        XDocument metadata = XDocument.Load(xmlMetadataFile);
                        XElement? ratingElement = metadata.Root!.Element("rating");
                        if (jsonRatingFormatted is null)
                        {
                            if (ratingElement is null)
                            {
                                return;
                            }

                            log($"Rating {ratingElement.Value} is removed for {movie}.");
                            if (!isDryRun)
                            {
                                ratingElement.Remove();
                                metadata.Save(xmlMetadataFile);
                            }
                        }
                        else
                        {
                            if (jsonRatingFormatted.EqualsOrdinal(ratingElement?.Value))
                            {
                                return;
                            }

                            log($"Rating {ratingElement?.Value} is set to {jsonRatingFormatted} for {movie}.");
                            if (!isDryRun)
                            {
                                metadata.Root!.SetElementValue("rating", jsonRatingFormatted);
                                metadata.Save(xmlMetadataFile);
                            }
                        }
                    });
            });
    }

    internal static void MoveMetadata(string directory, string cacheDirectory, string metadataDirectory, int level = DefaultDirectoryLevel)
    {
        string[] cacheFiles = Directory.GetFiles(cacheDirectory, ImdbCacheSearchPattern);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory, ImdbMetadataSearchPattern);

        EnumerateDirectories(directory, level).ForEach(movie =>
        {
            string[] files = Directory.GetFiles(movie);
            if (!ImdbMetadata.TryGet(files, out string? _, out string? imdbId))
            {
                return;
            }

            if (!files
                    .Where(IsXmlMetadata)
                    .All(xmlMetadata => xmlMetadata.TryLoadXmlImdbId(out string? xmlImdbId) && imdbId.EqualsIgnoreCase(xmlImdbId)))
            {
                return;
            }

            cacheFiles
                .Where(file =>
                {
                    string name = PathHelper.GetFileNameWithoutExtension(file);
                    return name.EqualsIgnoreCase(imdbId) || name.StartsWithIgnoreCase($"{imdbId}{Delimiter}");
                })
                .ToArray()
                .ForEach(file => FileHelper.MoveToDirectory(file, movie));

            metadataFiles
                .Where(file => file.HasImdbId(imdbId))
                .ToArray()
                .ForEach(file => FileHelper.MoveToDirectory(file, movie));
        });
    }

    private static async Task<bool> DownloadImdbMetadataAsync(string directory, WebDriverWrapper? webDriver, bool overwrite = false, bool useCache = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        string[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
        string jsonFile = files.SingleOrDefault(IsImdbMetadata, string.Empty);
        string imdbId;
        if (jsonFile.IsNotNullOrWhiteSpace())
        {
            if (ImdbMetadata.TryGet(jsonFile, out string? jsonImdbId))
            {
                imdbId = jsonImdbId;
            }
            else
            {
                log($"Skip {directory}.");
                return false;
            }
        }
        else
        {
            string[] xmlImdbIds = files
                .Where(IsXmlMetadata)
                .Select(file => file.TryLoadXmlImdbId(out string? xmlImdbId) ? xmlImdbId : string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (xmlImdbIds.Length == 0)
            {
                log($"!No JSON or XML metadata in {directory}.");
                return false;
            }

            if (xmlImdbIds.Length > 1)
            {
                log($"!Inconsistent IMDB ids {string.Join(", ", xmlImdbIds)} in {directory}.");
                return false;
            }

            imdbId = xmlImdbIds.Single();
            if (!imdbId.IsImdbId())
            {
                imdbId = NotExistingFlag;
            }
        }

        log($"Start {directory}");
        return await DownloadImdbMetadataAsync(imdbId, directory, directory, [jsonFile], files, webDriver, overwrite, useCache, log, cancellationToken);
    }

    internal static async Task<bool> DownloadImdbMetadataAsync(string imdbId, string metadataDirectory, string cacheDirectory, string[] metadataFiles, string[] cacheFiles, WebDriverWrapper? webDriver, bool overwrite = false, bool useCache = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        long startTime = Stopwatch.GetTimestamp();
        if (metadataFiles.Select(PathHelper.GetFileNameWithoutExtension).SequenceEqual([NotExistingFlag], StringComparer.OrdinalIgnoreCase))
        {
            Debug.Assert(imdbId.EqualsOrdinal(NotExistingFlag));
            log($"Skip {imdbId}.");
            return false;
        }

        string imdbFile = Path.Combine(cacheDirectory, $"{imdbId}{ImdbCacheExtension}");
        string releaseFile = Path.Combine(cacheDirectory, $"{imdbId}.Release{ImdbCacheExtension}");
        string keywordsFile = Path.Combine(cacheDirectory, $"{imdbId}.Keywords{ImdbCacheExtension}");
        string advisoriesFile = Path.Combine(cacheDirectory, $"{imdbId}.Advisories{ImdbCacheExtension}");

        if (imdbId.EqualsOrdinal(NotExistingFlag))
        {
            await new string[] { imdbFile, releaseFile, keywordsFile, advisoriesFile }
                .Where(file => !cacheFiles.ContainsIgnoreCase(file))
                .ForEachAsync(async file => await File.WriteAllTextAsync(file, string.Empty, cancellationToken), cancellationToken);
            string notExistingJsonFile = Path.Combine(metadataDirectory, $"{NotExistingFlag}{ImdbMetadata.Extension}");
            if (!File.Exists(notExistingJsonFile))
            {
                await File.WriteAllTextAsync(notExistingJsonFile, string.Empty, cancellationToken);
            }

            return false;
        }

        string jsonFile = metadataFiles.SingleOrDefault(file => file.HasImdbId(imdbId), string.Empty);
        if (jsonFile.IsNotNullOrWhiteSpace() && (!overwrite || IsLatestVersion(jsonFile)))
        {
            log($"Skip {imdbId}.");
            return false;
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
            webDriver, cancellationToken);
        Debug.Assert(imdbHtml.IsNotNullOrWhiteSpace());
        if (imdbMetadata.Regions.IsEmpty())
        {
            log($"!Location is missing for {imdbId}: {cacheDirectory}");
        }

        bool isDownloaded = false;
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
            .Where(data => data.Html.IsNotNullOrWhiteSpace() && (!useCache && overwrite || !cacheFiles.Any(file => file.EqualsIgnoreCase(data.File))))
            .ForEachAsync(
                async data =>
                {
                    isDownloaded = true;
                    log($"Downloaded {data.Url} to {data.File}.");
                    await File.WriteAllTextAsync(data.File, data.Html, cancellationToken);
                    log($"Saved to {data.File}.");
                },
                cancellationToken);

        string newJsonFile = imdbMetadata.GetFilePath(metadataDirectory);
        if (jsonFile.IsNotNullOrWhiteSpace() && !jsonFile.EqualsIgnoreCase(newJsonFile))
        {
            FileHelper.Recycle(jsonFile);
        }

        log($"Merged {imdbUrl}, {releaseUrl}, {keywordsUrl}, {advisoriesUrl} to {newJsonFile}.");
        await JsonHelper.SerializeToFileAsync(imdbMetadata, newJsonFile, cancellationToken);
        log($"Saved to {newJsonFile}.");
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
        log($"Elapsed {elapsed}");

        return isDownloaded;
    }

    internal static async Task DownloadImdbMetadataAsync(
        (string directory, int level)[] directories, Func<VideoDirectoryInfo, bool> predicate,
        bool overwrite = false, bool useCache = false, bool useBrowser = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        string[] movies = directories
            .SelectMany(directory => EnumerateDirectories(directory.directory, directory.level))
            .Where(directory => predicate(VideoDirectoryInfo.Parse(directory)))
            .ToArray();
        if (movies.Any())
        {
            using WebDriverWrapper? webDriver = useBrowser ? new() : null;
            if (webDriver is not null)
            {
                webDriver.Url = "https://www.imdb.com/";
            }

            await movies.ForEachAsync(async movie => await DownloadImdbMetadataAsync(movie, webDriver, overwrite, useCache, log, cancellationToken), cancellationToken);
        }
    }

    internal static async Task WriteLibraryMovieMetadataAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params string[][] directoryDrives)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);

        existingMetadata
            .Values
            .SelectMany(group => group.Keys, (group, video) => (group, video))
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .Where(video => !File.Exists(Path.IsPathRooted(video.video) ? video.video : Path.Combine(settings.LibraryDirectory, video.video)))
            .ForAll(video =>
            {
                log($"Delete {video.video}.");
                Debug.Assert(video.group.TryRemove(video.video, out _));
            });

        HashSet<string> existingVideos = new(existingMetadata.Values.SelectMany(group => group.Keys), StringComparer.OrdinalIgnoreCase);
        directoryDrives
            // Hard drives in parallel.
            .AsParallel()
            .ForAll(driveDirectories =>
            {
                ConcurrentDictionary<string, string> allMetadataFiles = new();
                ConcurrentDictionary<string, ConcurrentBag<string>> newVideos = new();

                driveDirectories
                    // IO per hard drive, sequential.
                    .SelectMany(directory => Directory.EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories))
                    // No IO involved.
                    .AsParallel()
                    .ForAll(file =>
                    {
                        string directory = PathHelper.GetDirectoryName(file);
                        if (file.IsImdbMetadata())
                        {
                            allMetadataFiles[directory] = file;
                            return;
                        }

                        if (file.IsVideo() && !file.IsDiskImage() && !existingVideos.Contains(Path.GetRelativePath(settings.LibraryDirectory, file)))
                        {

                            if (Path.GetFileName(directory).EqualsIgnoreCase(Featurettes))
                            {
                                return;
                            }

                            newVideos.AddOrUpdate(
                                directory,
                                key => [file],
                                (key, videos) =>
                                {
                                    videos.Add(file);
                                    return videos;
                                });
                        }
                    });

                Debug.Assert(newVideos.Values.All(group => group.Any()));
                Dictionary<string, Dictionary<string, VideoMetadata?>> newVideoMetadata = newVideos
                    .AsParallel()
                    .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
                    .Select(group =>
                    {
                        ImdbMetadata? imdbMetadata = null;
                        if (!allMetadataFiles.TryGetValue(group.Key, out string? metadataFile))
                        {
                            log($"!Metadata is missing for {group.Key}.");
                        }
                        else if (!ImdbMetadata.TryLoad(metadataFile, out imdbMetadata))
                        {
                            log($"!Metadata is unavailable for {group.Key}.");
                        }

                        return (Videos: group.Value, Metadata: imdbMetadata);
                    })
                    .SelectMany(group => group
                        .Videos
                        .Select(video =>
                        {
                            if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata, group.Metadata, settings.LibraryDirectory))
                            {
                                log($"!Fail: {video}");
                            }

                            return (
                                ImdbId: group.Metadata?.ImdbId ?? string.Empty,
                                RelativePath: Path.GetRelativePath(settings.LibraryDirectory, video),
                                Metadata: videoMetadata);
                        }))
                    .ToLookup(video => video.ImdbId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.ToDictionary(video => video.RelativePath, video => video.Metadata));

                newVideoMetadata.ForEach(group => existingMetadata.AddOrUpdate(group.Key,
                    key =>
                    {
                        log($"Create {group.Key} with {string.Join("|", group.Value.Keys)}.");
                        return new ConcurrentDictionary<string, VideoMetadata?>(group.Value);
                    },
                    (key, videos) =>
                    {
                        group.Value.ForEach(video =>
                        {
                            log($"Add to {group.Key} with {video.Key}.");
                            Debug.Assert(videos.TryAdd(video.Key, video.Value));
                        });
                        return videos;
                    }));
            });

        existingMetadata
            .Where(group => group.Value.IsEmpty())
            .Do(group => log($"Delete {group.Key}."))
            .ToArray()
            .ForEach(group => Debug.Assert(existingMetadata.TryRemove(group)));

        await settings.WriteMovieLibraryMetadataAsync(existingMetadata, cancellationToken);
    }

    internal static async Task WriteExternalVideoMetadataAsync(ISettings settings, CancellationToken cancellationToken = default, params string[] directories)
    {
        Dictionary<string, VideoMetadata> allVideoMetadata = directories
            .SelectMany(directory => Directory.GetFiles(directory, VideoSearchPattern, SearchOption.AllDirectories))
            .Select(video =>
            {
                string metadata = PathHelper.ReplaceExtension(video, XmlMetadataExtension);
                string imdbId = metadata.TryLoadXmlImdbId(out string? xmlImdbId) ? xmlImdbId : throw new InvalidOperationException(video);
                if (TryReadVideoMetadata(video, out VideoMetadata? videoMetadata))
                {
                    return (ImdbId: imdbId, Value: videoMetadata);
                }

                throw new InvalidOperationException(video);
            })
            .Distinct(metadata => metadata.ImdbId)
            .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value);

        await settings.WriteMovieExternalMetadataAsync(allVideoMetadata, cancellationToken);
    }

    internal static async Task WriteMergedMovieMetadataAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> libraryMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, ImdbMetadata> mergedMetadata = await settings.LoadMovieMergedMetadataAsync(cancellationToken);
        ILookup<string, string> cacheFilesByImdbId = Directory
            .EnumerateFiles(settings.MovieMetadataCacheDirectory, ImdbCacheSearchPattern)
            .ToLookup(file => PathHelper.GetFileNameWithoutExtension(file).Split(Delimiter).First());
        Dictionary<string, string> metadataFilesByImdbId = Directory
            .EnumerateFiles(settings.MovieMetadataDirectory, ImdbMetadataSearchPattern)
            .ToDictionary(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        HashSet<string> topLibraryImdbIds = new(
            libraryMetadata
                .Where(group => group
                    .Value
                    .Select(video => PathHelper.GetFileNameWithoutExtension(video.Key))
                    .Any(name => name.ContainsIgnoreCase(".1080p.") && (name.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || name.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
                .Select(group => group.Key),
            StringComparer.OrdinalIgnoreCase);
        Action<string> recycleMetadata = settings.MovieMetadataBackupDirectory.IsNullOrWhiteSpace()
            ? FileHelper.Recycle
            : file => FileHelper.MoveToDirectory(file, settings.MovieMetadataBackupDirectory, true, true);
        Action<string> recycleCache = settings.MovieMetadataCacheBackupDirectory.IsNullOrWhiteSpace()
            ? FileHelper.Recycle
            : file => FileHelper.MoveToDirectory(file, settings.MovieMetadataCacheBackupDirectory, true, true);
        KeyValuePair<string, string>[] metadataToDelete = metadataFilesByImdbId
            .Where(metadataFile => topLibraryImdbIds.Contains(metadataFile.Key))
            .ToArray();
        log($"Delete {metadataToDelete.Length}");
        metadataToDelete
            .ForEach(metadataFile =>
            {
                log($"Delete {metadataFile.Value}");
                recycleMetadata(metadataFile.Value);
                metadataFilesByImdbId.Remove(metadataFile.Key);
            });

        cacheFilesByImdbId
            .Where(group => topLibraryImdbIds.Contains(group.Key))
            .ToArray()
            .ForEach(group => group.ForEach(file =>
            {
                log($"Delete {file}");
                recycleCache(file);
            }));

        mergedMetadata
            .Keys
            .Except(metadataFilesByImdbId.Keys, StringComparer.OrdinalIgnoreCase)
            .Do(imdbId => log($"Delete {imdbId}."))
            .ToArray()
            .ForEach(imdbId => Debug.Assert(mergedMetadata.TryRemove(imdbId, out _)));

        string[] newImdbIds = metadataFilesByImdbId
            .Keys
            .Except(mergedMetadata.Keys, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        log($"Add {newImdbIds.Length}.");
        newImdbIds
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .ForAll(imdbId =>
            {
                string file = metadataFilesByImdbId[imdbId];
                if (ImdbMetadata.TryLoad(file, out ImdbMetadata? imdbMetadata))
                {
                    log($"Add {file}.");
                    mergedMetadata[imdbId] = imdbMetadata;
                }
                else
                {
                    Debug.Fail(file);
                }
            });

        await settings.WriteMovieMergedMetadataAsync(mergedMetadata, cancellationToken);
    }

    internal static async Task MergeMovieMetadataAsync(ISettings settings, string metadataDirectory, CancellationToken cancellationToken = default)
    {
        ConcurrentDictionary<string, ImdbMetadata> mergedMetadata = new();

        string[] movieMetadataFiles = Directory.GetFiles(metadataDirectory);
        Dictionary<string, string> metadataFilesByImdbId = movieMetadataFiles
            .ToDictionary(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        metadataFilesByImdbId
            .Keys
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .ForEach(imdbId =>
            {
                string file = metadataFilesByImdbId[imdbId];
                if (ImdbMetadata.TryLoad(file, out ImdbMetadata? imdbMetadata))
                {
                    mergedMetadata[imdbId] = imdbMetadata;
                }
                else
                {
                    throw new InvalidOperationException(file);
                }
            });

        await settings.WriteMovieMergedMetadataAsync(mergedMetadata, cancellationToken);
    }

    internal static async Task DownloadMissingTitlesFromDoubanAsync(ISettings settings, string directory, int level = DefaultDirectoryLevel, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        WebDriverHelper.DisposeAll();

        ConcurrentQueue<string> movies = new(EnumerateDirectories(directory, level));
        List<(string Title, string Year, string Directory)> noTranslation = [];
        await Enumerable
            .Range(0, Douban.MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (webDriverIndex, _, token) =>
                {
                    using WebDriverWrapper webDriver = new(() => WebDriverHelper.Start(webDriverIndex, keepExisting: true));
                    while (movies.TryDequeue(out string? movie))
                    {
                        string[] metadataFiles = Directory.GetFiles(movie, XmlMetadataSearchPattern);
                        string backupMetadataFile = metadataFiles.First(file => file.EndsWithIgnoreCase($"{Delimiter}{DefaultBackupFlag}{XmlMetadataExtension}"));
                        string metadataFile = metadataFiles.First(file => !file.EndsWithIgnoreCase($"{Delimiter}{DefaultBackupFlag}{XmlMetadataExtension}"));
                        XDocument metadataDocument = XDocument.Load(metadataFile);
                        string translatedTitle = metadataDocument.Root!.Element("title")!.Value;
                        if (translatedTitle.ContainsChineseCharacter() || Regex.IsMatch(translatedTitle, @"^[0-9]+$"))
                        {
                            continue;
                        }

                        XDocument backupMetadataDocument = XDocument.Load(backupMetadataFile);
                        string backupOriginalTitle = backupMetadataDocument.Root!.Element("originaltitle")?.Value ?? string.Empty;
                        string backupEnglishTitle = backupMetadataDocument.TryGetTitle(out string? xmlTitle) ? xmlTitle : string.Empty;
                        string year = backupMetadataDocument.Root!.Element("year")?.Value ?? string.Empty;

                        if (!backupMetadataFile.TryLoadXmlImdbId(out string? imdbId)
                            || translatedTitle.IsNullOrEmpty()) // Already searched Douban, no result.
                        {
                            if (backupOriginalTitle.IsNullOrWhiteSpace() || !backupOriginalTitle.ContainsCjkCharacter())
                            {
                                noTranslation.Add((backupEnglishTitle, year, movie));
                            }

                            continue;
                        }

                        string doubanTitle = await Douban.GetTitleAsync(webDriver, imdbId, token);
                        int lastIndex = doubanTitle.LastIndexOf(backupEnglishTitle, StringComparison.InvariantCultureIgnoreCase);
                        if (lastIndex >= 0)
                        {
                            doubanTitle = doubanTitle[..lastIndex].Trim();
                        }

                        string originalTitle = backupMetadataDocument.Root!.Element("originaltitle")?.Value ?? string.Empty;
                        if (originalTitle.IsNotNullOrWhiteSpace())
                        {
                            lastIndex = doubanTitle.LastIndexOf(originalTitle, StringComparison.InvariantCultureIgnoreCase);
                            if (lastIndex >= 0)
                            {
                                doubanTitle = doubanTitle[..lastIndex].Trim();
                            }
                        }

                        log($"""
                            {movie}
                            {backupEnglishTitle}
                            {translatedTitle}
                            {doubanTitle}
                            
                            """);
                        if (!doubanTitle.EqualsIgnoreCase(translatedTitle))
                        {
                            metadataFiles
                                .Where(file => !file.EndsWithIgnoreCase($"{Delimiter}{DefaultBackupFlag}{XmlMetadataExtension}"))
                                .ToArray()
                                .ForEach(metadataFile =>
                                {
                                    XDocument metadataDocument = XDocument.Load(metadataFile);
                                    metadataDocument.Root!.Element("title")!.Value = doubanTitle;
                                    metadataDocument.Save(metadataFile);
                                });
                        }

                        if (!doubanTitle.ContainsCjkCharacter() && !backupOriginalTitle.ContainsCjkCharacter())
                        {
                            noTranslation.Add((backupEnglishTitle, year, movie));
                        }
                    }
                },
                Douban.MaxDegreeOfParallelism,
                cancellationToken);

        noTranslation.ForEach(movie => log($"{movie.Title} ({movie.Year})"));
        log(string.Empty);
        noTranslation.OrderBy(movie => movie.Directory).ForEach(movie => log($"{movie.Title} ({movie.Year}) - {movie.Directory}"));

        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }

        string[] translatedTitles = await File.ReadAllLinesAsync(settings.TempFile, cancellationToken);
        Debug.Assert(noTranslation.Count == translatedTitles.Length);
        noTranslation.ForEach((movie, index) =>
        {
            string translatedTitle = translatedTitles[index];
            string year = translatedTitle[^5..^1];
            Debug.Assert(movie.Year.EqualsOrdinal(year));
            Directory.EnumerateFiles(movie.Directory, XmlMetadataSearchPattern)
                .Where(file => !file.EndsWithIgnoreCase($"{Delimiter}{DefaultBackupFlag}{XmlMetadataExtension}"))
                .ToArray()
                .ForEach(metadataFile =>
                {
                    XDocument metadataDocument = XDocument.Load(metadataFile);
                    metadataDocument.Root!.Element("title")!.Value = translatedTitle[..^7].Trim();
                    metadataDocument.Save(metadataFile);
                });
        });
    }

    internal static async Task WriteNikkatsuMetadataAsync(string cacheFile, string jsonFile)
    {
        const string WikipediaUri = "https://en.wikipedia.org";

        CQ mainCQ = await File.ReadAllTextAsync(cacheFile);
        WikipediaNikkatsu[] movies = mainCQ
            .Find("table.wikitable > tbody > tr")
            .Select(row => (row, cells: row.Cq().Find("td")))
            .Where(row => row.cells.Length == 5)
            .Select(row =>
            {
                CQ cells = row.cells;
                string releaseDate = cells[0].TextContent.Trim();

                string[] englishTitles = cells.Eq(1).Find("i > b").Select(dom => dom.TextContent).ToArray();
                Debug.Assert(englishTitles.Length is 0 or 1 or 2);
                string originalTitle = cells[1]
                    .ChildNodes
                    .Where(node => node.NodeType == NodeType.TEXT_NODE)
                    .Select(node => node.ToString() ?? string.Empty)
                    .FirstOrDefault(text => text.IsNotNullOrWhiteSpace(), string.Empty);
                CQ translatedTitleCQ = cells.Eq(1).Find("i").Last();
                string translatedTitle = translatedTitleCQ.Find("b").Any() ? string.Empty : translatedTitleCQ.Text();
                CQ linkCQ = cells.Eq(1).Find("a");
                Debug.Assert(linkCQ.Length is 0 or 1 or 2);
                string[] links = linkCQ.Select(dom => $"{WikipediaUri}{dom.GetAttribute("href")}").ToArray();

                string director = cells[2].TextContent.Trim();

                string[] cast = cells[3]
                    .ChildNodes
                    .Select(node => node.NodeType switch
                    {
                        NodeType.TEXT_NODE => node.ToString()?.Trim() ?? string.Empty,
                        _ => node.TextContent.Trim()
                    })
                    .Where(text => text.IsNotNullOrWhiteSpace())
                    .Order()
                    .ToArray();

                string note = cells[4].TextContent.Trim();

                return new WikipediaNikkatsu(releaseDate, originalTitle, translatedTitle, director, note)
                {
                    EnglishTitles = englishTitles,
                    Links = links,
                    Cast = cast,
                };
            })
            .DistinctBy(movie => (movie.ReleaseDate, movie.OriginalTitle))
            .ToArray();

        await JsonHelper.SerializeToFileAsync(movies, jsonFile);
    }

    private static readonly DateTime VersionDateTimeUtc = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal static bool IsLatestVersion(string metadata) =>
        new FileInfo(metadata).LastWriteTimeUtc > VersionDateTimeUtc;

    internal static async Task UpdateMetadataAsync(string directory, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        await Directory
            .EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
            .Select(metadata => (metadata, ImdbId: ImdbMetadata.TryGet(metadata, out string? imdbId) ? imdbId : string.Empty))
            .Where(metadata =>
            {
                if (metadata.ImdbId.IsNullOrWhiteSpace())
                {
                    return false;
                }

                DateTime metadataLastWriteTimeUtc = new FileInfo(metadata.metadata).LastWriteTimeUtc;
                return metadataLastWriteTimeUtc > new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    && metadataLastWriteTimeUtc < new DateTime(2024, 1, 18, 0, 0, 0, DateTimeKind.Utc);
            })
            .Select(metadata =>
            {
                string file = Path.Combine(PathHelper.GetDirectoryName(metadata.metadata), $"{metadata.ImdbId}{ImdbCacheExtension}");

                CQ imdbCQ = File.ReadAllText(file);
                (string Text, string Url)[] websites = imdbCQ.Find("div[data-testid='details-officialsites'] > ul > li")
                        .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Official sites"))
                        ?.Cq().Find("ul li")
                        .Select(listItem => new CQ(listItem))
                        .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                        .ToArray()
                    ?? imdbCQ
                        .Find("""ul.ipc-metadata-list li[data-testid="details-officialsites"] ul li""")
                        .Select(listItem => new CQ(listItem))
                        .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                        .ToArray();

                (string Text, string Url)[] locations = imdbCQ.Find("div[data-testid='title-details-filminglocations'] > ul > li")
                        .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Filming locations"))
                        ?.Cq().Find("ul li")
                        .Select(listItem => new CQ(listItem))
                        .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                        .Select(data => (data.Text, $"https://www.imdb.com{data.Url[..data.Url.IndexOfIgnoreCase("&ref_=")]}"))
                        .ToArray()
                    ?? imdbCQ
                        .Find("""ul.ipc-metadata-list li[data-testid="title-details-filminglocations"] ul li""")
                        .Select(listItem => new CQ(listItem))
                        .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                        .Select(data => (data.Text, $"https://www.imdb.com{data.Url[..data.Url.IndexOfIgnoreCase("&ref_=")]}"))
                        .ToArray();

                (string Text, string Url)[] companies = imdbCQ.Find("div[data-testid='title-details-companies'] > ul > li")
                        .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Production companies"))
                        ?.Cq().Find("ul li")
                        .Select(listItem => new CQ(listItem))
                        .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: new UriBuilder(new Uri(new Uri("https://www.imdb.com"), listItemCQ.Find("a").Attr("href"))) { Query = string.Empty, Port = -1 }.ToString()))
                        .ToArray()
                    ?? imdbCQ
                        .Find("""ul.ipc-metadata-list li[data-testid="title-details-companies"] ul li""")
                        .Select(listItem => new CQ(listItem))
                        .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: new UriBuilder(new Uri(new Uri("https://www.imdb.com"), listItemCQ.Find("a").Attr("href"))) { Query = string.Empty, Port = -1 }.ToString()))
                        .ToArray();

                return (metadata.metadata, websites, locations, companies);
            })
            .ForEachAsync(async data =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                log(PathHelper.GetDirectoryName(data.metadata));
                Debug.Assert(ImdbMetadata.TryLoad(data.metadata, out ImdbMetadata? imdbMetadata));
                imdbMetadata = imdbMetadata with
                {
                    Websites = data.websites.Distinct().ToLookup(item => item.Text, item => item.Url).ToDictionary(group => group.Key, group => group.ToArray()),
                    FilmingLocations = data.locations.Distinct().ToDictionary(item => item.Text, item => item.Url),
                    Companies = data.companies.Distinct().ToLookup(item => item.Text, item => item.Url).ToDictionary(group => group.Key, group => group.ToArray())
                };

                if (!isDryRun)
                {
                    await JsonHelper.SerializeToFileAsync(imdbMetadata, data.metadata, cancellationToken);
                }
            }, cancellationToken);
    }

    internal static void CopyMetadata(string sourceDirectory, string destinationDirectory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ILookup<string, string> sourceMetadataFiles = Directory
            .EnumerateFiles(sourceDirectory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
            .ToLookup(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);
        ILookup<string, string> destinationMetadataFiles = Directory
            .EnumerateFiles(destinationDirectory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
            .ToLookup(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        destinationMetadataFiles
            .Where(destinationGroup => destinationGroup.Key.IsNotNullOrWhiteSpace() && sourceMetadataFiles.Contains(destinationGroup.Key))
            .ForEach(destinationGroup =>
            {
                string imdbId = destinationGroup.Key;
                Debug.Assert(imdbId.IsImdbId());
                string[] sourceGroup = sourceMetadataFiles[imdbId].ToArray();
                Debug.Assert(sourceGroup.Any());
                (string File, DateTime LastWriteUtc) sourceMetadataFile = sourceGroup
                    .Select(file => (File: file, LastWriteUtc: new FileInfo(file).LastWriteTimeUtc))
                    .MaxBy(file => file.LastWriteUtc);
                string sourceMetadataDirectory = PathHelper.GetDirectoryName(sourceMetadataFile.File);
                string[] sourceFiles = Directory
                    .EnumerateFiles(sourceMetadataDirectory, ImdbCacheSearchPattern, SearchOption.TopDirectoryOnly)
                    .Append(sourceMetadataFile.File)
                    .ToArray();
                destinationGroup
                    .Where(destinationMetadataFile => new FileInfo(destinationMetadataFile).LastWriteTimeUtc < sourceMetadataFile.LastWriteUtc)
                    .ForEach(destinationMetadataFile =>
                    {
                        string destinationMetadataDirectory = PathHelper.GetDirectoryName(destinationMetadataFile);
                        Directory
                            .EnumerateFiles(destinationMetadataDirectory, ImdbCacheSearchPattern, SearchOption.TopDirectoryOnly)
                            .Append(destinationMetadataFile)
                            .ToArray()
                            .ForEach(file =>
                            {
                                log($"Delete {file}.");
                                if (!isDryRun)
                                {
                                    FileHelper.Recycle(file);
                                }
                            });
                        sourceFiles.ForEach(file =>
                        {
                            log($"Copy {file} to {destinationMetadataDirectory}.");
                            if (!isDryRun)
                            {
                                FileHelper.CopyToDirectory(file, destinationMetadataDirectory);
                            }
                        });
                    });
            });
    }

    internal static void UpdateCertifications(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params string[][] directoryDrives)
    {
        log ??= Logger.WriteLine;

        directoryDrives
            .AsParallel()
            .ForAll(driveDirectories =>
            {
                driveDirectories
                    .SelectMany(directory => Directory.EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
                    .Where(metadataFile => !PathHelper.GetFileNameWithoutExtension(metadataFile).EqualsIgnoreCase(NotExistingFlag))
                    .Select(metadataFile => (MetadataFile: metadataFile, AdvisoriesFile: PathHelper.ReplaceFileName(metadataFile, $"{metadataFile.GetImdbId()}.Advisories.log")))
                    .AsParallel()
                    .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
                    .ForAll(file =>
                    {
                        ImdbMetadata imdbMetadata = JsonHelper.DeserializeFromFile<ImdbMetadata>(file.MetadataFile);
                        if (imdbMetadata.Certifications is not null && imdbMetadata.Certifications.Any())
                        {
                            return;
                        }

                        CQ advisoriesCQ = File.ReadAllText(file.AdvisoriesFile);
                        Dictionary<string, string> certifications = advisoriesCQ
                            .Find("#certificates li.ipl-inline-list__item")
                            .Select(certificationDom => (
                                Certification: Regex.Replace(certificationDom.TextContent.Trim(), @"\s+", " "),
                                Link: certificationDom.Cq().Find("a").Attr("href").Trim()))
                            .DistinctBy(certification => certification.Certification)
                            .ToDictionary(certification => certification.Certification, certification => certification.Link);
                        if (certifications.IsEmpty())
                        {
                            return;
                        }

                        log(file.MetadataFile);
                        log(string.Join(Environment.NewLine, certifications.Keys));
                        imdbMetadata = imdbMetadata with { Certifications = certifications };
                        JsonHelper.SerializeToFile(imdbMetadata, file.MetadataFile);
                    });
            });
    }
}
