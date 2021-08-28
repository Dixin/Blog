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
    using Examples.Common;
    using Examples.Linq;
    using Examples.Net;

    internal static partial class Video
    {
        internal static void PrintVideosWithErrors(string directory, bool isNoAudioAllowed = false, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, Action<string>? is720 = null, Action<string>? is1080 = null, Action<string>? log = null)
        {
            PrintVideosWithErrors(
                Directory
                    .EnumerateFiles(directory, PathHelper.AllSearchPattern, searchOption)
                    .Where(file => predicate?.Invoke(file) ?? AllVideoExtensions
                        .Where(extension => !string.Equals(extension, ".iso", StringComparison.InvariantCultureIgnoreCase))
                        .Any(extension => file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))),
                isNoAudioAllowed,
                is720,
                is1080,
                log);
        }

        internal static void PrintVideosWithErrors(IEnumerable<string> files, bool isNoAudioAllowed = false, Action<string>? is720 = null, Action<string>? is1080 = null, Action<string>? log = null)
        {
            log ??= TraceLog;
            files
                .ToArray()
                .AsParallel()
                .Select((video, index) =>
                {
                    if (!TryGetVideoMetadata(video, out VideoMetadata? videoMetadata, log: message => log($"{index} {message}")))
                    {
                        log($"!Failed {video}");
                    }

                    return videoMetadata;
                })
                .NotNull()
                .Select(videoMetadata => (Video: videoMetadata, Error: GetVideoError(videoMetadata, isNoAudioAllowed, is720, is1080)))
                .Where(result => !string.IsNullOrWhiteSpace(result.Error.Message))
                .AsSequential()
                .OrderBy(result => result.Video.File)
                .ForEach(result =>
                {
                    log(result.Error.Message ?? string.Empty);
                    result.Error.Action?.Invoke(result.Video.File);
                });
        }

        internal static void PrintDirectoriesWithLowDefinition(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    List<string> files = Directory
                        .GetFiles(movie)
                        .Where(IsCommonVideo)
                        .ToList();
                    if (files.All(file => !file.Contains("1080p")))
                    {
                        log(movie);
                    }
                });
        }

        internal static void PrintDirectoriesWithMultipleMedia(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    string[] videos = Directory
                        .GetFiles(movie)
                        .Where(IsCommonVideo)
                        .OrderBy(video => video)
                        .ToArray();
                    if (videos.Length <= 1)
                    {
                        return;
                    }

                    if (!videos.Select((video, index) => Path.GetFileNameWithoutExtension(video).EndsWith($".cd{index + 1}")).All(isPart => isPart))
                    {
                        log(movie);
                    }
                });
        }

        internal static void PrintVideosNonPreferred(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file =>
            {
                VideoFileInfo videoInfo = new(file);
                return !videoInfo.IsTop && !videoInfo.IsPremium && !videoInfo.IsPreferred;
            });
        }

        internal static void PrintVideosPreferred(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file => new VideoFileInfo(file).IsPreferred);
        }

        internal static void PrintVideosNonTop(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file => !new VideoFileInfo(file).IsTop);
        }

        internal static void PrintVideosPremium(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file => new VideoFileInfo(file).IsPremium);
        }

        private static void PrintVideos(string directory, int level, Func<string, bool> predicate, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .OrderBy(movie => movie)
                .ForEach(movie =>
                {
                    string[] files = Directory
                        .GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
                    string[] videos = files
                        .Where(file => file.IsCommonVideo() && predicate(file))
                        .ToArray();
                    if (videos.Any())
                    {
                        log(Path.GetFileNameWithoutExtension(files.Single(file => file.HasExtension(JsonMetadataExtension))).Split(".")[0]);
                        videos.ForEach(log);
                        log(string.Empty);
                    }
                });
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .Where(movie => Directory.GetFiles(movie).All(video => AllSubtitleExtensions.All(extension => !video.EndsWith(extension, StringComparison.OrdinalIgnoreCase))))
                .ForEach(log);
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string>? log = null, params string[] languages)
        {
            log ??= TraceLog;
            string[] searchPatterns = languages.SelectMany(language => AllSubtitleExtensions.Select(extension => $"*{language}*{extension}")).ToArray();
            EnumerateDirectories(directory, level)
                .Where(movie => searchPatterns.All(searchPattern => !Directory.EnumerateFiles(movie, searchPattern, SearchOption.TopDirectoryOnly).Any()))
                .ForEach(log);
        }

        internal static void PrintMetadataByGroup(string directory, int level = 2, string field = "genre", Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .Select(movie => (movie, metadata: XDocument.Load(Directory.GetFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly).First())))
                .Select(movie => (movie.movie, field: movie.metadata.Root?.Element(field)?.Value))
                .OrderBy(movie => movie.field)
                .ForEach(movie => log($"{movie.field}: {Path.GetFileName(movie.movie)}"));
        }

        internal static void PrintMetadataByDuplication(string directory, string field = "imdbid", Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
                .Select(metadata => (metadata, field: XDocument.Load(metadata).Root?.Element(field)?.Value))
                .GroupBy(movie => movie.field)
                .Where(group => group.Count() > 1)
                .ForEach(group => group.ForEach(movie => log($"{movie.field} - {movie.metadata}")));
        }

        internal static void PrintYears(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    XDocument metadata = XDocument.Load(Directory.GetFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly).First());
                    string movieDirectory = Path.GetFileName(movie);
                    if (movieDirectory.StartsWith("0."))
                    {
                        movieDirectory = movieDirectory.Substring("0.".Length);
                    }

                    VideoDirectoryInfo videoDirectoryInfo = new(movieDirectory);
                    string directoryYear = videoDirectoryInfo.Year;
                    string metadataYear = metadata.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{metadata} has no year.");
                    string videoName = string.Empty;
                    if (!(directoryYear == metadataYear
                        && Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                            .Where(file => AllVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)))
                            .All(video => (videoName = Path.GetFileName(video) ?? throw new InvalidOperationException($"{video} is invalid.")).Contains(directoryYear))))
                    {
                        log($"Directory: {directoryYear}, Metadata {metadataYear}, Video: {videoName}, {movie}");
                    }
                });
        }

        internal static void PrintDirectoriesWithNonLatinOriginalTitle(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .Where(movie => movie.Contains("="))
                .Where(movie => !Regex.IsMatch(movie.Split("=")[1], "^[a-z]{1}.", RegexOptions.IgnoreCase))
                .ForEach(log);
        }

        internal static void PrintDuplicateImdbId(Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
            directories.SelectMany(directory => Directory.EnumerateFiles(directory, JsonMetadataSearchPattern, SearchOption.AllDirectories))
                .GroupBy(metadata => Path.GetFileNameWithoutExtension(metadata).Split(".")[0])
                .Where(group => group.Count() > 1)
                .ForEach(group =>
                {
                    group.OrderBy(metadata => metadata).ForEach(log);
                    log(string.Empty);
                });
        }

        internal static void PrintDirectoriesWithErrors(string directory, int level = 2, bool isLoadingVideo = false, bool isNoAudioAllowed = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            List<string>? allVideos = null;
            if (isLoadingVideo)
            {
                allVideos = new();
            }

            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    string trimmedMovie = Path.GetFileName(movie) ?? throw new InvalidOperationException(movie);
                    if (trimmedMovie.StartsWith("0."))
                    {
                        trimmedMovie = trimmedMovie.Substring("0.".Length);
                    }

                    if (trimmedMovie.Contains("{"))
                    {
                        trimmedMovie = trimmedMovie.Substring(0, trimmedMovie.IndexOf("{", StringComparison.Ordinal));
                    }

                    if (Regex.IsMatch(trimmedMovie, @"·[0-9]"))
                    {
                        log($"!Special character ·: {trimmedMovie}");
                    }

                    if (trimmedMovie.Contains("："))
                    {
                        log($"!Special character ：: {trimmedMovie}");
                    }

                    if (!VideoDirectoryInfo.TryParse(trimmedMovie, out VideoDirectoryInfo? videoDirectoryInfo))
                    {
                        log($"!Directory: {trimmedMovie}");
                        return;
                    }
                    //if (!string.Equals(level1Number1, level1Number3) || !string.IsNullOrWhiteSpace(level1Number2) && !string.Equals(level1Number1, level1Number2))
                    //{
                    //    log($"{movie}");
                    //}

                    //if (Regex.IsMatch(Regex.Replace(match.Groups[5].Value, "([0-9]{1,2})$", ""), "[0-9]+"))
                    //{
                    //    log($"!Index: {movie}");
                    //}

                    new string[] { videoDirectoryInfo.TranslatedTitle1, videoDirectoryInfo.TranslatedTitle2, videoDirectoryInfo.TranslatedTitle3 }
                        .Where(translated => Regex.IsMatch(translated.Split("`").First(), "[0-9]+"))
                        .ForEach(translated => log($"!Translation has number {translated}: {movie}"));

                    string[] files = Directory.EnumerateFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).Select(file => Path.GetFileName(file) ?? throw new InvalidOperationException(file)).ToArray();

                    if (trimmedMovie.Contains("2160p") && !files.Any(file => file.Contains("2160p")))
                    {
                        log($"!Not 2160p: {movie}");
                    }

                    if (!trimmedMovie.Contains("2160p") && files.Any(file => file.Contains("2160p")))
                    {
                        log($"!2160p: {movie}");
                    }

                    if (trimmedMovie.Contains("1080p") && !files.Any(file => file.Contains("1080p")))
                    {
                        log($"!Not 1080p: {movie}");
                    }

                    if (!trimmedMovie.Contains("1080p") && files.Any(file => file.Contains("1080p")))
                    {
                        log($"!1080p: {movie}");
                    }

                    if (trimmedMovie.Contains("720p") && !files.Any(file => file.Contains("720p")))
                    {
                        log($"!Not 720p: {movie}");
                    }

                    if (!trimmedMovie.Contains("1080p") && !trimmedMovie.Contains("720p") && files.Any(file => file.Contains("720p")))
                    {
                        log($"!720p: {movie}");
                    }

                    string[] videos = files.Where(IsCommonVideo).ToArray();
                    string[] subtitles = files.Where(file => file.HasAnyExtension(AllSubtitleExtensions)).ToArray();
                    string[] metadataFiles = files.Where(file => file.HasExtension(XmlMetadataExtension)).ToArray();
                    string[] imdbFiles = files.Where(file => file.HasExtension(JsonMetadataExtension)).ToArray();
                    string[] cacheFiles = files.Where(file => file.HasExtension(ImdbCacheExtension)).ToArray();
                    string[] otherFiles = files.Except(videos).Except(subtitles).Except(metadataFiles).Except(imdbFiles).Except(cacheFiles).ToArray();
                    if (videos.Length < 1)
                    {
                        log($"!No video: {movie}");
                    }
                    else if (videos.Length == 1 || videos.All(video => Regex.IsMatch(Path.GetFileNameWithoutExtension(video), @"\.cd[1-9]$")))
                    {
                        string[] allowedAttachments = Attachments.Concat(AdaptiveAttachments).ToArray();
                        otherFiles
                            .Where(file => !allowedAttachments.Contains(file, StringComparer.InvariantCultureIgnoreCase))
                            .ForEach(file => log($"!Attachment: {Path.Combine(movie, file)}"));
                    }
                    else
                    {
                        string[] allowedAttachments = videos
                            .SelectMany(video => AdaptiveAttachments.Select(attachment => $"{Path.GetFileNameWithoutExtension(video)}-{attachment}"))
                            .Concat(Attachments)
                            .ToArray();
                        otherFiles
                            .Where(file => !allowedAttachments.Contains(file, StringComparer.InvariantCultureIgnoreCase))
                            .ForEach(file => log($"!Attachment: {Path.Combine(movie, file)}"));
                    }

                    string[] allowedSubtitles = videos
                        .SelectMany(video => SubtitleLanguages
                            .Select(language => $"{Path.GetFileNameWithoutExtension(video)}.{language}")
                            .Prepend(Path.GetFileNameWithoutExtension(video)))
                        .SelectMany(subtitle => AllSubtitleExtensions.Select(extension => $"{subtitle}{extension}"))
                        .ToArray();
                    subtitles
                        .Where(subtitle => !allowedSubtitles.Contains(subtitle, StringComparer.InvariantCultureIgnoreCase))
                        .ForEach(file => log($"!Subtitle: {Path.Combine(movie, file)}"));

                    string[] allowedMetadataFiles = videos
                        .Select(video => $"{Path.GetFileNameWithoutExtension(video)}{XmlMetadataExtension}")
                        .ToArray();
                    metadataFiles
                        .Where(metadata => !allowedMetadataFiles.Contains(metadata, StringComparer.InvariantCultureIgnoreCase))
                        .ForEach(file => log($"!Metadata: {Path.Combine(movie, file)}"));

                    files
                        .Except(imdbFiles)
                        .Except(cacheFiles)
                        .Where(file => Regex.IsMatch(file, "(1080[^p]|720[^p])"))
                        .ForEach(file => log($"Definition: {Path.Combine(movie, file)}"));

                    if (isLoadingVideo)
                    {
                        allVideos!.AddRange(videos.Select(video => Path.Combine(movie, video)));
                    }

                    if (metadataFiles.Length < 1)
                    {
                        log($"!Metadata: {movie}");
                    }

                    if (imdbFiles.Length != 1)
                    {
                        log($"!Imdb files {imdbFiles.Length}: {movie}");
                    }

                    string imdbRating = Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata)
                        ? imdbMetadata.FormattedAggregateRating
                        : NotExistingFlag;
                    if (!string.Equals(videoDirectoryInfo.AggregateRating, imdbRating, StringComparison.InvariantCulture))
                    {
                        log($"!Imdb rating {videoDirectoryInfo.AggregateRating} should be {imdbRating}: {movie}");
                    }

                    string contentRating = imdbMetadata?.FormattedContentRating ?? NotExistingFlag;
                    if (!string.Equals(contentRating, videoDirectoryInfo.ContentRating, StringComparison.InvariantCulture))
                    {
                        log($"!Content rating {videoDirectoryInfo.ContentRating} should be {contentRating}: {movie}");
                    }

                    metadataFiles.ForEach(metadataFile =>
                    {
                        metadataFile = Path.Combine(movie, metadataFile);
                        XDocument metadata = XDocument.Load(Path.Combine(movie, metadataFile));
                        string? metadataImdbId = metadata.Root?.Element("imdbid")?.Value;
                        // string? metadataImdbRating = metadata.Root?.Element("rating")?.Value;
                        if (imdbMetadata == null)
                        {
                            if (!string.IsNullOrWhiteSpace(metadataImdbId))
                            {
                                log($"!Metadata should have no imdb id: {metadataFile}");
                            }

                            // if (!string.IsNullOrWhiteSpace(metadataImdbRating))
                            // {
                            //    log($"!Metadata should have no rating: {metadataFile}");
                            // }
                        }
                        else
                        {
                            if (!string.Equals(imdbMetadata.ImdbId, metadataImdbId))
                            {
                                log($"!Metadata imdb id {metadataImdbId} should be {imdbMetadata.ImdbId}: {metadataFile}");
                            }
                        }
                    });

                    string? imdbYear = imdbMetadata?.Year;
                    if (!string.IsNullOrWhiteSpace(imdbYear))
                    {
                        if (!string.Equals(videoDirectoryInfo.Year, imdbYear))
                        {
                            log($"!Year should be {imdbYear}: {movie}");
                        }
                    }

                    string[] directories = Directory.GetDirectories(movie);
                    if (directories.Length > 1)
                    {
                        log($"!Directory {directories.Length}: {movie}");
                    }

                    if (directories.Length == 1 && !string.Equals(Featurettes, Path.GetFileName(directories.Single())))
                    {
                        log($"!Directory: {directories.Single()}");
                    }
                });

            if (isLoadingVideo)
            {
                PrintVideosWithErrors(allVideos!, isNoAudioAllowed, log);
            }
        }

        internal static void PrintTitlesWithDifferences(string directory, int level = 2, Action<string?>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    string[] movieName = Path.GetFileName(movie).Split(".");
                    string movieTitle = movieName[0].Split("=")[0];
                    string movieYear = movieName[1];
                    string? videoYear = null;
                    string? videoTitle = null;
                    if (Imdb.TryLoad(movie, out ImdbMetadata? jsonMetadata))
                    {
                        videoYear = jsonMetadata.Year;
                        videoTitle = jsonMetadata.Name;
                        Debug.Assert(videoYear.Length == 4);
                    }

                    if (string.IsNullOrWhiteSpace(videoYear))
                    {
                        (videoTitle, videoYear) = Directory
                            .GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly)
                            .Select(metadata =>
                                {
                                    XElement root = XDocument.Load(metadata).Root ?? throw new InvalidOperationException(metadata);
                                    return (Title: root.Element("title"!)?.Value, Year: root.Element("year"!)?.Value);
                                })
                            .Distinct()
                            .Single();
                    }

                    if (!string.Equals(movieYear, videoYear))
                    {
                        log(movie);
                        movieName[1] = videoYear!;
                        string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), string.Join(".", movieName));
                        log(newMovie);
                        // Directory.Move(movie, newMovie);
                        string backMovie = movie.Replace(@"E:\", @"F:\");
                        if (Directory.Exists(backMovie))
                        {
                            log(backMovie);
                            string backupNewMovie = newMovie.Replace(@"E:\", @"F:\");
                            log(backupNewMovie);
                            // Directory.Move(backMovie, backupNewMovie);
                        }
                        log($"{movieYear}-{movieTitle}");
                        log($"{videoYear}-{videoTitle}");

                        log(Environment.NewLine);
                    }

                    if (Math.Abs(int.Parse(movieYear) - int.Parse(videoYear!)) > 0)
                    {
                        log(movie);
                        log(movieYear);
                        log(videoYear);
                    }

                    videoTitle = videoTitle?.Replace(Delimiter, string.Empty).Replace(":", string.Empty);
                    if (string.Equals(videoTitle, movieName[0].Split("=").Last().Replace(SubtitleSeparator, " "), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    if (string.Equals(videoTitle, movieTitle.Split(SubtitleSeparator).Last(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    movieTitle = movieTitle.Replace(SubtitleSeparator, " ");
                    if (!string.Equals(videoTitle, movieTitle, StringComparison.InvariantCultureIgnoreCase))
                    {
                        log(movie);
                        log(movieTitle);
                        log(videoTitle);
                        log(Environment.NewLine);
                    }
                });
        }

        internal static async Task PrintVersions(string x265JsonPath, string h264JsonPath, string ytsJsonPath, string ignoreJsonPath, Action<string>? log = null, params (string Directory, int Level)[] directories)
        {
            log ??= TraceLog;
            Dictionary<string, RarbgMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
            Dictionary<string, RarbgMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
            Dictionary<string, YtsMetadata[]> ytsMetadata = JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(ytsJsonPath))!;
            HashSet<string> ignore = new(JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(ignoreJsonPath))!);

            directories
                .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
                .ForEach(movie =>
                {
                    string[] files = Directory.GetFiles(movie);
                    string? json = files.SingleOrDefault(file => file.HasExtension(JsonMetadataExtension));
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        log($"!!! Missing IMDB metadata {movie}");
                        log(string.Empty);
                        return;
                    }

                    string imdbId = Path.GetFileNameWithoutExtension(json).Split(".")[0];
                    if (imdbId == NotExistingFlag)
                    {
                        log($"{NotExistingFlag} {movie}");
                        return;
                    }

                    string[] videos = files.Where(file => file.IsCommonVideo()).ToArray();
                    if (videos.All(video => VideoFileInfo.TryParse(video, out VideoFileInfo? parsed) && parsed.Definition == ".2160p"))
                    {
                        return;
                    }

                    RarbgMetadata[] availableX265Metadata = x265Metadata.ContainsKey(imdbId)
                        ? x265Metadata[imdbId]
                            .Where(metadata => VideoFileInfo.IsTopOrPremium(metadata.Title))
                            .ToArray()
                        : Array.Empty<RarbgMetadata>();
                    if (availableX265Metadata.Any())
                    {
                        RarbgMetadata[] otherX265Metadata = availableX265Metadata
                            .Where(metadata => !ignore.Contains(metadata.Title))
                            .Where(metadata => videos.All(video =>
                            {
                                string videoName = Path.GetFileNameWithoutExtension(video);
                                VideoFileInfo videoInfo = new(videoName);
                                string title = metadata.Title;
                                return !videoInfo.IsTop
                                    || !videoName.StartsWithIgnoreCase(metadata.Title)
                                    && (videoInfo.Origin != ".BluRay" || !title.ContainsIgnoreCase(".WEBRip."))
                                    && (videoInfo.Edition.Contains("DUBBED") || !title.ContainsIgnoreCase(".DUBBED."))
                                    && (videoInfo.Edition.Contains("SUBBED") || !title.ContainsIgnoreCase(".SUBBED."))
                                    && (string.IsNullOrWhiteSpace(videoInfo.Edition) || !(videoInfo with { Edition = string.Empty }).Name.StartsWithIgnoreCase(title))
                                    && (!videoInfo.Edition.Contains(".Part") || !title.Contains(".Part"))
                                    && (!VideoFileInfo.TryParse(title, out VideoFileInfo? metadataInfo) || !string.Equals(videoInfo.Origin, metadataInfo.Origin, StringComparison.OrdinalIgnoreCase) || !videoInfo.Edition.IsNotNullOrWhiteSpace()
                                        || !metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).All(edition => metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).Contains(edition)));
                            }))
                            .ToArray();
                        if (otherX265Metadata.Any())
                        {
                            log(movie);
                            videos.ForEach(file => log(Path.GetFileName(file)));
                            otherX265Metadata.ForEach(metadata =>
                            {
                                log($"x265 {metadata.Link}");
                                log(metadata.Title);
                                log(string.Empty);
                            });
                        }

                        return;
                    }

                    if (videos.Any(video => VideoFileInfo.TryParse(video, out VideoFileInfo? parsed) && parsed.IsTop))
                    {
                        return;
                    }

                    RarbgMetadata[] availableH264Metadata = h264Metadata.ContainsKey(imdbId)
                        ? h264Metadata[imdbId]
                            .Where(metadata => VideoFileInfo.IsTopOrPremium(metadata.Title))
                            .ToArray()
                        : Array.Empty<RarbgMetadata>();
                    if (availableH264Metadata.Any())
                    {
                        RarbgMetadata[] otherH264Metadata = availableH264Metadata
                            .Where(metadata => !ignore.Contains(metadata.Title))
                            .Where(metadata => videos.All(video =>
                            {
                                string videoName = Path.GetFileNameWithoutExtension(video);
                                VideoFileInfo videoInfo = new(videoName);
                                string title = metadata.Title;
                                return !videoInfo.IsPremium
                                    || !videoName.StartsWithIgnoreCase(metadata.Title)
                                    && (videoInfo.Origin != ".BluRay" || !title.ContainsIgnoreCase(".WEBRip."))
                                    && (videoInfo.Edition.Contains("DUBBED") || !title.ContainsIgnoreCase(".DUBBED."))
                                    && (videoInfo.Edition.Contains("SUBBED") || !title.ContainsIgnoreCase(".SUBBED."))
                                    && (string.IsNullOrWhiteSpace(videoInfo.Edition) || !(videoInfo with { Edition = string.Empty }).Name.StartsWithIgnoreCase(title))
                                    && (!videoInfo.Edition.Contains(".Part") || !title.Contains(".Part"))
                                    && (!VideoFileInfo.TryParse(title, out VideoFileInfo? metadataInfo) || !string.Equals(videoInfo.Origin, metadataInfo.Origin, StringComparison.OrdinalIgnoreCase) || !videoInfo.Edition.IsNotNullOrWhiteSpace()
                                        || !metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).All(edition => metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).Contains(edition)));
                            }))
                            .ToArray();
                        if (otherH264Metadata.Any())
                        {
                            log(movie);
                            videos.ForEach(file => log(Path.GetFileName(file)));
                            otherH264Metadata.ForEach(metadata =>
                            {
                                log($"H264 {metadata.Link}");
                                log(metadata.Title);
                                log(string.Empty);
                            });
                        }

                        return;
                    }

                    if (videos.Any(video => VideoFileInfo.TryParse(video, out VideoFileInfo? parsed) && parsed.IsPremium))
                    {
                        return;
                    }

                    YtsMetadata[] availableYtsMetadata = ytsMetadata.ContainsKey(imdbId)
                        ? ytsMetadata[imdbId]
                        : Array.Empty<YtsMetadata>();
                    if (availableYtsMetadata.Any())
                    {
                        string[] ytsVideos = videos
                            .Select(video => Path.GetFileNameWithoutExtension(video)!)
                            .Where(video => new VideoFileInfo(video).IsPreferred)
                            .ToArray();
                        Debug.Assert(ytsVideos.Length <= 1);
                        (YtsMetadata metadata, KeyValuePair<string, string> version)[] otherYtsMetadata = availableYtsMetadata
                            .SelectMany(metadata => metadata.Availabilities, (metadata, version) => (metadata, version))
                            .ToArray();
                        (YtsMetadata metadata, KeyValuePair<string, string> version)[] hdYtsMetadata = otherYtsMetadata
                            .Where(metadataVersion => metadataVersion.version.Key.ContainsIgnoreCase("1080p"))
                            .ToArray();
                        if (hdYtsMetadata.Any())
                        {
                            otherYtsMetadata = hdYtsMetadata;
                        }

                        (YtsMetadata metadata, KeyValuePair<string, string> version)[] blueRayYtsMetadata = otherYtsMetadata
                            .Where(metadataVersion => metadataVersion.version.Key.ContainsIgnoreCase("BluRay"))
                            .ToArray();
                        if (blueRayYtsMetadata.Any())
                        {
                            otherYtsMetadata = blueRayYtsMetadata;
                        }

                        if (ytsVideos.Any())
                        {
                            otherYtsMetadata = otherYtsMetadata
                                .Where(metadataVersion => ytsVideos.Any(ytsVideo =>
                                    metadataVersion.version.Key.Split(".").All(keyword => !ytsVideo.ContainsIgnoreCase(keyword))))
                                .ToArray();
                        }

                        if (otherYtsMetadata.Any())
                        {
                            if (videos.Any(video => video.Contains("1080p")) && otherYtsMetadata.All(metadataVersion => metadataVersion.version.Key.Contains("720p")))
                            {
                                return;
                            }

                            log(movie);
                            videos.ForEach(file => log(Path.GetFileName(file)));
                            otherYtsMetadata.ForEach(metadataVersion =>
                            {
                                log($"Preferred {metadataVersion.metadata.Link}");
                                log($"{metadataVersion.version.Key} {metadataVersion.version.Value}");
                                log(string.Empty);
                            });
                        }
                    }
                });
        }

        internal static async Task PrintSpecialTitles(string specialJsonPath, string x265JsonPath, string h264JsonPath, string ytsJsonPath, Action<string>? log = null)
        {
            log ??= TraceLog;
            string[] specialImdbIds = JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(specialJsonPath))!;
            Dictionary<string, RarbgMetadata[]> x265Summaries = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
            Dictionary<string, RarbgMetadata[]> h264Summaries = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
            Dictionary<string, YtsMetadata[]> ytsDetails = JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(ytsJsonPath))!;

            specialImdbIds = specialImdbIds
                .Where(imdbId => x265Summaries.ContainsKey(imdbId))
                .Concat(specialImdbIds.Where(imdbId => h264Summaries.ContainsKey(imdbId)))
                .Concat(specialImdbIds.Where(imdbId => ytsDetails.ContainsKey(imdbId)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            specialImdbIds.ForEach(imdbId =>
            {
                log($"{imdbId} https://www.imdb.com/title/{imdbId}");
                if (x265Summaries.ContainsKey(imdbId))
                {
                    x265Summaries[imdbId].ForEach(summary =>
                    {
                        log($"x265 {summary.Link}");
                        log(summary.Title);
                    });
                }

                if (h264Summaries.ContainsKey(imdbId))
                {
                    h264Summaries[imdbId].ForEach(summary =>
                    {
                        log($"H264 {summary.Link}");
                        log(summary.Title);
                    });
                }

                if (ytsDetails.ContainsKey(imdbId))
                {
                    ytsDetails[imdbId].ForEach(details =>
                    {
                        log($"Preferred {details.Link}");
                        log(details.Title);
                    });
                }

                log(string.Empty);
            });
        }

        internal static async Task PrintHighRatingAsync(string x265JsonPath, string h264JsonPath, string ytsJsonPath, 
            string threshold = "8.0", string[]? excludedGenres = null, Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
            HashSet<string> existingImdbIds = new(
                directories.SelectMany(directory => Directory
                    .EnumerateFiles(directory, JsonMetadataSearchPattern, SearchOption.AllDirectories)
                    .Select(file => Path.GetFileNameWithoutExtension(file).Split(".").First())
                    .Where(imdbId => imdbId != NotExistingFlag)),
                StringComparer.OrdinalIgnoreCase);
            Dictionary<string, RarbgMetadata[]> x265Summaries = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
            Dictionary<string, RarbgMetadata[]> h264Summaries = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
            Dictionary<string, YtsMetadata[]> ytsDetails = JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(ytsJsonPath))!;

            Dictionary<string, Dictionary<string, IMetadata[]>> highRatings = new();

            x265Summaries
                .Where(summaries =>
                    !existingImdbIds.Contains(summaries.Key)
                    && summaries.Value.Any(summary => string.Compare(summary.ImdbRating, threshold, StringComparison.Ordinal) >= 0)
                    && (excludedGenres is null || !summaries.Value.Any(summary => excludedGenres.Any(excludedGenre => summary.Genres.Contains(excludedGenre, StringComparer.OrdinalIgnoreCase)))))
                .ForEach(summaries =>
                {
                    if (!highRatings.ContainsKey(summaries.Key))
                    {
                        highRatings[summaries.Key] = new();
                    }

                    highRatings[summaries.Key]["x265"] = summaries.Value;
                });

            h264Summaries
                .Where(summaries =>
                    !existingImdbIds.Contains(summaries.Key)
                    && summaries.Value.Any(summary => string.Compare(summary.ImdbRating, threshold, StringComparison.Ordinal) >= 0)
                    && (excludedGenres is null || !summaries.Value.Any(summary => excludedGenres.Any(excludedGenre => summary.Genres.Contains(excludedGenre, StringComparer.OrdinalIgnoreCase)))))
                .ForEach(summaries =>
                {
                    if (!highRatings.ContainsKey(summaries.Key))
                    {
                        highRatings[summaries.Key] = new();
                    }

                    highRatings[summaries.Key]["H264"] = summaries.Value;
                });

            ytsDetails
                .Where(summaries =>
                    !existingImdbIds.Contains(summaries.Key)
                    && summaries.Value.Any(summary => string.Compare(summary.ImdbRating, threshold, StringComparison.Ordinal) >= 0)
                    && (excludedGenres is null || !summaries.Value.Any(summary => excludedGenres.Any(excludedGenre => summary.Genres.Contains(excludedGenre, StringComparer.OrdinalIgnoreCase)))))
                .ForEach(summaries =>
                {
                    if (!highRatings.ContainsKey(summaries.Key))
                    {
                        highRatings[summaries.Key] = new();
                    }

                    highRatings[summaries.Key]["Preferred"] = summaries.Value;
                });

            highRatings
                .ForEach(summaries =>
                {
                    log(string.Join(" ", summaries.Value.SelectMany(summary => summary.Value).SelectMany(summary => summary.Genres).Distinct()));
                    log($"{summaries.Key} https://www.imdb.com/title/{summaries.Key}");
                    summaries.Value.SelectMany(pair => pair.Value.Select(summary => (pair.Key, summary))).ForEach(summary =>
                    {
                        log($"{summary.Key} {summary.summary.ImdbRating} {summary.summary.Link}");
                        log(summary.summary.Title);
                    });
                    log(string.Empty);
                });
        }

        internal static void PrintDirectoryTitleMismatch(string directory, int level = 2, Action<string>? log = null)
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

                    string imdbTitle = HttpUtility.HtmlDecode(imdbMetadata.Title);

                    if (!imdbMetadata.Titles.TryGetValue("World-wide (English title)", out string[]? releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.Contains("World-wide (English title)", StringComparison.OrdinalIgnoreCase))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (!releaseTitles.Any()
                        && !imdbMetadata.Titles.TryGetValue("USA", out releaseTitles)
                        && !imdbMetadata.Titles.TryGetValue("USA (working title)", out releaseTitles)
                        && !imdbMetadata.Titles.TryGetValue("USA (informal English title)", out releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.Contains("USA", StringComparison.OrdinalIgnoreCase))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (!releaseTitles.Any()
                        && !imdbMetadata.Titles.TryGetValue("UK", out releaseTitles)
                        && !imdbMetadata.Titles.TryGetValue("UK (informal English title)", out releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.Contains("UK", StringComparison.OrdinalIgnoreCase))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (!releaseTitles.Any()
                        && !imdbMetadata.Titles.TryGetValue("Hong Kong (English title)", out releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.Contains("Hong Kong (English title)", StringComparison.OrdinalIgnoreCase))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    string[] imdbTitles = imdbTitle.Split(Imdb.TitleSeparator)
                        .Concat(releaseTitles)
                        .Select(title => title
                            .FilterForFileSystem()
                            .Replace(" 3D", string.Empty)
                            .Replace(" 3-D", string.Empty)
                            .Replace("3D ", string.Empty)
                            .Replace("3-D ", string.Empty)
                            .Replace(".", string.Empty)
                            .Replace("[", string.Empty)
                            .Replace("]", string.Empty)
                            .Trim())
                        .SelectMany(imdbTitle => new string[]
                        {
                            imdbTitle,
                            HttpUtility.HtmlDecode(imdbMetadata.Title).Replace(SubtitleSeparator, " ").FilterForFileSystem(),
                            imdbTitle.Replace("(", string.Empty).Replace(")", string.Empty),
                            Regex.Replace(imdbTitle, @"\(.+\)", string.Empty).Trim(),
                            imdbTitle.Replace("...", string.Empty),
                            imdbTitle.Replace("#", "No "),
                            imdbTitle,
                            imdbTitle.Replace(SubtitleSeparator, " "),
                            imdbTitle.Replace(" · ", " "),
                            imdbTitle.Replace(" - ", " "),
                            imdbTitle.Replace(" - ", SubtitleSeparator),
                            imdbTitle.Replace(" Vol ", SubtitleSeparator, StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" Chapter ", SubtitleSeparator, StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(SubtitleSeparator, " "),
                            imdbTitle.Replace("zero", "0", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("one", "1", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("two", "2", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("three", "3", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("four", "4", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("five", "5", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" IV", " 4", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" III", " 3", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" II", " 2", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" Part II", " 2", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" -Part ", " ", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("-Part ", " ", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" - Part ", " ", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("- Part ", " ", StringComparison.OrdinalIgnoreCase),
                            $"XXX {imdbTitle.Replace(" XXX", string.Empty)}"
                        })
                        .ToArray();
                    List<string> localTitles = new()
                    {
                        parsed.DefaultTitle1,
                        parsed.DefaultTitle1.Replace(InstallmentSeparator, " "),
                        parsed.DefaultTitle1.Replace(InstallmentSeparator, "-"),
                        parsed.DefaultTitle1.Split(InstallmentSeparator).First(),
                        $"{parsed.DefaultTitle1.Split(InstallmentSeparator).First()}{parsed.DefaultTitle2}",
                        $"{parsed.DefaultTitle1.Split(InstallmentSeparator).First()}{parsed.DefaultTitle2.Replace(SubtitleSeparator, " ")}",
                        parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()),
                        parsed.DefaultTitle3.TrimStart(SubtitleSeparator.ToCharArray()),
                        parsed.OriginalTitle1.TrimStart('='),
                        $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}",
                        $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(SubtitleSeparator, " "),
                        $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(SubtitleSeparator, " ").Replace(InstallmentSeparator, " "),
                        $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(InstallmentSeparator, " "),
                        $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(InstallmentSeparator, SubtitleSeparator),
                        $"{parsed.DefaultTitle1.Split(InstallmentSeparator).First()}{parsed.DefaultTitle2.Split(InstallmentSeparator).First()}".Replace(SubtitleSeparator, " "),
                        parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Replace(InstallmentSeparator, " "),
                        parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Replace(InstallmentSeparator, "-"),
                        parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First(),
                        $"{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First()}{parsed.DefaultTitle3.Split(InstallmentSeparator).First()}",
                        $"{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First()}{parsed.DefaultTitle3.Split(InstallmentSeparator).First()}".Replace(SubtitleSeparator, " "),
                    };
                    if (!string.IsNullOrWhiteSpace(parsed.DefaultTitle2))
                    {
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..]}");
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..].Replace(InstallmentSeparator, " ")}");
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()}");
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant().Replace(InstallmentSeparator, " ")}");
                    }
                    if (imdbTitles.Any(a => localTitles.Any(b => string.Equals(a, b))))
                    {
                        return;
                    }

                    parsed = parsed with { DefaultTitle1 = imdbTitle };
                    string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), parsed.ToString());
                    if (string.Equals(movie, newMovie, StringComparison.Ordinal))
                    {
                        return;
                    }

                    log(movie);
                    log(newMovie);
                    log(string.Empty);
                });
        }

        internal static void PrintDirectoryOriginalTitleMismatch(string directory, int level = 2, Action<string>? log = null)
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

                    string imdbTitle = HttpUtility.HtmlDecode(imdbMetadata.OriginalTitle);
                    if (string.IsNullOrEmpty(imdbTitle))
                    {
                        return;
                    }

                    string[] imdbTitles = imdbTitle.Split(Imdb.TitleSeparator)
                        .Select(title => title
                            .FilterForFileSystem()
                            .Replace(" 3D", string.Empty)
                            .Replace(" 3-D", string.Empty)
                            .Replace("3D ", string.Empty)
                            .Replace("3-D ", string.Empty)
                            .Replace(".", string.Empty)
                            .Replace("[", string.Empty)
                            .Replace("]", string.Empty)
                            .Trim())
                        .SelectMany(imdbTitle => new string[]
                        {
                            imdbTitle,
                            HttpUtility.HtmlDecode(imdbMetadata.Title).Replace(SubtitleSeparator, " ").FilterForFileSystem(),
                            imdbTitle.Replace("(", string.Empty).Replace(")", string.Empty),
                            Regex.Replace(imdbTitle, @"\(.+\)", string.Empty).Trim(),
                            imdbTitle.Replace("...", string.Empty),
                            imdbTitle.Replace("#", "No "),
                            imdbTitle,
                            imdbTitle.Replace(SubtitleSeparator, " "),
                            imdbTitle.Replace(" · ", " "),
                            imdbTitle.Replace(" - ", " "),
                            imdbTitle.Replace(" - ", SubtitleSeparator),
                            imdbTitle.Replace(" Vol ", SubtitleSeparator, StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" Chapter ", SubtitleSeparator, StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(SubtitleSeparator, " "),
                            imdbTitle.Replace("zero", "0", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("one", "1", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("two", "2", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("three", "3", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("four", "4", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("five", "5", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" IV", " 4", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" III", " 3", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" II", " 2", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" Part II", " 2", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" -Part ", " ", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("-Part ", " ", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace(" - Part ", " ", StringComparison.OrdinalIgnoreCase),
                            imdbTitle.Replace("- Part ", " ", StringComparison.OrdinalIgnoreCase),
                            $"XXX {imdbTitle.Replace(" XXX", string.Empty)}"
                        })
                        .ToArray();
                    List<string> localTitles = new()
                    {
                        parsed.DefaultTitle1,
                        parsed.OriginalTitle1.Replace(InstallmentSeparator, " "),
                        parsed.OriginalTitle1.Replace(InstallmentSeparator, "-"),
                        parsed.OriginalTitle1.Split(InstallmentSeparator).First(),
                        $"{parsed.OriginalTitle1.Split(InstallmentSeparator).First()}{parsed.OriginalTitle2}",
                        $"{parsed.OriginalTitle1.Split(InstallmentSeparator).First()}{parsed.OriginalTitle2.Replace(SubtitleSeparator, " ")}",
                        parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()),
                        parsed.OriginalTitle3.TrimStart(SubtitleSeparator.ToCharArray()),
                        parsed.OriginalTitle1.TrimStart('='),
                        $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}",
                        $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(SubtitleSeparator, " "),
                        $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(SubtitleSeparator, " ").Replace(InstallmentSeparator, " "),
                        $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(InstallmentSeparator, " "),
                        $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(InstallmentSeparator, SubtitleSeparator),
                        $"{parsed.OriginalTitle1.Split(InstallmentSeparator).First()}{parsed.OriginalTitle2.Split(InstallmentSeparator).First()}".Replace(SubtitleSeparator, " "),
                        parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Replace(InstallmentSeparator, " "),
                        parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Replace(InstallmentSeparator, "-"),
                        parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First(),
                        $"{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First()}{parsed.OriginalTitle3.Split(InstallmentSeparator).First()}",
                        $"{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First()}{parsed.OriginalTitle3.Split(InstallmentSeparator).First()}".Replace(SubtitleSeparator, " "),
                    };
                    if (!string.IsNullOrWhiteSpace(parsed.OriginalTitle2))
                    {
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..]}");
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..].Replace(InstallmentSeparator, " ")}");
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()}");
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant().Replace(InstallmentSeparator, " ")}");
                    }
                    if (imdbTitles.Any(a => localTitles.Any(b => string.Equals(a, b))))
                    {
                        return;
                    }

                    parsed = parsed with { OriginalTitle1 = $"={imdbTitle}" };
                    string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), parsed.ToString());
                    if (string.Equals(movie, newMovie, StringComparison.Ordinal))
                    {
                        return;
                    }

                    log(movie);
                    log(newMovie);
                    log(string.Empty);
                });
        }
    }
}
