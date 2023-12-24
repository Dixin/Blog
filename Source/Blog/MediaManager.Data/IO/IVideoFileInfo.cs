namespace MediaManager.IO;

using Examples.Common;

internal interface IVideoFileInfo : INaming
{
    string Version { get; }

    string VideoCodec { get; }

    string Encoder { get; }

    string Definition { get; }

    string Edition { get; }

    string MultipleAudio { get; }

    string FormattedAudioCount => this.MultipleAudio.IsNullOrWhiteSpace() ? string.Empty : Regex.Match(this.MultipleAudio, @"\.([2-9])Audio").Groups[1].Value;

    EncoderType EncoderType
    {
        get
        {
            if (this.Version.EqualsOrdinal(Video.TopEnglishKeyword) || this.Version.EqualsOrdinal(Video.TopForeignKeyword))
            {
                return this.VideoCodec.EqualsOrdinal(".x265") ? EncoderType.X : EncoderType.H;
            }

            if (this.Version.EqualsIgnoreCase(Video.PreferredOldKeyword) || this.Version.StartsWithIgnoreCase($"[{Video.PreferredNewKeyword}."))
            {
                return this.VideoCodec.StartsWithOrdinal(".x265") ? EncoderType.XY : EncoderType.Y;
            }

            return this.Encoder.TrimStart('.') switch
            {
                "ffmpeg" => EncoderType.F,
                "nvenc" => EncoderType.N,
                "handbrake" => EncoderType.B,
                _ => EncoderType.P
            };
        }
    }

    DefinitionType DefinitionType =>
        this.Edition.EndsWithIgnoreCase(Video.UpScaleDefinition)
            ? DefinitionType.P480
            : this.Definition switch
            {
                ".2160p" => DefinitionType.P2160,
                ".1080p" => DefinitionType.P1080,
                ".720p" => DefinitionType.P720,
                _ => DefinitionType.P480
            };

    bool IsHD =>
        this.Definition is ".2160p" or ".1080p" or ".720p" && !this.Edition.EndsWithIgnoreCase(Video.UpScaleDefinition);
}

internal enum EncoderType
{
    P = 0,
    B,
    N,
    F,
    Y,
    XY,
    H,
    X
}