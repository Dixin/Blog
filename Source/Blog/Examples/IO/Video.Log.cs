namespace Examples.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Examples.Net;

    internal static partial class Video
    {
        internal static void PrintVideosWithErrors(string directory, bool isNoAudioAllowed = false, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, Action<string>? log = null)
        {
            PrintVideosWithErrors(
               Directory.EnumerateFiles(directory, AllSearchPattern, searchOption)
                   .Where(file => predicate?.Invoke(file) ?? AllVideoExtensions.Where(extension => !string.Equals(extension, ".iso", StringComparison.InvariantCultureIgnoreCase)).Any(extension => file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))),
                isNoAudioAllowed,
                log);
        }

        internal static void PrintVideosWithErrors(IEnumerable<string> files, bool isNoAudioAllowed = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            files
                .ToArray()
                .AsParallel()
                .Select((video, index) => GetVideoMetadata(video, log: message => log($"{index} {message}")))
                .Select(video => (Video: video, Error: GetVideoError(video, isNoAudioAllowed)))
                .Where(result => !string.IsNullOrWhiteSpace(result.Error))
                .AsSequential()
                .OrderBy(result => result.Video.File)
                .ForEach(result => log(result.Error ?? string.Empty));
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
            Regex[] allPreferred = PreferredVersions.Concat(PremiumVersions).Concat(TopVersions).ToArray();
            PrintVideos(directory, level, file => !allPreferred.Any(version => version.IsMatch(Path.GetFileNameWithoutExtension(file))));
        }

        internal static void PrintVideosPreferred(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file => PreferredVersions.Any(version => version.IsMatch(Path.GetFileNameWithoutExtension(file))));
        }

        internal static void PrintVideosNonTop(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file => !TopVersions.Any(version => version.IsMatch(Path.GetFileNameWithoutExtension(file))));
        }

        internal static void PrintVideosPremium(string directory, int level = 2, Action<string>? log = null)
        {
            PrintVideos(directory, level, file => PremiumVersions.Any(version => version.IsMatch(Path.GetFileNameWithoutExtension(file))));
        }

        private static void PrintVideos(string directory, int level, Func<string, bool> predicate, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie => Directory
                    .GetFiles(movie, AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(file => file.IsCommonVideo() && predicate(file))
                    .ForEach(log));
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
                .Select(movie => (movie, metadata: XDocument.Load(Directory.GetFiles(movie, MetadataSearchPattern, SearchOption.TopDirectoryOnly).First())))
                .Select(movie => (movie.movie, field: movie.metadata.Root?.Element(field)?.Value))
                .OrderBy(movie => movie.field)
                .ForEach(movie => log($"{movie.field}: {Path.GetFileName(movie.movie)}"));
        }

        internal static void PrintMetadataByDuplication(string directory, string field = "imdbid", Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetFiles(directory, MetadataSearchPattern, SearchOption.AllDirectories)
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
                    XDocument metadata = XDocument.Load(Directory.GetFiles(movie, MetadataSearchPattern, SearchOption.TopDirectoryOnly).First());
                    string movieDirectory = Path.GetFileName(movie);
                    if (movieDirectory.StartsWith("0."))
                    {
                        movieDirectory = movieDirectory.Substring("0.".Length);
                    }
                    Match match = MovieDirectoryRegex.Match(movieDirectory);
                    Debug.Assert(match.Success);
                    string directoryYear = match.Groups[1].Value;
                    string metadataYear = metadata.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{metadata} has no year.");
                    string videoName = string.Empty;
                    if (!(directoryYear == metadataYear
                        && Directory.GetFiles(movie, AllSearchPattern, SearchOption.TopDirectoryOnly)
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
            directories.SelectMany(directory => Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories))
                .GroupBy(Path.GetFileNameWithoutExtension)
                .Where(group => group.Count() > 1)
                .ForEach(group => group.ForEach(log));
        }

        private static readonly string[] Attachments = { "Introduction.txt", "Introduction.mht" };

        private static readonly string[] AdaptiveAttachments = new string[] { "banner.jpg", "box.jpg", "clearart.png", "clearlogo.png", "disc.png", "discart.png", "fanart.jpg", "landscape.jpg", "logo.png", "poster.jpg", "poster.png" };

        private static readonly string[] ImdbExtensions = { ".json", ".region" };

        private static readonly string[] SubtitleLanguages = { "can", "chs", "chs&dan", "chs&eng", "chs&fre", "chs&ger", "chs&spa", "cht", "cht&eng", "cht&ger", "dut", "eng", "eng&chs", "fin", "fre", "ger", "ger&chs", "ita", "jap", "kor", "pol", "por", "rus", "spa", "swe", "commentary", "commentary1", "commentary2" };

        internal static void PrintDirectoriesWithErrors(string directory, int level = 2, bool isLoadingVideo = false, bool isNoAudioAllowed = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            List<string>? allVideos = null;
            if (isLoadingVideo)
            {
                allVideos = new List<string>();
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

                    if (!MovieDirectoryRegex.IsMatch(trimmedMovie))
                    {
                        log($"!Directory pattern: {movie}");
                    }

                    string[] files = Directory.GetFiles(movie, AllSearchPattern, SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();

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
                    string[] subtitles = files.Where(file => file.AnyExtension(AllSubtitleExtensions)).ToArray();
                    string[] metadataFiles = files.Where(file => file.HasExtension(MetadataExtension)).ToArray();
                    string[] imdbFiles = files.Where(file => file.AnyExtension(ImdbExtensions)).ToArray();
                    string[] otherFiles = files.Except(videos).Except(subtitles).Except(metadataFiles).Except(imdbFiles).ToArray();
                    if (videos.Length < 1)
                    {
                        log($"!No video: {movie}");
                    }
                    else if (videos.Length == 1 || videos.All(video => Regex.IsMatch(Path.GetFileNameWithoutExtension(video), @"\.cd[1-9]$", RegexOptions.IgnoreCase)))
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
                        .Select(video => $"{Path.GetFileNameWithoutExtension(video)}{MetadataExtension}")
                        .ToArray();
                    metadataFiles
                        .Where(metadata => !allowedMetadataFiles.Contains(metadata, StringComparer.InvariantCultureIgnoreCase))
                        .ForEach(file => log($"!Metadata: {Path.Combine(movie, file)}"));

                    files
                        .Except(imdbFiles)
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

                    string directoryRating = Regex.Match(trimmedMovie, @"\[([0-9]\.[0-9]|\-)\]").Value;
                    string imdbRating = Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata)
                        ? imdbMetadata.FormattedAggregateRating
                        : "-";
                    imdbRating = $"[{imdbRating}]";
                    if (!string.Equals(directoryRating, imdbRating, StringComparison.InvariantCulture))
                    {
                        log($"!Imdb rating {directoryRating} should be {imdbRating}: {movie}");
                    }

                    metadataFiles.ForEach(metadataFile =>
                    {
                        metadataFile = Path.Combine(movie, metadataFile);
                        XDocument metadata = XDocument.Load(Path.Combine(movie, metadataFile));
                        string? metadataImdbId = metadata.Root?.Element("imdbid")?.Value;
                        if (imdbMetadata == null)
                        {
                            if (!string.IsNullOrWhiteSpace(metadataImdbId))
                            {
                                log($"!Metadata should have no imdb id: {metadataFile}");
                            }
                        }
                        else
                        {
                            if (!string.Equals(imdbMetadata.Id, metadataImdbId))
                            {
                                log($"!Metadata imdb id {metadataImdbId} should be {imdbMetadata.Id}: {metadataFile}");
                            }
                        }
                    });

                    string? imdbYear = imdbMetadata?.Year;
                    if (!string.IsNullOrWhiteSpace(imdbYear))
                    {
                        string directoryYear = MovieDirectoryRegex.Match(trimmedMovie).Groups[1].Value;
                        if (!string.Equals(directoryYear, imdbYear))
                        {
                            log($"!Year should be {imdbYear}: {movie}");
                        }
                    }

                    string[] directories = Directory.GetDirectories(movie);
                    if (directories.Length > 1)
                    {
                        log($"!Directory {directories.Length}: {movie}");
                    }

                    if (directories.Length == 1 && !string.Equals("Featurettes", Path.GetFileName(directories.Single())))
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
                        Debug.Assert(videoYear?.Length == 4);
                    }

                    if (string.IsNullOrWhiteSpace(videoYear))
                    {
                        (videoTitle, videoYear) = Directory
                            .GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly)
                            .Select(metadata =>
                                {
                                    XElement root = XDocument.Load(metadata).Root;
                                    return (Title: root.Element("title")?.Value, Year: root.Element("year")?.Value);
                                })
                            .Distinct()
                            .Single();
                    }

                    if (!string.Equals(movieYear, videoYear))
                    {
                        log(movie);
                        movieName[1] = videoYear!;
                        string newMovie = Path.Combine(Path.GetDirectoryName(movie), string.Join(".", movieName));
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

                    if (Math.Abs(int.Parse(movieYear) - int.Parse(videoYear)) > 0)
                    {
                        log(movie);
                        log(movieYear);
                        log(videoYear);
                    }

                    videoTitle = videoTitle?.Replace(".", string.Empty).Replace(":", string.Empty);
                    if (string.Equals(videoTitle, movieName[0].Split("=").Last().Replace("-", " "), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    if (string.Equals(videoTitle, movieTitle.Split("-").Last(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    movieTitle = movieTitle.Replace("-", " ");
                    if (!string.Equals(videoTitle, movieTitle, StringComparison.InvariantCultureIgnoreCase))
                    {
                        log(movie);
                        log(movieTitle);
                        log(videoTitle);
                        log(Environment.NewLine);
                    }
                });
        }
    }
}
