using System.Buffers;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using CsQuery;
using Examples.Common;
using Examples.Diagnostics;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using Examples.Security;
using Examples.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V100.HeapProfiler;
using TagLib;
using TagLib.Mpeg;
using Ude;
using Xabe.FFmpeg;
using File = System.IO.File;
using TagFile = TagLib.File;

AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"..\..\..\Data");
//using TextWriterTraceListener traceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
//Trace.Listeners.Add(traceListener);
//Trace.Listeners.Remove(traceListener);
Trace.Listeners.Add(new ConsoleTraceListener());
FFmpeg.SetExecutablesPath(@"C:\ProgramData\chocolatey\bin");

Action<string> log = x => Trace.WriteLine(x);
log(typeof(Enumerable).Assembly.Location);

//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Politics.政治电影", 1);
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Uncategorized.未分类电影");

//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies 3D.立体电影", 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies 4K HDR.高动态范围电影", 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies Controversial.非主流电影", 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies Mainstream.主流电影", 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\Movies Musical.音乐", 2, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Move\Movies", 2, overwrite: false, useCache: true, useBrowser: true, 1);

//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\TV Controversial.非主流电视剧", 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\TV Documentary.记录电视剧", 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Files\Library\TV Mainstream.主流电视剧", 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.DownloadImdbMetadataAsync(@"D:\Move\TV\HD", 1, overwrite: false, useCache: true, useBrowser: true, 1);

//await Video.DownloadImdbMetadataAsync(
//    new[]
//    {
//        (@"D:\Files\Library\Movies 3D.立体电影", 2),
//        (@"D:\Files\Library\Movies 4K HDR.高动态范围电影", 2),
//        (@"D:\Files\Library\Movies Controversial.非主流电影", 2),
//        (@"D:\Files\Library\Movies Mainstream.主流电影", 2),
//        (@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", 2),
//        (@"D:\Files\Library\Movies Musical.音乐", 2),
//        (@"D:\Files\Library\TV Controversial.非主流电视剧", 1),
//        (@"D:\Files\Library\TV Documentary.记录电视剧", 1),
//        (@"D:\Files\Library\TV Mainstream.主流电视剧", 1)
//    },
//    movie => movie.Year is "2022", true, false, true);

//Video.PrintDirectoryTitleMismatch(@"D:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoryTitleMismatch(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoryTitleMismatch(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie2");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie3");
//Video.PrintDirectoryTitleMismatch(@"H:\Movie.Subtitle");

//Video.PrintDirectoryTitleMismatch(@"D:\Files\Library\TV Controversial.非主流电视剧", level: 1);
//Video.PrintDirectoryTitleMismatch(@"D:\Files\Library\TV Documentary.记录电视剧", level: 1);
//Video.PrintDirectoryTitleMismatch(@"D:\Files\Library\TV Mainstream.主流电视剧", level: 1);
//Video.PrintDirectoryTitleMismatch(@"H:\TV", level: 1);

//Video.PrintDirectoryOriginalTitleMismatch(@"D:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoryOriginalTitleMismatch(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoryOriginalTitleMismatch(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie2");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie3");
//Video.PrintDirectoryOriginalTitleMismatch(@"H:\Movie.Subtitle");

//Video.RenameDirectoriesWithoutAdditionalMetadata(@"D:\Files\Library\Movies 3D.立体电影");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie2");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie3");
//Video.RenameDirectoriesWithoutAdditionalMetadata(@"H:\Movie.Subtitle");

//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\Movies 3D.立体电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\Movies 4K HDR.高动态范围电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\Movies Controversial.非主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\Movies Mainstream.主流电影", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\Movies Musical.音乐", isDryRun: true);

//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\TV Controversial.非主流电视剧", level: 1, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\TV Documentary.记录电视剧", level: 1, isDryRun: true);
//Video.RenameDirectoriesWithImdbMetadata(@"D:\Files\Library\TV Mainstream.主流电视剧", level: 1, isDryRun: true);

//Video.SyncRating(@"D:\Files\Library\Movies 3D.立体电影");
//Video.SyncRating(@"D:\Files\Library\Movies 4K HDR.高动态范围电影");
//Video.SyncRating(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.SyncRating(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.SyncRating(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]");
//Video.SyncRating(@"D:\Files\Library\Movies Musical.音乐");

//Video.SyncRating(@"D:\Files\Library\TV Controversial.非主流电视剧", 1);
//Video.SyncRating(@"D:\Files\Library\TV Documentary.记录电视剧", level: 1);
//Video.SyncRating(@"D:\Files\Library\TV Mainstream.主流电视剧", level: 1);
//Video.SyncRating(@"D:\Files\Library\TV Tutorial.教程", level: 1);

//Video.BackupMetadata(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.BackupMetadata(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.BackupMetadata(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]");
//Video.BackupMetadata(@"D:\Files\Library\Movies Musical.音乐");

//Video.BackupMetadata(@"D:\Files\Library\TV Controversial.非主流电视剧");
//Video.BackupMetadata(@"D:\Files\Library\TV Documentary.记录电视剧");
//Video.BackupMetadata(@"D:\Files\Library\TV Mainstream.主流电视剧");

//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies 4K HDR.高动态范围电影");
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]");
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies Musical.音乐");
//Video.PrintDirectoriesWithErrors(@"D:\Move\Movies");

//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\TV Controversial.非主流电视剧", 1, isTV: true);
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\TV Documentary.记录电视剧", 1, isTV: true);
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\TV Mainstream.主流电视剧", 1, isTV: true);
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\TV Tutorial.教程", 1, isTV: true);

//Video.PrintVideosWithErrors(@"D:\Files\Library\Movies 3D.立体电影", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"D:\Files\Library\Movies Controversial.非主流电影", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"D:\Files\Library\Movies Mainstream.主流电影", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie2", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie3", searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Movie.Subtitle", searchOption: SearchOption.AllDirectories);

//await Video.ConvertToUtf8Async(@"D:\Files\Library\Movies 3D.立体电影");
//await Video.ConvertToUtf8Async(@"D:\Files\Library\Movies Mainstream.主流电影");
//await Video.ConvertToUtf8Async(@"D:\Files\Library\Movies Controversial.非主流电影");
//await Video.ConvertToUtf8Async(@"H:\Files\Library\Movies Mainstream.主流电影");
//await Video.ConvertToUtf8Async(@"H:\Files\Library\Movies Controversial.非主流电影");

//await Video.ConvertToUtf8Async(@"D:\Files\Library\TV Controversial.非主流电视剧");
//await Video.ConvertToUtf8Async(@"D:\Files\Library\TV Documentary.记录电视剧");
//await Video.ConvertToUtf8Async(@"D:\Files\Library\TV Mainstream.主流电视剧");
//await Video.ConvertToUtf8Async(@"D:\Files\Library\TV Tutorial.教程");
//await Video.ConvertToUtf8Async(@"D:\Files\Library\");

//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie", isDryRun: true);
//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie2", isDryRun: true);
//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie3", isDryRun: true);
//await Video.RenameSubtitlesByLanguageAsync(@"H:\Movie.Subtitle", isDryRun: true);

//Video.DeleteFeaturettesMetadata(@"D:\Files\Library\Movies 3D.立体电影", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"D:\Files\Library\Movies Mainstream.主流电影", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"D:\Files\Library\Movies Controversial.非主流电影", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie2", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie3", isDryRun: true);
//Video.DeleteFeaturettesMetadata(@"H:\Movie.Subtitle", isDryRun: true);

//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies 3D.立体电影");
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies 4K HDR.高动态范围电影");
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Mainstream.主流电影");
//Video.PrintDirectoriesWithMultipleMedia(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]");

//Video.PrintSubtitlesWithErrors(@"E:\Files\Library");

//Video.PrintMoviesWithNoSubtitle(@"D:\Files\Library\Movies Controversial.非主流电影");
//Video.PrintMoviesWithNoSubtitle(@"D:\Files\Library\Movies Mainstream.主流电影");

//Video.PrintDuplicateImdbId(null,
//    @"D:\Files\Library\Movies Mainstream.主流电影",
//    @"D:\Files\Library\Movies Controversial.非主流电影",
//    @"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", @"J:\Media\Movies");

//Video.PrintDefinitionErrors(@"D:\Files\Library");

//await Video.ConvertToUtf8Async(@"D:\Files\Library");

//Video.MoveSubtitleToParentDirectory(@"D:\Move\Movies\HD");
//await Video.SaveAllVideoMetadata(@"D:\Files\Library\Movie.LibraryMetadata.json", null, @"D:\Files\Library\Movies Controversial.非主流电影", @"D:\Files\Library\Movies Mainstream.主流电影", @"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", @"D:\Files\Library\Movies Musical.音乐");
//await Video.SaveExternalVideoMetadataAsync(@"J:\Media\Movies\Movie.json", @"J:\Media\Movies");
//await Video.CompareAndMoveAsync(@"J:\Media\Movies\Movie.json", @"D:\Files\Library\Movie.LibraryMetadata.json", @"J:\Movie.New", @"J:\Movie.Delete", isDryRun: false);

//Video.MoveAllSubtitles(@"J:\Media\Movies", @"J:\Media\Movies.Sub");

//await Rarbg.DownloadMetadataAsync("https://rarbg.to/torrents.php?category[]=54", @"D:\Files\Library\Movie.RarbgMetadata.x265.json", log, index => index <= 5);
//await Rarbg.DownloadMetadataAsync("https://rarbg.to/torrents.php?category[]=44", @"D:\Files\Library\Movie.RarbgMetadata.H264.json", log, index => index <= 20);
//await Rarbg.DownloadMetadataAsync("https://rarbg.to/torrents.php?category[]=45", @"D:\Files\Library\Movie.RarbgMetadata.H264.720p.json", log, index => index <= 10);
//await Rarbg.DownloadMetadataAsync("https://rarbg.to/torrents.php?search=x265+rarbg&category=41", @"D:\Files\Library\TV.RarbgMetadata.x265.json", log, index => index <= 5);
//await Yts.DownloadMetadataAsync(@"D:\Files\Library\Movie.YtsSummary.json", @"D:\Files\Library\Movie.YtsMetadata.json", log, index => index <= 10);
//await Video.PrintMovieVersions(@"D:\Files\Library\Movie.RarbgMetadata.x265.json ", @"D:\Files\Library\Movie.RarbgMetadata.H264.json", @"D:\Files\Library\Movie.RarbgMetadata.H264.720p.json", @"D:\Files\Library\Movie.YtsMetadata.json", @"D:\Files\Library\Movie.Ignore.json", null,
//    (@"D:\Files\Library\Movies Mainstream.主流电影", 2),
//    (@"D:\Files\Library\Movies Controversial.非主流电影", 2),
//    (@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", 2),
//    (@"D:\Files\Library\Movies Musical.音乐", 2),
//    (@"D:\Move\Movies", 2)
//    );
//await Video.PrintTVVersions(@"D:\Files\Library\TV.RarbgMetadata.x265.json", null,
//    (@"D:\Files\Library\TV Controversial.非主流电视剧", 1),
//    (@"D:\Files\Library\TV Documentary.记录电视剧", 1),
//    (@"D:\Files\Library\TV Mainstream.主流电视剧", 1));

//await Yts.DownloadMetadataAsync(@"D:\Files\Code\YtsSummary.json", @"D:\Files\Code\YtsMetadata.json", 4);
//Audio.ReplaceTraditionalChinese(@"D:\Files\Library\Audio Mainstream.主流音乐", true);

//Audio.PrintDirectoriesWithErrors(@"D:\Files\Library\Audio Controversial.非主流音乐");
//Audio.PrintDirectoriesWithErrors(@"D:\Files\Library\Audio Mainstream.主流音乐");
//Audio.PrintDirectoriesWithErrors(@"D:\Files\Library\Audio Show.节目音频");
//Audio.PrintDirectoriesWithErrors(@"D:\Files\Library\Audio Soundtrack.电影音乐");

//Video.MoveAllSubtitles(@"I:\Movie2\Movie", @"I:\Movie2\Movie.Subtitle");

//Directory.GetFiles(@"E:\Files\Library", "*", SearchOption.AllDirectories)
//    .Where(f => f.Length >= 255)
//    .ForEach(log);

//Video.RenameDirectoriesWithMetadata(@"D:\Move\Movies\New folder", 1);
//Video.RenameDirectoriesWithAdditionalMetadata(@"D:\Move\Movies", 2);
//Video.RenameDirectoriesWithoutMetadata(@"D:\Files\Library\Movies Controversial.非主流电影", isDryRun: false);

//Video.PrintVideosWithErrors(@"D:\Files\Library\TV Controversial.非主流电视剧\Fashion TV.2000.法国时尚台[-][-]", searchOption: SearchOption.AllDirectories);
//Video.RenameEpisodes(@"D:\Files\Library\TV Controversial.非主流电视剧\European Adult.2020.欧洲成人[-][-]", "European Adult");
//Video.CreateTVEpisodeMetadata(@"T:\Files\Library\TV Tutorial.教程\English Toefl.2017.英语托福[---][-]");

//Video.PrintVideosWithErrors(@"I:\Downloads", searchOption: SearchOption.AllDirectories);
//Video.RenameDirectoriesWithMetadata(@"H:\Downloads7\New folder", 1);
//Video.RenameDirectoriesWithMetadata(@"H:\Downloads7\New folder (2)", 1);
//Video.RestoreMetadata(@"H:\Downloads7\New folder");
//Video.RestoreMetadata(@"H:\Downloads7\New folder (2)");

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

//Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影")
//   .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]"))
//   .Select(m => (m, new VideoDirectoryInfo(m)))
//   .Where(m => !m.Item2.IsHD)
//   .OrderBy(d => d)
//   .ForEach(m => log(m.Item1));

//Video.EnumerateDirectories(@"D:\Files\Library\Movies Controversial.非主流电影")
//   .Select(m => (m, new VideoDirectoryInfo(m)))
//   .Where(m => m.Item2.IsHD)
//   .OrderBy(d => d)
//   .ForEach(m => log(m.Item1));

//Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影")
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Controversial.非主流电影"))
//    .OrderBy(m => m)
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

//Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影")
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"H:\Files\Library.Subtitle\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies Controversial.非主流电影"))
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

Dictionary<string, string> allLocalRegions = new()
{
    ["American"] = "USA:XXX|IMAX",
    ["British"] = "UK",
    ["Belgian"] = "Belgium",
    ["Brazilian"] = "Brazil",
    ["Canadian"] = "Canada",
    ["Canadian"] = "Canada",
    ["Chilean"] = "Chile",
    ["Chinese American"] = "Hong Kong|USA|China|Taiwan",
    ["Chinese Biography"] = "Hong Kong|USA|China",
    ["Chinese Cartoon"] = "China",
    ["Chinese Disability"] = "China|Hong Kong",
    ["Chinese Documentary"] = "China|Taiwan",
    ["Chinese Hongkong"] = "Hong Kong",
    ["Chinese Mainland"] = "China",
    ["Chinese Musical"] = "Hong Kong|Taiwan",
    ["Chinese Politics"] = "China|USA|UK",
    ["Chinese Singapore"] = "Singapore",
    ["Chinese Taiwan"] = "Taiwan",
    ["Czech"] = "Czech Republic|Czechoslovakia",
    ["Danish"] = "Denmark",
    ["Dominican"] = "Dominican Republic",
    ["Dutch "] = "Netherlands :Peter Greenaway",
    ["Egyptian"] = "Egypt",
    ["Filipino"] = "Philippines ",
    ["Finnish "] = "Finland ",
    ["French"] = "France:Emmanuelle",
    ["German"] = "Germany|West Germany",
    ["Greek"] = "Greece",
    ["Hungarian"] = "Hungary",
    ["Icelandic "] = "Iceland ",
    ["Irish"] = "Ireland",
    ["Italian"] = "Italy:Mario Salieri|Selen|",
    ["Japanese"] = "Japan",
    ["Japanese"] = "Japan",
    ["Korean Politics "] = "Poland |Russia|Germany ",
    ["Korean"] = "South Korea",
    ["Mexican"] = "Mexico",
    ["New"] = "New Zealand",
    ["Norwegian"] = "Norway",
    ["Pakistani"] = "Pakistan",
    ["Polish"] = "Poland",
    ["Russian Soviet"] = "Russia|Soviet Union",
    ["Russian Soviet Union"] = "Russia | Soviet Union",
    ["South"] = "South Africa",
    ["Spanish"] = "Spain:Jesús Franco",
    ["Swedish"] = "Sweden",
    ["Swiss"] = "Switzerland",
    ["Thai"] = "Thailand",
    ["Turkish"] = "Turkey",
    ["Ukrainian"] = "Ukraine",
    ["Yugoslavian"] = "Slovenian|Federal Republic of Yugoslavia|Serbia|Bosnia and Herzegovina|Yugoslavia",
};

//Directory
//    .GetDirectories(@"D:\Files\Library\Movies Controversial.非主流电影")
//    .Concat(Directory.GetDirectories(@"D:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Directory.GetDirectories(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]"))
//    .OrderBy(d => d)
//    .ForEach(localRegionDirectory =>
//    {
//        string localRegionText = Path.GetFileNameWithoutExtension(localRegionDirectory);
//        if (localRegionText.ContainsIgnoreCase("Delete"))
//        {
//            return;
//        }

//        if (!allLocalRegions.TryGetValue(localRegionText, out string? currentLocalRegion))
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

//        log($"==={currentLocalRegion}");
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

//Video.EnumerateDirectories(@"D:\Files\Library\Movies 4K HDR.高动态范围电影")
//    .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies 3D.立体电影"))
//    .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies Controversial.非主流电影"))
//    .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影"))
//    .Concat(Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]"))
//    .Where(d => new VideoDirectoryInfo(d).AggregateRating.CompareOrdinal("7.5") >= 0 && Imdb.TryLoad(d, out ImdbMetadata? metadata) && metadata.AggregateRating?.RatingCount < 5000)
//    .OrderBy(d => d)
//    .ToArray()
//    .ForEach(d =>
//    {
//        log(d);
//    });

// Video.MoveAllSubtitles(@"D:\Files\Library\TV Mainstream.主流电视剧[未抓取字幕]", @"D:\Files\Library\TV Mainstream.主流电视剧[未抓取字幕]s");

// new string[] { @"D:\Files\Library\Audio Controversial.非主流音乐", @"D:\Files\Library\Audio Mainstream.主流音乐", @"D:\Files\Library\Audio Show.节目音频", @"D:\Files\Library\Audio Soundtrack.电影音乐" }.SelectMany(d => Directory.EnumerateDirectories(d, "*", SearchOption.TopDirectoryOnly))
// .Where(d => d.EndsWith("[-][-]"))
// .ToArray()
// .ForEach(d =>
// {
//     string[] files = Directory.GetFiles(d);
//     if (files.Length == 0 || files.Length == 1 && Path.GetFileName(files.Single()) == "album.nfo")
//     {
//         DirectoryHelper.Delete(d);
//         log(d);

//     }
//     else
//     {
//         log("!" + d);

//     }
// });

//Video.RenameEpisodesWithTitle(
//    @"D:\Move\TV\HD\Money.Heist",
//    @"D:\Move\TV\HD\Money.Heist",
//    rename: (f, t) => f.Replace("-RARBG", $"-RARBG.{t}"),
//    //rename: (f, t) => Regex.Replace(f, @"(\.S[0-9]{2}E[0-9]{2})", $"{"$1".ToUpperInvariant()}.{t}"),
//    isDryRun: false);

// string[] tvs = {@"D:\Files\Library\TV Controversial.非主流电视剧",@"D:\Files\Library\TV Documentary.记录电视剧",@"D:\Files\Library\TV Mainstream.主流电视剧",@"D:\Files\Library\TV Mainstream.主流电视剧[未抓取字幕]",@"D:\Files\Library\TV Tutorial.教程"};
// tvs.ForEach(d=>Video.RenameFiles(d,(f, i) => f.ReplaceIgnoreCase(".BluRay.720p", ".720p.BluRay").ReplaceIgnoreCase(".BluRay.1080p", ".1080p.BluRay").ReplaceIgnoreCase(".WEBRip.720p", ".720p.WEBRip").ReplaceIgnoreCase(".WEBRip.1080p", ".1080p.WEBRip").ReplaceIgnoreCase(".HDTV.720p", ".720p.HDTV").ReplaceIgnoreCase(".HDTV.1080p", ".1080p.HDTV").Replace("Bluray","BluRay")));

//Video.RenameFiles(@"D:\Files\Library\TV Mainstream.主流电视剧[未抓取字幕]\Shameless.2011.无耻之徒[8.5-211K][TVMA][1080p]\Season 05", (f, i) => f.ReplaceIgnoreCase("简体", "chs").Replace("英文", "eng").Replace(".x264-rovers.ass", ".chs&eng.ass"), isDryRun: false);

//Video.RenameDirectories(@"D:\User\Downloads\Thomas Bergersen - Collection (MP3)", (d) => d.Replace("Trailer.", "Trailer-Thomas Bergersen.") + "[---][-]");
//Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]")
//    .ForEach(m =>
//    {
//        string[] files = Directory.GetFiles(m);
//        if (!files.Any(Video.IsTextSubtitle))
//        {
//            Imdb.TryLoad(m, out ImdbMetadata? meta);
//            log(meta?.ImdbId ?? "-");
//            log(m);
//            log("");
//        }
//    });
//await Video.DownloadImdbMetadataAsync(@"J:\Media\Movies", 2, useBrowser: true, overwrite: false, useCache: false, degreeOfParallelism: 1);
//await Video.DownloadImdbMetadataAsync(@"H:\Encode", 2, useBrowser: true, overwrite: true, useCache: false, degreeOfParallelism: 1);
//Video.RestoreMetadata(@"D:\Files\Library\New folder");
//Video.RestoreMetadata(@"H:\Encode");
//Video.RestoreMetadata(@"H:\Nikkatsu Roman Porno Recopilación AT");
//Video.BackupMetadata(@"D:\Move\Movies");
//Video.RenameDirectoriesWithMetadata(@"D:\Move\Movies", 2, isDryRun: false);
//Video.RestoreMetadata(@"D:\Move\Movies");
//Video.RestoreMetadata(@"J:\Media\Movies");

//Video.RenameFiles(@"H:\Downloads7\New folder (3)\The Tales of Nights.2007.奇幻孽缘[---][-]", (f, i) => f.ReplaceIgnoreCase(".HC.Eng.Subs.KR", ".KOREAN.SUBBED.eng"), searchOption: SearchOption.AllDirectories);
//Video.PrintVideosWithErrors(@"H:\Nikkatsu Roman Porno Recopilación AT", searchOption: SearchOption.AllDirectories);
//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\New folder", 2);
//Video.PrintDirectoriesWithErrors(@"H:\Encode", 2);
//Video.PrintDirectoriesWithErrors(@"H:\Nikkatsu Roman Porno Recopilación AT", 1);
//Video.PrintDirectoriesWithErrors(@"H:\SD", 1);

//Directory.EnumerateDirectories(@"D:\Files\Library\Movies 4K HDR.高动态范围电影", "Featurettes", SearchOption.AllDirectories)
//    .Concat(Directory.EnumerateDirectories(@"D:\Files\Library\Movies 3D.立体电影", "Featurettes", SearchOption.AllDirectories))
//    .Concat(Directory.EnumerateDirectories(@"D:\Files\Library\Movies Controversial.非主流电影", "Featurettes", SearchOption.AllDirectories))
//    .Concat(Directory.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影", "Featurettes", SearchOption.AllDirectories))
//    .Concat(Directory.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]", "Featurettes", SearchOption.AllDirectories))
//    .Where(d => Directory.GetFiles(d).Any(f => f.EndsWithIgnoreCase(".nfo")))
//    .OrderBy(d => d)
//    .ToArray()
//    .ForEach(d =>
//    {
//        string newD = d.ReplaceIgnoreCase(@"D:\Files\Library\", @"S:\Files\Featurettes\");
//        log(d);
//        log(newD);
//        //string newParent = Path.GetDirectoryName(newD)!;
//        //if (!Directory.Exists(newParent))
//        //{
//        //    Directory.CreateDirectory(newParent);
//        //}

//        //Directory.Move(d.Replace(@"E:\", @"S:\"), newD);

//        log("");
//    });
//Video.RenameFiles(@"H:\Downloads7\New folder (6)\New folder (3)\New folder", (f,i)=>f.ReplaceIgnoreCase("title_t", "Extra "));

//Video.EnumerateDirectories(@"D:\Files\Library\Movies Mainstream.主流电影[未抓取字幕]")
//    .ForEach(d =>
//    {
//        string[] files = Directory.GetFiles(d).Select(f => Path.GetFileName(f)!).ToArray();
//        if (!files.Any(Video.IsTextSubtitle))
//        {
//            log(files.Single(f => f.EndsWith(".json")).Split("-").First());
//            log(d);
//            log("");
//        }
//    });

//Directory.EnumerateFiles(@"D:\Files\Library", "*.nfo", SearchOption.AllDirectories)
//    .OrderBy(f=>f)
//    .ToArray()
//    .ForEach(f =>
//    {
//        string content = File.ReadAllText(f);
//        if (content.ContainsIgnoreCase(@"D:\Files\Library\"))
//        {
//            content.ReplaceIgnoreCase(@"D:\Files\Library\", @"D:\Files\Library\");
//            File.WriteAllText(f, content);
//            log(f);
//        }
//    });

//Video.RenameFiles(@"H:\Downloads7\New folder (6)\Satisfaction",
//    (f, i) => Regex.Replace(f, "(S[0-9]{2}E[0-9]{2})", "$1.1080p.WEBRip-NTb.ffmpeg")
//        .Replace(".1080p.BNGE.WEB-DL.DD5.1.H.264-NTb", "")
//        .Replace("_track3_[eng]", "").Replace("_track4_[eng]", ".eng"));
//Video.RenameFiles(@"T:\Files\Library\TV Mainstream.主流电视剧\Da Vinci's Demons.2013.达·芬奇的恶魔[8.0-75K][TVMA][1080p]\Season 02", (f, i) => f.Replace(".1080p", ".1080p.BluRay"));


//Directory.GetFiles(@"I:\Subtitle", "*", SearchOption.AllDirectories)
//    .ForEach(f => File.Move(f, f.ReplaceIgnoreCase(".ass", ".chs&eng.ass")));

//Directory.GetFiles(@"D:\Move\TV\New folder", "*", SearchOption.AllDirectories)
//    .ForEach(f => File.Move(f, f
//        .Replace(".简体&英文.", ".chs&eng.")
//    .Replace(".简体.", ".chs.").Replace(".英文.", ".eng.")
//        .Replace(".ass", ".chs&eng.ass")));
//Video.MoveSubtitlesForEpisodes(
//    @"D:\Move\TV\HD\Euphoria",
//    @"D:\Move\TV\New folder",
//    //".mkv",
//    overwrite: true);

//await Video.ConvertToUtf8Async(@"T:\Files\Library\TV Mainstream.主流电视剧\Hung.2009.大器晚成[7.1-21K][TVMA][1080p]\Season 01");
//Video.CreateTVEpisodeMetadata(@"H:\Downloads7\New folder (6)\阅读\_1", f => Path.GetFileNameWithoutExtension(f).Split(".").Last());

//Directory.GetDirectories(@"S:\Files\Media\Temp\1")
//    .Select(d => new VideoDirectoryInfo(d))
//    .Where(d => d.TranslatedTitle1.ContainsOrdinal(" "))
//    .ForEach(d => log(d.ToString()));

string nfo = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<movie>
  <plot />
  <outline />
  <lockdata>false</lockdata>
  <dateadded>2020-01-31 21:07:21</dateadded>
  <title>{title}</title>
  <year>{year}</year>
  <art>
    <poster>{directory}\poster.jpg</poster>
  </art>
  <actor>
    <name>Paul Wong</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\P\Paul Wong\folder.jpg</thumb>
  </actor>
  <actor>
    <name>Steve Wong Ka-Keung</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\S\Steve Wong Ka-Keung\folder.jpg</thumb>
  </actor>
  <actor>
    <name>Wong Ka-Kui</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\W\Wong Ka-Kui\folder.jpg</thumb>
  </actor>
  <actor>
    <name>Yip Sai-Wing</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\Y\Yip Sai-Wing\folder.jpg</thumb>
  </actor>
</movie>";

XElement koma = XElement.Parse(@"  <actor><name>Wong Ka-Kui</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\W\Wong Ka-Kui\folder.jpg</thumb>
  </actor>");

XElement wing = XElement.Parse(@"  <actor>
    <name>Yip Sai-Wing</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\Y\Yip Sai-Wing\folder.jpg</thumb>
  </actor>");

XElement paul = XElement.Parse(@"  <actor>
    <name>Paul Wong</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\P\Paul Wong\folder.jpg</thumb>
  </actor>");

XElement steve = XElement.Parse(@"  <actor>
    <name>Steve Wong Ka-Keung</name>
    <role>Self</role>
    <type>Actor</type>
    <thumb>D:\Files\Software\Jellyfin\metadata\People\S\Steve Wong Ka-Keung\folder.jpg</thumb>
  </actor>");

//Video.BackupMetadata(@"S:\Files\Media\Temp\1");
//Directory.GetFiles(@"S:\Files\Media\Temp\1", "*.nfo", SearchOption.AllDirectories)
//    .ForEach(f =>
//    {
//        log(f);
//        string d = Path.GetDirectoryName(f)!;
//        VideoDirectoryInfo directoryInfo = new(d);
//        string i = Path.Combine(d, "Introduction.txt");
//        XDocument document = XDocument.Load(f);

//        string title = $"{directoryInfo.TranslatedTitle1}: {directoryInfo.TranslatedTitle2.TrimStart('-')} {directoryInfo.TranslatedTitle3.TrimStart('-')} {directoryInfo.TranslatedTitle4.TrimStart('-')}".Trim();
//        log(title);
//        document.Root.Element("title").Value = title;

//        string year = directoryInfo.Year.ContainsOrdinal("-") ? "2020" : directoryInfo.Year;
//        XElement? yearElement = document.Root.Element("year");
//        if (yearElement is not null)
//        {
//            yearElement.Value = year;
//        }
//        else
//        {
//            document.Root.Element("title").AddAfterSelf(new XElement("year") { Value = year });
//        }

//        string plot = File.Exists(i) ? File.ReadAllText(i).Trim() : "";
//        document.Root.Element("plot").Value = plot.IsNullOrWhiteSpace() ? "" : plot;

//        document.Root.Elements("originaltitle")?.Remove();
//        document.Root.Elements("tag").Remove();
//        document.Root.Elements("genre").Remove();
//        document.Root.Elements("director").Remove();
//        document.Root.Elements("trailer").Remove();
//        document.Root.Elements("premiered").Remove();
//        document.Root.Elements("releasedate").Remove();
//        document.Root.Elements("actor").Remove();

//        if (directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Beyond"))
//        {
//            if (directoryInfo.Year.Contains("-"))
//            {
//                if (directoryInfo.Year.StartsWith("19"))
//                {
//                    document.Root.Element("art").AddAfterSelf(koma, paul, steve, wing);
//                }
//                else
//                {
//                    document.Root.Element("art").AddAfterSelf(paul, steve, wing);
//                }
//            }
//            else
//            {
//                if (string.Compare(directoryInfo.Year, "1993", StringComparison.OrdinalIgnoreCase) <= 0)
//                {
//                    document.Root.Element("art").AddAfterSelf(koma, paul, steve, wing);
//                }
//                else
//                {
//                    document.Root.Element("art").AddAfterSelf(paul, steve, wing);
//                }
//            }
//        }
//        else
//        {
//            if (directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Paul"))
//            {
//                document.Root.Element("art").AddAfterSelf(paul);
//            }

//            if (directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Wing"))
//            {
//                document.Root.Element("art").AddAfterSelf(wing);
//            }

//            if (directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Steve"))
//            {
//                document.Root.Element("art").AddAfterSelf(steve);
//            }
//        }

//        log("");
//        document.Save(f);
//    });

//Directory.GetFiles(@"S:\Files\Media\Music\Beyond Live.2009.Paul Steve-香港红磡This is rock & roll演唱会[---][-]", "*")
//    .ForEach(f => File.Move(f, f+".mp4"));
//Directory.GetDirectories(@"D:\Files\Library\Movies Musical.音乐\Chinese Musical.中国音乐", "Beyond Live.*", SearchOption.TopDirectoryOnly)
//    .ForEach(d =>
//    {
//        VideoDirectoryInfo directoryInfo = new(d);
//        //string poster3 = @"D:\User\Downloads\33.jpg";
//        //string poster4 = @"D:\User\Downloads\44.jpg";

//        string paul = @"D:\User\Downloads\Paul Live.jpg";
//        string steve = @"D:\User\Downloads\Steve Live.jpg";
//        string wing = @"D:\User\Downloads\Wing Live.jpg";
//        //if (directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Beyond"))
//        //{
//        //    if (directoryInfo.Year.Contains("-"))
//        //    {
//        //        if (directoryInfo.Year.StartsWith("19"))
//        //        {
//        //            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(poster4, f, true));
//        //        }
//        //        else
//        //        {
//        //            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(poster3, f, true));
//        //        }
//        //    }
//        //    else
//        //    {
//        //        if (string.Compare(directoryInfo.Year, "1993", StringComparison.OrdinalIgnoreCase) <= 0)
//        //        {
//        //            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(poster4, f, true));
//        //        }
//        //        else
//        //        {
//        //            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(poster3, f, true));
//        //        }
//        //    }
//        //}
//        //else if (directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Paul")
//        //    || directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Wing")
//        //    || directoryInfo.TranslatedTitle1.ContainsIgnoreCase("Steve"))
//        //{
//        //    Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(poster3, f, true));
//        //}
//        //else
//        //{
//        //    log(d);
//        //}

//        if (directoryInfo.TranslatedTitle1.EqualsIgnoreCase("Paul"))
//        {
//            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(paul, f, true));
//        }
//        else if (directoryInfo.TranslatedTitle1.EqualsIgnoreCase("Steve"))
//        {
//            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(steve, f, true));
//        }
//        else if (directoryInfo.TranslatedTitle1.EqualsIgnoreCase("Wing"))
//        {
//            Directory.GetFiles(d, "*poster.jpg").ForEach(f => File.Copy(wing, f, true));
//        }
//    });

//await Video.DownloadImdbMetadataAsync(@"S:\Files\Media\Temp", useBrowser:true, useCache:true,degreeOfParallelism:1);
//Video.RenameFiles(@"S:\Files\Media\Temp\1\Beyond Live.2013.Steve-It's Alright演唱会`2[---][-][1080p]", (f,i)=>f.Replace("Beyond Live It's Alright.2013.cd", "Beyond Live It's Alright.2013.1080p.cd"));
//Directory.GetDirectories(@"S:\Files\Library\Movies Musical.音乐\Chinese Musical.中国音乐", "Beyond *", SearchOption.TopDirectoryOnly)
//    .SelectMany(d => Directory.GetFiles(d, "*.nfo", SearchOption.TopDirectoryOnly))
//    .ForEach(f =>
//    {
//        XDocument document = XDocument.Load(f);
//        XElement? rating = document.Root.Element("rating");
//        if (rating is not null)
//        {
//            log(f);
//            rating.Remove();
//        }
//    });

//Video.PrintDirectoriesWithErrors(@"D:\Files\Library\Movies Musical.音乐");
//Video.RenameFiles(@"H:\Downloads7\New folder (6)\New folder", (f, i) => f.ReplaceIgnoreCase(".cht.srt", ".chs.srt")
//    .Replace(".1x", ".S01E")
//    .Replace(".2x", ".S02E")
//    .Replace(".3x", ".S03E")
//);
//await Video.DownloadImdbMetadataAsync(@"S:\Files\Library\Movies Mainstream.主流电影\Chinese Mainland.大陆", 1, overwrite: false, useCache: true, useBrowser: true, 1);
//await Video.ConvertToUtf8Async(@"D:\User\Downloads\New folder (3)");
//Directory.GetFiles(@"D:\User\Downloads\New folder (3)").ForEach(f => File.Move(f, f.ReplaceIgnoreCase(".srt", ".cht.srt")));
//Video.RenameFiles(@"I:\Temp\Naked SNCTM.2017.性爱俱乐部[5.2-203][TVMA][1080p]", (f, i) => f.Replace(".H264.DDP", ""), searchOption:SearchOption.AllDirectories);

//Video.RenameFiles(@"G:\Downloads3\步非烟1", (f,i)=>f.Replace("★", ""));

static void TraceLog(string? message) => Trace.WriteLine(message);

static void MoveTVEpisodes(string directory, string subtitleBackupDirectory, bool isDryRun = false, Action<string>? log = null)
{
    log ??= TraceLog;

    Directory
        .GetFiles(directory, "*.txt", SearchOption.AllDirectories)
        .ForEach(textFile =>
        {
            log($"Delete {textFile}");
            if (!isDryRun)
            {
                File.Delete(textFile);
            }
        });

    Directory
        .GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
        .Where(file => Path.GetFileNameWithoutExtension(file).ContainsOrdinal(" (1)"))
        .ForEach(duplicateFile =>
        {
            log($"Delete {duplicateFile}");
            if (!isDryRun)
            {
                File.Delete(duplicateFile);
            }
        });

    Directory
        .GetDirectories(directory)
        .Where(season => season.EndsWithIgnoreCase("-RARBG"))
        .ForEach(season =>
        {
            string subtitleDirectory = Path.Combine(season, "Subs");
            Directory
                .GetDirectories(subtitleDirectory)
                .ForEach(episodeSubtitleDirectory => Directory
                    .GetFiles(episodeSubtitleDirectory)
                    .ForEach(subtitle =>
                    {
                        string newSubtitle = Path.Combine(season, $"{Path.GetFileName(episodeSubtitleDirectory)}.{Path.GetFileName(subtitle)}");
                        log($"Move {subtitle}");
                        if (!isDryRun)
                        {
                            File.Move(subtitle, newSubtitle);
                        }

                        log(newSubtitle);
                        log(string.Empty);
                    }));
        });

    Directory
        .GetDirectories(directory)
        .Where(season => season.EndsWithIgnoreCase("-RARBG"))
        .ForEach(season =>
        {
            Match match = Regex.Match(Path.GetFileName(season), @"^(.+)\.S([0-9]{2})(\.[A-Z]+)?\.1080p\.+");
            Debug.Assert(match.Success && match.Groups.Count is 3 or 4);
            string title = match.Groups[1].Value;
            string seasonNumber = match.Groups[2].Value;
            string tv = Path.Combine(directory, title);
            string newSeason = Path.Combine(tv, $"Season {seasonNumber}");
            log($"Move {season}");
            if (!isDryRun)
            {
                if (!Directory.Exists(tv))
                {
                    Directory.CreateDirectory(tv);
                }

                Directory.Move(season, newSeason);
            }

            log(newSeason);
            log(string.Empty);
        });

    Directory
        .EnumerateDirectories(directory)
        .SelectMany(Directory.EnumerateDirectories)
        .Select(season => Path.Combine(season, "Subs"))
        .ToArray()
        .ForEach(seasonSubtitleDirectory =>
        {
            log($"Delete {seasonSubtitleDirectory}");
            if (!isDryRun && Directory.Exists(seasonSubtitleDirectory))
            {
                DirectoryHelper.Delete(seasonSubtitleDirectory);
            }
        });

    Directory
        .EnumerateDirectories(directory)
        .SelectMany(Directory.EnumerateDirectories)
        .SelectMany(season => Directory.EnumerateFiles(season, PathHelper.AllSearchPattern))
        .Where(Video.IsSubtitle)
        .GroupBy(subtitle =>
        {
            string name = Path.GetFileNameWithoutExtension(subtitle);
            return name[..name.LastIndexOf(".", StringComparison.Ordinal)];
        })
        .ToArray()
        .ForEach(group =>
        {
            string[] subtitles = group.ToArray();
            string? englishSubtitle = group
                .Where(subtitle => Path.GetFileNameWithoutExtension(subtitle).ContainsIgnoreCase("_eng"))
                .OrderByDescending(subtitle => new FileInfo(subtitle).Length)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(englishSubtitle))
            {
                const string language = "eng";
                string englishSubtitleName = Path.GetFileNameWithoutExtension(englishSubtitle);
                string newEnglishSubtitle = Path.Combine(Path.GetDirectoryName(englishSubtitle)!, $"{englishSubtitleName.Substring(0, englishSubtitleName.LastIndexOf(".", StringComparison.Ordinal))}.{language}{Path.GetExtension(englishSubtitle)}");
                log($"Move {englishSubtitle}");
                if (!isDryRun)
                {
                    File.Move(englishSubtitle, newEnglishSubtitle);
                }

                log(newEnglishSubtitle);
                log(string.Empty);
            }

            string[] chineseSubtitles = group
                .Where(subtitle => Path.GetFileNameWithoutExtension(subtitle).ContainsIgnoreCase("_chi"))
                .ToArray();
            switch (chineseSubtitles.Length)
            {
                case 1:
                    {
                        string chineseSubtitle = chineseSubtitles.Single();

                        string language = EncodingHelper.TryRead(chineseSubtitle, out string? content, out _) && "為們說無當".Any(content.ContainsOrdinal) ? "cht" : "chs";
                        string chineseSubtitleName = Path.GetFileNameWithoutExtension(chineseSubtitle);
                        string newChineseSubtitle = Path.Combine(Path.GetDirectoryName(chineseSubtitle)!, $"{chineseSubtitleName.Substring(0, chineseSubtitleName.LastIndexOf(".", StringComparison.Ordinal))}.{language}{Path.GetExtension(chineseSubtitle)}");
                        log($"Move {chineseSubtitle}");
                        if (!isDryRun)
                        {
                            File.Move(chineseSubtitle, newChineseSubtitle);
                        }

                        log(newChineseSubtitle);
                        log(string.Empty);
                        break;
                    }
                case > 1:
                    {
                        string chineseSubtitle = chineseSubtitles
                            .Where(subtitle => EncodingHelper.TryRead(subtitle, out string? content, out _) && !"為們說無當".Any(content.ContainsOrdinal))
                            .OrderByDescending(subtitle => new FileInfo(subtitle).Length)
                            .First();
                        chineseSubtitles = new string[] { chineseSubtitle };

                        const string language = "chs";
                        string chineseSubtitleName = Path.GetFileNameWithoutExtension(chineseSubtitle);
                        string newChineseSubtitle = Path.Combine(Path.GetDirectoryName(chineseSubtitle)!, $"{chineseSubtitleName.Substring(0, chineseSubtitleName.LastIndexOf(".", StringComparison.Ordinal))}.{language}{Path.GetExtension(chineseSubtitle)}");
                        log($"Move {chineseSubtitle}");
                        if (!isDryRun)
                        {
                            File.Move(chineseSubtitle, newChineseSubtitle);
                        }

                        log(newChineseSubtitle);
                        log(string.Empty);
                        break;
                    }
            }

            subtitles.Except(string.IsNullOrWhiteSpace(englishSubtitle) ? chineseSubtitles : chineseSubtitles.Append(englishSubtitle))
                .ForEach(subtitle =>
                {
                    string newSubtitle = Path.Combine(subtitleBackupDirectory, Path.GetFileName(subtitle));
                    log($"Move {subtitle}");
                    if (!isDryRun)
                    {
                        File.Move(subtitle, newSubtitle);
                    }

                    log(newSubtitle);
                    log(string.Empty);
                });
        });

    Directory
        .GetFiles(directory, "*.eng.srt", SearchOption.AllDirectories)
        .ForEach(f => File.Move(f, f.ReplaceIgnoreCase(".eng.srt", ".srt"), false));
}

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
            log($"Seasons mismath: {string.Join(", ", seasonMismatches)}, {tv}");
            //return false;
        }

        seasonMismatches = metadataSeasonNames.Except(seasonNames).ToArray();
        if (seasonMismatches.Any())
        {
            log($"Seasons mismath: {string.Join(", ", seasonMismatches)}, {metadataTV}");
            //return false;
        }

        string[] episodeNumbers = seasons
            .SelectMany(season => Directory.EnumerateFiles(season))
            .Where(Video.IsVideo)
            .Select(episode => Regex.Match(Path.GetFileNameWithoutExtension(episode), @"\.(S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.").Groups[1].Value)
            .OrderBy(number => number)
            .ToArray();
        string[] metadataEpisodeNumbers = metadataSeasons
            .SelectMany(season => Directory.EnumerateFiles(season))
            .Where(Video.IsVideo)
            .Select(episode => Regex.Match(Path.GetFileNameWithoutExtension(episode), @"\.(S[0-9]{2}E[0-9]{2}(E[0-9]{2})?)\.").Groups[1].Value)
            .OrderBy(number => number)
            .ToArray();
        string[] episodeMismatches = episodeNumbers.Except(metadataEpisodeNumbers).ToArray();
        if (episodeMismatches.Any())
        {
            log($"Episode mismath: {string.Join(", ", episodeMismatches)}, {tv}");
            //return false;
        }

        episodeMismatches = metadataEpisodeNumbers.Except(episodeNumbers).ToArray();
        if (episodeMismatches.Any())
        {
            log($"Episode mismath: {string.Join(", ", episodeMismatches)}, {metadataTV}");
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

//MoveTVEpisodes(@"D:\Move\TV\RARBG", @"D:\Move\TV.Subtitle");

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

//RenameFilesWithDuplicateTitle(@"T:\Files\Library\TV Mainstream.主流电视剧\The Pacific.2010.太平洋战争[8.3-105K][TVMA][1080x]\Season 01",
//"-RARBG", isDryRun: false);

//Directory.EnumerateDirectories(@"H:\TV", "Season 08")
//    .SelectMany(Directory.EnumerateFiles)
//    .ToArray()
//    .ForEach(f =>
//    {
//        string number = Regex.Match(f, @"S[0-9]{2}E[0-9]{2}").Value;
//        string newFIle = Path.Combine(Path.GetDirectoryName(f), $"Homeland.{number}.1080p.WEBRip.x265-Vyndros{Path.GetExtension(f)}");
//        log(f);
//        File.Move(f, newFIle);
//        log(newFIle);
//        log("");
//    });


//const string invitationBase = "ll2cxx";
char[] numbers = Enumerable.Range(0, 10).Select(value => value.ToString().Single()).ToArray();
char[] letters = Enumerable.Range(0, 26).Select(value => (char)('a' + value)).ToArray();
//int[] numberIndexes = Enumerable.Range(0, invitationBase.Length + 1).ToArray();
//int[] letterIndexes = Enumerable.Range(0, invitationBase.Length + 2).ToArray();

//List<char>[]? invitationsWithNumber = numbers
//    .SelectMany(number => numberIndexes.Select(numberIndex =>
//     {
//         List<char> invitation = invitationBase.ToList();
//         invitation.Insert(numberIndex, number);

//         return invitation;
//     }))
//    .ToArray();

//HashSet<string> oldInvitations = new HashSet<string>(File.ReadAllLines(@"d:\invitations.txt"));

//using FileStream stream = File.OpenWrite(@"D:\invitations2.txt");
//using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
//invitationsWithNumber
//    .ForEach(invitationWIthNumber => letters.ForEach(letter => letterIndexes.ForEach(letterIndex =>
//    {
//        List<char> invitationWithLetter = invitationWIthNumber.ToList();
//        invitationWithLetter.Insert(letterIndex, letter);
//        string invitationWithLetterText = new string(invitationWithLetter.ToArray());
//        if (!oldInvitations.Contains(invitationWithLetterText))
//        {
//            streamWriter.WriteLine(invitationWithLetterText);
//        }
//    })));

//string[] invitations = File.ReadAllLines(@"d:\invitations2.txt");

//invitations.ForEach(invitation =>
//{
//    using WebClient client = new WebClient();
//    client.AddChromeHeaders();
//    string result = client.DownloadString($"https://www.sehuatang.net/forum.php?mod=ajax&inajax=yes&infloat=register&handlekey=register&ajaxmenu=1&action=checkinvitecode&invitecode={invitation}");
//    if (result == @"<?xml version=""1.0"" encoding=""utf-8""?>
//<root><![CDATA[抱歉，邀请码错误，请重新填写，没有邀请码不允许注册]]></root>")
//    {
//        Trace.WriteLine(invitation);
//    }
//    else
//    {
//        Trace.WriteLine($"!!{invitation}");
//        Debugger.Break();
//    }
//});

//var invitationBase = "ll2cxx".ToCharArray();
//numbers.ForEach(number => letters.ForEach(letter =>
//{
//    var invitation = invitationBase.ToList();
//    invitation.Add(letter);
//    invitation.Add(number);
//    string invitationText = new string(invitation.ToArray());

//    using WebClient client = new WebClient();
//    client.AddChromeHeaders();
//    string result = client.DownloadString($"https://www.sehuatang.net/forum.php?mod=ajax&inajax=yes&infloat=register&handlekey=register&ajaxmenu=1&action=checkinvitecode&invitecode={invitationText}");
//    if (result == @"<?xml version=""1.0"" encoding=""utf-8""?>
//<root><![CDATA[抱歉，邀请码错误，请重新填写，没有邀请码不允许注册]]></root>")
//    {
//        Trace.WriteLine(invitationText);
//    }
//    else
//    {
//        Trace.WriteLine($"!!{invitationText}");
//        Debugger.Break();
//        Environment.FailFast($"!!{invitationText}");
//    }
//}));

//Video.RenameFiles(@"T:\Files\Library\TV Mainstream.主流电视剧\",
//    (f, i) => f.ReplaceIgnoreCase(".Bluray", ".BluRay"));

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

//Directory.GetDirectories(@"G:\AV\New folder", "*", SearchOption.TopDirectoryOnly)
//    .Where(d=>Path.GetFileName(d).StartsWith("n"))
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
//        //string name = Regex.Match(f, @"Heyzo\-[0-9]+", RegexOptions.IgnoreCase).Value;
//        //FileHelper.Move(f, f.ReplaceIgnoreCase("_", "-"));
//        DirectoryHelper.AddPrefix(f,"TOKYOHOT-");
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

//Video.RenameFiles(@"D:\Move\TV\New folder",
//    (f, i) => PathHelper.ReplaceFileNameWithoutExtension(f,
//        n => Regex.Replace(n, @"\.[0-9]{4}\.BluRay\.1080p\.x265\.10bit\.MNHD\-FRDS", ".1080p.BluRay.x265-RARBG")
//            .ReplaceIgnoreCase(".1994.BluRay.1080p.x265.10bit.MNHD-FRDS", ".1080p.BluRay.x265-RARBG")),
//        //pattern:"*.avi",
//        isDryRun: false);

//Directory.GetDirectories(@"D:\Move\TV\New folder")
//    .ForEach(d=>DirectoryHelper.ReplaceDirectoryName(d, n=>n.ReplaceIgnoreCase(".BluRay.1080p.x265.10bit.MNHD-FRDS", ".1080p.BluRay.x265-RARBG")));

//Directory.GetDirectories(@"I:\AV\c0", "*", SearchOption.TopDirectoryOnly)
//    .SelectMany(Directory.GetFiles)
//    //.Where(f => Regex.IsMatch(Path.GetFileNameWithoutExtension(f), @"kb[0-9]{4}.+"))
//    .ToArray()
//    .ForEach((f, i) =>
//    {
//        log((i + 1).ToString());
//        log(f);
//        string newF = PathHelper.ReplaceFileNameWithoutExtension(f, Path.GetFileNameWithoutExtension(f).Replace("KB-", "kb"));
//        log(newF);
//        File.Move(f, newF);
//    });

//Directory.GetDirectories(@"H:\AV\Movie Japan 苍井空\AoiSola\蒼井そら（05-10）[www.liankong.net]原创首发")
//    .ForEach(d =>
//    {
//        Directory.GetFiles(d).ForEach(f =>
//        {
//            log(f);
//            string newF = PathHelper.ReplaceFileNameWithoutExtension(f, n => Path.GetFileName(d));
//            log(newF);
//            if (!string.Equals(f, newF))
//            {
//                File.Move(f, newF);
//            }

//            log("");
//        });
//    });

//Console.Read();

//var ids = Directory.GetDirectories(@"H:\AV\Movie Japan 苍井空\Movie Japan 蒼井そら\多人作品")
//    .Select(d =>
//    {
//        string id = Path.GetFileName(d);
//        return id.EndsWithIgnoreCase("-c") ? id[..^2] : id;
//    });

//Directory.GetFiles(@"H:\AV\Movie Japan 苍井空\Movie Japan 蒼井そら\New folder")
//    .Where(f=>ids.Any(id=>Path.GetFileNameWithoutExtension(f).StartsWithIgnoreCase(id)))
//    .ToArray()
//    .ForEach(f=>FileHelper.Move(f, f.Replace("New folder", "New folder 2")));

//Directory.GetDirectories(@"F:\AV\Movie Japan Uncensored2\New folder")
//    .ForEach(d => AddPrefix(d));


static void AddPrefix(string directory)
{
    if (!Regex.IsMatch(Path.GetFileName(directory), @"^[0-9\-_]+(-C)?$", RegexOptions.IgnoreCase))
    {
        return;
    }

    string metadata = Directory.GetFiles(directory, Video.XmlMetadataSearchPattern).FirstOrDefault(string.Empty);
    if (metadata.IsNullOrWhiteSpace())
    {
        return;
    }

    XElement? root = XDocument.Load(metadata).Root;
    string name = root?.Element("studio")?.Value ?? root?.Element("maker")?.Value ?? root?.Element("publisher")?.Value ?? string.Empty;
    string id = root?.Element("num")?.Value ?? string.Empty;
    string prefix = GetPrefix(name, id);
    if (prefix.IsNotNullOrWhiteSpace())
    {
        prefix = $"{prefix}-";
    }

    DirectoryHelper.AddPrefix(directory, prefix);
}

static string GetPrefix(string name, string id)
{
    return name switch
    {
        "パコパコママ" => "PACOPACOMAMA",
        "一本道" => "1PONDO",
        "加勒比" => id switch
        {
            _ when id.ContainsOrdinal("-") => "CARIBBEAN",
            _ when id.ContainsOrdinal("_") => "CARIBBEANPR",
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "The id is invalid")
        },
        "天然むすめ" => "10MUSUME",
        "muramura" => "MURAMURA",
        "东京热" => "TOKYOHOT",
        "Hey動画" => "Heydouga".ToUpperInvariant(),
        _ => string.Empty
    };
}

//Directory.GetFiles(@"D:\Move\TV\RARBG\Money.Heist", "*.mp4", SearchOption.AllDirectories)
//    .ForEach(f =>
//    {
//        string seasonEpisode = Regex.Match(f, @"S[0-9]+E[0-9]+").Value;
//        string sub = Directory.GetFiles(@"D:\Move\TV\New folder", $"*{seasonEpisode}*")
//            .OrderByDescending(subtitle => new FileInfo(subtitle).Length)
//            .First();
//        File.Move(sub, Path.Combine(Path.GetDirectoryName(f), Path.GetFileNameWithoutExtension(f)+".spa.srt"));
//    });