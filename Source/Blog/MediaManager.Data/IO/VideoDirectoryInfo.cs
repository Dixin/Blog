namespace Examples.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Examples.Common;

    internal record VideoDirectoryInfo
    {
        private static readonly Regex NameRegex = new(@"^([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?((\=[^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?)?\.([0-9]{4})\.([^\.^\-^\=]+)(\-[^\.^\-^\=]+)?(\-[^\.^\-^\=]+)?\[([0-9]\.[0-9]|\-)(-([0-9\.KM]+|\-))?\]\[(\-|R|PG|PG13|Unrated|NA|TVPG|NC17|GP|G|Approved|TVMA|Passed|TV14|TVG|X|E|MPG|M|AO|NotRated)\](\[(2160|1080|720)(p|y|h|x)\])?(\[3D\])?(\[HDR\])?$");

        internal VideoDirectoryInfo(string name) => this.Name = name;

        internal string DefaultTitle1 { get; init; } = string.Empty;

        internal string DefaultTitle2 { get; init; } = string.Empty;

        internal string DefaultTitle3 { get; init; } = string.Empty;

        internal string OriginalTitle1 { get; init; } = string.Empty;

        internal string OriginalTitle2 { get; init; } = string.Empty;

        internal string OriginalTitle3 { get; init; } = string.Empty;

        internal string Year { get; init; } = string.Empty;

        internal string TranslatedTitle1 { get; init; } = string.Empty;

        internal string TranslatedTitle2 { get; init; } = string.Empty;

        internal string TranslatedTitle3 { get; init; } = string.Empty;

        internal string AggregateRating { get; init; } = string.Empty;

        internal string AggregateRatingCount { get; init; } = string.Empty;

        internal string ContentRating { get; init; } = string.Empty;

        internal string Resolution { get; init; } = string.Empty;

        internal string Source { get; init; } = string.Empty;

        internal string Is3D { get; init; } = string.Empty;

        internal string IsHdr { get; init; } = string.Empty;

        internal string FormattedDefinition => this.IsHD ? $"[{this.Resolution}{this.Source}]" : string.Empty;

        internal bool Is2160P => this.Resolution is "2160";

        internal bool Is1080P => this.Resolution is "1080";

        internal bool Is720P => this.Resolution is "720";

        internal bool IsHD => this.Resolution is ("2160" or "1080" or "720");

        public override string ToString() => this.Name;

        internal string Name
        {
            get => $"{this.DefaultTitle1}{this.DefaultTitle2}{this.DefaultTitle3}{this.OriginalTitle1}{this.OriginalTitle2}{this.OriginalTitle3}.{this.Year}.{this.TranslatedTitle1}{this.TranslatedTitle2}{this.TranslatedTitle3}[{this.AggregateRating}-{this.AggregateRatingCount}][{this.ContentRating}]{this.FormattedDefinition}{this.Is3D}{this.IsHdr}";

            init
            {
                if (Path.IsPathRooted(value))
                {
                    value = Path.GetFileName(value);
                }

                Match match = NameRegex.Match(value);
                if (!match.Success)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                this.DefaultTitle1 = match.Groups[1].Value;
                this.DefaultTitle2 = match.Groups[2].Value;
                this.DefaultTitle3 = match.Groups[3].Value;
                this.OriginalTitle1 = match.Groups[5].Value;
                this.OriginalTitle2 = match.Groups[6].Value;
                this.OriginalTitle3 = match.Groups[7].Value;
                this.Year = match.Groups[8].Value;
                this.TranslatedTitle1 = match.Groups[9].Value;
                this.TranslatedTitle2 = match.Groups[10].Value;
                this.TranslatedTitle3 = match.Groups[11].Value;
                this.AggregateRating = match.Groups[12].Value;
                this.AggregateRatingCount = match.Groups[14].Value;
                this.ContentRating = match.Groups[15].Value;
                // this.FormatedDefinition = match.Groups[16].Value;
                this.Resolution = match.Groups[17].Value;
                this.Source = match.Groups[18].Value;
                this.Is3D = match.Groups[19].Value;
                this.IsHdr = match.Groups[20].Value;
            }
        }

        internal static bool TryParse(string name, [NotNullWhen(true)] out VideoDirectoryInfo? info)
        {
            try
            {
                info = new VideoDirectoryInfo(name);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                info = null;
                return false;
            }
        }

        internal static string GetSource(string path) =>
            GetSource(Directory
                .EnumerateFiles(path, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                .Where(Video.IsVideo)
                .Select(video => new VideoFileInfo(video))
                .ToArray());

        internal static string GetSource(VideoFileInfo[] videos)
        {
            videos = videos.Where(video => video.IsHD).ToArray();
            if (videos.IsEmpty())
            {
                return string.Empty;
            }

            if (videos.Any(video => video.IsX))
            {
                return "x";
            }

            if (videos.Any(video => video.IsH))
            {
                return "h";
            }

            if (videos.Any(video => video.IsY))
            {
                return "y";
            }

            return "p";
        }
    }
}