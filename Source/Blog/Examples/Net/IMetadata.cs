namespace Examples.Net
{
    public interface IMetadata
    {
        string Link { get; }

        string Title { get; }

        string ImdbId { get; }

        string ImdbRating { get; }

        string[] Genres { get; }

        string Image { get; }
    }
}
