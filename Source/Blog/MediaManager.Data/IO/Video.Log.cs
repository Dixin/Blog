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
                        .Where(extension => !extension.EqualsIgnoreCase(".iso"))
                        .Any(file.EndsWithIgnoreCase)),
                isNoAudioAllowed,
                is720,
                is1080,
                log);
        }

        private static void PrintVideosWithErrors(IEnumerable<string> files, bool isNoAudioAllowed = false, Action<string>? is720 = null, Action<string>? is1080 = null, Action<string>? log = null)
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
                .Where(result => result.Error.Message.IsNotNullOrWhiteSpace())
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
                    if (files.All(file => !file.ContainsIgnoreCase("1080p") && !file.ContainsIgnoreCase("2160p")))
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

                    if (!videos.Select((video, index) => Path.GetFileNameWithoutExtension(video).EndsWithIgnoreCase($".cd{index + 1}")).All(isPart => isPart))
                    {
                        log(movie);
                    }
                });
        }

        internal static void PrintVideosP(string directory, int level = 2, Action<string>? log = null) =>
            PrintVideos(directory, level, file => new VideoFileInfo(file).IsP, log);

        internal static void PrintVideosY(string directory, int level = 2, Action<string>? log = null) =>
            PrintVideos(directory, level, file => new VideoFileInfo(file).IsY, log);

        internal static void PrintVideosNotX(string directory, int level = 2, Action<string>? log = null) =>
            PrintVideos(directory, level, file => !new VideoFileInfo(file).IsX, log);

        internal static void PrintVideosH(string directory, int level = 2, Action<string>? log = null) =>
            PrintVideos(directory, level, file => new VideoFileInfo(file).IsH, log);

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
                        log(Path.GetFileNameWithoutExtension(files.Single(file => file.HasExtension(ImdbMetadataExtension))).Split(".")[0]);
                        videos.ForEach(log);
                        log(string.Empty);
                    }
                });
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .Where(movie => Directory.GetFiles(movie).All(video => AllSubtitleExtensions.All(extension => !video.EndsWithIgnoreCase(extension))))
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
                    if (movieDirectory.StartsWithOrdinal("0."))
                    {
                        movieDirectory = movieDirectory.Substring("0.".Length);
                    }

                    VideoDirectoryInfo videoDirectoryInfo = new(movieDirectory);
                    string directoryYear = videoDirectoryInfo.Year;
                    string metadataYear = metadata.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{metadata} has no year.");
                    string videoName = string.Empty;
                    if (!(directoryYear.EqualsOrdinal(metadataYear)
                        && Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                            .Where(file => AllVideoExtensions.Any(file.EndsWithIgnoreCase))
                            .All(video => (videoName = Path.GetFileName(video) ?? throw new InvalidOperationException($"{video} is invalid.")).ContainsOrdinal(directoryYear))))
                    {
                        log($"Directory: {directoryYear}, Metadata {metadataYear}, Video: {videoName}, {movie}");
                    }
                });
        }

        internal static void PrintDirectoriesWithNonLatinOriginalTitle(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .Where(movie => movie.ContainsOrdinal("="))
                .Where(movie => !Regex.IsMatch(movie.Split("=")[1], "^[a-z]{1}.", RegexOptions.IgnoreCase))
                .ForEach(log);
        }

        internal static void PrintDuplicateImdbId(Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
            directories.SelectMany(directory => Directory.EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
                .GroupBy(metadata => Path.GetFileNameWithoutExtension(metadata).Split(".")[0])
                .Where(group => group.Count() > 1)
                .ForEach(group =>
                {
                    group.OrderBy(metadata => metadata).ForEach(log);
                    log(string.Empty);
                });
        }

        internal static void PrintDirectoriesWithErrors(string directory, int level = 2, bool isLoadingVideo = false, bool isNoAudioAllowed = false, bool isTV = false, Action<string>? log = null)
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
                    if (trimmedMovie.StartsWithOrdinal("0."))
                    {
                        trimmedMovie = trimmedMovie.Substring("0.".Length);
                    }

                    if (trimmedMovie.ContainsOrdinal("{"))
                    {
                        trimmedMovie = trimmedMovie.Substring(0, trimmedMovie.IndexOfOrdinal("{"));
                    }

                    if (!VideoDirectoryInfo.TryParse(trimmedMovie, out VideoDirectoryInfo? directoryInfo))
                    {
                        log($"!Directory: {trimmedMovie}");
                        return;
                    }

                    string[] allPaths = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
                    string[] translations = { directoryInfo.TranslatedTitle1, directoryInfo.TranslatedTitle2, directoryInfo.TranslatedTitle3 };
                    string[] titles =
                    {
                        directoryInfo.DefaultTitle1, directoryInfo.DefaultTitle2.TrimStart('-'), directoryInfo.DefaultTitle3.TrimStart('-'),
                        directoryInfo.OriginalTitle1.TrimStart('='), directoryInfo.OriginalTitle2.TrimStart('-'), directoryInfo.OriginalTitle3.TrimStart('-'),
                        directoryInfo.TranslatedTitle1, directoryInfo.TranslatedTitle2.TrimStart('-'), directoryInfo.TranslatedTitle3.TrimStart('-')
                    };

                    if (Regex.IsMatch(trimmedMovie, @"·[0-9]"))
                    {
                        log($"!Special character ·: {trimmedMovie}");
                    }

                    "：@#(){}".ToCharArray().Where(trimmedMovie.Contains).ForEach(specialCharacter => log($"!Special character {specialCharacter}: {trimmedMovie}"));

                    translations
                        .Where(translated => Regex.IsMatch(translated.Split("`").First(), "[0-9]+"))
                        .ForEach(translated => log($"!Translation has number {translated}: {movie}"));
                    translations
                        .Where(translation => !string.IsNullOrEmpty(translation) && translation.All(character => character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' || char.IsPunctuation(character) || char.IsSeparator(character) || char.IsSymbol(character) || char.IsWhiteSpace(character)))
                        .ForEach(translated => log($"!Title not translated {translated}: {movie}"));

                    titles
                        .Where(title => title.IsNotNullOrWhiteSpace() && (char.IsWhiteSpace(title.First()) || char.IsWhiteSpace(title.Last())))
                        .ForEach(title => log($"!Title has white space {title}: {movie}"));

                    allPaths.Where(path => path.Length > 256).ForEach(path => log($"!Path too long: {path}"));

                    string[] topFiles = Directory
                        .GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                        .Select(file => Path.GetFileName(file) ?? throw new InvalidOperationException(file))
                        .ToArray();

                    string imdbRating = Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata)
                        ? imdbMetadata.FormattedAggregateRating
                        : NotExistingFlag;

                    string? imdbYear = imdbMetadata?.Year;
                    if (imdbYear.IsNotNullOrWhiteSpace() && !directoryInfo.Year.EqualsOrdinal(imdbYear))
                    {
                        log($"!Year should be {imdbYear}: {movie}");
                    }

                    if (!directoryInfo.AggregateRating.EqualsOrdinal(imdbRating))
                    {
                        log($"!Imdb rating {directoryInfo.AggregateRating} should be {imdbRating}: {movie}");
                    }

                    string imdbRatingCount = imdbMetadata?.FormattedAggregateRatingCount ?? NotExistingFlag;
                    if (!imdbRatingCount.EqualsOrdinal(directoryInfo.AggregateRatingCount))
                    {
                        log($"!Imdb rating count {directoryInfo.AggregateRatingCount} should be {imdbRatingCount}: {movie}");
                    }

                    string contentRating = imdbMetadata?.FormattedContentRating ?? NotExistingFlag;
                    if (!contentRating.EqualsOrdinal(directoryInfo.ContentRating))
                    {
                        log($"!Content rating {directoryInfo.ContentRating} should be {contentRating}: {movie}");
                    }

                    string[] imdbFiles = topFiles.Where(file => file.HasExtension(ImdbMetadataExtension)).ToArray();
                    string[] cacheFiles = topFiles.Where(file => file.HasExtension(ImdbCacheExtension)).ToArray();

                    if (imdbFiles.Length != 1)
                    {
                        log($"!Imdb files {imdbFiles.Length}: {movie}");
                    }

                    string[] topDirectories = Directory.GetDirectories(movie).Select(topDirectory => Path.GetFileName(topDirectory)!).ToArray();

                    string[] allowedSubtitleLanguages = SubtitleLanguages.Concat(
                            from language1 in SubtitleLanguages
                            from language2 in SubtitleLanguages
                            where !language1.EqualsOrdinal(language2)
                            select $"{language1}&{language2}")
                        .ToArray();

                    if (isTV)
                    {
                        topDirectories
                            .Where(topDirectory => !Featurettes.EqualsOrdinal(topDirectory) && !Regex.IsMatch(topDirectory, @"^Season [0-9]{2,4}(\..+)?"))
                            .ForEach(topDirectory => log($"!Directory incorrect {topDirectory}: {movie}"));

                        string metadataFile = Path.Combine(movie, TVShowMetadataFile);
                        if (!File.Exists(metadataFile))
                        {
                            log($"!Metadata file missing {TVShowMetadataFile}: {movie}");
                        }

                        XDocument metadata = XDocument.Load(Path.Combine(movie, metadataFile));
                        string? metadataImdbId = metadata.Root?.Element("imdb_id")?.Value;
                        if (imdbMetadata is null)
                        {
                            if (metadataImdbId.IsNotNullOrWhiteSpace())
                            {
                                log($"!Metadata https://www.imdb.com/title/{metadataImdbId}/ should have no imdb id: {metadataFile}");
                            }
                        }
                        else if (!imdbMetadata.ImdbId.EqualsOrdinal(metadataImdbId))
                        {
                            log($"!Metadata imdb id {metadataImdbId} should be {imdbMetadata.ImdbId}: {metadataFile}");
                        }

                        string[] seasonPaths = Directory.GetDirectories(movie).Where(path => Regex.IsMatch(Path.GetFileName(path), @"^Season [0-9]{2,4}(\..+)?")).ToArray();
                        seasonPaths.ForEach(season =>
                        {
                            Directory.EnumerateDirectories(season).ForEach(seasonDirectory => log($"!Season directory: {seasonDirectory}"));

                            string[] seasonFiles = Directory.GetFiles(season, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).Select(file => Path.GetFileName(file)!).ToArray();
                            string[] videos = seasonFiles.Where(IsCommonVideo).ToArray();
                            string[] videoMetadataFiles = videos.Select(video => $"{Path.GetFileNameWithoutExtension(video)}{XmlMetadataExtension}").ToArray();
                            string[] videoThumbFiles = videos.Select(video => $"{Path.GetFileNameWithoutExtension(video)}-thumb{ThumbExtension}").ToArray();
                            VideoEpisodeFileInfo?[] allVideoFileInfos = videos
                                .Select(video =>
                                {
                                    VideoEpisodeFileInfo.TryParse(video, out VideoEpisodeFileInfo? videoFileInfo);
                                    return videoFileInfo;
                                })
                                .ToArray();
                            VideoEpisodeFileInfo[] videoFileInfos = allVideoFileInfos.NotNull().ToArray();
                            string[] subtitles = topFiles.Where(file => file.HasAnyExtension(AllSubtitleExtensions)).ToArray();
                            string[] metadataFiles = topFiles.Where(file => file.HasExtension(XmlMetadataExtension)).ToArray();

                            Enumerable.Range(0, videos.Length).Where(index => allVideoFileInfos[index] is null).ForEach(index => log($"!Video name {videos[index]}: {movie}"));

                            videoFileInfos.ForEach((video, index) =>
                            {
                                if (video.EpisodeTitle.IsNullOrWhiteSpace())
                                {
                                    log($"!Episode title missing {videos[index]}");
                                }
                            });

                            videos.Where(video => !Regex.IsMatch(video, @"S[0-9]{2,4}E[0-9]{2,3}[\.E]")).ForEach(video => log($"!Video name: {video}"));

                            string[] allowedSubtitles = videos
                                .Select(video => Path.GetFileNameWithoutExtension(video)!)
                                .Concat(videos.SelectMany(video => allowedSubtitleLanguages, (video, language) => $"{Path.GetFileNameWithoutExtension(video)!}.{language}"))
                                .SelectMany(subtitle => AllSubtitleExtensions, (subtitle, extension) => $"{subtitle}{extension}")
                                .ToArray();

                            seasonFiles
                                .Except(videos)
                                .Except(videoMetadataFiles)
                                .Except(videoThumbFiles)
                                .Except(allowedSubtitles)
                                .Except(EnumerableEx.Return(TVSeasonMetadataFile))
                                .ForEach(file => log($"!File: {file}"));
                        });

                        return;
                    }

                    switch (topDirectories.Length)
                    {
                        case > 1:
                            log($"!Directory count: {topDirectories.Length}: {movie}");
                            break;
                        case 1 when !Featurettes.EqualsOrdinal(topDirectories.Single()):
                            log($"!Directory incorrect: {topDirectories.Single()}");
                            break;
                    }

                    string[] videos = topFiles.Where(IsCommonVideo).ToArray();
                    VideoFileInfo?[] allVideoFileInfos = videos
                        .Select(video =>
                        {
                            VideoFileInfo.TryParse(video, out VideoFileInfo? videoFileInfo);
                            return videoFileInfo;
                        })
                        .ToArray();
                    VideoFileInfo[] videoFileInfos = allVideoFileInfos.NotNull().ToArray();
                    string[] subtitles = topFiles.Where(file => file.HasAnyExtension(AllSubtitleExtensions)).ToArray();
                    string[] metadataFiles = topFiles.Where(file => file.HasExtension(XmlMetadataExtension)).ToArray();
                    string[] otherFiles = topFiles.Except(videos).Except(subtitles).Except(metadataFiles).Except(imdbFiles).Except(cacheFiles).ToArray();

                    Enumerable.Range(0, videos.Length).Where(index => allVideoFileInfos[index] is null).ForEach(index => log($"!Video name {videos[index]}: {movie}"));

                    if (directoryInfo.Is2160P && !videoFileInfos.Any(video => video.Is2160P))
                    {
                        log($"!Not 2160: {movie}");
                    }

                    if (!directoryInfo.Is2160P && videoFileInfos.Any(video => video.Is2160P))
                    {
                        log($"!2160: {movie}");
                    }

                    if (directoryInfo.Is1080P && !videoFileInfos.Any(video => video.Is1080P))
                    {
                        log($"!Not 1080: {movie}");
                    }

                    if (!directoryInfo.Is1080P && videoFileInfos.Any(video => video.Is1080P) && !videoFileInfos.Any(video => video.Is2160P))
                    {
                        log($"!1080: {movie}");
                    }

                    if (directoryInfo.Is720P && !videoFileInfos.Any(video => video.Is720P))
                    {
                        log($"!Not 720: {movie}");
                    }

                    if (!directoryInfo.Is720P && videoFileInfos.Any(video => video.Is720P) && !videoFileInfos.Any(video => video.Is1080P) && !videoFileInfos.Any(video => video.Is2160P))
                    {
                        log($"!720: {movie}");
                    }

                    if (videos.Length < 1)
                    {
                        log($"!No video: {movie}");
                    }
                    else if (videos.Length == 1 || videos.All(video => Regex.IsMatch(Path.GetFileNameWithoutExtension(video), @"\.cd[0-9]+$")))
                    {
                        string[] allowedAttachments = Attachments.Concat(AdaptiveAttachments).ToArray();
                        otherFiles
                            .Where(file => !allowedAttachments.ContainsIgnoreCase(file))
                            .ForEach(file => log($"!Attachment: {Path.Combine(movie, file)}"));
                    }
                    else
                    {
                        string[] allowedAttachments = videos
                            .SelectMany(video => AdaptiveAttachments.Select(attachment => $"{Path.GetFileNameWithoutExtension(video)}-{attachment}"))
                            .Concat(Attachments)
                            .ToArray();
                        otherFiles
                            .Where(file => !allowedAttachments.ContainsIgnoreCase(file))
                            .ForEach(file => log($"!Attachment: {Path.Combine(movie, file)}"));
                    }

                    string source = VideoDirectoryInfo.GetSource(videoFileInfos);
                    if (!source.EqualsOrdinal(directoryInfo.Source))
                    {
                        log($"!Source {directoryInfo.Source} should be {source}: {movie}");
                    }

                    subtitles
                        .Where(subtitle => !(AllSubtitleExtensions.ContainsIgnoreCase(Path.GetExtension(subtitle)) && videos.Any(video =>
                        {
                            string videoName = Path.GetFileNameWithoutExtension(video);
                            string subtitleName = Path.GetFileNameWithoutExtension(subtitle);
                            return subtitleName.EqualsOrdinal(videoName) || subtitle.StartsWithOrdinal($"{videoName}.") && allowedSubtitleLanguages.Any(allowedLanguage =>
                            {
                                string subtitleLanguage = subtitleName.Substring($"{videoName}.".Length);
                                return subtitleLanguage.Equals(allowedLanguage) || subtitleLanguage.StartsWithOrdinal($"{allowedLanguage}-");
                            });
                        })))
                        .ForEach(file => log($"!Subtitle: {Path.Combine(movie, file)}"));

                    string[] allowedMetadataFiles = videos
                        .Select(video => $"{Path.GetFileNameWithoutExtension(video)}{XmlMetadataExtension}")
                        .ToArray();
                    metadataFiles
                        .Where(metadata => !allowedMetadataFiles.ContainsIgnoreCase(metadata))
                        .ForEach(file => log($"!Metadata: {Path.Combine(movie, file)}"));

                    topFiles
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
                        log($"!Metadata missing: {movie}");
                    }

                    metadataFiles.ForEach(metadataFile =>
                    {
                        metadataFile = Path.Combine(movie, metadataFile);
                        XDocument metadata = XDocument.Load(Path.Combine(movie, metadataFile));
                        string? metadataImdbId = metadata.Root?.Element("imdbid")?.Value;
                        // string? metadataImdbRating = metadata.Root?.Element("rating")?.Value;
                        if (imdbMetadata is null)
                        {
                            if (metadataImdbId.IsNotNullOrWhiteSpace())
                            {
                                log($"!Metadata https://www.imdb.com/title/{metadataImdbId}/ should have no imdb id: {metadataFile}");
                            }

                            // if (metadataImdbRating.IsNotNullOrWhiteSpace())
                            // {
                            //    log($"!Metadata should have no rating: {metadataFile}");
                            // }
                        }
                        else
                        {
                            if (!imdbMetadata.ImdbId.EqualsOrdinal(metadataImdbId))
                            {
                                log($"!Metadata imdb id {metadataImdbId} should be {imdbMetadata.ImdbId}: {metadataFile}");
                            }
                        }
                    });
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

                    if (videoYear.IsNullOrWhiteSpace())
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

                    if (!movieYear.EqualsOrdinal(videoYear))
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
                    if (videoTitle.EqualsIgnoreCase(movieName[0].Split("=").Last().Replace(SubtitleSeparator, " ")))
                    {
                        return;
                    }

                    if (videoTitle.EqualsIgnoreCase(movieTitle.Split(SubtitleSeparator).Last()))
                    {
                        return;
                    }

                    movieTitle = movieTitle.Replace(SubtitleSeparator, " ");
                    if (!videoTitle.EqualsIgnoreCase(movieTitle))
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
                    string? json = files.SingleOrDefault(file => file.HasExtension(ImdbMetadataExtension));
                    if (json.IsNullOrWhiteSpace())
                    {
                        log($"!!! Missing IMDB metadata {movie}");
                        log(string.Empty);
                        return;
                    }

                    string imdbId = Path.GetFileNameWithoutExtension(json).Split(".")[0];
                    if (imdbId.EqualsOrdinal(NotExistingFlag))
                    {
                        log($"{NotExistingFlag} {movie}");
                        return;
                    }

                    string[] videos = files.Where(file => file.IsCommonVideo()).ToArray();
                    if (videos.All(video => VideoFileInfo.TryParse(video, out VideoFileInfo? parsed) && parsed.Definition.EqualsOrdinal(".2160p")))
                    {
                        return;
                    }

                    RarbgMetadata[] availableX265Metadata = x265Metadata.ContainsKey(imdbId)
                        ? x265Metadata[imdbId]
                            .Where(metadata => VideoFileInfo.IsXOrH(metadata.Title))
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
                                return !videoInfo.IsX
                                    || !videoName.StartsWithIgnoreCase(metadata.Title)
                                    && (!videoInfo.Origin.EqualsIgnoreCase(".BluRay") || !title.ContainsIgnoreCase(".WEBRip."))
                                    && (videoInfo.Edition.ContainsIgnoreCase("DUBBED") || !title.ContainsIgnoreCase(".DUBBED."))
                                    && (videoInfo.Edition.ContainsIgnoreCase("SUBBED") || !title.ContainsIgnoreCase(".SUBBED."))
                                    && (videoInfo.Edition.IsNullOrWhiteSpace() || !(videoInfo with { Edition = string.Empty }).Name.StartsWithIgnoreCase(title))
                                    && (!videoInfo.Edition.ContainsIgnoreCase(".Part") || !title.ContainsIgnoreCase(".Part"))
                                    && (!VideoFileInfo.TryParse(title, out VideoFileInfo? metadataInfo) || !videoInfo.Origin.EqualsIgnoreCase(metadataInfo.Origin) || !videoInfo.Edition.IsNotNullOrWhiteSpace()
                                        || !metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).All(edition => metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).ContainsIgnoreCase(edition)));
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

                    if (videos.Any(video => VideoFileInfo.TryParse(video, out VideoFileInfo? parsed) && parsed.IsX))
                    {
                        return;
                    }

                    RarbgMetadata[] availableH264Metadata = h264Metadata.ContainsKey(imdbId)
                        ? h264Metadata[imdbId]
                            .Where(metadata => VideoFileInfo.IsXOrH(metadata.Title))
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
                                return !videoInfo.IsH
                                    || !videoName.StartsWithIgnoreCase(metadata.Title)
                                    && (!videoInfo.Origin.EqualsIgnoreCase(".BluRay") || !title.ContainsIgnoreCase(".WEBRip."))
                                    && (videoInfo.Edition.ContainsIgnoreCase("DUBBED") || !title.ContainsIgnoreCase(".DUBBED."))
                                    && (videoInfo.Edition.ContainsIgnoreCase("SUBBED") || !title.ContainsIgnoreCase(".SUBBED."))
                                    && (videoInfo.Edition.IsNullOrWhiteSpace() || !(videoInfo with { Edition = string.Empty }).Name.StartsWithIgnoreCase(title))
                                    && (!videoInfo.Edition.ContainsIgnoreCase(".Part") || !title.ContainsIgnoreCase(".Part"))
                                    && (!VideoFileInfo.TryParse(title, out VideoFileInfo? metadataInfo) || !videoInfo.Origin.EqualsIgnoreCase(metadataInfo.Origin) || !videoInfo.Edition.IsNotNullOrWhiteSpace()
                                        || !metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).All(edition => metadataInfo.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries).ContainsIgnoreCase(edition)));
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

                    if (videos.Any(video => VideoFileInfo.TryParse(video, out VideoFileInfo? parsed) && parsed.IsH))
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
                            .Where(video => new VideoFileInfo(video).IsY)
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
                            if (videos.Any(video => video.Contains("1080p")) && otherYtsMetadata.All(metadataVersion => metadataVersion.version.Key.ContainsIgnoreCase("720p")))
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
                .DistinctIgnoreCase()
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
                    .EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
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
                    && summaries.Value.Any(summary => summary.ImdbRating.CompareOrdinal(threshold) >= 0)
                    && (excludedGenres is null || !summaries.Value.Any(summary => excludedGenres.Any(excludedGenre => summary.Genres.ContainsIgnoreCase(excludedGenre)))))
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
                    && summaries.Value.Any(summary => summary.ImdbRating.CompareOrdinal(threshold) >= 0)
                    && (excludedGenres is null || !summaries.Value.Any(summary => excludedGenres.Any(excludedGenre => summary.Genres.ContainsIgnoreCase(excludedGenre)))))
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
                    && summaries.Value.Any(summary => summary.ImdbRating.CompareOrdinal(threshold) >= 0)
                    && (excludedGenres is null || !summaries.Value.Any(summary => excludedGenres.Any(excludedGenre => summary.Genres.ContainsIgnoreCase(excludedGenre)))))
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
                            .Where(pair => pair.Key.ContainsIgnoreCase("World-wide (English title)"))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (!releaseTitles.Any()
                        && !imdbMetadata.Titles.TryGetValue("USA", out releaseTitles)
                        && !imdbMetadata.Titles.TryGetValue("USA (working title)", out releaseTitles)
                        && !imdbMetadata.Titles.TryGetValue("USA (informal English title)", out releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.ContainsIgnoreCase("USA"))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (!releaseTitles.Any()
                        && !imdbMetadata.Titles.TryGetValue("UK", out releaseTitles)
                        && !imdbMetadata.Titles.TryGetValue("UK (informal English title)", out releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.ContainsIgnoreCase("UK"))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (!releaseTitles.Any()
                        && !imdbMetadata.Titles.TryGetValue("Hong Kong (English title)", out releaseTitles))
                    {
                        releaseTitles = imdbMetadata.Titles
                            .Where(pair => pair.Key.ContainsIgnoreCase("Hong Kong (English title)"))
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
                            imdbTitle.ReplaceIgnoreCase(" Vol ", SubtitleSeparator),
                            imdbTitle.ReplaceIgnoreCase(" Chapter ", SubtitleSeparator),
                            imdbTitle.Replace(SubtitleSeparator, " "),
                            imdbTitle.ReplaceIgnoreCase("zero", "0"),
                            imdbTitle.ReplaceIgnoreCase("one", "1"),
                            imdbTitle.ReplaceIgnoreCase("two", "2"),
                            imdbTitle.ReplaceIgnoreCase("three", "3"),
                            imdbTitle.ReplaceIgnoreCase("four", "4"),
                            imdbTitle.ReplaceIgnoreCase("five", "5"),
                            imdbTitle.ReplaceIgnoreCase(" IV", " 4"),
                            imdbTitle.ReplaceIgnoreCase(" III", " 3"),
                            imdbTitle.ReplaceIgnoreCase(" II", " 2"),
                            imdbTitle.ReplaceIgnoreCase(" Part II", " 2"),
                            imdbTitle.ReplaceIgnoreCase(" -Part ", " "),
                            imdbTitle.ReplaceIgnoreCase("-Part ", " "),
                            imdbTitle.ReplaceIgnoreCase(" - Part ", " "),
                            imdbTitle.ReplaceIgnoreCase("- Part ", " "),
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
                    if (parsed.DefaultTitle2.IsNotNullOrWhiteSpace())
                    {
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..]}");
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..].Replace(InstallmentSeparator, " ")}");
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()}");
                        localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant().Replace(InstallmentSeparator, " ")}");
                    }
                    if (imdbTitles.Any(a => localTitles.Any(a.EqualsOrdinal)))
                    {
                        return;
                    }

                    parsed = parsed with { DefaultTitle1 = imdbTitle };
                    string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), parsed.ToString());
                    if (movie.EqualsOrdinal(newMovie))
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
                            imdbTitle.ReplaceIgnoreCase(" Vol ", SubtitleSeparator),
                            imdbTitle.ReplaceIgnoreCase(" Chapter ", SubtitleSeparator),
                            imdbTitle.Replace(SubtitleSeparator, " "),
                            imdbTitle.ReplaceIgnoreCase("zero", "0"),
                            imdbTitle.ReplaceIgnoreCase("one", "1"),
                            imdbTitle.ReplaceIgnoreCase("two", "2"),
                            imdbTitle.ReplaceIgnoreCase("three", "3"),
                            imdbTitle.ReplaceIgnoreCase("four", "4"),
                            imdbTitle.ReplaceIgnoreCase("five", "5"),
                            imdbTitle.ReplaceIgnoreCase(" IV", " 4"),
                            imdbTitle.ReplaceIgnoreCase(" III", " 3"),
                            imdbTitle.ReplaceIgnoreCase(" II", " 2"),
                            imdbTitle.ReplaceIgnoreCase(" Part II", " 2"),
                            imdbTitle.ReplaceIgnoreCase(" -Part ", " "),
                            imdbTitle.ReplaceIgnoreCase("-Part ", " "),
                            imdbTitle.ReplaceIgnoreCase(" - Part ", " "),
                            imdbTitle.ReplaceIgnoreCase("- Part ", " "),
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
                    if (parsed.OriginalTitle2.IsNotNullOrWhiteSpace())
                    {
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..]}");
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()[0]}{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray())[1..].Replace(InstallmentSeparator, " ")}");
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant()}");
                        localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).ToLowerInvariant().Replace(InstallmentSeparator, " ")}");
                    }
                    if (imdbTitles.Any(a => localTitles.Any(a.EqualsOrdinal)))
                    {
                        return;
                    }

                    parsed = parsed with { OriginalTitle1 = $"={imdbTitle}" };
                    string newMovie = Path.Combine(Path.GetDirectoryName(movie) ?? throw new InvalidOperationException(movie), parsed.ToString());
                    if (movie.EqualsOrdinal(newMovie))
                    {
                        return;
                    }

                    log(movie);
                    log(newMovie);
                    log(string.Empty);
                });
        }

        internal static void PrintMovieByGenre(string directory, string genre, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory
                .GetFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
                .Select(file => (File: file, Metadata: Imdb.TryLoad(file, out ImdbMetadata? imdbMetadata) ? imdbMetadata : null))
                .Where(metadata => metadata.Metadata?.Genre.ContainsIgnoreCase(genre) is true)
                .ForEach(metadata => log($"{Path.GetDirectoryName(metadata.File)}"));
        }
    }
}
