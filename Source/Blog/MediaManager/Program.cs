using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using TagLib;
using Xabe.FFmpeg;
using File = System.IO.File;
using TagFile = TagLib.File;

AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"..\..\..\Data");
//using TextWriterTraceListener traceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
//Trace.Listeners.Add(traceListener);
//Trace.Listeners.Remove(traceListener);
Trace.Listeners.Add(new ConsoleTraceListener());
//FFmpeg.SetExecutablesPath(@"D:\Data\Software\ffmpeg\bin");

Action<string> log = x => Trace.WriteLine(x);
log(typeof(Enumerable).Assembly.Location);

//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies Politics.政治电影", 1);
//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies Uncategorized.未分类电影");

//await Video.DownloadImdbMetadataAsync(@"E:\Files\Library\Movies 3D.立体电影", overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"E:\Files\Library\Movies Controversial.非主流电影", overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"E:\Files\Library\Movies Mainstream.主流电影", overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"H:\Files\Library\Movies Mainstream.主流电影", 2, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"H:\Files\Library\Movies Controversial.非主流电影", 2, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影", 2, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影", 2, overwrite: false, useCache: true, useBrowser: true);

//await Video.DownloadImdbMetadataAsync(@"E:\Files\Library\TV Controversial.非主流电视剧", 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"E:\Files\Library\TV Documentary.记录电视剧", 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"E:\Files\Library\TV Mainstream.主流电视剧", 1, overwrite: false, useCache: true, useBrowser: true);
//await Video.DownloadImdbMetadataAsync(@"H:\TV", 1, overwrite: false, useCache: true, useBrowser: true);

//Video.PrintDirectoryTitleMismatch(@"E:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoryTitleMismatch(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoryTitleMismatch(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie2");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie3");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie.Subtitle");

//Video.PrintDirectoryTitleMismatch(@"E:\Files\Library\TV Controversial.非主流电视剧", level: 1);
//Video.PrintDirectoryTitleMismatch(@"E:\Files\Library\TV Documentary.记录电视剧", level: 1);
//Video.PrintDirectoryTitleMismatch(@"E:\Files\Library\TV Mainstream.主流电视剧", level: 1);
//Video.PrintDirectoryTitleMismatch(@"H:\TV", level: 1);

//Video.PrintDirectoryOriginalTitleMismatch(@"E:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoryOriginalTitleMismatch(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoryOriginalTitleMismatch(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie2");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie3");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie.Subtitle");

//Video.RenameDirectoriesWithoutAdditionalMetadata(@"E:\Files\Library\Movies 3D.立体电影");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie2");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie3");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie.Subtitle");

//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Library\Movies 3D.立体电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Library\Movies Controversial.非主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Library\Movies Mainstream.主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"H:\Files\Library\Movies Controversial.非主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"H:\Files\Library\Movies Mainstream.主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影", isDryRun: true);

//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Library\TV Controversial.非主流电视剧", level: 1, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Library\TV Documentary.记录电视剧", level: 1, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"E:\Files\Library\TV Mainstream.主流电视剧", level: 1, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"H:\TV", level: 1, isDryRun: true);

//Video.SyncRating(@"E:\Files\Library\Movies 3D.立体电影");
//Video.SyncRating(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.SyncRating(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.SyncRating(@"H:\Files\Library\Movies Controversial.非主流电影");
//Video.SyncRating(@"H:\Files\Library\Movies Mainstream.主流电影");
//Video.SyncRating(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影");
//Video.SyncRating(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影");

//Video.SyncRating(@"E:\Files\Library\TV Controversial.非主流电视剧", 1);
//Video.SyncRating(@"E:\Files\Library\TV Documentary.记录电视剧", level: 1);
//Video.SyncRating(@"E:\Files\Library\TV Mainstream.主流电视剧", level: 1);
//Video.SyncRating(@"H:\TV", level: 1);

//Video.PrintDirectoriesWithErrors(@"E:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoriesWithErrors(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithErrors(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithErrors(@"H:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithErrors(@"H:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithErrors(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithErrors(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影");


//Video.PrintVideosWithErrors(@"E:\Files\Library\Movies 3D.立体电影", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"E:\Files\Library\Movies Controversial.非主流电影", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"E:\Files\Library\Movies Mainstream.主流电影", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie2", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie3", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie.Subtitle", searchOption: SearchOption.AllDirectories);

//Video.PrintVideosWithErrors(@"H:\TV", searchOption: SearchOption.AllDirectories);

//await Video.ConvertToUtf8Async(@"E:\Files\Library\Movies 3D.立体电影");
//await Video.ConvertToUtf8Async(@"E:\Files\Library\Movies Mainstream.主流电影");
//await Video.ConvertToUtf8Async(@"E:\Files\Library\Movies Controversial.非主流电影");
//await Video.ConvertToUtf8Async(@"H:\Files\Library\Movies Mainstream.主流电影");
//await Video.ConvertToUtf8Async(@"H:\Files\Library\Movies Controversial.非主流电影");
//await Video.ConvertToUtf8Async(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影");
//await Video.ConvertToUtf8Async(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影");

//await Video.ConvertToUtf8Async(@"E:\Files\Library\TV Controversial.非主流电视剧");
//await Video.ConvertToUtf8Async(@"E:\Files\Library\TV Documentary.记录电视剧");
//await Video.ConvertToUtf8Async(@"E:\Files\Library\TV Mainstream.主流电视剧");
//await Video.ConvertToUtf8Async(@"E:\Files\Library\TV Tutorial.教程");
//await Video.ConvertToUtf8Async(@"H:\TV");

//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie", isDryRun: true);
//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie2", isDryRun: true);
//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie3", isDryRun: true);
//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie.Subtitle", isDryRun: true);

//Video.DeleteFeaturettesMetadata(@"E:\Files\Library\Movies 3D.立体电影", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"E:\Files\Library\Movies Mainstream.主流电影", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"E:\Files\Library\Movies Controversial.非主流电影", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie2", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie3", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie.Subtitle", isDryRun: true);

//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"E:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"H:\Movie");
//Video.PrintDirectoriesWithMultipleMedia(@"H:\Movie2");
//Video.PrintDirectoriesWithMultipleMedia(@"H:\Movie3");
//Video.PrintDirectoriesWithMultipleMedia(@"H:\Movie.Subtitle");

//Video.PrintSubtitlesWithErrors(@"E:\Files\Library");

//Video.PrintMoviesWithNoSubtitle(@"E:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintMoviesWithNoSubtitle(@"E:\Files\Library\Movies Mainstream.主流电影");

//Video.PrintDuplicateImdbId(null, @"E:\Files\Library\Movies Mainstream.主流电影", @"E:\Files\Library\Movies Controversial.非主流电影", @"E:\NoSubtitle", @"I:\Movie", @"i:\movie2", @"h:\movies", @"h:\movies2", @"h:\movies3", @"j:\encode", @"M:\Movies");

//Video.BackupMetadata(@"H:\Movie3\Andrew Blake");
//Video.RestoreMetadata(@"H:\Movies2");

//Video.PrintDefinitionErrors(@"E:\Files\Library");

//await Video.ConvertToUtf8Async(@"I:\Temple");
//Video.MoveSubtitleToParentDirectory(@"I:\Temple");
//await Video.SaveAllVideoMetadata(@"E:\Files\Library\LibraryMetadata.json", null, @"E:\Files\Library\Movies Controversial.非主流电影", @"E:\Files\Library\Movies Mainstream.主流电影", @"H:\Movie3", @"H:\Movie2", @"F:\New folder\Movie3", @"F:\New folder\Movie2");
//await Video.SaveExternalVideoMetadataAsync(@"H:\New folder (2)\Movie.json", @"H:\New folder (2)");
//await Video.CompareAndMoveAsync(@"H:\New folder (2)\Movie.json", @"E:\Files\Library\LibraryMetadata.json", @"H:\Movie.New", @"H:\Movie.Delete", isDryRun: false);

//Video.MoveSubtitles(@"K:\Move\New folder\The L Word-Generation Q.2019.拉字至上-Q世代[7.3][1080p]\Season 01", ".mp4", @"K:\Move\New folder\The L Word-Generation Q.2019.拉字至上-Q世代[7.3][1080p]\New folder");

//await Rarbg.DownloadMetadataAsync("https://rarbg.to/torrents.php?category[]=54", @"D:\Files\Code\RarbgMetadata.x265.json", index => index <= 10);
//await Rarbg.DownloadMetadataAsync("https://rarbg.to/torrents.php?category[]=44", @"D:\Files\Code\RarbgMetadata.H264.json", index => index <= 20);
//await Yts.DownloadMetadataAsync(@"D:\Files\Code\YtsSummary.json", @"D:\Files\Code\YtsMetadata.json", index => index <= 15);
//await Video.PrintVersions(@"D:\Files\Code\RarbgMetadata.x265.json", @"D:\Files\Code\RarbgMetadata.H264.json", @"D:\Files\Code\YtsMetadata.json", @"D:\Files\Code\Library.Ignore.json", null,
//    (@"E:\Files\Library\Movies Mainstream.主流电影", 2), (@"E:\Files\Library\Movies Controversial.非主流电影", 2),
//    (@"H:\Files\Library\Movies Mainstream.主流电影", 2), (@"H:\Files\Library\Movies Controversial.非主流电影", 2), (@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影", 2), (@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影", 2));

//Audio.ReplaceTraditionalChinese(@"E:\Files\Library\Audio Mainstream.主流音乐", true);

//Audio.PrintDirectoriesWithErrors(@"E:\Files\Library\Audio Controversial.非主流音乐");
//Audio.PrintDirectoriesWithErrors(@"E:\Files\Library\Audio Mainstream.主流音乐");
//Audio.PrintDirectoriesWithErrors(@"E:\Files\Library\Audio Show.节目音频");
//Audio.PrintDirectoriesWithErrors(@"E:\Files\Library\Audio Soundtrack.电影音乐");

//Video.MoveAllSubtitles(@"I:\Movie2\Movie", @"I:\Movie2\Movie.Subtitle");

//Directory.GetFiles(@"E:\Files\Library", "*", SearchOption.AllDirectories)
//    .Where(f => f.Length >= 255)
//    .ForEach(log);

//Video.RenameDirectoriesWithAdditionalMetadata(@"E:\Temp", 1);
//Video.RenameDirectoriesWithoutMetadata(@"E:\Files\Library\Movies Controversial.非主流电影", isDryRun: false);

//Video.PrintVideosWithErrors(@"E:\Files\Library\TV Controversial.非主流电视剧\Fashion TV.2000.法国时尚台[-][-]", searchOption: SearchOption.AllDirectories);
//Video.RenameEpisodes(@"E:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]", "European Adult");
//Video.CreateEpisodeMetadata(@"E:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]");

//Video.PrintVideosWithErrors(@"I:\Downloads", searchOption: SearchOption.AllDirectories);
//Video.RenameDirectoriesWithMetadata(@"I:\MovieMove");
//Video.RestoreMetadata(@"I:\MovieMove");

//Video.RenameEpisodes(@"E:\TV\Full Time Filmmaker.2020.视频制作[-][-][1080p]", "Full Time Filmmaker", isDryRun: false);
//Video.CreateTVEpisodeMetadata(@"E:\TV\Full Time Filmmaker.2020.视频制作[-][-][1080p]");

//Directory.GetFiles(@"E:\Files\Media\Music\Beyond Live.2013.Steve-It's Alright演唱会`2[1080p]", "*.mp4")
//    .OrderBy(s => s)
//    .ForEach((f, i) => File.Move(f, Path.Combine(Path.GetDirectoryName(f), $"Beyond Live It's Alright.2013.cd{i + 1:00}.mp4")));
//Directory.GetFiles(@"E:\Files\Media\Music\Beyond Live.2013.Steve-It's Alright演唱会`2[1080p]", "*.mp4")
//    .OrderBy(s => s)
//    .ForEach((f, i) => log($"{i + 1:00}.{Path.GetFileNameWithoutExtension(f).Replace(".1080p", "")}"));

//Video.RenameDirectoriesWithAdditionalMetadata(@"H:\Movie.Subtitle", isDryRun: false);
//Directory.GetDirectories(@"H:\Files\Library\Movies Mainstream.主流电影", "*", SearchOption.TopDirectoryOnly)
//    .Select(d => d.ReplaceOrdinal(@"H:\Files\Library\", @"H:\Files\Library.Subtitle\"))
//    .Where(d => !File.Exists(d))
//    .ForEach(d => Directory.CreateDirectory(d));
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Files\Library\Movies Mainstream.主流电影", isDryRun: false);
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Files\Library\Movies Controversial.非主流电影", isDryRun: false);
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影", isDryRun: false);
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影", isDryRun: false);

//Video.EnumerateDirectories(@"E:\Files\Library\Movies Mainstream.主流电影")
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
//    .Select(m => (m, new VideoDirectoryInfo(m)))
//    .Where(m => m.Item2.Definition is not ("[1080p]" or "[720p]"))
//    .ForEach(m => log(m.Item1));

//Video.EnumerateDirectories(@"E:\Files\Library\Movies Controversial.非主流电影")
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影"))
//    .Select(m => (m, new VideoDirectoryInfo(m)))
//    .Where(m => m.Item2.Definition is "[1080p]" or "[720p]")
//    .ForEach(m => log(m.Item1));

//Video.EnumerateDirectories(@"E:\Files\Library\Movies Mainstream.主流电影")
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"E:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影"))
//    .Select(m => (m, new VideoDirectoryInfo(m)))
//    .Where(m => m.Item2.Definition is "[720p]")
//    .Select(m => (m.Item1, m.Item2, Directory.GetFiles(m.Item1).Where(Video.IsVideo).ToArray()))
//    .Where(m => !m.Item3.Any(f => f.ContainsIgnoreCase("bluray") || f.ContainsIgnoreCase("hdtv") || f.ContainsIgnoreCase("hdrip") || f.ContainsIgnoreCase("rarbg") || f.ContainsIgnoreCase("yts") || f.ContainsIgnoreCase("fgt")))
//    .ForEach(m =>
//    {
//        log(m.Item1);
//        m.Item3.ForEach(log);
//        log("");
//    });



//Video.EnumerateDirectories(@"E:\Files\Library\Movies Mainstream.主流电影")
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"E:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影"))
//    .OrderBy(d=>d)
//    .ToArray()
//    .Select(m => (m, Imdb.TryLoad(m, out ImdbMetadata imdbMetadata) ? imdbMetadata : null))
//    .ForEach(d=>
//    {
//        log(d.Item1);
//        log(d.Item1 + $"@{string.Join(",", d.Item2?.Genres?.Take(4).OrderBy(g => g).ToArray() ?? Array.Empty<string>())}");
//        //DirectoryHelper.AddPostfix(d.Item1, $"@{string.Join(",", d.Item2.Genres?.OrderBy(g => g).ToArray() ?? Array.Empty<string>())}");
//        log("");
//    });
//.Where(m => m.Item2?.Genres?.ContainsIgnoreCase("Action") is true && !m.Item1.Contains(" Action."))
//.ForEach(m =>
//{
//    log(m.Item1);
//    log(m.Item2.Link);
//    log("");
//});

Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影\American", 1)
    //.Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
    //.Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
    //.Concat(Video.EnumerateDirectories(@"E:\Files\Library\Movies Controversial.非主流电影"))
    //.Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Controversial.非主流电影"))
    //.Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影"))
    .OrderBy(d => d)
    .ToArray()
    .Select(m => (m, Imdb.TryLoad(m, out ImdbMetadata imdbMetadata) ? imdbMetadata : null))
    .Where(m=>m.Item2?.FormattedAggregateRating?.CompareTo("6.5") >= 0)
    //.Where(m => m.Item2?.Genre?.Length == 2)
    .ForEach(d =>
    {
        log(d.Item1);
        log(d.Item1 + $"@{string.Join(",", d.Item2?.Genres?.OrderBy(g => g).ToArray() ?? Array.Empty<string>())}");
        //DirectoryHelper.AddPostfix(d.Item1, $"@{string.Join(",", d.Item2.Genres?.OrderBy(g => g).ToArray() ?? Array.Empty<string>())}");
        log(d.Item2?.Link);
        log("");
    });


//Dictionary<string, string> allLocalRegions = new()
//{
//    ["American"] = "USA:XXX|IMAX",
//    ["British"] = "UK",
//    ["Belgian"] = "Belgium",
//    ["Brazilian"] = "Brazil",
//    ["Canadian"] = "Canada",
//    ["Canadian"] = "Canada",
//    ["Chilean"] = "Chile",
//    ["Chinese American"] = "Hong Kong|USA|China|Taiwan",
//    ["Chinese Cartoon"] = "China",
//    ["Chinese Disability"] = "China|Hong Kong",
//    ["Chinese Documentary"] = "China|Taiwan",
//    ["Chinese Hongkong"] = "Hong Kong",
//    ["Chinese Mainland"] = "China",
//    ["Chinese Musical"] = "Hong Kong|Taiwan",
//    ["Chinese Politics"] = "China|USA|UK",
//    ["Chinese Taiwan"] = "Taiwan",
//    ["Czech"] = "Czech Republic|Czechoslovakia",
//    ["Danish"] = "Denmark",
//    ["Dominican"] = "Dominican Republic",
//    ["Dutch"] = "Netherlands:Peter Greenaway",
//    ["Egyptian"] = "Egypt",
//    ["Filipino"] = "Philippines",
//    ["Finnish"] = "Finland",
//    ["French"] = "France:Emmanuelle",
//    ["German"] = "Germany|West Germany",
//    ["Greek"] = "Greece",
//    ["Hungarian"] = "Hungary",
//    ["Icelandic"] = "Iceland",
//    ["Irish"] = "Ireland",
//    ["Italian"] = "Italy:Mario Salieri|Selen|",
//    ["Japanese"] = "Japan",
//    ["Japanese"] = "Japan",
//    ["Korean Politics"] = "Poland|Russia|Germany",
//    ["Korean"] = "South Korea",
//    ["Mexican"] = "Mexico",
//    ["New"] = "New Zealand",
//    ["Norwegian"] = "Norway",
//    ["Pakistani"] = "Pakistan",
//    ["Polish"] = "Poland",
//    ["Russian Soviet"] = "Russia|Soviet Union",
//    ["South"] = "South Africa",
//    ["Spanish"] = "Spain:Jesús Franco",
//    ["Swedish"] = "Sweden",
//    ["Swiss"] = "Switzerland",
//    ["Thai"] = "Thailand",
//    ["Turkish"] = "Turkey",
//    ["Ukrainian"] = "Ukraine",
//    ["Yugoslavian"] = "Slovenian|Federal Republic of Yugoslavia|Serbia|Bosnia and Herzegovina",
//};

//Directory
//    .GetDirectories(@"E:\Files\Library\Movies Controversial.非主流电影")
//    .Concat(Directory.GetDirectories(@"E:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Directory.GetDirectories(@"H:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Directory.GetDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Directory.GetDirectories(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影"))
//    .Concat(Directory.GetDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
//    .OrderBy(d => d)
//    .ForEach(localRegionDirectory =>
//    {
//        string localRegionText = Path.GetFileNameWithoutExtension(localRegionDirectory);
//        if (localRegionText.EqualsOrdinal("To Delete"))
//        {
//            return;
//        }

//        if (!allLocalRegions.TryGetValue(localRegionText, out string currentLocalRegion))
//        {
//            int lastIndex = localRegionText.LastIndexOfOrdinal(" ");
//            if (lastIndex >= 0)
//            {
//                localRegionText = localRegionText[..lastIndex];
//                if (allLocalRegions.TryGetValue(localRegionText, out currentLocalRegion))
//                {
//                }
//                else
//                {
//                    currentLocalRegion = localRegionText;
//                }
//            }
//            else
//            {
//                currentLocalRegion = localRegionText;
//            }
//        }

//        //log($"==={currentLocalRegions}");
//        string[] currentLocalRegionInfo = currentLocalRegion.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
//        string[] currentRegions = currentLocalRegionInfo.First().Split("|", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
//        string[] ignorePrefixes = currentLocalRegionInfo.Length == 1 ? Array.Empty<string>() : currentLocalRegionInfo.Last().Split("|", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
//        Directory
//            .GetDirectories(localRegionDirectory)
//            .ForEach(movie =>
//            {
//                string movieName = Path.GetFileName(movie);
//                if (ignorePrefixes.Any(movieName.StartsWithOrdinal))
//                {
//                    return;
//                }

//                if (Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata))
//                {
//                    if (!imdbMetadata.Regions.Any(imdbRegion => currentRegions.Any(localRegion => imdbRegion.EqualsOrdinal(localRegion) || $"{imdbRegion}n".EqualsOrdinal(localRegion))))
//                    {
//                        log(movie);
//                        log($"{currentLocalRegion}: {string.Join(", ", imdbMetadata.Regions)}");
//                        log(string.Empty);
//                    }
//                }
//                else
//                {
//                    //log($"!Missing IMDB metadata {movie}");
//                }
//            });
//    });
