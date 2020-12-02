namespace Examples.Net
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public partial record ImdbMetadata(string Name, string[] Genre, string ContentRating, string DatePublished, ImdbAggregateRating? AggregateRating, string Url)
    {
        [JsonConverter(typeof(StringOrArrayConverter))]
        public string[] Genre { get; init; } = Genre;
    }

    public partial record ImdbMetadata
    {
        [JsonIgnore]
        internal string Year { get; init; } = string.Empty;

        [JsonIgnore]
        internal string YearOfCurrentRegion => this.DatePublished.Split('-')[0];

        [JsonIgnore]
        internal string[] Regions { get; init; } = Array.Empty<string>();

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
            : this.ContentRating.Replace("-", string.Empty).Replace("Not Rated", "Unrated").Replace("/", string.Empty).Replace(":", string.Empty);

        [JsonIgnore]
        internal string Id => this.Url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
    }

    // public partial record ImdbMetadata : IEquatable<ImdbMetadata>
    // {
    //    public bool Equals(ImdbMetadata? other) =>
    //        other is not null && (ReferenceEquals(this, other) || string.Equals(this.Id, other.Id, StringComparison.OrdinalIgnoreCase));

    //    public override bool Equals(object? obj) =>
    //        obj is not null && (ReferenceEquals(this, obj) || obj is ImdbMetadata other && this.Equals(other));

    //    public override int GetHashCode() => this.Id.GetHashCode();
    // }

    public record ImdbAggregateRating(int RatingCount, string RatingValue);

    public class StringOrArrayConverter : JsonConverter<string[]>
    {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType == JsonTokenType.StartArray
                ? JsonSerializer.Deserialize<string[]>(ref reader, options) ?? Array.Empty<string>()
                : new[] { reader.GetString() ?? string.Empty };
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            if (value.Length == 1)
            {
                writer.WriteStringValue(value[0]);
            }
            else
            {
                writer.WriteStartArray(); // Do not call writer.WriteStartArray(propertyName).
                value.ForEach(writer.WriteStringValue);
                writer.WriteEndArray();
            }
        }
    }
}