using Examples.Common;
using Examples.IO;
using Examples.Net;
using MediaManager;
using MediaManager.IO;
using MediaManager.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SearchOption = System.IO.SearchOption;
using Video = MediaManager.IO.Video;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();
using IHost host = builder.Build();

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("Settings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile("Settings.Debug.json", optional: true, reloadOnChange: true)
#endif
    .AddEnvironmentVariables();
IConfigurationRoot? configuration = configurationBuilder.Build();
Settings settings = configuration.Get<Settings>() ?? throw new InvalidOperationException();
VideoMovieFileInfo.Settings = settings;
VideoEpisodeFileInfo.Settings = settings;

AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");

using TextWriterTraceListener textTraceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
Trace.Listeners.Add(textTraceListener);
using ConsoleTraceListener consoleTraceListener = new();
Trace.Listeners.Add(consoleTraceListener);

Console.OutputEncoding = Encoding.UTF8; // Or Unicode.

//Video.PrintDirectoriesWithMultipleMedia(settings.MovieControversial);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstream);

//await Video.DownloadImdbMetadataAsync(settings.Movie3D, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieHdr, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversial, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversialTemp3, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversialWithoutSubtitle, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstream, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstreamWithoutSubtitle, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieMusical, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp1, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp2, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp3, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp31, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp32, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp3Encode, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies Mainstream.主流电影\Test", 1, overwrite: true, useCache: false, useBrowser: true);

//await Video.DownloadImdbMetadataAsync(settings.TVControversial, 1, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVDocumentary, 1, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstream, 1, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstreamWithoutSubtitle, 1, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVTemp3, 1, overwrite: true, useCache: false, useBrowser: true);

// Video.MoveFanArt(settings.Movie3D);
// Video.MoveFanArt(settings.MovieHdr);
// Video.MoveFanArt(settings.MovieControversial);
// Video.MoveFanArt(settings.MovieControversialTemp3);
// Video.MoveFanArt(settings.MovieControversialWithoutSubtitle);
// Video.MoveFanArt(settings.MovieMainstream);
// Video.MoveFanArt(settings.MovieMainstreamWithoutSubtitle);
// Video.MoveFanArt(settings.MovieMusical);
// Video.MoveFanArt(settings.MovieTemp1);
// Video.MoveFanArt(settings.MovieTemp2);
// Video.MoveFanArt(settings.MovieTemp3);
// Video.MoveFanArt(settings.MovieTemp31);
// Video.MoveFanArt(settings.MovieTemp32);
// Video.MoveFanArt(settings.MovieTemp3Encode);

// Video.MoveFanArt(settings.TVControversial, 1);
// Video.MoveFanArt(settings.TVDocumentary, 1);
// Video.MoveFanArt(settings.TVMainstream, 1);
// Video.MoveFanArt(settings.TVMainstreamWithoutSubtitle, 1);
// Video.MoveFanArt(settings.TVTemp3, 1);

//await Video.DownloadImdbMetadataAsync(
//    new (string Directory, int Level)[]
//    {
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

//Video.PrintDirectoryTitleMismatch(settings.Movie3D);
//Video.PrintDirectoryTitleMismatch(settings.MovieControversial);
//Video.PrintDirectoryTitleMismatch(settings.MovieMainstream);

//Video.PrintDirectoryTitleMismatch(settings.TVControversial, level: 1);
//Video.PrintDirectoryTitleMismatch(settings.TVDocumentary, level: 1);
//Video.PrintDirectoryTitleMismatch(settings.TVMainstream, level: 1);

//Video.PrintDirectoryOriginalTitleMismatch(settings.Movie3D);
//Video.PrintDirectoryOriginalTitleMismatch(settings.MovieControversial);
//Video.PrintDirectoryOriginalTitleMismatch(settings.MovieMainstream);

//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.Movie3D);
//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.MovieControversial);
//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.MovieMainstream);

// Video.RenameDirectoriesWithImdbMetadata(settings, settings.Movie3D, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieHdr, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieControversial, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieControversialTemp3, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieControversialWithoutSubtitle, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieMainstream, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieMainstreamWithoutSubtitle, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieMusical, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp1, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp2, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp3, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp31, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp32, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp3Encode, isDryRun: true);

// Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVControversial, level: 1, isTV: true, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVDocumentary, level: 1, isTV: true, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVMainstream, level: 1, isTV: true, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVMainstreamWithoutSubtitle, level: 1, isTV: true, isDryRun: true);
// Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVTemp3, level: 1, isTV: true, isDryRun: true);

// Video.UpdateXmlRating(settings.Movie3D);
// Video.UpdateXmlRating(settings.MovieHdr);
// Video.UpdateXmlRating(settings.MovieControversial);
// Video.UpdateXmlRating(settings.MovieControversialTemp3);
// Video.UpdateXmlRating(settings.MovieControversialWithoutSubtitle);
// Video.UpdateXmlRating(settings.MovieMainstream);
// Video.UpdateXmlRating(settings.MovieMainstreamWithoutSubtitle);
// Video.UpdateXmlRating(settings.MovieMusical);
// Video.UpdateXmlRating(settings.MovieTemp1);
// Video.UpdateXmlRating(settings.MovieTemp2);
// Video.UpdateXmlRating(settings.MovieTemp3);
// Video.UpdateXmlRating(settings.MovieTemp31);
// Video.UpdateXmlRating(settings.MovieTemp32);
// Video.UpdateXmlRating(settings.MovieTemp3Encode);

// Video.UpdateXmlRating(settings.TVControversial, 1);
// Video.UpdateXmlRating(settings.TVDocumentary, 1);
// Video.UpdateXmlRating(settings.TVMainstream, 1);
// Video.UpdateXmlRating(settings.TVMainstreamWithoutSubtitle, 1);
// Video.UpdateXmlRating(settings.TVTemp3, 1);

// Video.PrintDirectoriesWithErrors(settings, settings.Movie3D);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieHdr);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieControversial);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieControversialTemp3);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieControversialWithoutSubtitle);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieMainstream);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieMainstreamWithoutSubtitle);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieMusical);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp1);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp2);
// Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp3);

// Video.PrintDirectoriesWithErrors(settings, settings.TVControversial, 1, isTV: true);
// Video.PrintDirectoriesWithErrors(settings, settings.TVDocumentary, 1, isTV: true);
// Video.PrintDirectoriesWithErrors(settings, settings.TVMainstream, 1, isTV: true);
// Video.PrintDirectoriesWithErrors(settings, settings.TVTemp3, 1, isTV: true);
// Video.PrintDirectoriesWithErrors(settings, settings.TVMainstreamWithoutSubtitle, 1, isTV: true);

//Video.PrintVideosWithErrors(settings.Movie3D, searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(settings.MovieControversial, searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(settings.MovieMainstream, searchOption: SearchOption.AllDirectories);

//await Video.ConvertToUtf8Async(settings.Movie3D);
//await Video.ConvertToUtf8Async(settings.MovieMainstream);
//await Video.ConvertToUtf8Async(settings.MovieControversial);

//await Video.ConvertToUtf8Async(settings.TVControversial);
//await Video.ConvertToUtf8Async(settings.TVDocumentary);
//await Video.ConvertToUtf8Async(settings.TVMainstream);
//await Video.ConvertToUtf8Async(settings.TVTutorial);
//await Video.ConvertToUtf8Async(settings.LibraryDirectory);

//await Video.RenameSubtitlesByLanguageAsync(settings.MovieTemp, isDryRun: true);

//Video.DeleteFeaturettesMetadata(settings.Movie3D, isDryRun: true);
//Video.DeleteFeaturettesMetadata(settings.MovieMainstream, isDryRun: true);
//Video.DeleteFeaturettesMetadata(settings.MovieControversial, isDryRun: true);

//Video.PrintDirectoriesWithMultipleMedia(settings.Movie3D);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieHdr);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieControversial);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstream);
//Video.PrintDirectoriesWithMultipleMedia(settings.MovieMainstreamWithoutSubtitle);

//Video.PrintSubtitlesWithErrors(settings.LibraryDirectory);

//Video.PrintMoviesWithoutSubtitle(settings.MovieControversial);
//Video.PrintMoviesWithoutSubtitle(settings.MovieMainstream);

//Video.PrintDuplicateImdbId(null,
//   settings.MovieMainstream,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieTemp31,
//   settings.MovieTemp32,
//   settings.MovieTemp3Encode,
//   settings.MovieControversial,
//   settings.MovieControversialWithoutSubtitle,
//   settings.MovieControversialTemp3);

//Video.PrintDefinitionErrors(settings.LibraryDirectory);

//await Video.ConvertToUtf8Async(settings.LibraryDirectory);

// await Video.WriteLibraryMovieMetadata(settings, null,
//    settings.MovieControversial,
//    settings.MovieControversialWithoutSubtitle,
//    settings.MovieControversialTemp3,
//    settings.MovieMainstream,
//    settings.MovieMainstreamWithoutSubtitle,
//    settings.MovieMusical,
//    settings.MovieTemp1,
//    settings.MovieTemp2,
//    settings.MovieTemp3,
//    settings.MovieTemp31,
//    settings.MovieTemp32,
//    settings.MovieTemp3Encode);
//await Video.WriteExternalVideoMetadataAsync(settings.MovieExternalMetadata, settings.MovieTemp);
//await Video.CompareAndMoveAsync(settings.MovieExternalMetadata, settings.MovieLibraryMetadata, settings.MovieExternalNew, settings.MovieExternalDelete, isDryRun: false);

//Video.MoveAllSubtitles(settings.MovieTemp, settings.MovieSubtitleBackupDirectory);
//await Drive115.WriteOfflineTasksAsync(settings.Drive115Url, settings.Drive115Metadata, "Goto.Isle.of.Love.1969");

//await Video.PrintMovieImdbIdErrorsAsync(settings, null,
//    settings.MovieControversial,
//    settings.MovieControversialTemp3,
//    settings.MovieControversialWithoutSubtitle,
//    settings.MovieMainstream,
//    settings.MovieMainstreamWithoutSubtitle,
//    settings.MovieMusical,
//    settings.MovieTemp1,
//    settings.MovieTemp2,
//    settings.MovieTemp3,
//    settings.MovieTemp3Encode,
//    (@"D:\Files\Library\Movies Temp3.2.电影3.2\Rarbg", 1),
//    (@"D:\Files\Library\Movies Temp3.2.电影3.2\Rarbg8.2", 1)
//);

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
//await Preferred.DownloadMetadataAsync(settings, index => index <= 250);
//await Video.PrintMovieVersions(settings, null,
//   settings.MovieMainstream,
//   settings.MovieControversial,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieTemp31,
//   settings.MovieTemp32,
//   settings.MovieTemp3Encode,
//   settings.MovieControversialWithoutSubtitle,
//   settings.MovieControversialTemp3);
//await Video.PrintTVVersions(settings.TVTopX265Metadata, null,
//    settings.TVControversial,
//    settings.TVDocumentary,
//    settings.TVMainstream,
//    settings.TVMainstreamWithoutSubtitle);

//await Imdb.DownloadAllMoviesAsync(
//    settings,
//    count => ..);
//await Imdb.DownloadAllTVsAsync(settings.TVTopX265Metadata, settings.TVMainstream, settings.TVMetadataCacheDirectory, settings.TVMetadataDirectory);
string[] genres = ["family", "animation", "documentary"];
//await Video.PrintTVLinks(settings.TVTopX265Metadata, new string[] { settings.TVMainstream, settings.TVMainstreamWithoutSubtitle }, @"D:\Files\Library\TVMetadata", @"D:\Files\Library\TVMetadataCache", settings.TVTopX265Url,
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
//await Video.UpdateMergedMovieMetadataAsync(settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, @"D:\Files\Library\Movie.MergedMetadata.json", settings.MovieLibraryMetadata);
//await Video.MergeMovieMetadataAsync(settings.MovieMetadataDirectory, @"D:\Files\Library\Movie.MergedMetadata.json");
//HashSet<string> keywords = new(settings.AllImdbKeywords, StringComparer.OrdinalIgnoreCase);
//await Video.PrintMovieLinksAsync(
//    settings,
//    imdbMetadata =>
//    //imdbMetadata.AllKeywords.Intersect(new string[] { "test" }, StringComparer.OrdinalIgnoreCase).IsEmpty()
//    // &&imdbMetadata.Genres.Intersect(genres, StringComparer.OrdinalIgnoreCase).IsEmpty()
//    //&& imdbMetadata.Genres.Intersect(new string[] { "action" }, StringComparer.OrdinalIgnoreCase).Any()
//     (imdbMetadata.Advisories
//            .Where(advisory => advisory.Key.ContainsIgnoreCase("sex") || advisory.Key.ContainsIgnoreCase("nudity"))
//            .SelectMany(advisory => advisory.Value)
//            .Any(advisory => advisory.FormattedSeverity == ImdbAdvisorySeverity.Severe)
//        || imdbMetadata.AllKeywords.Any(keyword => keywords.Contains(keyword)))
//    //&& string.Compare(imdbMetadata.FormattedAggregateRating, "8.0", StringComparison.Ordinal) <= 0
//    //&& string.Compare(imdbMetadata.FormattedAggregateRating, "7.0", StringComparison.Ordinal) >= 0
//    //&& imdbMetadata.AggregateRating?.RatingCount >= 10_000
//    //&& imdbMetadata.AllKeywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any()
//    , keywords,
//    isDryRun: true);

//await Preferred.DownloadMetadataAsync(settings.MoviePreferredSummary, settings.MoviePreferredMetadata, log, 4);
//Audio.ReplaceTraditionalChinese(settings.AudioMainstream, true);

//Audio.PrintDirectoriesWithErrors(settings.AudioControversial);
//Audio.PrintDirectoriesWithErrors(settings.AudioMainstream);
//Audio.PrintDirectoriesWithErrors(settings.AudioShow);
//Audio.PrintDirectoriesWithErrors(settings.AudioSoundtrack);

//Video.MoveAllSubtitles(settings.MovieTemp, settings.MovieSubtitleBackupDirectory);

//Video.RenameDirectoriesWithMetadata(settings.TVTemp3, 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithAdditionalMetadata(settings.MovieTemp, 2);
//Video.RenameDirectoriesWithoutMetadata(settings.MovieControversial, isDryRun: false);

//Video.RenameEpisodes(@"D:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]", "European Adult");
//Video.CreateTVEpisodeMetadata(@"T:\Files\Library\TV Tutorial.教程\English Toefl.2017.英语托福[---][-]");

//Video.PrintVideosWithErrors(@"E:\Files\Korean", searchOption: SearchOption.AllDirectories);
//Video.RenameDirectoriesWithMetadata(settings.MovieTemp, 2);
//Video.RestoreMetadata(settings.MovieTemp);

//Video.EnumerateDirectories(settings.MovieMainstream)
//   .Concat(Video.EnumerateDirectories(settings.MovieMainstreamWithoutSubtitle))
//   .Select(m => (m, new VideoDirectoryInfo(m)))
//   .Where(m => !m.Item2.IsHD)
//   .OrderBy(d => d)
//   .ForEach(m => log(m.Item1));

//Video.PrintMovieRegionsWithErrors(settings.MovieRegions, log, settings.MovieControversial, settings.MovieMainstream, settings.MovieMainstreamWithoutSubtitle);

//Video.EnumerateDirectories(settings.MovieHdr)
//    .Concat(Video.EnumerateDirectories(settings.Movie3D))
//    .Concat(Video.EnumerateDirectories(settings.MovieControversial))
//    .Concat(Video.EnumerateDirectories(settings.MovieMainstream))
//    .Concat(Video.EnumerateDirectories(settings.MovieMainstreamWithoutSubtitle))
//    .Where(d => new VideoDirectoryInfo(d).AggregateRating.CompareOrdinal("7.5") >= 0 && Imdb.TryLoad(d, out ImdbMetadata? metadata) && metadata.AggregateRating?.RatingCount < 5000)
//    .OrderBy(d => d)
//    .ToArray()
//    .ForEach(d =>
//    {
//        log(d);
//    });
//Directory.GetDirectories(@"N:\Files\Library\New folder").ToArray().ForEach(d => { 
//Video.RenameEpisodesWithTitle(
//    @"",
//    @"",
//    rename: (f, t) =>
//    {
//        //t = t.Substring(0, 1).ToUpper() + t.Substring(1);
//        //string postfix = PathHelper.GetFileNameWithoutExtension(f).EndsWithIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio") ? $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio" : $"{Video.VersionSeparator}{Video.TopEnglishKeyword}";
//        string postfix = $"{Video.VersionSeparator}{settings.TopEnglishKeyword}";
//        //Debug.Assert(!f.IsVideo() || PathHelper.GetFileNameWithoutExtension(f).EndsWithIgnoreCase(postfix));
//        string name = PathHelper.GetFileNameWithoutExtension(f);
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

//await Video.PrintMovieImdbIdErrorsAsync(settings, true, null, settings.MovieTemp31);
//Video.MoveSubtitleToParentDirectory(settings.MovieTemp);
//Video.MoveFanArt(settings.MovieTemp);
//Video.MoveMetadata(settings.MovieTemp, settings.MovieMetadataCacheDirectory, settings.MovieMetadataDirectory, 2);
//Video.BackupMetadata(settings.MovieTemp);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp, 2, overwrite: false, useCache: true, useBrowser: true, 1);
//Video.PrintDuplicateImdbId(null, settings.MovieTemp);
//await Video.DownloadMissingTitlesFromDoubanAsync(settings.MovieTemp);
//Video.PrintVideosWithErrors(settings.MovieTemp32, searchOption: SearchOption.AllDirectories);
//Video.RenameDirectoriesWithMetadata(settings.MovieTemp, isDryRun: false);
//Video.RestoreMetadata(settings.MovieTemp);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieMainstream);
//Video.RenameDirectoriesWithImdbMetadata(settings.MovieTemp, isDryRun: false);
//Video.RemoveSubtitleSuffix(settings.MovieTemp31);

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

//Video.CreateTVEpisodeMetadata(@"H:\Downloads7\New folder (6)\阅读\_1", f => PathHelper.GetFileNameWithoutExtension(f).Split(".").Last());

static void RenameEpisode(ISettings settings, string mediaDirectory, string metadataDirectory, bool isDryRun = false, Action<string>? log = null)
{
    log ??= Logger.WriteLine;

    string[] tvs = Directory.EnumerateDirectories(mediaDirectory)
        .Where(tv => VideoDirectoryInfo.TryParse(tv, out _))
        .ToArray();
    tvs.ForEach(tv =>
    {
        string metadataTV = Path.Combine(metadataDirectory, PathHelper.GetFileName(tv));
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

        string[] seasonNames = seasons.Select(PathHelper.GetFileName).ToArray();
        string[] metadataSeasonNames = metadataSeasons.Select(PathHelper.GetFileName).ToArray();
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
            .Select(episode => Regex.Match(PathHelper.GetFileNameWithoutExtension(episode), @"\.(S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.").Groups[1].Value)
            .OrderBy(number => number)
            .ToArray();
        string[] metadataEpisodeNumbers = metadataSeasons
            .SelectMany(Directory.EnumerateFiles)
            .Where(Video.IsVideo)
            .Select(episode => Regex.Match(PathHelper.GetFileNameWithoutExtension(episode), @"\.(S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.").Groups[1].Value)
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
        Path.Combine(metadataDirectory, PathHelper.GetFileName(tv)),
        (file, title) => PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") ? file.ReplaceIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}", $"{Video.VersionSeparator}{settings.TopEnglishKeyword}.{title}") : file.ReplaceIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}", $"{Video.VersionSeparator}{settings.TopForeignKeyword}.{title}"),
        isDryRun,
        log));
    //tvs.ForEach(tv => Video.MoveSubtitlesForEpisodes(tv, Path.Combine(metadataDirectory, PathHelper.GetFileName(tv)), isDryRun: isDryRun, log: log));
}

//Video.MoveTopTVEpisodes(settings, @"\\beyond-r\F\Files\Library\TV", @"\\beyond-r\F\Files\TV.Subtitles", isDryRun: false);
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
        .Select(file => (file, Regex.Match(PathHelper.GetFileNameWithoutExtension(file), @"\.S\d+E\d+")))
        .Where(file => file.Item2.Success)
        .Select(file => (file.Item1, file.Item2.Value))
        .OrderBy(file => file.Item1)
        .ToArray();

    files
        .Where(file => Video.IsVideo(file.Path))
        .ToArray()
        .ForEach(video =>
        {
            string videoName = PathHelper.GetFileNameWithoutExtension(video.Path);
            string title = videoName[(videoName.LastIndexOfOrdinal(titleFlag) + titleFlag.Length)..];
            if (title.IsNotNullOrWhiteSpace() && title.Length % 2 == 0 && title[..(title.Length / 2)].EqualsIgnoreCase(title[(title.Length / 2)..]))
            {
                string newTitle = title[..(title.Length / 2)];
                files
                    .Where(file => file.Number.EqualsIgnoreCase(video.Number))
                    .ToArray()
                    .ForEach(episodeFile =>
                    {
                        string newEpisodeFile = Path.Combine(PathHelper.GetDirectoryName(episodeFile.Path), $"{PathHelper.GetFileNameWithoutExtension(episodeFile.Path).ReplaceIgnoreCase(title, newTitle)}{PathHelper.GetExtension(episodeFile.Path)}");
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
//        string d = PathHelper.GetDirectoryName(f);
//        string name = PathHelper.GetFileNameWithoutExtension(f).Trim();
//        string idWithCD = Regex.Match(name, @"^[A-Z\-]+(\-| )?[0-9]+(\-c|\-leak|\-patched)?(-cd[0-9]+)?", RegexOptions.IgnoreCase).Value;
//        Debug.Assert(idWithCD.IsNotNullOrWhiteSpace());
//        string id = Regex.Match(name, @"^[A-Z\-]+(\-| )?[0-9]+(\-c|\-leak|\-patched)?", RegexOptions.IgnoreCase).Value;

//        string newFile = Path.Combine(d, id, $"{idWithCD}{PathHelper.GetExtension(f).ToLowerInvariant()}");
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
//            await FfmpegHelper.EncodeAsync(f, Path.Combine(@"E:\", PathHelper.GetFileName(f)), estimateCrop: true);
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

//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => DirectoryHelper.AddPostfix(d, "[HDR]"));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d=>DirectoryHelper.ReplaceDirectoryName(d,n=>n.Replace("[1080x]","[2160x]")));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => Directory.CreateDirectory(Path.Combine(@"T:\Move\New folder", PathHelper.GetFileName(d))));
//string[] sourceDirectories = Directory.GetDirectories(@"S:\Files\Library\Movies Mainstream.主流电影\American Fiction.美国科幻");
//string[] destinationDirectories = Directory.GetDirectories(@"T:\Files\Library\Movies 4K HDR.高动态范围电影\American Fiction.美国科幻");
//Enumerable.Range(1, 29).Select(index => $"Marvel`{index}-")
//    .ForEach(prefix =>
//    {
//        string sourceDirectory = sourceDirectories.Single(d => PathHelper.GetFileName(d).StartsWithOrdinal(prefix));
//        string destinationDirectory = destinationDirectories.Single(d => PathHelper.GetFileName(d).StartsWithOrdinal(prefix));
//        string sourceNfo = Directory.EnumerateFiles(sourceDirectory).First(f => f.EndsWithOrdinal(".nfo"));
//        string destinationVideo = Directory.EnumerateFiles(destinationDirectory).Single(f => f.EndsWithOrdinal(".mp4"));
//        string destinationName = PathHelper.GetFileNameWithoutExtension(destinationVideo);
//        string destinationNfo = Path.Combine(destinationDirectory, $"{destinationName}.nfo");
//        File.Copy(sourceNfo, destinationNfo, true);

//        string sourceJson = Directory.EnumerateFiles(sourceDirectory).Single(f => f.EndsWithOrdinal(".json"));
//        string destinationJson = Path.Combine(destinationDirectory, PathHelper.GetFileName(sourceJson));
//        File.Copy(sourceJson, destinationJson, true);

//        Directory.EnumerateFiles(sourceDirectory, "*.log")
//            .ForEach(f => File.Copy(f, Path.Combine(destinationDirectory, PathHelper.GetFileName(f)), true));
//    });

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
//        string name = PathHelper.GetFileName(d);
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
//        string name = PathHelper.GetFileName(m);
//        string source = sources.First(d => PathHelper.GetFileName(d).EqualsIgnoreCase(name));
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

//            if (PathHelper.GetFileNameWithoutExtension(f).EndsWithIgnoreCase(".chs&eng"))
//            {
//                string destination = PathHelper.ReplaceExtension(video, $".chs&eng{PathHelper.GetExtension(f)}");
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
//    string metadata = files.Single(f => f.HasExtension(ImdbMetadata.Extension));
//    if (Regex.IsMatch(PathHelper.GetFileName(d).Split(".").First(), $" [0-9]+"))
//    {
//        //string p = PathHelper.GetFileNameWithoutExtension(video).Split("-").First()+"-";
//        //DirectoryHelper.ReplaceDirectoryName(d,n=>n.Replace("Private Gold ", "Private Gold`"));
//        //Logger.WriteLine(d);
//    }

//    string[] a = PathHelper.GetFileName(d).Split(".");
//    string title = a[0].Replace("  ", " ");
//    string year = a[1];
//    string tailTitle = title[(title.IndexOf("-") + 1)..];
//    string metadata2 = PathHelper.GetFileName(d)[PathHelper.GetFileName(d).IndexOf("[")..];
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
//    //    string series = PathHelper.GetFileNameWithoutExtension(video).Split("-").First();
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


//await TorrentHelper.AddDefaultTrackersAsync(@"E:\Files\MonoTorrentDownload", Logger.WriteLine);

//Directory.GetFiles(@"Q:\Files\Movies.Raw3\Korean")
//    .ForEach(f=>FileHelper.MoveToDirectory(f, Path.Combine(@"Q:\Files\Movies.Raw3\Korean", PathHelper.GetFileNameWithoutExtension(f))));

//string[] allDownloadedTitles = new string[]
//{
//   settings.MovieMainstream,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieTemp,
//   settings.MovieControversial,
//   settings.MovieControversialWithoutSubtitle,
//   settings.MovieControversialTemp3,
//   @"Q:\Files\Movies.Raw"
//}.SelectMany(d => Directory.EnumerateFiles(d, "*.mp4", SearchOption.AllDirectories))
//    .Select(f=>PathHelper.GetFileNameWithoutExtension(f)!)
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

//HashSet<string> allDownloadedTitles = new(File.ReadAllLines(@"e:\AllDownloadedTitles.txt"), StringComparer.OrdinalIgnoreCase);

//Directory.GetDirectories(@"\\beyond-x\E\Files\Downloading")
//    .Where(d => allDownloadedTitles.Contains(PathHelper.GetFileName(d)))
//    .ToArray()
//    .ForEach(d => DirectoryHelper.MoveToDirectory(d, @"\\beyond-x\E\Files\New folder"));


//Directory.EnumerateDirectories(@"\\beyond-x\E\Files\Move").Where(d=>allDownloadedTitles.Contains(PathHelper.GetFileName(d))).ForEach(d=>DirectoryHelper.MoveToDirectory(d, @"\\beyond-x\E\Files\Move.Delete"));

//Directory.GetDirectories(@"E:\Files\Move").Where(d => downloaded.Contains(PathHelper.GetFileName(d))).ToArray()
//    .ForEach(d => DirectoryHelper.MoveToDirectory(d, @"E:\Files\Move.Delete"));

//Video.EnumerateDirectories(@"Q:\Files\Movies\_", 1)
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

//await TorrentHelper.DownloadAllFromCacheAsync(@"D:\User\Downloads\ToDownload.txt", @"E:\Files\Torrents", log: Logger.WriteLine);
//await TorrentHelper.DownloadAllFromCache2Async(@"D:\User\Downloads\ToDownload.txt", @"E:\Files\Torrents", "EBF23D9F32EBE73317A927E57C39A5FF23FE4297", Logger.WriteLine);
//await TorrentHelper.DownloadAllFromCache3Async(@"D:\User\Downloads\ToDownload.txt", @"E:\Files\Torrents", Logger.WriteLine);
//await TorrentHelper.DownloadAllAsync(@"D:\User\Downloads\ToDownload.txt", @"E:\Files\Torrents", log: Logger.WriteLine);
//await TorrentHelper.PrintNotDownloadedAsync(@"D:\User\Downloads\ToDownload.txt", @"E:\Files\Torrents", addTrackers: true, log: Logger.WriteLine);
//await TorrentHelper.AddDefaultTrackersAsync(@"E:\Files\Torrents", log: Logger.WriteLine);

//Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, Dictionary<string, VideoMetadata>>>(settings.MovieLibraryMetadata);
//HashSet<string> titles = new(libraryMetadata.Values.AsParallel().SelectMany(d=>d.Keys).Select(f=>PathHelper.GetFileNameWithoutExtension(f)!).Distinct(StringComparer.InvariantCultureIgnoreCase));
//downloadedTitles.AsParallel().Where(downloadedTitle => titles.Any(t=>t.StartsWith(downloadedTitle, StringComparison.OrdinalIgnoreCase))).ForEach(Logger.WriteLine);
//HashSet<string> xx = new(Directory.EnumerateDirectories(@"E:\Files\Delete").Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);

//Video.EnumerateDirectories(@"\\Beyond-r\f\Files\Library\Movies Temp3.电影3")
//    .Select(d => (d, Directory.EnumerateFiles(d, "*.nfo").Select(f => XDocument.Load(f).Root!.Element("tmdbid")?.Value ?? string.Empty).Distinct().Single()))
//    .GroupBy(m => m.Item2)
//    .Where(g => g.Count() > 1)
//    .ForEach(g => g.Select(m => m.d).Order().Append(string.Empty).Prepend(g.Key).ForEach(Logger.WriteLine));

//await Video.WriteNikkatsMetadataAsync(@"D:\Files\Library\Movie.Nikkatsu..txt", @"D:\Files\Library\Movie.Nikkatsu..json");

//Video.EnumerateDirectories(@"\\Beyond-r\f\Files\Library\Movies Temp3.2.电影3.2")
//    .ToArray()
//    .ForEach(m =>
//    {
//        string[] files = Directory.GetFiles(m);
//        string[] videos = files.Where(f => f.EndsWithIgnoreCase(".mkv")).ToArray();
//        if (videos.IsEmpty())
//        {
//            return;
//        }

//        string[] subtitles = files.Where(Video.IsSubtitle).ToArray();
//        if (subtitles.Any(f => Regex.IsMatch(PathHelper.GetFileNameWithoutExtension(f).Split(".").Last(), @"[a-z]{3}\-[0-9]")))
//        {
//            return;
//        }

//        if (videos.Any(v => Video.ReadVideoMetadataAsync(v).Result.Subtitle > 0))
//        {
//            Logger.WriteLine(m);
//            DirectoryHelper.Move(m, m.Replace(@"\Movies Temp3.2.电影3.2\", @"\Movies Temp3.电影3\"));
//        }
//    });

//Directory.EnumerateFiles(@"D:\Files\Library\Movies Temp3.2.电影3.2\New folder", "*.nfo", SearchOption.AllDirectories)
//    .ForEach(metadata =>
//    {
//        string title = XDocument.Load(metadata).Root!.Element("title")!.Value;
//        string name = PathHelper.GetFileNameWithoutExtension(metadata);
//        string year = Regex.Match(name, @"\.[0-9]{4}").Value;
//        string newName = title.Replace(": ", "-").Replace(":", "-") + name.Substring(name.IndexOf(year));
//        FileHelper.ReplaceFileNameWithoutExtension(metadata, newName);

//        string d = PathHelper.GetDirectoryName(metadata)!;
//        Directory
//            .EnumerateFiles(d)
//            .Where(f => PathHelper.GetFileNameWithoutExtension(f).StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
//            .ForEach(f =>
//            {
//                FileHelper.ReplaceFileNameWithoutExtension(f, n => n.Replace(name, newName));
//            });
//    });

//Video.FormatVideoFileNames(@"\\beyond-r\F\Files\Library\Movies", SearchOption.AllDirectories, isDryRun: true);
