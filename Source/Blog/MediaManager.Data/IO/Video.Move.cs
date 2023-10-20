namespace Examples.IO;

using Examples.Common;
using Examples.Net;
using Examples.Text;
using System.Linq;

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

            List<string> info = (Path.GetFileName(file) ?? throw new InvalidOperationException(file)).Split(".").ToList();
            if (info.Count <= 3)
            {
                // Space.
                info = Path.GetFileNameWithoutExtension(file).Split(" ").Append(info.Last()).ToList();
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
            string newFile = string.Join(".", info);
            string directory = Path.GetDirectoryName(file) ?? throw new InvalidOperationException(file);
            log(file);
            log(Path.Combine(directory, newFile));
            File.Move(file, Path.Combine(directory, newFile));
            Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                .Where(attachment => attachment != file)
                .Where(attachment => (Path.GetFileName(attachment) ?? throw new InvalidOperationException(file)).StartsWithIgnoreCase(Path.GetFileNameWithoutExtension(file)))
                .ToList()
                .ForEach(attachment =>
                {
                    string newAttachment = Path.Combine(directory, (Path.GetFileName(attachment) ?? throw new InvalidOperationException(file)).Replace(Path.GetFileNameWithoutExtension(file), string.Join(".", info.Take(..^1))));
                    log(newAttachment);
                    File.Move(attachment, newAttachment);
                });
        });
    }

    internal static void RenameEpisodesWithTitle(string mediaDirectory, string metadataDirectory = "", Func<string, string, string>? rename = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        if (metadataDirectory.IsNullOrWhiteSpace())
        {
            metadataDirectory = mediaDirectory;
        }

        rename ??= (file, title) => PathHelper.AddFilePostfix(file, $".{title}");

        Directory
            .GetFiles(metadataDirectory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .ForEach(nfo =>
            {
                string match = Regex.Match(Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException($"{nfo} is invalid."), @"S[\d]+E[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                if (match.IsNullOrWhiteSpace() || !File.Exists(nfo))
                {
                    return;
                }

                string title = XDocument.Load(nfo).Root?.Element("title")?.Value.FilterForFileSystem().Trim() ?? throw new InvalidOperationException($"{nfo} has no title.");
                Directory
                    .GetFiles(mediaDirectory, $"*{match}*", SearchOption.AllDirectories)
                    .ForEach(file =>
                    {
                        log(file);
                        string newFile = rename(file, title).Trim();
                        if (!isDryRun)
                        {
                            File.Move(file, newFile);
                        }

                        log(newFile);
                    });
            });
    }

    internal static void RenameVideosWithDefinition(string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool isDryRun = false, Action<string>? log = null) =>
        RenameVideosWithDefinition(
            Directory.GetFiles(directory, PathHelper.AllSearchPattern, searchOption)
                .Where(file => AllVideoExtensions.Any(file.EndsWithIgnoreCase))
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
                return (Definition: match.Groups[1].Value, File: path, Extension: Path.GetExtension(Path.GetFileNameWithoutExtension(path)));
            })
            .ForEach(result =>
            {
                string extension = Path.GetExtension(result.File);
                string newFile;
                if (result.File.HasAnyExtension(AllVideoExtensions))
                {
                    string file = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(result.File));
                    newFile = Path.Combine(Path.GetDirectoryName(result.File) ?? throw new InvalidOperationException(result.File), $"{file}.{result.Definition}{result.Extension}{extension}");
                }
                else
                {
                    string file = Path.GetFileNameWithoutExtension(result.File);
                    newFile = Path.Combine(Path.GetDirectoryName(result.File) ?? throw new InvalidOperationException(result.File), $"{file}.{result.Definition}{extension}");
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

    internal static void RenameDirectoriesWithAdditionalMetadata(string directory, int level = 2, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string movieName = Path.GetFileName(movie);
                if (!overwrite && movieName.ContainsOrdinal("{"))
                {
                    return;
                }

                Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata);
                string additional = $"@{string.Join(",", imdbMetadata?.Regions.Take(4) ?? Array.Empty<string>())}#{string.Join(",", imdbMetadata?.Languages.Take(3) ?? Array.Empty<string>())}";
                string originalMovie = movieName.ContainsOrdinal("{")
                    ? PathHelper.ReplaceFileName(movie, movieName.Substring(0, movieName.IndexOfOrdinal("@")))
                    : movie;
                string newMovie = $"{originalMovie}{additional}";
                log(movie);
                if (!isDryRun)
                {
                    Directory.Move(movie, newMovie);
                }

                log(newMovie);
            });
    }

    internal static void RenameDirectoriesWithMetadata(string directory, int level = 2, bool additionalInfo = false, bool overwrite = false, bool isDryRun = false, string backupFlag = "backup", bool isTV = false, bool skipRenamed = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                if (!overwrite && Path.GetFileName(movie).ContainsOrdinal("{"))
                {
                    return;
                }

                if (skipRenamed && VideoDirectoryInfo.TryParse(movie, out _))
                {
                    return;
                }

                string[] files = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).OrderBy(file => file).ToArray();
                string[] nfos = files.Where(file => file.HasExtension(XmlMetadataExtension)).ToArray();
                XDocument english;
                XDocument? translated;
                if (isTV)
                {
                    english = XDocument.Load(nfos.Single(nfo => Path.GetFileName(nfo).EqualsIgnoreCase(TVShowMetadataFile)));
                    translated = null;
                }
                else
                {
                    if (nfos.Any(nfo => nfo.EndsWithIgnoreCase($".{backupFlag}{XmlMetadataExtension}")) && nfos.Any(nfo => !nfo.EndsWithIgnoreCase($".{backupFlag}{XmlMetadataExtension}")))
                    {
                        english = XDocument.Load(nfos.First(nfo => nfo.EndsWithIgnoreCase($".{backupFlag}{XmlMetadataExtension}")));
                        translated = XDocument.Load(nfos.First(nfo => !nfo.EndsWithIgnoreCase($".{backupFlag}{XmlMetadataExtension}")));
                    }
                    else
                    {
                        english = XDocument.Load(nfos.First());
                        translated = null;
                    }
                }

                string json = files.Single(file => file.HasExtension(ImdbMetadataExtension));
                Imdb.TryLoad(json, out ImdbMetadata? imdbMetadata);

                string defaultTitle = english.Root?.Element("title")?.Value ?? throw new InvalidOperationException($"{movie} has no default title.");
                defaultTitle = defaultTitle.ReplaceOrdinal(" - ", "-");
                string translatedTitle = translated?.Root?.Element("title")?.Value ?? string.Empty;
                translatedTitle = translatedTitle.ReplaceOrdinal(" - ", "-");
                string originalTitle = imdbMetadata?.OriginalTitle ?? english.Root?.Element("originaltitle")?.Value ?? imdbMetadata?.Name ?? string.Empty;
                originalTitle = originalTitle.ReplaceOrdinal(" - ", "-");
                originalTitle = originalTitle.EqualsIgnoreCase(defaultTitle) || originalTitle.IsNullOrWhiteSpace()
                    ? string.Empty
                    : $"={originalTitle}";
                string year = imdbMetadata?.Year ?? english.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{movie} has no year.");
                string? imdbId = english.Root?.Element("imdbid")?.Value ?? english.Root?.Element("imdb_id")?.Value;
                Debug.Assert(imdbId.IsNullOrWhiteSpace()
                    ? NotExistingFlag.EqualsOrdinal(Path.GetFileNameWithoutExtension(json))
                    : imdbId.EqualsIgnoreCase(Path.GetFileNameWithoutExtension(json).Split("-")[0]));
                string rating = imdbMetadata?.FormattedAggregateRating ?? NotExistingFlag;
                string ratingCount = imdbMetadata?.FormattedAggregateRatingCount ?? NotExistingFlag;
                string[] videos = files.Where(IsVideo).ToArray();
                string contentRating = imdbMetadata?.FormattedContentRating ?? NotExistingFlag;
                VideoMovieFileInfo[] videoFileInfos = videos.Select(VideoMovieFileInfo.Parse).ToArray();
                VideoDirectoryInfo videoDirectoryInfo = new(
                    DefaultTitle1: defaultTitle.ReplaceOrdinal(" - ", " ").ReplaceOrdinal("-", " ").ReplaceOrdinal(".", " ").FilterForFileSystem().Trim(), DefaultTitle2: string.Empty, DefaultTitle3: string.Empty,
                    OriginalTitle1: originalTitle.ReplaceOrdinal(" - ", " ").ReplaceOrdinal("-", " ").ReplaceOrdinal(".", " ").FilterForFileSystem().Trim(), OriginalTitle2: string.Empty, OriginalTitle3: string.Empty,
                    Year: year,
                    TranslatedTitle1: translatedTitle.ReplaceOrdinal(" - ", " ").ReplaceOrdinal("-", " ").ReplaceOrdinal(".", " ").ReplaceIgnoreCase("：", "-").FilterForFileSystem().Trim(), TranslatedTitle2: string.Empty, TranslatedTitle3: string.Empty, TranslatedTitle4: string.Empty,
                    AggregateRating: rating, AggregateRatingCount: ratingCount,
                    ContentRating: contentRating,
                    Resolution: isTV ? string.Empty : VideoDirectoryInfo.GetResolution(videoFileInfos),
                    Source: isTV ? string.Empty : VideoDirectoryInfo.GetSource(videoFileInfos),
                    Is3D: string.Empty,
                    Hdr: string.Empty
                );
                string additional = additionalInfo
                    ? $"{{{string.Join(",", imdbMetadata?.Regions.Take(5) ?? Array.Empty<string>())};{string.Join(",", imdbMetadata?.Genres.Take(3) ?? Array.Empty<string>())}}}"
                    : string.Empty;
                string newMovie = $"{videoDirectoryInfo}{additional}";
                string newDirectory = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), newMovie);
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

    internal static void RenameDirectoriesWithoutAdditionalMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string movieName = Path.GetFileName(movie) ?? throw new InvalidOperationException(movie);
                bool isRenamed = false;
                if (movieName.StartsWithOrdinal("0."))
                {
                    movieName = movieName.Substring("0.".Length);
                    isRenamed = true;
                }

                if (movieName.ContainsOrdinal("]@"))
                {
                    movieName = movieName[..(movieName.IndexOfOrdinal("]@") + 1)];
                    isRenamed = true;
                }

                if (isRenamed)
                {
                    string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), movieName);
                    log(movie);
                    if (!isDryRun)
                    {
                        Directory.Move(movie, newMovie);
                    }
                    log(newMovie);
                }
            });
    }

    internal static void RenameDirectoriesWithImdbMetadata(string directory, int level = 2, bool isTV = false, bool isDryRun = false, Action<string>? log = null)
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

                if (Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata))
                {
                    if (imdbMetadata.Year.IsNotNullOrWhiteSpace())
                    {
                        parsed = parsed with { Year = imdbMetadata.Year };
                    }

                    parsed = parsed with
                    {
                        AggregateRating = imdbMetadata.FormattedAggregateRating,
                        AggregateRatingCount = imdbMetadata.FormattedAggregateRatingCount,
                        ContentRating = imdbMetadata.FormattedContentRating
                    };

                    if (imdbMetadata.Year.IsNotNullOrWhiteSpace())
                    {
                        parsed = parsed with
                        {
                            Year = imdbMetadata.Year
                        };
                    }
                }

                if (isTV)
                {
                    VideoEpisodeFileInfo[] episodes = VideoDirectoryInfo.GetEpisodes(movie).ToArray();
                    parsed = parsed with
                    {
                        Resolution = VideoDirectoryInfo.GetResolution(episodes),
                        Source = VideoDirectoryInfo.GetSource(episodes)
                    };
                }
                else
                {
                    VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetVideos(movie).ToArray();
                    parsed = parsed with
                    {
                        Resolution = VideoDirectoryInfo.GetResolution(videos),
                        Source = VideoDirectoryInfo.GetSource(videos)
                    };
                }


                string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), parsed.ToString());

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

    internal static void RenameMovies(string destination, string directory, int level = 2, string field = "genre", string? value = null, bool isDryRun = false)
    {
        RenameMovies(
            (movie, _) => Path.Combine(destination, Path.GetFileName(movie)),
            directory,
            level,
            (_, metadata) => value.EqualsIgnoreCase(metadata.Root?.Element(field)?.Value),
            isDryRun);
    }

    internal static void RenameMovies(Func<string, XDocument, string> rename, string directory, int level = 2, Func<string, XDocument, bool>? predicate = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .Select(movie => (Directory: movie, Metadata: XDocument.Load(Directory.GetFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly).First())))
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

    internal static async Task CompareAndMoveAsync(string fromJsonPath, string toJsonPath, string newDirectory, string deletedDirectory, Action<string>? log = null, bool isDryRun = false, bool moveAllAttachment = true)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, VideoMetadata> externalMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(fromJsonPath)) ?? throw new InvalidOperationException(fromJsonPath);
        Dictionary<string, Dictionary<string, VideoMetadata>> moviesMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(toJsonPath)) ?? throw new InvalidOperationException(toJsonPath);

        externalMetadata
            .Where(externalVideo => File.Exists(externalVideo.Value.File))
            .ForEach(externalVideo =>
            {
                VideoMetadata fromVideoMetadata = externalVideo.Value;
                string fromMovie = Path.GetDirectoryName(fromVideoMetadata.File) ?? throw new InvalidOperationException(fromVideoMetadata.File);
                log($"Starting {fromMovie}");

                if (!moviesMetadata.TryGetValue(externalVideo.Key, out Dictionary<string, VideoMetadata>? group) || group.IsEmpty())
                {
                    string newExternalMovie = Path.Combine(newDirectory, Path.GetFileName(fromMovie));
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
                toVideoMetadata = toVideoMetadata with { File = Path.Combine(Path.GetDirectoryName(toJsonPath) ?? throw new ArgumentException(toJsonPath), toVideoMetadata.File) };
                if (((IVideoFileInfo)VideoMovieFileInfo.Parse(Path.GetFileNameWithoutExtension(toVideoMetadata.File))).EncoderType is EncoderType.X)
                {
                    log($"Video {toVideoMetadata.File} is x265.");
                    return;
                }

                if (!File.Exists(toVideoMetadata.File))
                {
                    log($"Video {toVideoMetadata.File} does not exist.");
                    return;
                }

                if (((IVideoFileInfo)VideoMovieFileInfo.Parse(Path.GetFileNameWithoutExtension(toVideoMetadata.File))).EncoderType is EncoderType.X or EncoderType.H
                    && ((IVideoFileInfo)VideoMovieFileInfo.Parse(Path.GetFileNameWithoutExtension(fromVideoMetadata.File))).EncoderType is not EncoderType.X or EncoderType.H)
                {
                    log($"Video {toVideoMetadata.File} is better version.");
                    return;
                }

                if (toVideoMetadata.Width - fromVideoMetadata.Width > 5)
                {
                    log($"Width {fromVideoMetadata.File} {fromVideoMetadata.Width} {toVideoMetadata.Width}");
                    return;
                }

                if (toVideoMetadata.Height - fromVideoMetadata.Height > 20)
                {
                    log($"Height {fromVideoMetadata.File} {fromVideoMetadata.Height} {toVideoMetadata.Height}");
                    return;
                }

                if (Math.Abs(toVideoMetadata.TotalMilliseconds - fromVideoMetadata.TotalMilliseconds) > 1100)
                {
                    log($"Duration {fromVideoMetadata.TotalMilliseconds}ms to old {toVideoMetadata.TotalMilliseconds}ms: {fromVideoMetadata.File}.");
                    return;
                }

                if (toVideoMetadata.Audio - fromVideoMetadata.Audio > 0)
                {
                    log($"Audio {fromVideoMetadata.File} {fromVideoMetadata.Audio} {toVideoMetadata.Audio}");
                    return;
                }

                if (toVideoMetadata.Subtitle - fromVideoMetadata.Subtitle > 0)
                {
                    log($"Subtitle {fromVideoMetadata.File} {fromVideoMetadata.Subtitle} {toVideoMetadata.Subtitle}");
                    return;
                }

                string toMovie = Path.GetDirectoryName(toVideoMetadata.File) ?? throw new InvalidOperationException(toVideoMetadata.File);
                string newExternalVideo = Path.Combine(toMovie, Path.GetFileName(fromVideoMetadata.File));
                if (!isDryRun)
                {
                    File.Move(fromVideoMetadata.File, newExternalVideo);
                }

                log($"Move external video {fromVideoMetadata.File} to {newExternalVideo}");

                Directory.GetFiles(fromMovie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(file => moveAllAttachment || Path.GetFileNameWithoutExtension(file).ContainsIgnoreCase(Path.GetFileNameWithoutExtension(fromVideoMetadata.File)))
                    .ToArray()
                    .ForEach(fromAttachment =>
                    {
                        string toAttachment = Path.Combine(toMovie, Path.GetFileName(fromAttachment));
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
                    .Where(file => Path.GetFileNameWithoutExtension(file).ContainsIgnoreCase(Path.GetFileNameWithoutExtension(toVideoMetadata.File)))
                    .ToArray()
                    .ForEach(existingAttachment =>
                    {
                        string newExistingAttachment = Path.Combine(toMovie, Path.GetFileName(existingAttachment).Replace(Path.GetFileNameWithoutExtension(toVideoMetadata.File), Path.GetFileNameWithoutExtension(externalVideo.Value.File)));
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

                string deletedFromMovie = Path.Combine(deletedDirectory, Path.GetFileName(fromMovie));
                if (!isDryRun)
                {
                    Directory.Move(fromMovie, deletedFromMovie);
                }

                log($"Move external movie {fromMovie} to {deletedFromMovie}");
                log(string.Empty);
            });
    }

    internal static void RenameTVAttachment(IEnumerable<string> videos, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        videos.ForEach(video =>
        {
            string match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
            Directory
                .EnumerateFiles(Path.GetDirectoryName(video) ?? throw new InvalidOperationException(video), $"*{match}*", SearchOption.TopDirectoryOnly)
                .Where(file => !file.EqualsIgnoreCase(video))
                .ForEach(log);
        });
    }

    internal static void RenameWithUpdatedRatings(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string rating = Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata)
                    ? imdbMetadata.FormattedAggregateRating
                    : "-";
                string name = Path.GetFileName(movie);
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

    internal static void RenameCollections(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .Select(movie =>
            {
                string name = Path.GetFileName(movie);
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
            .Where(season => Regex.IsMatch(Path.GetFileName(season), "^Season [0-9]{2}"))
            .OrderBy(season => season)
            .ForEach(season =>
            {
                string seasonNumber = Path.GetFileName(season).Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First().ReplaceIgnoreCase("Season ", string.Empty);
                Directory
                    .EnumerateFiles(season, VideoSearchPattern)
                    .OrderBy(video => video)
                    .ToArray()
                    .ForEach((video, index) =>
                    {
                        string prefix = $"{tvTitle}.S{seasonNumber}E{index + 1:00}.";
                        if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata))
                        {
                            throw new InvalidOperationException(video);
                        }

                        prefix = videoMetadata.DefinitionType switch
                        {
                            DefinitionType.P1080 => $"{prefix}1080p.",
                            DefinitionType.P720 => $"{prefix}720p.",
                            _ => prefix
                        };

                        log(video);
                        string newVideo = PathHelper.AddFilePrefix(
                            video
                                .ReplaceIgnoreCase(".1080p", string.Empty).ReplaceIgnoreCase(".720p", string.Empty)
                                .ReplaceOrdinal("    ", " ").ReplaceOrdinal("   ", " ").ReplaceOrdinal("  ", " ")
                                .ReplaceOrdinal(" - ", "-").ReplaceOrdinal("- ", "-").ReplaceOrdinal(" -", "-"),
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

    internal static void MoveTopTVEpisodes(string directory, string subtitleBackupDirectory, bool isDryRun = false, Action<string>? log = null)
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
            .Where(file => Path.GetFileNameWithoutExtension(file).EndsWithOrdinal(" (1)") && File.Exists(PathHelper.ReplaceFileNameWithoutExtension(file, name => name[..^" (1)".Length])))
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
            .Where(season => season.ContainsIgnoreCase($"{VersionSeparator}{TopEnglishKeyword}"))
            .ForEach(season =>
            {
                string subtitleDirectory = Path.Combine(season, "Subs");
                Directory
                    .GetDirectories(subtitleDirectory)
                    .ForEach(episodeSubtitleDirectory => Directory
                        .GetFiles(episodeSubtitleDirectory)
                        .ForEach(subtitle =>
                        {
                            string newSubtitle = Path.Combine(season, $"{Path.GetFileName(episodeSubtitleDirectory)}.{Path.GetFileName(subtitle)}");
                            log($"Move {subtitle}");
                            if (!isDryRun)
                            {
                                FileHelper.Move(subtitle, newSubtitle);
                            }

                            log(newSubtitle);
                            log(string.Empty);
                        }));
            });

        Directory
            .GetDirectories(directory)
            .Where(season => season.ContainsIgnoreCase($"{VersionSeparator}{TopEnglishKeyword}"))
            .ForEach(season =>
            {
                Match match = Regex.Match(Path.GetFileName(season), @"^(.+)\.S([0-9]{2})(\.[A-Z]+)?\.1080p\.+");
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

        Directory
            .EnumerateDirectories(directory)
            .SelectMany(Directory.EnumerateDirectories)
            .Select(season => Path.Combine(season, "Subs"))
            .ToArray()
            .ForEach(seasonSubtitleDirectory =>
            {
                log($"Delete {seasonSubtitleDirectory}");
                if (!isDryRun && Directory.Exists(seasonSubtitleDirectory))
                {
                    DirectoryHelper.Recycle(seasonSubtitleDirectory);
                }
            });

        Directory
            .EnumerateDirectories(directory)
            .SelectMany(Directory.EnumerateDirectories)
            .SelectMany(season => Directory.EnumerateFiles(season, PathHelper.AllSearchPattern))
            .Where(IsSubtitle)
            .GroupBy(subtitle =>
            {
                string name = Path.GetFileNameWithoutExtension(subtitle);
                return name[..name.LastIndexOf(".", StringComparison.Ordinal)];
            })
            .ToArray()
            .ForEach(group =>
            {
                string[] subtitles = group.ToArray();
                string? englishSubtitle = group
                    .Where(subtitle => Path.GetFileNameWithoutExtension(subtitle).ContainsIgnoreCase("_eng"))
                    .MaxBy(subtitle => new FileInfo(subtitle).Length);
                if (!string.IsNullOrWhiteSpace(englishSubtitle))
                {
                    const string Language = "eng";
                    string englishSubtitleName = Path.GetFileNameWithoutExtension(englishSubtitle);
                    string newEnglishSubtitle = Path.Combine(Path.GetDirectoryName(englishSubtitle)!, $"{englishSubtitleName.Substring(0, englishSubtitleName.LastIndexOf(".", StringComparison.Ordinal))}.{Language}{Path.GetExtension(englishSubtitle)}");
                    log($"Move {englishSubtitle}");
                    if (!isDryRun)
                    {
                        FileHelper.Move(englishSubtitle, newEnglishSubtitle);
                    }

                    log(newEnglishSubtitle);
                    log(string.Empty);
                }

                string[] chineseSubtitles = group
                    .Where(subtitle => Path.GetFileNameWithoutExtension(subtitle).ContainsIgnoreCase("_chi"))
                    .ToArray();
                switch (chineseSubtitles.Length)
                {
                    case 1:
                        {
                            string chineseSubtitle = chineseSubtitles.Single();

                            string language = EncodingHelper.TryRead(chineseSubtitle, out string? content, out _) && "為們說無當".Any(content.ContainsOrdinal) ? "cht" : "chs";
                            string chineseSubtitleName = Path.GetFileNameWithoutExtension(chineseSubtitle);
                            string newChineseSubtitle = Path.Combine(Path.GetDirectoryName(chineseSubtitle)!, $"{chineseSubtitleName.Substring(0, chineseSubtitleName.LastIndexOf(".", StringComparison.Ordinal))}.{language}{Path.GetExtension(chineseSubtitle)}");
                            log($"Move {chineseSubtitle}");
                            if (!isDryRun)
                            {
                                FileHelper.Move(chineseSubtitle, newChineseSubtitle);
                            }

                            log(newChineseSubtitle);
                            log(string.Empty);
                            break;
                        }
                    case > 1:
                        {
                            string chineseSubtitle = chineseSubtitles
                                .Where(subtitle => EncodingHelper.TryRead(subtitle, out string? content, out _) && !"為們說無當嗎這".Any(content.ContainsOrdinal))
                                .OrderByDescending(subtitle => new FileInfo(subtitle).Length)
                                .First();
                            chineseSubtitles = new string[] { chineseSubtitle };

                            const string Language = "chs";
                            string chineseSubtitleName = Path.GetFileNameWithoutExtension(chineseSubtitle);
                            string newChineseSubtitle = Path.Combine(Path.GetDirectoryName(chineseSubtitle)!, $"{chineseSubtitleName.Substring(0, chineseSubtitleName.LastIndexOf(".", StringComparison.Ordinal))}.{Language}{Path.GetExtension(chineseSubtitle)}");
                            log($"Move {chineseSubtitle}");
                            if (!isDryRun)
                            {
                                FileHelper.Move(chineseSubtitle, newChineseSubtitle);
                            }

                            log(newChineseSubtitle);
                            log(string.Empty);
                            break;
                        }
                }

                subtitles.Except(string.IsNullOrWhiteSpace(englishSubtitle) ? chineseSubtitles : chineseSubtitles.Append(englishSubtitle))
                    .ForEach(subtitle =>
                    {
                        string newSubtitle = Path.Combine(subtitleBackupDirectory, Path.GetFileName(subtitle));
                        log($"Move {subtitle}");
                        if (!isDryRun)
                        {
                            FileHelper.Move(subtitle, newSubtitle);
                        }

                        log(newSubtitle);
                        log(string.Empty);
                    });
            });

        Directory
            .GetFiles(directory, "*.eng.srt", SearchOption.AllDirectories)
            .ForEach(f => FileHelper.Move(f, f.ReplaceIgnoreCase(".eng.srt", ".srt"), false));
    }

    internal static void MoveFanArt(string directory, int level = 2, bool overwrite = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string fanArtDirectory = Path.Combine(movie, "extrafanart");
                if (!Directory.Exists(fanArtDirectory))
                {
                    return;
                }

                string fanArt = Directory.GetFiles(fanArtDirectory).Single();
                Debug.Assert(Path.GetFileNameWithoutExtension(fanArt).StartsWithIgnoreCase("fanart"));
                Debug.Assert(Path.GetExtension(fanArt).EqualsIgnoreCase(".jpg"));

                string newFanArt = Path.Combine(movie, "fanart.jpg");
                Debug.Assert(overwrite || !File.Exists(newFanArt));

                log(fanArt);
                File.Move(fanArt, newFanArt, overwrite);
                log(newFanArt);

                DirectoryHelper.Recycle(fanArtDirectory);
            });
    }

    internal static void RenameEpisodeWithoutTitle(string directory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
        files
            .Where(IsVideo)
            .ForEach(video =>
            {
                log(video);
                if (VideoEpisodeFileInfo.TryParse(video, out VideoEpisodeFileInfo? episode) && ((IVideoFileInfo)episode).EncoderType == EncoderType.X && episode.EpisodeTitle.IsNotNullOrWhiteSpace())
                {
                    episode = episode with { EpisodeTitle = string.Empty };
                    if (!isDryRun)
                    {
                        FileHelper.ReplaceFileName(video, episode.Name);
                    }

                    log(PathHelper.ReplaceFileName(video, episode.Name));
                    log(string.Empty);

                    string initial = video.Substring(0, video.Length - Path.GetExtension(video).Length);
                    string newInitial = Path.Combine(Path.GetDirectoryName(video)!, Path.GetFileNameWithoutExtension(episode.Name));
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

    internal static void FormatTV(string mediaDirectory, string metadataDirectory, string subtitleDirectory = "", Func<string, string, string>? renameForTitle = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        metadataDirectory = metadataDirectory.IfNullOrWhiteSpace(mediaDirectory);

        (string Path, string Name)[] tvs = Directory
            .GetDirectories(mediaDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(tv => VideoDirectoryInfo.TryParse(tv, out _))
            .Select(tv => (Path: tv, Name: Path.GetFileName(tv) ?? throw new InvalidOperationException(tv)))
            .OrderBy(tv => tv.Name)
            .ToArray();

        string[] existingMetadataTVs = Directory
            .GetDirectories(metadataDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .ToArray();
        tvs
            .Select(tv => (tv.Path, tv.Name, Metadata: existingMetadataTVs.Single(existingTV => tv.Name.Equals(Path.GetFileName(existingTV)))))
            .ForEach(match => RenameEpisodesWithTitle(match.Path, match.Metadata, renameForTitle, isDryRun, log));

        string[] existingSubtitleTVs = subtitleDirectory.IsNullOrWhiteSpace()
            ? existingMetadataTVs
            : Directory
                .GetDirectories(subtitleDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .ToArray();
        tvs
            .Select(tv => (tv.Path, tv.Name, Subtitle: existingSubtitleTVs.Single(existingTV => tv.Name.Equals(Path.GetFileName(existingTV)))))
            .ForEach(match => MoveSubtitlesForEpisodes(match.Path, match.Subtitle, isDryRun: isDryRun, log: log));
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
}
