namespace Examples.Net;

public record YtsSummary(string Link, string Title, string ImdbRating, string[] Genres, string Image, int Year);

public record YtsMetadata(string Link, string Title, string ImdbId, string ImdbRating, string[] Genres, string Image, int Year, string Language, Dictionary<string, string> Availabilities)
    : YtsSummary(Link, Title, ImdbRating, Genres, Image, Year), IMetadata;