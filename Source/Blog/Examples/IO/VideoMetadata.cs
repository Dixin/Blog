namespace Examples.IO
{
    using System;

    public class VideoMetadata
    {
        public string ImdbId { get; set; } = string.Empty;

        public string File { get; set; } = string.Empty;

        public int Width { get; set; }

        public int Height { get; set; }

        public double TotalSeconds { get; set; }

        public int Audio { get; set; }

        internal int Subtitle { get; set; }

        internal TimeSpan Duration { get; set; }

        internal int[] AudioBitRates { get; set; } = new int[0];
    }
}