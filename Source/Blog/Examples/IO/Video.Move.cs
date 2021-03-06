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
    using System.Web;
    using System.Xml.Linq;
    using Examples.Net;

    internal static partial class Video
    {
        internal static void RenameFiles(string path, Func<string, int, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.EnumerateFiles(path, pattern ?? PathHelper.AllSearchPattern, searchOption ?? SearchOption.AllDirectories)
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
            Directory.GetDirectories(path, pattern ?? PathHelper.AllSearchPattern, searchOption ?? SearchOption.AllDirectories)
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
                    Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
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

        internal static void RenameEpisodesWithTitle(string nfoDirectory, string mediaDirectory, Func<string, string, string> rename, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.EnumerateFiles(nfoDirectory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
                .ToList()
                .ForEach(nfo =>
                {

                    string match = Regex.Match(Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException($"{nfo} is invalid."), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(match))
                    {
                        return;
                    }

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
                Directory.GetFiles(directory, PathHelper.AllSearchPattern, searchOption)
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

        internal static void RenameDirectoriesWithAdditionalMetadata(string directory, int level = 2, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
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

                    Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata);
                    string additional = $"{{{string.Join(",", imdbMetadata?.Regions ?? Array.Empty<string>())};{string.Join(",", imdbMetadata?.Genre.Take(3) ?? Array.Empty<string>())}}}";
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

                    string[] files = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).OrderBy(file => file).ToArray();
                    string[] nfos = files.Where(file => file.HasExtension(XmlMetadataExtension)).ToArray();
                    XDocument english;
                    XDocument? chinese;
                    if (nfos.Any(nfo => nfo.EndsWith($".{backupFlag}{XmlMetadataExtension}")) && nfos.Any(nfo => !nfo.EndsWith($".{backupFlag}{XmlMetadataExtension}")))
                    {
                        english = XDocument.Load(nfos.First(nfo => nfo.EndsWith($".{backupFlag}{XmlMetadataExtension}")));
                        chinese = XDocument.Load(nfos.First(nfo => !nfo.EndsWith($".{backupFlag}{XmlMetadataExtension}")));
                    }
                    else
                    {
                        english = XDocument.Load(nfos.First());
                        chinese = null;
                    }

                    string json = files.Single(file => PathHelper.HasExtension(file, JsonMetadataExtension));
                    Imdb.TryLoad(json, out ImdbMetadata? imdbMetadata);

                    string englishTitle = english.Root?.Element("title")?.Value ?? throw new InvalidOperationException($"{movie} has no English title.");
                    string chineseTitle = chinese?.Root?.Element("title")?.Value ?? string.Empty;
                    string? originalTitle = imdbMetadata?.Name ?? english.Root?.Element("originaltitle")?.Value ?? imdbMetadata?.Name;
                    string year = imdbMetadata?.Year ?? english.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{movie} has no year.");
                    string? imdbId = english.Root?.Element("imdbid")?.Value;
                    Debug.Assert(string.IsNullOrWhiteSpace(imdbId)
                        ? string.Equals("-", Path.GetFileNameWithoutExtension(json), StringComparison.InvariantCulture)
                        : string.Equals(imdbId, Path.GetFileNameWithoutExtension(json).Split(".")[0], StringComparison.InvariantCultureIgnoreCase));
                    string rating = string.IsNullOrWhiteSpace(imdbId)
                        ? "-"
                        : imdbMetadata?.AggregateRating?.RatingValue ?? "0.0";
                    string[] videos = files.Where(file => file.HasAnyExtension(AllVideoExtensions)).ToArray();
                    string contentRating = imdbMetadata?.FormattedContentRating ?? "-";
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
                        ? $"{{{string.Join(",", imdbMetadata?.Regions.Take(5) ?? Array.Empty<string>())};{string.Join(",", imdbMetadata?.Genre?.Take(3) ?? Array.Empty<string>())}}}"
                        : string.Empty;
                    string newMovie = $"{englishTitle.FilterForFileSystem()}{originalTitle.FilterForFileSystem()}.{year}.{chineseTitle.FilterForFileSystem()}[{rating}][{contentRating}]{definition}{additional}";
                    string newDirectory = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), newMovie);
                    if (isDryRun)
                    {
                        log(movie);
                        log(newDirectory);
                        log(string.Empty);
                    }
                    else
                    {
                        if (!string.Equals(movie, newDirectory))
                        {
                            log(movie);
                            Directory.Move(movie, newDirectory);
                            log(newDirectory);
                            log(string.Empty);
                        }
                    }
                });
        }

        internal static void RenameDirectoriesWithoutAdditionalMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
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

        internal static void RenameDirectoriesWithImdbMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    if (!VideoDirectoryInfo.TryParse(movie, out VideoDirectoryInfo? parsed))
                    {
                        log($"!Cannot parse {movie}");
                        return;
                    }

                    if (!Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata))
                    {
                        log($"!Missing metadata {movie}");
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(imdbMetadata.Year))
                    {
                        parsed = parsed with { Year = imdbMetadata.Year };
                    }

                    parsed = parsed with { AggregateRating = imdbMetadata.FormattedAggregateRating, ContentRating = imdbMetadata.FormattedContentRating };
                    string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), parsed.ToString());
                    if (string.Equals(movie, newMovie, StringComparison.Ordinal))
                    {
                        return;
                    }

                    log(movie);
                    if (!isDryRun)
                    {
                        Directory.Move(movie, newMovie);
                    }
                    log(newMovie);
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
            log ??= TraceLog;
            Dictionary<string, VideoMetadata> externalMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(fromJsonPath)) ?? throw new InvalidOperationException(fromJsonPath);
            Dictionary<string, Dictionary<string, VideoMetadata>> moviesMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(toJsonPath)) ?? throw new InvalidOperationException(toJsonPath);

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
                    if (new VideoFileInfo(Path.GetFileNameWithoutExtension(toVideoMetadata.File)).IsTop)
                    {
                        log($"Video {toVideoMetadata.File} is x265.");
                        return;
                    }

                    if (!File.Exists(toVideoMetadata.File))
                    {
                        log($"Video {toVideoMetadata.File} does not exist.");
                        return;
                    }

                    if (VideoFileInfo.IsTopOrPremium(Path.GetFileNameWithoutExtension(toVideoMetadata.File)) &&
                        !VideoFileInfo.IsTopOrPremium(Path.GetFileNameWithoutExtension(fromVideoMetadata.File)))
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

                    Directory.GetFiles(toMovie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
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
                    log(string.Empty);
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
                    VideoDirectoryInfo videoDirectoryInfo = new(name);
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

        internal static void RenameEpisodes(string directory, string tvTitle)
        {
            Directory
                .EnumerateDirectories(directory)
                .Where(season => Regex.IsMatch(Path.GetFileName(season), "Season [0-9]{2}"))
                .OrderBy(season => season)
                .ForEach(season =>
                {
                    string seasonNumber = Path.GetFileName(season).Replace("Season ", string.Empty);
                    Directory
                        .EnumerateFiles(season, VideoSearchPattern)
                        .OrderBy(video => video)
                        .ToArray()
                        .ForEach((video, index) =>
                        {
                            string prefix = $"{tvTitle}.S{seasonNumber}E{index + 1:00}.";
                            if (video.Contains(".1080p."))
                            {
                                prefix = $"{prefix}1080p.";
                            }

                            if (video.Contains(".720p."))
                            {
                                prefix = $"{prefix}720p.";
                            }

                            File.Move(video, PathHelper.AddFilePrefix(video.Replace(".1080p", string.Empty).Replace(".720p", string.Empty), prefix));
                        });
                });
        }
    }
}