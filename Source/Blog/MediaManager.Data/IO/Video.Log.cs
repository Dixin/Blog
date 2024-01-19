namespace MediaManager.IO;

using System.Linq;
using System.Web;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using MediaManager.Net;
using OpenQA.Selenium;

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
        log ??= Logger.WriteLine;
        files
            .ToArray()
            .AsParallel()
            .Select((video, index) =>
            {
                if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata, log: message => log($"{index} {message}")))
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

    internal static void PrintDirectoriesWithLowDefinition(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .Where(movie => !VideoDirectoryInfo.GetVideos(movie).Any(video => video.IsHD()))
            .ForEach(log);
    }

    internal static void PrintDirectoriesWithMultipleMedia(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetVideos(movie).ToArray();
                if (videos.Length <= 1 || videos.All(video => video.Part.IsNotNullOrWhiteSpace()))
                {
                    return;
                }

                log(movie);
            });
    }

    internal static void PrintVideosP(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.P, log);

    internal static void PrintVideosY(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.Y, log);

    internal static void PrintVideosNotX(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.X, log);

    internal static void PrintVideosH(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.H, log);

    private static void PrintVideos(string directory, int level, Func<string, bool> predicate, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .OrderBy(movie => movie)
            .ForEach(movie =>
            {
                VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetVideos(movie).ToArray();
                if (videos.Any())
                {
                    log(movie);
                    Imdb.TryRead(movie, out string? imdbId, out _, out _, out _, out _);
                    log(imdbId ?? string.Empty);
                    videos.ForEach(video => log(video.Name));
                    log(string.Empty);
                }
            });
    }

    internal static void PrintMoviesWithoutSubtitle(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null, params string[] languages)
    {
        log ??= Logger.WriteLine;
        Func<string, bool> noSubtitle;
        if (languages.IsEmpty())
        {
            noSubtitle = movie => !Directory.GetFiles(movie).Any(IsSubtitle);
        }
        else
        {
            List<Regex> searchPatterns = languages
                .SelectMany(language => AllSubtitleExtensions.
                    Select(extension => new Regex($@"\.{language}{extension}$", RegexOptions.IgnoreCase)))
                .ToList();
            if (languages.ContainsIgnoreCase("eng"))
            {
                searchPatterns.AddRange(AllSubtitleExtensions.Select(extension => new Regex($"{extension}$", RegexOptions.IgnoreCase)));
            }

            noSubtitle = movie => !Directory.GetFiles(movie).Any(file => searchPatterns.Any(searchPattern => searchPattern.IsMatch(file)));
        }

        EnumerateDirectories(directory, level)
            .Where(noSubtitle)
            .ForEach(movie => log($"{(Imdb.TryRead(movie, out string? imdbId, out _, out _, out _, out _) ? imdbId : "-")} {movie}"));
    }

    internal static void PrintMetadataByGroup(string directory, int level = DefaultDirectoryLevel, string field = "genre", Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .Select(movie => (movie, metadata: XDocument.Load(Directory.GetFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly).First())))
            .Select(movie => (movie.movie, field: movie.metadata.Root?.Element(field)?.Value))
            .OrderBy(movie => movie.field)
            .ForEach(movie => log($"{movie.field}: {PathHelper.GetFileName(movie.movie)}"));
    }

    internal static void PrintMetadataByDuplication(string directory, string field = "imdbid", Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Directory.GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .Select(metadata => (metadata, field: XDocument.Load(metadata).Root?.Element(field)?.Value))
            .GroupBy(movie => movie.field)
            .Where(group => group.Count() > 1)
            .ForEach(group => group.ForEach(movie => log($"{movie.field} - {movie.metadata}")));
    }

    internal static void PrintYears(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                XDocument metadata = XDocument.Load(Directory.GetFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly).First());
                string movieDirectory = PathHelper.GetFileName(movie);
                if (movieDirectory.StartsWithOrdinal("0."))
                {
                    movieDirectory = movieDirectory.Substring("0.".Length);
                }

                VideoDirectoryInfo videoDirectoryInfo = VideoDirectoryInfo.Parse(movieDirectory);
                string directoryYear = videoDirectoryInfo.Year;
                string metadataYear = metadata.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{metadata} has no year.");
                string videoName = string.Empty;
                if (!(directoryYear.EqualsOrdinal(metadataYear)
                        && Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                            .Where(file => AllVideoExtensions.Any(file.EndsWithIgnoreCase))
                            .All(video => (videoName = PathHelper.GetFileName(video)).ContainsOrdinal(directoryYear))))
                {
                    log($"Directory: {directoryYear}, Metadata {metadataYear}, Video: {videoName}, {movie}");
                }
            });
    }

    internal static void PrintDirectoriesWithNonLatinOriginalTitle(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .Where(movie => movie.ContainsOrdinal("="))
            .Where(movie => !Regex.IsMatch(movie.Split("=")[1], "^[a-z]{1}.", RegexOptions.IgnoreCase))
            .ForEach(log);
    }

    internal static void PrintDuplicateImdbId(Action<string>? log = null, params string[] directories)
    {
        log ??= Logger.WriteLine;
        directories.SelectMany(directory => Directory.EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .GroupBy(metadata => PathHelper.GetFileNameWithoutExtension(metadata).Split("-")[0])
            .Where(group => group.Count() > 1)
            .ForEach(group => group.OrderBy(metadata => metadata).Append(string.Empty).ForEach(log));
    }

    private static readonly string[] TitlesWithNoTranslation = ["Beyond", "IMAX", "Paul & Wing", "Paul & Steve", "Paul", "Wing", "Steve", "GEM", "Metro", "TVB"];

    private static readonly char[] DirectorySpecialCharacters = "：@#(){}".ToCharArray();

    internal static void PrintDirectoriesWithErrors(ISettings settings, string directory, int level = DefaultDirectoryLevel, bool isLoadingVideo = false, bool isNoAudioAllowed = false, bool isTV = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        List<string>? allVideos = null;
        if (isLoadingVideo)
        {
            allVideos = [];
        }

        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string trimmedMovie = PathHelper.GetFileName(movie);
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
                string[] translations = [directoryInfo.TranslatedTitle1, directoryInfo.TranslatedTitle2, directoryInfo.TranslatedTitle3];
                string[] titles =
                [
                    directoryInfo.DefaultTitle1,
                    directoryInfo.DefaultTitle2.TrimStart('-'),
                    directoryInfo.DefaultTitle3.TrimStart('-'),
                    directoryInfo.OriginalTitle1.TrimStart('='),
                    directoryInfo.OriginalTitle2.TrimStart('-'),
                    directoryInfo.OriginalTitle3.TrimStart('-'),
                    directoryInfo.TranslatedTitle1,
                    directoryInfo.TranslatedTitle2.TrimStart('-'),
                    directoryInfo.TranslatedTitle3.TrimStart('-')
                ];

                if (Regex.IsMatch(trimmedMovie, @"·[0-9]"))
                {
                    log($"!Special character ·: {trimmedMovie}");
                }

                DirectorySpecialCharacters.Where(trimmedMovie.Contains).ForEach(specialCharacter => log($"!Special character {specialCharacter}: {trimmedMovie}"));
                translations
                    .Where(translated => Regex.IsMatch(translated.Split("`").First(), "[0-9]+"))
                    .ForEach(translated => log($"!Translation has number {translated}: {movie}"));
                translations
                    .Where(translation => !string.IsNullOrEmpty(translation) && translation.All(character => character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' || char.IsPunctuation(character) || char.IsSeparator(character) || char.IsSymbol(character) || char.IsWhiteSpace(character)))
                    .Where(title => !TitlesWithNoTranslation.ContainsIgnoreCase(title))
                    .ForEach(translated => log($"!Title not translated {translated}: {movie}"));

                titles
                    .Where(title => title.IsNotNullOrWhiteSpace() && (char.IsWhiteSpace(title.First()) || char.IsWhiteSpace(title.Last())))
                    .ForEach(title => log($"!Title has white space {title}: {movie}"));

                allPaths.Where(path => path.Length > 256).ForEach(path => log($"!Path too long: {path}"));

                string[] topFiles = Directory
                    .GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Select(PathHelper.GetFileName)
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

                string[] topDirectories = Directory.GetDirectories(movie).Select(PathHelper.GetFileName).ToArray();

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

                    string[] seasonPaths = Directory.GetDirectories(movie).Where(path => Regex.IsMatch(PathHelper.GetFileName(path), @"^Season [0-9]{2,4}(\..+)?")).ToArray();
                    seasonPaths.ForEach(season =>
                    {
                        Directory.EnumerateDirectories(season).ForEach(seasonDirectory => log($"!Season directory: {seasonDirectory}"));

                        string[] seasonFiles = Directory.GetFiles(season, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).Select(PathHelper.GetFileName).ToArray();
                        string[] videos = seasonFiles.Where(IsCommonVideo).ToArray();
                        string[] videoMetadataFiles = videos.Select(video => $"{PathHelper.GetFileNameWithoutExtension(video)}{XmlMetadataExtension}").ToArray();
                        string[] videoThumbFiles = videos.Select(video => $"{PathHelper.GetFileNameWithoutExtension(video)}-thumb{ThumbExtension}").ToArray();
                        VideoEpisodeFileInfo?[] allVideoFileInfos = videos
                            .Select(video => VideoEpisodeFileInfo.TryParse(video, out VideoEpisodeFileInfo? videoFileInfo)
                                ? videoFileInfo
                                : throw new InvalidOperationException(video))
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
                            .Select(PathHelper.GetFileNameWithoutExtension)
                            .Concat(videos.SelectMany(video => allowedSubtitleLanguages, (video, language) => $"{PathHelper.GetFileNameWithoutExtension(video)}.{language}"))
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

                string[] videos = topFiles.Where(IsVideo).ToArray();
                VideoMovieFileInfo[] videoFileInfos = videos
                    .Select(video =>
                    {
                        if (VideoMovieFileInfo.TryParse(video, out VideoMovieFileInfo? videoFileInfo))
                        {
                            return videoFileInfo;
                        }

                        throw new InvalidOperationException(video);
                    })
                    .ToArray();
                string[] subtitles = topFiles.Where(file => file.HasAnyExtension(AllSubtitleExtensions)).ToArray();
                string[] metadataFiles = topFiles.Where(file => file.HasExtension(XmlMetadataExtension)).ToArray();
                string[] otherFiles = topFiles.Except(videos).Except(subtitles).Except(metadataFiles).Except(imdbFiles).Except(cacheFiles).ToArray();

                if (directoryInfo.Is2160P && !videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P2160))
                {
                    log($"!Not 2160: {movie}");
                }

                if (!directoryInfo.Is2160P && videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P2160))
                {
                    log($"!2160: {movie}");
                }

                if (directoryInfo.Is1080P && !videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P1080))
                {
                    log($"!Not 1080: {movie}");
                }

                if (!directoryInfo.Is1080P && videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P1080) && !videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P2160))
                {
                    log($"!1080: {movie}");
                }

                if (directoryInfo.Is720P && !videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P720))
                {
                    log($"!Not 720: {movie}");
                }

                if (!directoryInfo.Is720P && videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P720) && !videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P1080) && !videoFileInfos.Any(video => video.GetDefinitionType() is DefinitionType.P2160))
                {
                    log($"!720: {movie}");
                }

                if (videos.Length < 1)
                {
                    log($"!No video: {movie}");
                }
                else if (videos.Length == 1 || videos.All(video => Regex.IsMatch(PathHelper.GetFileNameWithoutExtension(video), @"\.cd[0-9]+$")))
                {
                    string[] allowedAttachments = Attachments.Concat(AdaptiveAttachments).ToArray();
                    otherFiles
                        .Where(file => !allowedAttachments.ContainsIgnoreCase(file))
                        .ForEach(file => log($"!Attachment: {Path.Combine(movie, file)}"));
                }
                else
                {
                    string[] allowedAttachments = videos
                        .SelectMany(video => AdaptiveAttachments.Select(attachment => $"{PathHelper.GetFileNameWithoutExtension(video)}-{attachment}"))
                        .Concat(Attachments)
                        .ToArray();
                    otherFiles
                        .Where(file => !allowedAttachments.ContainsIgnoreCase(file))
                        .ForEach(file => log($"!Attachment: {Path.Combine(movie, file)}"));
                }

                string source = VideoDirectoryInfo.GetSource(videoFileInfos, settings);
                if (!source.EqualsOrdinal(directoryInfo.Source))
                {
                    log($"!Source {directoryInfo.Source} should be {source}: {movie}");
                }

                subtitles
                    .Where(subtitle => !(AllSubtitleExtensions.ContainsIgnoreCase(PathHelper.GetExtension(subtitle)) && videos.Any(video =>
                    {
                        string videoName = PathHelper.GetFileNameWithoutExtension(video);
                        string subtitleName = PathHelper.GetFileNameWithoutExtension(subtitle);
                        return subtitleName.EqualsOrdinal(videoName) || subtitle.StartsWithOrdinal($"{videoName}.") && allowedSubtitleLanguages.Any(allowedLanguage =>
                        {
                            string subtitleLanguage = subtitleName.Substring($"{videoName}.".Length);
                            return subtitleLanguage.Equals(allowedLanguage) || subtitleLanguage.StartsWithOrdinal($"{allowedLanguage}-");
                        });
                    })))
                    .ForEach(file => log($"!Subtitle: {Path.Combine(movie, file)}"));

                string[] allowedMetadataFiles = videos
                    .Select(video => $"{PathHelper.GetFileNameWithoutExtension(video)}{XmlMetadataExtension}")
                    .ToArray();
                metadataFiles
                    .Where(metadata => !allowedMetadataFiles.ContainsIgnoreCase(metadata))
                    .ForEach(file => log($"!Metadata: {Path.Combine(movie, file)}"));

                topFiles
                    .Except(imdbFiles)
                    .Except(cacheFiles)
                    .Where(file => Regex.IsMatch(file, @"\.(1080[^p]|720[^p])"))
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

    internal static void PrintTitlesWithDifferences(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string[] movieName = PathHelper.GetFileName(movie).Split(".");
                string movieTitle = movieName[0].Split("=")[0];
                string movieYear = movieName[1];
                string? videoYear = null;
                string? videoTitle = null;
                if (Imdb.TryLoad(movie, out ImdbMetadata? jsonMetadata))
                {
                    videoYear = jsonMetadata.Year;
                    videoTitle = jsonMetadata.Title;
                    Debug.Assert(videoYear.Length == 4);
                }

                if (videoYear.IsNullOrWhiteSpace())
                {
                    (videoTitle, videoYear) = Directory
                        .GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly)
                        .Select(metadata =>
                        {
                            XElement root = XDocument.Load(metadata).Root ?? throw new InvalidOperationException(metadata);
                            return (Title: root.Element("title")?.Value, Year: root.Element("year")?.Value);
                        })
                        .Distinct()
                        .Single();
                }

                if (!movieYear.EqualsOrdinal(videoYear))
                {
                    log(movie);
                    movieName[1] = videoYear!;
                    string newMovie = Path.Combine(PathHelper.GetDirectoryName(movie), string.Join(".", movieName));
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
                    log(videoYear ?? string.Empty);
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
                    log(videoTitle ?? string.Empty);
                    log(Environment.NewLine);
                }
            });
    }

    internal static async Task PrintMovieVersions(ISettings settings, Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopX265Metadata);
        Dictionary<string, TopMetadata[]> h264Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264Metadata);
        Dictionary<string, TopMetadata[]> h264720PMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264720PMetadata);
        Dictionary<string, PreferredMetadata[]> preferredMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(settings.MoviePreferredMetadata);
        HashSet<string> ignore = new(await JsonHelper.DeserializeFromFileAsync<string[]>(settings.MovieIgnoreMetadata), StringComparer.OrdinalIgnoreCase);

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

                string imdbId = PathHelper.GetFileNameWithoutExtension(json);
                if (imdbId.EqualsOrdinal(NotExistingFlag))
                {
                    log($"{NotExistingFlag} {movie}");
                    return;
                }

                imdbId = imdbId.Split(SubtitleSeparator)[0];
                if (!Regex.IsMatch(imdbId, "tt[0-9]+"))
                {
                    log($"Invalid IMDB id {imdbId}: {movie}");
                    return;
                }

                (string File, string Name, VideoMovieFileInfo Info)[] videos = files
                    .Where(file => file.IsVideo())
                    .Select(file => (file, PathHelper.GetFileNameWithoutExtension(file), VideoMovieFileInfo.Parse(file)))
                    .ToArray();
                (string File, string Name, VideoMovieFileInfo Info)[] preferredVideos = videos
                    .Where(video => video.Info.GetEncoderType() is EncoderType.Y or EncoderType.XY)
                    .ToArray();

                PreferredMetadata[] availablePreferredMetadata = preferredMetadata
                    .TryGetValue(imdbId, out PreferredMetadata[]? preferredResult)
                    ? preferredResult
                        .Where(metadata => !ignore.Contains(metadata.Link))
                        .ToArray()
                    : [];
                (PreferredMetadata Metadata, KeyValuePair<string, string> Version)[] otherPreferredMetadata = availablePreferredMetadata
                    .SelectMany(metadata => metadata.Availabilities, (metadata, version) => (metadata, version))
                    .Where(metadataVersion => !metadataVersion.version.Key.Contains("2160p"))
                    .ToArray();
                (PreferredMetadata Metadata, KeyValuePair<string, string> Version)[] bluRayPreferredMetadata = otherPreferredMetadata
                    .Where(metadataVersion => metadataVersion.Version.Key.ContainsIgnoreCase("BluRay"))
                    .ToArray();

                (PreferredMetadata Metadata, KeyValuePair<string, string> Version)[] hdPreferredMetadata = otherPreferredMetadata
                    .Where(metadataVersion => metadataVersion.Version.Key.ContainsIgnoreCase("1080p"))
                    .ToArray();
                if (hdPreferredMetadata.Any())
                {
                    otherPreferredMetadata = hdPreferredMetadata;
                }

                (PreferredMetadata Metadata, KeyValuePair<string, string> Version)[] blueRayHDPreferredMetadata = otherPreferredMetadata
                    .Where(metadataVersion => metadataVersion.Version.Key.ContainsIgnoreCase("BluRay"))
                    .ToArray();
                if (blueRayHDPreferredMetadata.Any())
                {
                    otherPreferredMetadata = blueRayHDPreferredMetadata;
                }

                if (preferredVideos.Any())
                {
                    otherPreferredMetadata = otherPreferredMetadata
                        .Where(metadataVersion => preferredVideos.Any(preferredVideo =>
                            metadataVersion.Version.Key.Split(".").All(keyword => !preferredVideo.Name.ContainsIgnoreCase(keyword))))
                        .ToArray();
                }

                if (videos.All(video => video.Info.GetDefinitionType() is DefinitionType.P2160))
                {
                    return;
                }

                if (x265Metadata.TryGetValue(imdbId, out TopMetadata[]? availableX265Metadata))
                {
                    availableX265Metadata = availableX265Metadata
                        .Where(metadata =>
                        {
                            if (VideoMovieFileInfo.TryParse(metadata.Title, out VideoMovieFileInfo? video))
                            {
                                return video.GetEncoderType() is EncoderType.X or EncoderType.H;
                            }

                            log($"!Fail to parse metadata {metadata.Title} {metadata.Link}");
                            return false;
                        })
                        .ToArray();
                }
                else
                {
                    availableX265Metadata = [];
                }

                if (availableX265Metadata.Any())
                {
                    TopMetadata[] otherX265Metadata = availableX265Metadata
                        .Where(metadata => !ignore.Contains(metadata.Title))
                        .Where(metadata => !videos.Any(video =>
                        {
                            string metadataTitle = metadata.Title;
                            string[] videoEditions = video.Info.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries);
                            // (Not any) Local is equal to or better than remote metadata.
                            return video.Info.GetEncoderType() is EncoderType.X
                                && (video.Name.StartsWithIgnoreCase(metadata.Title)
                                    || video.Info.Origin.ContainsIgnoreCase(".BluRay") && metadataTitle.ContainsIgnoreCase(".WEBRip.")
                                    || !video.Info.Edition.ContainsIgnoreCase("PREVIEW") && metadataTitle.ContainsIgnoreCase(".PREVIEW.")
                                    || !video.Info.Edition.ContainsIgnoreCase("DUBBED") && metadataTitle.ContainsIgnoreCase(".DUBBED.")
                                    || !video.Info.Edition.ContainsIgnoreCase("SUBBED") && (metadataTitle.ContainsIgnoreCase(".SUBBED.") || metadataTitle.ContainsIgnoreCase(".ENSUBBED."))
                                    || video.Info.Edition.ContainsIgnoreCase("DC") && metadataTitle.ContainsIgnoreCase(".THEATRICAL.")
                                    || video.Info.Edition.ContainsIgnoreCase("EXTENDED") && !metadataTitle.ContainsIgnoreCase(".EXTENDED.")
                                    || video.Info.Edition.IsNotNullOrWhiteSpace() && (video.Info with { Edition = string.Empty }).Name.StartsWithIgnoreCase(metadataTitle)
                                    || video.Info.Edition.ContainsIgnoreCase(".Part") && metadataTitle.ContainsIgnoreCase(".Part")
                                    || VideoMovieFileInfo.TryParse(metadataTitle, out VideoMovieFileInfo? metadataInfo)
                                    && video.Info.Origin.EqualsIgnoreCase(metadataInfo.Origin)
                                    && video.Info.Edition.IsNotNullOrWhiteSpace()
                                    && metadataInfo
                                        .Edition
                                        .Split(".", StringSplitOptions.RemoveEmptyEntries)
                                        .All(metadataEdition => videoEditions.ContainsIgnoreCase(metadataEdition)));
                        }))
                        .ToArray();
                    if (otherX265Metadata.Any())
                    {
                        if (otherX265Metadata.All(metadata => metadata.Title.Contains(".WEBRip."))
                            && preferredVideos.Any(video => video.Info.GetDefinitionType() == DefinitionType.P1080 && video.Info.Origin.ContainsIgnoreCase("BluRay")))
                        {

                        }
                        else
                        {
                            log(movie);
                            videos.ForEach(video => log(video.Name));
                            otherX265Metadata.ForEach(metadata =>
                            {
                                log($"x265 {metadata.Link}");
                                log(metadata.Title);
                                log(string.Empty);
                            });
                        }
                    }

                    return;
                }

                if (videos.Any(video => video.Info.GetEncoderType() is EncoderType.X))
                {
                    if (videos.All(video => !video.Info.Origin.ContainsIgnoreCase("BluRay")) && bluRayPreferredMetadata.Any())
                    {
                        log(movie);
                        videos.ForEach(video => log(video.Name));
                        bluRayPreferredMetadata.ForEach(metadataVersion =>
                        {
                            log($"Preferred {metadataVersion.Metadata.Link}");
                            log($"{metadataVersion.Version.Key} {metadataVersion.Version.Value}");
                            log(string.Empty);
                        });
                    }
                    return;
                }

                TopMetadata[] availableH264Metadata = h264Metadata.ContainsKey(imdbId)
                    ? h264Metadata[imdbId]
                        .Where(metadata =>
                        {
                            if (VideoMovieFileInfo.TryParse(metadata.Title, out VideoMovieFileInfo? video))
                            {
                                return video.GetEncoderType() is EncoderType.X or EncoderType.H;
                            }

                            log($"!Fail to parse metadata {metadata.Title} {metadata.Link}");
                            return false;
                        })
                        .ToArray()
                    : [];
                if (availableH264Metadata.Any())
                {
                    TopMetadata[] otherH264Metadata = availableH264Metadata
                        .Where(metadata => !ignore.Contains(metadata.Title))
                        .Where(metadata => !videos.Any(video =>
                        {
                            string metadataTitle = metadata.Title;
                            string[] videoEditions = video.Info.Edition.Split(".", StringSplitOptions.RemoveEmptyEntries);
                            // (Not any) Local is equal to or better than remote metadata.
                            return video.Info.GetEncoderType() is EncoderType.H
                                && (video.Name.StartsWithIgnoreCase(metadata.Title)
                                    || video.Info.Origin.ContainsIgnoreCase(".BluRay") && metadataTitle.ContainsIgnoreCase(".WEBRip.")
                                    || !video.Info.Edition.ContainsIgnoreCase("PREVIEW") && metadataTitle.ContainsIgnoreCase(".PREVIEW.")
                                    || !video.Info.Edition.ContainsIgnoreCase("DUBBED") && metadataTitle.ContainsIgnoreCase(".DUBBED.")
                                    || !video.Info.Edition.ContainsIgnoreCase("SUBBED") && (metadataTitle.ContainsIgnoreCase(".SUBBED.") || metadataTitle.ContainsIgnoreCase(".ENSUBBED."))
                                    || video.Info.Edition.ContainsIgnoreCase("DC") && metadataTitle.ContainsIgnoreCase(".THEATRICAL.")
                                    || video.Info.Edition.ContainsIgnoreCase("EXTENDED") && !metadataTitle.ContainsIgnoreCase(".EXTENDED.")
                                    || video.Info.Edition.IsNotNullOrWhiteSpace() && (video.Info with { Edition = string.Empty }).Name.StartsWithIgnoreCase(metadataTitle)
                                    || video.Info.Edition.ContainsIgnoreCase(".Part") && metadataTitle.ContainsIgnoreCase(".Part")
                                    || VideoMovieFileInfo.TryParse(metadataTitle, out VideoMovieFileInfo? metadataInfo)
                                    && video.Info.Origin.EqualsIgnoreCase(metadataInfo.Origin)
                                    && video.Info.Edition.IsNotNullOrWhiteSpace()
                                    && metadataInfo
                                        .Edition
                                        .Split(".", StringSplitOptions.RemoveEmptyEntries)
                                        .All(metadataEdition => videoEditions.ContainsIgnoreCase(metadataEdition)));
                        }))
                        .ToArray();
                    if (otherH264Metadata.Any())
                    {
                        if (otherH264Metadata.All(metadata => metadata.Title.Contains(".WEBRip."))
                            && preferredVideos.Any(video => video.Info.GetDefinitionType() == DefinitionType.P1080 && video.Info.Origin.ContainsIgnoreCase("BluRay")))
                        {

                        }
                        else
                        {
                            log(movie);
                            videos.ForEach(video => log(video.Name));
                            otherH264Metadata.ForEach(metadata =>
                            {
                                log($"H264 {metadata.Link}");
                                log(metadata.Title);
                                log(string.Empty);
                            });
                        }
                    }

                    return;
                }

                if (videos.Any(video => video.Info.GetEncoderType() is EncoderType.H))
                {
                    if (videos.All(video => !video.Info.Origin.ContainsIgnoreCase("BluRay")) && bluRayPreferredMetadata.Any())
                    {
                        log(movie);
                        videos.ForEach(video => log(video.Name));
                        bluRayPreferredMetadata.ForEach(metadataVersion =>
                        {
                            log($"Preferred {metadataVersion.Metadata.Link}");
                            log($"{metadataVersion.Version.Key} {metadataVersion.Version.Value}");
                            log(string.Empty);
                        });
                    }

                    return;
                }

                if (availablePreferredMetadata.Any())
                {
                    Debug.Assert(preferredVideos.Length <= 1 || imdbId is "tt2208216");

                    if (otherPreferredMetadata.Any())
                    {
                        log(movie);
                        videos.ForEach(video => log(video.Name));
                        otherPreferredMetadata.ForEach(metadataVersion =>
                        {
                            log($"Preferred {metadataVersion.Metadata.Link}");
                            log($"{metadataVersion.Version.Key} {metadataVersion.Version.Value}");
                            log(string.Empty);
                        });
                    }

                    return;
                }

                TopMetadata[] availableOtherMetadata = [];
                if (videos.Any(video => video.Info.GetDefinitionType() is DefinitionType.P1080))
                {
                    return;
                }

                availableOtherMetadata = x265Metadata.ContainsKey(imdbId)
                    ? x265Metadata[imdbId]
                    : availableOtherMetadata;
                availableOtherMetadata = h264Metadata.ContainsKey(imdbId)
                    ? availableOtherMetadata.Concat(h264Metadata[imdbId]).ToArray()
                    : availableOtherMetadata;
                if (availableOtherMetadata.Any())
                {
                    log(movie);
                    videos.ForEach(video => log(video.Name));
                    availableOtherMetadata.ForEach(metadata =>
                    {
                        log($"HD {metadata.Link}");
                        log(metadata.Title);
                        log(string.Empty);
                    });

                    return;
                }

                if (videos.Any(video => video.Info.GetDefinitionType() is DefinitionType.P720))
                {
                    return;
                }

                availableOtherMetadata = h264720PMetadata.ContainsKey(imdbId)
                    ? availableOtherMetadata.Concat(h264720PMetadata[imdbId]).ToArray()
                    : availableOtherMetadata;
                if (availableOtherMetadata.Any())
                {
                    log(movie);
                    videos.ForEach(video => log(video.Name));
                    availableOtherMetadata.ForEach(metadata =>
                    {
                        log($"HD {metadata.Link}");
                        log(metadata.Title);
                        log(string.Empty);
                    });
                }
            });
    }

    internal static async Task PrintTVVersions(ISettings settings, Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.TVTopX265Metadata);

        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .ForEach(tv =>
            {
                string[] topFiles = Directory.GetFiles(tv);
                string? json = topFiles.SingleOrDefault(file => file.HasExtension(ImdbMetadataExtension));
                if (json.IsNullOrWhiteSpace())
                {
                    log($"!!! Missing IMDB metadata {tv}");
                    log(string.Empty);
                    return;
                }

                string imdbId = PathHelper.GetFileNameWithoutExtension(json);
                if (imdbId.EqualsOrdinal(NotExistingFlag))
                {
                    log($"{NotExistingFlag} {tv}");
                    return;
                }

                imdbId = imdbId.Split(SubtitleSeparator)[0];
                if (!Regex.IsMatch(imdbId, "tt[0-9]+"))
                {
                    log($"Invalid IMDB id {imdbId}: {tv}");
                    return;
                }

                if (!x265Metadata.TryGetValue(imdbId, out TopMetadata[]? metadata))
                {
                    return;
                }

                string[] seasons = Directory.EnumerateDirectories(tv, "Season *").ToArray();

                metadata.ForEach(seasonMetadata =>
                {
                    string seasonNumber = Regex.Match(seasonMetadata.Title, @"\.S([0-9]{2})\.").Groups[1].Value;
                    string seasonDirectory = seasons.SingleOrDefault(season => PathHelper.GetFileName(season).StartsWithIgnoreCase($"Season {seasonNumber}"), string.Empty);
                    if (seasonDirectory.IsNotNullOrWhiteSpace()
                        && Directory
                            .EnumerateFiles(seasonDirectory)
                            .Where(IsCommonVideo)
                            .All(episode => VideoEpisodeFileInfo.TryParse(episode, out VideoEpisodeFileInfo? parsed) && parsed.GetEncoderType() is EncoderType.X && !(parsed.Origin.ContainsIgnoreCase("WEBRip") && seasonMetadata.Title.ContainsIgnoreCase("BluRay"))))
                    {
                        return;
                    }

                    log(tv);
                    log($"x265 {seasonMetadata.Link}");
                    log(seasonMetadata.Title);
                    log(string.Empty);
                });
            });
    }

    internal static async Task PrintSpecialTitles(ISettings settings, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        string[] specialImdbIds = await JsonHelper.DeserializeFromFileAsync<string[]>(settings.MovieImdbSpecialMetadata);
        Dictionary<string, TopMetadata[]> x265Summaries = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopX265Metadata);
        Dictionary<string, TopMetadata[]> h264Summaries = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264Metadata);
        Dictionary<string, PreferredMetadata[]> preferredDetails = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(settings.MoviePreferredMetadata);

        specialImdbIds = specialImdbIds
            .Where(imdbId => x265Summaries.ContainsKey(imdbId))
            .Concat(specialImdbIds.Where(imdbId => h264Summaries.ContainsKey(imdbId)))
            .Concat(specialImdbIds.Where(imdbId => preferredDetails.ContainsKey(imdbId)))
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

            if (preferredDetails.ContainsKey(imdbId))
            {
                preferredDetails[imdbId].ForEach(details =>
                {
                    log($"Preferred {details.Link}");
                    log(details.Title);
                });
            }

            log(string.Empty);
        });
    }

    internal static async Task PrintHighRatingAsync(ISettings settings, string threshold = "8.0", string[]? excludedGenres = null, Action<string>? log = null, params string[] directories)
    {
        log ??= Logger.WriteLine;
        HashSet<string> existingImdbIds = new(
            directories.SelectMany(directory => Directory
                .EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
                .Select(file => PathHelper.GetFileNameWithoutExtension(file).Split(".").First())
                .Where(imdbId => imdbId != NotExistingFlag)),
            StringComparer.OrdinalIgnoreCase);
        Dictionary<string, TopMetadata[]> x265Summaries = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopX265Metadata);
        Dictionary<string, TopMetadata[]> h264Summaries = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264Metadata);
        Dictionary<string, PreferredMetadata[]> preferredDetails = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(settings.MoviePreferredMetadata);

        Dictionary<string, Dictionary<string, IImdbMetadata[]>> highRatings = new();

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

        preferredDetails
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

    internal static void PrintDirectoryTitleMismatch(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
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

                if (releaseTitles.IsEmpty()
                    && !imdbMetadata.Titles.TryGetValue("USA", out releaseTitles)
                    && !imdbMetadata.Titles.TryGetValue("USA (working title)", out releaseTitles)
                    && !imdbMetadata.Titles.TryGetValue("USA (informal English title)", out releaseTitles))
                {
                    releaseTitles = imdbMetadata.Titles
                        .Where(pair => pair.Key.ContainsIgnoreCase("USA"))
                        .SelectMany(pair => pair.Value)
                        .ToArray();
                }

                if (releaseTitles.IsEmpty()
                    && !imdbMetadata.Titles.TryGetValue("UK", out releaseTitles)
                    && !imdbMetadata.Titles.TryGetValue("UK (informal English title)", out releaseTitles))
                {
                    releaseTitles = imdbMetadata.Titles
                        .Where(pair => pair.Key.ContainsIgnoreCase("UK"))
                        .SelectMany(pair => pair.Value)
                        .ToArray();
                }

                if (releaseTitles.IsEmpty()
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
                List<string> localTitles =
                [
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
                    $"{parsed.DefaultTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First()}{parsed.DefaultTitle3.Split(InstallmentSeparator).First()}".Replace(SubtitleSeparator, " ")
                ];
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
                string newMovie = Path.Combine(PathHelper.GetDirectoryName(movie), parsed.ToString());
                if (movie.EqualsOrdinal(newMovie))
                {
                    return;
                }

                log(movie);
                log(newMovie);
                log(string.Empty);
            });
    }

    internal static void PrintDirectoryOriginalTitleMismatch(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
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
                List<string> localTitles =
                [
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
                    $"{parsed.OriginalTitle2.TrimStart(SubtitleSeparator.ToCharArray()).Split(InstallmentSeparator).First()}{parsed.OriginalTitle3.Split(InstallmentSeparator).First()}".Replace(SubtitleSeparator, " ")
                ];
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
                string newMovie = Path.Combine(PathHelper.GetDirectoryName(movie), parsed.ToString());
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
        log ??= Logger.WriteLine;
        Directory
            .GetFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
            .Select(file => (File: file, Metadata: Imdb.TryLoad(file, out ImdbMetadata? imdbMetadata) ? imdbMetadata : null))
            .Where(metadata => metadata.Metadata?.Genres.ContainsIgnoreCase(genre) is true)
            .ForEach(metadata => log($"{PathHelper.GetDirectoryName(metadata.File)}"));
    }

    internal static void PrintMovieRegionsWithErrors(Dictionary<string, string[]> allLocalRegions, Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level - 1))
            .OrderBy(localRegionDirectory => localRegionDirectory)
            .ForEach(localRegionDirectory =>
            {
                string localRegionText = PathHelper.GetFileNameWithoutExtension(localRegionDirectory);
                if (localRegionText.ContainsIgnoreCase("Delete") || localRegionText.ContainsIgnoreCase("Temp"))
                {
                    return;
                }

                if (!allLocalRegions.TryGetValue(localRegionText, out string[]? currentLocalRegion))
                {
                    int lastIndex = localRegionText.LastIndexOfOrdinal(" ");
                    if (lastIndex >= 0)
                    {
                        localRegionText = localRegionText[..lastIndex];
                        if (allLocalRegions.TryGetValue(localRegionText, out currentLocalRegion))
                        {
                        }
                        else
                        {
                            currentLocalRegion = [localRegionText];
                        }
                    }
                    else
                    {
                        currentLocalRegion = [localRegionText];
                    }
                }

                log($"==={localRegionDirectory}==={string.Join(", ", currentLocalRegion)}");
                string[] currentRegions = currentLocalRegion.Where(region => !region.EndsWithOrdinal(NotExistingFlag)).ToArray();
                string[] ignorePrefixes = currentLocalRegion.Where(prefix => prefix.EndsWithOrdinal(NotExistingFlag)).Select(prefix => prefix.TrimEnd(NotExistingFlag.ToCharArray())).ToArray();
                Directory
                    .GetDirectories(localRegionDirectory)
                    .ForEach(movie =>
                    {
                        string movieName = PathHelper.GetFileName(movie);
                        if (ignorePrefixes.Any(movieName.StartsWithOrdinal))
                        {
                            return;
                        }

                        if (Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata))
                        {
                            if (!imdbMetadata.Regions.Any(imdbRegion => currentRegions.Any(localRegion => imdbRegion.EqualsOrdinal(localRegion) || $"{imdbRegion}n".EqualsOrdinal(localRegion))))
                            {
                                log(movie);
                                log($"{string.Join(", ", currentRegions)}==={string.Join(", ", imdbMetadata.Regions)}");
                                log(string.Empty);
                            }
                        }
                    });
            });
    }

    internal static void PrintMovieWithoutTextSubtitle(Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .ForEach(m =>
            {
                string[] files = Directory.GetFiles(m);
                if (!files.Any(IsTextSubtitle))
                {
                    Imdb.TryLoad(m, out ImdbMetadata? meta);
                    log(meta?.ImdbId ?? "-");
                    log(m);
                    log("");
                }
            });
    }

    internal static async Task PrintMovieImdbIdErrorsAsync(ISettings settings, Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopX265Metadata);
        Dictionary<string, TopMetadata[]> h264Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264Metadata);
        Dictionary<string, TopMetadata[]> x265XMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopX265XMetadata);
        Dictionary<string, TopMetadata[]> h264XMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264XMetadata);
        //Dictionary<string, PreferredMetadata[]> preferredMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(preferredJsonPath);
        //Dictionary<string, TopMetadata[]> h264720PMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(h264720PJsonPath);
        HashSet<string> topDuplications = new(settings.MovieTopDuplications, StringComparer.OrdinalIgnoreCase);

        Dictionary<string, string> x265TitlesImdbIds = x265Metadata
            .SelectMany(pair => pair.Value)
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        Dictionary<string, string> x265XTitlesImdbIds = x265XMetadata
            .SelectMany(pair => pair.Value)
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        Dictionary<string, string> h264TitlesImdbIds = h264Metadata
            .Where(pair => pair.Key.IsNotNullOrWhiteSpace())
            .SelectMany(pair => pair.Value)
            .Where(metadata => !topDuplications.Contains(metadata.Title))
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        Dictionary<string, string> h264XTitlesImdbIds = h264XMetadata
            .SelectMany(pair => pair.Value)
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);

        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .ForEach(movie =>
            {
                string nfo = Directory.EnumerateFiles(movie, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly).FirstOrDefault(string.Empty);
                if (nfo.IsNullOrWhiteSpace())
                {
                    log($"-IMDB id is unavailable: {movie}");
                    return;
                }

                XDocument nfoDocument = XDocument.Load(nfo);
                string localImdbId = nfoDocument.Root?.Element("imdbid")?.Value ?? nfoDocument.Root?.Element("imdb_id")?.Value ?? string.Empty;
                if (localImdbId.IsNullOrWhiteSpace())
                {
                    log($"!!!IMDB id is unavailable: {movie}");
                    return;
                }

                if (Imdb.TryRead(movie, out string? jsonImdbId, out _, out _, out _, out _) && !localImdbId.EqualsIgnoreCase(jsonImdbId))
                {
                    log($"-IMDB id is inconsistent: {movie}");
                    return;
                }

                VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetVideos(movie).ToArray();
                if (videos.IsEmpty())
                {
                    return;
                }

                VideoMovieFileInfo[] x265Videos = videos.Where(video => video.GetEncoderType() is EncoderType.X).ToArray();
                if (x265Videos.Any())
                {
                    if (localImdbId.IsNullOrWhiteSpace() || localImdbId.EqualsOrdinal(SubtitleSeparator))
                    {
                        log($"!IMDB id is missing as x265: {movie}");
                        return;
                    }

                    x265Videos.ForEach(x265Video =>
                    {
                        string x265Title = x265TitlesImdbIds.Keys.FirstOrDefault(key => x265Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (x265Title.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = x265TitlesImdbIds[x265Title];
                            if (!remoteImdbId.EqualsIgnoreCase(localImdbId))
                            {
                                log($"!IMDB id {localImdbId} should be {remoteImdbId}: {movie}");
                            }

                            return;
                        }

                        string x265XTitle = x265XTitlesImdbIds.Keys.FirstOrDefault(key => x265Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (x265XTitle.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = x265XTitlesImdbIds[x265XTitle];
                            if (!remoteImdbId.EqualsIgnoreCase(localImdbId))
                            {
                                log($"!IMDB id {localImdbId} should be {remoteImdbId}: {movie}");
                            }

                            return;
                        }

                        log($"-Title {x265Video.Name} is missing in x265: {movie}");
                    });
                }

                VideoMovieFileInfo[] h264Videos = videos.Where(video => video.GetEncoderType() is EncoderType.H && video.GetDefinitionType() == DefinitionType.P1080).ToArray();
                if (h264Videos.Any())
                {
                    if (localImdbId.IsNullOrWhiteSpace() || localImdbId.EqualsOrdinal(SubtitleSeparator))
                    {
                        log($"!IMDB id is missing as H264: {movie}");
                        return;
                    }

                    h264Videos.ForEach(h264Video =>
                    {
                        string h264Title = h264TitlesImdbIds.Keys.FirstOrDefault(key => h264Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (h264Title.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = h264TitlesImdbIds[h264Title];
                            if (!remoteImdbId.EqualsIgnoreCase(localImdbId))
                            {
                                log($"!IMDB id {localImdbId} should be {remoteImdbId}: {movie}");
                            }

                            return;
                        }

                        string h264XTitle = h264XTitlesImdbIds.Keys.FirstOrDefault(key => h264Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (h264XTitle.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = h264XTitlesImdbIds[h264XTitle];
                            if (!remoteImdbId.EqualsIgnoreCase(localImdbId))
                            {
                                log($"!IMDB id {localImdbId} should be {remoteImdbId}: {movie}");
                            }

                            return;
                        }

                        log($"-Title {h264Video.Name}is missing in H264: {movie}");
                    });
                }
            });
    }

    internal static void PrintNikkatsu(Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        directories.SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .Where(movie => !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Nikkatsu") && !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Oniroku Dan") && !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Angel Guts"))
            .Select(movie => (movie, XDocument.Load(Directory.EnumerateFiles(movie, "*.nfo").First())))
            .Where(movie => movie.Item2.Root?.Elements("studio").Any(element => element.Value.ContainsIgnoreCase("Nikkatsu")) is true && movie.Item2.Root.Elements("tag").Any(element => element.Value.ContainsIgnoreCase("pink")))
            .ForEach(movie =>
            {
                log(movie.Item1);
                log(string.Join(", ", movie.Item2.Root!.Elements("studio").Select(element => element.Value)));
                log(string.Join(", ", movie.Item2.Root.Elements("tag").Select(element => element.Value)));
                // DirectoryHelper.AddPrefix(movie.Item1, "Nikkatsu Pink-");
                log(string.Empty);
            });
    }

    internal static async Task PrintMovieLinksAsync(
        ISettings settings, Func<ImdbMetadata, bool> predicate, HashSet<string> keywords, string initialUrl = "", bool updateMetadata = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ILookup<string, string> topMagnetUris = (await File.ReadAllLinesAsync(settings.TopMagnetUrls)).ToLookup(line => MagnetUri.Parse(line).DisplayName, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, ImdbMetadata> mergedMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, ImdbMetadata>>(settings.MovieMergedMetadata);

        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, Dictionary<string, VideoMetadata>>>(settings.MovieLibraryMetadata);
        Dictionary<string, TopMetadata[]> x265Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopX265Metadata);
        Dictionary<string, TopMetadata[]> h264Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.MovieTopH264Metadata);
        //Dictionary<string, PreferredMetadata[]> preferredMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(preferredJsonPath);
        //Dictionary<string, TopMetadata[]> h264720PMetadata = await JsonHelper.DeserializeFromFileAsync(h264720PJsonPath);
        //Dictionary<string, RareMetadata> rareMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, RareMetadata>>(rareJsonPath);
        string[] metadataFiles = Directory.GetFiles(settings.MovieMetadataDirectory);
        Dictionary<string, string> metadataFilesByImdbId = metadataFiles.ToDictionary(file => PathHelper.GetFileName(file).Split("-").First());
        MagnetUri[] downloadingLinks = (await File.ReadAllLinesAsync(@"D:\Files\Library\Movie.Downloading.txt"))
            .Select(line => (MagnetUri.TryParse(line, out MagnetUri? result), result))
            .Where(result => result.Item1)
            .Select(result => result.result!).ToArray();
        string[] cacheFiles = Directory.GetFiles(settings.MovieMetadataCacheDirectory);

        ImdbMetadata[] imdbIds = x265Metadata.Keys
            .Concat(h264Metadata.Keys)
            //.Concat(preferredMetadata.Keys)
            //.Concat(h264720PMetadata.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Except(libraryMetadata.Keys, StringComparer.OrdinalIgnoreCase)
            //.Intersect(rareMetadata
            //    .SelectMany(rare => Regex
            //        .Matches(rare.Value.Content, @"imdb\.com/title/(tt[0-9]+)")
            //        .Where(match => match.Success)
            //        .Select(match => match.Groups[1].Value)))
            //.Intersect(mergedMetadata.Keys)
            .Select(imdbId => mergedMetadata[imdbId])
            .Where(predicate)
            .OrderBy(imdbMetadata => imdbMetadata.ImdbId)
            .ToArray();
        int length = imdbIds.Length;
        log(length.ToString());
        log(x265Metadata.Keys.Intersect(imdbIds.Select(metadata => metadata.ImdbId)).Count().ToString());

        keywords.Select(keyword => (keyword, imdbIds.Count(imdbId => imdbId.AllKeywords.ContainsIgnoreCase(keyword))))
            .OrderByDescending(keyword => keyword.Item2)
            .ForEach(keyword => log($"{keyword.Item2} - {keyword}"));

        //HashSet<string> downloadedTitles = new(
        //    new string[] { }.SelectMany(Directory.GetDirectories).Select(Path.GetFileName)!,
        //    StringComparer.OrdinalIgnoreCase);
        //HashSet<string> downloadedTorrentHashes = new(/*Directory.GetFiles(@"E:\Files\MonoTorrent").Concat(Directory.GetFiles(@"E:\Files\MonoTorrentDownload")).Select(torrent => PathHelper.GetFileNameWithoutExtension(torrent).Split("@").Last()), StringComparer.OrdinalIgnoreCase*/);
        using WebDriverWrapper? webDriver = isDryRun ? null : new(() => WebDriverHelper.Start(isLoadingAll: true));
        using HttpClient? httpClient = isDryRun ? null : new HttpClient().AddEdgeHeaders();
        if (!isDryRun && initialUrl.IsNotNullOrWhiteSpace())
        {
            webDriver!.Url = initialUrl;
            webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.Id("pager_links")));
        }

        if (updateMetadata)
        {
            await imdbIds
                .Select(imdbId => imdbId.ImdbId)
                .ForEachAsync(async (imdbId, index) =>
                {
                    log($"{index * 100 / length}% - {index}/{length} - {imdbId}");
                    try
                    {
                        await DownloadImdbMetadataAsync(imdbId, settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, metadataFiles, cacheFiles, webDriver, overwrite: true, useCache: false, log: log);
                    }
                    catch (ArgumentOutOfRangeException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                    {
                        log($"!!!{imdbId}");
                    }
                    catch (ArgumentException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                    {
                        log($"!!!{imdbId}");
                    }
                });
        }

        await imdbIds
            .ForEachAsync(async (imdbMetadata, index) =>
            {
                //log($"{index * 100 / length}% - {index}/{length} - {imdbMetadata!.ImdbId}");
                if (x265Metadata.TryGetValue(imdbMetadata.ImdbId, out TopMetadata[]? x265Videos))
                {
                    List<TopMetadata> excluded = [];
                    if (x265Videos.Length > 1)
                    {
                        if (x265Videos.Any(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")) && x265Videos.Any(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
                        {
                            excluded.AddRange(x265Videos.Where(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))));
                            x265Videos = x265Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                        }

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

                    if (x265Videos.Any(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")))
                    {
                        excluded.AddRange(x265Videos.Where(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")));
                        x265Videos = x265Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                    }

                    bool hasDownload = false;
                    await x265Videos.ForEachAsync(async metadata =>
                    {
                        if (isDryRun)
                        {
                            //log($"{metadata.ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{metadata.Title} {metadata.Link} {imdbMetadata.Link}");
                            //log($"{imdbMetadata.Link}keywords");
                            //log($"{imdbMetadata.Link}parentalguide");
                            if (topMagnetUris.Contains(metadata.Title))
                            {
                                string[] uris = topMagnetUris[metadata.Title].ToArray();
                                if (uris.Length > 0)
                                {
                                    MagnetUri[] result = uris
                                        .Take(0..)
                                        .Select(MagnetUri.Parse)
                                        .Where(uri => !downloadingLinks.Any(downloadingUri => downloadingUri.ExactTopic.EqualsIgnoreCase(uri.ExactTopic) || downloadingUri.DisplayName.EqualsIgnoreCase(uri.DisplayName)))
                                        .ToArray();
                                    if (result.Any())
                                    {
                                        hasDownload = true;
                                        result.Select(uri => uri.ToString()).Append(string.Empty).ForEach(log);
                                    }
                                }
                            }
                            else
                            {
                                //log($"!!! Cannot find magnet for title {metadata.Title}");
                            }

                            return;
                        }

                        string cacheFile = Path.Combine(settings.MovieMetadataCacheDirectory, $"{metadata.ImdbId}-{metadata.Title}{ImdbCacheExtension}");
                        string html = cacheFiles.ContainsIgnoreCase(cacheFile) && new FileInfo(cacheFile).LastWriteTimeUtc > DateTime.UtcNow - TimeSpan.FromDays(1)
                            ? await File.ReadAllTextAsync(cacheFile)
                            : await webDriver!.GetStringAsync(metadata.Link, () => webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))));
                        await Task.Delay(WebDriverHelper.DefaultDomWait);
                        await File.WriteAllTextAsync(cacheFile, html);
                        (string httpUrl, string fileName, string magnetUrl) = TopGetUrls(html, metadata.Link);
                        log(magnetUrl);
                        //webDriver!.Url = httpUrl;
                        //await Task.Delay(TimeSpan.FromSeconds(3));
                    });

                    if (hasDownload)
                    {
                        return;
                    }
                }

                if (h264Metadata.TryGetValue(imdbMetadata.ImdbId, out TopMetadata[]? h264Videos))
                {
                    List<TopMetadata> excluded = [];
                    if (h264Videos.Length > 1)
                    {
                        if (h264Videos.Any(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")) && h264Videos.Any(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
                        {
                            excluded.AddRange(h264Videos.Where(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))));
                            h264Videos = h264Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
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

                    if (h264Videos.Any(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")))
                    {
                        excluded.AddRange(h264Videos.Where(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")));
                        h264Videos = h264Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                    }

                    bool hasDownload = false;
                    await h264Videos.ForEachAsync(async metadata =>
                    {
                        if (isDryRun)
                        {
                            //log($"{metadata.ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{metadata.Title} {metadata.Link} {imdbMetadata.Link}");
                            //log($"{imdbMetadata.Link}keywords");
                            //log($"{imdbMetadata.Link}parentalguide");
                            if (topMagnetUris.Contains(metadata.Title))
                            {
                                string[] uris = topMagnetUris[metadata.Title].ToArray();
                                if (uris.Length > 0)
                                {
                                    MagnetUri[] result = uris
                                        .Take(0..)
                                        .Select(MagnetUri.Parse)
                                        .Where(uri => !downloadingLinks.Any(downloadingUri => downloadingUri.ExactTopic.EqualsIgnoreCase(uri.ExactTopic) || downloadingUri.DisplayName.EqualsIgnoreCase(uri.DisplayName)))
                                        .ToArray();
                                    if (result.Any())
                                    {
                                        hasDownload = true;
                                        result.Select(uri => uri.ToString()).Append(string.Empty).ForEach(log);
                                    }
                                }
                            }
                            else
                            {
                                //log($"!!! Cannot find magnet for title {metadata.Title}");
                            }

                            return;
                        }

                        string cacheFile = Path.Combine(settings.MovieMetadataCacheDirectory, $"{metadata.ImdbId}-{metadata.Title}{ImdbCacheExtension}");
                        string html = cacheFiles.ContainsIgnoreCase(cacheFile) && new FileInfo(cacheFile).LastWriteTimeUtc > DateTime.UtcNow - TimeSpan.FromDays(1)
                            ? await File.ReadAllTextAsync(cacheFile)
                            : await webDriver!.GetStringAsync(metadata.Link, () => webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))));
                        await Task.Delay(WebDriverHelper.DefaultDomWait);
                        await File.WriteAllTextAsync(cacheFile, html);
                        (string httpUrl, string fileName, string magnetUrl) = TopGetUrls(html, metadata.Link);
                        log(magnetUrl);
                        //webDriver!.Url = httpUrl;
                        //await Task.Delay(TimeSpan.FromSeconds(3));
                    });

                    if (hasDownload)
                    {
                        return;
                    }
                }

                //if (preferredMetadata.TryGetValue(imdbMetadata.ImdbId, out PreferredMetadata[]? preferredVideos))
                //{
                //    KeyValuePair<string, string>[] availabilities = preferredVideos.SelectMany(preferredVideo => preferredVideo.Availabilities).ToArray();
                //    KeyValuePair<string, string>[] videos = availabilities.Where(availability => availability.Key.ContainsIgnoreCase("1080p.BluRay")).ToArray();
                //    if (videos.IsEmpty())
                //    {
                //        videos = availabilities.Where(availability => availability.Key.ContainsIgnoreCase("1080p.WEB")).ToArray();
                //    }

                //    if (videos.IsEmpty())
                //    {
                //        videos = availabilities.Where(availability => availability.Key.ContainsIgnoreCase("720p.BluRay")).ToArray();
                //    }

                //    if (videos.IsEmpty())
                //    {
                //        videos = availabilities.Where(availability => availability.Key.ContainsIgnoreCase("720p.WEB")).ToArray();
                //    }

                //    if (videos.IsEmpty())
                //    {
                //        videos = availabilities.Where(availability => availability.Key.ContainsIgnoreCase("480p.DVD")).ToArray();
                //    }

                //    log($"{preferredVideos.First().ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{preferredVideos.First().Title} {preferredVideos.First().Link} {imdbMetadata.Link}");
                //    log($"{imdbMetadata.Link}keywords");
                //    log($"{imdbMetadata.Link}parentalguide");
                //    await videos.ForEachAsync(async video =>
                //    {
                //        log(string.Empty);
                //        log(video.Value);
                //        if (httpClient is not null)
                //        {
                //            string file = Path.Combine(cacheDirectory, $"{preferredVideos.First().ImdbId}-{preferredVideos.First().Title}-{video.Key}.torrent".ReplaceOrdinal(" - ", " ").ReplaceOrdinal("-", " ").ReplaceOrdinal(".", " ").ReplaceIgnoreCase("：", "-").FilterForFileSystem().Trim());
                //            if (!File.Exists(file))
                //            {
                //                try
                //                {
                //                    await httpClient.GetFileAsync(video.Value, file);
                //                }
                //                catch (HttpRequestException exception)
                //                {
                //                    log(exception.ToString());
                //                }
                //            }
                //        }

                //        log(string.Empty);
                //    });
                //}

                //if (h264720PMetadata.TryGetValue(imdbMetadata.ImdbId, out TopMetadata[]? h264720PVideos))
                //{
                //    List<TopMetadata> excluded = new();
                //    if (h264720PVideos.Length > 1)
                //    {
                //        if (h264720PVideos.Any(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopForeignKeyword}")) && h264720PVideos.Any(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopForeignKeyword}"))))
                //        {
                //            excluded.AddRange(h264720PVideos.Where(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopForeignKeyword}"))));
                //            h264720PVideos = h264720PVideos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{TopForeignKeyword}")).ToArray();
                //        }

                //        if (h264720PVideos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && h264720PVideos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                //        {
                //            excluded.AddRange(h264720PVideos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                //            h264720PVideos = h264720PVideos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                //        }

                //        if (h264720PVideos.Any(video => video.Title.ContainsIgnoreCase(".FRENCH.")) && h264720PVideos.Any(video => video.Title.ContainsIgnoreCase(".DUBBED.")))
                //        {
                //            excluded.AddRange(h264720PVideos.Where(video => video.Title.ContainsIgnoreCase(".DUBBED.")));
                //            h264720PVideos = h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase(".DUBBED.")).ToArray();
                //        }

                //        if (h264720PVideos.Any(video => video.Title.ContainsIgnoreCase(".EXTENDED.")) && h264720PVideos.Any(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")))
                //        {
                //            excluded.AddRange(h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")));
                //            h264720PVideos = h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")).ToArray();
                //        }

                //        if (h264720PVideos.Any(video => video.Title.ContainsIgnoreCase(".REMASTERED.")) && h264720PVideos.Any(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")))
                //        {
                //            excluded.AddRange(h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")));
                //            h264720PVideos = h264720PVideos.Where(video => video.Title.ContainsIgnoreCase(".REMASTERED.")).ToArray();
                //        }

                //        if (h264720PVideos.Any(video => video.Title.ContainsIgnoreCase(".PROPER.")) && h264720PVideos.Any(video => !video.Title.ContainsIgnoreCase(".PROPER.")))
                //        {
                //            excluded.AddRange(h264720PVideos.Where(video => !video.Title.ContainsIgnoreCase(".PROPER.")));
                //            h264720PVideos = h264720PVideos.Where(video => video.Title.ContainsIgnoreCase(".PROPER.")).ToArray();
                //        }
                //    }

                //    await h264720PVideos.ForEachAsync(async metadata =>
                //    {
                //        string file = Path.Combine(cacheDirectory, $"{metadata.ImdbId}-{metadata.Title}{ImdbCacheExtension}");
                //        if (cacheFiles.ContainsIgnoreCase(file))
                //        {
                //            return;
                //        }

                //        string html = await webDriver.GetStringAsync(metadata.Link, () => new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))));
                //        await Task.Delay(WebDriverHelper.DefaultDomWait);
                //        await File.WriteAllTextAsync(file, html);
                //        string magnet = TopGetMagnetLink(html);

                //        Debug.Assert(magnet.IsNotNullOrWhiteSpace());
                //        log($"{metadata.ImdbId}-{metadata.Title}");
                //        log(magnet);
                //        log(string.Empty);
                //    });

                //    return;
                //}
            });
    }

    internal static (string HttpUrl, string FileName, string MagnetUrl) TopGetUrls(CQ pageCQ, string baseUrl)
    {
        CQ magnetCQ = pageCQ.Find("""img[src$="magnet.gif"]""").Parent();
        string httpUrl = magnetCQ.Prev().Attr("href");
        string magnetUrl = magnetCQ.Attr("href");
        Debug.Assert(httpUrl.IsNotNullOrWhiteSpace());
        Debug.Assert(magnetUrl.IsNotNullOrWhiteSpace());
        Uri httpUri = new(new Uri(baseUrl), httpUrl);
        httpUrl = httpUri.ToString();
        string fileName = HttpUtility.ParseQueryString(httpUri.Query)["f"] ?? throw new InvalidOperationException(httpUrl);
        return (httpUrl, fileName, magnetUrl);
    }

    internal static async Task PrintTVLinks(
        ISettings settings, string[] tvDirectories, string initialUrl, Func<ImdbMetadata, bool> predicate,
        bool isDryRun = false, bool updateMetadata = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(settings.TVTopX265Metadata);
        Dictionary<string, string> metadataFiles = Directory.EnumerateFiles(settings.TVMetadataDirectory).ToDictionary(file => PathHelper.GetFileName(file).Split("-").First());
        string[] cacheFiles = Directory.GetFiles(settings.TVMetadataCacheDirectory);
        string[] libraryImdbIds = tvDirectories.SelectMany(tvDirectory => Directory.EnumerateFiles(tvDirectory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .Select(file => Imdb.TryRead(file, out string? imdbId, out _, out _, out _, out _) ? imdbId : string.Empty)
            .Where(imdbId => imdbId.IsNotNullOrWhiteSpace())
            .ToArray();

        ImdbMetadata?[] imdbIds = x265Metadata.Keys
            .Distinct()
            .Except(libraryImdbIds)
            .Select(imdbId => metadataFiles.TryGetValue(imdbId, out string? file) && Imdb.TryLoad(file, out ImdbMetadata? imdbMetadata)
                ? imdbMetadata
                : null)
            .Where(imdbMetadata => imdbMetadata is not null
                && predicate(imdbMetadata))
            .OrderBy(imdbId => imdbId.ImdbId)
            .ToArray();
        int length = imdbIds.Length;
        if (isDryRun)
        {
            log(length.ToString());
        }

        using WebDriverWrapper webDriver = new(() => WebDriverHelper.Start(isLoadingAll: true));
        webDriver.Url = initialUrl;
        webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.Id("pager_links")));

        if (updateMetadata)
        {

            await imdbIds.ForEachAsync(async (imdbMetadata, index) =>
            {
                log($"{index * 100 / length}% - {index}/{length} - {imdbMetadata.ImdbId}");
                try
                {
                    await DownloadImdbMetadataAsync(imdbMetadata.ImdbId, settings.TVMetadataDirectory, settings.TVMetadataCacheDirectory, metadataFiles.Values.ToArray(), cacheFiles, webDriver, overwrite: true, useCache: false, log: log);
                }
                catch (ArgumentOutOfRangeException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbMetadata.ImdbId}");
                }
                catch (ArgumentException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbMetadata.ImdbId}");
                }
            });
        }

        await imdbIds
            .ForEachAsync(async (imdbMetadata, index) =>
            {
                if (x265Metadata.TryGetValue(imdbMetadata.ImdbId, out TopMetadata[]? h265Videos))
                {
                    if (h265Videos.Any(video => video.Genres.ContainsIgnoreCase("Animation")))
                    {
                        return;
                    }

                    List<TopMetadata> excluded = [];
                    if (h265Videos.Length > 1)
                    {
                        if (h265Videos.Any(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")) && h265Videos.Any(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
                        {
                            excluded.AddRange(h265Videos.Where(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))));
                            h265Videos = h265Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                        }

                        if (h265Videos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && h265Videos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                        {
                            excluded.AddRange(h265Videos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                            h265Videos = h265Videos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                        }

                        if (h265Videos.Any(video => video.Title.ContainsIgnoreCase(".FRENCH.")) && h265Videos.Any(video => video.Title.ContainsIgnoreCase(".DUBBED.")))
                        {
                            excluded.AddRange(h265Videos.Where(video => video.Title.ContainsIgnoreCase(".DUBBED.")));
                            h265Videos = h265Videos.Where(video => !video.Title.ContainsIgnoreCase(".DUBBED.")).ToArray();
                        }

                        if (h265Videos.Any(video => video.Title.ContainsIgnoreCase(".EXTENDED.")) && h265Videos.Any(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")))
                        {
                            excluded.AddRange(h265Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")));
                            h265Videos = h265Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")).ToArray();
                        }

                        if (h265Videos.Any(video => video.Title.ContainsIgnoreCase(".REMASTERED.")) && h265Videos.Any(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")))
                        {
                            excluded.AddRange(h265Videos.Where(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")));
                            h265Videos = h265Videos.Where(video => video.Title.ContainsIgnoreCase(".REMASTERED.")).ToArray();
                        }

                        if (h265Videos.Any(video => video.Title.ContainsIgnoreCase(".PROPER.")) && h265Videos.Any(video => !video.Title.ContainsIgnoreCase(".PROPER.")))
                        {
                            excluded.AddRange(h265Videos.Where(video => !video.Title.ContainsIgnoreCase(".PROPER.")));
                            h265Videos = h265Videos.Where(video => video.Title.ContainsIgnoreCase(".PROPER.")).ToArray();
                        }
                    }

                    await h265Videos.ForEachAsync(async metadata =>
                    {
                        if (isDryRun)
                        {
                            log($"{metadata.ImdbId}-{metadata.Title} {metadata.Link} {imdbMetadata.Link}");
                            log($"{imdbMetadata.Link}keywords");
                            log($"{imdbMetadata.Link}parentalguide");
                            log(string.Empty);
                            return;
                        }

                        string file = Path.Combine(settings.TVMetadataCacheDirectory, $"{metadata.ImdbId}-{metadata.Title}{ImdbCacheExtension}");
                        string html;
                        bool isCached = cacheFiles.ContainsIgnoreCase(file);
                        if (isCached)
                        {
                            FileInfo cacheFileInfo = new(file);
                            if (cacheFileInfo.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromDays(1))
                            {
                                return;
                            }

                            html = await File.ReadAllTextAsync(file);
                        }
                        else
                        {
                            html = await webDriver.GetStringAsync(metadata.Link, () => webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))));
                            await Task.Delay(WebDriverHelper.DefaultDomWait);
                            await File.WriteAllTextAsync(file, html);
                        }
                        (string httpUrl, string fileName, string magnetUrl) = TopGetUrls(html, metadata.Link);

                        // log(magnetUrl);
                        // webDriver!.Url = httpUrl;
                        log($"-{imdbMetadata.Title} | {metadata.Title}");
                        log($"https://www.imdb.com/title/{imdbMetadata.ImdbId}/parentalguide");
                        log(metadata.Link);
                        log(httpUrl);
                        log(magnetUrl);
                        log(string.Empty);
                        if (!isCached)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(3));
                        }
                    });
                }
            });
    }
}