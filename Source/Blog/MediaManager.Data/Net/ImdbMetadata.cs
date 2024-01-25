namespace MediaManager.Net;

using System.Text.Json.Serialization;
using Examples.Common;
using Examples.IO;
using MediaManager.IO;

public record ImdbEntry(string Type)
{
    [JsonPropertyName("@type")]
    public string Type { get; init; } = Type;
}

public record ImdbEntity(string Type, string Url, string Name) : ImdbEntry(Type);

public partial record ImdbMetadata(
    string Context, string Type, string Url, string Name, string AlternateName, string Image, string ContentRating,
    string[] Genres, ImdbEntity[] Actor, ImdbEntity[] Director, ImdbEntity[] Creator,
    string Description, string DatePublished, string Keywords, ImdbAggregateRating? AggregateRating,
    Dictionary<string, string[]> Websites, Dictionary<string, string> FilmingLocations, Dictionary<string, string[]> Companies,
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
    internal string FormattedAggregateRating => (this.AggregateRating?.RatingValue).IfNullOrWhiteSpace("0.0");

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

    internal const string FileNameSeparator = "-";

    internal const string FileNameMetadataSeparator = ",";

    internal const string Extension = ".json";

    internal static bool TryRead(string path, [NotNullWhen(true)] out string? imdbId, [NotNullWhen(true)] out string? year, [NotNullWhen(true)] out string[]? regions, [NotNullWhen(true)] out string[]? languages, [NotNullWhen(true)] out string[]? genres)
    {
        if (TryRead(path, out string? file))
        {
            return TryGet(file, out imdbId, out year, out regions, out languages, out genres);
        }

        imdbId = null;
        year = null;
        regions = null;
        languages = null;
        genres = null;
        return false;
    }

    private static bool TryRead(string? path, [NotNullWhen(true)] out string? file)
    {
        if (Directory.Exists(path))
        {
            file = Directory.GetFiles(path, Video.ImdbMetadataSearchPattern, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Debug.Assert(file.IsNullOrWhiteSpace() || PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag) || PathHelper.GetFileNameWithoutExtension(file).Split(FileNameSeparator).Length == 5);
            return file.IsNotNullOrWhiteSpace();
        }

        if (path.IsNotNullOrWhiteSpace() && path.EndsWith(Extension) && File.Exists(path))
        {
            file = path;
            Debug.Assert(file.IsNullOrWhiteSpace() || PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag) || PathHelper.GetFileNameWithoutExtension(file).Split(FileNameSeparator).Length == 5);
            return true;
        }

        file = null;
        return false;
    }

    internal static bool TryGet(string file, [NotNullWhen(true)] out string? imdbId, [NotNullWhen(true)] out string? year, [NotNullWhen(true)] out string[]? regions, [NotNullWhen(true)] out string[]? languages, [NotNullWhen(true)] out string[]? genres)
    {
        imdbId = null;
        year = null;
        regions = null;
        languages = null;
        genres = null;

        if (!file.HasExtension(Extension))
        {
            return false;
        }

        string name = PathHelper.GetFileNameWithoutExtension(file);
        if (name.EqualsOrdinal(Video.NotExistingFlag))
        {
            return false;
        }

        string[] info = name.Split(FileNameSeparator, StringSplitOptions.TrimEntries);
        Debug.Assert(info.Length == 5 && info[0].IsImdbId());
        imdbId = info[0];
        year = info[1];
        regions = info[2].Split(FileNameMetadataSeparator);
        languages = info[3].Split(FileNameMetadataSeparator);
        genres = info[4].Split(FileNameMetadataSeparator);
        return true;
    }

    internal static bool TryGet(IEnumerable<string> files, [NotNullWhen(true)] out string? file, [NotNullWhen(true)] out string? imdbId)
    {
        string[] metadataFiles = files.Where(file => file.HasExtension(Extension)).ToArray();
        if (metadataFiles.Length != 1)
        {
            file = null;
            imdbId = null;
            return false;
        }

        file = metadataFiles.Single();
        if (TryGet(file, out imdbId))
        {
            return true;
        }

        file = null;
        return false;
    }

    internal static bool TryGet(string file, [NotNullWhen(true)] out string? imdbId)
    {
        imdbId = null;

        if (!file.HasExtension(Extension))
        {
            return false;
        }

        string name = PathHelper.GetFileNameWithoutExtension(file);
        if (name.EqualsOrdinal(Video.NotExistingFlag))
        {
            return false;
        }

        string[] info = name.Split(FileNameSeparator, StringSplitOptions.TrimEntries);
        Debug.Assert(info.Length == 5 && info[0].IsImdbId());
        imdbId = info[0];
        return true;
    }

    internal static bool TryLoad(string? path, [NotNullWhen(true)] out ImdbMetadata? imdbMetadata)
    {
        if (TryRead(path, out string? file) && !PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag))
        {
            imdbMetadata = JsonHelper.DeserializeFromFile<ImdbMetadata>(file);
            return true;
        }

        imdbMetadata = null;
        return false;
    }

    internal string GetFilePath(string directory)
    {
        string name = string.Join(
            FileNameSeparator,
            [
                this.ImdbId,
                this.Year,
                string.Join(
                    FileNameMetadataSeparator,
                    this.Regions.Take(5).Select(value => value.ReplaceOrdinal(FileNameSeparator, string.Empty).ReplaceOrdinal(FileNameMetadataSeparator, string.Empty))),
                string.Join(
                    FileNameMetadataSeparator,
                    this.Languages.Take(3).Select(value => value.ReplaceOrdinal(FileNameSeparator, string.Empty).ReplaceOrdinal(FileNameMetadataSeparator, string.Empty))),
                string.Join(
                    FileNameMetadataSeparator,
                    this.Genres.Take(5).Select(value => value.ReplaceOrdinal(FileNameSeparator, string.Empty).ReplaceOrdinal(FileNameMetadataSeparator, string.Empty)))
            ]);
        return Path.Combine(directory, $"{name}{Extension}");
    }
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
            JsonTokenType.String => [reader.GetString() ?? string.Empty],
            JsonTokenType.StartArray => JsonSerializer.Deserialize<string[]>(ref reader, options) ?? [],
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
            JsonTokenType.StartObject => [JsonSerializer.Deserialize<ImdbEntity>(ref reader, options)!],
            JsonTokenType.StartArray => JsonSerializer.Deserialize<ImdbEntity[]>(ref reader, options) ?? [],
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