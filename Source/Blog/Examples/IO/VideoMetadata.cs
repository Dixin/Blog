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

        public int[] AudioBitRates { get; set; } = new int[0];

        internal int Subtitle { get; set; }

        internal TimeSpan Duration => TimeSpan.FromSeconds(this.TotalMilliseconds);

    }
}