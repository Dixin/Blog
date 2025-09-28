namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using Examples.Text.RegularExpressions;

internal record VideoEpisodeFileInfo(
    string TVTitle, string Year,
    string Season, string Episode, string AdditionalEpisode,
    string Edition,
    string Definition,
    string Origin, string VideoCodec, string AudioCodec,
    string Version,
    string MultipleAudio,
    string Watermark,
    string Encoder,
    string Subtitle,
    string EpisodeTitle,
    string Extension) : IVideoFileInfo, ISimpleParsable<VideoEpisodeFileInfo>, IDefaultSettings
{
    private static ISettings? settings;

    private static Regex? nameRegex;

    private static Regex NameRegex => nameRegex ??= RegexHelper.Create(
        @"^",
        @"(.+?)(\.([0-9]{4}))?",
        @"\.S([0-9]{2,4})E([0-9]{2,4})(E([0-9]{2,4}))?",
        @"(\.[A-Z\.\-]{2,})?(\.FAKE)?",
        @"(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p|\.Upscale\.2160p|\.Upscale\.1080p|\.Upscale\.720p)?",
        @"(\.4K\.WEB|\.BluRay|\.CAM|\.DVD|\.DVDRip|\.HDRip|\.HDTV|\.HQDVDRip|\.LDRip|\.LDDVDRip|\.LDVDRip|\.TV|\.TVRip|\.TS|\.UHD\.BluRay|\.UHD\.WEBRip|\.VCD|\.VCDRip|\.VHSRip|\.WEBRip)?",
        @"(\.AV1\.10bit\.HDR\.DV|\.AV1\.10bit\.HDR|\.AV1|\.DivX|\.H264|\.x264|\.x265\.10bit\.HDR\.10\+|\.x265\.10bit\.HDR\.DV|\.x265\.10bit\.HDR|\.x265\.10bit|\.x265|\.Xvid)?",
        @"(\.AAC5\.1|\.AAC|\.AC3|\.DD1\.0|\.DD2\.0|\.DD5\.1|\.DD|\.DDP1\.0|\.DDP2\.0|\.DDP5\.1\.Atmos|\.DDP5\.1|\.DDP|\.DTS\-HD\.MA\.1\.0|\.DTS\-HD\.MA\.2\.0|\.DTS\-HD\.MA\.5\.0|\.DTS\-HD\.MA\.5\.1|\.DTS\-HD\.MA\.6\.1|\.DTS\-HD\.MA\.7\.1|\.DTS\-HR\.5\.1|\.DTS\-HR\.7\.1|\.DTS\-X\.7\.1|\.DTS\.5\.1|\.DTS|\.FLAC|\.LPCM\.1\.0|\.LPCM\.2\.0|\.MP3|\.TrueHD\.2\.0|\.TrueHD\.5\.1|\.TrueHD\.7\.1\.Atmos|\.TrueHD\.7\.1)?",
        @$"(\{Video.VersionSeparator}({Settings.TopEnglishKeyword}|{Settings.TopForeignKeyword}|{Settings.PreferredOldKeyword}|\[{Settings.PreferredNewKeyword}\.(AG|AM|LT|ME|MX)\]|[a-zA-Z0-9@]+?))?",
        @"(\.[2-9]Audio)?",
        @"(\.watermark)?",
        @"(\.ffmpeg|\.nvenc|\.handbrake)?",
        @"(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.eng&cht|\.fre|\.heb|\.jpn|\.kor|\.pol|\.por|\.rus|\.spa|\.swe)?",
        @"(\.(.+))?",
        @"(\.avi|\.mkv|\.mp4|\.rmvb|\.ts)",
        @"$");

    public override string ToString() => this.Name;

    public string Name => $"{this.TVTitle}{(this.Year.IsNullOrWhiteSpace() ? string.Empty : $".{this.Year}")}.S{this.Season}E{this.Episode}{(this.AdditionalEpisode.IsNullOrWhiteSpace() ? string.Empty : $"E{this.AdditionalEpisode}")}{this.Edition}{this.Definition}{this.Origin}{this.VideoCodec}{this.AudioCodec}{(this.Version.IsNullOrWhiteSpace() ? string.Empty : $"{Video.VersionSeparator}{this.Version}")}{this.MultipleAudio}{this.Watermark}{this.Encoder}{this.Subtitle}{(this.EpisodeTitle.IsNullOrWhiteSpace() ? string.Empty : $".{this.EpisodeTitle}")}{this.Extension}";

    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out VideoEpisodeFileInfo? result)
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

        Match match = NameRegex.Match(value);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        result = new VideoEpisodeFileInfo(
            TVTitle: match.Groups[1].Value, Year: match.Groups[3].Value,
            Season: match.Groups[4].Value, Episode: match.Groups[5].Value, AdditionalEpisode: match.Groups[7].Value,
            Edition: match.Groups[8].Value,
            Definition: match.Groups[10].Value,
            Origin: match.Groups[11].Value,
            VideoCodec: match.Groups[12].Value, AudioCodec: match.Groups[13].Value, Version: match.Groups[15].Value,
            MultipleAudio: match.Groups[17].Value,
            Watermark: match.Groups[18].Value,
            Encoder: match.Groups[19].Value,
            Subtitle: match.Groups[20].Value,
            EpisodeTitle: match.Groups[22].Value,
            Extension: match.Groups[23].Value);
        return true;
    }

    public static VideoEpisodeFileInfo Parse(string value) =>
        TryParse(value, out VideoEpisodeFileInfo? result) ? result : throw new ArgumentOutOfRangeException(nameof(value), value, "Input is invalid.");

    public static ISettings Settings
    {
        get => settings ?? throw new InvalidOperationException();
        set => settings = value;
    }
}