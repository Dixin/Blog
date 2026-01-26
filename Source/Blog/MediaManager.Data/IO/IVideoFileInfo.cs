namespace MediaManager.IO;

internal interface IVideoFileInfo : INaming
{
    string Version { get; }

    string VideoCodec { get; }

    string Encoder { get; }

    string Definition { get; }

    string Edition { get; }

    string MultipleAudio { get; }

    string Origin { get; }
}

internal enum EncoderType
{
    HD = 0,
    HDBluRay,
    Korean,
    KoreanPremium,
    Contrast,
    ContrastBluRay,
    HandbrakeH264,
    HandbrakeH264BluRay,
    NvidiaX265,
    NvidiaX265BluRay,
    FfmpegX265,
    FfmpegX265BluRay,
    PreferredH264,
    PreferredH264BluRay,
    PreferredX265,
    PreferredX265BluRay,
    TopH264,
    TopH264BluRay,
    TopX265,
    TopX265BluRay
}