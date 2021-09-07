namespace Examples.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal static partial class Video
    {
        private const string XmlMetadataExtension = ".nfo";

        private const string VideoExtension = ".mp4";

        private const string XmlMetadataSearchPattern = PathHelper.AllSearchPattern + XmlMetadataExtension;

        private const string VideoSearchPattern = PathHelper.AllSearchPattern + VideoExtension;

        private const string Featurettes = nameof(Featurettes);

        private const string DefaultBackupFlag = "backup";

        private const string Delimiter = ".";

        private static readonly string[] UncommonVideoExtensions = { ".avi", ".wmv", ".webm", ".mpg", ".mpeg", ".rmvb", ".rm", ".3gp", ".divx", ".m1v", ".mov", ".ts", ".vob", ".flv", ".m4v", ".mkv", ".dat" };

        private static readonly string[] CommonVideoExtensions = { ".avi", VideoExtension, ".mkv", ".iso" };

        private static readonly string[] AllVideoExtensions = UncommonVideoExtensions.Union(CommonVideoExtensions).ToArray();

        private static readonly string[] TVMetadataFiles = { $"tvshow{XmlMetadataExtension}", $"season{XmlMetadataExtension}" };

        private static readonly string[] Attachments = { "Introduction.txt", "Introduction.mht" };

        private static readonly string[] AdaptiveAttachments = new[] { "banner.jpg", "box.jpg", "clearart.png", "clearlogo.png", "disc.png", "discart.png", "fanart.jpg", "landscape.jpg", "logo.png", "poster.jpg", "poster.png", "backdrop.jpg", "back.jpg" };

        private const string ImdbMetadataExtension = ".json";

        internal const string ImdbCacheExtension = ".log";

        internal const string ImdbMetadataSearchPattern = PathHelper.AllSearchPattern + ImdbMetadataExtension;

        private const string ImdbCacheSearchPattern = PathHelper.AllSearchPattern + ImdbCacheExtension;

        private static readonly string[] SubtitleLanguages = { "can", "chs", "cht", "dan", "dut", "eng", "fin", "fre", "hin", "ger", "ita", "jap", "kor", "nor", "pol", "por", "rus", "spa", "swe", "tam" };

        internal const string NotExistingFlag = "-";

        private const string InstallmentSeparator = "`";

        private const string SubtitleSeparator = "-";
        
        private static void TraceLog(string? message) => Trace.WriteLine(message);

        private static readonly int IOMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4);

        internal static string FilterForFileSystem(this string value)
        {
            value = value.Replace(": ", SubtitleSeparator).Replace(":", SubtitleSeparator).Replace("*", "_").Replace("/", "_");
            Path.GetInvalidFileNameChars().ForEach(invalid => value = value.Replace(new string(invalid, 1), string.Empty));
            return value;
        }

        internal static void DeletePictures(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            EnumerateDirectories(directory, level)
                .ForEach(movie =>
                {
                    string[] files = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).Select(file => Path.GetFileName(file) ?? throw new InvalidOperationException(file)).ToArray();
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
    }
}
