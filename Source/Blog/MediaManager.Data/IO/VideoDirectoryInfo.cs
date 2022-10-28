namespace Examples.IO;

using Examples.Common;

internal record VideoDirectoryInfo(
    string DefaultTitle1, string DefaultTitle2, string DefaultTitle3,
    string OriginalTitle1, string OriginalTitle2, string OriginalTitle3,
    string Year,
    string TranslatedTitle1, string TranslatedTitle2, string TranslatedTitle3, string TranslatedTitle4,
    string AggregateRating, string AggregateRatingCount,
    string ContentRating,
    string Resolution, string Source,
    string Is3D, string Hdr)
{
    private static readonly Regex NameRegex = new(@"^([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?((\=[^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?)?\.([0-9\-]{4})\.([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?\[([0-9]\.[0-9]|\-)-([0-9\.KM]+|\-)\]\[(\-|R|PG|PG13|Unrated|NA|TVPG|NC17|GP|G|Approved|TVMA|Passed|TV14|TVG|X|E|MPG|M|AO|NotRated)\](\[(2160|1080|720|480)(b|b[2-9]|f|f[2-9]|h|h[2-9]|n|n[2-9]|p|p[2-9]|x|x[2-9]|y|y[2-9])(\+)?\])?(\[3D\])?(\[HDR\])?$");

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

    internal string Name => $"{this.DefaultTitle1}{this.DefaultTitle2}{this.DefaultTitle3}{this.OriginalTitle1}{this.OriginalTitle2}{this.OriginalTitle3}.{this.Year}.{this.TranslatedTitle1}{this.TranslatedTitle2}{this.TranslatedTitle3}{this.TranslatedTitle4}[{this.AggregateRating}-{this.AggregateRatingCount}][{this.ContentRating}]{this.FormattedDefinition}{this.Is3D}{this.Hdr}";

    internal static bool TryParse(string value, [NotNullWhen(true)] out VideoDirectoryInfo? info)
    {
        if (Path.IsPathRooted(value))
        {
            value = Path.GetFileName(value);
        }

        Match match = NameRegex.Match(value);
        if (!match.Success)
        {
            info = null;
            return false;
        }

        info = new VideoDirectoryInfo(
            DefaultTitle1: match.Groups[1].Value, DefaultTitle2: match.Groups[2].Value, DefaultTitle3: match.Groups[3].Value,
            OriginalTitle1: match.Groups[5].Value, OriginalTitle2: match.Groups[6].Value, OriginalTitle3: match.Groups[7].Value,
            Year: match.Groups[8].Value,
            TranslatedTitle1: match.Groups[9].Value, TranslatedTitle2: match.Groups[10].Value, TranslatedTitle3: match.Groups[11].Value, TranslatedTitle4: match.Groups[12].Value,
            AggregateRating: match.Groups[13].Value, AggregateRatingCount: match.Groups[14].Value,
            ContentRating: match.Groups[15].Value,
            // FormatedDefinition: match.Groups[16].Value;
            Resolution: match.Groups[17].Value, Source: match.Groups[18].Value,
            Is3D: match.Groups[19].Value, Hdr: match.Groups[21].Value);
        return true;
    }

    internal static VideoDirectoryInfo Parse(string value) =>
        TryParse(value, out VideoDirectoryInfo? info) ? info : throw new ArgumentOutOfRangeException(nameof(value), value, "The input is invalid");

    internal static IEnumerable<VideoFileInfo> GetVideos(string path) =>
        Directory
            .EnumerateFiles(path, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
            .Where(Video.IsVideo)
            .Select(VideoFileInfo.Parse);

    internal static string GetResolution(VideoFileInfo[] videos) => 
        videos switch
        {
            _ when videos.Any(video => video.Is2160P) => "2160",
            _ when videos.Any(video => video.Is1080P) => "1080",
            _ when videos.Any(video => video.Is720P) => "720",
            _ when videos.Any(video => video.Encoder.IsNotNullOrWhiteSpace()) => "480",
            _ => string.Empty
        };

    internal static string GetSource(VideoFileInfo[] videos)
    {
        VideoFileInfo[] hdVideos = videos.Where(video => video.IsHD).ToArray();
        if (hdVideos.Any())
        {
            VideoFileInfo[] xVideos = hdVideos.Where(video => video.IsX).ToArray();
            if (xVideos.Any())
            {
                return $"x{xVideos.Max(video => video.FormattedAudioCount)}";
            }

            VideoFileInfo[] hVideos = hdVideos.Where(video => video.IsH).ToArray();
            if (hVideos.Any())
            {
                return $"h{hVideos.Max(video => video.FormattedAudioCount)}";
            }

            VideoFileInfo[] yVideos = hdVideos.Where(video => video.IsY).ToArray();
            if (yVideos.Any())
            {
                return $"y{yVideos.Max(video => video.FormattedAudioCount)}";
            }

            return GetEncodedSource(hdVideos) ?? $"p{hdVideos.Max(video => video.FormattedAudioCount)}";
        }

        return GetEncodedSource(videos) ?? string.Empty;

        static string? GetEncodedSource(VideoFileInfo[] videos)
        {
            VideoFileInfo[] encodedVideos = videos.Where(video => video.Encoder.IsNotNullOrWhiteSpace()).ToArray();
            if (encodedVideos.Any())
            {
                Debug.Assert(encodedVideos.Length == 1 || encodedVideos.Distinct(video => video.Encoder).Count() == 1);
                string encoder = encodedVideos.First().Encoder.TrimStart('.')[..1];
                encoder = encoder is "h" ? "b" : encoder;
                Debug.Assert(encoder is "b" or "f" or "n");
                return $"{encoder}{encodedVideos.Max(video => video.FormattedAudioCount)}";
            }

            return null;
        }
    }
}