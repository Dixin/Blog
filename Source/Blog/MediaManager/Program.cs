using CsQuery.ExtensionMethods.Internal;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using Examples.Security;
using Examples.Text;
using MediaManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;
using OpenQA.Selenium.DevTools;
using Xabe.FFmpeg;
using SearchOption = System.IO.SearchOption;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();
using IHost host = builder.Build();

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("Settings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
IConfigurationRoot? configuration = configurationBuilder.Build();
Settings settings = configuration.Get<Settings>() ?? throw new InvalidOperationException();

AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"..\..\..\Data");
using TextWriterTraceListener textTraceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
Trace.Listeners.Add(textTraceListener);
using ConsoleTraceListener consoleTraceListener = new();
Trace.Listeners.Add(consoleTraceListener);

Console.OutputEncoding = Encoding.UTF8; // Or Unicode.

FFmpeg.SetExecutablesPath(settings.FfmpegDirectory);

Video.Initialize(settings.TopEnglishKeyword, settings.TopForeignKeyword, settings.PreferredOldKeyword, settings.PreferredNewKeyword);

//Video.PrintDirectoriesWithMultipleMedia(settings.MovieControversial.Directory);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstream.Directory);

//await Video.DownloadImdbMetadataAsync(settings.Movie3D.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieHdr.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversial.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstream.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstreamWithoutSubtitle.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieMusical.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp1.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp2.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp3.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversialTemp.Directory, 2, overwrite: false, useCache: true, useBrowser: true, 2);

//await Video.DownloadImdbMetadataAsync(settings.TVControversial.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVDocumentary.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstream.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstreamWithoutSubtitle.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(settings.TVTemp.Directory, 1, overwrite: false, useCache: true, useBrowser: true, 1);

//Video.MoveFanArt(settings.Movie3D.Directory, 2);
//Video.MoveFanArt(settings.MovieHdr.Directory, 2);
//Video.MoveFanArt(settings.MovieControversial.Directory, 2);
//Video.MoveFanArt(settings.MovieControversialTemp.Directory, 2);
//Video.MoveFanArt(settings.MovieMainstream.Directory, 2);
//Video.MoveFanArt(settings.MovieMainstreamWithoutSubtitle.Directory, 2);
//Video.MoveFanArt(settings.MovieMusical.Directory, 2);
//Video.MoveFanArt(settings.MovieTemp.Directory, 2);
//Video.MoveFanArt(settings.MovieTemp1.Directory, 2);
//Video.MoveFanArt(settings.MovieTemp2.Directory, 2);
//Video.MoveFanArt(settings.MovieTemp3.Directory, 2);

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
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieControversialTemp.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieMainstream.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieMainstreamWithoutSubtitle.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieMusical.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieTemp.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieTemp1.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieTemp2.Directory, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieTemp3.Directory, isDryRun: true);

//Video.RenameDirectoriesWithImdbMetadata(settings.TVControversial.Directory, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.TVDocumentary.Directory, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.TVMainstream.Directory, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings.TVMainstreamWithoutSubtitle.Directory, level: 1, isTV: true, isDryRun: true);

//Video.WriteRating(settings.Movie3D.Directory);
//Video.WriteRating(settings.MovieHdr.Directory);
//Video.WriteRating(settings.MovieControversial.Directory);
//Video.WriteRating(settings.MovieMainstream.Directory);
//Video.WriteRating(settings.MovieMainstreamWithoutSubtitle.Directory);
//Video.WriteRating(settings.MovieMusical.Directory);

//Video.WriteRating(settings.TVControversial.Directory, 1);
//Video.WriteRating(settings.TVDocumentary.Directory, level: 1);
//Video.WriteRating(settings.TVMainstream.Directory, level: 1);
//Video.WriteRating(settings.TVTutorial.Directory, level: 1);

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
//Video.PrintDirectoriesWithErrors(settings.MovieControversialTemp.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieMainstream.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieMainstreamWithoutSubtitle.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieMusical.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieTemp.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieTemp1.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieTemp2.Directory);
//Video.PrintDirectoriesWithErrors(settings.MovieTemp3.Directory);

//Video.PrintDirectoriesWithErrors(settings.TVControversial.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVDocumentary.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVMainstream.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVTutorial.Directory, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings.TVMainstreamWithoutSubtitle.Directory, 1, isTV: true);

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
//   settings.MovieMainstream.Directory,
//   settings.MovieMainstreamWithoutSubtitle.Directory,
//   settings.MovieMusical.Directory,
//   settings.MovieTemp1.Directory,
//   settings.MovieTemp2.Directory,
//   settings.MovieTemp3.Directory,
//   settings.MovieTemp.Directory,
//   settings.MovieControversial.Directory,
//   settings.MovieControversialTemp.Directory,
//   settings.MovieControversialTemp1.Directory,
//   @"Q:\Files\Movies.Raw");

//Video.PrintDefinitionErrors(settings.LibraryDirectory);

//await Video.ConvertToUtf8Async(settings.LibraryDirectory);

//Video.MoveSubtitleToParentDirectory(settings.MovieTemp.Directory);
//await Video.WriteLibraryMovieMetadata(settings.MovieLibraryMetadata, null,
//    settings.MovieControversial.Directory,
//    settings.MovieControversialTemp.Directory,
//    settings.MovieMainstream.Directory,
//    settings.MovieMainstreamWithoutSubtitle.Directory,
//    settings.MovieMusical.Directory,
//    settings.MovieTemp.Directory,
//    settings.MovieTemp1.Directory,
//    settings.MovieTemp2.Directory,
//    settings.MovieTemp3.Directory);
//await Video.WriteExternalVideoMetadataAsync(settings.MovieExternalMetadata, settings.MovieTemp.Directory);
//await Video.CompareAndMoveAsync(settings.MovieExternalMetadata, settings.MovieLibraryMetadata, settings.MovieExternalNew.Directory, settings.MovieExternalDelete.Directory, isDryRun: false);

//Video.MoveAllSubtitles(settings.MovieTemp.Directory, settings.MovieSubtitleBackupDirectory);
//await Drive115.WriteOfflineTasksAsync(settings.Drive115Url, settings.Drive115Metadata, "Goto.Isle.of.Love.1969");

//await Video.PrintMovieImdbIdErrorsAsync(settings.MovieTopX265Metadata, settings.MovieTopH264Metadata, settings.MovieTopX265XMetadata, settings.MovieTopH264XMetadata, /*settings.MovieTopH264720PMetadata, settings.MoviePreferredMetadata,*/ null,
//    settings.MovieMainstream,
//    settings.MovieControversial,
//    settings.MovieControversialTemp,
//    settings.MovieMainstreamWithoutSubtitle,
//    settings.MovieMusical,
//    settings.MovieTemp,
//    settings.MovieTemp1,
//    settings.MovieTemp2,
//    settings.MovieTemp3);

//Dictionary<string, TopMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(settings.TVTopX265Metadata))!;
//x265Metadata.Values.SelectMany(values=>values).OrderByDescending(meta=>meta.ImdbRating).ThenBy(meta=>meta.ImdbRating)

//await Top.DownloadMetadataAsync(settings.MovieTopX265EnglishUrl, settings.MovieTopX265XMetadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopX265ForeignUrl, settings.MovieTopX265XMetadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopH264EnglishUrl, settings.MovieTopH264XMetadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopH264ForeignUrl, settings.MovieTopH264XMetadata, index => index <= 5);

//await Top.DownloadMetadataAsync(settings.MovieTopX265Url, settings.MovieTopX265Metadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopH264Url, settings.MovieTopH264Metadata, index => index <= 20);
//await Top.DownloadMetadataAsync(settings.MovieTopH264720PUrl, settings.MovieTopH264720PMetadata, index => index <= 10);
//await Top.DownloadMetadataAsync(settings.TVTopX265Url, settings.TVTopX265Metadata, index => index <= 5);
//await Preferred.DownloadMetadataAsync(settings.MoviePreferredUrl, settings.MoviePreferredSummary, settings.MoviePreferredMetadata, index => index <= 50);
//await Video.PrintMovieVersions(settings.MovieTopX265Metadata, settings.MovieTopH264Metadata, settings.MoviePreferredMetadata, settings.MovieTopH264720PMetadata, settings.MovieIgnoreMetadata, null,
//   settings.MovieMainstream,
//   settings.MovieControversial,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieControversialTemp,
//   settings.MovieControversialTemp1);
//await Video.PrintTVVersions(settings.TVTopX265Metadata, null,
//    settings.TVControversial,
//    settings.TVDocumentary,
//    settings.TVMainstream,
//    settings.TVMainstreamWithoutSubtitle);

await Imdb.UpdateAllMoviesAsync(
    settings.MovieLibraryMetadata,
    settings.MovieTopX265Metadata, settings.MovieTopH264Metadata, settings.MoviePreferredMetadata, settings.MovieTopH264720PMetadata, settings.MovieRareMetadata, settings.MovieTopX265XMetadata, settings.MovieTopH264XMetadata,
    settings.MovieMetadataCacheDirectory, settings.MovieMetadataDirectory,
    count => ..(count / 5));
//await Imdb.DownloadAllTVsAsync(settings.TVTopX265Metadata, settings.TVMainstream.Directory, settings.TVMetadataCacheDirectory, settings.TVMetadataDirectory);
string[] keywords =
{
    "female full frontal nudity", "female topless nudity", "female pubic hair", "female frontal nudity", "female nudity",
    "erotica", "softcore", "female star appears nude",
    "unsimulated sex", "vagina", "labia", "shaved labia","labia minora","labia majora","shaved vagina","spread vagina",
    "spread eagle", "bottomless",
    "lesbian sex","leg spreading"
};
string[] genres = { "family", "animation", "documentary" };
//await Video.PrintTVLinks(settings.TVTopX265Metadata, new string[] { settings.TVMainstream.Directory, settings.TVMainstreamWithoutSubtitle.Directory }, @"D:\Files\Library\TVMetadata", @"D:\Files\Library\TVMetadataCache", "https://rarbg.to/torrents.php?search=x265.rarbg&category%5B%5D=41",
//    imdbMetadata =>
//        !imdbMetadata.AllKeywords.Intersect(new string[] { "test" }, StringComparer.OrdinalIgnoreCase).Any()
//        && !imdbMetadata.Genres.Intersect(genres, StringComparer.OrdinalIgnoreCase).Any()
//        && (imdbMetadata
//            .Advisories
//                .Where(advisory => advisory.Key.ContainsIgnoreCase("sex") || advisory.Key.ContainsIgnoreCase("nudity"))
//                .SelectMany(advisory => advisory.Value)
//                .Any(advisory => advisory.FormattedSeverity == ImdbAdvisorySeverity.Severe)
//    || imdbMetadata.AllKeywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any()), isDryRun: true);

//await Video.MergeMovieMetadataAsync(settings.MovieMetadataDirectory, @"D:\Files\Library\Movie.MergedMetadata.json");
//await Video.PrintMovieLinksAsync(
//    settings.MovieLibraryMetadata, settings.MovieTopX265Metadata, settings.MovieTopH264Metadata, settings.MoviePreferredMetadata, settings.MovieTopH264720PMetadata, settings.MovieRareMetadata,
//    @"D:\Files\Library\Top.MagnetUris.txt",
//    @"D:\Files\Library\Movie.MergedMetadata.json", settings.MovieMetadataCacheDirectory, settings.MovieMetadataDirectory, string.Empty,
//    imdbMetadata =>
//    //imdbMetadata.AllKeywords.Intersect(new string[] { "test" }, StringComparer.OrdinalIgnoreCase).IsEmpty()
//    // &&imdbMetadata.Genres.Intersect(genres, StringComparer.OrdinalIgnoreCase).IsEmpty()
//    //&& imdbMetadata.Genres.Intersect(new string[] { "action" }, StringComparer.OrdinalIgnoreCase).Any()
//     (imdbMetadata.Advisories
//            .Where(advisory => advisory.Key.ContainsIgnoreCase("sex") || advisory.Key.ContainsIgnoreCase("nudity"))
//            .SelectMany(advisory => advisory.Value)
//            .Any(advisory => advisory.FormattedSeverity == ImdbAdvisorySeverity.Severe)
//        || imdbMetadata.AllKeywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any())
//    //&& string.Compare(imdbMetadata.FormattedAggregateRating, "8.0", StringComparison.Ordinal) <= 0
//    //&& string.Compare(imdbMetadata.FormattedAggregateRating, "7.0", StringComparison.Ordinal) >= 0
//    //&& imdbMetadata.AggregateRating?.RatingCount >= 10_000
//    //&& imdbMetadata.AllKeywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any()
//    ,
//    isDryRun: true);

//await Preferred.DownloadMetadataAsync(settings.MoviePreferredSummary, settings.MoviePreferredMetadata, log, 4);
//Audio.ReplaceTraditionalChinese(settings.AudioMainstream.Directory, true);

//Audio.PrintDirectoriesWithErrors(settings.AudioControversial.Directory);
//Audio.PrintDirectoriesWithErrors(settings.AudioMainstream.Directory);
//Audio.PrintDirectoriesWithErrors(settings.AudioShow.Directory);
//Audio.PrintDirectoriesWithErrors(settings.AudioSoundtrack.Directory);

//Video.MoveAllSubtitles(settings.MovieTemp.Directory, settings.MovieSubtitleBackupDirectory);

//Video.RenameDirectoriesWithMetadata(settings.TVTemp.Directory, 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithAdditionalMetadata(settings.MovieTemp.Directory, 2);
//Video.RenameDirectoriesWithoutMetadata(settings.MovieControversial.Directory, isDryRun: false);

//Video.RenameEpisodes(@"D:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]", "European Adult");
//Video.CreateTVEpisodeMetadata(@"T:\Files\Library\TV Tutorial.教程\English Toefl.2017.英语托福[---][-]");

//Video.PrintVideosWithErrors(@"E:\Files\Korean", searchOption: SearchOption.AllDirectories);
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
//Directory.GetDirectories(@"N:\Files\Library\New folder").ToArray().ForEach(d => { 
//Video.RenameEpisodesWithTitle(
//    @"N:\Files\Library\TV Mainstream.主流电视剧[未抓取字幕]\Slasher.2016.鲜血淋漓[6.7-15K][TV14]\Season 05",
//    @"",
//    rename: (f, t) =>
//    {
//        //string postfix = Path.GetFileNameWithoutExtension(f).EndsWithIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio") ? $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio" : $"{Video.VersionSeparator}{Video.TopEnglishKeyword}";
//        string postfix = $"{Video.VersionSeparator}{Video.TopEnglishKeyword}";
//        //Debug.Assert(!f.IsVideo() || Path.GetFileNameWithoutExtension(f).EndsWithIgnoreCase(postfix));
//        string name = Path.GetFileNameWithoutExtension(f);
//        return name.EndsWithOrdinal(postfix) || name.EndsWithOrdinal($"{postfix}-thumb") || Regex.IsMatch(name, $@"{postfix.Replace(".", @"\.").Replace("-", @"\-")}\.[a-z]{{3}}(&[a-z]{{3}})?$")
//            ? f.Replace(postfix, $"{postfix}.{t}")
//            : f;
//        //return Regex.Replace(f, @"(S[0-9]{2}E[0-9]{2})", $"$1.{t}");
//        //return f.Replace(".ffmpeg", $".ffmpeg.{t}");
//        //return PathHelper.AddFilePostfix(f, t);
//    },
//    //rename: (f, t) => f.Replace($"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio", $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio.{t}"),
//    //rename: (f, t) => Regex.Replace(f, @"(\.S[0-9]{2}E[0-9]{2})", $"{"$1".ToUpperInvariant()}.{t}"),
//    isDryRun: false);
//});
//Video.RestoreMetadata(settings.MovieTemp.Directory);
//Video.MoveSubtitleToParentDirectory(@"L:\Files3\Movies2");
//Video.BackupMetadata(@"Q:\Files\Movies.Raw");
//Video.RenameDirectoriesWithMetadata(@"Q:\Files\Movies.Raw", 2, isDryRun: false, isTV: false);
//Video.RenameDirectoriesWithMetadata(@"S:\Files\Library\New folder", 2, isDryRun: false, isTV: false);
//Video.RenameDirectoriesWithMetadata(@"E:\Files\Movies", 2, isDryRun: false, isTV: false);
//Video.RestoreMetadata(@"Q:\Files\Movies.Raw2");
//Video.RestoreMetadata(@"S:\Files\Library\New folder");
//Video.RestoreMetadata(@"E:\Files\Movies");

//Directory.GetFiles(@"D:\User\Downloads\New folder", "*", SearchOption.AllDirectories)
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
//    @"D:\User\Downloads\TV\Sex & Violence.2013.性与暴力[7.3-159][NA][1080x]",
//    @"N:\Files\Library\TV Controversial.非主流电视剧\Sex & Violence.2013.性与暴力[7.3-159][NA][720p]",
//    //".mkv",
//    overwrite: false);

//Video.CreateTVEpisodeMetadata(@"H:\Downloads7\New folder (6)\阅读\_1", f => Path.GetFileNameWithoutExtension(f).Split(".").Last());

static void RenameEpisode(string mediaDirectory, string metadataDirectory, bool isDryRun = false, Action<string>? log = null)
{
    log ??= Logger.WriteLine;

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
        (file, title) => Path.GetFileNameWithoutExtension(file).ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") ? file.ReplaceIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}", $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.{title}") : file.ReplaceIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}", $"{Video.VersionSeparator}{Video.TopForeignKeyword}.{title}"),
        isDryRun,
        log));
    //tvs.ForEach(tv => Video.MoveSubtitlesForEpisodes(tv, Path.Combine(metadataDirectory, Path.GetFileName(tv)), isDryRun: isDryRun, log: log));
}

//Video.MoveTopTVEpisodes(@"E:\Files\Move\TV", @"E:\Files\TV.Subtitle");
//Video.FormatTV(
//    @"E:\Files\TV",
//    @"",
//    renameForTitle: (f, t) => f.Replace($"{Video.VersionSeparator}{Video.TopEnglishKeyword}", $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.{t}"),
//    isDryRun: false);

static void RenameFilesWithDuplicateTitle(
    string directory,
    string titleFlag,
    SearchOption searchOption = SearchOption.TopDirectoryOnly,
    bool isDryRun = false,
    Action<string>? log = null)
{
    log ??= Logger.WriteLine;

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
            string title = videoName[(videoName.LastIndexOfOrdinal(titleFlag) + titleFlag.Length)..];
            if (title.IsNotNullOrWhiteSpace() && title.Length % 2 == 0 && title[..(title.Length / 2)].EqualsIgnoreCase(title[(title.Length / 2)..]))
            {
                string newTitle = title[..(title.Length / 2)];
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
                                FileHelper.Move(episodeFile.Path, newEpisodeFile);
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
//await Entry.DownloadMetadataAsync("http://hotxshare.com", 1, 238, @"D:\Files\Library\Movie.EntryMetadata.json", settings.MovieLibraryMetadata, settings.MovieTopX265Metadata, settings.MovieTopH264Metadata, settings.MoviePreferredMetadata, settings.MovieTopH264720PMetadata, log);
// await Parallel.ForEachAsync(
//    Directory.GetFiles(@"L:\Files3\Movies\Korean", "*", SearchOption.AllDirectories)
//        .Where(f => f.EndsWithIgnoreCase(".mkv") || f.EndsWithIgnoreCase(".mp4"))
//        .Order()
//        .ToArray()
//        .Hide(),
//    new ParallelOptions() { MaxDegreeOfParallelism = 3 },
//    async (f, _) =>
//    {
//        try
//        {
//            await FfmpegHelper.EncodeAsync(f, Path.Combine(@"E:\", Path.GetFileName(f)), estimateCrop: true);
//        }
//        catch (Exception e)
//        {
//            Logger.WriteLine(e.ToString());
//        }
//    });
//Video.PrintMoviesWithoutSubtitle(@"E:\Files\Movies", 2, null, "eng");
//Video.MoveAllSubtitles(@"S:\Files\Library\Movies Temp", @"S:\Files\Library\Movies Temp.Subs");
//string[] files = Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory).Order().ToArray();
//int length = files.Length;
//object @lock = new();
//files.ParallelForEach((file, index) =>
//{
//    log($"{index * 100 / length}% {index}/{length}");
//    string html = File.ReadAllText(file);
//    CQ cq = html;
//    string title = cq.Find("title").Text();
//    if (title.ContainsOrdinal("404") || title.ContainsIgnoreCase("error"))
//    {
//        string line = $"{title} {file}";
//        log(line);
//        lock (@lock)
//        {
//            File.AppendAllLines(file, new string[] { line });
//        }
//    }
//}, 4);
//Video.RenameFiles(@"E:\Files\Move\TV",
//    (f, i) => f
//        .Replace(".2160p.UHD.BluRay.x265.10bit.HDR.DDP5.1.Atmos-RARBG", ".ATMOS.HDR.2160p.BluRay.x265.DDP-RARBG")
//        .Replace(".2160p.UHD.BluRay.x265.10bit.HDR.DDP5.1-RARBG", ".HDR.2160p.BluRay.x265.DDP-RARBG"));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => DirectoryHelper.AddPostfix(d, "[HDR]"));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d=>DirectoryHelper.ReplaceDirectoryName(d,n=>n.Replace("[1080x]","[2160x]")));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => Directory.CreateDirectory(Path.Combine(@"T:\Move\New folder", Path.GetFileName(d))));
//string[] sourceDirectories = Directory.GetDirectories(@"S:\Files\Library\Movies Mainstream.主流电影\American Fiction.美国科幻");
//string[] destinationDirectories = Directory.GetDirectories(@"T:\Files\Library\Movies 4K HDR.高动态范围电影\American Fiction.美国科幻");
//Enumerable.Range(1, 29).Select(index => $"Marvel`{index}-")
//    .ForEach(prefix =>
//    {
//        string sourceDirectory = sourceDirectories.Single(d => Path.GetFileName(d).StartsWithOrdinal(prefix));
//        string destinationDirectory = destinationDirectories.Single(d => Path.GetFileName(d).StartsWithOrdinal(prefix));
//        string sourceNfo = Directory.EnumerateFiles(sourceDirectory).First(f => f.EndsWithOrdinal(".nfo"));
//        string destinationVideo = Directory.EnumerateFiles(destinationDirectory).Single(f => f.EndsWithOrdinal(".mp4"));
//        string destinationName = Path.GetFileNameWithoutExtension(destinationVideo);
//        string destinationNfo = Path.Combine(destinationDirectory, $"{destinationName}.nfo");
//        File.Copy(sourceNfo, destinationNfo, true);

//        string sourceJson = Directory.EnumerateFiles(sourceDirectory).Single(f => f.EndsWithOrdinal(".json"));
//        string destinationJson = Path.Combine(destinationDirectory, Path.GetFileName(sourceJson));
//        File.Copy(sourceJson, destinationJson, true);

//        Directory.EnumerateFiles(sourceDirectory, "*.log")
//            .ForEach(f => File.Copy(f, Path.Combine(destinationDirectory, Path.GetFileName(f)), true));
//    });

//FfmpegHelper.MergeAllDubbed(@"L:\Files3\New folder (3)\DOM.2021"/*, "*.POLISH.1080p.WEBRip.x265-RARBG.mp4", original =>
//{
//    string name = Path.GetFileName(original).Replace(".POLISH.", ".DUBBED.");
//    return Path.Combine(@"U:\Move\Glitter\Season 01", name);
//}*/);
//Video.RenameFiles(@"E:\Files\Move\TV\Black.Sails", (f, i) => f.Replace("..", "."), isDryRun: false);
//await Rare.PrintVersionsAsync(settings.MovieRareMetadata, settings.MovieLibraryMetadata, settings.MovieTopX265Metadata, settings.MovieTopH264720PMetadata, settings.MoviePreferredMetadata, settings.MovieTopH264720PMetadata, log);
//await Rare.DownloadMetadataAsync(
//    "https://rarelust.com/movies-index/", 
//    settings.MovieRareMetadata, 
//    settings.MovieTopX265Metadata, 
//    settings.MovieTopH264Metadata, 
//    settings.MoviePreferredMetadata, 
//    settings.MovieTopH264720PMetadata, 
//    settings.MovieLibraryMetadata, 
//    log);
//Video.MoveMetadata(@"Q:\Files\Movies", settings.MovieMetadataCacheDirectory, settings.MovieMetadataDirectory, 2);
//await Video.PrintMovieLinksAsync(settings.MovieLibraryMetadata, settings.MovieTopX265Metadata, settings.MovieTopH264Metadata, settings.MoviePreferredMetadata, settings.MovieTopH264720PMetadata, settings.MovieRareMetadata, settings.MovieMetadataCacheDirectory, settings.MovieMetadataDirectory, false, null, "female full frontal nudity", "unsimulated sex", "vagina", "labia", "female pubic");

//FfmpegHelper.Encode(@"E:\Files\TV.Encode.Crop\Veneno=Vida y muerte de un icono.2020.毒药-一名偶像的生与死[8.7-4K][TVMA][1080f]\Season 01\Veneno.S01E01.1080p.WEBRip-hdalx.ffmpeg.La.noche.que.cruzamos.el.Mississippi.mkv", estimateCrop: true, sample: true);
//Drive115.DownloadOfflineTasks(
//    "https://115.com/?tab=offline&mode=wangpan",
//    (title, link) => title.Contains("A.History.of.Sex.2003")
//        || title.ContainsIgnoreCase("Jesus.2016")
//        || link.ContainsIgnoreCase("0e394f73e99ed6e9c4e5f8b4a75befd1de1cc0b3")
//        || link.ContainsIgnoreCase("3fef3fb6764a95383c941db41b8a2522ac1cfcd3"),
//    (title, link) => Debugger.Break());
//Drive115.WriteOfflineTasksAsync("https://115.com/?tab=offline&mode=wangpan",
//    @"D:\Files\Library\Drive115.OfflineTasks.json");

//Video.RenameFiles(@"E:\Files\Korean\Even.closer.hautnah.s01.french.web.h264-freamon\Season 01", (f, i) =>
//    f.ReplaceIgnoreCase(@"even.closer.hautnah.s01e0", "Even Closer-Hautnah.S01E0")
//        .Replace(".french.web.h264-freamon", ".FRENCH.WEBRip.H264-Freamon")
//        .Replace(".1080p.BluRay.DTS.x264-SbR", ".1080p.BluRay-SbR.ffmpeg")
//        .Replace(".1080p.WEB-DL.DD+5.1.x264-SbR", ".1080p.WEBRip.H264-SbR")
//    , searchOption: SearchOption.AllDirectories);
//string[] keywords = { "unsimulated sex", "labia", "vagina", "female full frontal nudity", "female pubic" };
//Directory.GetDirectories(@"T:\Files\Library\Movies Mainstream.Temp")
//    .Where(d =>
//    {
//        string name = Path.GetFileName(d);
//        return !name.StartsWith("_") && !name.ContainsIgnoreCase(".");
//    })
//    .ForEach(d => Directory.GetDirectories(d).ForEach(m =>
//    {
//        if (Imdb.TryLoad(m, out ImdbMetadata? imdbMetadata))
//        {
//            keywords.ForEach(keyword =>
//            {
//                if (imdbMetadata.AllKeywords.Any(movieKeyword => movieKeyword.ContainsIgnoreCase(keyword)))
//                {
//                    DirectoryHelper.MoveToDirectory(m, $"{d}.{keyword}");
//                    return false;
//                }

//                return true;
//            });
//        }
//    }));
//string[] sources = Directory.GetDirectories(@"S:\Files\Library\Movies Mainstream.主流电影", "*", SearchOption.AllDirectories);
//Directory.GetDirectories(@"T:\New folder (2)")
//    .ForEach(m =>
//    {
//        string video = Directory.EnumerateFiles(m).First(f => f.EndsWithIgnoreCase(".mp4"));
//        string name = Path.GetFileName(m);
//        string source = sources.First(d => Path.GetFileName(d).EqualsIgnoreCase(name));
//        Directory.GetFiles(source).ForEach(f =>
//        {
//            if (f.EndsWithIgnoreCase(".mp4"))
//            {
//                return;
//            }

//            if (f.EndsWithIgnoreCase(".nfo") )
//            {
//                string destination = PathHelper.ReplaceExtension(video, ".nfo");
//                if(!File.Exists(destination))
//                {
//                    FileHelper.Copy(f, destination);
//                }

//                return;
//            }

//            if (f.EndsWithIgnoreCase(".json")
//                || f.EndsWithIgnoreCase(".log")
//                || f.EndsWithIgnoreCase(".jpg")
//                || f.EndsWithIgnoreCase(".png")
//                || f.EndsWithIgnoreCase(".svg"))
//            {
//                FileHelper.CopyToDirectory(f, m, true);
//                return;
//            }

//            if (Path.GetFileNameWithoutExtension(f).EndsWithIgnoreCase(".chs&eng"))
//            {
//                string destination = PathHelper.ReplaceExtension(video, $".chs&eng{Path.GetExtension(f)}");
//                if (!File.Exists(destination))
//                {
//                    FileHelper.Copy(f, destination);
//                }
//            }
//        });
//    });

//Video.RenameFiles(@"L:\Files3\New folder (4)\New folder", (f, i) =>
//    f
//        .Replace(".chi-3.", ".chs.")
//        .Replace(".chi-4.", ".cht.")
//        .Replace(".jpn-5.", ".jap.")
//        .Replace("红楼梦.", "A Dream in Red Mansions.S01E")
//);

//Directory.EnumerateDirectories(@"L:\Files3\Movies\Private Gold 1-250").ToArray().ForEach((d, i) =>
//{
//    string[] files = Directory.GetFiles(d);
//    string video = files.Single(Video.IsVideo);
//    string xml = files.Single(f => f.HasExtension(Video.XmlMetadataExtension));
//    string metadata = files.Single(f => f.HasExtension(Video.ImdbMetadataExtension));
//    if (Regex.IsMatch(Path.GetFileName(d).Split(".").First(), $" [0-9]+"))
//    {
//        //string p = Path.GetFileNameWithoutExtension(video).Split("-").First()+"-";
//        //DirectoryHelper.ReplaceDirectoryName(d,n=>n.Replace("Private Gold ", "Private Gold`"));
//        //Logger.WriteLine(d);
//    }

//    string[] a = Path.GetFileName(d).Split(".");
//    string title = a[0].Replace("  ", " ");
//    string year = a[1];
//    string tailTitle = title[(title.IndexOf("-") + 1)..];
//    string metadata2 = Path.GetFileName(d)[Path.GetFileName(d).IndexOf("[")..];
//    int index = tailTitle.IndexOfOrdinal("`");
//    string postfix = string.Empty;
//    if (index > 0)
//    {
//        //tailTitle = tailTitle[..index];
//        postfix = tailTitle[index..];
//    }
//    string nt = $"{title}.{year}.{postfix}{metadata2}";
//    //Logger.WriteLine(nt);
//    //DirectoryHelper.ReplaceDirectoryName(d, nt);
//    //if (Imdb.TryLoad(metadata, out ImdbMetadata? imdbMetadata))
//    //{
//    //    string series = Path.GetFileNameWithoutExtension(video).Split("-").First();
//    //    Logger.WriteLine(series);
//    //    if (
//    //        !(imdbMetadata.Title.StartsWithIgnoreCase(series) || imdbMetadata.Title.StartsWithIgnoreCase(series.ReplaceOrdinal(" 0", " ")))
//    //        && !imdbMetadata.Titles.SelectMany(g => g.Value).Any(t => t.StartsWithIgnoreCase(series) || t.StartsWithIgnoreCase(series.ReplaceOrdinal(" 0", " "))))
//    //    {
//    //        Logger.WriteLine(d);
//    //    }
//    //}

//    //if (Video.TryReadVideoMetadata(video, out VideoMetadata? videoMetadata, imdbMetadata))
//    //{
//    //    if (videoMetadata.DefinitionType == DefinitionType.P1080)
//    //    {
//    //        FileHelper.AddPostfix(video, ".1080p");
//    //        FileHelper.AddPostfix(xml, ".1080p");
//    //        FileHelper.ReplaceFileNameWithoutExtension(backupXml, n => n.ReplaceIgnoreCase(".backup", $".1080p.backup"));
//    //        return;
//    //    }

//    //    if (videoMetadata.DefinitionType == DefinitionType.P720)
//    //    {
//    //        FileHelper.AddPostfix(video, ".720p");
//    //        FileHelper.AddPostfix(xml, ".720p");
//    //        FileHelper.ReplaceFileNameWithoutExtension(backupXml, n => n.ReplaceIgnoreCase(".backup", $".720p.backup"));
//    //        return;
//    //    }
//    //}
//    //else
//    //{
//    //    Logger.WriteLine(video);
//    //}
//    //Logger.WriteLine(d);
//});
//Video.RenameDirectoriesWithFormattedNumber(@"D:\Files\Library", SearchOption.AllDirectories, false, null, "Fast & Furious", "速度与激情");

//Video.RenameFiles(@"Q:\Files\TV.Raw\Red Shoe Diaries.1992.[5.7-1.2K][TVMA]",
//   (f, i) =>
//       Regex.Replace(f, @"S([0-9]{1})EP([0-9]{2}) Red Shoe Diaries ", "Red Shoe Diaries.S0$1E$2.", RegexOptions.IgnoreCase)
//       //f.ReplaceIgnoreCase("-BDYS.", "-BDYS.watermark.")
//   //Regex.Replace(f, @" [0-9]+GB", "")
//       );


HashSet<string> downloadedHashes = new HashSet<string>(Directory.GetFiles(@"E:\Files\MonoTorrentDownload").Select(f => Path.GetFileNameWithoutExtension(f).Split("@").Last()), StringComparer.OrdinalIgnoreCase);

//await TorrentHelper.AddDefaultTrackersAsync(@"E:\Files\MonoTorrentDownload", Logger.WriteLine);

//Directory.GetFiles(@"Q:\Files\Movies.Raw3\Korean")
//    .ForEach(f=>FileHelper.MoveToDirectory(f, Path.Combine(@"Q:\Files\Movies.Raw3\Korean", Path.GetFileNameWithoutExtension(f))));

//string[] allDownloadedTitles = new string[]
//{
//   settings.MovieMainstream.Directory,
//   settings.MovieMainstreamWithoutSubtitle.Directory,
//   settings.MovieMusical.Directory,
//   settings.MovieTemp1.Directory,
//   settings.MovieTemp2.Directory,
//   settings.MovieTemp3.Directory,
//   settings.MovieTemp.Directory,
//   settings.MovieControversial.Directory,
//   settings.MovieControversialTemp.Directory,
//   settings.MovieControversialTemp1.Directory,
//   @"Q:\Files\Movies.Raw"
//}.SelectMany(d => Directory.EnumerateFiles(d, "*.mp4", SearchOption.AllDirectories))
//    .Select(f=>Path.GetFileNameWithoutExtension(f)!)
//    .Where(f => f.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") || f.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}"))
//    .Select(f =>
//    {
//        if (f.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}"))
//        {
//            return f[..(f.IndexOfIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}") + $"{Video.VersionSeparator}{Video.TopEnglishKeyword}".Length)];
//        }

//        if (f.ContainsIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}"))
//        {
//            return f[..(f.IndexOfIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}") + $"{Video.VersionSeparator}{Video.TopForeignKeyword}".Length)];
//        }

//        throw new InvalidOperationException(f);
//    })
//    .ToArray();

//File.WriteAllLines(@"e:\AllDownloadedTitles.txt", allDownloadedTitles);

HashSet<string> allDownloadedTitles = new(File.ReadAllLines(@"e:\AllDownloadedTitles.txt"), StringComparer.OrdinalIgnoreCase);

//HashSet<string> downloadedTitles = new(Directory.GetDirectories(@"Q:\Files\Movies\Rarbg_")
//    .Concat(Directory.GetDirectories(@"Q:\Files\Movies\Rarbg_.Dubbed"))
//    .Concat(Directory.GetDirectories(@"Q:\Files\Movies\Rarbg_.Dubbed2"))
//    .Concat(Directory.GetDirectories(@"Q:\Files\Movies\Rarbg2_"))
//    .Select(d => Path.GetFileName(d)!), StringComparer.OrdinalIgnoreCase);
//Debug.Assert(Directory.GetDirectories(@"Q:\Files\Movies\Rarbg_").All(d => Directory.EnumerateFiles(d).Any(Video.IsVideo)));

//Directory.GetDirectories(@"\\beyond-x\E\Files\Downloading")
//    .Where(d => allDownloadedTitles.Contains(Path.GetFileName(d)))
//    .ToArray()
//    .ForEach(d => DirectoryHelper.MoveToDirectory(d, @"\\beyond-x\E\Files\New folder"));


//Directory.EnumerateDirectories(@"\\beyond-x\E\Files\Move").Where(d=>allDownloadedTitles.Contains(Path.GetFileName(d))).ForEach(d=>DirectoryHelper.MoveToDirectory(d, @"\\beyond-x\E\Files\Move.Delete"));

//Directory.GetDirectories(@"E:\Files\Move").Where(d => downloaded.Contains(Path.GetFileName(d))).ToArray()
//    .ForEach(d => DirectoryHelper.MoveToDirectory(d, @"E:\Files\Move.Delete"));

//Directory.GetDirectories(@"Q:\Files\Movies\Rarbg_").Where(d=>Directory.EnumerateFiles(d, "*.json").Count()!=1).ForEach(Logger.WriteLine);

//Video.EnumerateDirectories(@"Q:\Files\Movies\Rarbg_", 1)
//    .GroupBy(d => XDocument.Load(Directory.EnumerateFiles(d, Video.XmlMetadataSearchPattern).First(f => f.EndsWithIgnoreCase(".backup.nfo"))).Root!.Element("imdbid")!.Value)
//    .Where(group => group.Count() > 1)
//    .ToArray()
//    .ForEach(g =>
//    {
//        if (g.All(dd => Regex.IsMatch(dd, @"\.Part[1-9]\.") || Regex.IsMatch(dd, @"\.Part\.[1-9]\.")))
//        {
//            return;
//        }

//        string destinationDirectory;
//        if (g.All(dd => !dd.ContainsIgnoreCase(".DUBBED.")) && g.Any(dd => dd.EndsWithIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}")))
//        {
//            destinationDirectory = g.First(dd => dd.EndsWithIgnoreCase($"{Video.VersionSeparator}{Video.TopForeignKeyword}"));
//        }
//        else if (g.Any(dd => dd.ContainsIgnoreCase(".DUBBED.")) && g.Any(dd => !dd.ContainsIgnoreCase(".DUBBED.")))
//        {
//            destinationDirectory = g.First(dd => !dd.ContainsIgnoreCase(".DUBBED."));
//        }
//        else if (g.Any(dd => dd.ContainsIgnoreCase(".UNRATED.")))
//        {
//            destinationDirectory = g.First(dd => dd.ContainsIgnoreCase(".UNRATED."));
//        }
//        else if (g.Any(dd => dd.ContainsIgnoreCase(".DC.")))
//        {
//            destinationDirectory = g.First(dd => dd.ContainsIgnoreCase(".DC."));
//        }
//        else if (g.Any(dd => dd.ContainsIgnoreCase(".UNCUT.")))
//        {
//            destinationDirectory = g.First(dd => dd.ContainsIgnoreCase(".UNCUT."));
//        }
//        else
//        {
//            destinationDirectory = g.First();
//        }

//        g.Select(dd => dd.EqualsIgnoreCase(destinationDirectory) ? $"{dd} *" : dd).Append("").ForEach(Logger.WriteLine);

//        g.Where(dd => !dd.EqualsIgnoreCase(destinationDirectory))
//            .ToArray()
//            .ForEach(dd =>
//            {
//                Logger.WriteLine(dd);
//                string[] files = Directory.GetFiles(dd).ToArray();
//                files.ForEach(f =>
//                {
//                    Logger.WriteLine(f);
//                    FileHelper.TryMoveToDirectory(f, destinationDirectory, false);
//                });
//            });
//    });
//await Video.DownloadMissingTitlesFromDoubanAsync(@"Q:\Files\Movies");
