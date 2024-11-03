namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;

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
        
        nameRegex ??= new Regex(string.Join(
            string.Empty,
            [
                @$"^",
                @$"(.+?)(\.([0-9]{{4}}))?",
                @$"\.S([0-9]{{2,4}})E([0-9]{{2,3}})(E([0-9]{{2,3}}))?",
                @$"(\.[A-Z\.\-]+?)?(\.HDR)?",
                @$"(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?",
                @$"(\.BluRay|\.DVD|\.DVDRip|\.HDRip|\.HDTV|\.HQDVDRip|\.LDRip|\.LDDVDRip|\.LDVDRip|\.TV|\.TS|\.UHD\.BluRay|\.UHD\.WEBRip|\.VCD|\.VCDRip|\.VHSRip|\.WEBRip)?",
                @$"(\.DivX|\.H264|\.x264|\.x265|\.x265\.10bit|\.x265\.10bit\.HDR|\.Xvid)?",
                @$"(\.AAC|\.AAC5\.1|\.AC3|\.DD|\.DDP|\.DDP1\.0|\.DDP2\.0|\.DDP5\.1|\.DDP5\.1\.Atmos|\.DTS|\.DTS\-HD\.MA\.2\.0|\.DTS\-HD\.MA\.5\.0|\.DTS\-HD\.MA\.5\.1|\.DTS\-HD\.MA\.6\.1|\.DTS\-HD\.MA\.7\.1|\.DTS\-HR\.5\.1|\.DTS\-X\.7\.1|\.DTS\.5\.1|\.LPCM\.1\.0|\.LPCM\.2\.0|\.MP3|\.TrueHD\.2\.0|\.TrueHD\.5\.1|\.TrueHD\.7\.1|\.TrueHD\.7\.1\.Atmos)?",
                @$"(\{Video.VersionSeparator}({Settings.TopEnglishKeyword}|{Settings.TopForeignKeyword}|{Settings.PreferredOldKeyword}|\[{Settings.PreferredNewKeyword}\.(AG|AM|LT|ME|MX)\]|[a-zA-Z0-9@]+?))?",
                @$"(\.[2-9]Audio)?",
                @$"(\.watermark)?",
                @$"(\.{FfmpegHelper.Executable}|\.nvenc|\.handbrake)?",
                @$"(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.eng&cht|\.fre|\.heb|\.jpn|\.kor|\.pol|\.por|\.rus|\.spa|\.swe)?",
                @$"(\.(.+))?",
                @$"(\.avi|\.mkv|\.mp4|\.rmvb|\.ts)",
                @$"$"
            ]));
        Match match = nameRegex.Match(value);
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