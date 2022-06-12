namespace Examples.IO;

internal record VideoDirectoryInfo(
    string DefaultTitle1, string DefaultTitle2, string DefaultTitle3,
    string OriginalTitle1, string OriginalTitle2, string OriginalTitle3,
    string Year,
    string TranslatedTitle1, string TranslatedTitle2, string TranslatedTitle3, string TranslatedTitle4,
    string AggregateRating, string AggregateRatingCount,
    string ContentRating,
    string Resolution, string Source,
    string Is3D, string IsHdr)
{
    private static readonly Regex NameRegex = new(@"^([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?((\=[^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?)?\.([0-9\-]{4})\.([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?\[([0-9]\.[0-9]|\-)-([0-9\.KM]+|\-)\]\[(\-|R|PG|PG13|Unrated|NA|TVPG|NC17|GP|G|Approved|TVMA|Passed|TV14|TVG|X|E|MPG|M|AO|NotRated)\](\[(2160|1080|720)(p|y|h|x)\])?(\[3D\])?(\[HDR\])?$");

    internal string FormattedDefinition => this.IsHD ? $"[{this.Resolution}{this.Source}]" : string.Empty;

    internal bool Is2160P => this.Resolution is "2160";

    internal bool Is1080P => this.Resolution is "1080";

    internal bool Is720P => this.Resolution is "720";

    internal bool IsHD => this.Resolution is "2160" or "1080" or "720";

    public override string ToString() => this.Name;

    internal string Name => $"{this.DefaultTitle1}{this.DefaultTitle2}{this.DefaultTitle3}{this.OriginalTitle1}{this.OriginalTitle2}{this.OriginalTitle3}.{this.Year}.{this.TranslatedTitle1}{this.TranslatedTitle2}{this.TranslatedTitle3}{this.TranslatedTitle4}[{this.AggregateRating}-{this.AggregateRatingCount}][{this.ContentRating}]{this.FormattedDefinition}{this.Is3D}{this.IsHdr}";

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
            Is3D: match.Groups[19].Value, IsHdr: match.Groups[20].Value);
        return true;
    }

    internal static VideoDirectoryInfo Parse(string value) => 
        TryParse(value, out VideoDirectoryInfo? info) ? info : throw new ArgumentOutOfRangeException(nameof(value), value, "The input is invalid");

    internal static string GetSource(string path) =>
        GetSource(Directory
            .EnumerateFiles(path, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
            .Where(Video.IsVideo)
            .Select(VideoFileInfo.Parse)
            .ToArray());

    internal static string GetSource(VideoFileInfo[] videos)
    {
        videos = videos.Where(video => video.IsHD).ToArray();
        if (videos.IsEmpty())
        {
            return string.Empty;
        }

        if (videos.Any(video => video.IsX))
        {
            return "x";
        }

        if (videos.Any(video => video.IsH))
        {
            return "h";
        }

        if (videos.Any(video => video.IsY))
        {
            return "y";
        }

        return "p";
    }
}