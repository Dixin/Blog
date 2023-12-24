namespace MediaManager.Net;

public record PreferredSummary(string Link, string Title, string ImdbRating, string[] Genres, string Image, int Year);

public record PreferredMetadata(string Link, string Title, string ImdbId, string ImdbRating, string[] Genres, string Image, int Year, string Language, Dictionary<string, string> Availabilities)
    : PreferredSummary(Link, Title, ImdbRating, Genres, Image, Year), IImdbMetadata;