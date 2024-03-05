namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;

internal record VideoMovieFileInfo(
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
    string Extension) : IVideoFileInfo, ISimpleParsable<VideoMovieFileInfo>, IDefaultSettings
{
    private static ISettings? settings;

    private static Regex? nameRegex;

    public override string ToString() => this.Name;

    public string Name => $"{this.Title}.{this.Year}{this.ThreeD}{this.Edition}{this.Definition}{this.AdditionalEdition}{this.Origin}{this.VideoCodec}{this.AudioCodec}{(this.Version.IsNullOrWhiteSpace() ? string.Empty : $"{Video.VersionSeparator}{this.Version}")}{this.MultipleAudio}{this.Watermark}{this.Encoder}{this.Subtitle}{this.Part}{this.Extension}";

    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out VideoMovieFileInfo? result)
    {
        if (value.IsNullOrWhiteSpace())
        {
            result = null;
            return false;
        }

        if (Path.IsPathRooted(value))
        {
            value = PathHelper.GetFileName(value);
        }

        nameRegex ??= new Regex(@$"^(.+?)\.([0-9\-]{{4}})(\.3D(\.HSBS)?)?((\.Part[1-9])?(\.[A-Z\.\-]+?)?(\.Part[\.]{{0,1}}[1-9]|\.PART[\.]{{0,1}}[1-9])?|\.RE\-EDIT|\.88F|\.S[0-9]{{2}}E[0-9]{{2}}\.[a-zA-Z\.]+?)?(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?(\.BOOTLEG|\.US|\.RESTORED)?(\.WEBRip|\.BluRay|\.DVDRip|\.HDRip|\.HDTV|\.VHSRip|\.LDRip|\.DVD|\.LDVDRip|\.LDDVDRip|\.HQDVDRip|\.TV|\.VCD|\.VCDRip|\.TS)?(\.H264|\.x264|\.x265|\.x265\.10bit|\.DivX|\.Xvid)?(\.AAC|\.AAC5\.1|\.AC3|\.DD|\.DDP|\.DTS|\.MP3)?(\{Video.VersionSeparator}({Settings.TopEnglishKeyword}|{Settings.TopForeignKeyword}|\[{Settings.PreferredNewKeyword}\.(AG|AM|LT|ME|MX)\]|[a-zA-Z0-9@]+?))?(\.[2-9]Audio)?(\.watermark)?(\.{FfmpegHelper.Executable}|\.nvenc|\.handbrake)?(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.eng&cht|\.fre|\.heb|\.jap|\.kor|\.pol|\.por|\.rus|\.spa|\.swe)?(\.cd[0-9]{{1,2}})?(\.mp4|\.avi|\.iso|\.m2ts|\.mkv|\.mpg|\.rmvb|\.wmv|\.m4v)?$");
        Match match = nameRegex.Match(value);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        result = new(
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

    public static VideoMovieFileInfo Parse(string value) =>
        TryParse(value, out VideoMovieFileInfo? result) ? result : throw new ArgumentOutOfRangeException(nameof(value), value, "Input is invalid.");

    public static ISettings Settings
    {
        get => settings ?? throw new InvalidOperationException();
        set => settings = value.ThrowIfNull();
    }
}