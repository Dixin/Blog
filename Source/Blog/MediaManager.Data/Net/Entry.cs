﻿namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.Linq;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Entry
{
    internal static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 16);

    internal static async Task DownloadMetadataAsync(
        string baseUrl, int startIndex, int count,
        string entryJsonPath, string libraryJsonPath, string x265JsonPath, string h264JsonPath, string preferredJsonPath, string h264720PJsonPath,
        int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= MaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        List<string> entryLinks = [];
        using HttpClient httpClient = new();
        await Enumerable
            .Range(startIndex, count)
            .Select(index => $"{baseUrl}/page/{index}/")
            .ForEachAsync(
                async (url, index, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, token));
                        CQ listCQ = html;
                        log($"Done {url}");
                        listCQ
                            .Find("h2.entry-title a")
                            .Select(entryLink => entryLink.GetAttribute("href"))
                            .ForEach(entryLinks.Add);
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log(exception.ToString());
                    }
                },
                cancellationToken);
        ConcurrentDictionary<string, EntryMetadata> entryMetadata = await JsonHelper
            .DeserializeFromFileAsync<ConcurrentDictionary<string, EntryMetadata>>(entryJsonPath, new(), cancellationToken);
        await entryLinks.ParallelForEachAsync(
            async (entryLink, index, token) =>
            {
                token.ThrowIfCancellationRequested();
                using HttpClient httpClient = new();
                try
                {
                    string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(entryLink, cancellationToken));
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
            },
            degreeOfParallelism,
            cancellationToken);

        await JsonHelper.SerializeToFileAsync(entryMetadata, entryJsonPath, cancellationToken);

        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, Dictionary<string, VideoMetadata>>>(libraryJsonPath, cancellationToken);
        Dictionary<string, TopMetadata[]> x265Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(x265JsonPath, cancellationToken);
        Dictionary<string, TopMetadata[]> h264Metadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(h264JsonPath, cancellationToken);
        Dictionary<string, PreferredMetadata[]> preferredMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, PreferredMetadata[]>>(preferredJsonPath, cancellationToken);
        Dictionary<string, TopMetadata[]> h264720PMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(h264720PJsonPath, cancellationToken);
        entryMetadata
            .SelectMany(entry => Regex
                .Matches(entry.Value.Content, @"imdb\.com/title/(tt[0-9]+)")
                .Where(match => match.Success)
                .Select(match => (Link: entry.Key, match.Groups[1].Value)))
            .Distinct(imdbId => imdbId.Value)
            .ForEach(imdbId =>
            {
                if (libraryMetadata.ContainsKey(imdbId.Value) && libraryMetadata[imdbId.Value].Any())
                {
                    //libraryMetadata[imdbId.Value].ForEach(video => log(video.Key));
                    //log(string.Empty);
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

                if (preferredMetadata.ContainsKey(imdbId.Value))
                {
                    log(imdbId.Link);
                    preferredMetadata[imdbId.Value].ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                }

                if (h264720PMetadata.ContainsKey(imdbId.Value))
                {
                    log(imdbId.Link);
                    h264720PMetadata[imdbId.Value].ForEach(metadata => log($"{metadata.Link} {metadata.Title}"));
                    log(string.Empty);
                }
            });
    }
}