namespace MediaManager.IO;

internal interface IVideoFileInfo : INaming
{
    string Version { get; }

    string VideoCodec { get; }

    string Encoder { get; }

    string Definition { get; }

    string Edition { get; }

    string MultipleAudio { get; }
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