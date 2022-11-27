namespace Examples.IO;

using Examples.Net;

public record VideoMetadata
{
    public ImdbMetadata? Imdb { get; init; }

    public string File { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }

    public double TotalMilliseconds { get; init; }

    public int Audio { get; init; }

    public int[] AudioBitRates { get; init; } = Array.Empty<int>();

    public double FrameRate { get; init; }

    internal int Subtitle { get; init; }

    internal TimeSpan Duration => TimeSpan.FromSeconds(this.TotalMilliseconds);

    internal DefinitionType DefinitionType =>
        this.Width >= 3800 || this.Height >= 2150
            ? DefinitionType.P2160
            : this.Width >= 1900 || this.Height >= 1070
                ? DefinitionType.P1080
                : this.Width >= 1280 || this.Height >= 720
                    ? DefinitionType.P720
                    : DefinitionType.P480;
}