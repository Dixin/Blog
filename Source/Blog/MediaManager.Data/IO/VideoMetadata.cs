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

    internal bool Is2160P => this.Width >= 3800 || this.Height >= 2150;

    internal bool Is1080P => !this.Is2160P && (this.Width >= 1900 || this.Height >= 1070);

    internal bool Is720P => !this.Is2160P && !this.Is1080P && (this.Width >= 1280 || this.Height >= 720);

}