namespace Examples.IO
{
    using System;
    using System.Text.RegularExpressions;

    internal class VideoDirectoryInfo
    {
        private static readonly Regex MovieDirectoryRegex = new Regex(@"^([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?((\=[^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?)?\.([0-9]{4})\.([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?\[([0-9]\.[0-9]|\-)\]\[(\-|R|PG|PG13|Unrated|NA|TVPG|NC17|GP|G|Approved|TVMA|Passed|TV14|TVG|X|E|MPG|M)\](\[(1080p|720p)\])?(\[3D\])?$");

        internal VideoDirectoryInfo(string name) => this.Initialize(name);

        internal string DefaultTitle1 { get; set; } = string.Empty;

        internal string DefaultTitle2 { get; set; } = string.Empty;

        internal string DefaultTitle3 { get; set; } = string.Empty;

        internal string OriginalTitle1 { get; set; } = string.Empty;

        internal string OriginalTitle2 { get; set; } = string.Empty;

        internal string OriginalTitle3 { get; set; } = string.Empty;

        internal string Year { get; set; } = string.Empty;

        internal string TranslatedTitle1 { get; set; } = string.Empty;

        internal string TranslatedTitle2 { get; set; } = string.Empty;

        internal string TranslatedTitle3 { get; set; } = string.Empty;

        internal string AggregateRating { get; set; } = string.Empty;

        internal string ContentRating { get; set; } = string.Empty;

        internal string Definition { get; set; } = string.Empty;

        internal string Is3D { get; set; } = string.Empty;

        public override string ToString() => this.Name;

        internal string Name
        {
            get => $"{this.DefaultTitle1}{this.DefaultTitle2}{this.DefaultTitle3}{this.OriginalTitle1}{this.OriginalTitle2}{this.OriginalTitle3}.{this.Year}.{this.TranslatedTitle1}{this.TranslatedTitle2}{this.TranslatedTitle3}[{this.AggregateRating}][{this.ContentRating}]{this.Definition}{this.Is3D}";
            set => this.Initialize(value);
        }

        private void Initialize(string name)
        {
            Match directoryMatch = MovieDirectoryRegex.Match(name);
            if (!directoryMatch.Success)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            this.DefaultTitle1 = directoryMatch.Groups[1].Value;
            this.DefaultTitle2 = directoryMatch.Groups[2].Value;
            this.DefaultTitle3 = directoryMatch.Groups[3].Value;
            this.OriginalTitle1 = directoryMatch.Groups[5].Value;
            this.OriginalTitle2 = directoryMatch.Groups[6].Value;
            this.OriginalTitle3 = directoryMatch.Groups[7].Value;
            this.Year = directoryMatch.Groups[8].Value;
            this.TranslatedTitle1 = directoryMatch.Groups[9].Value;
            this.TranslatedTitle2 = directoryMatch.Groups[10].Value;
            this.TranslatedTitle3 = directoryMatch.Groups[11].Value;
            this.AggregateRating = directoryMatch.Groups[12].Value;
            this.ContentRating = directoryMatch.Groups[13].Value;
            this.Definition = directoryMatch.Groups[14].Value;
            this.Is3D = directoryMatch.Groups[16].Value;
        }
    }
}