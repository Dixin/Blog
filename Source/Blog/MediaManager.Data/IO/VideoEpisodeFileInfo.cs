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

        nameRegex ??= new Regex(@$"^(.+?)(\.([0-9]{{4}}))?\.S([0-9]{{2,4}})E([0-9]{{2,3}})(E([0-9]{{2,3}}))?(\.[A-Z\.\-]+?)?(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?(\.WEBRip|\.BluRay|\.DVDRip|\.HDRip|\.HDTV|\.VHSRip|\.LDRip|\.DVD|\.LDVDRip|\.LDDVDRip|\.HQDVDRip|\.TV|\.VCD|\.VCDRip)?(\.H264|\.x264|\.x265|\.DivX|\.Xvid)?(\.AAC|\.AC3|\.MP3|\.AAC5\.1|\.DTS)?(\{Video.VersionSeparator}({Settings.TopEnglishKeyword}|{Settings.TopForeignKeyword}|\[{Settings.PreferredNewKeyword}\.(MX|AM|AG|LT)\]|[a-zA-Z0-9@]+?))?(\.[2-9]Audio)?(\.watermark)?(\.{FfmpegHelper.Executable}|\.nvenc|\.handbrake)?(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.fre|\.heb|\.jap|\.kor|\.pol|\.dut|\.spa|\.swe|\.por)?(\.(.+))?(\.mp4|\.avi|\.rmvb)$");
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
            Definition: match.Groups[9].Value,
            Origin: match.Groups[10].Value,
            VideoCodec: match.Groups[11].Value, AudioCodec: match.Groups[12].Value, Version: match.Groups[14].Value,
            MultipleAudio: match.Groups[16].Value,
            Watermark: match.Groups[17].Value,
            Encoder: match.Groups[18].Value,
            Subtitle: match.Groups[19].Value,
            EpisodeTitle: match.Groups[21].Value,
            Extension: match.Groups[22].Value);
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