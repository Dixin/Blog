namespace Examples.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Examples.Net;

    internal static partial class Video
    {
        internal static void RenameFiles(string path, Func<string, int, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.EnumerateFiles(path, pattern ?? "*", searchOption ?? SearchOption.AllDirectories)
                .Where(file => predicate?.Invoke(file) ?? true)
                .OrderBy(file => file)
                .ToArray()
                .ForEach((file, index) =>
                {
                    string newFile = rename(file, index);
                    if (!string.Equals(file, newFile))
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

        internal static void RenameDirectories(string path, Func<string, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetDirectories(path, pattern ?? "*", searchOption ?? SearchOption.AllDirectories)
                .Where(directory => predicate?.Invoke(directory) ?? true)
                .ForEach(directory =>
                {
                    string newDirectory = rename(directory);
                    if (!string.Equals(directory, newDirectory))
                    {
                        log(directory);
                        Directory.Move(directory, newDirectory);
                        log(newDirectory);
                    }
                });
        }

        internal static void RenameVideosWithMultipleAudio(IEnumerable<string> files, Action<string> log)
        {
            files.ForEach(file =>
                {
                    if (!File.Exists(file))
                    {
                        log($"Not exist {file}");
                        return;
                    }

                    int audio = GetAudioMetadata(file, log);
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
                    if (last.IndexOf("handbrake", StringComparison.OrdinalIgnoreCase) >= 0)
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
                    Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly)
                        .Where(attachment => attachment != file)
                        .Where(attachment => (Path.GetFileName(attachment) ?? throw new InvalidOperationException(file)).StartsWith(Path.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase))
                        .ToList()
                        .ForEach(attachment =>
                            {
                                string newAttachment = Path.Combine(directory, (Path.GetFileName(attachment) ?? throw new InvalidOperationException(file)).Replace(Path.GetFileNameWithoutExtension(file), string.Join(".", info.SkipLast(1))));
                                log(newAttachment);
                                File.Move(attachment, newAttachment);
                            });
                });
        }

        internal static void RenameEpisodesWithTitle(string nfoDirectory, string mediaDirectory, string searchPattern, Func<string, string, string> rename, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.EnumerateFiles(nfoDirectory, MetadataSearchPattern, SearchOption.AllDirectories)
                .ToList()
                .ForEach(nfo =>
                {

                    string match = Regex.Match(Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException($"{nfo} is invalid."), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    string title = XDocument.Load(nfo).Root?.Element("title")?.Value.FilterForFileSystem() ?? throw new InvalidOperationException($"{nfo} has no title.");
                    Directory
                        .EnumerateFiles(mediaDirectory, $"*{match}*", SearchOption.AllDirectories)
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

        internal static void RenameVideosWithDefinition(string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool isDryRun = false, Action<string>? log = null)
        {
            RenameVideosWithDefinition(
                Directory.GetFiles(directory, "*", searchOption)
                    .Where(file => AllVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)))
                    .ToArray(),
                isDryRun,
                log);
        }

        internal static void RenameVideosWithDefinition(string[] files, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
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

        internal static void RenameDirectoriesWithAppendingMetadata(string directory, int level = 2, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    string movieName = Path.GetFileName(movie);
                    if (!overwrite && movieName.Contains("{"))
                    {
                        return;
                    }

                    string json = Directory.GetFiles(movie, "*.json", SearchOption.TopDirectoryOnly).Single();
                    Imdb.TryLoad(json, out ImdbMetadata? imdbMetadata);
                    string additional = $"{{{Path.GetFileNameWithoutExtension(json).Split('.')[2]};{string.Join(",", imdbMetadata?.Genre.Take(3) ?? Array.Empty<string>())};{imdbMetadata?.ContentRating}}}";
                    string originalMovie = movieName.Contains("{")
                        ? PathHelper.ReplaceFileName(movie, movieName.Substring(0, movieName.IndexOf("{", StringComparison.InvariantCulture)))
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

        internal static void RenameDirectoriesWithMetadata(string directory, int level = 2, bool additionalInfo = false, bool overwrite = false, bool isDryRun = false, string backupFlag = "backup", Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    if (!overwrite && Path.GetFileName(movie).Contains("{"))
                    {
                        return;
                    }

                    string[] nfos = Directory.GetFiles(movie, MetadataSearchPattern, SearchOption.TopDirectoryOnly).OrderBy(nfo => nfo).ToArray();
                    XDocument english;
                    XDocument? chinese;
                    if (nfos.Any(nfo => nfo.EndsWith($".{backupFlag}{MetadataExtension}")) && nfos.Any(nfo => !nfo.EndsWith($".{backupFlag}{MetadataExtension}")))
                    {
                        english = XDocument.Load(nfos.First(nfo => nfo.EndsWith($".{backupFlag}{MetadataExtension}")));
                        chinese = XDocument.Load(nfos.First(nfo => !nfo.EndsWith($".{backupFlag}{MetadataExtension}")));
                    }
                    else
                    {
                        english = XDocument.Load(nfos.First());
                        chinese = null;
                    }

                    string json = Directory.GetFiles(movie, "*.json", SearchOption.TopDirectoryOnly).Single();
                    Imdb.TryLoad(json, out ImdbMetadata? imdbMetadata);

                    string englishTitle = english.Root?.Element("title")?.Value ?? throw new InvalidOperationException($"{movie} has no English title.");
                    string chineseTitle = chinese?.Root?.Element("title")?.Value ?? string.Empty;
                    string? originalTitle = imdbMetadata?.Name ?? english.Root?.Element("originaltitle")?.Value ?? imdbMetadata?.Name;
                    string year = imdbMetadata?.Year ?? english.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{movie} has no year.");
                    string? imdb = english.Root?.Element("imdbid")?.Value;
                    Debug.Assert(string.IsNullOrWhiteSpace(imdb)
                        ? string.Equals("-", Path.GetFileNameWithoutExtension(json), StringComparison.InvariantCulture)
                        : string.Equals(imdb, Path.GetFileNameWithoutExtension(json).Split(".")[0], StringComparison.InvariantCultureIgnoreCase));
                    string rating = string.IsNullOrWhiteSpace(imdb)
                        ? "-"
                        : float.TryParse(imdbMetadata?.AggregateRating?.RatingValue, out float ratingFloat) ? ratingFloat.ToString("0.0") : "0.0";
                    string[] videos = Directory.GetFiles(movie, VideoSearchPattern, SearchOption.TopDirectoryOnly).Concat(Directory.GetFiles(movie, "*.avi", SearchOption.TopDirectoryOnly)).ToArray();
                    string definition = videos switch
                    {
                        _ when videos.Any(video => Path.GetFileNameWithoutExtension(video)?.Contains("1080p") ?? false) => "[1080p]",
                        _ when videos.Any(video => Path.GetFileNameWithoutExtension(video)?.Contains("720p") ?? false) => "[720p]",
                        _ => string.Empty
                    };
                    originalTitle = string.Equals(originalTitle, englishTitle, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(originalTitle)
                        ? string.Empty
                        : $"={originalTitle}";
                    string additional = additionalInfo
                        ? $"{{{string.Join(",", imdbMetadata?.Regions.Take(5) ?? Array.Empty<string>())};{string.Join(",", imdbMetadata?.Genre.Take(3) ?? Array.Empty<string>())};{imdbMetadata?.ContentRating}}}"
                        : string.Empty;
                    string newMovie = $"{englishTitle.FilterForFileSystem()}{originalTitle.FilterForFileSystem()}.{year}.{chineseTitle.FilterForFileSystem()}[{rating}]{definition}{additional}";
                    string newDirectory = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), newMovie);
                    if (isDryRun)
                    {
                        log(newDirectory);
                    }
                    else
                    {
                        if (!string.Equals(movie, newDirectory))
                        {
                            log(movie);
                            Directory.Move(movie, newDirectory);
                            log(newDirectory);
                        }
                    }
                });
        }

        internal static void RenameDirectoriesWithoutMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    string movieName = Path.GetFileName(movie) ?? throw new InvalidOperationException(movie);
                    bool isRenamed = false;
                    if (movieName.StartsWith("0."))
                    {
                        movieName = movieName.Substring("0.".Length);
                        isRenamed = true;
                    }

                    if (movieName.Contains("{"))
                    {
                        movieName = movieName.Substring(0, movieName.IndexOf("{", StringComparison.Ordinal));
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

        internal static void RenameMovies(string destination, string directory, int level = 2, string field = "genre", string? value = null, bool isDryRun = false)
        {
            RenameMovies(
                (movie, metadata) => Path.Combine(destination, Path.GetFileName(movie)),
                directory,
                level,
                (movie, metadata) => string.Equals(value, metadata.Root?.Element(field)?.Value, StringComparison.OrdinalIgnoreCase),
                isDryRun);
        }

        internal static void RenameMovies(Func<string, XDocument, string> rename, string directory, int level = 2, Func<string, XDocument, bool>? predicate = null, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .Select(movie => (Directory: movie, Metadata: XDocument.Load(Directory.GetFiles(movie, MetadataSearchPattern, SearchOption.TopDirectoryOnly).First())))
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
            log ??= TraceLog;
            Dictionary<string, VideoMetadata> externalMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(fromJsonPath));
            Dictionary<string, Dictionary<string, VideoMetadata>> moviesMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(toJsonPath));

            externalMetadata
                .Where(externalVideo => File.Exists(externalVideo.Value.File))
                .ForEach(externalVideo =>
                {
                    VideoMetadata fromVideoMetadata = externalVideo.Value;
                    string fromMovie = Path.GetDirectoryName(fromVideoMetadata.File) ?? throw new InvalidOperationException(fromVideoMetadata.File);
                    log($"Starting {fromMovie}");

                    if (!moviesMetadata.TryGetValue(externalVideo.Key, out Dictionary<string, VideoMetadata>? group) || !group.Any())
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
                    toVideoMetadata.File = Path.Combine(Path.GetDirectoryName(toJsonPath) ?? throw new ArgumentException(toJsonPath), toVideoMetadata.File);
                    if (TopVersion.IsMatch(Path.GetFileNameWithoutExtension(toVideoMetadata.File)))
                    {
                        log($"Video {toVideoMetadata.File} is x265.");
                        return;
                    }

                    if (!File.Exists(toVideoMetadata.File))
                    {
                        log($"Video {toVideoMetadata.File} does not exist.");
                        return;
                    }

                    if (Regex.IsMatch(Path.GetFileNameWithoutExtension(toVideoMetadata.File), "(RARBG|VXT)", RegexOptions.IgnoreCase) &&
                        !Regex.IsMatch(Path.GetFileNameWithoutExtension(fromVideoMetadata.File), "(RARBG|VXT)", RegexOptions.IgnoreCase))
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

                    Directory.GetFiles(fromMovie, AllSearchPattern, SearchOption.TopDirectoryOnly)
                        .Where(file => moveAllAttachment || Path.GetFileNameWithoutExtension(file).Contains(Path.GetFileNameWithoutExtension(fromVideoMetadata.File), StringComparison.InvariantCultureIgnoreCase))
                        .ToArray()
                        .ForEach(fromAttachment =>
                        {
                            string toAttachment = Path.Combine(toMovie, Path.GetFileName(fromAttachment));
                            if (File.Exists(toAttachment))
                            {
                                if (!isDryRun)
                                {
                                    new FileInfo(toAttachment).IsReadOnly = false;
                                    File.Delete(toAttachment);
                                }

                                log($"Delete attachment {toAttachment}");
                            }
                            if (!isDryRun)
                            {
                                File.Move(fromAttachment, toAttachment);
                            }

                            log($"Move external attachment {fromAttachment} to {toAttachment}");
                        });

                    if (!isDryRun)
                    {
                        new FileInfo(toVideoMetadata.File).IsReadOnly = false;
                        File.Delete(toVideoMetadata.File);
                    }

                    log($"Delete video {toVideoMetadata.File}");

                    Directory.GetFiles(toMovie, AllSearchPattern, SearchOption.TopDirectoryOnly)
                        .Where(file => Path.GetFileNameWithoutExtension(file).Contains(Path.GetFileNameWithoutExtension(toVideoMetadata.File), StringComparison.InvariantCultureIgnoreCase))
                        .ToArray()
                        .ForEach(existingAttachment =>
                        {
                            string newExistingAttachment = Path.Combine(toMovie, Path.GetFileName(existingAttachment).Replace(Path.GetFileNameWithoutExtension(toVideoMetadata.File), Path.GetFileNameWithoutExtension(externalVideo.Value.File)));
                            if (File.Exists(newExistingAttachment))
                            {
                                if (!isDryRun)
                                {
                                    new FileInfo(existingAttachment).IsReadOnly = false;
                                    File.Delete(existingAttachment);
                                }

                                log($"Delete attachment {existingAttachment}");
                            }
                            else
                            {
                                if (!isDryRun)
                                {
                                    File.Move(existingAttachment, newExistingAttachment);
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
                });
        }

        internal static void RenameTVAttachment(IEnumerable<string> videos)
        {
            videos.ForEach(video =>
            {
                string match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                Directory
                    .EnumerateFiles(Path.GetDirectoryName(video) ?? throw new InvalidOperationException(video), $"*{match}*", SearchOption.TopDirectoryOnly)
                    .Where(file => !string.Equals(file, video, StringComparison.InvariantCultureIgnoreCase))
                    .ForEach(file => Trace.WriteLine(file));
            });
        }

        internal static void RenameWithUpdatedRatings(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    string rating = Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata)
                        ? imdbMetadata.FormattedAggregateRating
                        : "-";
                    string name = Path.GetFileName(movie);
                    string newName = Regex.Replace(name, @"\[([0-9]\.[0-9]|\-)\]", $"[{rating}]");
                    if (!string.Equals(name, newName, StringComparison.InvariantCulture))
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
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .Select(movie =>
                {
                    string name = Path.GetFileName(movie);
                    VideoDirectoryInfo videoDirectoryInfo = new VideoDirectoryInfo(name);
                    string defaultNumber1 = Regex.Match(videoDirectoryInfo.DefaultTitle1, " ([0-9]{1,2})$").Value.TrimStart(' ');
                    string defaultNumber2 = Regex.Match(videoDirectoryInfo.DefaultTitle2, " ([0-9]{1,2})$").Value.TrimStart(' ');
                    string originalNumber1 = Regex.Match(videoDirectoryInfo.OriginalTitle1, " ([0-9]{1,2})$").Value.TrimStart(' ');
                    string originalNumber2 = Regex.Match(videoDirectoryInfo.OriginalTitle2, " ([0-9]{1,2})$").Value.TrimStart(' ');
                    string translatedNumber1 = Regex.Match(videoDirectoryInfo.TranslatedTitle1, "([0-9]{1,2})$").Value;
                    string translatedNumber2 = Regex.Match(videoDirectoryInfo.TranslatedTitle2, "([0-9]{1,2})$").Value;
                    if (string.IsNullOrWhiteSpace(defaultNumber1) && string.IsNullOrWhiteSpace(defaultNumber2))
                    {
                        return $"-{movie}";
                    }

                    if (!string.Equals(defaultNumber1, translatedNumber1, StringComparison.InvariantCulture))
                    {
                        return $"#Translated number is inconsistent: {movie}";
                    }

                    if (!string.IsNullOrWhiteSpace(originalNumber1) && !string.Equals(defaultNumber1, originalNumber1, StringComparison.InvariantCulture))
                    {
                        return $"!Original number is inconsistent: {movie}";
                    }

                    if (!string.IsNullOrWhiteSpace(defaultNumber2) && !string.Equals(defaultNumber2, translatedNumber2))
                    {
                        return $"!Translated secondary number is inconsistent:  {movie}";
                    }

                    if (!string.IsNullOrWhiteSpace(originalNumber2) && !string.Equals(defaultNumber2, originalNumber2, StringComparison.InvariantCulture))
                    {
                        return $"!Original secondary number is inconsistent:  {movie}";
                    }

                    videoDirectoryInfo.DefaultTitle1 = Regex.Replace(videoDirectoryInfo.DefaultTitle1, " ([0-9]{1,2})$", "`$1");
                    videoDirectoryInfo.DefaultTitle2 = Regex.Replace(videoDirectoryInfo.DefaultTitle2, " ([0-9]{1,2})$", "`$1");
                    videoDirectoryInfo.OriginalTitle1 = Regex.Replace(videoDirectoryInfo.OriginalTitle1, " ([0-9]{1,2})$", "`$1");
                    videoDirectoryInfo.OriginalTitle2 = Regex.Replace(videoDirectoryInfo.OriginalTitle2, " ([0-9]{1,2})$", "`$1");
                    videoDirectoryInfo.TranslatedTitle1 = Regex.Replace(videoDirectoryInfo.TranslatedTitle1, "([0-9]{1,2})$", "`$1");
                    videoDirectoryInfo.TranslatedTitle2 = Regex.Replace(videoDirectoryInfo.TranslatedTitle2, "([0-9]{1,2})$", "`$1");
                    string newMovie = PathHelper.ReplaceFileName(movie, videoDirectoryInfo.Name);
                    Debug.Assert(newMovie.Length - movie.Length == 1 || newMovie.Length - movie.Length == 2);
                    Directory.Move(movie, newMovie);
                    return newMovie;
                })
                .OrderBy(message => message, StringComparer.Ordinal)
                .ForEach(log);
        }
    }
}