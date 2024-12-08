namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonoTorrent;

internal static class Preferred
{
    private const int WriteCount = 800;

    private static readonly int MaxDegreeOfParallelism = int.Min(16, Environment.ProcessorCount);

    internal static async Task<ConcurrentQueue<PreferredSummary>> DownloadSummariesAsync(
        ISettings settings, Func<int, bool>? predicate = null, int initialPageIndex = 1, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        degreeOfParallelism ??= MaxDegreeOfParallelism;
        predicate ??= _ => true;
        ConcurrentDictionary<string, PreferredSummary> allSummaries = await settings.LoadMoviePreferredSummaryAsync(cancellationToken);
        ConcurrentQueue<PreferredSummary> downloadedSummaries = [];
        ConcurrentQueue<int> pageIndexes = new(Enumerable.Range(initialPageIndex, int.MaxValue).Where(predicate));
        object writeJsonLock = new();
        await Enumerable
            .Range(0, degreeOfParallelism.Value)
            .ParallelForEachAsync(
                async (httpClientIndex, _, token) =>
                {
                    using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                    while (pageIndexes.TryDequeue(out int pageIndex))
                    {
                        string url = $"{settings.MoviePreferredUrl}/browse-movies?page={pageIndex}";
                        log($"Start {url}");
                        string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, cancellationToken), cancellationToken: token);
                        CQ cq = new(html);
                        if (cq[".browse-movie-wrap"].IsEmpty())
                        {
                            log($"! {url} is empty");
                            break;
                        }

                        PreferredSummary[] summaries = cq
                            .Find(".browse-movie-wrap")
                            .Select(dom =>
                            {
                                CQ cqMovie = new(dom);
                                return new PreferredSummary(
                                    Link: cqMovie.Find(".browse-movie-title").Attr("href"),
                                    Title: cqMovie.Find(".browse-movie-title").Text(),
                                    ImdbRating: cqMovie.Find(".rating").Text().Replace(" / 10", string.Empty),
                                    Genres: cqMovie.Find("""h4[class!="rating"]""").Select(genre => genre.TextContent).ToArray(),
                                    Year: int.TryParse(cqMovie.Find(".browse-movie-year").Text(), out int year) ? year : -1,
                                    Image: cqMovie.Find("img.img-responsive").Attr("src"));
                            })
                            .ToArray();

                        summaries
                            .Do(summary => downloadedSummaries.Enqueue(summary))
                            .ForEach(summary => allSummaries[summary.Link] = summary);

                        if (pageIndex % WriteCount == 0)
                        {
                            JsonHelper.SerializeToFile(allSummaries, settings.MoviePreferredSummary, ref writeJsonLock);
                        }

                        log($"End {url}");
                    }
                },
                degreeOfParallelism.Value,
                cancellationToken);

        log($"Downloaded {downloadedSummaries.Count}");

        await settings.WriteMoviePreferredSummaryAsync(allSummaries, cancellationToken);
        return downloadedSummaries;
    }

    internal static async Task DownloadMetadataAsync(ISettings settings, Func<int, bool>? @continue = null, int index = 1, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= MaxDegreeOfParallelism;
        ConcurrentQueue<PreferredSummary> downloadedSummaries = await DownloadSummariesAsync(settings, @continue, index, degreeOfParallelism, log, cancellationToken);
        await DownloadMetadataAsync(settings, downloadedSummaries, degreeOfParallelism, log, cancellationToken);
    }

    private static async Task DownloadMetadataAsync(ISettings settings, ConcurrentQueue<PreferredSummary>? summaries = null, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= MaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        summaries ??= new ConcurrentQueue<PreferredSummary>((await settings.LoadMoviePreferredSummaryAsync(cancellationToken)).Values);
        summaries = new ConcurrentQueue<PreferredSummary>(summaries.OrderBy(summary => summary.Link));
        ConcurrentDictionary<string, List<PreferredMetadata>> details = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

        int summaryIndex = 0;
        int summaryCount = summaries.Count;
        object writeJsonLock = new();
        await Enumerable
            .Range(0, degreeOfParallelism.Value)
            .ParallelForEachAsync(
                async (httpClientIndex, _, token) =>
                {
                    using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                    while (summaries.TryDequeue(out PreferredSummary? summary))
                    {
                        token.ThrowIfCancellationRequested();
                        //log($"Start {summaryIndex}/{summaryCount}: {summary.Link}");
                        try
                        {
                            string html = await Retry.FixedIntervalAsync(
                                async () => await httpClient.GetStringAsync(summary.Link, token),
                                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound }, cancellationToken: token);
                            CQ cq = new(html);
                            CQ info = cq.Find("#movie-info");
                            CQ specsCQ = cq.Find("#movie-tech-specs");
                            string[] qualities = specsCQ
                                .Find(".tech-quality")
                                .Select(qualityDom => qualityDom.TextContent.Trim())
                                .ToArray();
                            (string FileSize, string Resolution, string Language, string MpaRating, string Subtitles, string FrameRate, string Runtime, string Seeds)[] specs = specsCQ
                                .Find(".tech-spec-info")
                                .Select(specDom => specDom.Cq())
                                .Select(specCQ => (FileSize: specCQ.Find(".tech-spec-element span[title='File Size']")[0].NextSibling.NodeValue.Trim(),
                                    Resolution: specCQ.Find(".tech-spec-element span[title='Resolution']")[0].NextSibling.NodeValue.Trim(),
                                    Language: specCQ.Find(".tech-spec-element span[title='Language']")[0].NextSibling.NodeValue.Trim(),
                                    MpaRating: specCQ.Find(".tech-spec-element span[title='MPA Rating']")[0].NextSibling.NodeValue.Trim(),
                                    Subtitles: specCQ.Find(".tech-spec-element span[title='Subtitles']").Next("a").Attr("href"),
                                    FrameRate: specCQ.Find(".tech-spec-element span[title='Frame Rate']")[0].NextSibling.NodeValue.Trim(),
                                    Runtime: specCQ.Find(".tech-spec-element span[title='Runtime']")[0].NextSibling.NodeValue.Trim(),
                                    Seeds: specCQ.Find(".tech-spec-element span[title='Seeds']").Next().Text().Trim().IfNullOrWhiteSpace(specCQ.Find(".tech-spec-element span[title='Seeds']")[0].NextSibling.NodeValue.Trim())))
                                .ToArray();
                            Debug.Assert(qualities.Length == specs.Length);
                            PreferredMetadata newDetail = new(
                                Link: summary.Link,
                                Title: summary.Title,
                                ImdbId: info.Find("a.icon[title='IMDb Rating']").Attr("href").Replace("https://www.imdb.com/title/", string.Empty).Trim('/'),
                                ImdbRating: summary.ImdbRating,
                                Genres: summary.Genres,
                                Image: summary.Image,
                                Year: summary.Year,
                                Language: info.Find("h2 a span").Text().Trim().TrimStart('[').TrimEnd(']'),
                                Subtitle: info.Find("p.hidden-sm a.button").Attr("href"),
                                Availabilities: info.Find("p.hidden-sm a[rel='nofollow']").ToDictionary(link => link.TextContent.Trim(), link => link.GetAttribute("href")),
                                Remarks: info.Find("p.hidden-sm > span").Select(remarkDom => remarkDom.TextContent.Trim()).ToArray(),
                                Specs: qualities
                                    .Zip(specs)
                                    .Select(qualitySpec => new PreferredSpec(
                                        Quality: qualitySpec.First,
                                        FileSize: qualitySpec.Second.FileSize,
                                        Resolution: qualitySpec.Second.Resolution,
                                        Language: qualitySpec.Second.Language,
                                        MpaRating: qualitySpec.Second.MpaRating,
                                        Subtitles: qualitySpec.Second.Subtitles,
                                        FrameRate: qualitySpec.Second.FrameRate,
                                        Runtime: qualitySpec.Second.Runtime,
                                        Seeds: qualitySpec.Second.Seeds))
                                    .ToArray());

                            details.AddOrUpdate(
                                newDetail.ImdbId,
                                key => [newDetail],
                                (key, group) =>
                                {
                                    PreferredMetadata? oldDetail = group.SingleOrDefault(detail => detail.Link.EqualsIgnoreCase(newDetail.Link));
                                    if (oldDetail is null)
                                    {
                                        group.Add(newDetail);
                                        return group;
                                    }

                                    Dictionary<string, string> mergedAvailabilities = new(oldDetail.Availabilities, StringComparer.OrdinalIgnoreCase);
                                    newDetail.Availabilities.ForEach(newAvailability =>
                                    {
                                        mergedAvailabilities
                                            .Keys
                                            .Where(key => mergedAvailabilities[key].EqualsIgnoreCase(newAvailability.Value))
                                            .ToArray()
                                            .ForEach(key => Debug.Assert(mergedAvailabilities.Remove(key)));

                                        mergedAvailabilities[newAvailability.Key] = newAvailability.Value;
                                    });
                                    newDetail.Availabilities.Clear();
                                    mergedAvailabilities.ForEach(mergedAvailability => newDetail.Availabilities.Add(mergedAvailability.Key, mergedAvailability.Value));
                                    group.RemoveAll(detail => detail.Link.EqualsIgnoreCase(newDetail.Link));
                                    group.Add(newDetail);
                                    return group;
                                });
                        }
                        catch (Exception exception) when (exception.IsNotCritical())
                        {
                            log($"{summary.Link} {exception}");
                        }


                        if (Interlocked.Increment(ref summaryIndex) % WriteCount == 0)
                        {
                            log($"{summaryIndex}/{summaryCount}: {summary.Link}");
                            JsonHelper.SerializeToFile(details, settings.MoviePreferredMetadata, ref writeJsonLock);
                        }
                    }
                },
                degreeOfParallelism,
                cancellationToken);

        await settings.WriteMoviePreferredMetadataAsync(details, cancellationToken);
    }

    internal static void CleanupMetadata(ConcurrentDictionary<string, List<PreferredMetadata>> details)
    {
        details
            .Where(g => g.Value.Count > 1)
            .Do(g => Debug.Assert(g.Value.All(m => m.PreferredAvailabilities.Count <= 1)))
            .Where(g => g
                .Value
                .SelectMany(m => m.PreferredAvailabilities)
                .DistinctBy(v => v.Value, StringComparer.OrdinalIgnoreCase)
                .Count() == 1)
            .Do(g => g.Value.Select(m => $"{m.Link} {m.Title} {string.Join("|", m.PreferredAvailabilities.Select(v => $"{v.Key} {v.Value}"))}").Append("").Prepend($"https://www.imdb.com/title/{g.Key}").ForEach(Logger.WriteLine))
            .ToArray()
            .AsParallel()
            .WithDegreeOfParallelism(MaxDegreeOfParallelism)
            .ForAll(g =>
            {
                using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                g.Value.AsEnumerable().Reverse().ToArray().ForEach(m =>
                {
                    if (details[g.Key].Count > 1)
                    {
                        HttpResponseMessage response = Retry.FixedInterval(() => httpClient.GetAsync(m.Link, HttpCompletionOption.ResponseHeadersRead).Result);
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                details[g.Key].RemoveAll(mm => mm.Link.EqualsOrdinal(m.Link));
                                Logger.WriteLine($"Remove {m.Link}");
                                break;
                            case HttpStatusCode.OK:
                                Logger.WriteLine($"Keep {m.Link}");
                                break;
                        }
                    }
                });
            });

        details
    .Where(g => g.Value.Count > 1)
    .Do(g => Debug.Assert(g.Value.All(m => m.PreferredAvailabilities.Count <= 1)))
    //.Where(g => g
    //    .Value
    //    .SelectMany(m => m.PreferredAvailabilities)
    //    .DistinctBy(v => v.Value, StringComparer.OrdinalIgnoreCase)
    //    .Count() == 1)
    .Do(g => g.Value.Select(m => $"{m.Link} {m.Title} {string.Join("|", m.PreferredAvailabilities.Select(v => $"{v.Key} {v.Value}"))}").Append("").Prepend($"https://www.imdb.com/title/{g.Key}").ForEach(Logger.WriteLine))
    .ToArray()
    .AsParallel()
    .ForAll(g =>
    {
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        g
            .Value
            .ToArray()
            .ForEach(m =>
            {
                if (details[g.Key].Count > 1)
                {
                    HttpResponseMessage response = Retry.FixedInterval(() => httpClient.GetAsync(m.Link, HttpCompletionOption.ResponseHeadersRead).Result);
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            if (g.Value
                                .Where(mm => !mm.Link.EqualsOrdinal(m.Link))
                                .Any(mm => m.PreferredAvailabilities.All(a => mm.Availabilities.ContainsValue(a.Value))))
                            {
                                details[g.Key].RemoveAll(mm => mm.Link.EqualsOrdinal(m.Link));
                                Logger.WriteLine($"Remove {m.Link}");
                            }
                            else if (m.PreferredAvailabilities
                                     .All(a => Retry.FixedInterval(() => httpClient.GetAsync(a.Value, HttpCompletionOption.ResponseHeadersRead).Result.StatusCode == HttpStatusCode.NotFound)))
                            {
                                details[g.Key].RemoveAll(mm => mm.Link.EqualsOrdinal(m.Link));
                                Logger.WriteLine($"Remove {m.Link}");
                            }
                            break;
                        case HttpStatusCode.OK:
                            Logger.WriteLine($"Keep {m.Link}");
                            break;
                    }
                }
            });
    });
    }

    internal static async Task WriteImdbSpecialTitles(ISettings settings, string imdbBasicsPath, CancellationToken cancellationToken = default)
    {
        string[] specialTitles = File.ReadLines(imdbBasicsPath)
            .Skip(1)
            .Select(line => line.Split('\t'))
            .Where(line => !"0".EqualsOrdinal(line.ElementAtOrDefault(4)))
            .Select(line => line[0])
            .ToArray();
        await settings.WriteMovieImdbSpecialMetadataAsync(specialTitles, cancellationToken);
    }

    internal static async Task WritePreferredSpecialTitles(string directory, string jsonPath, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        string[] titles = Directory
            .GetFiles(directory)
            .Select(file =>
            {
                string url = CQ.CreateDocumentFromFile(file)?.Find("""a.icon[title="IMDb Rating"]""")?.Attr<string>("href")?.Replace("../../external.html?link=", string.Empty) ?? string.Empty;
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    try
                    {
                        string imdbId = PathHelper.GetFileName(uri.LocalPath.TrimEnd('/'));
                        if (!imdbId.IsImdbId())
                        {
                            log($"Invalid IMDB Id: {file}, {url}.");
                        }

                        return imdbId;
                    }
                    catch
                    {
                        log($"Invalid IMDB Id: {file}, {url}.");
                        return string.Empty;
                    }
                }

                log($"Invalid IMDB Id: {file}, {url}.");
                return string.Empty;
            })
            .Where(imdbId => imdbId.IsNotNullOrWhiteSpace())
            .ToArray();
        await JsonHelper.SerializeToFileAsync(titles, jsonPath);
    }

    internal static async Task DownloadAllTorrentsAsync(ISettings settings, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        HashSet<string> existingFiles = new(Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, TorrentHelper.TorrentSearchPattern), StringComparer.OrdinalIgnoreCase);

        ConcurrentDictionary<string, List<PreferredMetadata>> preferredMetadata = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

        PreferredMetadata[] metadataToDownload = preferredMetadata
            .SelectMany(group => group.Value)
            .Where(metadata => metadata.PreferredAvailabilities
                .Select(availability => availability.Value.Split("/").Last())
                .Select(exactTopic => Path.Combine(settings.MovieMetadataCacheDirectory, $"{metadata.ImdbId}.{exactTopic}{TorrentHelper.TorrentExtension}"))
                .Any(file => !existingFiles.Contains(file)))
            .ToArray();
        int length = metadataToDownload.Length;
        log($"Estimation {length} files.");
        await metadataToDownload
            .ParallelForEachAsync(
                async (metadata, index, token) =>
                {
                    if (index % 100 == 0)
                    {
                        log($"{index * 100 / length}% - {index}/{length}");
                    }

                    await DownloadTorrentsAsync(settings, metadata, f => existingFiles.Contains(f), isDryRun, log, token);
                },
                MaxDegreeOfParallelism,
                cancellationToken);
    }

    internal static async Task<bool> DownloadTorrentsAsync(
        ISettings settings, PreferredMetadata preferredMetadata, Func<string, bool>? skip = null, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        skip ??= File.Exists;

        Dictionary<string, string> videos = preferredMetadata.PreferredAvailabilities;

        if (videos.IsEmpty())
        {
            log($"!{preferredMetadata.ImdbId} has no valid availability: {string.Join("|", preferredMetadata.Availabilities.Select(availability => availability.Key))}.");
            return false;
        }

        //log($"{preferredVideos.First().ImdbId}-{imdbMetadata.FormattedAggregateRating}-{imdbMetadata.FormattedAggregateRatingCount}-{preferredVideos.First().Title} {preferredVideos.First().Link} {imdbMetadata.Link}");
        //log($"{imdbMetadata.Link}keywords");
        //log($"{imdbMetadata.Link}parentalguide");
        using HttpClient? httpClient = isDryRun ? null : new HttpClient().AddEdgeHeaders();
        bool isDownloaded = false;
        await videos.ForEachAsync(
            async (video, index, token) =>
            {
                //log($"{video.Key} | {video.Link} | {video.Metadata.Link}");
                string exactTopic = video.Value.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last().Trim().ToUpperInvariant();
                string file = Path.Combine(settings.MovieMetadataCacheDirectory, $"{preferredMetadata.ImdbId}.{exactTopic}{TorrentHelper.TorrentExtension}");
                if (skip(file))
                {
                    log($"""
                         Skip {file}

                         """);
                    return;
                }

                if (httpClient is null)
                {
                    log($"""
                         Skip {file}

                         """);
                    return;
                }

                try
                {
                    await Retry.FixedIntervalAsync(
                            async () => await httpClient.GetFileAsync(video.Value, file, token),
                            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound }, cancellationToken: token);
                    isDownloaded = true;
                    log($"""
                         Download {file}

                         """);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                    log($"""
                         magnet:?xt=urn:btih:{exactTopic}

                         """);
                }
            },
            cancellationToken);
        return isDownloaded;
    }

    internal static async Task WriteFileMetadataAsync(ISettings settings, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, List<PreferredFileMetadata>> allFileMetadata = await settings.LoadMoviePreferredFileMetadataAsync(cancellationToken);

        HashSet<string> existingFiles = new(Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, TorrentHelper.TorrentSearchPattern), StringComparer.OrdinalIgnoreCase);

        ConcurrentDictionary<string, List<PreferredMetadata>> allMetadata = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

        (string Quality, string Link, PreferredMetadata Metadata)[] allVideos = allMetadata
            .SelectMany(group => group.Value)
            .SelectMany(
                preferredMetadata => preferredMetadata.PreferredAvailabilities,
                (preferredMetadata, video) => (Quality: video.Key, Link: video.Value, Metadata: preferredMetadata))
            .ToArray();
        int length = allVideos.Length;
        log(length.ToString());

        ILookup<string, string> existingImdbIds = existingFiles
            .ToLookup(
                file => PathHelper.GetFileNameWithoutExtension(file).Split(".").First(),
                file => PathHelper.GetFileNameWithoutExtension(file).Split(".").Last());

        allFileMetadata
            .Keys
            .Where(imdbId => !existingImdbIds.Contains(imdbId))
            .ToArray()
            .Do(imdbId => log($"Remove {imdbId}."))
            .ForEach(imdbId => Debug.Assert(allFileMetadata.TryRemove(imdbId, out _)));

        allFileMetadata
            .Keys
            .Where(imdbId => allFileMetadata[imdbId]
                .RemoveAll(metadata => !existingImdbIds[imdbId].ContainsIgnoreCase(metadata.FileLink.Split("/").Last())) > 0)
            .ForEach(imdbId => log($"Remove under {imdbId}."));

        await allVideos
            .ParallelForEachAsync(
                async (video, index, token) =>
                {
                    if (index % 100 == 0)
                    {
                        log($"{index * 100 / length}% - {index}/{length}");
                    }

                    string linkExactTopic = video.Link.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last().Trim().ToUpperInvariant();
                    string file = Path.Combine(settings.MovieMetadataCacheDirectory, $"{video.Metadata.ImdbId}.{linkExactTopic}{TorrentHelper.TorrentExtension}");
                    if (!existingFiles.Contains(file))
                    {
                        Debugger.Break(); // Missing file.
                        return;
                    }

                    if (allFileMetadata.TryGetValue(video.Metadata.ImdbId, out List<PreferredFileMetadata>? group) && group.Any(metadata => metadata.FileLink.EqualsIgnoreCase(video.Link)))
                    {
                        return;
                    }

                    Torrent torrent = await Torrent.LoadAsync(file);
                    string torrentVideoPath = torrent.Files.OrderByDescending(file => file.Length).First().Path;
                    Debug.Assert(torrentVideoPath.HasAnyExtension(Video.VideoExtension, ".mkv"));
                    string torrentVideoName = PathHelper.GetFileNameWithoutExtension(torrentVideoPath);
                    Debug.Assert(torrentVideoName.ContainsIgnoreCase("samples") || !torrentVideoName.ContainsIgnoreCase("sample"));
                    Debug.Assert(!torrentVideoName.ContainsOrdinal("@"));
                    string actualExactTopic = torrent.InfoHashes.V1OrV2.ToHex().ToUpperInvariant();
                    ITorrentFile[] torrentVideos = torrent
                        .Files
                        .Where(file => file.Path.IsVideo() && !PathHelper.GetFileNameWithoutExtension(file.Path).ContainsIgnoreCase("sample"))
                        .ToArray();
                    if (torrentVideos.Length != 1)
                    {
                        log($"Videos {torrentVideos.Length}: {string.Join("|", torrentVideos.Select(f => f.Path))}");
                    }
                    else
                    {
                        Debug.Assert(torrentVideoPath.EqualsIgnoreCase(torrentVideos.Select(file => file.Path).Single()));
                    }

                    PreferredFileMetadata fileMetadata = new(
                        video.Metadata.Link, video.Metadata.Title, video.Metadata.ImdbRating, video.Metadata.Genres, video.Metadata.Image, video.Metadata.ImdbId,
                        video.Metadata.Year, video.Metadata.Language,
                        video.Link, actualExactTopic, torrent.Name, torrent.AnnounceUrls.Concat().Where(tracker => tracker.IsNotNullOrWhiteSpace()).ToArray(),
                        torrent.Files.ToDictionary(file => file.Path, file => file.Length), torrentVideoName, torrent.CreationDate, torrent.Size);
                    allFileMetadata.AddOrUpdate(video.Metadata.ImdbId,
                        key => [fileMetadata],
                        (key, group) =>
                        {
                            group.RemoveAll(metadata => metadata.FileLink.EqualsIgnoreCase(video.Link));
                            group.Add(fileMetadata);
                            return group;
                        });
                },
                Video.IOMaxDegreeOfParallelism,
                cancellationToken);

        allFileMetadata
            .Where(group => group.Value.IsEmpty())
            .ToArray()
            .ForEach(group => Debug.Assert(allFileMetadata.TryRemove(group)));

        if (!isDryRun)
        {
            await settings.WriteMoviePreferredFileMetadataAsync(allFileMetadata, cancellationToken);
        }
    }

    internal static async Task CleanUpMetadataErrorsAsync(ISettings settings, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        ConcurrentDictionary<string, List<PreferredMetadata>> details = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);

        details
            .Values
            .SelectMany(group => group)
            .SelectMany(metadata => metadata.PreferredAvailabilities, (metadata, availability) => (Metadata: metadata, Availability: availability))
            .GroupBy(availability => availability.Availability.Value.Split("/").Last())
            .Where(groupByExactTopic => groupByExactTopic.Count() > 1)
            .Do(groupByExactTopic => groupByExactTopic
                .Select(a => $"{a.Availability.Key} {a.Metadata.Title} {a.Metadata.Link} https://www.imdb.com/title/{a.Metadata.ImdbId} {a.Availability.Value}")
                .Append(string.Empty)
                .Prepend(groupByExactTopic.Key)
                .ForEach(log))
            .ToArray()
            .ForEach(groupByExactTopic =>
            {
                PreferredMetadata[] metadataNotFound = groupByExactTopic
                    .Select(a => a.Metadata)
                    .Where(metadata => ReadStatus(metadata.Link, httpClient) == HttpStatusCode.NotFound)
                    .ToArray();
                if (metadataNotFound.IsEmpty())
                {
                    return;
                }

                Debug.Assert(metadataNotFound.Length < groupByExactTopic.Count());
                metadataNotFound.ForEach(metadata =>
                {
                    details[metadata.ImdbId].RemoveAll(item => item.Link.EqualsIgnoreCase(metadata.Link));
                    Logger.WriteLine($"Remove not found: {metadata.Link}");
                    Debug.Assert(details[metadata.ImdbId].Any());
                    // if (details[metadata.ImdbId].IsEmpty())
                    // {
                    //     Debug.Assert(details.Remove(metadata.ImdbId, out _));
                    // }
                });
            });

        if (!isDryRun)
        {
            await settings.WriteMoviePreferredMetadataAsync(details, cancellationToken);
        }

        details
            .Where(group => group.Value.Count > 1)
            .Do(group => group
                .Value
                .Select(metadata => $"{metadata.Title} {metadata.Link}")
                .Append(string.Empty)
                .Prepend($"https://www.imdb.com/title/{group.Key}")
                .ForEach(log))
            .ToArray()
            .ForEach(groupByImdbId =>
            {
                PreferredMetadata[] metadataNotFound = groupByImdbId
                    .Value
                    .Where(metadata => ReadStatus(metadata.Link, httpClient) == HttpStatusCode.NotFound)
                    .ToArray();
                Debug.Assert(metadataNotFound.Length < groupByImdbId.Value.Count);
                if (metadataNotFound.IsEmpty())
                {
                    return;
                }
                metadataNotFound.ForEach(metadata =>
                {
                    details[metadata.ImdbId].RemoveAll(item => item.Link.EqualsIgnoreCase(metadata.Link));
                    Logger.WriteLine($"Remove not found: {metadata.Link}");
                    Debug.Assert(details[metadata.ImdbId].Any());
                });
            });

        details
            .Values
            .SelectMany(group => group)
            .GroupBy(metadata => metadata.Link)
            .Where(groupByLink => groupByLink.Count() > 1)
            .ForEach(groupByLink => groupByLink
                .Select(metadata => $"{metadata.Title} https://www.imdb.com/title/{metadata.ImdbId}")
                .Append(string.Empty)
                .Prepend(groupByLink.Key)
                .ForEach(log));

        if (!isDryRun)
        {
            await settings.WriteMoviePreferredMetadataAsync(details, cancellationToken);
        }
    }

    private static HttpStatusCode ReadStatus(string url, HttpClient httpClient)
    {
        return Retry.FixedInterval(() => httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result.StatusCode);
    }

    internal static async Task CleanUpFiles(ISettings settings, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ILookup<string, string> allFiles = Directory
            .EnumerateFiles(settings.MovieMetadataCacheDirectory, TorrentHelper.TorrentSearchPattern)
            .ToLookup(file => PathHelper.GetFileNameWithoutExtension(file).Split(".")[1]);

        ConcurrentDictionary<string, List<PreferredMetadata>> details = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);
        Dictionary<string, string> exactTopicToImdbIds = details
                .Values
                .SelectMany(group => group)
                .SelectMany(metadata => metadata.PreferredAvailabilities, (metadata, availability) => (Metadata: metadata, Availability: availability))
                .GroupBy(availability => availability.Availability.Value.Split("/").Last())
                .ToDictionary(
                    groupByExactTopic => groupByExactTopic.Key,
                    groupByExactTopic => groupByExactTopic.Single().Metadata.ImdbId);

        allFiles.ForEach(fileGroup =>
        {
            if (!exactTopicToImdbIds.TryGetValue(fileGroup.Key, out string? imdbId))
            {
                log($"Delete {string.Join("|", fileGroup)}.");
                if (!isDryRun)
                {
                    fileGroup.ForEach(file => FileHelper.MoveToDirectory(file, settings.MovieMetadataCacheBackupDirectory, true, true));
                }

                return;
            }

            fileGroup.ForEach(file =>
            {
                if (PathHelper.GetFileNameWithoutExtension(file).Split(".")[0].EqualsIgnoreCase(imdbId))
                {
                    return;
                }

                log($"Rename {file} to {imdbId}.{fileGroup.Key}");
                if (!isDryRun)
                {
                    FileHelper.ReplaceFileNameWithoutExtension(file, $"{imdbId}.{fileGroup.Key}", true, true);
                }
            });
        });
    }
}