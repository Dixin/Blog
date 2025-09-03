using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using Examples.Text;
using MediaManager;
using MediaManager.IO;
using MediaManager.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;
using SearchOption = System.IO.SearchOption;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
using IHost host = builder.Build();

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("Settings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile("Settings.Debug.json", optional: true, reloadOnChange: true)
#endif
    .AddEnvironmentVariables();
IConfigurationRoot configuration = configurationBuilder.Build();
Settings settings = configuration.Get<Settings>() ?? throw new InvalidOperationException();
VideoMovieFileInfo.Settings = settings;
VideoEpisodeFileInfo.Settings = settings;

using CancellationTokenSource cancellationTokenSource = new();
CancellationToken cancellationToken = cancellationTokenSource.Token;
AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
using TextWriterTraceListener textTraceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
//Trace.Listeners.Add(textTraceListener);
using ConsoleTraceListener consoleTraceListener = new();
Trace.Listeners.Add(consoleTraceListener);
Console.OutputEncoding = Encoding.UTF8; // Or Unicode.
FFmpeg.SetExecutablesPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-8.0-full_build\bin"));
Action<string> log = Logger.WriteLine;

string[][] mediaDrives = [
    [
        settings.TVControversial,
        settings.TVDocumentary,
        settings.TVMainstream,
        settings.TVMainstreamWithoutSubtitle,
        settings.TVHdr
    ],
    [
        settings.MovieMainstream,
        settings.MovieMainstreamWithoutSubtitle,
        settings.MovieTemp1
    ],
    [
        settings.Movie3D,
        settings.MovieHdr,
        settings.MovieControversial,
        settings.MovieControversialWithoutSubtitle,
        settings.MovieTemp2,
        settings.MovieMusical,
        settings.MovieDisk,
        settings.TVTutorial
    ],
    [
        settings.MovieTemp3,
        settings.MovieTemp31,
        settings.MovieTemp32
    ],
    [
        settings.MovieControversialTemp4,
        settings.MovieTemp4,
        settings.MovieTemp41,
        settings.MovieTemp42,
        settings.MovieTemp4Encode,
        settings.TVTemp4
    ]
];

string[][] movieDrives = [
   [
        settings.MovieMainstream,
        settings.MovieMainstreamWithoutSubtitle,
        settings.MovieTemp1
   ],
   [
        settings.Movie3D,
        settings.MovieHdr,
        settings.MovieControversial,
        settings.MovieControversialWithoutSubtitle,
        settings.MovieTemp2,
        settings.MovieMusical,
        settings.MovieDisk
   ],
   [
        settings.MovieTemp3,
        settings.MovieTemp31,
        settings.MovieTemp32
   ],
   [
        settings.MovieControversialTemp4,
        settings.MovieTemp4,
        settings.MovieTemp41,
        settings.MovieTemp42,
        settings.MovieTemp4Encode
   ]
];

string[][] sdrMovieDrives = [
   [
        settings.MovieMainstream,
        settings.MovieMainstreamWithoutSubtitle,
        settings.MovieTemp1
   ],
   [
        settings.MovieControversial,
        settings.MovieControversialWithoutSubtitle,
        settings.MovieTemp2,
        settings.MovieMusical
   ],
   [
        settings.MovieTemp3,
        settings.MovieTemp31,
        settings.MovieTemp32
   ],
   [
        settings.MovieControversialTemp4,
        settings.MovieTemp4,
        settings.MovieTemp41,
        settings.MovieTemp42,
        settings.MovieTemp4Encode
   ]
];

string[][] tvDrives = [
    [
        settings.TVTutorial
    ],
    [
        settings.TVControversial,
        settings.TVDocumentary,
        settings.TVMainstream,
        settings.TVMainstreamWithoutSubtitle,
        settings.TVHdr
    ],
    [
        settings.TVTemp4
    ]
];

string[][] metadataDrives = [
    [
        settings.MovieMetadataDirectory,
        settings.MovieMetadataBackupDirectory,
        settings.TVMetadataDirectory
    ]
];

//Video.PrintDirectoriesWithMultipleVideos(settings.MovieControversial);
//Video.PrintDirectoriesWithMultipleVideos(settings.MovieMainstream);

//await Video.DownloadImdbMetadataAsync(settings.Movie3D, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieHdr, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversial, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversialTemp4, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieControversialWithoutSubtitle, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstream, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieMainstreamWithoutSubtitle, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieMusical, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp1, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp2, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp3, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp31, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp32, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp4, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp41, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp42, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp4Encode, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.MovieDisk, 2, overwrite: true, useCache: false, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(Path.Combine(settings.MovieMainstream, "Test"), 1, overwrite: false, useCache: true, useBrowser: true, degreeOfParallelism:16);

//await Video.DownloadImdbMetadataAsync(settings.TVControversial, 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVDocumentary, 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstream, 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVMainstreamWithoutSubtitle, 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVTutorial, 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(settings.TVTemp4, 1, overwrite: false, useCache: true, useBrowser: true);

//Video.MoveFanArt(settings.Movie3D);
//Video.MoveFanArt(settings.MovieHdr);
//Video.MoveFanArt(settings.MovieControversial);
//Video.MoveFanArt(settings.MovieControversialTemp4);
//Video.MoveFanArt(settings.MovieControversialWithoutSubtitle);
//Video.MoveFanArt(settings.MovieMainstream);
//Video.MoveFanArt(settings.MovieMainstreamWithoutSubtitle);
//Video.MoveFanArt(settings.MovieMusical);
//Video.MoveFanArt(settings.MovieTemp1);
//Video.MoveFanArt(settings.MovieTemp2);
//Video.MoveFanArt(settings.MovieTemp3);
//Video.MoveFanArt(settings.MovieTemp31);
//Video.MoveFanArt(settings.MovieTemp32);
//Video.MoveFanArt(settings.MovieTemp4);
//Video.MoveFanArt(settings.MovieTemp41);
//Video.MoveFanArt(settings.MovieTemp42);
//Video.MoveFanArt(settings.MovieTemp4Encode);

//Video.MoveFanArt(settings.TVControversial, 1);
//Video.MoveFanArt(settings.TVDocumentary, 1);
//Video.MoveFanArt(settings.TVMainstream, 1);
//Video.MoveFanArt(settings.TVMainstreamWithoutSubtitle, 1);
//Video.MoveFanArt(settings.TVTemp4, 1);

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

//Video.RenameDirectoriesWithImdbMetadata(settings, settings.Movie3D, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieHdr, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieControversial, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieControversialTemp4, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieControversialWithoutSubtitle, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieMainstream, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieMainstreamWithoutSubtitle, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieMusical, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp1, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp2, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp3, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp31, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp32, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp4, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp41, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp42, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp4Encode, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieDisk, isDryRun: true);

//Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVControversial, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVDocumentary, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVMainstream, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVMainstreamWithoutSubtitle, level: 1, isTV: true, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.TVTemp4, level: 1, isTV: true, isDryRun: true);

//Video.UpdateXmlRating(settings.Movie3D);
//Video.UpdateXmlRating(settings.MovieHdr);
//Video.UpdateXmlRating(settings.MovieControversial);
//Video.UpdateXmlRating(settings.MovieControversialTemp4);
//Video.UpdateXmlRating(settings.MovieControversialWithoutSubtitle);
//Video.UpdateXmlRating(settings.MovieMainstream);
//Video.UpdateXmlRating(settings.MovieMainstreamWithoutSubtitle);
//Video.UpdateXmlRating(settings.MovieMusical);
//Video.UpdateXmlRating(settings.MovieTemp1);
//Video.UpdateXmlRating(settings.MovieTemp2);
//Video.UpdateXmlRating(settings.MovieTemp3);
//Video.UpdateXmlRating(settings.MovieTemp31);
//Video.UpdateXmlRating(settings.MovieTemp32);
//Video.UpdateXmlRating(settings.MovieTemp4Encode);

//Video.UpdateXmlRating(settings.TVControversial, 1);
//Video.UpdateXmlRating(settings.TVDocumentary, 1);
//Video.UpdateXmlRating(settings.TVMainstream, 1);
//Video.UpdateXmlRating(settings.TVMainstreamWithoutSubtitle, 1);
//Video.UpdateXmlRating(settings.TVTemp4, 1);

//Video.PrintDirectoriesWithErrors(settings, settings.Movie3D);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieHdr);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieControversial);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieControversialTemp4);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieControversialWithoutSubtitle);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieMainstream);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieMainstreamWithoutSubtitle);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieMusical);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp1);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp2);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp3);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp4);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieDisk);

//Video.PrintDirectoriesWithErrors(settings, settings.TVControversial, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings, settings.TVDocumentary, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings, settings.TVMainstream, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings, settings.TVTemp4, 1, isTV: true);
//Video.PrintDirectoriesWithErrors(settings, settings.TVMainstreamWithoutSubtitle, 1, isTV: true);

//Video.PrintVideosWithErrors(settings.Movie3D, searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(settings.MovieControversial, searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(settings.MovieMainstream, searchOption: SearchOption.AllDirectories);

//await Video.ConvertToUtf8Async(settings.LibraryDirectory);
//await Video.ConvertToUtf8Async(settings.Movie3D);
//await Video.ConvertToUtf8Async(settings.MovieMainstream);
//await Video.ConvertToUtf8Async(settings.MovieControversial);

//await Video.ConvertToUtf8Async(settings.TVControversial);
//await Video.ConvertToUtf8Async(settings.TVDocumentary);
//await Video.ConvertToUtf8Async(settings.TVMainstream);
//await Video.ConvertToUtf8Async(settings.TVTutorial);
//await Video.ConvertToUtf8Async(settings.LibraryDirectory);

//await Video.RenameSubtitlesByLanguageAsync(settings.MovieTemp4, isDryRun: true);

//Video.DeleteFeaturettesMetadata(settings.Movie3D, isDryRun: true);
//Video.DeleteFeaturettesMetadata(settings.MovieMainstream, isDryRun: true);
//Video.DeleteFeaturettesMetadata(settings.MovieControversial, isDryRun: true);

//Video.PrintDirectoriesWithMultipleVideos(settings.Movie3D);
//Video.PrintDirectoriesWithMultipleVideos(settings.MovieHdr);
//Video.PrintDirectoriesWithMultipleVideos(settings.MovieControversial);
//Video.PrintDirectoriesWithMultipleVideos(settings.MovieMainstream);
//Video.PrintDirectoriesWithMultipleVideos(settings.MovieMainstreamWithoutSubtitle);

//Video.PrintSubtitlesWithErrors(settings.LibraryDirectory);

//Video.PrintMoviesWithoutSubtitle(settings.MovieControversial);
//Video.PrintMoviesWithoutSubtitle(settings.MovieMainstream);

//Video.PrintDuplicateImdbId(log,
//   settings.MovieMainstream,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieTemp31,
//   settings.MovieTemp32,
//   settings.MovieTemp4,
//   settings.MovieTemp41,
//   settings.MovieTemp42,
//   settings.MovieTemp4Encode,
//   settings.MovieControversial,
//   settings.MovieControversialWithoutSubtitle,
//   settings.MovieControversialTemp4);

//Video.PrintVideosWithErrors(settings.LibraryDirectory);

//await Video.WriteLibraryMovieMetadataAsync(settings, log, default,
//    [
//        settings.MovieMainstream,
//        settings.MovieMainstreamWithoutSubtitle,
//        settings.MovieTemp1,
//    ],
//    [
//        settings.MovieControversial,
//        settings.MovieControversialWithoutSubtitle,
//        settings.MovieTemp2,
//        settings.MovieMusical,
//    ],
//    [
//        settings.MovieTemp3,
//        settings.MovieTemp31,
//        settings.MovieTemp32
//    ],
//    [
//        settings.MovieControversialTemp4,
//        settings.MovieTemp4,
//        settings.MovieTemp41,
//        settings.MovieTemp42,
//        settings.MovieTemp4Encode
//    ]);

//await Video.WriteExternalVideoMetadataAsync(settings);
//await Video.CompareAndMoveAsync(settings, @"", @"", @"", isDryRun: false);

//Video.MoveAllSubtitles(settings.MovieTemp4, settings.MovieSubtitleBackupDirectory);

//await Drive115.WriteOfflineTasksAsync(settings.Drive115Url, settings.Drive115Metadata, log, "Goto.Isle.of.Love.1969");

//await Video.PrintMovieImdbIdErrorsAsync(settings, false, log, cancellationTokenSource.Token,
//    settings.MovieControversial,
//    settings.MovieControversialTemp4,
//    settings.MovieControversialWithoutSubtitle,
//    settings.MovieMainstream,
//    settings.MovieMainstreamWithoutSubtitle,
//    settings.MovieMusical,
//    settings.MovieTemp1,
//    settings.MovieTemp2,
//    settings.MovieTemp3,
//    settings.MovieTemp4,
//    settings.MovieTemp4Encode
//);

//await Top.DownloadMetadataAsync(settings.MovieTopX265EnglishUrl, settings.MovieTopX265XMetadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopX265ForeignUrl, settings.MovieTopX265XMetadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopH264EnglishUrl, settings.MovieTopH264XMetadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopH264ForeignUrl, settings.MovieTopH264XMetadata, index => index <= 5);

//await Top.DownloadMetadataAsync(settings.MovieTopX265Url, settings.MovieTopX265Metadata, index => index <= 5);
//await Top.DownloadMetadataAsync(settings.MovieTopH264Url, settings.MovieTopH264Metadata, index => index <= 20);
//await Top.DownloadMetadataAsync(settings.MovieTopH264720PUrl, settings.MovieTopH264720PMetadata, index => index <= 10);
//await Top.DownloadMetadataAsync(settings.TVTopX265Url, settings.TVTopX265Metadata, index => index <= 5);

//await Preferred.DownloadMetadataAsync(settings, index => index <= 3286);
//await Preferred.DownloadAllTorrentsAsync(settings, false);
//await Preferred.WriteFileMetadataAsync(settings, false);
//await Preferred.CleanUpMetadataErrorsAsync(settings);
//await Preferred.CleanUpFiles(settings);

//await Video.PrintMovieVersions(settings, log, cancellationTokenSource.Token,
//   settings.MovieMainstream,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieControversial,
//   settings.MovieControversialWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieTemp31,
//   settings.MovieTemp32,
//   settings.MovieTemp4,
//   settings.MovieTemp41,
//   settings.MovieTemp42,
//   settings.MovieTemp4Encode,
//   settings.MovieControversialTemp4);

//await Video.PrintMovieVersions(settings, log, cancellationTokenSource.Token,
//    (@"G:\Files\Library", 3),
//    (@"H:\Files\Library", 3),
//    (@"I:\Files\Library", 3),
//    (@"K:\Files\Library\_Movies Encode4.电影4", 2),
//    (@"K:\Files\Library\Movies Controversial.非主流电影", 2));

//await Video.PrintLibraryMovieVersions(settings, log, cancellationToken,
//    @"G:\Files\Library", @"H:\Files\Library", @"I:\Files\Library", @"K:\Files\Library\_Movies Encode4.电影4");

//await Video.PrintTVVersions(settings, log, cancellationTokenSource.Token,
//    settings.TVControversial,
//    settings.TVDocumentary,
//    settings.TVMainstream,
//    settings.TVMainstreamWithoutSubtitle);

//await Imdb.DownloadAllMoviesAsync(
//    settings,
//    count => ..);

//await Imdb.DownloadAllTVsAsync(settings, [settings.TVMainstream], settings.TVMetadataCacheDirectory, settings.TVMetadataDirectory);

//string[] genres = ["family", "animation", "documentary"];
//string[] keywords = [];
//await Video.PrintTVLinks(
//    settings, [settings.TVMainstream, settings.TVMainstreamWithoutSubtitle], settings.TVTopX265Url,
//    imdbMetadata =>
//        !imdbMetadata.AllKeywords.Intersect(["test"], StringComparer.OrdinalIgnoreCase).Any()
//        && !imdbMetadata.Genres.Intersect(genres, StringComparer.OrdinalIgnoreCase).Any()
//        && (imdbMetadata
//            .Advisories
//                .Where(advisory => advisory.Key.ContainsIgnoreCase("sex") || advisory.Key.ContainsIgnoreCase("nudity"))
//                .SelectMany(advisory => advisory.Value)
//                .Any(advisory => advisory.FormattedSeverity == ImdbAdvisorySeverity.Severe)
//    || imdbMetadata.AllKeywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any()), isDryRun: true);

//await Video.WriteMergedMovieMetadataAsync(settings);
//await Video.PrintMovieLinksAsync(
//    settings,
//    (imdbMetadata, keywords) =>
//     imdbMetadata.Advisories
//        .Where(advisory => advisory.Key.ContainsIgnoreCase("sex") || advisory.Key.ContainsIgnoreCase("nudity"))
//        .SelectMany(advisory => advisory.Value)
//        .Any(advisory => advisory.FormattedSeverity == ImdbAdvisorySeverity.Severe)
//        || imdbMetadata.AllKeywords.Any(keywords.Contains),
//    isDryRun: true,
//    drives: [@"G:\Files\Library", @"H:\Files\Library", @"I:\Files\Library", @"K:\Files\Library\_Movies Encode4.电影4"]);

//Audio.ReplaceTraditionalChinese(settings.AudioMainstream, true);

//Audio.PrintDirectoriesWithErrors(settings.AudioControversial);
//Audio.PrintDirectoriesWithErrors(settings.AudioMainstream);
//Audio.PrintDirectoriesWithErrors(settings.AudioShow);
//Audio.PrintDirectoriesWithErrors(settings.AudioSoundtrack);

//Video.MoveAllSubtitles(settings.MovieTemp4, settings.MovieSubtitleBackupDirectory);

//Video.RenameDirectoriesWithMetadata(settings, settings.TVTemp4, 1, isTV: true, isDryRun: false, skipRenamed: true);
//Video.RenameDirectoriesWithAdditionalMetadata(settings.MovieTemp4, 2);
//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.MovieControversial, isDryRun: false);

//Video.RenameEpisodes(@"D:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]", "European Adult");
//Video.CreateTVEpisodeMetadata(@"T:\Files\Library\TV Tutorial.教程\English Toefl.2017.英语托福[---][-]");

//Video.PrintVideosWithErrors(@"E:\Files\Korean", searchOption: SearchOption.AllDirectories);
//Video.RenameDirectoriesWithMetadata(settings, settings.MovieTemp4, 2);
//Video.RestoreMetadata(settings.MovieTemp4);

//Video.PrintMovieRegionsWithErrors(settings, log, [settings.MovieMainstream]);

//Video.RenameEpisodesWithTitle(
//    @"",
//    @"",
//    rename: (f, t) =>
//    {
//        //t = t.Substring(0, 1).ToUpper() + t.Substring(1);
//        //string postfix = PathHelper.GetFileNameWithoutExtension(f).EndsWithIgnoreCase($"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio") ? $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio" : $"{Video.VersionSeparator}{Video.TopEnglishKeyword}";

//        string postfix = $"{Video.VersionSeparator}{settings.ContrastKeyword}";
//        //Debug.Assert(!f.IsVideo() || PathHelper.GetFileNameWithoutExtension(f).EndsWithIgnoreCase(postfix));
//        string name = PathHelper.GetFileNameWithoutExtension(f);
//        Match match = Regex.Match(name, @"\.[2-9]Audio");
//        if (match.Success)
//        {
//            postfix += match.Value;
//        }

//        return name.EndsWithOrdinal(postfix) || name.EndsWithOrdinal($"{postfix}-thumb") || Regex.IsMatch(name, $@"{postfix.Replace(".", @"\.").Replace("-", @"\-")}\.[a-z]{{3}}(&[a-z]{{3}})?(\-[0-9]{{1,2}}|\-[a-z]+)?$")
//            ? PathHelper.ReplaceFileNameWithoutExtension(f, n => n.Replace(postfix, $"{postfix}.{t}"))
//            : f;
//        //return Regex.Replace(f, @"(S[0-9]{2}E[0-9]{2})", $"$1.{t}");
//        //return f.Replace(".ffmpeg", $".ffmpeg.{t}");
//        //return PathHelper.AddFilePostfix(f, t);
//    },
//    //rename: (f, t) => f.Replace($"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio", $"{Video.VersionSeparator}{Video.TopEnglishKeyword}.2Audio.{t}"),
//    //rename: (f, t) => Regex.Replace(f, @"(\.S[0-9]{2}E[0-9]{2})", $"{"$1".ToUpperInvariant()}.{t}"),
//    isDryRun: false);

//Video.PrintVideosWithErrors(settings, settings.MovieTemp42, searchOption: SearchOption.AllDirectories);
//Video.CopyMovieMetadata(settings.MovieTemp42, 2);
//await Video.PrintMovieImdbIdErrorsAsync(settings, true, log, cancellationTokenSource.Token, settings.MovieTemp42);
//await Video.ConvertToUtf8Async(settings.MovieTemp42, true);
//Video.MoveMovieSubtitleToParent(settings.MovieTemp42, settings.MovieSubtitleBackupDirectory, false);
//Video.MoveMetadata(settings.MovieTemp42, settings.MovieMetadataCacheDirectory, settings.MovieMetadataDirectory);
//await Video.DownloadImdbMetadataAsync(settings.MovieTemp42, 2, overwrite: false, useCache: true, useBrowser: true);
//FfmpegHelper.MergeAllDubbedMovies(settings.MovieTemp42, isDryRun: true);
//Video.PrintDuplicateImdbId(null, @"G:\Files\Library",
//    @"H:\Files\Library",
//    @"I:\Files\Library",
//    @"K:\Files\Library\_Movies Encode4.电影4");
//Video.BackupMetadata(settings.MovieTemp42);
//await Video.DownloadMissingTitlesFromDoubanAsync(settings, settings.MovieTemp42, skipFormatted: true);
//Video.CopyMovieMetadata(settings.MovieTemp42, 2, true);
//Video.RenameDirectoriesWithMetadata(settings, settings.MovieTemp42, isDryRun: false, skipRenamed: true);
//Video.RenameDirectoriesWithImdbMetadata(settings, settings.MovieTemp42);
//Video.MoveFanArt(settings.MovieTemp42);
//Video.RestoreMetadata(settings.MovieTemp42);
//Video.PrintDirectoriesWithErrors(settings, settings.MovieTemp42);
//Video.RenameDirectoriesWithDigits(settings.MovieTemp42);
//Video.RenameDirectoriesWithGraphicMetadata(settings.MovieTemp42);
//Video.MoveDirectoriesByRegions(settings, settings.MovieTemp42, isDryRun: false);
//Video.RenameDirectoriesWithoutAdditionalMetadata(settings.MovieTemp42);

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
//    settings,
//    @"",
//    @"",
//    isDryRun: false,
//    overwrite: false);

//Video.CreateTVEpisodeMetadata(@"H:\Downloads7\New folder (6)\阅读\_1", f => PathHelper.GetFileNameWithoutExtension(f).Split(".").Last());

//Video.MoveTopTVEpisodes(settings, @"", settings.TVSubtitleBackupDirectory, isDryRun: false);
//Video.FormatTV(
//    @"E:\Files\TV",
//    @"",
//    renameForTitle: (f, t) => f.Replace($"{Video.VersionSeparator}{settings.TopEnglishKeyword}", $"{Video.VersionSeparator}{settings.TopEnglishKeyword}.{t}"),
//    isDryRun: false);

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
            .Select(episode => Video.SeasonEpisodeRegex.Match(PathHelper.GetFileNameWithoutExtension(episode)).Value)
            .OrderBy(number => number)
            .ToArray();
        string[] metadataEpisodeNumbers = metadataSeasons
            .SelectMany(Directory.EnumerateFiles)
            .Where(Video.IsVideo)
            .Select(episode => Video.SeasonEpisodeRegex.Match(PathHelper.GetFileNameWithoutExtension(episode)).Value)
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
        false,
        isDryRun,
        log));
    //tvs.ForEach(tv => Video.MoveSubtitlesForEpisodes(tv, Path.Combine(metadataDirectory, PathHelper.GetFileName(tv)), isDryRun: isDryRun, log: log));
}

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
        .Select(file => (file.file, file.Item2.Value))
        .OrderBy(file => file.file)
        .ToArray();

    files
        .Where(file => file.Path.IsVideo())
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
//    .SelectMany(Directory.GetFiles)
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
//await Entry.DownloadMetadataAsync(settings, "http://hotxshare.com", 1, 238, @"D:\Files\Library\Movie.EntryMetadata.json");

//await FfmpegHelper.EncodeAllAsync(
//    Path.Combine(settings.MovieTemp4Encode, "SD.Encode"), @"D:\Temp\Encode", VideoCropMode.NoCrop,
//    inputPredicate: input => input.HasExtension(".mkv"),
//    maxDegreeOfParallelism: 2, cancellationToken: cancellationTokenSource.Token);

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
//            File.AppendAllLines(file, [line]);
//        }
//    }
//}, 4);

//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => DirectoryHelper.AddPostfix(d, "[HDR]"));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => DirectoryHelper.ReplaceDirectoryName(d, n => n.Replace("[1080x]", "[2160x]")));
//Directory.GetDirectories(@"U:\Move\New folder").ForEach(d => Directory.CreateDirectory(Path.Combine(@"T:\Move\New folder", PathHelper.GetFileName(d))));
//string[] sourceDirectories = Directory.GetDirectories(@"S:\Files\Library\Movies Mainstream.主流电影\American Fiction.美国科幻");
//string[] destinationDirectories = Directory.GetDirectories(@"T:\Files\Library\Movies 4K HDR.高动态范围电影\American Fiction.美国科幻");
//Enumerable.Range(1, 29).Select(index => $"Marvel`{index}-")
//    .ForEach(prefix =>
//    {
//        string sourceDirectory = sourceDirectories.Single(d => PathHelper.GetFileName(d).StartsWithOrdinal(prefix));
//        string destinationDirectory = destinationDirectories.Single(d => PathHelper.GetFileName(d).StartsWithOrdinal(prefix));
//        string sourceNfo = Directory.EnumerateFiles(sourceDirectory).First(f => f.EndsWithOrdinal(".nfo"));
//        string destinationVideo = Directory.EnumerateFiles(destinationDirectory).Single(f => f.HasExtension(Video.VideoExtension));
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
//await Rare.PrintVersionsAsync(settings, log);
//await Rare.DownloadMetadataAsync(
//    settings,
//    "https://rarelust.com/movies-index/");

//Drive115.DownloadOfflineTasks(
//    "https://115.com/?tab=offline&mode=wangpan",
//    (title, link) => title.Contains("A.History.of.Sex.2003")
//        || title.ContainsIgnoreCase("Jesus.2016")
//        || link.ContainsIgnoreCase("0e394f73e99ed6e9c4e5f8b4a75befd1de1cc0b3")
//        || link.ContainsIgnoreCase("3fef3fb6764a95383c941db41b8a2522ac1cfcd3"),
//    (title, link) => Debugger.Break());
//await Drive115.WriteOfflineTasksAsync("https://115.com/?tab=offline&mode=wangpan",
//    @"D:\Files\Library\Drive115.OfflineTasks.json");

//Video.RenameFiles(@"E:\Files\Korean\Even.closer.hautnah.s01.french.web.h264-freamon\Season 01", (f, i) =>
//    f.ReplaceIgnoreCase(@"even.closer.hautnah.s01e0", "Even Closer-Hautnah.S01E0")
//        .Replace(".french.web.h264-freamon", ".FRENCH.WEBRip.H264-Freamon")
//        .Replace(".1080p.BluRay.DTS.x264-SbR", ".1080p.BluRay-SbR.ffmpeg")
//        .Replace(".1080p.WEB-DL.DD+5.1.x264-SbR", ".1080p.WEBRip.H264-SbR")
//    , searchOption: SearchOption.AllDirectories);
//string[] keywords = ["unsimulated sex", "labia", "vagina", "female full frontal nudity", "female pubic"];
//Directory.GetDirectories(@"T:\Files\Library\Movies Mainstream.Temp")
//    .Where(d =>
//    {
//        string name = PathHelper.GetFileName(d);
//        return !name.StartsWith("_") && !name.ContainsIgnoreCase(".");
//    })
//    .ForEach(d => Directory.GetDirectories(d).ForEach(m =>
//    {
//        if (ImdbMetadata.TryLoad(m, out ImdbMetadata? imdbMetadata))
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
//        string video = Directory.EnumerateFiles(m).First(f => f.HasExtension(Video.VideoExtension));
//        string name = PathHelper.GetFileName(m);
//        string source = sources.First(d => PathHelper.GetFileName(d).EqualsIgnoreCase(name));
//        Directory.GetFiles(source).ForEach(f =>
//        {
//            if (f.HasExtension(Video.VideoExtension))
//            {
//                return;
//            }

//            if (f.EndsWithIgnoreCase(".nfo"))
//            {
//                string destination = PathHelper.ReplaceExtension(video, ".nfo");
//                if (!File.Exists(destination))
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
//    string tailTitle = title[(title.IndexOf("-", StringComparison.Ordinal) + 1)..];
//    string metadata2 = PathHelper.GetFileName(d)[PathHelper.GetFileName(d).IndexOf("[", StringComparison.Ordinal)..];
//    int index = tailTitle.IndexOfOrdinal(Video.InstallmentSeparator);
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
//       //Regex.Replace(f, @" [0-9]+GB", "")
//       );


//await TorrentHelper.AddDefaultTrackersAsync(@"E:\Files\MonoTorrentDownload", Logger.WriteLine);

//Directory.GetFiles(@"Q:\Files\Movies.Raw3\Korean")
//    .ForEach(f => FileHelper.MoveToDirectory(f, Path.Combine(@"Q:\Files\Movies.Raw3\Korean", PathHelper.GetFileNameWithoutExtension(f))));

//string[] allDownloadedTitles = new string[]
//{
//   settings.MovieMainstream,
//   settings.MovieMainstreamWithoutSubtitle,
//   settings.MovieMusical,
//   settings.MovieTemp1,
//   settings.MovieTemp2,
//   settings.MovieTemp3,
//   settings.MovieTemp4,
//   settings.MovieControversial,
//   settings.MovieControversialWithoutSubtitle,
//   settings.MovieControversialTemp4,
//   @"Q:\Files\Movies.Raw"
//}.SelectMany(d => Directory.EnumerateFiles(d, Video.VideoSearchPattern, SearchOption.AllDirectories))
//    .Select(PathHelper.GetFileNameWithoutExtension)
//    .Where(f => f.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") || f.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}"))
//    .Select(f =>
//    {
//        if (f.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}"))
//        {
//            return f[..(f.IndexOfIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") + $"{Video.VersionSeparator}{settings.TopEnglishKeyword}".Length)];
//        }

//        if (f.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}"))
//        {
//            return f[..(f.IndexOfIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}") + $"{Video.VersionSeparator}{settings.TopForeignKeyword}".Length)];
//        }

//        throw new InvalidOperationException(f);
//    })
//    .ToArray();

//File.WriteAllLines(@"e:\AllDownloadedTitles.txt", allDownloadedTitles);

//HashSet<string> allDownloadedTitlesHashSet = new(File.ReadAllLines(@"e:\AllDownloadedTitles.txt"), StringComparer.OrdinalIgnoreCase);

//Directory.GetDirectories(@"\\beyond-x\E\Files\Downloading")
//    .Where(d => allDownloadedTitlesHashSet.Contains(PathHelper.GetFileName(d)))
//    .ToArray()
//    .ForEach(d => DirectoryHelper.MoveToDirectory(d, @"\\beyond-x\E\Files\New folder"));


//Directory.EnumerateDirectories(@"\\beyond-x\E\Files\Move").Where(d => allDownloadedTitles.Contains(PathHelper.GetFileName(d))).ForEach(d => DirectoryHelper.MoveToDirectory(d, @"\\beyond-x\E\Files\Move.Delete"));

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
//        if (g.All(dd => !dd.ContainsIgnoreCase(".DUBBED.")) && g.Any(dd => dd.EndsWithIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")))
//        {
//            destinationDirectory = g.First(dd => dd.EndsWithIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}"));
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

//await Video.WriteNikkatsuMetadataAsync(@"D:\Files\Library\Movie.Nikkatsu..txt", @"D:\Files\Library\Movie.Nikkatsu..json");

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
//        string newName = title.Replace(": ", "-").Replace(":", "-") + name[name.IndexOf(year, StringComparison.Ordinal)..];
//        FileHelper.ReplaceFileNameWithoutExtension(metadata, newName);

//        string d = PathHelper.GetDirectoryName(metadata);
//        Directory
//            .EnumerateFiles(d)
//            .Where(f => PathHelper.GetFileNameWithoutExtension(f).StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
//            .ForEach(f =>
//            {
//                FileHelper.ReplaceFileNameWithoutExtension(f, n => n.Replace(name, newName));
//            });
//    });

//Video.FormatVideoFileNames(@"", SearchOption.AllDirectories, isDryRun: true);
//Directory.GetFiles(settings.MovieTemp3, "*.2Audio.mp4", SearchOption.AllDirectories)
//    .Select(v => Video.ReadVideoMetadataAsync(v).Result)
//    .Where(v => !(v.AudioStreams.Length == 2
//        && v.AudioStreams[0].Language != v.AudioStreams[1].Language
//        && v.AudioStreams[0].Language is not "eng"
//        && v.AudioStreams[1].Language is "eng"))
//    .ForEach(v => Logger.WriteLine(v.File));


//Torrent t = Torrent.Load(@"\\beyond-r\J\Files\Library\MovieMetadataCache\tt7491128.3333D9F8BC4009FA809B0AF2464E6972636CD506.ami-ami-2018.1080p BluRay BluRay.[FR] Ami ami.torrent");
//t.Files.ForEach(f => Logger.WriteLine(f.Path));

//ConcurrentDictionary<string, PreferredFileMetadata[]> allFileMetadata = await JsonHelper
//    .DeserializeFromFileAsync<ConcurrentDictionary<string, PreferredFileMetadata[]>>(settings.MoviePreferredFileMetadata, new(), cancellationTokenSource.Token);
//ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationTokenSource.Token);

//Dictionary<string, (string ImdbId, PreferredFileMetadata metadata)[]> allTitleToImdbIds = allFileMetadata.Values
//    .SelectMany(group => group)
//    .ToLookup(
//        metadata => metadata.File
//        .ReplaceIgnoreCase($".{settings.PreferredOldKeyword}", $"{Video.VersionSeparator}{settings.PreferredOldKeyword}")
//        .ReplaceIgnoreCase(".BRRip.", ".BluRay.")
//        .ReplaceIgnoreCase(".1080.BluRay.", ".1080p.BluRay."),
//        metadata => (metadata.ImdbId, metadata), StringComparer.OrdinalIgnoreCase)
//    .Select(group => (Title: group.Key, ImdbIds: group.DistinctBy(imdbId => imdbId.ImdbId, StringComparer.OrdinalIgnoreCase).ToArray()))
//    .Do(group =>
//    {
//        if (group.ImdbIds.Length > 1)
//        {
//            group
//                .ImdbIds
//                .Select(imdbId => $"https://www.imdb.com/title/{imdbId.ImdbId}/ | {imdbId.metadata.Link} | {imdbId.metadata.DisplayName} | {imdbId.metadata.ExactTopic}")
//                .Prepend($"{group.Title} ")
//                .Append(string.Empty)
//                .ForEach(Logger.WriteLine);
//        }
//        else
//        {
//            Debug.Assert(group.ImdbIds.Length == 1);
//        }
//    })
//    .ToDictionary(group => group.Title, group => group.ImdbIds, StringComparer.OrdinalIgnoreCase);
//existingMetadata
//    .SelectMany(group => group.Value.Keys, (group, file) => (File: file, ImdbId: group.Key, Title: PathHelper.GetFileNameWithoutExtension(file), Parsed: VideoMovieFileInfo.Parse(file)))
//    .Where(file => file.Parsed.GetEncoderType() is EncoderType.PreferredX265 or EncoderType.PreferredH264)
//    .ForEach(file =>
//    {
//        string title = Regex.Replace(file.Title, @"\.[0-9]+Audio", string.Empty);
//        if (allTitleToImdbIds.TryGetValue(title, out (string ImdbId, PreferredFileMetadata metadata)[]? libraryImdbIds))
//        {
//            if (libraryImdbIds.Select(imdbId => imdbId.ImdbId).ContainsIgnoreCase(file.ImdbId))
//            {
//                return;
//            }

//            Logger.WriteLine($"""
//                  {title} | {file.File}
//                  https://www.imdb.com/title/{file.ImdbId}/
//                  Found title with different IMDB id: {string.Join("|", libraryImdbIds.Select(imdbId => $"https://www.imdb.com/title/{imdbId.ImdbId}/"))}

//                  """);
//            return;
//        }

//        (string ImdbId, PreferredFileMetadata metadata)[] matches = allTitleToImdbIds.Where(pair => title.StartsWithIgnoreCase(pair.Key)).SelectMany(pair => pair.Value).ToArray();
//        if (matches.Any())
//        {
//            if (matches.Select(match => match.ImdbId).ContainsIgnoreCase(file.ImdbId))
//            {
//                return;
//            }

//            Logger.WriteLine($"""
//                  {title} | {file.File}
//                  https://www.imdb.com/title/{file.ImdbId}/
//                  Found title with different IMDB id: {string.Join("|", matches.Select(match => $"https://www.imdb.com/title/{match.ImdbId}/"))}

//                  """);
//            return;
//        }

//        Logger.WriteLine($"""
//             {title} | {file.File}
//             Found With {file.ImdbId}: {(allFileMetadata.TryGetValue(file.ImdbId, out PreferredFileMetadata[]? group) ? string.Join("|", group.Select(metadata => metadata.File)) : string.Empty)}

//             """);
//    });

//Video.EnumerateDirectories(settings.MovieTemp41)
//    .Where(d => d.Contains("`"))
//    .ToArray()
//    .ForEach(d => DirectoryHelper.MoveToDirectory(d, Path.Combine(settings.MovieTemp41, "Franchise")));
//string[] downloadingTitles = @"".Split(Environment.NewLine);
//string[] existingTitles = existingMetadata.Values.SelectMany(group => group.Keys).Select(PathHelper.GetFileNameWithoutExtension).ToArray();

//downloadingTitles
//    .Where(downloadingTitle => existingTitles.Any(existingTitle => existingTitle.StartsWithIgnoreCase(downloadingTitle)))
//    .ForEach(Logger.WriteLine);
//Video.RenameFiles(@"D:\Files\Library\TV Mainstream.主流电视剧[未抓取字幕]\Rebecka Martinsson.2017.丽贝卡·马丁森[7.1-3.1K][NotRated][1080p]\Season 01",
//    (f, i) => f.Replace(".2017.1080p.WEBRip", ".1080p.WEBRip"));
//ILookup<string, string> topDatabase = (await File.ReadAllLinesAsync(settings.TopDatabase))
//    .AsParallel()
//    .Where(line => (line.ContainsIgnoreCase("|movies_x265|") || line.ContainsIgnoreCase("|movies|"))
//        && (line.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") || line.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")))
//    .Select(line => line.Split('|'))
//    .Do(cells => Debug.Assert(string.IsNullOrEmpty(cells[^2]) || cells[^2].IsImdbId()))
//    .Do(cells => Debug.Assert(cells[1].ContainsIgnoreCase($"-{settings.TopEnglishKeyword}") || cells[1].ContainsIgnoreCase($"-{settings.TopForeignKeyword}")))
//    .ToLookup(cells => cells[1], cells => cells[^3]);

//existingTitles = existingMetadata.Values.SelectMany(group => group.Keys).Select(PathHelper.GetFileNameWithoutExtension).ToArray();
//string[] titles = File.ReadAllLines(settings.TempFile);
//titles.Where(title => !existingTitles.Any(existingTitle => existingTitle.StartsWithIgnoreCase(title)))
//    .SelectMany(title => topDatabase[title])
//    .ForEach(Logger.WriteLine);

//await MoveSharedMoviesAsync(settings);

static async Task MoveSharedMoviesAsync(ISettings settings, CancellationToken cancellationToken = default, Action<string>? log = null)
{
    log ??= Logger.WriteLine;

    SharedMetadata[] ordered = await settings.LoadMovieSharedMetadataAsync(cancellationToken);
    //var allMetadata = ordered
    //    .AsParallel()
    //    .Select(metadata =>
    //    {
    //        string[] titles = metadata.Title
    //            .ToLowerInvariant()
    //            .Split([" aka ", ".aka."], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    //            .Select(title => Regex.Replace(title, @"\([0-9]{4}\)", "").Trim())
    //            .Select(title => new string(title
    //                .ReplaceIgnoreCase(" the ", "")
    //                .Replace("ô", "o")
    //                .Where(c => !char.IsPunctuation(c) && !char.IsSymbol(c) && !char.IsWhiteSpace(c)).ToArray()))
    //            .ToArray();
    //        Match yearMatch = Regex.Match(metadata.Title, @"\(([0-9]{4})\)");
    //        string year = yearMatch.Success ? yearMatch.Groups[1].Value : string.Empty;
    //        if (yearMatch.Success)
    //        {
    //            Debug.Assert(yearMatch.Groups.Count == 2);
    //        }

    //        return new { Titles = titles, Year = year, ImdbIds = metadata.ImdbIds, Metadata = metadata };
    //    })
    //    .ToArray();
    (SharedMetadata metadata, string)[] fileToMetadata = ordered
        .SelectMany(
            metadata => metadata
                .Downloads
                .Where(download => download.IsNotNullOrWhiteSpace() && !Regex.IsMatch(download, @"imdb\.com/title/tt")),
            (metadata, download) => (metadata, download.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last()))
        .ToArray();

    (SharedMetadata metadata, string)[] fileNameToMetadata = fileToMetadata
        .Select(metadata => (metadata.metadata, metadata.Item2.LastIndexOf('.') < 0 ? metadata.Item2 : metadata.Item2[..metadata.Item2.LastIndexOf('.')]))
        .ToArray();

    (SharedMetadata metadata, string)[] fileNameNameToMetadata = fileNameToMetadata
        .Select(metadata => (metadata.metadata, metadata.Item2.LastIndexOf('.') < 0 ? metadata.Item2 : metadata.Item2[..metadata.Item2.LastIndexOf('.')]))
        .ToArray();

    Directory
        .EnumerateDirectories(@"\\beyond-r\J\Files\Library\Movies")
        .Where(d => d.ContainsIgnoreCase(@"\Shared."))
        .SelectMany(Directory.EnumerateDirectories)
        .ToArray()
        .ForEach(d =>
        {
            string fileName = PathHelper.GetFileName(d);

            (SharedMetadata metadata, string)[] fileNameMatches = fileNameToMetadata.Where(metadata => metadata.Item2.EqualsIgnoreCase(fileName)).ToArray();
            if (fileNameMatches.Any())
            {
                fileNameMatches
                    .SelectMany(metadata => metadata.metadata.ImdbIds)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ForEach(imdbId =>
                    {
                        string imdbIdFile = Path.Combine(d, $"{imdbId}.tmp");
                        File.WriteAllText(imdbIdFile, string.Empty);
                        log(imdbIdFile);
                    });
                return;
            }

            (SharedMetadata metadata, string)[] fileNameNameMatches = fileNameNameToMetadata.Where(metadata => metadata.Item2.EqualsIgnoreCase(fileName)).ToArray();
            if (fileNameNameMatches.Any())
            {
                fileNameNameMatches
                    .SelectMany(metadata => metadata.metadata.ImdbIds)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ForEach(imdbId =>
                    {
                        string imdbIdFile = Path.Combine(d, $"{imdbId}.tmp");
                        File.WriteAllText(imdbIdFile, string.Empty);
                        log(imdbIdFile);
                    });
                return;
            }

            log($"!!!{d}");
            DirectoryHelper.Move(d, d.Replace(@"\Shared.", @"\"));
        });

    //Video.EnumerateDirectories(@"\\beyond-r\J\Files\Library\Movies Temp4.2.电影4.2\Downloaded.Pink", 1)
    //    .AsParallel()
    //    .AsOrdered()
    //    .WithDegreeOfParallelism(Video.IOMaxDegreeOfParallelism)
    //    .ForAll(movie =>
    //    {
    //        string[] files = Directory.GetFiles(movie);
    //        string imdbMetadataFile = files.Single(file => file.IsImdbMetadata());

    //        XDocument xmlMetadata = XDocument.Load(files.Single(file => file.IsXmlMetadata()));
    //        string xmlTitle = xmlMetadata.Root?.Element("title")?.Value ?? string.Empty;
    //        Debug.Assert(xmlTitle.IsNotNullOrWhiteSpace());

    //        string xmlOriginalTitle = xmlMetadata.Root?.Element("title")?.Value ?? string.Empty;

    //        string video = files.Single(f => f.IsVideo());
    //        VideoMovieFileInfo info = VideoMovieFileInfo.Parse(video);
    //        string[] titles = info.Title
    //            .ToLowerInvariant()
    //            .Split([" aka ", ".aka."], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    //            //.Concat(xmlTitle.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    //            //.Concat(xmlOriginalTitle.IsNullOrWhiteSpace() ? [] : [xmlOriginalTitle])
    //            .Select(title => new string(title
    //                .ReplaceIgnoreCase(" the ", "")
    //                .Replace("ô", "o")
    //                .Where(c => !char.IsPunctuation(c) && !char.IsSymbol(c) && !char.IsWhiteSpace(c)).ToArray()))
    //            .ToArray();

    //        var matches = allMetadata.Where(metadata => metadata.Titles.Intersect(titles, StringComparer.InvariantCultureIgnoreCase).Any()).ToArray();
    //        //if (m == @"\\beyond-r\J\Files\Library\Movies Temp4.2.电影4.2\Downloaded.Pink\A Woman in Revolt.Aka.Hanjo mugen jigoku.1970")
    //        //{
    //        //    Debugger.Break();
    //        //}
    //        if (matches.Length == 1)
    //        {
    //            var match = matches.Single();
    //            if (match.ImdbIds.IsEmpty() && PathHelper.GetFileNameWithoutExtension(imdbMetadataFile).EqualsOrdinal(Video.NotExistingFlag))
    //            {
    //                DirectoryHelper.MoveToDirectory(movie, @"\\beyond-r\J\Files\Library\Movies Temp4.2.电影4.2\Pink");
    //                return;
    //            }

    //            if (match.ImdbIds.Length == 1 && ImdbMetadata.TryGet(imdbMetadataFile, out string? imdbId) && imdbId.EqualsIgnoreCase(match.ImdbIds.Single()))
    //            {
    //                DirectoryHelper.MoveToDirectory(movie, @"\\beyond-r\J\Files\Library\Movies Temp4.2.电影4.2\Pink");
    //                return;
    //            }
    //        }

    //        matches.ForEach(match =>
    //        {
    //            if (match.ImdbIds.IsEmpty())
    //            {
    //                string tempFile = $"{Video.NotExistingFlag}{Video.Delimiter}{new Uri(match.Metadata.Url).LocalPath.Trim('/')}.tmp";
    //                File.WriteAllText(Path.Combine(movie, tempFile), string.Empty);
    //                return;
    //            }

    //            match.ImdbIds.ForEach(imdbId =>
    //            {
    //                string tempFile = $"'{imdbId}{Video.Delimiter}{new Uri(match.Metadata.Url).LocalPath.Trim('/')}.tmp";
    //                File.WriteAllText(Path.Combine(movie, tempFile), string.Empty);
    //            });
    //        });

    //        //if (!files.Any(f => f.EndsWithIgnoreCase(@"\-.json")))
    //        //{
    //        //    DirectoryHelper.MoveToDirectory(m, @"\\beyond-r\J\Files\Library\Movies Temp4.2.电影4.2\Downloaded.Pink");
    //        //}
    //    });
}

//Directory.GetFiles(@"D:/Files/Library/Movies Temp4.2.电影4.2", "*.mkv", SearchOption.AllDirectories)
//    .AsParallel()
//    .WithDegreeOfParallelism(3)
//    .ForAll(video =>
//    {
//        try
//        {
//            TimeSpan duration1 = FFmpeg.GetMediaInfo(video).Result.Duration;
//            TimeSpan duration2 = FFmpeg.GetMediaInfo(video + ".mp4").Result.Duration;
//            TimeSpan delta = duration1 - duration2;
//            if (delta >= TimeSpan.FromSeconds(-1) && delta <= TimeSpan.FromSeconds(1))
//            {
//                return;
//            }

//            Logger.WriteLine(delta.ToString());
//            Logger.WriteLine(video);
//            Logger.WriteLine("");
//        }
//        catch (Exception e)
//        {
//            Logger.WriteLine(e.ToString());
//            Logger.WriteLine(video);
//            Logger.WriteLine("");
//        }
//    });
//Video.FormatVideoFileNames(@"\\beyond-r\J\Files\Library\new folder", SearchOption.AllDirectories, false);

//Video.PrintDirectoriesWithMultipleVideos(@"\\beyond-r\J\Files\Library\New folder");
//Video.RenameFiles(@"\\beyond-r\J\Files\Library\Movies", (f, i) => f.Replace(".2Audio.SUBBED", ".SUBBED.2Audio"));
//Directory.EnumerateFiles(@"\\beyond-r\J\Files\Library\Movies", "*", SearchOption.AllDirectories)
//    .Where(f => f.IsVideo() && !VideoMovieFileInfo.TryParse(f, out _))
//    .ForEach(Logger.WriteLine);
//Directory.EnumerateDirectories(@"\\beyond-r\J\Files\Library\Movies")
//    .SelectMany(Directory.EnumerateDirectories)
//    .ToArray()
//    .ForEach(d =>
//    {
//        string[] subs = Directory.GetDirectories(d);
//        if (subs.Any())
//        {
//            subs.Append("").ForEach(Logger.WriteLine);
//        }
//    });

//Video.EnumerateDirectories(@"\\beyond-r\J\Files\Library\Movies")
//    .ForEach(d =>
//    {
//        string[] files = Directory.GetFiles(d);
//        string[] subtitleFiles = files.Where(f => f.IsSubtitle()).ToArray();
//        //string currentDirectory = PathHelper.GetDirectoryName(video);
//        //string videoName = PathHelper.GetFileNameWithoutExtension(video);
//        subtitleFiles
//            .Where(f => Regex.IsMatch(PathHelper.GetFileNameWithoutExtension(f), @"\.[a-z]{3}-[0-9]+$"))
//            .ToArray()
//            .ForEach(s =>
//            {
//                //string newSubtitle = PathHelper.ReplaceFileNameWithoutExtension(s, name => name[..^3] + "t");
//                //if (!File.Exists(newSubtitle))
//                //{
//                //    FileHelper.Move(s, newSubtitle);
//                //    return;
//                //}
//                //newSubtitle = PathHelper.ReplaceFileNameWithoutExtension(s, name => name[..^3] + "s");
//                //if (!File.Exists(newSubtitle))
//                //{
//                //    FileHelper.Move(s, newSubtitle);
//                //}
//                Logger.WriteLine(s);
//            });
//    });

//Video.EnumerateDirectories(@"\\beyond-r\J\Files\Library\Movies Temp4.2.电影4.2")
//    .Where(d => VideoDirectoryInfo.Parse(d).Source.StartsWithIgnoreCase("f"))
//    .ToArray()
//    .ForEach(d => DirectoryHelper.Move(d, d.Replace(@"\Movies Temp4.2.电影4.2\", @"\Encode\")));


//Video.EnumerateDirectories(settings.MovieTemp41)
//    .ToArray()
//    .ForEach(d =>
//    {
//        //string video = Directory.EnumerateFiles(d).First(Video.IsVideo);
//        //string name = PathHelper.GetFileNameWithoutExtension(video);
//        //DirectoryHelper.ReplaceDirectoryName(d, name);
//        string[] files = Directory.GetFiles(d, "*");
//        string[] nfos = files.Where(f => f.EndsWithIgnoreCase(".nfo")).ToArray();
//        //if (nfos.Length == 1)
//        //{
//        //    return;
//        //}

//        //if (nfos.Length == 0)
//        //{
//        //    Logger.WriteLine(d);
//        //    return;
//        //}

//        //if (nfos.Select(nfo =>
//        //    {
//        //        XDocument doc = XDocument.Load(nfo);
//        //        return (doc.Root.Element("title").Value, doc.Root.Element("year").Value);
//        //    }).Distinct().Count() != 1)
//        //{
//        //    Logger.WriteLine(d);
//        //    return;
//        //}

//        string[] imdbFiles = files.Where(f => f.EndsWithIgnoreCase(".tmp")).ToArray();
//        if (imdbFiles.Length == 1)
//        {
//            string fileImdbId = PathHelper.GetFileNameWithoutExtension(imdbFiles.Single());
//            (string, int)[] xmlImdbIds = nfos.Select((nfo, index) => (XDocument.Load(nfo).Root?.Element("imdbid")?.Value ?? string.Empty, index)).ToArray();
//            (string, int)[] differentXmlImdbIds = xmlImdbIds.Where(xmlImdbId => xmlImdbId.Item1 != fileImdbId).ToArray();
//            if (differentXmlImdbIds.IsEmpty())
//            {
//                File.Move(imdbFiles.Single(), imdbFiles.Single() + ".txt");
//            }
//            else
//            {
//                Logger.WriteLine(d);
//                Logger.WriteLine(fileImdbId);
//                differentXmlImdbIds.ForEach(xmlImdbId =>
//                {
//                    int index = xmlImdbId.Item2;
//                    string nfo = nfos[index];
//                    string title = XDocument.Load(nfo).Root?.Element("title")?.Value ?? string.Empty;
//                    Logger.WriteLine(title);
//                });
//                Logger.WriteLine("");
//            }
//        }
//    });

const string Subdirectory = "HD.Encode.Crop";

//string[] localDirectoryNames = Directory.GetDirectories(Path.Combine(@"E:\Encode\", Subdirectory)).Select(PathHelper.GetFileName).ToArray();
//string[] remoteDirectories = Directory.GetDirectories(Path.Combine(@"\\beyond-r\J\Files\Library\Movies Encode4.电影4\", Subdirectory)).Select(PathHelper.GetFileName).ToArray();
//localDirectoryNames.Except(remoteDirectories).ForEach(Logger.WriteLine);
//remoteDirectories.Except(localDirectoryNames).ForEach(Logger.WriteLine);
//Directory.EnumerateFiles(Path.Combine(@"\\beyond-r\J\Files\Library\Movies Encode4.电影4\", Subdirectory), "*", SearchOption.AllDirectories)
//    .Where(f => f.IsVideo() && PathHelper.GetFileNameWithoutExtension(f).ContainsIgnoreCase(".ffmpeg"))
//    .Select(v =>
//    {
//        string local = PathHelper.ReplaceExtension(v.ReplaceIgnoreCase(@"\\beyond-r\J\Files\Library\Movies Encode4.电影4\", @"E:\Encode\"), ".mp4");
//        return (Remote: v, LocalEncoded: local.ContainsIgnoreCase(".ffmpeg") ? local : PathHelper.AddFilePostfix(local, ".ffmpeg"));
//    })
//    .ForEach(ff =>
//    {
//        TimeSpan remote = FFmpeg.GetMediaInfo(ff.Remote).Result.Duration;
//        TimeSpan local = FFmpeg.GetMediaInfo(ff.LocalEncoded).Result.Duration;
//        TimeSpan difference = remote - local;
//        if (difference > TimeSpan.FromSeconds(1) || difference < TimeSpan.FromSeconds(-1))
//        {
//            Logger.WriteLine($"{remote} {ff.Remote}");
//            Logger.WriteLine($"{local} {ff.LocalEncoded}");
//            Logger.WriteLine("");
//        }
//    });
;
;

//await FfmpegHelper.EncodeAllAsync(
//    @"", _=> VideoCropMode.AdaptiveCropWithoutLimit, Path.Combine(@"E:\Encode", "2"),
//    inputPredicate: input => true,
//    cropTimestampCount: 7, maxDegreeOfParallelism: 1, sample: true, cancellationToken: cancellationTokenSource.Token);

//Video.PrintVideosWithErrors(@"E:\Files\New folder (2)\SD", searchOption: SearchOption.AllDirectories);

// Video.EnumerateDirectories(settings.MovieTemp42)
//     .ForEach(movie =>
//     {
//         string[] files = Directory.GetFiles(movie);
//         string xmlMetadataFile = files.SingleOrDefault(file => PathHelper.GetFileName(file).EqualsIgnoreCase("movie.nfo"), string.Empty);
//         if (xmlMetadataFile.IsNullOrWhiteSpace())
//         {
//             return;
//         }

//         string video = files.Single(file => file.IsVideo());
//         string videoName = PathHelper.GetFileNameWithoutExtension(video);
//         FileHelper.ReplaceFileNameWithoutExtension(xmlMetadataFile, videoName);
//     });

// Dictionary<string, (string, string)[]> top = (await File.ReadAllLinesAsync(settings.TopDatabase, cancellationTokenSource.Token))
//             .AsParallel()
//             .Where(line => (line.ContainsIgnoreCase("|movies_x265|"))
//                 && (line.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") || line.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")))
//             .Select(line => line.Split('|'))
//             .Do(cells => Debug.Assert(string.IsNullOrEmpty(cells[^2]) || cells[^2].IsImdbId()))
//             //.Do(cells => Debug.Assert(cells[1].ContainsIgnoreCase($"-{settings.TopEnglishKeyword}") || cells[1].ContainsIgnoreCase($"-{settings.TopForeignKeyword}")))
//             .ToLookup(cells => cells[^2], cells => (cells[1], cells[^3]))
//             .ToDictionary(group => group.Key, group => group.ToArray());

// HashSet<string> imdbIds = new(Directory.EnumerateFiles(settings.MovieHdr, "*.json", SearchOption.AllDirectories).Select(f => (f.GetImdbId())));
// top.Where(group => imdbIds.Contains(group.Key))
//     .ForEach(group => group.Select(item => item.Item1).Prepend(group.Key).Append("").ForEach(log));

// string[] imdbIds = File.ReadAllLines(@"d:\temp\2.txt");

// ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationTokenSource.Token);
// imdbIds.Intersect(existingMetadata.Keys).ForEach(log);

// Directory.GetFiles(@"P:\New folder (3)", "*.mkv", SearchOption.AllDirectories)
// .ForEach(sourceVideo =>
// {
//     string destinationDirectory = @"N:\";
//     if (!Directory.Exists(destinationDirectory))
//     {
//         Directory.CreateDirectory(destinationDirectory);
//     }

//     string destinationVideo = Path.Combine(destinationDirectory, PathHelper.GetFileNameWithoutExtension(sourceVideo) + ".mp4");
//     if (File.Exists(destinationVideo))
//     {
//         return;
//     }
//     int result = FfmpegHelper.ExtractAndCompareAsync(settings, sourceVideo, "", destinationVideo).Result;
//     log($"{result} {sourceVideo}");
//     log("");
// });

//Directory.GetFiles(@"K:\Files\Library\Kontrast", "*.srt", SearchOption.AllDirectories)
//    .ForEach(f =>
//    {
//        string name = PathHelper.GetFileNameWithoutExtension(f);
//        string language = name[name.LastIndexOfOrdinal(".")..];
//        if (language.EqualsIgnoreCase(".x265-KONTRAST") || language.EqualsIgnoreCase(".cht-cantonese") || !language.ContainsOrdinal("-"))
//        {
//            return;
//        }

//        if (!language.ContainsIgnoreCase("-sdh"))
//        {
//            return;
//        }

//        string newFile = PathHelper.ReplaceFileNameWithoutExtension(f, n =>
//            language.StartsWithIgnoreCase(".eng") ? n[..n.LastIndexOfOrdinal(".")] : n[..n.LastIndexOfOrdinal("-")]);
//        //if (File.Exists(newFile))
//        //{
//        //    return;
//        //}

//        log(f);
//        File.Move(f, newFile, true);
//        log(newFile);

//        //if (language.EqualsIgnoreCase(".eng-sdh"))
//        //{
//        //    log(f);
//        //    f = FileHelper.ReplaceFileNameWithoutExtension(f, n =>
//        //        n[..n.LastIndexOfOrdinal(".eng-sdh")]);
//        //    log(f);
//        //    return;
//        //}

//        //if (!language.StartsWithIgnoreCase(".chi-"))
//        //{
//        //    return;
//        //}

//        //string detail = language[".chi-".Length..];
//        //if (detail.ContainsIgnoreCase("Simplified") || detail.ContainsIgnoreCase("简"))
//        //{
//        //    log(f);
//        //    f = FileHelper.ReplaceFileNameWithoutExtension(f, n =>
//        //        n[..n.LastIndexOfOrdinal(".")] + ".chs");
//        //    log(f);
//        //    return;
//        //}

//        //if (detail.ContainsIgnoreCase("Cantonese") || detail.ContainsIgnoreCase("Hong") && detail.ContainsIgnoreCase("Kong"))
//        //{
//        //    log(f);
//        //    f = FileHelper.ReplaceFileNameWithoutExtension(f, n =>
//        //        n[..n.LastIndexOfOrdinal(".")] + ".cht-cantonese");
//        //    log(f);
//        //    return;
//        //}

//        //if (detail.ContainsIgnoreCase("Traditional") || detail.ContainsIgnoreCase("繁"))
//        //{
//        //    log(f);
//        //    f = FileHelper.ReplaceFileNameWithoutExtension(f, n =>
//        //        n[..n.LastIndexOfOrdinal(".")] + ".cht");
//        //    log(f);
//        //    return;
//        //}


//    });
static void MoveSubtitles(string sourceDirectory, string destinationDirectory, bool isDryRun = false, Action<string>? log = null)
{
    log ??= Logger.WriteLine;

    string[] sourceFiles = Directory.GetFiles(sourceDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
    Dictionary<string, string[]> sourceVideos = sourceFiles
        .Where(file => file.IsVideo())
        .ToLookup(file =>
        {
            Match match = Regex.Match(PathHelper.GetFileNameWithoutExtension(file), @"\.(?<seasonEpisode>S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["seasonEpisode"].Value : string.Empty;
        })
        .ToDictionary(group => group.Key, group => group.ToArray());
    ILookup<string, string> sourceSubtitles = sourceFiles
        .Where(file => file.IsSubtitle())
        .ToLookup(file =>
        {
            Match match = Regex.Match(PathHelper.GetFileNameWithoutExtension(file), @"\.(?<seasonEpisode>S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["seasonEpisode"].Value : string.Empty;
        });
    HashSet<string> destinationFiles = new(
        Directory.EnumerateFiles(destinationDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories),
        StringComparer.OrdinalIgnoreCase);
    Dictionary<string, string[]> destinationVideos = destinationFiles
        .Where(file => file.IsVideo())
        .ToLookup(file =>
        {
            Match match = Regex.Match(PathHelper.GetFileNameWithoutExtension(file), @"\.(?<seasonEpisode>S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.", RegexOptions.IgnoreCase);
            string key = match.Success ? match.Groups["seasonEpisode"].Value : string.Empty;
            log(key);
            return key;
        })
        .ToDictionary(group => group.Key, group => group.ToArray());
    sourceSubtitles
        .Where(sourceSubtitles => sourceSubtitles.Key.IsNotNullOrWhiteSpace())
        .ForEach(sourceSubtitles =>
        {
            string seasonEpisode = sourceSubtitles.Key;
            Debug.Assert(sourceVideos.TryGetValue(seasonEpisode, out string[]? sourceVideoGroup));
            string sourceVideo = sourceVideoGroup.Single();
            string sourceVideoName = PathHelper.GetFileNameWithoutExtension(sourceVideo);
            sourceSubtitles.ForEach(sourceSubtitle =>
            {
                string sourceSubtitleName = PathHelper.GetFileNameWithoutExtension(sourceSubtitle);
                Debug.Assert(sourceSubtitleName.StartsWithIgnoreCase(sourceVideoName));
                string sourceSubtitleLanguage = sourceSubtitleName[sourceVideoName.Length..];
                Debug.Assert(destinationVideos.TryGetValue(seasonEpisode, out string[]? destinationVideoGroup));
                string destinationVideo = destinationVideoGroup.Single();
                string destinationSubtitle = PathHelper.AddFilePostfix(destinationVideo, sourceSubtitleLanguage);
                destinationSubtitle = PathHelper.ReplaceExtension(destinationSubtitle, PathHelper.GetExtension(sourceSubtitle));
                if (destinationFiles.Contains(destinationSubtitle))
                {
                    return;
                }

                log(sourceSubtitle);
                if (!isDryRun)
                {
                    File.Move(sourceSubtitle, destinationSubtitle);
                }

                log(destinationSubtitle);
            });
        });
}

//Video.PrintDuplicateImdbId(null, @"G:\Files\Library",
//    @"H:\Files\Library",
//    @"I:\Files\Library",
//    @"K:\Files\Library\_Movies Encode4.电影4");

//new Action[]
//{
//    () => Video.RenameDirectoriesWithImdbMetadata(settings, @"G:\Files\Library", 3),
//    () => Video.RenameDirectoriesWithImdbMetadata(settings, @"H:\Files\Library", 3),
//    () => Video.RenameDirectoriesWithImdbMetadata(settings, @"I:\Files\Library", 3),
//    () => Video.RenameDirectoriesWithImdbMetadata(settings, @"K:\Files\Library\_Movies Encode4.电影4", 2)
//}
//.AsParallel()
//.ForAll(action => action());

//await FfmpegHelper.ExtractAllAsync(settings, @"\\box-x\E\Files\New folder (2)\New folder", outputVideos: [input => PathHelper.ReplaceExtension(input, ".mp4")
//    .ReplaceIgnoreCase(@"\\box-x\E\Files\New folder (2)\New folder", @"G:\Files\Library\Movies\New folder")]);
