namespace MediaManager.IO;

using MediaManager.Net;

public record VideoMetadata
{
    public ImdbMetadata? Imdb { get; init; }

    public string File { get; init; } = string.Empty;

    public int VideoWidth { get; init; }

    public int VideoHeight { get; init; }

    public double VideoFrameRate { get; init; }

    public string[] AudioLanguages { get; init; } = [];

    public string[] AudioTitles { get; init; } = [];

    public long[] AudioBitRates { get; init; } = [];

    internal (string Language, string Title, string Path)[] SubtitleStreams { get; init; } = [];

    internal int Subtitle { get; init; }

    public TimeSpan Duration { get; init; }

    internal DefinitionType PhysicalDefinitionType =>
        this.VideoWidth >= 3800 || this.VideoHeight >= 2150
            ? DefinitionType.P2160
            : this.VideoWidth >= 1900 || this.VideoHeight >= 1070
                ? DefinitionType.P1080
                : this.VideoWidth >= 1280 || this.VideoHeight >= 720
                    ? DefinitionType.P720
                    : DefinitionType.P480;
}