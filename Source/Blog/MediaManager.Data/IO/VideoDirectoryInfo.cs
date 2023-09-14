namespace Examples.IO;

using Examples.Common;
using System.Linq;

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
    private static readonly Regex NameRegex = new(@"^([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?((\=[^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?)?\.([0-9\-]{4})\.([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?\[([0-9]\.[0-9]|\-)-([0-9\.KM]+|\-)\]\[(\-|13\+|16\+|18\+|AO|Approved|E|G|GP|M|MPG|NA|NC17|NotRated|Passed|PG|PG13|R|T|TV13|TV14|TVG|TVMA|TVPG|TVY7|Unrated|X)\](\[(2160|1080|720|480)(b|b[2-9]|f|f[2-9]|h|h[2-9]|n|n[2-9]|p|p[2-9]|x|x[2-9]|X|X[2-9]|y|y[2-9])(\+)?\])?(\[3D\])?(\[HDR\])?$");

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
            value = Path.GetFileName(value);
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
            // FormatedDefinition: match.Groups[16].Value;
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

    internal static string GetResolution(VideoMovieFileInfo[] videos)
    {
        return videos.Select<IVideoFileInfo, DefinitionType>(video => video.DefinitionType).Max() switch
        {
            DefinitionType.P480 when videos.Any<IVideoFileInfo>(video => video.EncoderType is not EncoderType.P) => "480",
            DefinitionType.P720 => "720",
            DefinitionType.P1080 => "1080",
            DefinitionType.P2160 => "2160",
            _ => string.Empty
        };
    }

    internal static string GetResolution(VideoEpisodeFileInfo[] videos)
    {
        int total1080P = videos.Count<IVideoFileInfo>(video => video.DefinitionType is DefinitionType.P1080);
        int total720P = videos.Count<IVideoFileInfo>(video => video.DefinitionType is DefinitionType.P720);
        int total480PWithEncoder = videos.Count<IVideoFileInfo>(video => !video.IsHD && video.EncoderType is not EncoderType.P);
        int total480P = videos.Count<IVideoFileInfo>(video => !video.IsHD && video.EncoderType is EncoderType.P);
        int max = new int[] { total1080P, total720P, total480PWithEncoder, total480P }.Max();
        return max switch
        {
            _ when max == total1080P => "1080",
            _ when max == total720P => "720",
            _ when max == total480PWithEncoder => "480",
            _ => string.Empty
        };
    }

    internal static string GetSource(VideoMovieFileInfo[] videos)
    {
        IVideoFileInfo[] hdVideos = videos.Where<IVideoFileInfo>(video => video.IsHD).ToArray();
        if (hdVideos.Any())
        {
            IOrderedEnumerable<IGrouping<EncoderType, IVideoFileInfo>> videosByEncoder = hdVideos
                .GroupBy(video => video.EncoderType)
                .OrderByDescending(group => group.Key);
            IGrouping<EncoderType, IVideoFileInfo> group = videosByEncoder.First();
            string encoder = group.Key switch
            {
                EncoderType.X => "x",
                EncoderType.H => "h",
                EncoderType.XY => "X",
                EncoderType.Y => "y",
                EncoderType.F => "f",
                EncoderType.N => "n",
                EncoderType.B => "b",
                EncoderType.P => "p",
                _ => throw new ArgumentOutOfRangeException(nameof(videos))
            };

            return $"{encoder}{videos.Max<IVideoFileInfo, string>(video => video.FormattedAudioCount)}";
        }
        else
        {
            IOrderedEnumerable<IGrouping<EncoderType, IVideoFileInfo>> videosByEncoder = videos
                .GroupBy<IVideoFileInfo, EncoderType>(video => video.EncoderType)
                .OrderByDescending(group => group.Key);
            IGrouping<EncoderType, IVideoFileInfo> group = videosByEncoder.First();
            return group.Key switch
            {
                EncoderType.Y => $"y{videos.Max<IVideoFileInfo, string>(video => video.FormattedAudioCount)}",
                EncoderType.F => $"f{videos.Max<IVideoFileInfo, string>(video => video.FormattedAudioCount)}",
                EncoderType.N => $"n{videos.Max<IVideoFileInfo, string>(video => video.FormattedAudioCount)}",
                EncoderType.B => $"b{videos.Max<IVideoFileInfo, string>(video => video.FormattedAudioCount)}",
                EncoderType.P => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(videos))
            };
        }
    }

    internal static string GetSource(VideoEpisodeFileInfo[] videos)
    {
        IGrouping<EncoderType, IVideoFileInfo> maxGroup = videos
            .GroupBy<IVideoFileInfo, EncoderType>(video => video.EncoderType)
            .OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Key)
            .First();
        string encoder = maxGroup.Key switch
        {
            EncoderType.X => "x",
            EncoderType.H => "h",
            EncoderType.Y => "y",
            EncoderType.F => "f",
            EncoderType.N => "n",
            EncoderType.B => "b",
            _ when maxGroup.GroupBy(video => video.IsHD).OrderByDescending(group => group.Count()).First().Key => "p",
            _ => string.Empty
        };

        if (encoder.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        string maxAudio = videos
            .GroupBy<IVideoFileInfo, string>(video => video.FormattedAudioCount)
            .OrderByDescending(group => group.Count())
            .First()
            .Key;
        return $"{encoder}{maxAudio}";
    }
}