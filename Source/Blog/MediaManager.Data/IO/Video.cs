namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using MediaInfoLib;
using MediaManager.Net;

internal static partial class Video
{
    internal const int DefaultDirectoryLevel = 2;

    internal const string XmlMetadataExtension = ".nfo";

    internal const string VideoExtension = ".mp4";

    private const string ThumbExtension = ".jpg";

    internal const string XmlMetadataSearchPattern = $"{PathHelper.AllSearchPattern}{XmlMetadataExtension}";

    internal const string VideoSearchPattern = $"{PathHelper.AllSearchPattern}{VideoExtension}";

    private const string Featurettes = nameof(Featurettes);

    private const string DefaultBackupFlag = "backup";

    internal const string Delimiter = ".";

    private static readonly string[] UncommonVideoExtensions = [".3gp", ".dat", ".divx", ".flv", ".m1v", ".m2ts", ".m4v", ".mkv", ".mov", ".mpeg", ".mpg", ".rm", ".rmvb", ".ts", ".vob", ".webm", ".wmv"];

    private const string DiskImageExtension = ".iso";

    private static readonly string[] CommonVideoExtensions = [".avi", DiskImageExtension, ".mkv", ".mpg", VideoExtension, ".wmv"];

    private static readonly string[] AllVideoExtensions = UncommonVideoExtensions.Union(CommonVideoExtensions).ToArray();

    private static readonly string MovieMetadataFile = $"movie{XmlMetadataExtension}";
    private static readonly string TVShowMetadataFile = $"tvshow{XmlMetadataExtension}";

    private static readonly string TVSeasonMetadataFile = $"season{XmlMetadataExtension}";

    private static readonly string[] Attachments = ["Introduction.txt", "Introduction.mht"];

    private static readonly string[] AdaptiveAttachments = ["banner.jpg", "banner.png", "box.jpg", "clearart.png", "clearlogo.png", "disc.jpg", "disc.png", "discart.png", "fanart.jpg", "fanart.png", "landscape.jpg", "landscape.png", "logo.png", "logo.svg", "poster.jpg", "poster.png", "backdrop.jpg", "back.jpg"];

    internal const string ImdbCacheExtension = ".log";

    internal const string ImdbMetadataSearchPattern = $"{PathHelper.AllSearchPattern}{ImdbMetadata.Extension}";

    private const string ImdbCacheSearchPattern = $"{PathHelper.AllSearchPattern}{ImdbCacheExtension}";

    private static readonly string[] SubtitleLanguages = ["baq", "ben", "bho", "bok", "bul", "can", "cat", "chs", "cht", "cro", "cze", "dan", "dut", "eng", "est", "fil", "fin", "fre", "glg", "gre", "heb", "hin", "hrv", "ind", "ger", "hun", "ice", "ita", "jpn", "kan", "kor", "lat", "lav", "lit", "mac", "mal", "may", "nob", "nor", "pol", "por", "rum", "rus", "slo", "slv", "spa", "srp", "swe", "tam", "tel", "tha", "tur", "ukr", "vie"];

    internal const string NotExistingFlag = VersionSeparator;

    internal const string InstallmentSeparator = "`";

    internal const string TitleSeparator = VersionSeparator;

    internal const string VersionSeparator = "-";

    internal const string UpScaleDefinition = ".FAKE";

    internal static readonly int IOMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4);

    internal const int IODefaultRetryCount = 3;

    private const string SubtitleDirectory = "Subs";

    internal static string FilterForFileSystem(this string value)
    {
        value = value.Replace(": ", TitleSeparator).Replace(":", TitleSeparator).Replace("*", "_").Replace("/", "_");
        Path.GetInvalidFileNameChars().ForEach(invalidCharacter => value = value.Replace(new string(invalidCharacter, 1), string.Empty));
        return value;
    }

    internal static void DeletePictures(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string[] files = Directory.GetFiles(movie, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly).Select(PathHelper.GetFileName).ToArray();
                string[] videos = files.Where(IsCommonVideo).ToArray();
                string[] allowedAttachments = videos.Length == 1 || videos.All(video => Regex.IsMatch(video, "cd[1-9]", RegexOptions.IgnoreCase))
                    ? AdaptiveAttachments.ToArray()
                    : videos
                        .SelectMany(video => AdaptiveAttachments.Select(attachment => $"{PathHelper.GetFileNameWithoutExtension(video)}-{attachment}"))
                        .ToArray();
                allowedAttachments
                    .Select(attachment => Path.Combine(movie, attachment))
                    .Where(File.Exists)
                    .Do(log)
                    .Where(_ => !isDryRun)
                    .ForEach(FileHelper.Delete);
            });
    }

    internal static bool IsDolbyVision(string video)
    {
        using MediaInfo mediaInfo = new();
        mediaInfo.Open(video);
        mediaInfo.Option("Complete"); //or Option("Complete", "1") or Option("Info_Parameters")
        string hdrFormat = mediaInfo.Get(StreamKind.Video, 0, "HDR_Format");
        bool isDolbyVision = hdrFormat.ContainsIgnoreCase("Dolby Vision") || hdrFormat.ContainsIgnoreCase("DolbyVision");
#if DEBUG
        string inform = mediaInfo.Inform();
        Debug.Assert(isDolbyVision == inform.ContainsIgnoreCase("Dolby Vision") || inform.ContainsIgnoreCase("DolbyVision"));
#endif
        return isDolbyVision;
    }

    internal static bool IsAtmos(string video)
    {
        using MediaInfo mediaInfo = new();
        mediaInfo.Open(video);
        mediaInfo.Option("Complete");
        int audioCount = mediaInfo.Count_Get(StreamKind.Audio);
        bool isAtmos = Enumerable
            .Range(0, audioCount)
            .Select(index => mediaInfo.Get(StreamKind.Audio, 0, "Format_Commercial_IfAny"))
            .Any(audio => audio.ContainsIgnoreCase("Atmos"));
#if DEBUG
        string inform = mediaInfo.Inform();
        Debug.Assert(isAtmos == inform.ContainsIgnoreCase("Atmos"));
#endif
        return isAtmos;
    }
}