namespace Examples.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Entry
{
    internal static async Task DownloadMetadataAsync(
        string baseUrl, int startIndex, int count,
        string entryJsonPath, string x265JsonPath, string h264JsonPath, string ytsJsonPath, string libraryJsonPath,
        Action<string> log)
    {
        List<string> entryLinks = new();
        using WebClient webClient = new();
        await Enumerable
            .Range(startIndex, count)
            .Select(index => $"{baseUrl}/page/{index}/")
            .ForEachAsync(async url =>
            {
                try
                {
                    string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(url));
                    CQ listCQ = html;
                    log($"Done {url}");
                    listCQ
                        .Find("h1.entry-title a")
                        .Select(entryLink => entryLink.GetAttribute("href"))
                        .ForEach(entryLinks.Add);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                    log(exception.ToString());
                }
            });
        ConcurrentDictionary<string, EntryMetadata> entryMetadata = new();
        await entryLinks.ParallelForEachAsync(async entryLink =>
        {
            using WebClient webClient = new();
            try
            {
                string html = await Retry.FixedIntervalAsync(async () => await webClient.DownloadStringTaskAsync(entryLink));
                CQ entryCQ = html;
                string title = entryCQ.Find("h1.entry-title").Text().Trim();
                log($"Done {title} {entryLink}");
                entryMetadata[entryLink] = new EntryMetadata(
                    title,
                    entryCQ.Find("div.entry-content").Html());
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
                log(exception.ToString());
            }
        }, 4);

        string jsonString = JsonSerializer.Serialize(entryMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(entryJsonPath, jsonString);

        Dictionary<string, RarbgMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        Dictionary<string, RarbgMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
        Dictionary<string, YtsMetadata[]> ytsMetadata = JsonSerializer.Deserialize<Dictionary<string, YtsMetadata[]>>(await File.ReadAllTextAsync(ytsJsonPath))!;
        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;
        entryMetadata
            .SelectMany(entry => Regex
                .Matches(entry.Value.Content, "imdb.com/title/(tt[0-9]+)")
                .Where(match => match.Success)
                .Select(match => (Link: entry.Key, match.Groups[1].Value)))
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