namespace MediaManager.Net;

public record SkinSummary(string Title, string Url, string Year, string Image, int Rating);

public record SkinMetadata(
    string Title, string Url, string Image, string Year,
    int Rating, string RatingDescription, string UserRating, string Description, int BlogCount, string BlogUrl,
    Dictionary<string, string[]> Details,
    SkinClip[] Clips,
    SkinPicture[] Pictures,
    SkinCelebrity[] Celebrities,
    SkinCelebrityScenes[] CelebScenes,
    SkinEpisode[] Episodes);

public record SkinClip(string Title, string Url, string Image, Dictionary<string, string> Names, int Rating, string Level, string[] Keywords);

public record SkinPicture(string Title, string Url, string Image, Dictionary<string, string> Names, string As, string Level, string[] Keywords);

public record SkinCelebrity(string Name, string Url, string Image, string Level, string As, SkinClip[] Clips, SkinPicture[] Pictures);

public record SkinCelebrityScenes(string Name, string Url, string Level, string As, SkinScene[] Scenes);

public record SkinScene(string Title, string Url, string Image, int Rating, string Level, string[] Keywords, string Position, string Description);

public record SkinEpisode(string Title, string Description, SkinClip[] Clips, SkinPicture[] Pictures);