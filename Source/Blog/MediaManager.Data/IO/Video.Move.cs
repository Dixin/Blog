namespace MediaManager.IO;

using System.Linq;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using MediaManager.Net;

internal static partial class Video
{
    internal static void RenameFiles(string path, Func<string, int, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Directory.EnumerateFiles(path, pattern ?? PathHelper.AllSearchPattern, searchOption ?? SearchOption.AllDirectories)
            .Where(file => predicate?.Invoke(file) ?? true)
            .OrderBy(file => file, StringComparer.CurrentCulture)
            .ToArray()
            .ForEach((file, index) =>
            {
                string newFile = rename(file, index);
                if (!file.EqualsOrdinal(newFile))
                {
                    log(file);
                    if (!isDryRun)
                    {
                        FileHelper.Move(file, newFile, overwrite);
                    }
                    log(newFile);
                }
            });
    }

    internal static void RenameDirectories(string path, Func<string, int, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory.EnumerateDirectories(path, pattern ?? PathHelper.AllSearchPattern, searchOption ?? SearchOption.AllDirectories)
            .Where(directory => predicate?.Invoke(directory) ?? true)
            .Order()
            .ToArray()
            .ForEach((directory, index) =>
            {
                string newDirectory = rename(directory, index);
                if (directory.EqualsIgnoreCase(newDirectory))
                {
                    return;
                }

                log(directory);
                if (!isDryRun)
                {
                    DirectoryHelper.Move(directory, newDirectory);
                }

                log(newDirectory);
            });
    }

    internal static void RenameVideosWithMultipleAudio(IEnumerable<string> files, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        files.ForEach(file =>
        {
            if (!File.Exists(file))
            {
                log($"Not exist {file}");
                return;
            }

            int audio = ReadAudioMetadata(file, log);
            if (audio <= 1)
            {
                log($"Audio {audio} {file}");
                return;
            }

            List<string> info = PathHelper.GetFileName(file).Split(Delimiter).ToList();
            if (info.Count <= 3)
            {
                // Space.
                info = PathHelper.GetFileNameWithoutExtension(file).Split(" ").Append(info.Last()).ToList();
            }

            string last = info[^2];
            if (last.IndexOfIgnoreCase("handbrake") >= 0)
            {
                info.Insert(info.Count - 3, $"{audio}Audio");
            }
            else
            {
                info.Insert(info.Count - 2, $"{audio}Audio");

            }
            string newFile = string.Join(Delimiter, info);
            string directory = PathHelper.GetDirectoryName(file);
            log(file);
            log(Path.Combine(directory, newFile));
            File.Move(file, Path.Combine(directory, newFile));
            Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                .Where(attachment => attachment != file)
                .Where(attachment => PathHelper.GetFileName(attachment).StartsWithIgnoreCase(PathHelper.GetFileNameWithoutExtension(file)))
                .ToList()
                .ForEach(attachment =>
                {
                    string newAttachment = Path.Combine(directory, PathHelper.GetFileName(attachment).Replace(PathHelper.GetFileNameWithoutExtension(file), string.Join(Delimiter, info.Take(..^1))));
                    log(newAttachment);
                    File.Move(attachment, newAttachment);
                });
        });
    }

    internal static void RenameEpisodesWithTitle(string mediaDirectory, string metadataDirectory = "", Func<string, string, string>? rename = null, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        if (metadataDirectory.IsNullOrWhiteSpace())
        {
            metadataDirectory = mediaDirectory;
        }

        rename ??= (file, title) => PathHelper.AddFilePostfix(file, $"{Delimiter}{title.TrimStart(Delimiter.Single())}");

        string[] mediaFiles = Directory.GetFiles(mediaDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);

        Directory
            .GetFiles(metadataDirectory, TmdbMetadata.NfoSearchPattern, SearchOption.AllDirectories)
            .ForEach(file =>
            {
                string seasonEpisode = SeasonEpisodeRegex().Match(PathHelper.GetFileNameWithoutExtension(file)).Value;
                if (seasonEpisode.IsNullOrWhiteSpace() || !File.Exists(file))
                {
                    return;
                }

                string title = XDocument.Load(file).Root?.Element("title")?.Value.FilterForFileSystem().Trim() ?? throw new InvalidOperationException($"{file} has no title.");
                string currentDirectory = PathHelper.GetDirectoryName(file);
                string relative = Path.GetRelativePath(mediaDirectory, currentDirectory);
                mediaFiles
                    .Where(file => PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase(seasonEpisode))
                    .ToArray()
                    .ForEach(file =>
                    {
                        log(file);
                        string newFile = rename(file, title).Trim();
                        if (!isDryRun)
                        {
                            FileHelper.Move(file, newFile, overwrite, true);
                        }

                        log(newFile);
                    });
            });
    }

    internal static void RenameVideosWithDefinition(string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool isDryRun = false, Action<string>? log = null) =>
        RenameVideosWithDefinition(
            Directory.GetFiles(directory, PathHelper.AllSearchPattern, searchOption)
                .Where(IsVideo)
                .ToArray(),
            isDryRun,
            log);

    internal static void RenameVideosWithDefinition(string[] files, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        files.ForEach(file => Debug.Assert(Regex.IsMatch(file, @"^\!(720p|1080p)\: ([0-9]{2,4}x[0-9]{2,4} )?(.*)$")));
        files
            .Select(file =>
            {
                Match match = Regex.Match(file, @"^\!(720p|1080p)\: ([0-9]{2,4}x[0-9]{2,4} )?(.*)$");
                string path = match.Groups.Last<Group>().Value;
                return (Definition: match.Groups[1].Value, File: path, Extension: PathHelper.GetExtension(PathHelper.GetFileNameWithoutExtension(path)));
            })
            .ForEach(result =>
            {
                string extension = PathHelper.GetExtension(result.File);
                string newFile;
                if (result.File.IsVideo())
                {
                    string file = PathHelper.GetFileNameWithoutExtension(PathHelper.GetFileNameWithoutExtension(result.File));
                    newFile = Path.Combine(PathHelper.GetDirectoryName(result.File), $"{file}.{result.Definition}{result.Extension}{extension}");
                }
                else
                {
                    string file = PathHelper.GetFileNameWithoutExtension(result.File);
                    newFile = Path.Combine(PathHelper.GetDirectoryName(result.File), $"{file}.{result.Definition}{extension}");
                }
                if (isDryRun)
                {
                    log(newFile);
                }
                else
                {
                    if (File.Exists(result.File))
                    {
                        FileHelper.Move(result.File, newFile, true);
                    }
                }
            });
    }

    internal const string AdditionalMetadataSeparator = "@";

    internal static void RenameDirectoriesWithAdditionalMetadata(ISettings settings, string directory, int level = DefaultDirectoryLevel, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] directories = EnumerateDirectories(directory, level).ToArray();
        directories.Where(DirectoryHelper.IsHidden).Prepend("Hidden backup").Append(string.Empty).ForEach(log);
        directories
            .ForEach(movie =>
            {
                string name = PathHelper.GetFileName(movie);
                int index = name.IndexOfOrdinal(AdditionalMetadataSeparator);
                if (index >= 0)
                {
                    name = name[(index + 1)..];
                    Debug.Assert(VideoDirectoryInfo.TryParse(name, out _));
                }

                string[] languages = Directory
                    .EnumerateFiles(movie)
                    .Where(file => file.IsVideo())
                    .Select(VideoMovieFileInfo.Parse)
                    .Where(video => video.Version.Contains($"{VersionSeparator}{settings.TopForeignKeyword}"))
                    .Select(video => video.Edition)
                    .ToArray();
                string[] regions = [];
                string[] genres = [];
                bool hasImdbMetadata = ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata);
                if (hasImdbMetadata)
                {
                    if (languages.IsEmpty())
                    {
                        languages = imdbMetadata.Languages;
                    }

                    regions = imdbMetadata.Regions;
                    genres = imdbMetadata.Genres;
                }

                if (languages.IsEmpty() || regions.IsEmpty() || genres.IsEmpty())
                {
                    if (TmdbMetadata.TryRead(movie, out string? tmdbId, out string? year, out string[]? tmdbRegions, out string[]? tmdbGenres))
                    {
                        if (regions.IsEmpty())
                        {
                            regions = tmdbRegions;
                        }

                        if (genres.IsEmpty())
                        {
                            genres = tmdbGenres;
                        }
                    }
                    else if (!hasImdbMetadata)
                    {
                        return;
                    }
                }

                string additional = $"{string.Join(VersionSeparator, regions.Take(4))}{Delimiter}{string.Join(VersionSeparator, languages.Take(4))}{Delimiter}{string.Join(VersionSeparator, genres.Take(4))}";
                string newMovie = PathHelper.ReplaceDirectoryName(movie, $"{additional}{AdditionalMetadataSeparator}{name}");

                log(movie);
                if (!isDryRun)
                {
                    DirectoryHelper.Move(movie, newMovie, overwrite);
                }

                log(newMovie);
            });
    }

    internal static void RenameDirectoriesWithMetadata(ISettings settings, string directory, int level = DefaultDirectoryLevel, bool additionalInfo = false, bool overwrite = false, bool isDryRun = false, string backupFlag = DefaultBackupFlag, bool isTV = false, bool skipRenamed = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                if (!overwrite && PathHelper.GetFileName(movie).ContainsOrdinal("{"))
                {
                    return;
                }

                if (skipRenamed && VideoDirectoryInfo.TryParse(movie, out _))
                {
                    return;
                }

                string[] files = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).OrderBy(file => file).ToArray();
                string[] xmlMetadataFiles = files.Where(IsTmdbNfoMetadata).ToArray();
                XDocument english;
                XDocument? translated;
                if (isTV)
                {
                    english = XDocument.Load(xmlMetadataFiles.Single(file => PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.TVNfoFile)));
                    translated = null;
                }
                else
                {
                    if (xmlMetadataFiles.Any(file => PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile)) && xmlMetadataFiles.Any(file => PathHelper.GetFileName(file).EqualsIgnoreCase(PathHelper.AddFilePostfix(TmdbMetadata.MovieNfoFile, $"{Delimiter}{backupFlag}"))))
                    {
                        english = XDocument.Load(xmlMetadataFiles.Single(file => PathHelper.GetFileName(file).EqualsIgnoreCase(PathHelper.AddFilePostfix(TmdbMetadata.MovieNfoFile, $"{Delimiter}{backupFlag}"))));
                        translated = XDocument.Load(xmlMetadataFiles.Single(file => PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile)));
                    }
                    else
                    {
                        english = XDocument.Load(xmlMetadataFiles.Single(file => PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile)));
                        translated = null;
                    }
                }

                string json = files.Single(IsImdbMetadata);
                ImdbMetadata.TryLoad(json, out ImdbMetadata? imdbMetadata);

                string defaultTitle = english.Root?.Element("title")?.Value ?? throw new InvalidOperationException($"{movie} has no default title.");
                defaultTitle = defaultTitle.ReplaceOrdinal(" - ", TitleSeparator);
                string translatedTitle = translated?.Root?.Element("title")?.Value ?? string.Empty;
                translatedTitle = translatedTitle.ReplaceOrdinal(" - ", TitleSeparator);
                string originalTitle = imdbMetadata?.OriginalTitle ?? english.Root?.Element("originaltitle")?.Value ?? imdbMetadata?.Name ?? string.Empty;
                originalTitle = originalTitle.ReplaceOrdinal(" - ", TitleSeparator);
                originalTitle = originalTitle.EqualsIgnoreCase(defaultTitle) || originalTitle.IsNullOrWhiteSpace()
                    ? string.Empty
                    : $"={originalTitle}";
                string year = imdbMetadata?.Year ?? english.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{movie} has no year.");
                Debug.Assert(english.TryGetImdbId(out string? imdbId)
                    ? json.HasImdbId(imdbId)
                    : NotExistingFlag.EqualsOrdinal(PathHelper.GetFileNameWithoutExtension(json)));
                string rating = imdbMetadata?.FormattedAggregateRating ?? NotExistingFlag;
                string ratingCount = imdbMetadata?.FormattedAggregateRatingCount ?? NotExistingFlag;
                string[] videos = files.Where(IsVideo).ToArray();
                string contentRating = imdbMetadata?.FormattedContentRating ?? NotExistingFlag;
                VideoMovieFileInfo[] movies = isTV ? [] : videos.Select(VideoMovieFileInfo.Parse).ToArray();
                VideoEpisodeFileInfo[] episodes = isTV ? VideoDirectoryInfo.GetEpisodes(movie).ToArray() : [];
                VideoDirectoryInfo videoDirectoryInfo = new(
                    DefaultTitle1: defaultTitle.ReplaceOrdinal(" - ", " ").ReplaceOrdinal(TitleSeparator, " ").ReplaceOrdinal(".", " ").FilterForFileSystem().Trim(), DefaultTitle2: string.Empty, DefaultTitle3: string.Empty,
                    OriginalTitle1: originalTitle.ReplaceOrdinal(" - ", " ").ReplaceOrdinal(TitleSeparator, " ").ReplaceOrdinal(".", " ").FilterForFileSystem().Trim(), OriginalTitle2: string.Empty, OriginalTitle3: string.Empty,
                    Year: year,
                    TranslatedTitle1: translatedTitle.ReplaceOrdinal(" - ", " ").ReplaceOrdinal(TitleSeparator, " ").ReplaceOrdinal(".", " ").ReplaceIgnoreCase("：", TitleSeparator).FilterForFileSystem().Trim(), TranslatedTitle2: string.Empty, TranslatedTitle3: string.Empty, TranslatedTitle4: string.Empty,
                    AggregateRating: rating, AggregateRatingCount: ratingCount,
                    ContentRating: contentRating,
                    Resolution: isTV ? VideoDirectoryInfo.GetResolution(episodes) : VideoDirectoryInfo.GetResolution(movies),
                    Source: isTV ? VideoDirectoryInfo.GetSource(episodes) : VideoDirectoryInfo.GetSource(movies),
                    Is3D: isTV ? VideoDirectoryInfo.Get3D(episodes) : VideoDirectoryInfo.Get3D(movies),
                    Hdr: isTV ? VideoDirectoryInfo.GetHdr(episodes) : VideoDirectoryInfo.GetHdr(movies)
                );

                string additional = additionalInfo
                    ? $"{{{string.Join(",", imdbMetadata?.Regions.Take(5) ?? [])};{string.Join(",", imdbMetadata?.Genres.Take(3) ?? [])}}}"
                    : string.Empty;
                string newMovie = $"{videoDirectoryInfo}{additional}";
                string newDirectory = Path.Combine(PathHelper.GetDirectoryName(movie), newMovie);
                if (movie.EqualsOrdinal(newDirectory))
                {
                    return;
                }

                log(movie);
                if (!isDryRun)
                {
                    Directory.Move(movie, newDirectory);
                }

                log(newDirectory);
                log(string.Empty);
            });
    }

    internal static void RenameDirectoriesWithoutAdditionalMetadata(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] directories = EnumerateDirectories(directory, level).ToArray();
        directories.Where(DirectoryHelper.IsHidden).Prepend("Hidden backup").Append(string.Empty).ForEach(log);
        directories
            .ForEach(movie =>
            {
                string name = PathHelper.GetFileName(movie);
                bool isRenamed = false;
                if (name.StartsWithOrdinal("0."))
                {
                    name = name["0.".Length..];
                    isRenamed = true;
                }

                if (name.ContainsOrdinal(AdditionalMetadataSeparator))
                {
                    name = name
                        .Split(AdditionalMetadataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Single(segment => VideoDirectoryInfo.TryParse(segment, out _));
                    isRenamed = true;
                }

                if (isRenamed)
                {
                    log(movie);
                    string newMovie = PathHelper.ReplaceDirectoryName(movie, name);
                    if (!isDryRun)
                    {
                        DirectoryHelper.Move(movie, newMovie);
                    }

                    log(newMovie);
                }
            });
    }

    internal static void RenameDirectoriesWithImdbMetadata(ISettings settings, string directory, int level = DefaultDirectoryLevel, bool isTV = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                if (!VideoDirectoryInfo.TryParse(movie, out VideoDirectoryInfo? parsed))
                {
                    log($"!Cannot parse {movie}");
                    return;
                }

                if (ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata))
                {
                    if (imdbMetadata.Year.IsNotNullOrWhiteSpace())
                    {
                        parsed = parsed with { Year = imdbMetadata.Year };
                    }

                    Debug.Assert(parsed.Year.IsNotNullOrWhiteSpace());
                    parsed = parsed with
                    {
                        AggregateRating = imdbMetadata.FormattedAggregateRating,
                        AggregateRatingCount = imdbMetadata.FormattedAggregateRatingCount,
                        ContentRating = imdbMetadata.FormattedContentRating
                    };
                }

                if (isTV)
                {
                    VideoEpisodeFileInfo[] episodes = VideoDirectoryInfo.GetEpisodes(movie).ToArray();
                    parsed = parsed with
                    {
                        Resolution = VideoDirectoryInfo.GetResolution(episodes),
                        Source = VideoDirectoryInfo.GetSource(episodes),
                        Is3D = VideoDirectoryInfo.Get3D(episodes),
                        Hdr = VideoDirectoryInfo.GetHdr(episodes)
                    };
                }
                else
                {
                    VideoMovieFileInfo[] movies = VideoDirectoryInfo.GetMovies(movie).ToArray();
                    parsed = parsed with
                    {
                        Resolution = VideoDirectoryInfo.GetResolution(movies),
                        Source = VideoDirectoryInfo.GetSource(movies),
                        Is3D = VideoDirectoryInfo.Get3D(movies),
                        Hdr = VideoDirectoryInfo.GetHdr(movies)
                    };
                }

                string newMovie = Path.Combine(PathHelper.GetDirectoryName(movie), parsed.ToString());

                if (movie.EqualsOrdinal(newMovie))
                {
                    return;
                }

                log(movie);
                if (!isDryRun)
                {
                    Directory.Move(movie, newMovie);
                }

                log(newMovie);
                log(string.Empty);
            });
    }

    internal static void RenameMovies(string destination, string directory, int level = DefaultDirectoryLevel, string field = "genre", string? value = null, bool isDryRun = false)
    {
        RenameMovies(
            (movie, _) => Path.Combine(destination, PathHelper.GetFileName(movie)),
            directory,
            level,
            (_, metadata) => value.EqualsIgnoreCase(metadata.Root?.Element(field)?.Value),
            isDryRun);
    }

    internal static void RenameMovies(Func<string, XDocument, string> rename, string directory, int level = DefaultDirectoryLevel, Func<string, XDocument, bool>? predicate = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .Select(movie => (Directory: movie, Metadata: XDocument.Load(Directory.GetFiles(movie, TmdbMetadata.NfoSearchPattern, SearchOption.TopDirectoryOnly).First())))
            .Where(movie => predicate?.Invoke(movie.Directory, movie.Metadata) ?? true)
            .ForEach(movie =>
            {
                string newMovie = rename(movie.Directory, movie.Metadata);
                log(movie.Directory);
                if (!isDryRun)
                {
                    Directory.Move(movie.Directory, newMovie);
                }
                log(newMovie);
            });
    }

    internal static async Task CompareAndMoveAsync(ISettings settings, string fromJsonPath, string newDirectory, string deletedDirectory, Action<string>? log = null, bool isDryRun = false, bool moveAllAttachment = true, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, VideoMetadata> externalMetadata = await settings.LoadMovieExternalMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> moviesMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);

        externalMetadata
            .Where(externalVideo => File.Exists(externalVideo.Value.File))
            .ForEach(externalVideo =>
            {
                VideoMetadata fromVideoMetadata = externalVideo.Value;
                string fromMovie = PathHelper.GetDirectoryName(fromVideoMetadata.File);
                log($"Starting {fromMovie}");

                if (!moviesMetadata.TryGetValue(externalVideo.Key, out ConcurrentDictionary<string, VideoMetadata>? group) || group.IsEmpty())
                {
                    string newExternalMovie = Path.Combine(newDirectory, PathHelper.GetFileName(fromMovie));
                    if (!isDryRun)
                    {
                        Directory.Move(fromMovie, newExternalMovie);
                    }

                    log($"Move external movie {fromMovie} to {newExternalMovie}");
                    return;
                }

                if (group.Count > 1)
                {
                    log($"Multiple videos: {string.Join(", ", group.Keys)}");
                    return;
                }

                VideoMetadata toVideoMetadata = group.Single().Value;
                toVideoMetadata = toVideoMetadata with { File = Path.Combine(settings.LibraryDirectory, toVideoMetadata.File) };
                if (VideoMovieFileInfo.Parse(PathHelper.GetFileNameWithoutExtension(toVideoMetadata.File)).GetEncoderType() is EncoderType.TopX265)
                {
                    log($"Video {toVideoMetadata.File} is x265.");
                    return;
                }

                if (!File.Exists(toVideoMetadata.File))
                {
                    log($"Video {toVideoMetadata.File} does not exist.");
                    return;
                }

                if (VideoMovieFileInfo.Parse(PathHelper.GetFileNameWithoutExtension(toVideoMetadata.File)).GetEncoderType() is EncoderType.TopX265 or EncoderType.TopH264
                    && VideoMovieFileInfo.Parse(PathHelper.GetFileNameWithoutExtension(fromVideoMetadata.File)).GetEncoderType() is not (EncoderType.TopX265 or EncoderType.TopH264))
                {
                    log($"Video {toVideoMetadata.File} is better version.");
                    return;
                }

                if (toVideoMetadata.VideoWidth - fromVideoMetadata.VideoWidth > 5)
                {
                    log($"Width {fromVideoMetadata.File} {fromVideoMetadata.VideoWidth} {toVideoMetadata.VideoWidth}");
                    return;
                }

                if (toVideoMetadata.VideoHeight - fromVideoMetadata.VideoHeight > 20)
                {
                    log($"Height {fromVideoMetadata.File} {fromVideoMetadata.VideoHeight} {toVideoMetadata.VideoHeight}");
                    return;
                }

                TimeSpan difference = toVideoMetadata.Duration - fromVideoMetadata.Duration;
                if (difference < TimeSpan.FromSeconds(-1) || difference > TimeSpan.FromSeconds(1))
                {
                    log($"Duration {fromVideoMetadata.Duration} to old {toVideoMetadata.Duration}: {fromVideoMetadata.File}.");
                    return;
                }

                if (toVideoMetadata.AudioBitRates.Length - fromVideoMetadata.AudioBitRates.Length > 0)
                {
                    log($"Audio {fromVideoMetadata.File} {fromVideoMetadata.AudioBitRates.Length} {toVideoMetadata.AudioBitRates.Length}");
                    return;
                }

                if (toVideoMetadata.Subtitle - fromVideoMetadata.Subtitle > 0)
                {
                    log($"Subtitle {fromVideoMetadata.File} {fromVideoMetadata.Subtitle} {toVideoMetadata.Subtitle}");
                    return;
                }

                string toMovie = PathHelper.GetDirectoryName(toVideoMetadata.File);
                string newExternalVideo = Path.Combine(toMovie, PathHelper.GetFileName(fromVideoMetadata.File));
                if (!isDryRun)
                {
                    File.Move(fromVideoMetadata.File, newExternalVideo);
                }

                log($"Move external video {fromVideoMetadata.File} to {newExternalVideo}");

                Directory.GetFiles(fromMovie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(file => moveAllAttachment || PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase(PathHelper.GetFileNameWithoutExtension(fromVideoMetadata.File)))
                    .ToArray()
                    .ForEach(fromAttachment =>
                    {
                        string toAttachment = Path.Combine(toMovie, PathHelper.GetFileName(fromAttachment));
                        if (File.Exists(toAttachment))
                        {
                            if (!isDryRun)
                            {
                                FileHelper.Recycle(toAttachment);
                            }

                            log($"Delete attachment {toAttachment}");
                        }
                        if (!isDryRun)
                        {
                            FileHelper.Move(fromAttachment, toAttachment);
                        }

                        log($"Move external attachment {fromAttachment} to {toAttachment}");
                    });

                if (!isDryRun)
                {
                    FileHelper.Recycle(toVideoMetadata.File);
                }

                log($"Delete video {toVideoMetadata.File}");

                Directory.GetFiles(toMovie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(file => PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase(PathHelper.GetFileNameWithoutExtension(toVideoMetadata.File)))
                    .ToArray()
                    .ForEach(existingAttachment =>
                    {
                        string newExistingAttachment = Path.Combine(toMovie, PathHelper.GetFileName(existingAttachment).Replace(PathHelper.GetFileNameWithoutExtension(toVideoMetadata.File), PathHelper.GetFileNameWithoutExtension(externalVideo.Value.File)));
                        if (File.Exists(newExistingAttachment))
                        {
                            if (!isDryRun)
                            {
                                FileHelper.Recycle(existingAttachment);
                            }

                            log($"Delete attachment {existingAttachment}");
                        }
                        else
                        {
                            if (!isDryRun)
                            {
                                FileHelper.Move(existingAttachment, newExistingAttachment);
                            }

                            log($"Move attachment {existingAttachment} to {newExistingAttachment}");
                        }
                    });

                string deletedFromMovie = Path.Combine(deletedDirectory, PathHelper.GetFileName(fromMovie));
                if (!isDryRun)
                {
                    Directory.Move(fromMovie, deletedFromMovie);
                }

                log($"Move external movie {fromMovie} to {deletedFromMovie}");
                log(string.Empty);
            });
    }

    internal static void RenameWithUpdatedRatings(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string rating = ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata)
                    ? imdbMetadata.FormattedAggregateRating
                    : NotExistingFlag;
                string name = PathHelper.GetFileName(movie);
                string newName = Regex.Replace(name, @"\[([0-9]\.[0-9]|\-)\]", $"[{rating}]");
                if (!name.EqualsOrdinal(newName))
                {
                    string newMovie = PathHelper.ReplaceFileName(movie, newName);
                    log(movie);
                    log(newMovie);
                    log(string.Empty);
                    if (!isDryRun)
                    {
                        Directory.Move(movie, newMovie);
                    }
                }
            });
    }

    internal static void RenameCollections(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .Select(movie =>
            {
                string name = PathHelper.GetFileName(movie);
                VideoDirectoryInfo videoDirectoryInfo = VideoDirectoryInfo.Parse(name);
                string defaultNumber1 = Regex.Match(videoDirectoryInfo.DefaultTitle1, " ([0-9]{1,2})$").Value.TrimStart(' ');
                string defaultNumber2 = Regex.Match(videoDirectoryInfo.DefaultTitle2, " ([0-9]{1,2})$").Value.TrimStart(' ');
                string originalNumber1 = Regex.Match(videoDirectoryInfo.OriginalTitle1, " ([0-9]{1,2})$").Value.TrimStart(' ');
                string originalNumber2 = Regex.Match(videoDirectoryInfo.OriginalTitle2, " ([0-9]{1,2})$").Value.TrimStart(' ');
                string translatedNumber1 = Regex.Match(videoDirectoryInfo.TranslatedTitle1, "([0-9]{1,2})$").Value;
                string translatedNumber2 = Regex.Match(videoDirectoryInfo.TranslatedTitle2, "([0-9]{1,2})$").Value;
                if (defaultNumber1.IsNullOrWhiteSpace() && defaultNumber2.IsNullOrWhiteSpace())
                {
                    return $"-{movie}";
                }

                if (!defaultNumber1.EqualsOrdinal(translatedNumber1))
                {
                    return $"#Translated number is inconsistent: {movie}";
                }

                if (originalNumber1.IsNotNullOrWhiteSpace() && !defaultNumber1.EqualsOrdinal(originalNumber1))
                {
                    return $"!Original number is inconsistent: {movie}";
                }

                if (defaultNumber2.IsNotNullOrWhiteSpace() && !defaultNumber2.EqualsOrdinal(translatedNumber2))
                {
                    return $"!Translated secondary number is inconsistent:  {movie}";
                }

                if (originalNumber2.IsNotNullOrWhiteSpace() && !defaultNumber2.EqualsOrdinal(originalNumber2))
                {
                    return $"!Original secondary number is inconsistent:  {movie}";
                }

                VideoDirectoryInfo newVideoDirectoryInfo = videoDirectoryInfo with
                {
                    DefaultTitle1 = Regex.Replace(videoDirectoryInfo.DefaultTitle1, " ([0-9]{1,2})$", "`$1"),
                    DefaultTitle2 = Regex.Replace(videoDirectoryInfo.DefaultTitle2, " ([0-9]{1,2})$", "`$1"),
                    OriginalTitle1 = Regex.Replace(videoDirectoryInfo.OriginalTitle1, " ([0-9]{1,2})$", "`$1"),
                    OriginalTitle2 = Regex.Replace(videoDirectoryInfo.OriginalTitle2, " ([0-9]{1,2})$", "`$1"),
                    TranslatedTitle1 = Regex.Replace(videoDirectoryInfo.TranslatedTitle1, "([0-9]{1,2})$", "`$1"),
                    TranslatedTitle2 = Regex.Replace(videoDirectoryInfo.TranslatedTitle2, "([0-9]{1,2})$", "`$1")
                };
                string newMovie = PathHelper.ReplaceFileName(movie, newVideoDirectoryInfo.Name);
                Debug.Assert(newMovie.Length - movie.Length == 1 || newMovie.Length - movie.Length == 2);
                Directory.Move(movie, newMovie);
                return newMovie;
            })
            .OrderBy(message => message, StringComparer.Ordinal)
            .ForEach(log);
    }

    internal static void RenameEpisodes(string directory, string tvTitle, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .EnumerateDirectories(directory)
            .Where(season => Regex.IsMatch(PathHelper.GetFileName(season), "^Season [0-9]{2}"))
            .OrderBy(season => season)
            .ForEach(season =>
            {
                string seasonNumber = PathHelper.GetFileName(season).Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First().ReplaceIgnoreCase("Season ", string.Empty);
                Directory
                    .EnumerateFiles(season, VideoSearchPattern)
                    .OrderBy(video => video)
                    .ToArray()
                    .ForEach((video, index) =>
                    {
                        string prefix = $"{tvTitle}{Delimiter}S{seasonNumber}E{index + 1:00}{Delimiter}";
                        if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata))
                        {
                            throw new InvalidOperationException(video);
                        }

                        prefix = videoMetadata.PhysicalDefinitionType switch
                        {
                            DefinitionType.P1080 => $"{prefix}1080p{Delimiter}",
                            DefinitionType.P720 => $"{prefix}720p{Delimiter}",
                            _ => prefix
                        };

                        log(video);
                        string newVideo = PathHelper.AddFilePrefix(
                            video
                                .ReplaceIgnoreCase(".1080p", string.Empty).ReplaceIgnoreCase(".720p", string.Empty)
                                .ReplaceOrdinal("    ", " ").ReplaceOrdinal("   ", " ").ReplaceOrdinal("  ", " ")
                                .ReplaceOrdinal(" - ", TitleSeparator).ReplaceOrdinal("- ", TitleSeparator).ReplaceOrdinal(" -", TitleSeparator),
                            prefix);
                        if (!isDryRun)
                        {
                            File.Move(video, newVideo);
                        }

                        log(newVideo);
                        log(string.Empty);
                    });
            });
    }

    internal static void MoveTopTVEpisodes(ISettings settings, string directory, string subtitleBackupDirectory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .GetFiles(directory, "*.txt", SearchOption.AllDirectories)
            .ForEach(textFile =>
            {
                log($"Delete {textFile}");
                if (!isDryRun)
                {
                    FileHelper.Recycle(textFile);
                }
            });

        Directory
            .GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => PathHelper.GetFileNameWithoutExtension(file).EndsWithOrdinal(" (1)") && File.Exists(PathHelper.ReplaceFileNameWithoutExtension(file, name => name[..^" (1)".Length])))
            .ForEach(duplicateFile =>
            {
                log($"Delete {duplicateFile}");
                if (!isDryRun)
                {
                    FileHelper.Recycle(duplicateFile);
                }
            });

        Directory
            .GetDirectories(directory)
            .Where(season => season.ContainsIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}"))
            .ForEach(season => MoveSubtitlesForEpisodes(settings, season, season, subtitleBackupDirectory, false, isDryRun: isDryRun, log: log));

        Directory
            .GetDirectories(directory)
            .Where(season => season.ContainsIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}"))
            .ForEach(season =>
            {
                Match match = Regex.Match(PathHelper.GetFileName(season), @"^(.+)\.S([0-9]{2})(\.[A-Z]+)?\.1080p\.+");
                Debug.Assert(match.Success && match.Groups.Count is 3 or 4);
                string title = match.Groups[1].Value;
                string seasonNumber = match.Groups[2].Value;
                string tv = Path.Combine(directory, title);
                string newSeason = Path.Combine(tv, $"Season {seasonNumber}");
                log($"Move {season}");
                if (!isDryRun)
                {
                    DirectoryHelper.Move(season, newSeason);
                }

                log(newSeason);
                log(string.Empty);
            });
    }

    internal static void MoveFanArt(string directory, int level = DefaultDirectoryLevel, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] fanArts = Directory.GetFiles(directory, "*fanart*.jpg", SearchOption.AllDirectories);
        fanArts
            .ForEach(fanArt =>
            {
                string destinationFanArt = fanArt.ReplaceIgnoreCase(@"\extrafanart\", @"\");
                destinationFanArt = Regex.Replace(destinationFanArt, @"(\\|\-)fanart[0-9]+\.jpg$", "$1fanart.jpg");
                if (!fanArt.EqualsIgnoreCase(destinationFanArt))
                {
                    log(fanArt);
                    if (!isDryRun)
                    {
                        FileHelper.Move(fanArt, destinationFanArt, overwrite, true);
                    }

                    log(destinationFanArt);
                }
            });

        if (!isDryRun)
        {
            fanArts
                .Select(PathHelper.GetDirectoryName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(fanArtDirectory => PathHelper.GetFileName(fanArtDirectory).EqualsIgnoreCase("extrafanart") && Directory.EnumerateFileSystemEntries(fanArtDirectory).IsEmpty())
                .ToArray()
                .ForEach(DirectoryHelper.Recycle);
        }

        // EnumerateDirectories(directory, level)
        //     .AsParallel()
        //     .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
        //     .ForAll(movie =>
        //     {
        //         Directory
        //             .EnumerateFiles(movie, "*fanart*.jpg", SearchOption.AllDirectories)
        //             .Where(file => !file.EndsWithIgnoreCase("fanart.jpg"))
        //             .ToArray()
        //             .ForEach(file =>
        //             {
        //                 string destinationFile = Regex.Replace(file, @"fanart[0-9]+\.jpg$", "fanart.jpg");
        //                 if (File.Exists(destinationFile) && new FileInfo(destinationFile).LastWriteTimeUtc > new FileInfo(file).LastWriteTimeUtc)
        //                 {
        //                     FileHelper.Recycle(file);
        //                 }
        //                 else
        //                 {
        //                     log(file);
        //                     FileHelper.Move(file, destinationFile, overwrite);
        //                     log(destinationFile);
        //                 }
        //             });

        //         string fanArtDirectory = Path.Combine(movie, "extrafanart");
        //         if (!Directory.Exists(fanArtDirectory))
        //         {
        //             return;
        //         }

        //         string fanArt = Directory.GetFiles(fanArtDirectory).SingleOrDefault(string.Empty);
        //         if (fanArt.IsNotNullOrWhiteSpace())
        //         {
        //             Debug.Assert(PathHelper.GetFileName(fanArt).EqualsIgnoreCase("fanart.jpg"));

        //             string newFanArt = Path.Combine(movie, "fanart.jpg");
        //             Debug.Assert(overwrite || !File.Exists(newFanArt));

        //             log(fanArt);
        //             File.Move(fanArt, newFanArt, overwrite);
        //             log(newFanArt);
        //         }

        //         DirectoryHelper.Recycle(fanArtDirectory);
        //     });
    }

    internal static void RenameEpisodeWithoutTitle(ISettings settings, string directory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
        files
            .Where(IsVideo)
            .ForEach(video =>
            {
                log(video);
                if (VideoEpisodeFileInfo.TryParse(video, out VideoEpisodeFileInfo? episode) && episode.GetEncoderType() == EncoderType.TopX265 && episode.EpisodeTitle.IsNotNullOrWhiteSpace())
                {
                    episode = episode with { EpisodeTitle = string.Empty };
                    if (!isDryRun)
                    {
                        FileHelper.ReplaceFileName(video, episode.Name);
                    }

                    log(PathHelper.ReplaceFileName(video, episode.Name));
                    log(string.Empty);

                    string initial = video[..^PathHelper.GetExtension(video).Length];
                    string newInitial = Path.Combine(PathHelper.GetDirectoryName(video), PathHelper.GetFileNameWithoutExtension(episode.Name));
                    string[] attachments = files
                        .Where(file => !file.EqualsIgnoreCase(video) && file.StartsWithIgnoreCase(initial))
                        .ToArray();
                    attachments.ForEach(attachment =>
                    {
                        string newAttachment = attachment.Replace(initial, newInitial);
                        log(attachment);
                        if (!isDryRun)
                        {
                            FileHelper.Move(attachment, newAttachment);
                        }

                        log(newAttachment);
                        log(string.Empty);
                    });
                }
            });
    }

    internal static void FormatTV(ISettings settings, string mediaDirectory, string metadataDirectory, string subtitleDirectory = "", Func<string, string, string>? renameForTitle = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        metadataDirectory = metadataDirectory.IfNullOrWhiteSpace(mediaDirectory);

        (string Path, string Name)[] tvs = Directory
            .GetDirectories(mediaDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(tv => VideoDirectoryInfo.TryParse(tv, out _))
            .Select(tv => (Path: tv, Name: PathHelper.GetFileName(tv)))
            .OrderBy(tv => tv.Name)
            .ToArray();

        string[] existingMetadataTVs = Directory
            .GetDirectories(metadataDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .ToArray();
        tvs
            .Select(tv => (tv.Path, tv.Name, Metadata: existingMetadataTVs.Single(existingTV => tv.Name.Equals(PathHelper.GetFileName(existingTV)))))
            .ForEach(match => RenameEpisodesWithTitle(match.Path, match.Metadata, renameForTitle, false, isDryRun, log));

        string[] existingSubtitleTVs = subtitleDirectory.IsNullOrWhiteSpace()
            ? existingMetadataTVs
            : Directory
                .GetDirectories(subtitleDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .ToArray();
        tvs
            .Select(tv => (tv.Path, tv.Name, Subtitle: existingSubtitleTVs.Single(existingTV => tv.Name.Equals(PathHelper.GetFileName(existingTV)))))
            .ForEach(match => MoveSubtitlesForEpisodes(settings, match.Path, match.Subtitle, isDryRun: isDryRun, log: log));
    }

    internal static void RenameDirectoriesWithFormattedNumber(string directory, SearchOption? searchOption = null, bool isDryRun = false, Action<string>? log = null, params string[] titles)
    {
        log ??= Logger.WriteLine;

        titles
            .Select(title => title.Trim())
            .Where(title => title.IsNotNullOrWhiteSpace())
            .ForEach(keyword => RenameDirectories(
                directory,
                (f, i) => Regex.Replace(f, @$"(\\|\.){keyword}`([0-9]{{1}})(\-|\.|\[)", $"$1{keyword}`0$2$3"),
                searchOption: searchOption,
                isDryRun: isDryRun,
                log: log));
    }

    internal static void MoveDirectoriesWithMixedTranslation(string directory, int level, string destination) =>
        EnumerateDirectories(directory, level)
            .Where(d =>
            {
                string title = PathHelper.GetFileName(d);
                title = title[..title.IndexOfOrdinal("[")];
                string name = title[(title.LastIndexOfOrdinal(Delimiter) + 1)..];
                return string.IsNullOrWhiteSpace(name) || Regex.IsMatch(name, "[a-z]+");
            })
            .ToArray()
            .ForEach(movie => DirectoryHelper.MoveToDirectory(movie, destination));

    internal static void MoveVideosToDirectories(string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, searchOption);
        string[] videos = files.Where(IsVideo).ToArray();
        videos
            .Do(log)
            .ForEach(video =>
            {
                string videoDirectory = Path.Combine(PathHelper.GetDirectoryName(video), PathHelper.GetFileNameWithoutExtension(video));
                FileHelper.MoveToDirectory(video, videoDirectory);
                string videoWithoutExtension = video[..video.LastIndexOfOrdinal(".")];
                files
                    .Where(file => !file.IsVideo() && file.StartsWithIgnoreCase(videoWithoutExtension))
                    .ToArray()
                    .ForEach(file => FileHelper.MoveToDirectory(file, videoDirectory));
            });
    }

    internal static void FormatVideoFileNames(string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        List<string> videosWithErrors = [];

        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, searchOption)
            .Where(IsVideo)
            .ToArray()
            .ForEach(video =>
            {
                string videoName = PathHelper.GetFileNameWithoutExtension(video);

                string newVideoName = Regex.Replace(videoName, "[ ]+", " ", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]{0,}[\-]{1,}[ \.]{0,}", TitleSeparator, RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]{0,}[\(\[]{0,}([0-9]{4})[\)\]]{0,}", ".$1", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}(HD)?1080p[\)\]]{0,}", ".1080p", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}(HD)?720p[\)\]]{0,}", ".720p", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}[X|H][\.]?264[\)\]]{0,}", ".H264", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}(WEB[\-]?DL|WEBRip)[\)\]]{0,}", ".WEBRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}(Amazon\.WEBRip|AMZN\.WEBRip)[\)\]]{0,}", ".WEBRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}Xvid[\)\]]{0,}", ".Xvid", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}DivX[\)\]]{0,}", ".DivX", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}UNCUT[\)\]]{0,}", ".UNCUT", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}NTSC[\)\]]{0,}", ".UNCUT", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}DVD[0-9][\)\]]{0,}", ".DVDRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}DVDRip[\)\]]{0,}", ".DVDRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}VHSRip[\)\]]{0,}", ".VHSRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}LDRip[\)\]]{0,}", ".LDRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}HDRip[\)\]]{0,}", ".HDRip", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}2Audio[s]{0,}[\)\]]{0,}", ".2Audio", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}(BluRay|BDRip)[\)\]]{0,}", ".BluRay", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}DD[0-9]\.[0-9][\)\]]{0,}", ".DD", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}DDP[0-9]\.[0-9][\)\]]{0,}", ".DDP", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}FLAC\.[0-9]\.[0-9][\)\]]{0,}", "", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}[1-9]AC3[\)\]]{0,}", ".AC3", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}[1-9]AAC[\)\]]{0,}", ".AAC", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}cd1[\)\]]{0,}", ".cd1", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}cd2[\)\]]{0,}", ".cd2", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}mkv[\)\]]{0,}", "", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+[\(\[]{0,}mp4[\)\]]{0,}", "", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]*[\(\[]{0,}18\+[\)\]]{0,}", "", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]*[\(\[]{0,}\+18[\)\]]{0,}", "", RegexOptions.IgnoreCase);
                //newVideoName = Regex.Replace(newVideoName, @"\-([^\-]+)[ \-]([^\-]+)", "-$1@$2", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]{1,}[\(\[]{0,}DVD[\)\]]{0,}[ \.]]{1,}", ".DVDRip.", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"(.+)\.([0-9]{4})[ \.]aka[ \.](.+)", "$1.Aka.$3.$2", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+([12-9])Audio\-([a-zA-Z]+)", "-$2.$1Audio", RegexOptions.IgnoreCase);
                newVideoName = Regex.Replace(newVideoName, @"[ \.]+cd([12-9])\-([a-zA-Z]+)", "-$2.cd$1", RegexOptions.IgnoreCase);
                newVideoName = newVideoName
                    .ReplaceIgnoreCase("：", TitleSeparator)
                    .ReplaceIgnoreCase(".BluRay.1080p", ".1080p.BluRay")
                    .ReplaceIgnoreCase(".WEBRip.1080p", ".1080p.WEBRip")
                    .ReplaceIgnoreCase(".AC3.H264", ".H264.AC3")
                    .ReplaceIgnoreCase(".DD.H264", ".H264.DD")
                    .ReplaceIgnoreCase(".DDP.H264", ".H264.DDP")
                    .ReplaceIgnoreCase(" DVD.", ".DVDRip.")
                    .ReplaceIgnoreCase(".D9.", ".DVDRip.")
                    .ReplaceIgnoreCase(".WEB.", ".WEBRip.")
                    .ReplaceIgnoreCase(".MiniSD-TLF", "-MiniSD@TLF")
                    .Trim();

                if (videoName.Equals(newVideoName))
                {
                    return;
                }

                log(video);
                string newVideo = PathHelper.ReplaceFileNameWithoutExtension(video, newVideoName);
                if (!VideoMovieFileInfo.TryParse(newVideo, out _))
                {
                    videosWithErrors.Add(newVideo);
                }

                if (!isDryRun)
                {
                    FileHelper.Move(video, newVideo);
                }

                log(newVideo);
                log(string.Empty);

                Directory.EnumerateFiles(PathHelper.GetDirectoryName(video))
                    .Where(file => !file.IsVideo() && PathHelper.GetFileNameWithoutExtension(file).StartsWith(videoName))
                    .ToArray()
                    .ForEach(file =>
                    {
                        log(file);
                        string newFile = PathHelper.ReplaceFileNameWithoutExtension(file, fileName => fileName.ReplaceIgnoreCase(videoName, newVideoName));
                        if (!isDryRun)
                        {
                            FileHelper.Move(file, newFile);
                        }

                        log(newFile);
                        log(string.Empty);
                    });
            });

        videosWithErrors.Prepend(string.Empty).ForEach(log);
    }

    internal static void FormatDirectoriesWithVideo(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string video = Directory.EnumerateFiles(movie).First(IsVideo);
                log(movie);
                string newMovie = PathHelper.ReplaceDirectoryName(directory, PathHelper.GetFileNameWithoutExtension(video));
                DirectoryHelper.Move(movie, newMovie);
                log(newMovie);
            });
    }

    internal static void MoveDirectoriesByRegions(ISettings settings, string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ILookup<string, (string[] Genres, string SubDirectory)> regionWithGenres = settings
            .MovieRegionDirectories
            .Where(pair => pair.Key.ContainsOrdinal(Delimiter))
            .Select(pair =>
            {
                string[] keys = pair.Key.Split(Delimiter);
                string region = keys.First();
                string[] genres = keys.Last().Split("|");
                return (region, genres, pair.Value);
            })
            .ToLookup(pair => pair.region, pair => (pair.genres, pair.Value), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> regionWithoutGenres = settings
            .MovieRegionDirectories
            .Where(pair => !pair.Key.ContainsOrdinal(Delimiter))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string metadataFile = Directory.EnumerateFiles(movie, ImdbMetadataSearchPattern).Single();
                if (!ImdbMetadata.TryGet(metadataFile, out _, out _, out string[]? regions, out string[]? languages, out string[]? genres))
                {
                    return;
                }

                string region = regions.First();
                if (regionWithGenres.Contains(region))
                {
                    (string[] Genres, string SubDirectory)[] matches = regionWithGenres[region]
                        .Where(group => genres.Intersect(group.Genres, StringComparer.OrdinalIgnoreCase).Any())
                        .ToArray();
                    if (matches.Any())
                    {
                        (string[] Genres, string SubDirectory)[] matchesWithoutDrama = matches.Where(match => !match.Genres.ContainsIgnoreCase("Drama")).ToArray();
                        string subDirectory = Path.Combine(directory, (matchesWithoutDrama.Any() ? matchesWithoutDrama : matches).First().SubDirectory);
                        log(movie);
                        log(subDirectory);
                        if (!isDryRun)
                        {
                            DirectoryHelper.MoveToDirectory(movie, subDirectory);
                        }

                        log(string.Empty);
                        return;
                    }
                }

                if (regionWithoutGenres.TryGetValue(region, out string? value))
                {
                    string subDirectory = Path.Combine(directory, value);
                    log(movie);
                    log(subDirectory);
                    if (!isDryRun)
                    {
                        DirectoryHelper.MoveToDirectory(movie, subDirectory);
                    }

                    log(string.Empty);
                    return;
                }

                log($"!{movie}");
            });
    }

    private static readonly string[] OrderedKeywords =
    [
        "bottomless",
        "closeup penetration",
        "female full frontal nudity",
        "female frontal nudity",
        "female explicit nudity",
        "female genitalia",
        "female topless nudity",
        "female nipple",
        "female nipples",
        "female pubic hair",
        "female nudity",
        "female exhibitionist",
        "female star appears nude",
        "clothed male naked female scene",
        "clothed female naked female scene",
        "blindfolded nude woman",
        "female star appears topless",
    ];

    internal static void RenameDirectoriesWithGraphicMetadata(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] directories = EnumerateDirectories(directory, level).ToArray();
        directories.Where(DirectoryHelper.IsHidden).Prepend("Hidden backup").Append(string.Empty).ForEach(log);
        directories
            .ForEach(movie =>
            {
                if (!ImdbMetadata.TryLoad(movie, out ImdbMetadata? metadata))
                {
                    return;
                }

                string name = PathHelper.GetFileName(movie);
                int index = name.LastIndexOfOrdinal(AdditionalMetadataSeparator);
                if (index >= 0)
                {
                    name = name[..index];
                }

                string severity = string.Join(
                    VersionSeparator,
                    metadata.
                        Advisories
                        .Where(advisory => advisory.Key.ContainsIgnoreCase("sex"))
                        .SelectMany(advisory => advisory.Value)
                        .Select(advisory => advisory.Severity));
                string keywords = string.Join(VersionSeparator, OrderedKeywords.Intersect(metadata.AllKeywords).Take(3));
                log(movie);
                string newMovie = DirectoryHelper.ReplaceDirectoryName(movie, $"{name}{AdditionalMetadataSeparator}{severity}{Delimiter}{keywords}");
                log(newMovie);
            });
    }

    internal static void RenameNikkatsuDirectories(bool isDryRun = false, Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        directories.SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .Where(movie => !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Nikkatsu") && !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Oniroku Dan"))
            .Select(movie =>
            {
                ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata);
                return (movie, XDocument.Load(Path.Combine(movie, TmdbMetadata.MovieNfoFile)), imdbMetadata);
            })
            .Where(movie => movie.Item2.Root?.Elements("studio").Any(element => element.Value.ContainsIgnoreCase("Nikkatsu")) is true
                && movie.Item2.Root?.Elements("tag").Any(element => element.Value.ContainsIgnoreCase("pink")) is true
                || movie.imdbMetadata?.Companies?.Keys.Any(company => company.ContainsIgnoreCase("Nikkatsu")) is true
                && movie.imdbMetadata?.AllKeywords.Any(keyword => keyword.ContainsIgnoreCase("roman porno") || keyword.ContainsIgnoreCase("pink")) is true)
            .ForEach(movie =>
            {
                log(string.Join(", ", movie.Item2.Root?.Elements("studio").Select(element => element.Value) ?? []));
                log(string.Join(", ", movie.Item2.Root?.Elements("tag").Select(element => element.Value) ?? []));
                log(string.Join(", ", movie.imdbMetadata?.Companies?.Keys.AsEnumerable() ?? []));
                log(string.Join(", ", movie.imdbMetadata?.AllKeywords.Where(keyword => keyword.ContainsIgnoreCase("roman porno") || keyword.ContainsIgnoreCase("pink")) ?? []));
                log(movie.movie);
                string newMovie = PathHelper.ReplaceDirectoryName(movie.movie, name => "Nikkatsu Pink-" + Regex.Replace(name, @"\.[0-9]{4}\.", "$0日活粉画-"));
                if (!isDryRun)
                {
                    DirectoryHelper.Move(movie.movie, newMovie);
                }

                log(newMovie);
                log(string.Empty);
            });
    }

    internal static void RenameDirectoriesWithDigits(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        int count = 0;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                Debug.Assert(VideoDirectoryInfo.TryParse(movie, out _));

                string name = PathHelper.GetFileName(movie);
                string title = name[..name.IndexOfOrdinal(".")];
                title = title[..(title.IndexOfOrdinal("=") > 0 ? title.IndexOfOrdinal("=") : title.Length)];
                if (!StringHelper.TryReplaceNumbers(title, out string newTitle))
                {
                    return;
                }

                if (isDryRun)
                {
                    log(title);
                    log(newTitle);
                }
                else
                {
                    log(movie);
                    string newMovie = DirectoryHelper.ReplaceDirectoryName(movie, movieName => newTitle + name[title.Length..]);
                    log(newMovie);
                }

                log(string.Empty);
                count++;
            });

        log(count.ToString());
    }

    internal static async Task ReplaceTopWebWithPreferredBluRay(ISettings settings, string deleteParentDirectory, string directory, int level = DefaultDirectoryLevel, Action<string>? log = null, CancellationToken cancellationToken = default, params string[] drives)
    {
        log ??= Logger.WriteLine;

        ILookup<string, string> libraryImdbIdToMetadataFiles = drives
            .AsParallel()
            .SelectMany(drive => Directory.EnumerateFiles(drive, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .Where(file => !file.ContainsIgnoreCase($"{Path.DirectorySeparatorChar}Delete"))
            .ToLookup(json => ImdbMetadata.TryGet(json, out string? imdbId) ? imdbId : string.Empty, json => json);
        ConcurrentDictionary<string, List<PreferredFileMetadata>> preferredFilesMetadata = await settings.LoadMoviePreferredFileMetadataAsync(cancellationToken);
        ILookup<string, string> preferredDisplayNameToImdbIds = preferredFilesMetadata.Values.Concat()
            .AsParallel()
            .ToLookup(metadata => metadata.DisplayName, metadata => metadata.ImdbId);
        (string Movie, string DisplayName)[] displayNames = EnumerateDirectories(directory, level)
            .Select(movie => (Movie: movie, DisplayName: PathHelper.GetFileName(movie)))
            .Order()
            .ToArray();
        await displayNames.ForEachAsync(
            async displayName =>
            {
                string[] sourceFiles = Directory.GetFiles(displayName.Movie);
                string sourceVideo = sourceFiles.Single(file => file.IsVideo());
                string imdbId = preferredDisplayNameToImdbIds[displayName.DisplayName].Distinct().Single();
                string destinationMetadata = libraryImdbIdToMetadataFiles[imdbId].Distinct().Single();
                string destinationDirectory = PathHelper.GetDirectoryName(destinationMetadata);
                string[] destinationFiles = Directory.GetFiles(destinationDirectory);
                string[] destinationVideos = destinationFiles.Where(file => file.IsVideo()).ToArray();
                if (destinationVideos.Length != 1)
                {
                    Debugger.Break();
                    log($"{displayName.DisplayName} {imdbId} {destinationDirectory}");
                    DirectoryHelper.MoveToDirectory(displayName.Movie, PathHelper.AddDirectoryPostfix(directory, ".Manual"));
                    return;
                }

                //log(movie);
                //Directory.GetFiles(displayName.Item1).ForEach(file => FileHelper.MoveToDirectory(file, movie));
                //string xml = files.First(file => file.IsTmdbNfoMetadata());
                //File.Copy(xml, Path.Combine(movie, PathHelper.ReplaceExtension(PathHelper.GetFileName(sourceVideo), ".nfo")));
                //if (movie.Contains("[1080Y") || movie.Contains("[1080Z"))
                //{
                //    DirectoryHelper.MoveToDirectory(displayName.Item1, @"I:\Yts\Preferred");
                //}

                //if (movie.Contains("[1080X") || movie.Contains("[1080H"))
                //{
                //    DirectoryHelper.MoveToDirectory(displayName.Item1, @"I:\Yts\Top");
                //}

                string destinationVideo = destinationVideos.Single();
                switch (await FfmpegHelper.CompareDurationAsync(sourceVideo, destinationVideo))
                {
                    case > 0:
                        log(DirectoryHelper.MoveToDirectory(displayName.Item1, PathHelper.AddDirectoryPostfix(directory, ".Longer")));
                        break;
                    case < 0:
                        log(DirectoryHelper.MoveToDirectory(displayName.Item1, PathHelper.AddDirectoryPostfix(directory, ".Shorter")));
                        break;
                    default:
                        {
                            log(displayName.Movie);
                            log(destinationDirectory);

                            string destinationDirectoryName = PathHelper.GetFileName(destinationDirectory);
                            string deletedDirectory = Path.Combine(deleteParentDirectory, destinationDirectoryName);
                            Directory.CreateDirectory(deletedDirectory);
                            if (DirectoryHelper.IsHidden(destinationDirectory))
                            {
                                DirectoryHelper.SetHidden(deletedDirectory, true);
                            }

                            string[] destinationFilesToMove = destinationFiles.Where(destinationFile => destinationFile.IsSubtitle() || destinationFile.IsVideo()).Order().ToArray();
                            string[] destinationFilesToCopy = destinationFiles.Except(destinationFilesToMove).ToArray();
                            destinationFilesToCopy.ForEach(destinationFile => FileHelper.CopyToDirectory(destinationFile, deletedDirectory));
                            string destinationXmlMetadata = destinationFiles.Single(destinationFile => destinationFile.IsTmdbNfoMetadata() && PathHelper.GetFileNameWithoutExtension(destinationFile).EqualsOrdinal(PathHelper.GetFileNameWithoutExtension(destinationVideo)));
                            FileHelper.ReplaceFileNameWithoutExtension(destinationXmlMetadata, PathHelper.GetFileNameWithoutExtension(sourceVideo));
                            sourceFiles.ForEach(sourceFile => FileHelper.MoveToDirectory(sourceFile, destinationDirectory));
                            destinationFilesToMove
                                .Do(destinationFile => log($"Move {destinationFile}"))
                                .ForEach(destinationFile => FileHelper.MoveToDirectory(destinationFile, deletedDirectory));
                            log("");
                            break;
                        }
                }
            },
            cancellationToken);
    }

    internal static void DeleteSpecialCharacters(string directory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        char[] directorySpecialCharacters = ['_', '~', '#', '^', '(', ')', '{', '}', '@', '\u3000'];
        char[] fileSpecialCharacters = ['_', '~', '#', '^', '(', ')', '{', '}', '\u3000'];

        string[] subDirectories = Directory.GetDirectories(directory);
        bool hasSubDirectoryRenamed = false;
        subDirectories
            .ForEach(subDirectory =>
            {
                string subDirectoryName = PathHelper.GetFileName(subDirectory);
                string newSubDirectoryName = subDirectoryName;
                directorySpecialCharacters.ForEach(character => newSubDirectoryName = newSubDirectoryName.Replace(character, ' '));
                newSubDirectoryName = newSubDirectoryName.Replace('–', '-');
                newSubDirectoryName = newSubDirectoryName.Trim(' ').Trim('.');
                newSubDirectoryName = Regex.Replace(newSubDirectoryName, @"([ ])+", "$1");
                newSubDirectoryName = Regex.Replace(newSubDirectoryName, @"([\.])+", "$1");
                //newSubDirectoryName = Regex.Replace(newSubDirectoryName, @"([\-])+", "$1");
                newSubDirectoryName = Regex.Replace(newSubDirectoryName, @"[\s]+([\-\=\.\]\[])[\s]+", "$1");
                newSubDirectoryName = Regex.Replace(newSubDirectoryName, @"[\s]+([\-\=\.\]\[])", "$1");
                newSubDirectoryName = Regex.Replace(newSubDirectoryName, @"([\-\=\.\]\[])[\s]+", "$1");
                if (newSubDirectoryName.EqualsIgnoreCase(subDirectoryName))
                {
                    return;
                }

                log(subDirectory);
                log(isDryRun ? PathHelper.ReplaceDirectoryName(subDirectory, newSubDirectoryName) : DirectoryHelper.ReplaceDirectoryName(subDirectory, newSubDirectoryName));
                log("");
                hasSubDirectoryRenamed = true;
            });

        Directory.EnumerateFiles(directory)
            .Where(file => !file.IsImdbMetadata() && !file.IsTmdbXmlMetadata())
            .ToArray()
            .ForEach(file =>
            {
                string fileName = PathHelper.GetFileNameWithoutExtension(file);
                string extension = PathHelper.GetExtension(file);
                string newFileName = fileName;
                fileSpecialCharacters.ForEach(character => newFileName = newFileName.Replace(character, ' '));
                newFileName = newFileName.Replace('–', '-');
                newFileName = newFileName.Trim(' ').Trim('.');
                newFileName = Regex.Replace(newFileName, @"([ ])+", "$1");
                //newFileName = Regex.Replace(newFileName, @"([\.])+", "$1");
                newFileName = Regex.Replace(newFileName, @"([\-])+", "$1");
                newFileName = Regex.Replace(newFileName, @"[\s]+([\-\=\.\]\[])[\s]+", "$1");
                newFileName = Regex.Replace(newFileName, @"[\s]+([\-\=\.\]\[])", "$1");
                newFileName = Regex.Replace(newFileName, @"([\-\=\.\]\[])[\s]+", "$1");
                if (newFileName.EqualsIgnoreCase(fileName))
                {
                    return;
                }

                log(file);
                string newFile = isDryRun ? PathHelper.ReplaceFileNameWithoutExtension(file, newFileName) : FileHelper.ReplaceFileNameWithoutExtension(file, newFileName);
                log(newFileName.EndsWithIgnoreCase(extension) ? $"!{newFile}" : newFile);
                log("");
            });

        if (hasSubDirectoryRenamed)
        {
            subDirectories = Directory.GetDirectories(directory);
        }

        subDirectories.ForEach(subDirectory => DeleteSpecialCharacters(subDirectory, isDryRun, log));
    }
}
