namespace MediaManager.Net;

using Examples.Common;
using Examples.IO;
using MediaManager.IO;
using System.Text.Json.Serialization;

public record ImdbEntry(string Type)
{
    [JsonPropertyName("@type")]
    public string Type { get; init; } = Type;
}

public record ImdbEntity(string Type, string Name, string Url) : ImdbEntry(Type);

public partial record ImdbMinMetadata(
    string Url, string Title, string OriginalTitle,
    string Type, string Name, string Image, string[]? Genres, 
    ImdbAggregateRating? AggregateRating,
    Dictionary<string, string[][]> Details,
    string AlternateName = "", string Year = "", string ContentRating = "") : ImdbEntity(Type, Name, Url)
{

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
    
    [JsonIgnore]
    internal IEnumerable<string> Regions =>
        this.Details.TryGetValue("Countries of origin", out string[][]? regions) || this.Details.TryGetValue("Country of origin", out regions)
            ? regions
                .Select(region => region.First())
                .Select(region => region switch
                {
                    "United States" => "USA",
                    "United Kingdom" => "UK",
                    _ => region
                })
            : [];

    [JsonIgnore]
    internal IEnumerable<string> Languages =>
        this.Details.TryGetValue("Languages", out string[][]? languages) || this.Details.TryGetValue("Language", out languages)
            ? languages.Select(language => language.First())
            : [];

    [JsonIgnore]
    internal IEnumerable<string> Studios =>
        this.Details.TryGetValue("Production company", out string[][]? languages)
            ? languages.Select(studio => studio.First())
            : [];
}

public partial record ImdbMinMetadata : IImdbMetadata
{
    public string ImdbId => this.Link.GetUrlPath().Split("/", StringSplitOptions.RemoveEmptyEntries).Single(item => ImdbMetadata.ImdbIdOnlyRegex().IsMatch(item));

    public string ImdbRating => this.AggregateRating?.RatingValue ?? string.Empty;

    [JsonConverter(typeof(StringOrArrayConverter))]
    [JsonPropertyName("genre")]
    public string[] Genres { get; init; } = Genres ?? [];

    public string Link => this.Url;
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ImdbMinMetadata))]
internal partial class ImdbMinMetadataSourceGenerationContext : JsonSerializerContext
{
    internal static ImdbMinMetadataSourceGenerationContext Deserialization => field ??= new(JsonHelper.DeserializerOptions);
}

public partial record ImdbMinMetadata
{
    internal static ImdbMinMetadata Deserialize(string json) =>
        JsonSerializer.Deserialize<ImdbMinMetadata>(json, ImdbMinMetadataSourceGenerationContext.Deserialization.ImdbMinMetadata)
        ?? throw new InvalidOperationException($"{json} should not be null.");

    internal static ImdbMinMetadata DeserializeFromFile(string file) =>
        Deserialize(File.ReadAllText(file));

    public static async Task<ImdbMinMetadata> DeserializeFromFileAsync(string file, CancellationToken cancellationToken = default) =>
        Deserialize(await File.ReadAllTextAsync(file, cancellationToken));

    internal static bool TryLoad(string? path, [NotNullWhen(true)] out ImdbMinMetadata? imdbMetadata)
    {
        if (TryRead(path, out string? file) && !PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag))
        {
            imdbMetadata = DeserializeFromFile(file);
            return true;
        }

        imdbMetadata = null;
        return false;
    }

    private static bool TryRead(string? path, [NotNullWhen(true)] out string? file)
    {
        if (Directory.Exists(path))
        {
            file = Directory.GetFiles(path, Video.ImdbMetadataSearchPattern, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Debug.Assert(file.IsNullOrWhiteSpace() || PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag) || PathHelper.GetFileNameWithoutExtension(file).Split(ImdbMetadata.FileNameSeparator).Length == 5);
            return file.IsNotNullOrWhiteSpace();
        }

        if (path.IsNotNullOrWhiteSpace() && path.EndsWith(ImdbMetadata.Extension) && File.Exists(path))
        {
            file = path;
            Debug.Assert(file.IsNullOrWhiteSpace() || PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag) || PathHelper.GetFileNameWithoutExtension(file).Split(ImdbMetadata.FileNameSeparator).Length == 5);
            return true;
        }

        file = null;
        return false;
    }
}

public partial record ImdbMetadata(
    string Url, string Type, string Context, ImdbMetadata? Parent,
    string Name,
    string Title, string OriginalTitle, Dictionary<string, string[][]> Titles,
    string[]? Genres, string Duration,
    string[][] ReleaseDates,
    string Image, ImdbTrailer? Trailer,
    ImdbAggregateRating? AggregateRating,
    ImdbEntity[]? Actor, ImdbEntity[]? Director, ImdbEntity[]? Creator,
    Dictionary<string, string> AllKeywords,
    string MpaaRating, Dictionary<string, string> Certifications, Dictionary<string, Dictionary<string, string[]>> Advisories,
    Dictionary<string, string[][]> Connections,
    Dictionary<string, string[][]> Credits,
    Dictionary<string, string[]> Trivia,
    Dictionary<string, string[]> Goofs,
    string[][] Quotes,
    string[] CrazyCredits,
    string[] AlternateVersions,
    string[][] Soundtracks,
    Dictionary<string, string[]> BoxOffice,
    Dictionary<string, string[][]> TechSpecs,
    string[] Awards,
    ImdbAwards[] AllAwards,
    Dictionary<string, string[][]> Details,
    Dictionary<string, string[][]> Companies,
    Dictionary<string, string[][]> Locations,
    Dictionary<string, string[][]> Sites,
    Dictionary<string, string[][]> Faqs,
    string Tagline,
    string[] Taglines,
    string AlternateName = "", string Description = "", string Year = "", string DatePublished = "", string ContentRating = "", string Keywords = ""
) : ImdbEntity(Type, Name, Url)
{
    [JsonPropertyName("@context")]
    public string Context { get; init; } = Context;

    [JsonConverter(typeof(EntityOrArrayConverter))]
    public ImdbEntity[] Actor { get; init; } = Actor ?? [];

    [JsonConverter(typeof(EntityOrArrayConverter))]
    public ImdbEntity[] Director { get; init; } = Director ?? [];

    [JsonConverter(typeof(EntityOrArrayConverter))]
    public ImdbEntity[] Creator { get; init; } = Creator ?? [];

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

    internal const string FileNameSeparator = Video.Delimiter;

    internal const string FileNameMetadataSeparator = Video.VersionSeparator;

    internal const string Extension = ".json";

    [JsonIgnore]
    internal IEnumerable<string> Regions =>
        this.Details.TryGetValue("Countries of origin", out string[][]? regions) || this.Details.TryGetValue("Country of origin", out regions)
            ? regions
                .Select(region => region.First())
                .Select(region => region switch
                {
                    "United States" => "USA",
                    "United Kingdom" => "UK",
                    _ => region
                })
            : [];

    [JsonIgnore]
    internal IEnumerable<string> Languages =>
        this.Details.TryGetValue("Languages", out string[][]? languages) || this.Details.TryGetValue("Language", out languages)
            ? languages.Select(language => language.First())
            : [];

    [JsonIgnore]
    internal IEnumerable<string> Studios =>
        this.Details.TryGetValue("Production company", out string[][]? languages)
            ? languages.Select(studio => studio.First())
            : [];

    internal IEnumerable<string> MergedKeywords => this.Keywords.Split(",").Union(this.AllKeywords.Keys, StringComparer.OrdinalIgnoreCase);

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
        regions = info[2].Split(FileNameMetadataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        languages = info[3].Split(FileNameMetadataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        genres = info[4].Split(FileNameMetadataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
                    this.Regions.Where(value => value.IsNotNullOrWhiteSpace()).Take(5).Select(value => value.ReplaceOrdinal(FileNameSeparator, string.Empty).ReplaceOrdinal(FileNameMetadataSeparator, string.Empty))),
                string.Join(
                    FileNameMetadataSeparator,
                    this.Languages.Where(value => value.IsNotNullOrWhiteSpace()).Take(3).Select(value => value.ReplaceOrdinal(FileNameSeparator, string.Empty).ReplaceOrdinal(FileNameMetadataSeparator, string.Empty))),
                string.Join(
                    FileNameMetadataSeparator,
                    this.Genres.Where(value => value.IsNotNullOrWhiteSpace()).Take(5).Select(value => value.ReplaceOrdinal(FileNameSeparator, string.Empty).ReplaceOrdinal(FileNameMetadataSeparator, string.Empty)))
            ]);
        return Path.Combine(directory, $"{name}{Extension}");
    }

    [GeneratedRegex("tt[0-9]+")]
    internal static partial Regex ImdbIdSubstringRegex();

    [GeneratedRegex("^tt[0-9]+$")]
    internal static partial Regex ImdbIdOnlyRegex();

    [GeneratedRegex(@"imdb\.com/title/(tt[0-9]+)")]
    internal static partial Regex ImdbIdInLinkRegex();
}

public partial record ImdbMetadata : IImdbMetadata
{
    public string ImdbId => this.Link.GetUrlPath().Split("/", StringSplitOptions.RemoveEmptyEntries).Single(item => ImdbIdOnlyRegex().IsMatch(item));

    public string ImdbRating => this.AggregateRating?.RatingValue ?? string.Empty;

    [JsonConverter(typeof(StringOrArrayConverter))]
    [JsonPropertyName("genre")]
    public string[] Genres { get; init; } = Genres ?? [];

    public string Link => this.Url;
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

public record ImdbAwards(string Event, string Url, ImdbAward[] Awards);

public record ImdbAward(string Status, string Url, string Title, string Description, string Remark, string[][] Items);

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

//[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
//[JsonSerializable(typeof(ImdbMetadata))]
//internal partial class ImdbMetadataSourceGenerationContext : JsonSerializerContext
//{
//	internal static ImdbMetadataSourceGenerationContext Deserialization => field ??= new(JsonHelper.DeserializerOptions);

//	internal static ImdbMetadataSourceGenerationContext Serialization => field ??= new ImdbMetadataAlphabeticalSourceGenerationContext(JsonHelper.SerializerOptions);
//}

//internal class ImdbMetadataAlphabeticalSourceGenerationContext(JsonSerializerOptions options) : ImdbMetadataSourceGenerationContext(options)
//{
//	public override JsonTypeInfo? GetTypeInfo(Type type)
//	{
//		JsonTypeInfo? typeInfo = base.GetTypeInfo(type);
//		if (typeInfo?.Kind == JsonTypeInfoKind.Object)
//		{
//			JsonPropertyInfo[] orderedProperties = typeInfo.Properties.OrderBy(property => property.Name, StringComparer.Ordinal).ToArray();
//			typeInfo.Properties.Clear();
//			foreach (JsonPropertyInfo property in orderedProperties)
//			{
//				typeInfo.Properties.Add(property);
//			}
//		}

//		return typeInfo;
//	}
//}

//public partial record ImdbMetadata
//{
//	internal static ImdbMetadata Deserialize(string jsonContext) =>
//		JsonSerializer.Deserialize<ImdbMetadata>(jsonContext, ImdbMetadataSourceGenerationContext.Deserialization.ImdbMetadata)
//		?? throw new InvalidOperationException($"{jsonContext} should not be null.");

//	internal static ImdbMetadata DeserializeFromFile(string file) =>
//		Deserialize(File.ReadAllText(file));

//	public static async Task<ImdbMetadata> DeserializeFromFileAsync(string file, CancellationToken cancellationToken = default) =>
//		Deserialize(await File.ReadAllTextAsync(file, cancellationToken));

//	internal static string Serialize(ImdbMetadata imdbMetadata) =>
//		JsonSerializer.Serialize(imdbMetadata, ImdbMetadataSourceGenerationContext.Serialization.ImdbMetadata);

//	internal static void SerializeToFile(ImdbMetadata imdbMetadata, string file) =>
//		FileHelper.WriteText(file, Serialize(imdbMetadata));

//	public static async Task SerializeToFileAsync(ImdbMetadata imdbMetadata, string file, CancellationToken cancellationToken = default) =>
//		await FileHelper.WriteTextAsync(file, Serialize(imdbMetadata), cancellationToken: cancellationToken);
//}