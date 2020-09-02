namespace Examples.Net
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ImdbMetadata
    {
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(StringOrArrayConverter))]
        public string[] Genre { get; set; } = Array.Empty<string>();

        public string ContentRating { get; set; } = string.Empty;

        public string DatePublished { get; set; } = string.Empty;

        public ImdbAggregateRating? AggregateRating { get; set; }

        public string Url { get; set; } = string.Empty;

        [JsonIgnore]
        internal string Year { get; set; } = string.Empty;

        [JsonIgnore]
        internal string YearOfCurrentRegion => this.DatePublished.Split('-')[0];

        [JsonIgnore]
        internal string[] Regions { get; set; } = Array.Empty<string>();

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
        internal string Id => this.Url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
    }

    public class ImdbAggregateRating
    {
        public int RatingCount { get; set; }

        public string RatingValue { get; set; } = string.Empty;
    }

    public class StringOrArrayConverter : JsonConverter<string[]>
    {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType == JsonTokenType.StartArray
                ? JsonSerializer.Deserialize<string[]>(ref reader, options)
                : new string[] { reader.GetString() };
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            if (value.Length == 1)
            {
                writer.WriteStringValue(value[0]);
            }
            else
            {
                writer.WriteStartArray(nameof(ImdbMetadata.Genre));
                value.ForEach(writer.WriteStringValue);
                writer.WriteEndArray();
            }
        }
    }
}