namespace Examples.Net
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Examples.Common;

    public record ImdbEntry(string Type)
    {
        internal const string TypePropertyName = "@type";

        [JsonPropertyName(TypePropertyName)]
        public string Type { get; init; } = Type;
    }

    public record ImdbEntity(string Type, string Url, string Name) : ImdbEntry(Type)
    {
        public string Name { get; set; } = Name;
    }

    public partial record ImdbMetadata(
        string Context, string Type, string Url, string Name, string AlternateName, string Image, string[] Genre, string ContentRating,
        ImdbEntity[] Actor, ImdbEntity[] Director, ImdbEntity[] Creator,
        string Description, string DatePublished, string Keywords, ImdbAggregateRating? AggregateRating,
        string Duration, ImdbTrailer Trailer) : ImdbEntity(Type, Url, Name)
    {
        [JsonPropertyName("@context")]
        public string Context { get; init; } = Context;

        [JsonConverter(typeof(StringOrArrayConverter))]
        public string[] Genre { get; init; } = Genre;

        [JsonConverter(typeof(EntityOrArrayConverter))]
        public ImdbEntity[] Actor { get; init; } = Actor;

        [JsonConverter(typeof(EntityOrArrayConverter))]
        public ImdbEntity[] Director { get; init; } = Director;

        [JsonConverter(typeof(EntityOrArrayConverter))]
        public ImdbEntity[] Creator { get; init; } = Creator;

        public string OriginalTitle { get; set; } = string.Empty;

        public string Year { get; set; } = string.Empty;

        public string[] Regions { get; set; } = Array.Empty<string>();

        public string[] Languages { get; set; } = Array.Empty<string>();

        public Dictionary<string, string[]> Titles { get; set; } = new();

        public ImdbMetadata? Parent { get; set; }

        [JsonIgnore]
        internal string YearOfLatestRelease => DateTime.Parse(this.DatePublished).Year.ToString(CultureInfo.InvariantCulture);

        [JsonIgnore]
        internal string FormattedAggregateRating
        {
            get
            {
                string? rating = this.AggregateRating?.RatingValue;
                return string.IsNullOrWhiteSpace(rating) ? "0.0" : rating;
            }
        }

        [JsonIgnore]
        internal string FormattedContentRating => string.IsNullOrWhiteSpace(this.ContentRating)
            ? "NA"
            : this.ContentRating.Replace("-", string.Empty).Replace(" ", string.Empty).Replace("/", string.Empty).Replace(":", string.Empty);
    }

    public partial record ImdbMetadata : IMetadata
    {
        public string Link => $"https://www.imdb.com{this.Url}".AppendIfMissing("/");

        public string Title { get; set; } = string.Empty;

        public string ImdbId => this.Url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();

        public string ImdbRating => this.AggregateRating?.RatingValue ?? string.Empty;

        [JsonIgnore]
        public string[] Genres => this.Genre;
    }

    // public partial record ImdbMetadata : IEquatable<ImdbMetadata>
    // {
    //    public bool Equals(ImdbMetadata? other) =>
    //        other is not null && (ReferenceEquals(this, other) || string.Equals(this.Id, other.Id, StringComparison.OrdinalIgnoreCase));

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
}