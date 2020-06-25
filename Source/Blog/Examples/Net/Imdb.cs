namespace Examples.Net
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using CsQuery;

    internal static class Imdb
    {
        internal static async Task<(string Json, string[] Regions)> DownloadJsonAsync(string url)
        {
            using WebClient webClient = new WebClient();
            string imdbHtml = await webClient.DownloadStringTaskAsync(url);
            CQ cqImdb = new CQ(imdbHtml);
            return (
                cqImdb.Find(@"script[type=""application/ld+json""]").Text(),
                cqImdb
                    .Find(@"#titleDetails .txt-block")
                    .Elements
                    .Select(element => new CQ(element).Text().Trim())
                    .FirstOrDefault(text => text.StartsWith("Country:", StringComparison.InvariantCultureIgnoreCase))
                    ?.Replace("Country:", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                    .Split('|')
                    .Select(region => region.Trim())
                    .ToArray() ?? Array.Empty<string>());
        }
    }

    public class ImdbMetadata
    {
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(StringOrArrayConverter))]
        public string[] Genre { get; set; } = Array.Empty<string>();

        public string ContentRating { get; set; } = string.Empty;

        public string DatePublished { get; set; } = string.Empty;

        public string Year => this.DatePublished.Split('-')[0];

        public ImdbAggregateRating? AggregateRating { get; set; }
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