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

    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out VideoMovieFileInfo? result) => TryParse(value, out result, false);
    public static bool TryParseIgnoreCase([NotNullWhen(true)] string? value, [NotNullWhen(true)] out VideoMovieFileInfo? result) => TryParse(value, out result, true);

    private static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out VideoMovieFileInfo? result, bool isCaseIgnored)
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

        nameRegex ??= new Regex(
            string.Join(
                string.Empty,
                [
                    @$"^",
                    @$"(.+?)\.([0-9\-]{{4}})",
                    @$"(\.3D(\.HSBS)?)?(\.4K)?((\.Part[1-9])?(\.[A-Z\.\-]+?)?(\.Part[\.]{{0,1}}[1-9]|\.PART[\.]{{0,1}}[1-9])?|\.RE\-EDIT|\.BLURAY\.10BIT|\.88F|\.S[0-9]{{2}}E[0-9]{{2}}\.[a-zA-Z\.]+?)?",
                    @$"(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?",
                    @$"(\.BOOTLEG|\.GBR|\.RESTORED|\.US)?",
                    @$"(\.BluRay|\.DVD|\.DVDRip|\.HDRip|\.HDTV|\.HQDVDRip|\.LDRip|\.LDDVDRip|\.LDVDRip|\.TV|\.TS|\.UHD\.BluRay|\.UHD\.WEBRip|\.VCD|\.VCDRip|\.VHSRip|\.WEBRip)?",
                    @$"(\.AV1|\.DivX|\.H264|\.x264|\.x265|\.x265\.10bit|\.x265\.10bit\.HDR|\.Xvid)?",
                    @$"(\.AAC|\.AAC5\.1|\.AC3|\.DD|\.DD5\.1\.DDP|\.DDP1\.0|\.DDP2\.0|\.DDP5\.1|\.DDP5\.1\.Atmos|\.DTS|\.DTS\-HD\.MA\.2\.0|\.DTS\-HD\.MA\.5\.0|\.DTS\-HD\.MA\.5\.1|\.DTS\-HD\.MA\.6\.1|\.DTS\-HD\.MA\.7\.1|\.DTS\-HR\.5\.1|\.DTS\-X\.7\.1|\.DTS\.5\.1|\.LPCM\.1\.0|\.LPCM\.2\.0|\.MP3|\.TrueHD\.2\.0|\.TrueHD\.5\.1|\.TrueHD\.7\.1|\.TrueHD\.7\.1\.Atmos)?",
                    @$"(\{Video.VersionSeparator}({Settings.TopEnglishKeyword}|{Settings.TopForeignKeyword}|{Settings.PreferredOldKeyword}|\[{Settings.PreferredNewKeyword}\.(AG|AM|LT|ME|MX)\]|[a-zA-Z0-9@]+?))?",
                    @$"(\.[2-9]Audio)?",
                    @$"(\.watermark)?",
                    @$"(\.{FfmpegHelper.Executable}|\.nvenc|\.handbrake)?",
                    @$"(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.eng&cht|\.fre|\.heb|\.jpn|\.kor|\.pol|\.por|\.rus|\.spa|\.swe)?",
                    @$"(\.cd[0-9]{{1,2}})?",
                    @$"(\.avi|\.iso|\.m2ts|\.m4v|\.mkv|\.mp4|\.mpg|\.rmvb|\.wmv|\.ts)?",
                    @$"$"
                ]),
            isCaseIgnored ? RegexOptions.IgnoreCase : RegexOptions.None);
        Match match = nameRegex.Match(value);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        result = new(
            Title: match.Groups[1].Value, Year: match.Groups[2].Value,
            ThreeD: match.Groups[3].Value, Edition: $"{match.Groups[5].Value}{match.Groups[6].Value}",
            Definition: match.Groups[10].Value, AdditionalEdition: match.Groups[11].Value,
            Origin: match.Groups[12].Value, VideoCodec: match.Groups[13].Value, AudioCodec: match.Groups[14].Value,
            Version: match.Groups[16].Value,
            MultipleAudio: match.Groups[18].Value,
            Watermark: match.Groups[19].Value,
            Encoder: match.Groups[20].Value,
            Subtitle: match.Groups[21].Value,
            Part: match.Groups[22].Value,
            Extension: match.Groups[23].Value);
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