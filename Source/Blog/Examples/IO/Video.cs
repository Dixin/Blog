namespace Examples.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    internal static partial class Video
    {
        private const string MetadataExtension = ".nfo";

        private const string AllSearchPattern = "*";

        private const string VideoExtension = ".mp4";

        private const string MetadataSearchPattern = AllSearchPattern + MetadataExtension;

        private const string VideoSearchPattern = AllSearchPattern + VideoExtension;

        private const string Featurettes = nameof(Featurettes);

        private const string DefaultBackupFlag = "backup";

        private static readonly string[] UncommonVideoExtensions = { ".avi", ".wmv", ".webm", ".mpg", ".mpeg", ".rmvb", ".rm", ".3gp", ".divx", ".m1v", ".mov", ".ts", ".vob", ".flv", ".m4v", ".mkv", ".dat" };

        internal static readonly string[] CommonVideoExtensions = { ".avi", VideoExtension, ".mkv", ".iso" };

        private static readonly string[] AllVideoExtensions = UncommonVideoExtensions.Union(CommonVideoExtensions).ToArray();

        private static readonly string[] IndependentNfos = { $"tvshow{MetadataExtension}", $"season{MetadataExtension}" };

        private static readonly Regex[] PreferredVersions = new string[] { @"[\. ]YIFY(\+HI)?$", @"[\. ]YIFY(\.[1-9]Audio)?$", @"\[YTS\.[A-Z]{2}\](\.[1-9]Audio)?$", @"\.GAZ$" }.Select(version => new Regex(version)).ToArray();

        private static readonly Regex TopVersion = new Regex("x265.+(VXT|RARBG)");

        private static readonly Regex PremiumVersion = new Regex("[Hx]264.+(VXT|RARBG)");

        private static void TraceLog(string? message) => Trace.WriteLine(message);

        private static readonly int MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount, 4);

        internal static string FilterForFileSystem(this string value)
        {
            return value.Replace("?", "").Replace(": ", "-").Replace(":", "-").Replace("*", "_").Replace("/", "_");
        }

        internal static void BackupMetadata(string directory, string flag = DefaultBackupFlag)
        {
            Directory
                .GetFiles(directory, MetadataSearchPattern, SearchOption.AllDirectories)
                .ForEach(metadata => File.Copy(metadata, PathHelper.AddFilePostfix(metadata, $".{flag}")));
        }

        internal static void RestoreMetadata(string directory, string flag = DefaultBackupFlag)
        {
            Directory
                .GetFiles(directory, MetadataSearchPattern, SearchOption.AllDirectories)
                .Where(nfo => nfo.EndsWith($".{flag}{MetadataExtension}"))
                .Where(nfo => File.Exists(nfo.Replace($".{flag}{MetadataExtension}", MetadataExtension)))
                .ForEach(nfo => FileHelper.Move(nfo, Path.Combine(Path.GetDirectoryName(nfo) ?? throw new InvalidOperationException(nfo), (Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException(nfo)).Replace($".{flag}", string.Empty) + Path.GetExtension(nfo)), true));
        }

        internal static void DeletePictures(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    string[] files = Directory.GetFiles(movie, AllSearchPattern, SearchOption.TopDirectoryOnly).Select(file=>Path.GetFileName(file) ?? throw new InvalidOperationException(file)).ToArray();
                    string[] videos = files.Where(IsCommonVideo).ToArray();
                    string[] allowedAttachments = videos.Length == 1 || videos.All(video => Regex.IsMatch(video, "cd[1-9]", RegexOptions.IgnoreCase))
                        ? AdaptiveAttachments.ToArray()
                        : videos
                            .SelectMany(video => AdaptiveAttachments.Select(attachment => $"{Path.GetFileNameWithoutExtension(video)}-{attachment}"))
                            .ToArray();
                    allowedAttachments
                        .Select(attachment => Path.Combine(movie, attachment))
                        .Where(File.Exists)
                        .Do(log)
                        .Where(attachment => !isDryRun)
                        .ForEach(File.Delete);
                });
        }

        internal static void CleanFeaturettes(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    string featurettes = Path.Combine(movie, Featurettes);
                    if (Directory.Exists(featurettes))
                    {
                        string[] metadataFiles = Directory.GetFiles(featurettes, MetadataSearchPattern, SearchOption.AllDirectories);
                        metadataFiles
                            .Do(log)
                            .Where(metadataFile => !isDryRun)
                            .ForEach(File.Delete);
                    }
                });
        }

        internal static void CreateEpisodeMetadata(string directory, Func<string, string> getTitle, Func<string, int, string> getEpisode, Func<string, string>? getSeason = null, bool overwrite = false)
        {
            Directory.GetFiles(directory, VideoSearchPattern, SearchOption.TopDirectoryOnly)
                .OrderBy(video => video)
                .ForEach((video, index) =>
                {
                    string metadataPath = PathHelper.ReplaceExtension(video, MetadataExtension);
                    if (!overwrite && File.Exists(metadataPath))
                    {
                        return;
                    }

                    XDocument metadata = XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<episodedetails>
  <plot />
  <outline />
  <lockdata>false</lockdata>
  <title></title>
  <episode></episode>
  <season></season>
</episodedetails>");
                    metadata.Root!.Element("title"!)!.Value = getTitle(video);
                    metadata.Root!.Element("episode"!)!.Value = getEpisode(video, index);
                    metadata.Root!.Element("season"!)!.Value = getSeason?.Invoke(video) ?? "1";
                    metadata.Save(metadataPath);
                });
        }

        internal static void CreateEpisodeMetadata(string directory, bool overwrite = false)
        {
            Directory
                .EnumerateDirectories(directory)
                .OrderBy(season=>season)
                .ForEach(season=> CreateEpisodeMetadata(
                    season,
                    video =>
                    {
                        video = Path.GetFileNameWithoutExtension(video);
                        string seasonEpisode = Regex.Match(video, @"\.S[0-9]+E[0-9]+\.").Value;
                        return video.Substring(video.IndexOf(seasonEpisode, StringComparison.InvariantCulture) + seasonEpisode.Length).Replace("720p.", "").Replace("1080p.", "");
                    },
                    (video, index) =>
                    {
                        video = Path.GetFileNameWithoutExtension(video);
                        string seasonEpisode = Regex.Match(video, @"\.S[0-9]+E[0-9]+\.").Value.Trim('.');
                        return seasonEpisode.Substring(seasonEpisode.IndexOf("E", StringComparison.InvariantCulture) + "E".Length).TrimStart('0');
                    },
                    video =>
                    {
                        video = Path.GetFileNameWithoutExtension(video);
                        string seasonEpisode = Regex.Match(video, @"\.S[0-9]+E[0-9]+\.").Value.Trim('.');
                        return seasonEpisode.Substring(seasonEpisode.IndexOf("S", StringComparison.InvariantCulture) + "S".Length).Substring(0, seasonEpisode.IndexOf("E", StringComparison.InvariantCulture) - 1).TrimStart('0');
                    },
                    overwrite));
        }
    }
}
