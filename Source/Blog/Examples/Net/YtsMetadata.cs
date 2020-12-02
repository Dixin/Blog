namespace Examples.Net
{
    using System.Collections.Generic;

    public record YtsSummary(string Link, string Title, string ImdbRating, string[] Genres, int Year, string Image);

    public record YtsMetadata(string Link, string Title, string ImdbId, string ImdbRating, string[] Genres, int Year, string Image, string Language, Dictionary<string, string> Availabilities)
        : YtsSummary(Link, Title, ImdbRating, Genres, Year, Image), IMetadata;
}
