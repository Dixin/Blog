using System.Text.Json.Serialization;

namespace Examples.Net;

public record WikipediaNikkatsu(string ReleaseDate, string OriginalTitle, string TranslatedTitle, string Director, string Note)
{
    public string[] EnglishTitles { get; init; } = [];

    public string[] Links { get; init; } = [];

    public string[] Cast { get; init; } = [];

    [JsonIgnore]
    public string Year => this.ReleaseDate.Split("-").First();

    [JsonIgnore]
    public string EnglishTitleMerge => string.Join("|", this.EnglishTitles);
}
