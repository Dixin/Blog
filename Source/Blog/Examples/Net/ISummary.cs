namespace Examples.Net
{
    public interface ISummary
    {
        string Link { get; }

        string Title { get; }

        string ImdbId { get; }

        string ImdbRating { get; }

        string[] Genres { get; }
    }
}
