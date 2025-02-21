namespace MediaManager.IO;

using System.Linq;
using System.Web;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using MediaManager.Net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;

internal static partial class Video
{
    internal static void PrintVideosWithErrors(ISettings settings, string directory, bool isNoAudioAllowed = false, bool skipTopPreferred = false, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, Action<string>? is720 = null, Action<string>? is1080 = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        PrintVideosWithErrors(
            settings,
            Directory
                .EnumerateFiles(directory, PathHelper.AllSearchPattern, searchOption)
                .Where(file => file.IsVideo() && !file.HasExtension(DiskImageExtension) && (predicate is null || predicate(file))),
            isNoAudioAllowed,
            skipTopPreferred,
            is720,
            is1080,
            log);
    }

    private static void PrintVideosWithErrors(ISettings settings, IEnumerable<string> files, bool isNoAudioAllowed = false, bool skipTopPreferred = false, Action<string>? is720 = null, Action<string>? is1080 = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ConcurrentBag<(string Video, (string? Message, Action<string>? Action) Error)> results = [];

        ParallelQuery<string> parallelFiles = files
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism);
        if (skipTopPreferred)
        {
            parallelFiles = parallelFiles
                .Where(video =>
                {
                    string name = PathHelper.GetFileNameWithoutExtension(video);
                    return !name.ContainsIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !name.ContainsIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}") && !name.ContainsIgnoreCase(settings.PreferredNewKeyword) && !name.ContainsIgnoreCase(settings.PreferredOldKeyword);
                });
        }

        Lock errorLock = new();
        parallelFiles
            .Select((video, index) =>
            {
                if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata, log: message => log($"{index} {message}")))
                {
                    log($"!Failed {video}");
                }

                return (video, videoMetadata);
            })
            .Select(videoMetadata => (
                Video: videoMetadata.video,
                Error: videoMetadata.videoMetadata is null ? (null, null) : GetVideoError(videoMetadata.videoMetadata, isNoAudioAllowed, is720, is1080)))
            .Where(result => result.Error.Message.IsNotNullOrWhiteSpace())
            .ForAll(result =>
            {
                results.Add(result);
                lock (errorLock)
                {
                    File.AppendAllLines(@"d:\temp\errors.txt", [result.Video, result.Error.Message ?? string.Empty, string.Empty]);
                }
            });

        results
            .OrderBy(result => result.Video)
            .ForEach(result =>
            {
                log(result.Error.Message ?? result.Video);
                result.Error.Action?.Invoke(result.Video);
            });
    }

    internal static void PrintDirectoriesWithLowDefinition(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .Where(movie => !VideoDirectoryInfo.GetMovies(movie).Any(video => video.IsHD()))
            .ForEach(log);
    }

    internal static void PrintDirectoriesWithMultipleVideos(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetMovies(movie).ToArray();
                if (videos.Length <= 1 || videos.All(video => video.Part.IsNotNullOrWhiteSpace()))
                {
                    return;
                }

                log(movie);
            });
    }

    internal static void PrintVideosP(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.HD, log);

    internal static void PrintVideosY(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.PreferredH264, log);

    internal static void PrintVideosNotX(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.TopX265, log);

    internal static void PrintVideosH(string directory, int level = DefaultDirectoryLevel, Action<string>? log = null) =>
        PrintVideos(directory, level, file => VideoMovieFileInfo.Parse(file).GetEncoderType() is EncoderType.TopH264, log);

    private static void PrintVideos(string directory, int level, Func<string, bool> predicate, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .OrderBy(movie => movie)
            .ForEach(movie =>
            {
                VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetMovies(movie).ToArray();
                if (videos.Any())
                {
                    log(movie);
                    ImdbMetadata.TryRead(movie, out string? imdbId, out _, out _, out _, out _);
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

            noSubtitle = movie => !Directory.EnumerateFiles(movie).Any(file => searchPatterns.Any(searchPattern => searchPattern.IsMatch(file)));
        }

        EnumerateDirectories(directory, level)
            .Where(noSubtitle)
            .ForEach(movie => log($"{(ImdbMetadata.TryRead(movie, out string? imdbId, out _, out _, out _, out _) ? imdbId : NotExistingFlag)} {movie}"));
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
                    movieDirectory = movieDirectory["0.".Length..];
                }

                VideoDirectoryInfo videoDirectoryInfo = VideoDirectoryInfo.Parse(movieDirectory);
                string directoryYear = videoDirectoryInfo.Year;
                string metadataYear = metadata.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{metadata} has no year.");
                string videoName = string.Empty;
                if (!(directoryYear.EqualsOrdinal(metadataYear)
                        && Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                            .Where(IsVideo)
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

    internal static async Task PrintLibraryDuplicateImdbIdAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        existingMetadata
            .Where(pair => pair.Value.Count > 1 && pair
                .Value
                .Keys
                .Where(video => !video.ContainsIgnoreCase(@"\Delete\"))
                .DistinctBy(PathHelper.GetDirectoryName, StringComparer.OrdinalIgnoreCase)
                .Count() > 1)
            .ForEach(pair => pair
                .Value
                .Keys
                .Select(video => Path.Combine(settings.LibraryDirectory, video))
                .Order()
                .Append(string.Empty)
                .ForEach(log));
    }

    internal static void PrintDuplicateImdbId(Action<string>? log = null, params string[] directories)
    {
        log ??= Logger.WriteLine;

        List<string> noImdbId = [];
        directories
            .SelectMany(directory => Directory.EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .Where(metadata => !metadata.ContainsIgnoreCase($"{Path.DirectorySeparatorChar}Delete{Path.DirectorySeparatorChar}"))
            .Do(metadata =>
            {
                if (PathHelper.GetFileNameWithoutExtension(metadata).EqualsOrdinal(NotExistingFlag))
                {
                    noImdbId.Add(metadata);
                }
            })
            .GroupBy(metadata => ImdbMetadata.TryGet(metadata, out string? imdbId) ? imdbId : string.Empty)
            .Where(group => group.Count() > 1)
            .ForEach(group => group
                .Select(metadata => (metadata, Movie: PathHelper.GetFileName(PathHelper.GetDirectoryName(metadata))))
                .OrderBy(metadata => metadata.Movie)
                .ThenBy(metadata => metadata.metadata)
                .Select(metadata => $"{metadata.Movie} - {metadata.metadata}")
                .Append(string.Empty)
                .Prepend(group.Key)
                .ForEach(log));

        noImdbId
            .Select(PathHelper.GetDirectoryName)
            .Select(movie =>
            {
                string[] metadataFiles = Directory
                    .EnumerateFiles(movie, XmlMetadataSearchPattern)
                    .Order()
                    .ToArray();
                string tmdbId = metadataFiles
                    .Select(metadata => XDocument.Load(metadata).Root?.Element("tmdbid")?.Value ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Single();
                return (
                    Movie: PathHelper.GetFileName(movie),
                    Metadata: metadataFiles.First(),
                    TmdbId: tmdbId
                );
            })
            .GroupBy(metadata => metadata.TmdbId)
            .Where(group => group.Count() > 1)
            .ForEach(group => group
                .OrderBy(metadata => metadata.Movie)
                .ThenBy(metadata => metadata.Metadata)
                .Select(metadata => $"{PathHelper.GetFileName(metadata.Movie)} - {metadata.Metadata}")
                .Append(string.Empty)
                .Prepend(group.Key)
                .ForEach(log));
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
                    trimmedMovie = trimmedMovie["0.".Length..];
                }

                if (trimmedMovie.ContainsOrdinal("{"))
                {
                    trimmedMovie = trimmedMovie[..trimmedMovie.IndexOfOrdinal("{")];
                }

                if (!VideoDirectoryInfo.TryParse(trimmedMovie, out VideoDirectoryInfo? directoryInfo))
                {
                    log($"!Directory: {trimmedMovie}");
                    return;
                }

                //string[] allPaths = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
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

                if (Regex.IsMatch(trimmedMovie, "·[0-9]"))
                {
                    log($"!Special character ·: {trimmedMovie}");
                }

                DirectorySpecialCharacters.Where(trimmedMovie.Contains).ForEach(specialCharacter => log($"!Special character {specialCharacter}: {trimmedMovie}"));
                translations
                    .Where(translated => Regex.IsMatch(translated.Split(InstallmentSeparator).First(), "[0-9]+"))
                    .ForEach(translated => log($"!Translation has number {translated}: {movie}"));
                translations
                    .Where(translation => !string.IsNullOrEmpty(translation) && translation.All(character => character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' || char.IsPunctuation(character) || char.IsSeparator(character) || char.IsSymbol(character) || char.IsWhiteSpace(character)))
                    .Where(title => !TitlesWithNoTranslation.ContainsIgnoreCase(title))
                    .ForEach(translated => log($"!Title not translated {translated}: {movie}"));

                titles
                    .Where(title => title.IsNotNullOrWhiteSpace() && (char.IsWhiteSpace(title.First()) || char.IsWhiteSpace(title.Last())))
                    .ForEach(title => log($"!Title has white space {title}: {movie}"));

                //allPaths.Where(path => path.Length > 256).ForEach(path => log($"!Path too long: {path}"));

                string[] topFiles = Directory
                    .GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Select(PathHelper.GetFileName)
                    //.Where(file => !file.EndsWithIgnoreCase(".temp") && !file.EndsWithIgnoreCase(".tmp") && !file.EndsWithIgnoreCase(".tmp.txt"))
                    .ToArray();

                string imdbRating = ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata)
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

                string[] imdbFiles = topFiles.Where(IsImdbMetadata).ToArray();
                string[] cacheFiles = topFiles.Where(IsImdbCache).ToArray();

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

                    XDocument xmlMetadata = XDocument.Load(Path.Combine(movie, metadataFile));
                    string? metadataImdbId = xmlMetadata.Root?.Element("imdb_id")?.Value;
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
                        //string[] subtitles = topFiles.Where(IsSubtitle).ToArray();
                        //string[] metadataFiles = topFiles.Where(IsXmlMetadata).ToArray();

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
                VideoMovieFileInfo[] videoFileInfos = videos.Select(VideoMovieFileInfo.Parse).ToArray();
                string[] subtitles = topFiles.Where(IsSubtitle).ToArray();
                string[] metadataFiles = topFiles.Where(IsXmlMetadata).ToArray();
                string[] tmdbFiles = topFiles.Where(file => file.HasExtension(TmdbMetadata.Extension)).ToArray();
                string[] otherFiles = topFiles.Except(videos).Except(subtitles).Except(metadataFiles).Except(imdbFiles).Except(cacheFiles).Except(tmdbFiles).ToArray();

                if (tmdbFiles.Length < 1)
                {
                    log($"!TMDB file is missing. {movie}");
                }
                else if (tmdbFiles.Length > 1)
                {
                    log($"!TMDB file has duplicate. {movie}");
                }
                else
                {
                    string tmdbFile = tmdbFiles.Single();
                    string tmdbFileName = PathHelper.GetFileNameWithoutExtension(tmdbFile);
                    if (!tmdbFileName.EqualsOrdinal(NotExistingFlag) && tmdbFileName.Count(@char => @char == Delimiter.Single()) != 3)
                    {
                        log($"!TMDB file has wrong format. {tmdbFile}");
                    }
                }

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

                string source = VideoDirectoryInfo.GetSource(videoFileInfos);
                if (!source.EqualsOrdinal(directoryInfo.Source))
                {
                    log($"!Source {directoryInfo.Source} should be {source}: {movie}");
                }

                subtitles
                    .Where(subtitle => !(AllSubtitleExtensions.ContainsIgnoreCase(PathHelper.GetExtension(subtitle)) && videos.Any(video =>
                    {
                        string videoName = PathHelper.GetFileNameWithoutExtension(video);
                        string subtitleName = PathHelper.GetFileNameWithoutExtension(subtitle);
                        return subtitleName.EqualsOrdinal(videoName) || subtitle.StartsWithOrdinal($"{videoName}{Delimiter}") && allowedSubtitleLanguages.Any(allowedLanguage =>
                        {
                            string subtitleLanguage = subtitleName[$"{videoName}{Delimiter}".Length..];
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
                    Path.Combine(movie, metadataFile).TryLoadXmlImdbId(out string? metadataImdbId);
                    // string? metadataImdbRating = metadata.Root?.Element("rating")?.Value;
                    if (imdbMetadata is null)
                    {
                        if (metadataImdbId.IsNotNullOrWhiteSpace())
                        {
                            log($"!Metadata https://www.imdb.com/title/{metadataImdbId}/ should have no imdb id: {Path.Combine(movie, metadataFile)}");
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
                            log($"!Metadata imdb id {metadataImdbId} should be {imdbMetadata.ImdbId}: {Path.Combine(movie, metadataFile)}");
                        }
                    }
                });
            });

        if (isLoadingVideo)
        {
            PrintVideosWithErrors(settings, allVideos!, isNoAudioAllowed, false, log);
        }
    }

    internal static async Task PrintMovieVersions(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Metadata = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264720PMetadata = await settings.LoadMovieTopH264720PMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredMetadata>> preferredMetadata = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);
        HashSet<string> ignore = await settings.LoadIgnoredAsync(cancellationToken);

        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .ForEach(movie =>
            {
                string[] files = Directory.GetFiles(movie);
                if (!ImdbMetadata.TryGet(files, out string? _, out string? imdbId))
                {
                    log($"Missing or no JSON IMDB id: {movie}");
                    return;
                }

                (string File, string Name, VideoMovieFileInfo Info)[] videos = files
                    .Where(file => file.IsVideo())
                    .Select(file => (file, PathHelper.GetFileNameWithoutExtension(file), VideoMovieFileInfo.Parse(file)))
                    .ToArray();
                (string File, string Name, VideoMovieFileInfo Info)[] preferredVideos = videos
                    .Where(video => video.Info.GetEncoderType() is EncoderType.PreferredH264 or EncoderType.PreferredX265 or EncoderType.PreferredH264BluRay or EncoderType.PreferredX265BluRay)
                    .ToArray();

                PreferredMetadata[] availablePreferredMetadata = preferredMetadata
                    .TryGetValue(imdbId, out List<PreferredMetadata>? preferredResult)
                    ? preferredResult
                        .Where(metadata => !ignore.Contains(metadata.Link))
                        .ToArray()
                    : [];
                (PreferredMetadata Metadata, KeyValuePair<string, string> Version)[] otherPreferredMetadata = availablePreferredMetadata
                    .SelectMany(metadata => metadata.Availabilities, (metadata, version) => (metadata, version))
                    .Where(metadataVersion => !metadataVersion.version.Key.ContainsIgnoreCase("2160p"))
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
                            metadataVersion.Version.Key.Split(Delimiter).All(keyword => !preferredVideo.Name.ContainsIgnoreCase(keyword))))
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
                            if (VideoMovieFileInfo.TryParseIgnoreCase(metadata.Title, out VideoMovieFileInfo? video))
                            {
                                return video.GetEncoderType() is EncoderType.TopX265 or EncoderType.TopX265BluRay;
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
                            string[] videoEditions = video.Info.Edition.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
                            // (Not any) Local is equal to or better than remote metadata.
                            return video.Info.GetEncoderType() is EncoderType.TopX265 or EncoderType.TopX265BluRay
                                && (video.Name.StartsWithIgnoreCase(metadata.Title)
                                    || video.Info.Origin.ContainsIgnoreCase(".BluRay") && metadataTitle.ContainsIgnoreCase(".WEBRip.")
                                    || !video.Info.Edition.ContainsIgnoreCase("PREVIEW") && metadataTitle.ContainsIgnoreCase(".PREVIEW.")
                                    || !video.Info.Edition.ContainsIgnoreCase("DUBBED") && metadataTitle.ContainsIgnoreCase(".DUBBED.")
                                    || !video.Info.Edition.ContainsIgnoreCase("SUBBED") && (metadataTitle.ContainsIgnoreCase(".SUBBED.") || metadataTitle.ContainsIgnoreCase(".ENSUBBED."))
                                    || video.Info.Edition.ContainsIgnoreCase("DC") && metadataTitle.ContainsIgnoreCase(".THEATRICAL.")
                                    || video.Info.Edition.ContainsIgnoreCase("EXTENDED") && !metadataTitle.ContainsIgnoreCase(".EXTENDED.")
                                    || video.Info.Edition.IsNotNullOrWhiteSpace() && (video.Info with { Edition = string.Empty }).Name.StartsWithIgnoreCase(metadataTitle)
                                    || video.Info.Edition.ContainsIgnoreCase(".Part") && metadataTitle.ContainsIgnoreCase(".Part")
                                    || VideoMovieFileInfo.TryParseIgnoreCase(metadataTitle, out VideoMovieFileInfo? metadataInfo)
                                    && video.Info.Origin.EqualsIgnoreCase(metadataInfo.Origin)
                                    && video.Info.Edition.IsNotNullOrWhiteSpace()
                                    && metadataInfo
                                        .Edition
                                        .Split(Delimiter, StringSplitOptions.RemoveEmptyEntries)
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

                if (videos.Any(video => video.Info.GetEncoderType() is EncoderType.TopX265)) // Top X265 but not BluRay.
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

                if (videos.Any(video => video.Info.GetEncoderType() is EncoderType.TopX265BluRay or EncoderType.TopX265))
                {
                    return;
                }

                TopMetadata[] availableH264Metadata = h264Metadata.ContainsKey(imdbId)
                    ? h264Metadata[imdbId]
                        .Where(metadata =>
                        {
                            if (VideoMovieFileInfo.TryParseIgnoreCase(metadata.Title, out VideoMovieFileInfo? video))
                            {
                                return video.GetEncoderType() is EncoderType.TopH264 or EncoderType.TopH264BluRay;
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
                            string[] videoEditions = video.Info.Edition.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
                            // (Not any) Local is equal to or better than remote metadata.
                            return video.Info.GetEncoderType() is EncoderType.TopH264 or EncoderType.TopH264BluRay
                                && (video.Name.StartsWithIgnoreCase(metadata.Title)
                                    || video.Info.Origin.ContainsIgnoreCase(".BluRay") && metadataTitle.ContainsIgnoreCase(".WEBRip.")
                                    || !video.Info.Edition.ContainsIgnoreCase("PREVIEW") && metadataTitle.ContainsIgnoreCase(".PREVIEW.")
                                    || !video.Info.Edition.ContainsIgnoreCase("DUBBED") && metadataTitle.ContainsIgnoreCase(".DUBBED.")
                                    || !video.Info.Edition.ContainsIgnoreCase("SUBBED") && (metadataTitle.ContainsIgnoreCase(".SUBBED.") || metadataTitle.ContainsIgnoreCase(".ENSUBBED."))
                                    || video.Info.Edition.ContainsIgnoreCase("DC") && metadataTitle.ContainsIgnoreCase(".THEATRICAL.")
                                    || video.Info.Edition.ContainsIgnoreCase("EXTENDED") && !metadataTitle.ContainsIgnoreCase(".EXTENDED.")
                                    || video.Info.Edition.IsNotNullOrWhiteSpace() && (video.Info with { Edition = string.Empty }).Name.StartsWithIgnoreCase(metadataTitle)
                                    || video.Info.Edition.ContainsIgnoreCase(".Part") && metadataTitle.ContainsIgnoreCase(".Part")
                                    || VideoMovieFileInfo.TryParseIgnoreCase(metadataTitle, out VideoMovieFileInfo? metadataInfo)
                                    && video.Info.Origin.EqualsIgnoreCase(metadataInfo.Origin)
                                    && video.Info.Edition.IsNotNullOrWhiteSpace()
                                    && metadataInfo
                                        .Edition
                                        .Split(Delimiter, StringSplitOptions.RemoveEmptyEntries)
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

                if (videos.Any(video => video.Info.GetEncoderType() is EncoderType.TopH264)) // Top H264 but not BluRay.
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

                if (videos.Any(video => video.Info.GetEncoderType() is EncoderType.TopH264BluRay or EncoderType.TopH264))
                {
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

    internal static async Task PrintTVVersions(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadTVTopX265MetadataAsync(cancellationToken);

        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .ForEach(tv =>
            {
                string[] topFiles = Directory.GetFiles(tv);
                if (!ImdbMetadata.TryGet(topFiles, out string? _, out string? imdbId))
                {
                    log($"Missing or no IMDB id: {tv}");
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
                            .All(episode => VideoEpisodeFileInfo.TryParse(episode, out VideoEpisodeFileInfo? parsed) && parsed.GetEncoderType() is EncoderType.TopX265 && !(parsed.Origin.ContainsIgnoreCase("WEBRip") && seasonMetadata.Title.ContainsIgnoreCase("BluRay"))))
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

    internal static async Task PrintSpecialTitles(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        string[] specialImdbIds = await settings.LoadMovieImdbSpecialMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> x265Summaries = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Summaries = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredMetadata>> preferredDetails = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

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

    internal static async Task PrintHighRatingAsync(ISettings settings, string threshold = "8.0", string[]? excludedGenres = null, Action<string>? log = null, CancellationToken cancellationToken = default, params string[] directories)
    {
        log ??= Logger.WriteLine;
        HashSet<string> existingImdbIds = new(
            directories.SelectMany(directory => Directory
                .EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
                .Select(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty))
                .Where(imdbId => imdbId.IsNotNullOrWhiteSpace()),
            StringComparer.OrdinalIgnoreCase);
        Dictionary<string, TopMetadata[]> x265Summaries = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Summaries = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredMetadata>> preferredDetails = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

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

                highRatings[summaries.Key]["Preferred"] = summaries.Value.ToArray();
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

                if (!ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata))
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

                string[] imdbTitles = imdbTitle.Split(TitleSeparator)
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
                        HttpUtility.HtmlDecode(imdbMetadata.Title).Replace(TitleSeparator, " ").FilterForFileSystem(),
                        imdbTitle.Replace("(", string.Empty).Replace(")", string.Empty),
                        Regex.Replace(imdbTitle, @"\(.+\)", string.Empty).Trim(),
                        imdbTitle.Replace("...", string.Empty),
                        imdbTitle.Replace("#", "No "),
                        imdbTitle,
                        imdbTitle.Replace(TitleSeparator, " "),
                        imdbTitle.Replace(" · ", " "),
                        imdbTitle.Replace(" - ", " "),
                        imdbTitle.Replace(" - ", TitleSeparator),
                        imdbTitle.ReplaceIgnoreCase(" Vol ", TitleSeparator),
                        imdbTitle.ReplaceIgnoreCase(" Chapter ", TitleSeparator),
                        imdbTitle.Replace(TitleSeparator, " "),
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
                    parsed.DefaultTitle1.Replace(InstallmentSeparator, TitleSeparator),
                    parsed.DefaultTitle1.Split(InstallmentSeparator).First(),
                    $"{parsed.DefaultTitle1.Split(InstallmentSeparator).First()}{parsed.DefaultTitle2}",
                    $"{parsed.DefaultTitle1.Split(InstallmentSeparator).First()}{parsed.DefaultTitle2.Replace(TitleSeparator, " ")}",
                    parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()),
                    parsed.DefaultTitle3.TrimStart(TitleSeparator.Single()),
                    parsed.OriginalTitle1.TrimStart('='),
                    $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}",
                    $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(TitleSeparator, " "),
                    $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(TitleSeparator, " ").Replace(InstallmentSeparator, " "),
                    $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(InstallmentSeparator, " "),
                    $"{parsed.DefaultTitle1}{parsed.DefaultTitle2}".Replace(InstallmentSeparator, TitleSeparator),
                    $"{parsed.DefaultTitle1.Split(InstallmentSeparator).First()}{parsed.DefaultTitle2.Split(InstallmentSeparator).First()}".Replace(TitleSeparator, " "),
                    parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).Replace(InstallmentSeparator, " "),
                    parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).Replace(InstallmentSeparator, TitleSeparator),
                    parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).Split(InstallmentSeparator).First(),
                    $"{parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).Split(InstallmentSeparator).First()}{parsed.DefaultTitle3.Split(InstallmentSeparator).First()}",
                    $"{parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).Split(InstallmentSeparator).First()}{parsed.DefaultTitle3.Split(InstallmentSeparator).First()}".Replace(TitleSeparator, " ")
                ];
                if (parsed.DefaultTitle2.IsNotNullOrWhiteSpace())
                {
                    localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant()[0]}{parsed.DefaultTitle2.TrimStart(TitleSeparator.Single())[1..]}");
                    localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant()[0]}{parsed.DefaultTitle2.TrimStart(TitleSeparator.Single())[1..].Replace(InstallmentSeparator, " ")}");
                    localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant()}");
                    localTitles.Add($"{parsed.DefaultTitle1} {parsed.DefaultTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant().Replace(InstallmentSeparator, " ")}");
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

                if (!ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata))
                {
                    log($"!Missing metadata {movie}");
                    return;
                }

                string imdbTitle = HttpUtility.HtmlDecode(imdbMetadata.OriginalTitle);
                if (string.IsNullOrEmpty(imdbTitle))
                {
                    return;
                }

                string[] imdbTitles = imdbTitle.Split(TitleSeparator)
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
                        HttpUtility.HtmlDecode(imdbMetadata.Title).Replace(TitleSeparator, " ").FilterForFileSystem(),
                        imdbTitle.Replace("(", string.Empty).Replace(")", string.Empty),
                        Regex.Replace(imdbTitle, @"\(.+\)", string.Empty).Trim(),
                        imdbTitle.Replace("...", string.Empty),
                        imdbTitle.Replace("#", "No "),
                        imdbTitle,
                        imdbTitle.Replace(TitleSeparator, " "),
                        imdbTitle.Replace(" · ", " "),
                        imdbTitle.Replace(" - ", " "),
                        imdbTitle.Replace(" - ", TitleSeparator),
                        imdbTitle.ReplaceIgnoreCase(" Vol ", TitleSeparator),
                        imdbTitle.ReplaceIgnoreCase(" Chapter ", TitleSeparator),
                        imdbTitle.Replace(TitleSeparator, " "),
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
                    parsed.OriginalTitle1.Replace(InstallmentSeparator, TitleSeparator),
                    parsed.OriginalTitle1.Split(InstallmentSeparator).First(),
                    $"{parsed.OriginalTitle1.Split(InstallmentSeparator).First()}{parsed.OriginalTitle2}",
                    $"{parsed.OriginalTitle1.Split(InstallmentSeparator).First()}{parsed.OriginalTitle2.Replace(TitleSeparator, " ")}",
                    parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()),
                    parsed.OriginalTitle3.TrimStart(TitleSeparator.Single()),
                    parsed.OriginalTitle1.TrimStart('='),
                    $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}",
                    $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(TitleSeparator, " "),
                    $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(TitleSeparator, " ").Replace(InstallmentSeparator, " "),
                    $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(InstallmentSeparator, " "),
                    $"{parsed.OriginalTitle1}{parsed.OriginalTitle2}".Replace(InstallmentSeparator, TitleSeparator),
                    $"{parsed.OriginalTitle1.Split(InstallmentSeparator).First()}{parsed.OriginalTitle2.Split(InstallmentSeparator).First()}".Replace(TitleSeparator, " "),
                    parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).Replace(InstallmentSeparator, " "),
                    parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).Replace(InstallmentSeparator, TitleSeparator),
                    parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).Split(InstallmentSeparator).First(),
                    $"{parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).Split(InstallmentSeparator).First()}{parsed.OriginalTitle3.Split(InstallmentSeparator).First()}",
                    $"{parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).Split(InstallmentSeparator).First()}{parsed.OriginalTitle3.Split(InstallmentSeparator).First()}".Replace(TitleSeparator, " ")
                ];
                if (parsed.OriginalTitle2.IsNotNullOrWhiteSpace())
                {
                    localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant()[0]}{parsed.OriginalTitle2.TrimStart(TitleSeparator.Single())[1..]}");
                    localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant()[0]}{parsed.OriginalTitle2.TrimStart(TitleSeparator.Single())[1..].Replace(InstallmentSeparator, " ")}");
                    localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant()}");
                    localTitles.Add($"{parsed.OriginalTitle1} {parsed.OriginalTitle2.TrimStart(TitleSeparator.Single()).ToLowerInvariant().Replace(InstallmentSeparator, " ")}");
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
            .Select(file => (File: file, Metadata: ImdbMetadata.TryLoad(file, out ImdbMetadata? imdbMetadata) ? imdbMetadata : null))
            .Where(metadata => metadata.Metadata?.Genres.ContainsIgnoreCase(genre) is true)
            .ForEach(metadata => log($"{PathHelper.GetDirectoryName(metadata.File)}"));
    }

    internal static void PrintMovieRegionsWithErrors(ISettings settings, Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level - 1))
            .OrderBy(subDirectory => subDirectory)
            .ForEach(subDirectory =>
            {
                string subDirectoryName = PathHelper.GetFileName(subDirectory);
                if (subDirectoryName.ContainsIgnoreCase("Delete") || subDirectoryName.ContainsIgnoreCase("Temp") || subDirectoryName.ContainsIgnoreCase("Test"))
                {
                    return;
                }

                if (!settings.MovieRegions.TryGetValue(subDirectoryName, out string[]? allowed))
                {
                    log($"!Unkown directory {subDirectory}");
                    return;
                }

                log($"==={subDirectory}==={string.Join(", ", allowed)}");
                List<string> allowedRegions = [];
                List<string> allowedGenres = [];
                List<string> allowedLanguages = [];
                List<string> ignoredFranchises = [];
                allowed.ForEach(item =>
                {
                    if (item.StartsWithIgnoreCase("Genre:"))
                    {
                        allowedGenres.Add(item.Split(":").Last());
                        return;
                    }

                    if (item.StartsWithIgnoreCase("Language:"))
                    {
                        allowedLanguages.Add(item.Split(":").Last());
                        return;
                    }

                    if (item.StartsWithIgnoreCase("Franchise:"))
                    {
                        ignoredFranchises.Add(item.Split(":").Last());
                        return;
                    }

                    allowedRegions.Add(item);
                });

                Directory
                    .GetDirectories(subDirectory)
                    .ForEach(movie =>
                    {
                        string movieName = PathHelper.GetFileName(movie);
                        if (ignoredFranchises.Any(movieName.StartsWithIgnoreCase))
                        {
                            return;
                        }

                        if (ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata))
                        {
                            if (imdbMetadata.Regions.Any() && imdbMetadata.Regions.Intersect(allowedRegions).IsEmpty()
                                && imdbMetadata.Languages.Intersect(allowedLanguages).IsEmpty())
                            {
                                log(movie);
                                log($"{string.Join(", ", allowedRegions)}==={string.Join(", ", imdbMetadata.Regions)}");
                                log(string.Empty);
                                return;
                            }

                            if (allowedGenres.Any() && imdbMetadata.Genres.Intersect(allowedGenres).IsEmpty())
                            {
                                log(movie);
                                log($"{string.Join(", ", allowedGenres)}==={string.Join(", ", imdbMetadata.Genres)}");
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
                    ImdbMetadata.TryLoad(m, out ImdbMetadata? meta);
                    log(meta?.ImdbId ?? NotExistingFlag);
                    log(m);
                    log("");
                }
            });
    }

    internal static async Task PrintMovieImdbIdErrorsAsync(ISettings settings, bool ignoreJsonImdbId = false, Action<string>? log = null, CancellationToken cancellationToken = default, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Metadata = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> x265XMetadata = await settings.LoadMovieTopX265XMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264XMetadata = await settings.LoadMovieTopH264XMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredFileMetadata>> preferredFileMetadata = await settings.LoadMoviePreferredFileMetadataAsync(cancellationToken);
        HashSet<string> topDuplications = new(settings.MovieTopDuplications, StringComparer.OrdinalIgnoreCase);

        Dictionary<string, string> x265TitlesToImdbIds = x265Metadata
            .SelectMany(pair => pair.Value)
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        Dictionary<string, string> x265XTitlesToImdbIds = x265XMetadata
            .SelectMany(pair => pair.Value)
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        Dictionary<string, string> h264TitlesToImdbIds = h264Metadata
            .Where(pair => pair.Key.IsNotNullOrWhiteSpace())
            .SelectMany(pair => pair.Value)
            .Where(metadata => !topDuplications.Contains(metadata.Title))
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        Dictionary<string, string> h264XTitlesToImdbIds = h264XMetadata
            .SelectMany(pair => pair.Value)
            .ToDictionary(metadata => metadata.Title, metadata => metadata.ImdbId);
        ILookup<string, string> preferredTitlesToImdbIds = preferredFileMetadata
            .SelectMany(pair => pair.Value)
            .ToLookup(
                metadata => metadata.File
                    .ReplaceIgnoreCase($".{settings.PreferredOldKeyword}", $"{VersionSeparator}{settings.PreferredOldKeyword}")
                    .ReplaceIgnoreCase(".BRRip.", ".BluRay.")
                    .ReplaceIgnoreCase(".1080.BluRay.", ".1080p.BluRay."),
                metadata => metadata.ImdbId);

        directories
            .SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .Select(movie => (Directory: movie, HasJson: ImdbMetadata.TryRead(movie, out string? jsonImdbId, out _, out _, out _, out _), JsonImdbId: jsonImdbId))
            .ForEach(movie =>
            {
                Dictionary<string, (string File, string XmlImdbId, string XmlTitle)> xmlDocuments = Directory
                    .EnumerateFiles(movie.Directory, XmlMetadataSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(file => !PathHelper.GetFileNameWithoutExtension(file).EqualsIgnoreCase("movie"))
                    .Select(file => (Metadata: XDocument.Load(file), file))
                    .Select(xml => (
                        xml.file,
                        xml.Metadata.TryGetImdbId(out string? imdbId) ? imdbId : string.Empty,
                        xml.Metadata.TryGetTitle(out string? title) ? title : string.Empty))
                    .ToDictionary(xml => PathHelper.GetFileNameWithoutExtension(xml.file), StringComparer.OrdinalIgnoreCase);
                if (xmlDocuments.IsEmpty())
                {
                    log($"!XML is missing: {movie.Directory}");
                    return;
                }

                VideoMovieFileInfo[] videos = VideoDirectoryInfo.GetMovies(movie.Directory).ToArray();
                if (videos.IsEmpty())
                {
                    log($"!Video is missing: {movie.Directory}");
                    return;
                }

                if (!xmlDocuments
                        .Keys
                        .Order()
                        .SequenceEqual(videos.Where(video => video.Part is "" or ".cd1" or ".cd01").Select(video => PathHelper.GetFileNameWithoutExtension(video.Name)).Order()))
                {
                    log($"!Video is inconsistent with XML: {movie.Directory}");
                    return;
                }

                VideoMovieFileInfo[] x265Videos = videos.Where(video => video.GetEncoderType() is EncoderType.TopX265 or EncoderType.TopX265BluRay).ToArray();
                VideoMovieFileInfo[] h264Videos = videos.Where(video => video.GetEncoderType() is EncoderType.TopH264 or EncoderType.TopH264BluRay && video.GetDefinitionType() == DefinitionType.P1080).ToArray();
                VideoMovieFileInfo[] preferredVideos = videos.Where(video => video.GetEncoderType() is EncoderType.PreferredH264 or EncoderType.PreferredH264BluRay or EncoderType.PreferredX265 or EncoderType.PreferredX265BluRay).ToArray();

                if (x265Videos.Any())
                {
                    x265Videos.ForEach(x265Video =>
                    {
                        (string File, string XmlImdbId, string XmlTitle) xml = xmlDocuments[PathHelper.GetFileNameWithoutExtension(x265Video.Name)];

                        string x265Title = x265TitlesToImdbIds.Keys.FirstOrDefault(key => x265Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (x265Title.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = x265TitlesToImdbIds[x265Title];
                            if (!remoteImdbId.EqualsIgnoreCase(xml.XmlImdbId))
                            {
                                log($"!XML IMDB id {xml.XmlImdbId} should be {remoteImdbId} for '{xml.XmlTitle}': {xml.File}");
                            }

                            return;
                        }

                        string x265XTitle = x265XTitlesToImdbIds.Keys.FirstOrDefault(key => x265Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (x265XTitle.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = x265XTitlesToImdbIds[x265XTitle];
                            if (!remoteImdbId.EqualsIgnoreCase(xml.XmlImdbId))
                            {
                                log($"!XML IMDB id {xml.XmlImdbId} should be {remoteImdbId} for '{xml.XmlTitle}': {xml.File}");
                            }

                            return;
                        }

                        log($"-{xml.XmlImdbId} with title {x265Video.Name} is missing in x265 for '{xml.XmlTitle}': {movie.Directory}");
                    });
                }

                if (h264Videos.Any())
                {
                    h264Videos.ForEach(h264Video =>
                    {
                        (string File, string XmlImdbId, string XmlTitle) xml = xmlDocuments[PathHelper.GetFileNameWithoutExtension(h264Video.Name)];

                        string h264Title = h264TitlesToImdbIds.Keys.FirstOrDefault(key => h264Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (h264Title.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = h264TitlesToImdbIds[h264Title];
                            if (!remoteImdbId.EqualsIgnoreCase(xml.XmlImdbId))
                            {
                                log($"!XML IMDB id {xml.XmlImdbId} should be {remoteImdbId} for '{xml.XmlTitle}': {xml.File}");
                            }

                            return;
                        }

                        string h264XTitle = h264XTitlesToImdbIds.Keys.FirstOrDefault(key => h264Video.Name.StartsWithIgnoreCase(key), string.Empty);
                        if (h264XTitle.IsNotNullOrWhiteSpace())
                        {
                            string remoteImdbId = h264XTitlesToImdbIds[h264XTitle];
                            if (!remoteImdbId.EqualsIgnoreCase(xml.XmlImdbId))
                            {
                                log($"!XML IMDB id {xml.XmlImdbId} should be {remoteImdbId} for '{xml.XmlTitle}': {xml.File}");
                            }

                            return;
                        }

                        log($"-{xml.XmlImdbId} with title {h264Video.Name} is missing in H264 for '{xml.XmlTitle}': {movie.Directory}");
                    });
                }

                if (preferredVideos.Any())
                {
                    preferredVideos.ForEach(preferredVideo =>
                    {
                        (string File, string XmlImdbId, string XmlTitle) xml = xmlDocuments[PathHelper.GetFileNameWithoutExtension(preferredVideo.Name)];

                        string[] preferredTitles = preferredTitlesToImdbIds
                            .Select(group => group.Key)
                            .Where(key => preferredVideo.Name.StartsWithIgnoreCase(key))
                            .ToArray();
                        if (preferredTitles.Any() && preferredTitles.All(preferredTitle => preferredTitle.IsNotNullOrWhiteSpace()))
                        {
                            string[] remoteImdbIds = preferredTitles.SelectMany(preferredTitle => preferredTitlesToImdbIds[preferredTitle]).ToArray();
                            if (!remoteImdbIds.ContainsIgnoreCase(xml.XmlImdbId))
                            {
                                log($"!XML IMDB id {xml.XmlImdbId} should be {string.Join("|", remoteImdbIds)} for '{xml.XmlTitle}': {xml.File}");
                            }

                            return;
                        }

                        log($"-{xml.XmlImdbId} with title {preferredVideo.Name} is missing in preferred for '{xml.XmlTitle}': {movie.Directory}");
                    });
                }

                if (ignoreJsonImdbId)
                {
                    return;
                }

                if (!movie.HasJson)
                {
                    x265Videos
                        .Concat(h264Videos)
                        .ForEach(video => log($"!Json is missing for {video.Name}: {movie.Directory}"));

                    xmlDocuments
                        .Values
                        .Where(xml => xml.XmlImdbId.IsNotNullOrWhiteSpace())
                        .ForEach(xml => log($"!XML IMDB id {xml.XmlImdbId} should be empty for {xml.XmlTitle}: {xml.File}."));
                }
                else
                {
                    Debug.Assert(movie.JsonImdbId.IsNotNullOrWhiteSpace());

                    xmlDocuments
                        .Values
                        .Where(xml => !xml.XmlImdbId.EqualsIgnoreCase(movie.JsonImdbId))
                        .ForEach(xml => log($"!Xml IMDB id {xml.XmlImdbId} should be {movie.JsonImdbId} for {xml.XmlTitle}: {xml.File}"));
                }
            });
    }

    internal static void PrintNikkatsu(Action<string>? log = null, params (string Directory, int Level)[] directories)
    {
        log ??= Logger.WriteLine;
        directories.SelectMany(directory => EnumerateDirectories(directory.Directory, directory.Level))
            .Where(movie => !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Nikkatsu") && !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Oniroku Dan") && !PathHelper.GetFileName(movie).StartsWithIgnoreCase("Angel Guts"))
            .Select(movie => (movie, XDocument.Load(Directory.EnumerateFiles(movie, XmlMetadataSearchPattern).First())))
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
        ISettings settings, Func<ImdbMetadata, HashSet<string>, bool> predicate, string initialUrl = "", bool updateMetadata = false, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ILookup<string, string> topMagnetUris = (await File.ReadAllLinesAsync(settings.TopMagnetUrls, cancellationToken))
            .ToLookup(line => MagnetUri.Parse(line).DisplayName, StringComparer.OrdinalIgnoreCase);
        ConcurrentDictionary<string, ImdbMetadata> mergedMetadata = await settings.LoadMovieMergedMetadataAsync(cancellationToken);

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> libraryMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Metadata = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        //Dictionary<string, TopMetadata[]> h264720PMetadata = await settings.LoadMovieTopH264720PMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredMetadata>> preferredMetadata = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);
        //Dictionary<string, RareMetadata> rareMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, RareMetadata>>(rareJsonPath);
        //string[] metadataFiles = Directory.GetFiles(settings.MovieMetadataDirectory);
        //string[] cacheFiles = Directory.GetFiles(settings.MovieMetadataCacheDirectory);
        HashSet<string> keywords = new(settings.ImdbKeywords, StringComparer.OrdinalIgnoreCase);
        //Dictionary<string, string> metadataFilesByImdbId = metadataFiles.ToDictionary(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        ImdbMetadata[] imdbIds = x265Metadata.Keys
            .Concat(h264Metadata.Keys)
            .Concat(preferredMetadata.Keys)
            //.Concat(h264720PMetadata.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Except(libraryMetadata.Keys, StringComparer.OrdinalIgnoreCase)
            //.Intersect(rareMetadata
            //    .SelectMany(rare => Regex
            //        .Matches(rare.Value.Content, @"imdb\.com/title/(tt[0-9]+)")
            //        .Where(match => match.Success)
            //        .Select(match => match.Groups[1].Value)))
            //.Intersect(mergedMetadata.Keys)
            .Select(imdbId => mergedMetadata.GetValueOrDefault(imdbId))
            .NotNull()
            .Where(imdbMetadata => predicate(imdbMetadata, keywords))
            .OrderBy(imdbMetadata => imdbMetadata.ImdbId)
            .ToArray();
        int length = imdbIds.Length;
        log(length.ToString());
        log(x265Metadata.Keys.Intersect(imdbIds.Select(metadata => metadata.ImdbId)).Count().ToString());

        keywords
            .Select(keyword => (keyword, imdbIds.Count(imdbId => imdbId.AllKeywords.ContainsIgnoreCase(keyword))))
            .OrderByDescending(keyword => keyword.Item2)
            .ForEach(keyword => log($"{keyword.Item2} - {keyword}"));

        //HashSet<string> downloadedTitles = new(
        //    new string[] { }.SelectMany(Directory.GetDirectories).Select(Path.GetFileName)!,
        //    StringComparer.OrdinalIgnoreCase);
        //using WebDriverWrapper? webDriver = isDryRun ? null : new(() => WebDriverHelper.Start(isLoadingAll: true), initialUrl);
        using HttpClient? httpClient = isDryRun ? null : new HttpClient().AddEdgeHeaders();
        //if (!isDryRun && initialUrl.IsNotNullOrWhiteSpace())
        //{
        //    webDriver!.Url = initialUrl;
        //    webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.Id("pager_links")));
        //}

        //if (updateMetadata)
        //{
        //    HashSet<string> metadataDirectoryImdbIds = new(metadataFiles.Select(file => file.GetImdbId()), StringComparer.OrdinalIgnoreCase);
        //    await imdbIds
        //        .Select(imdbId => imdbId.ImdbId)
        //        .Where(imdbId => !metadataDirectoryImdbIds.Contains(imdbId))
        //        .ForEachAsync(async (imdbId, index) =>
        //        {
        //            log($"{index * 100 / length}% - {index}/{length} - {imdbId}");
        //            try
        //            {
        //                await Retry.FixedIntervalAsync(
        //                    async () => await DownloadImdbMetadataAsync(imdbId, settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, metadataFiles, cacheFiles, webDriver, overwrite: true, useCache: false, log: log),
        //                    isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.InternalServerError });
        //            }
        //            catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
        //            {
        //                log($"!!!{imdbId} {exception}");
        //            }
        //        });
        //}

        await imdbIds
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .ForEachAsync(async (imdbMetadata, index, token) =>
            {
                //log($"{index * 100 / length}% - {index}/{length} - {imdbMetadata.ImdbId}");
                //if (x265Metadata.TryGetValue(imdbMetadata.ImdbId, out TopMetadata[]? x265Videos))
                //{
                //    List<TopMetadata> excluded = [];
                //    if (x265Videos.Length > 1)
                //    {
                //        if (x265Videos.Any(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")) && x265Videos.Any(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
                //        {
                //            excluded.AddRange(x265Videos.Where(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))));
                //            x265Videos = x265Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                //        }

                //        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && x265Videos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                //        {
                //            excluded.AddRange(x265Videos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                //            x265Videos = x265Videos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                //        }

                //        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".FRENCH.")) && x265Videos.Any(video => video.Title.ContainsIgnoreCase(".DUBBED.")))
                //        {
                //            excluded.AddRange(x265Videos.Where(video => video.Title.ContainsIgnoreCase(".DUBBED.")));
                //            x265Videos = x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".DUBBED.")).ToArray();
                //        }

                //        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".EXTENDED.")) && x265Videos.Any(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")))
                //        {
                //            excluded.AddRange(x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")));
                //            x265Videos = x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")).ToArray();
                //        }

                //        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".REMASTERED.")) && x265Videos.Any(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")))
                //        {
                //            excluded.AddRange(x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")));
                //            x265Videos = x265Videos.Where(video => video.Title.ContainsIgnoreCase(".REMASTERED.")).ToArray();
                //        }

                //        if (x265Videos.Any(video => video.Title.ContainsIgnoreCase(".PROPER.")) && x265Videos.Any(video => !video.Title.ContainsIgnoreCase(".PROPER.")))
                //        {
                //            excluded.AddRange(x265Videos.Where(video => !video.Title.ContainsIgnoreCase(".PROPER.")));
                //            x265Videos = x265Videos.Where(video => video.Title.ContainsIgnoreCase(".PROPER.")).ToArray();
                //        }
                //    }

                //    if (x265Videos.Any(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")))
                //    {
                //        excluded.AddRange(x265Videos.Where(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")));
                //        x265Videos = x265Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                //    }

                //    bool hasDownload = false;
                //    await x265Videos.ForEachAsync(async metadata =>
                //    {
                //        if (isDryRun)
                //        {
                //            //log($"{metadata.ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{metadata.Title} {metadata.Link} {imdbMetadata.Link}");
                //            //log($"{imdbMetadata.Link}keywords");
                //            //log($"{imdbMetadata.Link}parentalguide");
                //            if (topMagnetUris.Contains(metadata.Title))
                //            {
                //                string[] uris = topMagnetUris[metadata.Title].ToArray();
                //                if (uris.Length > 0)
                //                {
                //                    MagnetUri[] result = uris
                //                        .Take(..)
                //                        .Select(MagnetUri.Parse)
                //                        //.Where(uri => !downloadingLinks.Any(downloadingUri => downloadingUri.ExactTopic.EqualsIgnoreCase(uri.ExactTopic) || downloadingUri.DisplayName.EqualsIgnoreCase(uri.DisplayName)))
                //                        .ToArray();
                //                    if (result.Any())
                //                    {
                //                        hasDownload = true;
                //                        result
                //                            .Select(uri => uri.ToString())
                //                            //.Append(string.Empty)
                //                            .ForEach(log);
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                //log($"!!! Cannot find magnet for title {metadata.Title}");
                //            }

                //            return;
                //        }

                //        //string cacheFile = Path.Combine(settings.MovieMetadataCacheDirectory, $"{metadata.ImdbId}-{metadata.Title}{ImdbCacheExtension}");
                //        //string html = cacheFiles.ContainsIgnoreCase(cacheFile) && new FileInfo(cacheFile).LastWriteTimeUtc > DateTime.UtcNow - TimeSpan.FromDays(1)
                //        //    ? await File.ReadAllTextAsync(cacheFile, token)
                //        //    : await webDriver!.GetStringAsync(metadata.Link, () => webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))), cancellationToken: token);
                //        //await Task.Delay(WebDriverHelper.DefaultDomWait, token);
                //        //await File.WriteAllTextAsync(cacheFile, html, token);
                //        //(string _, string _, string magnetUrl) = TopGetUrls(html, metadata.Link);
                //        //log(magnetUrl);
                //        //webDriver!.Url = httpUrl;
                //        //await Task.Delay(TimeSpan.FromSeconds(3));
                //    }, cancellationToken: token);

                //    if (hasDownload)
                //    {
                //        return;
                //    }
                //}

                //if (h264Metadata.TryGetValue(imdbMetadata.ImdbId, out TopMetadata[]? h264Videos))
                //{
                //    List<TopMetadata> excluded = [];
                //    if (h264Videos.Length > 1)
                //    {
                //        if (h264Videos.Any(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")) && h264Videos.Any(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
                //        {
                //            excluded.AddRange(h264Videos.Where(video => !(video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))));
                //            h264Videos = h264Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                //        }

                //        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase("BluRay")) && h264Videos.Any(video => video.Title.ContainsIgnoreCase("WEBRip")))
                //        {
                //            excluded.AddRange(h264Videos.Where(video => video.Title.ContainsIgnoreCase("WEBRip")));
                //            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase("BluRay")).ToArray();
                //        }

                //        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".FRENCH.")) && h264Videos.Any(video => video.Title.ContainsIgnoreCase(".DUBBED.")))
                //        {
                //            excluded.AddRange(h264Videos.Where(video => video.Title.ContainsIgnoreCase(".DUBBED.")));
                //            h264Videos = h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".DUBBED.")).ToArray();
                //        }

                //        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".EXTENDED.")) && h264Videos.Any(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")))
                //        {
                //            excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")));
                //            h264Videos = h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".EXTENDED.")).ToArray();
                //        }

                //        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".REMASTERED.")) && h264Videos.Any(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")))
                //        {
                //            excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".REMASTERED.")));
                //            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase(".REMASTERED.")).ToArray();
                //        }

                //        if (h264Videos.Any(video => video.Title.ContainsIgnoreCase(".PROPER.")) && h264Videos.Any(video => !video.Title.ContainsIgnoreCase(".PROPER.")))
                //        {
                //            excluded.AddRange(h264Videos.Where(video => !video.Title.ContainsIgnoreCase(".PROPER.")));
                //            h264Videos = h264Videos.Where(video => video.Title.ContainsIgnoreCase(".PROPER.")).ToArray();
                //        }
                //    }

                //    if (h264Videos.Any(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")))
                //    {
                //        excluded.AddRange(h264Videos.Where(video => !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") && !video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")));
                //        h264Videos = h264Videos.Where(video => video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || video.Title.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")).ToArray();
                //    }

                //    bool hasDownload = false;
                //    await h264Videos.ForEachAsync(async metadata =>
                //    {
                //        if (isDryRun)
                //        {
                //            //log($"{metadata.ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{metadata.Title} {metadata.Link} {imdbMetadata.Link}");
                //            //log($"{imdbMetadata.Link}keywords");
                //            //log($"{imdbMetadata.Link}parentalguide");
                //            if (topMagnetUris.Contains(metadata.Title))
                //            {
                //                string[] uris = topMagnetUris[metadata.Title].ToArray();
                //                if (uris.Length > 0)
                //                {
                //                    MagnetUri[] result = uris
                //                        .Take(..)
                //                        .Select(MagnetUri.Parse)
                //                        //.Where(uri => !downloadingLinks.Any(downloadingUri => downloadingUri.ExactTopic.EqualsIgnoreCase(uri.ExactTopic) || downloadingUri.DisplayName.EqualsIgnoreCase(uri.DisplayName)))
                //                        .ToArray();
                //                    if (result.Any())
                //                    {
                //                        hasDownload = true;
                //                        result
                //                            .Select(uri => uri.ToString())
                //                            //.Append(string.Empty)
                //                            .ForEach(log);
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                //log($"!!! Cannot find magnet for title {metadata.Title}");
                //            }

                //            return;
                //        }

                //        //string cacheFile = Path.Combine(settings.MovieMetadataCacheDirectory, $"{metadata.ImdbId}-{metadata.Title}{ImdbCacheExtension}");
                //        //string html = cacheFiles.ContainsIgnoreCase(cacheFile) && new FileInfo(cacheFile).LastWriteTimeUtc > DateTime.UtcNow - TimeSpan.FromDays(1)
                //        //    ? await File.ReadAllTextAsync(cacheFile, token)
                //        //    : await webDriver!.GetStringAsync(metadata.Link, () => webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))), cancellationToken: token);
                //        //await Task.Delay(WebDriverHelper.DefaultDomWait, token);
                //        //await File.WriteAllTextAsync(cacheFile, html, token);
                //        //(string _, string _, string magnetUrl) = TopGetUrls(html, metadata.Link);
                //        //log(magnetUrl);
                //        //webDriver!.Url = httpUrl;
                //        //await Task.Delay(TimeSpan.FromSeconds(3));
                //    }, cancellationToken: token);

                //    if (hasDownload)
                //    {
                //        return;
                //    }
                //}

                if (preferredMetadata.TryGetValue(imdbMetadata.ImdbId, out List<PreferredMetadata>? preferredVideos))
                {
                    await preferredVideos
                        .Do(preferredMetadata => Debug.Assert(preferredMetadata.ImdbId.EqualsIgnoreCase(imdbMetadata.ImdbId)))
                        .ForEachAsync(async preferredMetadata =>
                        {
                            await Task.Yield();
                            //await Preferred.DownloadTorrentsAsync(settings, preferredMetadata, null, isDryRun, log, token);
                            preferredMetadata.PreferredAvailabilities
                                .Select(availability => Path.Combine(settings.MovieMetadataCacheDirectory, $"{preferredMetadata.ImdbId}.{availability.Value.Split("/").Last()}{TorrentHelper.TorrentExtension}"))
                                .Do(log)
                                .ForEach(file =>
                                {
                                    if (File.Exists(file))
                                    {
                                        FileHelper.CopyToDirectory(file, settings.LibraryDirectory, true, true);
                                    }
                                    else
                                    {
                                        log($"magnet:?xt=urn:btih:{PathHelper.GetFileNameWithoutExtension(file).Split(".").Last()}");
                                    }
                                });
                        }, token);
                }

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
            }, cancellationToken: cancellationToken);
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
        bool isDryRun = false, bool updateMetadata = false, Action<string>? log = null,
        CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadTVTopX265MetadataAsync(cancellationToken);
        Dictionary<string, string> metadataFiles = Directory.EnumerateFiles(settings.TVMetadataDirectory).ToDictionary(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);
        string[] cacheFiles = Directory.GetFiles(settings.TVMetadataCacheDirectory);
        string[] libraryImdbIds = tvDirectories.SelectMany(tvDirectory => Directory.EnumerateFiles(tvDirectory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .Select(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty)
            .Where(imdbId => imdbId.IsNotNullOrWhiteSpace())
            .ToArray();

        ImdbMetadata[] imdbIds = x265Metadata.Keys
            .Distinct()
            .Except(libraryImdbIds)
            .Select(imdbId => metadataFiles.TryGetValue(imdbId, out string? file) && ImdbMetadata.TryLoad(file, out ImdbMetadata? imdbMetadata)
                ? imdbMetadata
                : null)
            .NotNull()
            .Where(predicate)
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
                    await Retry.FixedIntervalAsync(
                        async () => await DownloadImdbMetadataAsync(imdbMetadata.ImdbId, settings.TVMetadataDirectory, settings.TVMetadataCacheDirectory, metadataFiles.Values.ToArray(), cacheFiles, webDriver, overwrite: true, useCache: false, log: log, cancellationToken: cancellationToken),
                    isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.InternalServerError }, cancellationToken: cancellationToken);
                }
                catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
                {
                    log($"!!!{imdbMetadata.ImdbId} {exception}");
                }
            }, cancellationToken: cancellationToken);
        }

        await imdbIds
            .ForEachAsync(
                async (imdbMetadata, index, token) =>
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

                                html = await File.ReadAllTextAsync(file, token);
                            }
                            else
                            {
                                html = await webDriver.GetStringAsync(metadata.Link, () => webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("""img[src$="magnet.gif"]"""))), cancellationToken: token);
                                await Task.Delay(WebDriverHelper.DefaultDomWait, token);
                                await File.WriteAllTextAsync(file, html, token);
                            }
                            (string httpUrl, string _, string magnetUrl) = TopGetUrls(html, metadata.Link);

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
                                await Task.Delay(TimeSpan.FromSeconds(3), token);
                            }
                        }, cancellationToken: token);
                    }
                },
                cancellationToken: cancellationToken);
    }

    internal static async Task PrintFranchiseAsync(ISettings settings, string franchiseDirectory, int level = DefaultDirectoryLevel, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        ILookup<string, string> directoryNames = existingMetadata
            .Values
            .SelectMany(group => group.Keys)
            .ToLookup(
                relativePath => Regex.Replace(PathHelper.GetFileName(PathHelper.GetDirectoryName(relativePath)), "^The ", string.Empty, RegexOptions.IgnoreCase),
                relativePath => relativePath);
        EnumerateDirectories(franchiseDirectory, level)
            .Select(directory => Regex.Replace(PathHelper.GetFileName(directory).Split("`").First(), "^The ", string.Empty, RegexOptions.IgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ForEach(franchise => directoryNames
                .Where(group => group.Key.StartsWithIgnoreCase(franchise))
                .ForEach(group => group.Order().ForEach(video => log($"{franchise} | {group.Key} | {video}"))));
    }

    internal static async Task PrintMoviesByCollectionAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params string[][] directoryDrives)
    {
        log ??= Logger.WriteLine;

        //HashSet<string> xmls = new(Directory.GetFiles(settings.MovieTemp41, XmlMetadataSearchPattern, SearchOption.AllDirectories), StringComparer.OrdinalIgnoreCase);

        //directoryDrives
        //    // Hard drives in parallel.
        //    .AsParallel()
        //    .SelectMany(driveDirectories => driveDirectories
        //        .SelectMany(directory => EnumerateDirectories(directory))
        //        .AsParallel()
        //        .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
        //        .Where(directory => !Directory.EnumerateFiles(directory).Any(file => file.IsVideo())))
        //.Order()
        //.Do(log)
        //.ForEach(directory =>
        //{
        //    DirectoryHelper.Delete(directory);
        //});

        //directoryDrives
        //    // Hard drives in parallel.
        //    .AsParallel()
        //    .SelectMany(driveDirectories => driveDirectories
        //        .SelectMany(directory => Directory.EnumerateFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories))
        //        .AsParallel()
        //        .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
        //        .Select(file =>
        //        {
        //            XElement root = XDocument.Load(file).Root ?? throw new InvalidOperationException(file);
        //            string directory = PathHelper.GetDirectoryName(file);
        //            return (
        //                Path: file,
        //                Directory: directory,
        //                DirectoryName: PathHelper.GetFileName(directory),
        //                Collection: root.Element("collectionnumber")?.Value ?? string.Empty,
        //                CollectionName: root.Element("set")?.Value ?? string.Empty,
        //                ImdbId: root.Element("imdbid")?.Value ?? string.Empty,
        //                TmdbId: root.Element("tmdbid")?.Value ?? string.Empty);
        //        })
        //        .Where(metadata => metadata.TmdbId.IsNullOrWhiteSpace()))
        //    .GroupBy(metadata => metadata.Directory)
        //    .OrderBy(group => group.Key)
        //    .ForEach(group => group
        //        .DistinctBy(metadata => metadata.Directory)
        //        .OrderBy(metadata => metadata.DirectoryName)
        //        .Select(metadata => $"{metadata.DirectoryName} | {metadata.Directory} | https://www.imdb.com/title/{metadata.ImdbId}/")
        //        .Append(string.Empty)
        //        .ForEach(log));

        //directoryDrives
        //    // Hard drives in parallel.
        //    .AsParallel()
        //    .SelectMany(driveDirectories => driveDirectories
        //        .SelectMany(directory => Directory.EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
        //        .AsParallel()
        //        .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
        //        .Where(file => PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal("-")))
        //    .OrderBy(file => file)
        //    .ForEach(file => log($"{PathHelper.GetFileName(PathHelper.GetDirectoryName(file))} | {PathHelper.GetDirectoryName(file)}{Environment.NewLine}"));

        directoryDrives
            // Hard drives in parallel.
            .AsParallel()
            .ForAll(driveDirectories => driveDirectories
                .SelectMany(directory => EnumerateDirectories(directory))
                .ForEach(movie =>
                {

                    //string newMovie = Regex.Replace(movie, @"[ ]{2,}", " ");
                    if (movie.ContainsOrdinal(".."))
                    {
                        log(movie);
                    }
                }));

        directoryDrives
            // Hard drives in parallel.
            .AsParallel()
            .SelectMany(driveDirectories => driveDirectories
                .SelectMany(directory => Directory.EnumerateFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories))
                .AsParallel()
                .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
                .Select(file =>
                {
                    XElement root = XDocument.Load(file).Root ?? throw new InvalidOperationException(file);
                    string directory = PathHelper.GetDirectoryName(file);
                    return (
                        Path: file,
                        Directory: directory,
                        DirectoryName: PathHelper.GetFileName(directory),
                        Collection: root.Element("collectionnumber")?.Value ?? string.Empty,
                        CollectionName: root.Element("set")?.Value ?? string.Empty,
                        ImdbId: root.Element("imdbid")?.Value ?? string.Empty,
                        TmdbId: root.Element("tmdbid")?.Value ?? string.Empty);
                })
                .Where(metadata => metadata.Collection.IsNotNullOrWhiteSpace()))
            .GroupBy(metadata => metadata.Collection)
            .OrderBy(group => group.Key)
            //.Where(group => group.Any(metadata => xmls.Contains(metadata.Path)))
            .ForEach(group =>
            {
                (string Path, string Directory, string DirectoryName, string Collection, string CollectionName, string ImdbId, string TmdbId)[] movies = group
                    .DistinctBy(metadata => metadata.Directory)
                    .OrderBy(metadata => metadata.DirectoryName)
                    .ToArray();

                if (movies.Length > 1 && movies.DistinctBy(movie => PathHelper.GetDirectoryName(movie.Directory), StringComparer.OrdinalIgnoreCase).Count() == 1)
                {
                    return;
                }

                movies
                    .Select(metadata => $"{metadata.CollectionName} | {metadata.DirectoryName} | {metadata.Directory} | https://www.imdb.com/title/{metadata.ImdbId}/ | https://www.themoviedb.org/movie/{metadata.TmdbId}")
                    .Prepend($"https://www.themoviedb.org/collection/{group.Key}")
                    .Append(string.Empty)
                    .ForEach(log);
            });
    }

    internal static async Task PrintTVContrast(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ILookup<string, ContrastMetadata> contrastMetadata = (await JsonHelper.DeserializeFromFileAsync<ContrastMetadata[]>(settings.TVContrastMetadata, cancellationToken))
            .Where(metadata => metadata.ImdbId.IsNotNullOrWhiteSpace()
                && metadata.Title.ContainsIgnoreCase($"{VersionSeparator}{settings.ContrastKeyword}")
                && metadata.Title.ContainsIgnoreCase("1080p"))
            .ToLookup(metadata => metadata.ImdbId);

        string[] tvDirectories =
        [
            settings.TVControversial,
            settings.TVDocumentary,
            settings.TVMainstream,
            settings.TVMainstreamWithoutSubtitle,
            settings.TVTemp4
        ];

        tvDirectories
            .SelectMany(tvDirectory => EnumerateDirectories(tvDirectory, 1))
            .Select(tv => (
                tv,
                Imdb: Directory.EnumerateFiles(tv, ImdbMetadataSearchPattern).Single()))
            .Where(tv => !PathHelper.GetFileNameWithoutExtension(tv.Imdb).EqualsOrdinal(NotExistingFlag))
            .Select(tv => (tv.tv, ImdbId: tv.Imdb.GetImdbId()))
            .Where(tv => contrastMetadata.Contains(tv.ImdbId))
            .Select(tv => (
                TV: tv.tv,
                ImdbId: tv.ImdbId,
                Seasons: Directory
                    .EnumerateDirectories(tv.tv, "Season *")
                    .Select(season => (
                        season,
                        SeasonNumber: PathHelper.GetFileName(season).Split(Delimiter).First().Split(" ").Last(),
                        Episodes: Directory.EnumerateFiles(season).Where(file =>
                        {
                            string name = Path.GetFileNameWithoutExtension(file);
                            return file.IsVideo()
                                && !name.ContainsIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}")
                                && !name.ContainsIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}")
                                && !name.ContainsIgnoreCase($"{VersionSeparator}{settings.ContrastKeyword}");
                        }).ToArray()))
                    .ToArray()))
            .Where(tv => tv.Seasons.Any())
            .ForEach(tv =>
            {
                ContrastMetadata[] tvMetadata = contrastMetadata[tv.ImdbId].ToArray();
                tv
                    .Seasons
                    .Where(season => season.Episodes.Any())
                    .ForEach(season =>
                    {
                        if (Regex.IsMatch(season.SeasonNumber, "^[0-9]{2}$"))
                        {
                            tvMetadata
                                .Where(seasonMetadata => seasonMetadata.Title.Contains($".S{season.SeasonNumber}."))
                                .ForEach(seasonMetadata =>
                                {
                                    if (season.Episodes.Any(e => e.ContainsIgnoreCase("BluRay")) && !seasonMetadata.Title.ContainsIgnoreCase("BluRay"))
                                    {
                                        log("!!!");
                                    }

                                    log(PathHelper.GetFileName(season.Episodes.First()));
                                    log(seasonMetadata.Title);
                                    log(seasonMetadata.Magnet);
                                    log(string.Empty);
                                });
                        }
                    });

                tvMetadata
                    .Where(seasonMetadata => tv
                        .Seasons
                        .Select(season => season.SeasonNumber)
                        .All(seasonNumber => !seasonMetadata.Title.ContainsIgnoreCase($".S{seasonNumber}.")))
                    .ForEach(seasonMetadata =>
                    {
                        log(seasonMetadata.Title);
                        log(seasonMetadata.Magnet);
                        log(string.Empty);
                    });
            });
    }
}