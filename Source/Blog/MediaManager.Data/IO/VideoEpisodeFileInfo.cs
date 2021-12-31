namespace Examples.IO;

using Examples.Common;

internal record VideoEpisodeFileInfo
{
    private static readonly Regex NameRegex = new(@"^(.+?)(\.([0-9]{4}))?(\.[A-Z]+?)?\.S([0-9]{2,4})E([0-9]{2,3})(E([0-9]{2,3}))?(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?(\.WEBRip|\.BluRay|\.DVDRip|\.HDRip|\.HDTV|\.VHSRip|\.LDRip|\.DVD|\.LDVDRip|\.LDDVDRip|\.HQDVDRip|\.TV|\.VCD|\.VCDRip)?(\.H264|\.x264|\.x265|\.DivX|\.Xvid)?(\.AAC|\.AC3|\.MP3|\.AAC5\.1|\.DTS)?(\-(RARBG|VXT|\[YTS\.(MX|AM|AG|LT)\]|[a-zA-Z0-9@]+?))?(\.[2-9]Audio)?(\.watermark)?(\.ffmpeg|\.nvenc|\.handbrake)?(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.fre|\.heb|\.jap|\.kor|\.pol|\.dut|\.spa|\.swe|\.por)?(\.(.+))?(\.mp4|\.avi)$");

    internal VideoEpisodeFileInfo(string name) => this.Name = name;

    internal string TVTitle { get; init; } = string.Empty;

    internal string Year { get; init; } = string.Empty;

    internal string Season { get; init; } = string.Empty;

    internal string Episode { get; init; } = string.Empty;

    internal string AdditionalEpisode { get; init; } = string.Empty;

    internal string Edition { get; init; } = string.Empty;

    internal string Definition { get; init; } = string.Empty;

    internal string Origin { get; init; } = string.Empty;

    internal string VideoCodec { get; init; } = string.Empty;

    internal string AudioCodec { get; init; } = string.Empty;

    internal string Version { get; init; } = string.Empty;

    internal string MultipleAudio { get; init; } = string.Empty;

    internal string Watermark { get; init; } = string.Empty;

    internal string Encoder { get; init; } = string.Empty;

    internal string Subtitle { get; init; } = string.Empty;

    internal string EpisodeTitle { get; init; } = string.Empty;

    internal string Extension { get; init; } = string.Empty;

    public override string ToString() => this.Name;

    internal string Name
    {
        get => $"{this.TVTitle}{(this.Year.IsNullOrWhiteSpace() ? string.Empty : $".{this.Year}")}.S{this.Season}E{this.Episode}{this.Definition}{this.Origin}{this.VideoCodec}{this.AudioCodec}{(this.Version.IsNullOrWhiteSpace() ? string.Empty : $"-{this.Version}")}{this.MultipleAudio}{this.Watermark}{this.Encoder}{this.Subtitle}{(this.EpisodeTitle.IsNullOrWhiteSpace() ? string.Empty : $".{this.EpisodeTitle}")}{this.Extension}";
        init
        {
            if (Path.IsPathRooted(value))
            {
                value = Path.GetFileName(value);
            }

            Match match = NameRegex.Match(value);
            if (!match.Success)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            this.TVTitle = match.Groups[1].Value;
            this.Year = match.Groups[3].Value;
            this.Edition = match.Groups[4].Value;
            this.Season = match.Groups[5].Value;
            this.Episode = match.Groups[6].Value;
            this.AdditionalEpisode = match.Groups[8].Value;
            this.Definition = match.Groups[9].Value;
            this.Origin = match.Groups[10].Value;
            this.VideoCodec = match.Groups[11].Value;
            this.AudioCodec = match.Groups[12].Value;
            this.Version = match.Groups[14].Value;
            this.MultipleAudio = match.Groups[16].Value;
            this.Watermark = match.Groups[17].Value;
            this.Encoder = match.Groups[18].Value;
            this.Subtitle = match.Groups[19].Value;
            this.EpisodeTitle = match.Groups[21].Value;
            this.Extension = match.Groups[22].Value;
        }
    }

    internal static bool TryParse(string name, [NotNullWhen(true)] out VideoEpisodeFileInfo? info)
    {
        try
        {
            info = new VideoEpisodeFileInfo(name);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            info = null;
            return false;
        }
    }

    internal bool IsX =>
        this.IsHD
        && (this.Version.EqualsOrdinal("RARBG")
            || this.Version.EqualsOrdinal("VXT"))
        && this.VideoCodec.EqualsOrdinal(".x265");

    internal bool IsH =>
        this.IsHD
        && (this.Version.EqualsOrdinal("RARBG")
            || this.Version.EqualsOrdinal("VXT"))
        && !this.VideoCodec.EqualsOrdinal(".x265");

    internal bool IsY =>
        this.IsHD
        && (this.Version.EqualsIgnoreCase("YIFY")
            || this.Version.StartsWithIgnoreCase("[YTS."));

    internal bool IsP =>
        this.IsHD
        && !this.Version.EqualsIgnoreCase("RARBG")
        && !this.Version.EqualsIgnoreCase("VXT")
        && !this.Version.EqualsIgnoreCase("YIFY")
        && !this.Version.StartsWithIgnoreCase("[YTS.");

    internal bool IsHD =>
        this.Definition is (".2160p" or ".1080p" or ".720p")
        && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is2160P => this.Definition is ".2160p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is1080P => this.Definition is ".1080p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is720P => this.Definition is ".720p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);
}