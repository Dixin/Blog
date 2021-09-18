namespace Examples.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.RegularExpressions;
    using Examples.Common;

    internal record VideoFileInfo
    {
        private static readonly Regex NameRegex = new(@"^(.+)\.([0-9]{4})(\.3D(\.HSBS)?)?((\.Part[1-9])?(\.[A-Z\.]+)?(\.Part[1-9])?|\.RE\-EDIT|\.S[0-9]{2}E[0-9]{2}\.[a-zA-Z\.]+)?(\.2160p|\.1080p|\.720p|\.540p|\.480p|\.360p)?(\.BOOTLEG|\.US)?(\.WEBRip|\.BluRay|\.DVDRip|\.HDRip|\.HDTV|\.VHSRip|\.LDRip|\.DVD|\.LDVDRip|\.LDDVDRip|\.HQDVDRip|\.TV|\.VCD|\.VCDRip)?(\.H264|\.x264|\.x265|\.DivX|\.Xvid)?(\.AAC|\.AC3|\.MP3|\.AAC5\.1|\.DTS)?(\-(RARBG|VXT|\[YTS\.(MX|AM|AG|LT)\]|[a-zA-Z0-9@]+))?(\.[2-9]Audio)?(\.watermark)?(\.ffmpeg|\.nvenc|\.handbrake)?(\.bul|\.chs|\.cht|\.cht&eng|\.chs&eng|\.dut|\.eng|\.fre|\.heb|\.jap|\.kor|\.pol|\.dut|\.spa|\.swe|\.por)?(\.cd[0-9]{1,2})?(\.mp4|\.avi|\.iso|\.mkv)?$");

        internal VideoFileInfo(string name) => this.Name = name;

        internal string Title { get; init; } = string.Empty;

        internal string Year { get; init; } = string.Empty;

        internal string ThreeD { get; init; } = string.Empty;

        internal string Edition { get; init; } = string.Empty;

        internal string Definition { get; init; } = string.Empty;

        internal string AdditionalEdition { get; init; } = string.Empty;

        internal string Origin { get; init; } = string.Empty;

        internal string VideoCodec { get; init; } = string.Empty;

        internal string AudioCodec { get; init; } = string.Empty;

        internal string Version { get; init; } = string.Empty;

        internal string MultipleAudio { get; init; } = string.Empty;

        internal string Watermark { get; init; } = string.Empty;

        internal string Encoder { get; init; } = string.Empty;

        internal string Subtitle { get; init; } = string.Empty;

        internal string Part { get; init; } = string.Empty;

        internal string Extension { get; init; } = string.Empty;

        public override string ToString() => this.Name;

        internal string Name
        {
            get => $"{this.Title}.{this.Year}{this.ThreeD}{this.Edition}{this.Definition}{this.AdditionalEdition}{this.Origin}{this.VideoCodec}{this.AudioCodec}-{this.Version}{this.MultipleAudio}{this.Watermark}{this.Encoder}{this.Subtitle}{this.Part}{this.Extension}";
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

                this.Title = match.Groups[1].Value;
                this.Year = match.Groups[2].Value;
                this.ThreeD = match.Groups[3].Value;
                this.Edition = match.Groups[5].Value;
                this.Definition = match.Groups[9].Value;
                this.AdditionalEdition = match.Groups[10].Value;
                this.Origin = match.Groups[11].Value;
                this.VideoCodec = match.Groups[12].Value;
                this.AudioCodec = match.Groups[13].Value;
                this.Version = match.Groups[15].Value;
                this.MultipleAudio = match.Groups[17].Value;
                this.Watermark = match.Groups[18].Value;
                this.Encoder = match.Groups[19].Value;
                this.Subtitle = match.Groups[20].Value;
                this.Part = match.Groups[21].Value;
                this.Extension = match.Groups[22].Value;
            }
        }

        internal static bool TryParse(string name, [NotNullWhen(true)] out VideoFileInfo? info)
        {
            try
            {
                info = new VideoFileInfo(name);
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

        internal static bool IsXOrH(string nameWithoutExtension) => Regex.IsMatch(nameWithoutExtension, @"\-(RARBG|VXT)$");
    }
}
