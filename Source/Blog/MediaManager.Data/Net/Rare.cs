namespace Examples.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Rare
{
    internal static async Task DownloadMetadataAsync(
        string indexUrl,
        string rareJsonPath, string x265JsonPath, string h264JsonPath, string ytsJsonPath, string libraryJsonPath,
        Action<string> log)
    {
        using WebClient webClient = new();
        string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(indexUrl));
        CQ indexCQ = html;
        string[] links = indexCQ
            .Find("#inner-slider li a")
            .Select(link => link.GetAttribute("href"))
            .ToArray();
            
        ConcurrentDictionary<string, RareMetadata> rareMetadata = new();
        await links.ParallelForEachAsync(async link =>
        {
            using WebClient webClient = new();
            try
            {
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(link));
                CQ rareCQ = html;
                string title = rareCQ.Find("#content article h1").Text().Trim();
                log($"Done {title} {link}");
                rareMetadata[link] = new RareMetadata(
                    title,
                    rareCQ.Find("#content article div.entry-content").Html());
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                log(exception.ToString());
            }
        }, 4);

        string jsonString = JsonSerializer.Serialize(rareMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(rareJsonPath, jsonString);

        Dictionary<string, RarbgMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        Dictionary<string, RarbgMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
        Dictionary<string, YtsMetadata[]> ytsMetadata = JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(ytsJsonPath))!;
        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;
        rareMetadata
            .SelectMany(rare => Regex
                .Matches(rare.Value.Content, "imdb.com/title/(tt[0-9]+)")
                .Where(match => match.Success)
                .Select(match => (Link: rare.Key, match.Groups[1].Value)))
            .Distinct(imdbId => imdbId.Value)
            .ForEach(imdbId =>
            {
                if (libraryMetadata.ContainsKey(imdbId.Value) && libraryMetadata[imdbId.Value].Any())
                {
                    libraryMetadata[imdbId.Value].ForEach(video => log(video.Key));
                    log(string.Empty);
                    return;
                }

                if (x265Metadata.ContainsKey(imdbId.Value))
                {
                    log(imdbId.Link);
                    x265Metadata[imdbId.Value].ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                    return;
                }

                if (h264Metadata.ContainsKey(imdbId.Value))
                {
                    log(imdbId.Link);
                    h264Metadata[imdbId.Value].ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                    return;
                }

                if (ytsMetadata.ContainsKey(imdbId.Value))
                {
                    log(imdbId.Link);
                    ytsMetadata[imdbId.Value].ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                }
            });
    }
}