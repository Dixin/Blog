using Examples.Common;
using Examples.IO;
using Examples.Net;
using MediaManager;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("Settings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
IConfigurationRoot? configuration = configurationBuilder.Build();
Settings settings = configuration.Get<Settings>() ?? throw new InvalidOperationException();

AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"..\..\..\Data");
//using TextWriterTraceListener traceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
//Trace.Listeners.Add(traceListener);
//Trace.Listeners.Remove(traceListener);
Trace.Listeners.Add(new ConsoleTraceListener());
FFmpeg.SetExecutablesPath(settings.FfmpegDirectory);

Action<string> log = x => Trace.WriteLine(x);
log(typeof(Enumerable).Assembly.Location);

//Video.PrintDirectoriesWithMultipleMedia(settings.MovieControversial.Directory);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstream.Directory);
//await Video.DownloadImdbMetadataAsync(settings.Movie3D.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieHdr.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversial.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstream.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstreamWithoutSubtitle.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieMusical.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"E:\Files\Movies", 2, overwrite: false, useCache: true, useBrowser: true, 1);

//await Video.DownloadImdbMetadataAsync(settings.TVControversial.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVDocumentary.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstream.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVTemp.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);

//Video.MoveFanArt(settings.Movie3D.Directory, 2);
//Video.MoveFanArt(settings.MovieHdr.Directory, 2);
//Video.MoveFanArt(settings.MovieControversial.Directory, 2);
//Video.MoveFanArt(settings.MovieMainstream.Directory, 2);
//Video.MoveFanArt(settings.MovieMainstreamWithoutSubtitle.Directory, 2);
//Video.MoveFanArt(settings.MovieMusical.Directory, 2);

//Video.MoveFanArt(settings.TVControversial.Directory, 1);
//Video.MoveFanArt(settings.TVDocumentary.Directory, 1);
//Video.MoveFanArt(settings.TVMainstream.Directory, 1);

//await Video.DownloadImdbMetadataAsync(
//    new (string Directory, int Level)[]
//    {`
//        settings.Movie3D,
//        settings.MovieHdr,
//        settings.MovieControversial,
//        settings.MovieMainstream,
//        settings.MovieMainstreamWithoutSubtitle,
//        settings.MovieMusical,
//        settings.TVControversial,
//        settings.TVDocumentary,
//        settings.TVMainstream
//    },
//    movie => movie.Year is "2022" or "2021" or "2020", true, false, true);

//Video.PrintDirectoryTitleMismatch(settiHouse of the Dragonngs.Movie3D.Directory);
//Video.PrintDirectoryTitleMismatch(settings.MovieControversial.Directory);
//Video.PrintDirectoryTitleMismatch(settings.MovieMainstream.Directory);

//Video.PrintDirectoryTitleMismatch(settings.TVControversial.Directory, level: 1);
//Video.PrintDirectoryTitleMismatch(settings.TVDocumentary.Directory, level: 1);
//Video.PrintDirectoryTitleMismatch(settings.TVMainstream.Directory, level: 1);

//Video.PrintDirectoryOriginalTitleMismatch(settings.Movie3D.Directory);
//Video.PrintDirectoryOriginalTitleMismatch(settings.MovieControversial.Directory);
//Video.PrintDirectoryOriginalTitleMismatch(settings.MovieMainstream.Directory);

//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.Movie3D.Directory);
//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.MovieControversial.Directory);
//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.MovieMainstream.Directory);

//Video.RenameDirectoriesWithImdbMetadata(settings.Movie3D.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieHdr.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieControversial.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieMainstream.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieMainstreamWithoutSubtitle.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieMusical.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieTemp.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Movies", isDryRun: true);

//Video.RenameDirectoriesWithImdbMetadata(settings.TVControversial.Directory, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.TVDocumentary.Directory, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.TVMainstream.Directory, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.TVTemp.Directory, level: 1, isTV: true, isDryRun: true);

//Video.SyncRating(settings.Movie3D.Directory);
//Video.SyncRating(settings.MovieHdr.Directory);
//Video.SyncRating(settings.MovieControversial.Directory);
//Video.SyncRating(settings.MovieMainstream.Directory);
//Video.SyncRating(settings.MovieMainstreamWithoutSubtitle.Directory);
//Video.SyncRating(settings.MovieMusical.Directory);

//Video.SyncRating(settings.TVControversial.Directory, 1);
//Video.SyncRating(settings.TVDocumentary.Directory, level: 1);
//Video.SyncRating(settings.TVMainstream.Directory, level: 1);
//Video.SyncRating(settings.TVTutorial.Directory, level: 1);

//Video.BackupMetadata(settings.MovieControversial.Directory);
//Video.BackupMetadata(settings.MovieMainstream.Directory);
//Video.BackupMetadata(settings.MovieMainstreamWithoutSubtitle.Directory);
//Video.BackupMetadata(settings.MovieMusical.Directory);

//Video.BackupMetadata(settings.TVControversial.Directory);
//Video.BackupMetadata(settings.TVDocumentary.Directory);
//Video.BackupMetadata(settings.TVMainstream.Directory);

//Video.PrintDirectoriesWithErrors(settings.Movie3D.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieHdr.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieControversial.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieMainstream.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieMainstreamWithoutSubtitle.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieMusical.Directory);
//Video.PrintDirectoriesWithErrors(@"E:\Files\Movies");

//Video.PrintDirectoriesWithErrors(settings.TVControversial.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVDocumentary.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVMainstream.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVTutorial.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVTemp.Directory, 1, isTV: true);

//Video.PrintVideosWithErrors(settings.Movie3D.Directory, searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(settings.MovieControversial.Directory, searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(settings.MovieMainstream.Directory, searchOption: SearchOption.AllDirectories);

//await Video.ConvertToUtf8Async(settings.Movie3D.Directory);
//await Video.ConvertToUtf8Async(settings.MovieMainstream.Directory);
//await Video.ConvertToUtf8Async(settings.MovieControversial.Directory);

//await Video.ConvertToUtf8Async(settings.TVControversial.Directory);
//await Video.ConvertToUtf8Async(settings.TVDocumentary.Directory);
//await Video.ConvertToUtf8Async(settings.TVMainstream.Directory);
//await Video.ConvertToUtf8Async(settings.TVTutorial.Directory);
//await Video.ConvertToUtf8Async(settings.LibraryDirectory);

//await Video.RenameSubtitlesByLanguageAsync(settings.MovieTemp.Directory, isDryRun: true);

//Video.DeleteFeaturettesMetadata(settings.Movie3D.Directory, isDryRun: true);
//Video.DeleteFeaturettesMetadata(settings.MovieMainstream.Directory, isDryRun: true);
//Video.DeleteFeaturettesMetadata(settings.MovieControversial.Directory, isDryRun: true);

//Video.PrintDirectoriesWithMultipleMedia(settings.Movie3D.Directory);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieHdr.Directory);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieControversial.Directory);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstream.Directory);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstreamWithoutSubtitle.Directory);

//Video.PrintSubtitlesWithErrors(settings.LibraryDirectory);

//Video.PrintMoviesWithoutSubtitle(settings.MovieControversial.Directory);
//Video.PrintMoviesWithoutSubtitle(settings.MovieMainstream.Directory);

//Video.PrintDuplicateImdbId(null,
//    settings.MovieMainstream.Directory,
//    settings.MovieControversial.Directory,
//    settings.MovieMainstreamWithoutSubtitle.Directory,
//    settings.MovieMusical.Directory,
//    settings.MovieTemp.Directory,
//    @"E:\Files\Movies");

//Video.PrintDefinitionErrors(settings.LibraryDirectory);

//await Video.ConvertToUtf8Async(settings.LibraryDirectory);

//Video.MoveSubtitleToParentDirectory(settings.MovieTemp.Directory);
//await Video.SaveAllVideoMetadata(settings.MovieLibraryMetadata, null, settings.MovieControversial.Directory, settings.MovieMainstream.Directory, settings.MovieMainstreamWithoutSubtitle.Directory, settings.MovieMusical.Directory, settings.MovieTemp.Directory, @"E:\Files\Movies");
//await Video.SaveExternalVideoMetadataAsync(settings.MovieExternalMetadata, settings.MovieTemp.Directory);
//await Video.CompareAndMoveAsync(settings.MovieExternalMetadata, settings.MovieLibraryMetadata, settings.MovieExternalNew.Directory, settings.MovieExternalDelete.Directory, isDryRun: false);

//Video.MoveAllSubtitles(settings.MovieTemp.Directory, settings.MovieSubtitleBackupDirectory);
//await Drive115.SaveOfflineTasksAsync(settings.Drive115Url, settings.Drive115Metadata, log, "3063bd4d991fc9a6626be1fec54c83cdc0f22b58");

//await Video.PrintMovieImdbIdErrorsAsync(settings.MovieRarbgX265Metadata, settings.MovieRarbgH264Metadata, settings.MovieRarbgH264720PMetadata, settings.MovieYtsMetadata, settings.MovieIgnoreMetadata, null,
//    settings.MovieMainstream,
//    settings.MovieControversial,
//    settings.MovieMainstreamWithoutSubtitle,
//    settings.MovieMusical,
//    settings.MovieTemp);

//Dictionary<string, RarbgMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(settings.TVRarbgX265Metadata))!;
//x265Metadata.Values.SelectMany(values=>values).OrderByDescending(meta=>meta.ImdbRating).ThenBy(meta=>meta.ImdbRating)
//    .ForEach(meta=>log($"{meta.ImdbRating} {meta.Title} {meta.Link}"));

//await Rarbg.DownloadMetadataAsync(settings.MovieRarbgX265Url, settings.MovieRarbgX265Metadata, log, index => index <= 5);
//await Rarbg.DownloadMetadataAsync(settings.MovieRarbgH264Url, settings.MovieRarbgH264Metadata, log, index => index <= 20);
//await Rarbg.DownloadMetadataAsync(settings.MovieRarbgH264720PUrl, settings.MovieRarbgH264720PMetadata, log, index => index <= 10);
//await Rarbg.DownloadMetadataAsync(settings.TVRarbgX265Url, settings.TVRarbgX265Metadata, log, index => index <= 5);
//await Yts.DownloadMetadataAsync(settings.MovieYtsUrl, settings.MovieYtsSummary, settings.MovieYtsMetadata, log, index => index <= 10);
//await Video.PrintMovieVersions(settings.MovieRarbgX265Metadata, settings.MovieRarbgH264Metadata, settings.MovieYtsMetadata, settings.MovieRarbgH264720PMetadata, settings.MovieIgnoreMetadata, null,
//    settings.MovieMainstream,
//    settings.MovieControversial,
//    settings.MovieMainstreamWithoutSubtitle,
//    settings.MovieMusical,
//    settings.MovieTemp,
//    (@"E:\Files\Movies", 2));
//await Video.PrintTVVersions(settings.TVRarbgX265Metadata, null,
//    settings.TVControversial,
//    settings.TVDocumentary,
//    settings.TVMainstream/*,
//    settings.TVTemp*/);

//await Yts.DownloadMetadataAsync(settings.MovieYtsSummary, settings.MovieYtsMetadata, log, 4);
//Audio.ReplaceTraditionalChinese(settings.AudioMainstream.Directory, true);

//Audio.PrintDirectoriesWithErrors(settings.AudioControversial.Directory);
//Audio.PrintDirectoriesWithErrors(settings.AudioMainstream.Directory);
//Audio.PrintDirectoriesWithErrors(settings.AudioShow.Directory);
//Audio.PrintDirectoriesWithErrors(settings.AudioSoundtrack.Directory);

//Video.MoveAllSubtitles(settings.MovieTemp.Directory, settings.MovieSubtitleBackupDirectory);

//Video.RenameDirectoriesWithMetadata(settings.TVTemp.Directory, 1, isTV: true);
//Video.RenameDirectoriesWithAdditionalMetadata(settings.MovieTemp.Directory, 2);
//Video.RenameDirectoriesWithoutMetadata(settings.MovieControversial.Directory, isDryRun: false);

//Video.RenameEpisodes(@"D:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]", "European Adult");
//Video.CreateTVEpisodeMetadata(@"T:\Files\Library\TV Tutorial.教程\English Toefl.2017.英语托福[---][-]");

//Video.PrintVideosWithErrors(settings.MovieTemp.Directory, searchOption: SearchOption.AllDirectories);
//Video.RenameDirectoriesWithMetadata(settings.MovieTemp.Directory, 2);
//Video.RestoreMetadata(settings.MovieTemp.Directory);

//Video.EnumerateDirectories(settings.MovieMainstream.Directory)
//   .Concat(Video.EnumerateDirectories(settings.MovieMainstreamWithoutSubtitle.Directory))
//   .Select(m => (m, new VideoDirectoryInfo(m)))
//   .Where(m => !m.Item2.IsHD)
//   .OrderBy(d => d)
//   .ForEach(m => log(m.Item1));

//Video.PrintMovieRegionsWithErrors(settings.MovieRegions, log, settings.MovieControversial, settings.MovieMainstream, settings.MovieMainstreamWithoutSubtitle);

//Video.EnumerateDirectories(settings.MovieHdr.Directory)
//    .Concat(Video.EnumerateDirectories(settings.Movie3D.Directory))
//    .Concat(Video.EnumerateDirectories(settings.MovieControversial.Directory))
//    .Concat(Video.EnumerateDirectories(settings.MovieMainstream.Directory))
//    .Concat(Video.EnumerateDirectories(settings.MovieMainstreamWithoutSubtitle.Directory))
//    .Where(d => new VideoDirectoryInfo(d).AggregateRating.CompareOrdinal("7.5") >= 0 && Imdb.TryLoad(d, out ImdbMetadata? metadata) && metadata.AggregateRating?.RatingCount < 5000)
//    .OrderBy(d => d)
//    .ToArray()
//    .ForEach(d =>
//    {
//        log(d);
//    });

//Video.RenameEpisodesWithTitle(
//    @"E:\Files\Library\TV Mainstream.主流电视剧\The Office.2005.办公室[9.0-588K][TV14][1080x]",
//    @"E:\Files\Library\TV Mainstream.主流电视剧\The Office.2005.办公室[9.0-588K][TV14][1080x]",
//    rename: (f, t) => f.Replace("-RARBG", $"-RARBG.{t}"),
//    //rename: (f, t) => Regex.Replace(f, @"(\.S[0-9]{2}E[0-9]{2})", $"{"$1".ToUpperInvariant()}.{t}"),
//    isDryRun: false);

//Video.RestoreMetadata(settings.MovieTemp.Directory);

//Video.MoveSubtitleToParentDirectory(@"E:\Files\Movies\Rarbg");
//Video.BackupMetadata(@"E:\Files\Movies");
//Video.RenameDirectoriesWithMetadata(@"E:\Files\Movies", 2, isDryRun: false);
//Video.RestoreMetadata(@"E:\Files\Movies");

//Directory.GetFiles(@"D:\Move\TV\New folder", "*", SearchOption.AllDirectories)
//    .ForEach(f => File.Move(f, f
//        .Replace(".简体&英文.", ".chs&eng.")
//        .Replace(".简体.", ".chs.")
//        .Replace(".英文.", ".eng.")
//        .Replace(".CHS&ENG.", ".chs&eng.")
//        .Replace(".CHS.", ".chs.")
//        .Replace(".ENG.", ".eng.")
//        .ReplaceIgnoreCase(".chs.eng.", ".chs&eng.")
//        .ReplaceIgnoreCase(".ChsEngA.", ".chs&eng.")
//        .Replace(".ass", ".chs&eng.ass")
//    .Replace(".srt", ".chs&eng.srt")
//    ));
//Video.MoveSubtitlesForEpisodes(
//    @"E:\Files\Library\TV Mainstream.主流电视剧\SEAL Team.2017.海豹突击队[7.7-26K][TV14][1080x]",
//    @"D:\Move\TV\New folder",
//    //".mkv",
//    overwrite: false);

//Video.CreateTVEpisodeMetadata(@"H:\Downloads7\New folder (6)\阅读\_1", f => Path.GetFileNameWithoutExtension(f).Split(".").Last());

//Video.PrintDirectoriesWithErrors(settings.MovieMusical.Directory);

static void TraceLog(string? message) => Trace.WriteLine(message);

static void RenameEpisode(string mediaDirectory, string metadataDirectory, bool isDryRun = false, Action<string>? log = null)
{
    log ??= TraceLog;

    string[] tvs = Directory.EnumerateDirectories(mediaDirectory)
        .Where(tv => VideoDirectoryInfo.TryParse(tv, out _))
        .ToArray();
    tvs.ForEach(tv =>
    {
        string metadataTV = Path.Combine(metadataDirectory, Path.GetFileName(tv));
        if (!Directory.Exists(metadataTV))
        {
            log($"Not exist {metadataTV}");
            //return false;
        }

        string[] seasons = Directory
            .EnumerateDirectories(tv, "Season *")
            .OrderBy(season => season)
            .ToArray();
        string[] metadataSeasons = Directory
            .EnumerateDirectories(metadataTV, "Season *")
            .OrderBy(season => season)
            .ToArray();

        string[] seasonNames = seasons.Select(season => Path.GetFileName(season)!).ToArray();
        string[] metadataSeasonNames = metadataSeasons.Select(season => Path.GetFileName(season)!).ToArray();
        string[] seasonMismatches = seasonNames.Except(metadataSeasonNames).ToArray();
        if (seasonMismatches.Any())
        {
            log($"Seasons mismatch: {string.Join(", ", seasonMismatches)}, {tv}");
            //return false;
        }

        seasonMismatches = metadataSeasonNames.Except(seasonNames).ToArray();
        if (seasonMismatches.Any())
        {
            log($"Seasons mismatch: {string.Join(", ", seasonMismatches)}, {metadataTV}");
            //return false;
        }

        string[] episodeNumbers = seasons
            .SelectMany(Directory.EnumerateFiles)
            .Where(Video.IsVideo)
            .Select(episode => Regex.Match(Path.GetFileNameWithoutExtension(episode), @"\.(S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.").Groups[1].Value)
            .OrderBy(number => number)
            .ToArray();
        string[] metadataEpisodeNumbers = metadataSeasons
            .SelectMany(Directory.EnumerateFiles)
            .Where(Video.IsVideo)
            .Select(episode => Regex.Match(Path.GetFileNameWithoutExtension(episode), @"\.(S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.").Groups[1].Value)
            .OrderBy(number => number)
            .ToArray();
        string[] episodeMismatches = episodeNumbers.Except(metadataEpisodeNumbers).ToArray();
        if (episodeMismatches.Any())
        {
            log($"Episode mismatch: {string.Join(", ", episodeMismatches)}, {tv}");
            //return false;
        }

        episodeMismatches = metadataEpisodeNumbers.Except(episodeNumbers).ToArray();
        if (episodeMismatches.Any())
        {
            log($"Episode mismatch: {string.Join(", ", episodeMismatches)}, {metadataTV}");
            //return false;
        }

        //return true;
    });

    tvs.ForEach(tv => Video.RenameEpisodesWithTitle(
        tv,
        Path.Combine(metadataDirectory, Path.GetFileName(tv)),
        (file, title) => Path.GetFileNameWithoutExtension(file).ContainsIgnoreCase("-RARBG") ? file.ReplaceIgnoreCase("-RARBG", $"-RARBG.{title}") : file.ReplaceIgnoreCase("-VXT", $"-VXT.{title}"),
        isDryRun,
        log));
    //tvs.ForEach(tv => Video.MoveSubtitlesForEpisodes(tv, Path.Combine(metadataDirectory, Path.GetFileName(tv)), isDryRun: isDryRun, log: log));
}

//Video.MoveRarbgTVEpisodes(@"D:\Move\TV\RARBG", @"D:\Move\TV.Subtitle");
//Video.FormatTV(
//    settings.TVTemp.Directory,
//    settings.TVTemp.Directory,
//    renameForTitle: (f, t) => f.Replace("-RARBG", $"-RARBG.{t}"),
//    isDryRun: false,
//    log: log);

static void RenameFilesWithDuplicateTitle(
    string directory,
    string titleFlag,
    SearchOption searchOption = SearchOption.TopDirectoryOnly,
    bool isDryRun = false,
    Action<string>? log = null)
{
    log ??= TraceLog;

    (string Path, string Number)[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, searchOption)
        .Select(file => (file, Regex.Match(Path.GetFileNameWithoutExtension(file), @"\.S\d+E\d+")))
        .Where(file => file.Item2.Success)
        .Select(file => (file.Item1, file.Item2.Value))
        .OrderBy(file => file.Item1)
        .ToArray();

    files
        .Where(file => Video.IsVideo(file.Path))
        .ToArray()
        .ForEach(video =>
        {
            string videoName = Path.GetFileNameWithoutExtension(video.Path);
            string title = videoName.Substring(videoName.LastIndexOfOrdinal(titleFlag) + titleFlag.Length);
            if (title.IsNotNullOrWhiteSpace() && title.Length % 2 == 0 && title.Substring(0, title.Length / 2).EqualsIgnoreCase(title.Substring(title.Length / 2)))
            {
                string newTitle = title.Substring(0, title.Length / 2);
                files
                    .Where(file => file.Number.EqualsIgnoreCase(video.Number))
                    .ToArray()
                    .ForEach(episodeFile =>
                    {
                        string newEpisodeFile = Path.Combine(Path.GetDirectoryName(episodeFile.Path)!, $"{Path.GetFileNameWithoutExtension(episodeFile.Path).ReplaceIgnoreCase(title, newTitle)}{Path.GetExtension(episodeFile.Path)}");
                        if (!title.EqualsOrdinal(newTitle))
                        {
                            log(episodeFile.Path);
                            if (!isDryRun)
                            {
                                File.Move(episodeFile.Path, newEpisodeFile);
                            }

                            log(newEpisodeFile);
                        }

                        log(string.Empty);
                    });
            }
        });
}

//Directory.GetDirectories(@"G:\AV\New folder", "*", SearchOption.TopDirectoryOnly)
//    .SelectMany(d => Directory.GetFiles(d))
//    .ForEach(f =>
//    {
//        //string metadata = Directory.GetFiles(d, "*.nfo").Single();
//        //string content = File.ReadAllText(metadata);

//        //string prefix = content switch
//        //{
//        //    _ when content.Contains("一本道") => "1PONDO",
//        //    _ when content.Contains("加勒比") => "CARIBBEAN",
//        //    _ when content.Contains("10musume") => "10musume".ToUpperInvariant(),
//        //    _ when content.Contains("PACOPACO MAMA") => "PACOPACOMAMA",
//        //    _ => ""
//        //};
//        //DirectoryHelper.AddPrefix(d, $"{prefix}-");
//        FileHelper.Move(f, f.ReplaceIgnoreCase("_hd", "")
//            .ReplaceIgnoreCase("-whole", "-cd").ReplaceIgnoreCase("-1080p", ""));
//    });

//Directory.GetFiles(@"F:\AV\Movie Japan Leak", "*", SearchOption.TopDirectoryOnly)
//    .ForEach(f =>
//    {
//        string d = Path.GetDirectoryName(f);
//        string name = Path.GetFileNameWithoutExtension(f).Trim();
//        string idWithCD = Regex.Match(name, @"^[A-Z\-]+(\-| )?[0-9]+(\-c|\-leak|\-patched)?(-cd[0-9]+)?", RegexOptions.IgnoreCase).Value;
//        Debug.Assert(idWithCD.IsNotNullOrWhiteSpace());
//        string id = Regex.Match(name, @"^[A-Z\-]+(\-| )?[0-9]+(\-c|\-leak|\-patched)?", RegexOptions.IgnoreCase).Value;

//        string newFile = Path.Combine(d, id, $"{idWithCD}{Path.GetExtension(f).ToLowerInvariant()}");
//        FileHelper.Move(f, newFile);
//        log(f);
//        log(newFile);
//        log(string.Empty);
//    });
//await Entry.DownloadMetadataAsync("http://hotxshare.com", 1, 238, @"D:\Files\Library\Movie.EntryMetadata.json", settings.MovieLibraryMetadata, settings.MovieRarbgX265Metadata, settings.MovieRarbgH264Metadata, settings.MovieYtsMetadata, settings.MovieRarbgH264720PMetadata, log);
//Parallel.ForEach(Directory.GetFiles(@"S:\Files\Library\Movies Controversial.非主流电影\French.法国\The Public Woman=La femme publique.1984.公共女人[6.5-1.5K][NA][480f2]\Featurettes", "*.mkv", SearchOption.AllDirectories), f => FfmpegHelper.Encode(f, estimateCrop: true, overwrite: true));
//Video.PrintMoviesWithoutSubtitle(@"E:\Files\Movies", 2, null, "eng");
//Video.MoveAllSubtitles(@"S:\Files\Library\Movies Temp", @"S:\Files\Library\Movies Temp.Subs");
await Imdb.DownloadAllAsync(settings.MovieLibraryMetadata, settings.MovieRarbgX265Metadata, settings.MovieRarbgH264Metadata, settings.MovieYtsMetadata, settings.MovieRarbgH264720PMetadata, @"D:\Files\Library\Movie.RareMetadata.json", @"D:\Files\Library\ImdbCache", @"D:\Files\Library\ImdbMetadata", log);