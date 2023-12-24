namespace MediaManager.Net;

public interface IImdbMetadata
{
    string Link { get; }

    string Title { get; }

    string ImdbId { get; }

    string ImdbRating { get; }

    string[] Genres { get; }

    string Image { get; }
}