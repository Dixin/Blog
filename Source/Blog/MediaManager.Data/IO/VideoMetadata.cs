namespace Examples.IO
{
    using System;
    using Examples.Net;

    public class VideoMetadata
    {
        public ImdbMetadata? Imdb { get; set; }

        public string File { get; set; } = string.Empty;

        public int Width { get; set; }

        public int Height { get; set; }

        public double TotalMilliseconds { get; set; }

        public int Audio { get; set; }

        public int[] AudioBitRates { get; set; } = Array.Empty<int>();

        public double FrameRate { get; set; }

        internal int Subtitle { get; set; }

        internal TimeSpan Duration => TimeSpan.FromSeconds(this.TotalMilliseconds);

        internal bool Is2160P => this.Width >= 3800 || this.Height >= 2150;

        internal bool Is1080P => !this.Is2160P && (this.Width >= 1900 || this.Height >= 1070);

        internal bool Is720P => !this.Is2160P && !this.Is1080P && (this.Width >= 1280 || this.Height >= 720);

    }
}