﻿namespace Examples.IO;

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
            if (this.Version.EqualsOrdinal("RARBG") || this.Version.EqualsOrdinal("VXT"))
            {
                return this.VideoCodec.EqualsOrdinal(".x265") ? EncoderType.X : EncoderType.H;
            }

            if (this.Version.EqualsIgnoreCase("YIFY") || this.Version.StartsWithIgnoreCase("[YTS."))
            {
                return EncoderType.Y;
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
        this.Edition.EndsWithIgnoreCase(Video.FakeDefinition)
            ? DefinitionType.P480
            : this.Definition switch
            {
                ".2160p" => DefinitionType.P2160,
                ".1080p" => DefinitionType.P1080,
                ".720p" => DefinitionType.P720,
                _ => DefinitionType.P480
            };

    bool IsHD =>
        this.Definition is ".2160p" or ".1080p" or ".720p" && !this.Edition.EndsWithIgnoreCase(Video.FakeDefinition);
}

internal enum EncoderType
{
    P = 0,
    B,
    N,
    F,
    Y,
    H,
    X
}