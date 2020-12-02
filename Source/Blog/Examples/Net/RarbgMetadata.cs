namespace Examples.Net
{
    internal record RarbgMetadata(string Link, string Title, string ImdbId, string ImdbRating, string[] Genres, string DateAdded, string Size, int Seed, int Leech, string Uploader)
        : IMetadata;
}
