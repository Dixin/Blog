namespace MediaManager.Net;

public record SkinMediaSummary(string Title, string Url, string Year, string Image, int Rating);

public record SkinMediaMetadata(
    string Title, string Url, string Image, string Year,
    int Rating, string RatingDescription, string UserRating, string Description, int BlogCount, string BlogUrl,
    Dictionary<string, string[]> Details,
    SkinMediaClip[] Clips,
    SkinMediaPicture[] Pictures,
    SkinCelebrity[] Celebrities,
    SkinCelebrityScenes[] CelebrityScenes,
    SkinEpisode[] Episodes);

public record SkinMediaClip(string Title, string Url, string Image, Dictionary<string, string> Names, int Rating, string Level, string[] Keywords);

public record SkinMediaPicture(string Title, string Url, string Image, Dictionary<string, string> Names, string As, string Level, string[] Keywords);

public record SkinCelebrity(string Name, string Url, string Image, string Level, string As, SkinMediaClip[] Clips, SkinMediaPicture[] Pictures);

public record SkinCelebrityScenes(string Name, string Url, string Level, string As, SkinScene[] Scenes);

public record SkinScene(string Title, string Url, string Image, int Rating, string Level, string[] Keywords, string Position, string Description);

public record SkinEpisode(string Title, string Description, SkinMediaClip[] Clips, SkinMediaPicture[] Pictures);

public record SkinCelebritySummary(string Name, string Url, string Image, int Rating);

public record SkinCelebrityMetadata(
    string Name, string Url, string Image, string Level,
    int Rating, string RatingDescription, string UserRating, string Description, int BlogCount, string BlogUrl,
    Dictionary<string, string[]> Details,
    SkinCelebrityClip[] Clips,
    SkinCelebrityPicture[] Pictures,
    SkinMedia[] Titles,
    SkinMediaScenes[] TitleScenes,
    SkinPlaylist[] Playlist,
    SkinVideo[] Videos);

public record SkinMedia(string Title, string Url, string Year, string Image, string Level, string As, SkinMediaClip[] Clips, SkinMediaPicture[] Pictures);

public record SkinMediaScenes(string Title, string Url, string Level, string As, SkinScene[] Scenes);

public record SkinPlaylist(string Title, string Url, string Image, string Duration, int SceneCount);

public record SkinVideo(string Title, string Url, string Image, string[] Keywords);

public record SkinCelebrityClip(string Title, string Url, string Image, string Year, int Rating, string Level, string[] Keywords);

public record SkinCelebrityPicture(string Title, string Url, string Image, string Year, string As, string Level, string[] Keywords);