namespace MediaManager.Net;

public record TopMetadata(
    string Link, string Title, string ImdbId, string ImdbRating, string[] Genres, string Image,
    string DateAdded, string Size, int Seed, int Leech, string Uploader)
    : IImdbMetadata;