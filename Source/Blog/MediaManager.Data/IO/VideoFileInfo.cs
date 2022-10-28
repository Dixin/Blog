namespace Examples.IO;

using Examples.Common;

internal record VideoFileInfo(
    string Title, string Year,
    string ThreeD, string Edition,
    string Definition, string AdditionalEdition,
    string Origin, string VideoCodec, string AudioCodec,
    string Version,
    string MultipleAudio,
    string Watermark,
    string Encoder,
    string Subtitle,
    string Part,
    string Extension)
{
    private static readonly Regex NameRegex = new(@"^(.+?)\.([0-9\-]{4})(\.3D(\.HSBS)?)?((\.Part[1-9])?(\.[A-Z\.\-]+?)?(\.Part[1-9])?|\.RE\-EDIT|\.S[0-9]{2}E[0-9]{2}\.[a-zA-Z\.]+?)?(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?(\.BOOTLEG|\.US)?(\.WEBRip|\.BluRay|\.DVDRip|\.HDRip|\.HDTV|\.VHSRip|\.LDRip|\.DVD|\.LDVDRip|\.LDDVDRip|\.HQDVDRip|\.TV|\.VCD|\.VCDRip|\.TS)?(\.H264|\.x264|\.x265|\.DivX|\.Xvid)?(\.AAC|\.AC3|\.MP3|\.AAC5\.1|\.DTS|\.DDP)?(\-(RARBG|VXT|\[YTS\.(MX|AM|AG|LT)\]|[a-zA-Z0-9@]+?))?(\.[2-9]Audio)?(\.watermark)?(\.ffmpeg|\.nvenc|\.handbrake)?(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.fre|\.heb|\.jap|\.kor|\.pol|\.dut|\.spa|\.swe|\.por)?(\.cd[0-9]{1,2})?(\.mp4|\.avi|\.iso|\.mkv|\.mpg)?$");

    public override string ToString() => this.Name;

    internal string Name => $"{this.Title}.{this.Year}{this.ThreeD}{this.Edition}{this.Definition}{this.AdditionalEdition}{this.Origin}{this.VideoCodec}{this.AudioCodec}{(this.Version.IsNullOrWhiteSpace() ? string.Empty : $"-{this.Version}")}{this.MultipleAudio}{this.Watermark}{this.Encoder}{this.Subtitle}{this.Part}{this.Extension}";

    internal static bool TryParse(string value, [NotNullWhen(true)] out VideoFileInfo? info)
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

        info = new(
            Title: match.Groups[1].Value, Year: match.Groups[2].Value,
            ThreeD: match.Groups[3].Value, Edition: match.Groups[5].Value,
            Definition: match.Groups[9].Value, AdditionalEdition: match.Groups[10].Value,
            Origin: match.Groups[11].Value, VideoCodec: match.Groups[12].Value, AudioCodec: match.Groups[13].Value,
            Version: match.Groups[15].Value,
            MultipleAudio: match.Groups[17].Value,
            Watermark: match.Groups[18].Value,
            Encoder: match.Groups[19].Value,
            Subtitle: match.Groups[20].Value,
            Part: match.Groups[21].Value,
            Extension: match.Groups[22].Value);
        return true;
    }

    internal static VideoFileInfo Parse(string value) =>
        TryParse(value, out VideoFileInfo? info) ? info : throw new ArgumentOutOfRangeException(nameof(value), value, "Input is invalid.");

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

    internal bool IsF =>
        this.Encoder.IsNotNullOrWhiteSpace();

    internal bool IsP =>
        this.IsHD
        && !this.Version.EqualsIgnoreCase("RARBG")
        && !this.Version.EqualsIgnoreCase("VXT")
        && !this.Version.EqualsIgnoreCase("YIFY")
        && !this.Version.StartsWithIgnoreCase("[YTS.")
        && this.Encoder.IsNullOrWhiteSpace();

    internal bool IsHD =>
        this.Definition is (".2160p" or ".1080p" or ".720p")
        && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is2160P => this.Definition is ".2160p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is1080P => this.Definition is ".1080p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is720P => this.Definition is ".720p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);

    internal bool Is480P => !this.IsHD;

    internal string FormattedAudioCount => this.MultipleAudio.IsNullOrWhiteSpace() ? string.Empty : Regex.Match(this.MultipleAudio, @"\.([2-9])Audio").Groups[1].Value;

    internal static bool IsXOrH(string nameWithoutExtension) => Regex.IsMatch(nameWithoutExtension, @"\-(RARBG|VXT)$");
}