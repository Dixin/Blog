namespace Examples.Net
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CsQuery;
    using Examples.IO;

    internal static class Imdb
    {
        internal static async Task<(string Json, string Year, string[] Regions)> DownloadJsonAsync(string url)
        {
            using WebClient webClient = new WebClient();
            string imdbHtml = await webClient.DownloadStringTaskAsync(url);
            CQ cqImdb = new CQ(imdbHtml);
            string year = cqImdb.Find(@"#titleYear").Text().Trim().TrimStart('(').TrimEnd(')').Trim();
            string json = cqImdb.Find(@"script[type=""application/ld+json""]").Text();
            if (string.IsNullOrWhiteSpace(year))
            {
                string episodeYear = cqImdb.Find(@"a[title=""See more release dates""]").Text();
                Match match = Regex.Match(episodeYear, "[0-9]{4}");
                if (match.Success)
                {
                    year = match.Value;
                    Debug.Assert(year.Length == 4);
                }
            }

            if (string.IsNullOrWhiteSpace(year))
            {
                ImdbMetadata imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                    json,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true });
                year = imdbMetadata.YearOfCurrentRegion;
            }

            return (
                json,
                year,
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

        internal static bool TryLoad(string path, [NotNullWhen(true)] out ImdbMetadata? imdbMetadata)
        {
            if (Directory.Exists(path))
            {
                path = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly).Single();
            }

            string name = Path.GetFileNameWithoutExtension(path);
            if (name.Length > 1)
            {
                imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true });
                string[] names = name.Split('.');
                imdbMetadata.Year = names[1];
                imdbMetadata.Regions = names[2]?.Split(",") ?? Array.Empty<string>();
                return true;
            }

            imdbMetadata = null;
            return false;
        }
    }
}