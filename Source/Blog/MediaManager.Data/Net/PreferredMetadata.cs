using Examples.Net;

namespace MediaManager.Net;

public record PreferredSummary(
    string Link, string Title, string ImdbRating, string[] Genres, string Image,
    int Year);

public record PreferredMetadata(
    string Link, string Title, string ImdbRating, string[] Genres, string Image, string ImdbId,
    int Year, string Language,
    Dictionary<string, string> Availabilities) : PreferredSummary(Link, Title, ImdbRating, Genres, Image, Year), IImdbMetadata;

public record PreferredFileMetadata(
    string Link, string Title, string ImdbRating, string[] Genres, string Image, string ImdbId,
    int Year, string Language, string FileLink,
    string ExactTopic, string DisplayName, string[] Trackers,
    Dictionary<string, long> Files, string File, DateTime CreationDate, long Size) : IImdbMetadata, IMagnetUri;