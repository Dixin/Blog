namespace Examples.Net;

using System.Text.Json.Serialization;
using Examples.Common;

public record ImdbEntry(string Type)
{
    [JsonPropertyName("@type")]
    public string Type { get; init; } = Type;
}

public record ImdbEntity(string Type, string Url, string Name) : ImdbEntry(Type)
{
}

public partial record ImdbMetadata(
    string Context, string Type, string Url, string Name, string AlternateName, string Image, string ContentRating,
    string[] Genres, ImdbEntity[] Actor, ImdbEntity[] Director, ImdbEntity[] Creator,
    string Description, string DatePublished, string Keywords, ImdbAggregateRating? AggregateRating,
    string Duration, ImdbTrailer Trailer,
    string OriginalTitle, string Year, string[] Regions, string[] Languages, Dictionary<string, string[]> Titles, ImdbMetadata? Parent, string Title,
    string[] AllKeywords, string MpaaRating, Dictionary<string, ImdbAdvisory[]> Advisories, Dictionary<string, string[]> Releases, string AlsoKnownAs) : ImdbEntity(Type, Url, Name)
{
    [JsonPropertyName("@context")]
    public string Context { get; init; } = Context;

    [JsonConverter(typeof(EntityOrArrayConverter))]
    public ImdbEntity[] Actor { get; init; } = Actor;

    [JsonConverter(typeof(EntityOrArrayConverter))]
    public ImdbEntity[] Director { get; init; } = Director;

    [JsonConverter(typeof(EntityOrArrayConverter))]
    public ImdbEntity[] Creator { get; init; } = Creator;

    [JsonIgnore]
    internal string YearOfLatestRelease => DateTime.TryParse(this.DatePublished, out DateTime dateTime) ? dateTime.Year.ToString(CultureInfo.InvariantCulture) : string.Empty;

    [JsonIgnore]
    internal string FormattedAggregateRating => (this.AggregateRating?.RatingValue).IfNullOrWhiteSpace("0.0")!;

    [JsonIgnore]
    internal string FormattedAggregateRatingCount =>
        this.AggregateRating switch
        {
            null => "0",
            { RatingCount: < 1_000 } => this.AggregateRating.RatingCount.ToString(),
            { RatingCount: >= 1_000_000 } => $"{Regex.Replace(Math.Round(this.AggregateRating.RatingCount / 1_000_000d, 1).ToString(CultureInfo.InvariantCulture), @"\.0$", string.Empty)}M",
            { RatingCount: >= 10_000 } => $"{Math.Round(this.AggregateRating.RatingCount / 1000d)}K",
            _ => $"{Regex.Replace(Math.Round(this.AggregateRating.RatingCount / 1_000d, 1).ToString(CultureInfo.InvariantCulture), @"\.0$", string.Empty)}K"
        };

    [JsonIgnore]
    internal string FormattedContentRating => this.ContentRating.IsNullOrWhiteSpace()
        ? "NA"
        : this.ContentRating.Replace("-", string.Empty).Replace(" ", string.Empty).Replace("/", string.Empty).Replace(":", string.Empty);
}

public partial record ImdbMetadata : IImdbMetadata
{
    public string Link => $"https://www.imdb.com{this.Url}".AppendIfMissing("/");

    public string ImdbId => this.Url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();

    public string ImdbRating => this.AggregateRating?.RatingValue ?? string.Empty;

    [JsonConverter(typeof(StringOrArrayConverter))]
    [JsonPropertyName("genre")]
    public string[] Genres { get; init; } = Genres;
}

// public partial record ImdbMetadata : IEquatable<ImdbMetadata>
// {
//    public bool Equals(ImdbMetadata? other) =>
//        other is not null && (ReferenceEquals(this, other) || this.Id.EqualsIgnoreCase(other.Id));

//    public override bool Equals(object? obj) =>
//        obj is not null && (ReferenceEquals(this, obj) || obj is ImdbMetadata other && this.Equals(other));

//    public override int GetHashCode() => this.Id.GetHashCode();
// }

public record ImdbAggregateRating(string Type, int RatingCount, string BestRating, string WorstRating, string RatingValue) : ImdbEntry(Type)
{
    [JsonConverter(typeof(StringOrDoubleConverter))]
    public string BestRating { get; init; } = BestRating;

    [JsonConverter(typeof(StringOrDoubleConverter))]
    public string WorstRating { get; init; } = WorstRating;

    [JsonConverter(typeof(StringOrDoubleConverter))]
    public string RatingValue { get; init; } = RatingValue;
}

public record ImdbTrailer(string Type, string Name, string EmbedUrl, string ThumbnailUrl, string Description, DateTime UploadDate) : ImdbEntry(Type);

public record ImdbAdvisory(string Category, string Severity, string[] Details)
{
    [JsonIgnore]
    public ImdbAdvisorySeverity? FormattedSeverity => Enum.TryParse(this.Severity, out ImdbAdvisorySeverity severity) ? severity : null;
}

public enum ImdbAdvisorySeverity
{
    None = 0,
    Mild,
    Moderate,
    Severe
}

public class StringOrArrayConverter : JsonConverter<string[]>
{
    public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => new[] { reader.GetString() ?? string.Empty },
            JsonTokenType.StartArray => JsonSerializer.Deserialize<string[]>(ref reader, options) ?? Array.Empty<string>(),
            _ => throw new InvalidOperationException($"The value should be either string or array. It is actually {reader.TokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        // if (value.Length == 1)
        // {
        //    writer.WriteStringValue(value[0]);
        // }
        // else
        // {
        //    writer.WriteStartArray(); // Do not call writer.WriteStartArray(propertyName).
        //    value.ForEach(writer.WriteStringValue);
        //    writer.WriteEndArray();
        // }
        JsonSerializer.Serialize(writer, value, options);
    }
}

public class EntityOrArrayConverter : JsonConverter<ImdbEntity[]>
{
    public override ImdbEntity[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.StartObject => new[] { JsonSerializer.Deserialize<ImdbEntity>(ref reader, options)! },
            JsonTokenType.StartArray => JsonSerializer.Deserialize<ImdbEntity[]>(ref reader, options) ?? Array.Empty<ImdbEntity>(),
            _ => throw new InvalidOperationException($"The value should be either string or array. It is actually {reader.TokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, ImdbEntity[] value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}

public class StringOrDoubleConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number => JsonSerializer.Deserialize<double>(ref reader, options).ToString("0.0", CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException($"The value should be either string or number. It is actually {reader.TokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}