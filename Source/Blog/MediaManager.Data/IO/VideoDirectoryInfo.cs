namespace MediaManager.IO;

using System.Linq;
using Examples.Common;
using Examples.IO;

internal record VideoDirectoryInfo(
    string DefaultTitle1, string DefaultTitle2, string DefaultTitle3,
    string OriginalTitle1, string OriginalTitle2, string OriginalTitle3,
    string Year,
    string TranslatedTitle1, string TranslatedTitle2, string TranslatedTitle3, string TranslatedTitle4,
    string AggregateRating, string AggregateRatingCount,
    string ContentRating,
    string Resolution, string Source,
    string Is3D, string Hdr) : INaming, ISimpleParsable<VideoDirectoryInfo>
{
    private static readonly Regex NameRegex = new(string.Join(
        string.Empty,
        [
            @"^",
            @"([^\.^\-^\=]+)",
            @"(\-[^\.^\-^\=]+)?",
            @"(\-[^\.^\-^\=]+)?",
            @"((\=[^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?)?",
            @"\.",
            @"([0-9\-]{4})",
            @"\.",
            @"([^\.^\-^\=]+)",
            @"(\-[^\.^\-^\=]+)?",
            @"(\-[^\.^\-^\=]+)?",
            @"(\-[^\.^\-^\=]+)?",
            @"\[([0-9]\.[0-9]|\-)-([0-9\.KM]+|\-)\]",
            @"\[(\-|13\+|16\+|18|18\+|AO|Approved|C|E|G|GP|M|MA17|MPG|NA|NC17|NotRated|Passed|PG|PG13|R|T|TV13|TV14|TVG|TVMA|TVPG|TVY|TVY7|TVY7FV|Unrated|X)\]",
            @"(\[(2160|1080|720|480)(B|B[2-9]|F|F[2-9]|H|H[2-9]|K|K[2-9]|N|N[2-9]|P|P[2-9]|X|X[2-9]|Y|Y[2-9]|Z|Z[2-9]|b|b[2-9]|f|f[2-9]|h|h[2-9]|k|k[2-9]|n|n[2-9]|p|p[2-9]|x|x[2-9]|y|y[2-9]|z|z[2-9])(\+)?\])?",
            @"(\[3D\])?",
            @"(\[HDR\])?",
            @"$"
        ]));

    internal string FormattedDefinition
    {
        get
        {
            Debug.Assert(this.Source.IsNullOrWhiteSpace() == this.Resolution.IsNullOrWhiteSpace());
            return this.Source.IsNotNullOrWhiteSpace() ? $"[{this.Resolution}{this.Source}]" : string.Empty;
        }
    }

    internal bool Is2160P => this.Resolution is "2160";

    internal bool Is1080P => this.Resolution is "1080";

    internal bool Is720P => this.Resolution is "720";

    internal bool IsHD => this.Resolution is "2160" or "1080" or "720";

    public override string ToString() => this.Name;

    public string Name => $"{this.DefaultTitle1}{this.DefaultTitle2}{this.DefaultTitle3}{this.OriginalTitle1}{this.OriginalTitle2}{this.OriginalTitle3}.{this.Year}.{this.TranslatedTitle1}{this.TranslatedTitle2}{this.TranslatedTitle3}{this.TranslatedTitle4}[{this.AggregateRating}-{this.AggregateRatingCount}][{this.ContentRating}]{this.FormattedDefinition}{this.Is3D}{this.Hdr}";

    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out VideoDirectoryInfo? result)
    {
        if (Path.IsPathRooted(value))
        {
            value = PathHelper.GetFileName(value);
        }

        if (value.IsNullOrWhiteSpace())
        {
            result = null;
            return false;
        }

        Match match = NameRegex.Match(value);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        result = new VideoDirectoryInfo(
            DefaultTitle1: match.Groups[1].Value, DefaultTitle2: match.Groups[2].Value, DefaultTitle3: match.Groups[3].Value,
            OriginalTitle1: match.Groups[5].Value, OriginalTitle2: match.Groups[6].Value, OriginalTitle3: match.Groups[7].Value,
            Year: match.Groups[8].Value,
            TranslatedTitle1: match.Groups[9].Value, TranslatedTitle2: match.Groups[10].Value, TranslatedTitle3: match.Groups[11].Value, TranslatedTitle4: match.Groups[12].Value,
            AggregateRating: match.Groups[13].Value, AggregateRatingCount: match.Groups[14].Value,
            ContentRating: match.Groups[15].Value,
            // FormattedDefinition: match.Groups[16].Value;
            Resolution: match.Groups[17].Value, Source: match.Groups[18].Value,
            Is3D: match.Groups[20].Value, Hdr: match.Groups[21].Value);
        return true;
    }

    public static VideoDirectoryInfo Parse(string value) =>
        TryParse(value, out VideoDirectoryInfo? result) ? result : throw new ArgumentOutOfRangeException(nameof(value), value, "The input is invalid");

    internal static IEnumerable<VideoMovieFileInfo> GetVideos(string path) =>
        Directory
            .EnumerateFiles(path, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
            .Where(Video.IsVideo)
            .Select(VideoMovieFileInfo.Parse);

    internal static IEnumerable<VideoEpisodeFileInfo> GetEpisodes(string path) =>
        Directory
            .EnumerateDirectories(path, "Season *", SearchOption.TopDirectoryOnly)
            .SelectMany(season => Directory.EnumerateFiles(season, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly))
            .Where(Video.IsVideo)
            .Select(VideoEpisodeFileInfo.Parse);

    internal static string GetResolution(VideoMovieFileInfo[] videos, ISettings settings)
    {
        return videos.Select(video => video.GetDefinitionType()).Max() switch
        {
            DefinitionType.P480 when videos.Any(video => video.GetEncoderType() is not (EncoderType.HD or EncoderType.HDBluRay)) => "480",
            DefinitionType.P720 => "720",
            DefinitionType.P1080 => "1080",
            DefinitionType.P2160 => "2160",
            _ => string.Empty
        };
    }

    internal static string GetResolution(VideoEpisodeFileInfo[] videos, ISettings settings)
    {
        int total2160P = videos.Count(video => video.GetDefinitionType() is DefinitionType.P2160);
        int total1080P = videos.Count(video => video.GetDefinitionType() is DefinitionType.P1080);
        int total720P = videos.Count(video => video.GetDefinitionType() is DefinitionType.P720);
        int total480PWithEncoder = videos.Count(video => !video.IsHD() && video.GetEncoderType() is not EncoderType.HD);
        int total480P = videos.Count(video => !video.IsHD() && video.GetEncoderType() is EncoderType.HD);
        int max = new int[] { total2160P, total1080P, total720P, total480PWithEncoder, total480P }.Max();
        return max switch
        {
            _ when max == total2160P => "2160",
            _ when max == total1080P => "1080",
            _ when max == total720P => "720",
            _ when max == total480PWithEncoder => "480",
            _ => string.Empty
        };
    }

    internal static string GetSource(VideoMovieFileInfo[] videos, ISettings settings)
    {
        VideoMovieFileInfo[] hdVideos = videos.Where(video => video.IsHD()).ToArray();
        if (hdVideos.Any())
        {
            IOrderedEnumerable<IGrouping<EncoderType, VideoMovieFileInfo>> videosByEncoder = hdVideos
                .GroupBy(video => video.GetEncoderType())
                .OrderByDescending(group => group.Key);
            IGrouping<EncoderType, VideoMovieFileInfo> group = videosByEncoder.First();
            string encoder = group.Key switch
            {
                EncoderType.TopX265BluRay => "x",
                EncoderType.TopX265 => "X",
                EncoderType.TopH264BluRay => "h",
                EncoderType.TopH264 => "H",
                EncoderType.PreferredX265BluRay => "z",
                EncoderType.PreferredX265 => "Z",
                EncoderType.PreferredH264BluRay => "y",
                EncoderType.PreferredH264 => "Y",
                EncoderType.FfmpegX265BluRay => "f",
                EncoderType.FfmpegX265 => "F",
                EncoderType.NvidiaX265BluRay => "n",
                EncoderType.NvidiaX265 => "N",
                EncoderType.HandbrakeH264BluRay => "b",
                EncoderType.HandbrakeH264 => "B",
                EncoderType.KoreanPremium => "k",
                EncoderType.Korean => "K",
                EncoderType.HDBluRay => "p",
                EncoderType.HD => "P",
                _ => throw new ArgumentOutOfRangeException(nameof(videos))
            };

            return $"{encoder}{videos.Max(video => video.FormatAudioCount())}";
        }
        else
        {
            IOrderedEnumerable<IGrouping<EncoderType, VideoMovieFileInfo>> videosByEncoder = videos
                .GroupBy(video => video.GetEncoderType())
                .OrderByDescending(group => group.Key);
            IGrouping<EncoderType, VideoMovieFileInfo> group = videosByEncoder.First();
            return group.Key switch
            {
                EncoderType.PreferredH264BluRay => $"y{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.PreferredH264 => $"Y{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.FfmpegX265BluRay => $"f{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.FfmpegX265 => $"F{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.NvidiaX265BluRay => $"n{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.NvidiaX265 => $"N{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.HandbrakeH264BluRay => $"b{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.HandbrakeH264 => $"B{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.KoreanPremium => $"k{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.Korean => $"K{videos.Max(video => video.FormatAudioCount())}",
                EncoderType.HDBluRay => string.Empty,
                EncoderType.HD => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(videos))
            };
        }
    }

    internal static string GetSource(VideoEpisodeFileInfo[] videos, ISettings settings)
    {
        IGrouping<EncoderType, VideoEpisodeFileInfo> maxGroup = videos
            .GroupBy(video => video.GetEncoderType())
            .OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Key)
            .First();
        string encoder = maxGroup.Key switch
        {
            EncoderType.TopX265BluRay => "x",
            EncoderType.TopX265 => "X",
            EncoderType.TopH264BluRay => "h",
            EncoderType.TopH264 => "H",
            EncoderType.PreferredH264BluRay => "y",
            EncoderType.PreferredH264 => "Y",
            EncoderType.FfmpegX265BluRay => "f",
            EncoderType.FfmpegX265 => "F",
            EncoderType.NvidiaX265BluRay => "n",
            EncoderType.NvidiaX265 => "N",
            EncoderType.HandbrakeH264BluRay => "b",
            EncoderType.HandbrakeH264 => "B",
            EncoderType.HDBluRay when maxGroup.GroupBy(video => video.IsHD()).OrderByDescending(group => group.Count()).First().Key => "p",
            EncoderType.HD when maxGroup.GroupBy(video => video.IsHD()).OrderByDescending(group => group.Count()).First().Key => "P",
            _ => string.Empty
        };

        if (encoder.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        string maxAudio = videos
            .GroupBy(video => video.FormatAudioCount())
            .OrderByDescending(group => group.Count())
            .First()
            .Key;
        return $"{encoder}{maxAudio}";
    }
}