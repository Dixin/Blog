namespace Examples.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal static partial class Video
    {
        private static readonly string[] UncommonVideoExtensions = { ".avi", ".wmv", ".webm", ".mpg", ".mpeg", ".rmvb", ".rm", ".3gp", ".divx", ".m1v", ".mov", ".ts", ".vob", ".flv", ".m4v", ".mkv", ".dat" };

        private static readonly string[] CommonVideoExtensions = { ".avi", VideoExtension, ".mkv", ".dat" };

        private static readonly string[] AllVideoExtensions = UncommonVideoExtensions.Union(CommonVideoExtensions).ToArray();

        private static readonly string[] IndependentNfos = { $"tvshow{MetadataExtension}", $"season{MetadataExtension}" };

        private static readonly Regex MovieDirectoryRegex = new Regex(@"^[^\.]+\.([0-9]{4})\..+\[([0-9]\.[0-9]|\-)\](\[[0-9]{3,4}p\])?(\[3D\])?$");

        private static readonly Regex[] PreferredVersions = new string[] { @"[\. ]YIFY(\+HI)?$", @"[\. ]YIFY(\.[1-9]Audio)?$", @"\[YTS\.[A-Z]{2}\](\.[1-9]Audio)?$", @"\.GAZ$" }.Select(version => new Regex(version)).ToArray();

        private static readonly Regex[] TopVersions = new string[] { @"x265.+RARBG", @"x265.+VXT" }.Select(version => new Regex(version)).ToArray();

        private static readonly Regex[] PremiumVersions = new string[] { @"H264.+RARBG", @"H264.+VXT", @"x264.+RARBG", @"x264.+VXT" }.Select(version => new Regex(version)).ToArray();

        private const string MetadataExtension = ".nfo";

        private const string AllSearchPattern = "*";

        private const string VideoExtension = ".mp4";

        private const string MetadataSearchPattern = AllSearchPattern + MetadataExtension;

        private const string VideoSearchPattern = AllSearchPattern + VideoExtension;

        private static void TraceLog(string message) => Trace.WriteLine(message);

        private static string FilterTitleForFileSystem(this string value)
        {
            return value.Replace("?", "").Replace(": ", "-").Replace(":", "-").Replace("*", "_").Replace("/", "_");
        }

        internal static void BackupMetadata(string directory, string language = "eng")
        {
            Directory
                .GetFiles(directory, MetadataSearchPattern, SearchOption.AllDirectories)
                .ForEach(metadata => File.Copy(metadata, PathHelper.AddFilePostfix(metadata, $".{language}")));
        }

        internal static void RestoreMetadata(string directory, string language = "eng")
        {
            Directory
                .GetFiles(directory, MetadataSearchPattern, SearchOption.AllDirectories)
                .Where(nfo => nfo.EndsWith($".{language}{MetadataExtension}"))
                .Where(nfo => File.Exists(nfo.Replace($".{language}{MetadataExtension}", MetadataExtension)))
                .ForEach(nfo => FileHelper.Move(nfo, Path.Combine(Path.GetDirectoryName(nfo), (Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException(nfo)).Replace($".{language}", string.Empty) + Path.GetExtension(nfo)), true));
        }
    }
}
