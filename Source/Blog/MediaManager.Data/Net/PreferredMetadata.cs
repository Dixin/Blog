namespace MediaManager.Net;

using System.Text.Json.Serialization;
using Examples.Common;
using Examples.Net;

public record PreferredSummary(
    string Link, string Title, string ImdbRating, string[] Genres, string Image,
    int Year);

public record PreferredMetadata(
    string Link, string Title, string ImdbRating, string[] Genres, string Image, string ImdbId,
    int Year, string Language,
    Dictionary<string, string> Availabilities, PreferredSpec[] Specs,
    string[] Remarks, string Subtitle) : PreferredSummary(Link, Title, ImdbRating, Genres, Image, Year), IImdbMetadata
{
    [JsonIgnore]
    public Dictionary<string, string> PreferredAvailabilities
    {
        get
        {
            KeyValuePair<string, string>[] videos = this
                .Availabilities
                .Where(availability => availability.Key.ContainsIgnoreCase("1080") && availability.Key.ContainsIgnoreCase("Blu") && availability.Key.ContainsIgnoreCase("265"))
                .ToArray();
            if (videos.IsEmpty())
            {
                videos = this
                    .Availabilities
                    .Where(availability => availability.Key.ContainsIgnoreCase("1080") && availability.Key.ContainsIgnoreCase("Blu"))
                    .ToArray();
            }

            if (videos.IsEmpty())
            {
                videos = this
                    .Availabilities
                    .Where(availability => availability.Key.ContainsIgnoreCase("1080") && availability.Key.ContainsIgnoreCase("WEB") && availability.Key.ContainsIgnoreCase("265"))
                    .ToArray();
            }

            if (videos.IsEmpty())
            {
                videos = this
                    .Availabilities
                    .Where(availability => availability.Key.ContainsIgnoreCase("1080") && availability.Key.ContainsIgnoreCase("WEB"))
                    .ToArray();
            }

            if (videos.IsEmpty())
            {
                videos = this
                    .Availabilities
                    .Where(availability => availability.Key.ContainsIgnoreCase("720") && availability.Key.ContainsIgnoreCase("Blu"))
                    .ToArray();
            }

            if (videos.IsEmpty())
            {
                videos = this
                    .Availabilities
                    .Where(availability => availability.Key.ContainsIgnoreCase("720") && availability.Key.ContainsIgnoreCase("WEB"))
                    .ToArray();
            }

            if (videos.IsEmpty())
            {
                videos = this
                    .Availabilities
                    .Where(availability => availability.Key.ContainsIgnoreCase("480"))
                    .ToArray();
            }

            return new Dictionary<string, string>(videos.DistinctBy(video => video.Value, StringComparer.OrdinalIgnoreCase));
        }
    }
}

public record PreferredSpec(
    string Quality,
    string FileSize, string Resolution, string Language, string MpaRating, 
    string Subtitles, string FrameRate, string Runtime, string Seeds);

public record PreferredFileMetadata(
    string Link, string Title, string ImdbRating, string[] Genres, string Image, string ImdbId,
    int Year, string Language, string FileLink,
    string ExactTopic, string DisplayName, string[] Trackers,
    Dictionary<string, long> Files, string File, DateTime CreationDate, long Size) : IImdbMetadata, IMagnetUri;