namespace MediaManager.IO;

using Examples.IO;

internal static partial class Video
{
    internal const int DefaultDirectoryLevel = 2;

    internal const string XmlMetadataExtension = ".nfo";

    internal const string VideoExtension = ".mp4";

    private const string ThumbExtension = ".jpg";

    internal const string XmlMetadataSearchPattern = PathHelper.AllSearchPattern + XmlMetadataExtension;

    private const string VideoSearchPattern = PathHelper.AllSearchPattern + VideoExtension;

    private const string Featurettes = nameof(Featurettes);

    private const string DefaultBackupFlag = "backup";

    private const string Delimiter = ".";

    private static readonly string[] UncommonVideoExtensions = [".3gp", ".dat", ".divx", ".flv", ".m1v", ".m2ts", ".m4v", ".mkv", ".mov", ".mpeg", ".mpg", ".rm", ".rmvb", ".ts", ".vob", ".webm", ".wmv"];

    private const string DiskImageExtension = ".iso";

    private static readonly string[] CommonVideoExtensions = [".avi", DiskImageExtension, ".mkv", ".mpg", VideoExtension, ".wmv"];

    private static readonly string[] AllVideoExtensions = UncommonVideoExtensions.Union(CommonVideoExtensions).ToArray();

    private static readonly string TVShowMetadataFile = $"tvshow{XmlMetadataExtension}";

    private static readonly string TVSeasonMetadataFile = $"season{XmlMetadataExtension}";

    private static readonly string[] Attachments = ["Introduction.txt", "Introduction.mht"];

    private static readonly string[] AdaptiveAttachments = ["banner.jpg", "box.jpg", "clearart.png", "clearlogo.png", "disc.jpg", "disc.png", "discart.png", "fanart.jpg", "landscape.jpg", "landscape.png", "logo.png", "logo.svg", "poster.jpg", "poster.png", "backdrop.jpg", "back.jpg"];

    internal const string ImdbMetadataExtension = ".json";

    internal const string ImdbCacheExtension = ".log";

    internal const string ImdbMetadataSearchPattern = PathHelper.AllSearchPattern + ImdbMetadataExtension;

    private const string ImdbCacheSearchPattern = PathHelper.AllSearchPattern + ImdbCacheExtension;

    private static readonly string[] SubtitleLanguages = ["bul", "can", "cat", "chs", "cht", "cze", "dan", "dut", "eng", "fil", "fin", "fre", "gre", "heb", "hin", "ind", "ger", "hun", "ita", "jap", "kor", "mal", "nor", "pol", "por", "rom", "rum", "rus", "slo", "spa", "swe", "tam", "tha", "tur", "ukr"];

    internal const string NotExistingFlag = "-";

    private const string InstallmentSeparator = "`";

    internal const string SubtitleSeparator = "-";

    internal const string VersionSeparator = "-";

    internal const string UpScaleDefinition = ".FAKE";

    internal static readonly int IOMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4);

    internal static string FilterForFileSystem(this string value)
    {
        value = value.Replace(": ", SubtitleSeparator).Replace(":", SubtitleSeparator).Replace("*", "_").Replace("/", "_");
        Path.GetInvalidFileNameChars().ForEach(invalid => value = value.Replace(new string(invalid, 1), string.Empty));
        return value;
    }

    internal static void DeletePictures(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
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
                    .Where(_ => !isDryRun)
                    .ForEach(FileHelper.Delete);
            });
    }
}