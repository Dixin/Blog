namespace MediaManager.Net;

public record AZTitleSummary(string Title, string Url, string Image, int VideoCount, int ImageCount, int UserRating);

public record AZTitleMetadata(
    string Title, string Url, string Image, string Year, int VideoCount, int ImageCount, string ViewCount,
    string Region, string RegionUrl,
    AZCelebrate[] Celebrates);

public record AZCelebrate(string Name, string Url, string Character, AZVideo[] Videos, AZImage[] Images);

public record AZVideo(
    string Title, string Url, string Image, string Regions, string Duration, string Description, string Video, string ImageHigh,
    string Thumb, string EmbedUrl);

public record AZTagVideo(
    string Url, string Image, string Regions, string Duration, string Description,
    string Title, string TitleUrl, string[] Celebrates);

public record AZImage(string Title, string Name, string Image, string Url);

public record AZTagImage(string Url, string Image, string Description,
    string Title, string TitleUrl, string[] Celebrates);