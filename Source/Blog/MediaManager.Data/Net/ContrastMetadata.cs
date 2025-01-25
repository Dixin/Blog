namespace MediaManager.Net;

public record ContrastMetadata(
    string Link, string Title, string ImdbRating, string[] Genres, string Image, string ImdbId,
    string DateAdded, string Size, int Seed, int Leech, string Uploader, 
    string Torrent, string Magnet) : IImdbMetadata;