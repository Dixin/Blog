namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Spectre.Console;

internal static partial class AZ
{
    private static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 12);

    private const int WriteTitleMetadataCount = 1000;

    private const int WriteTagMetadataCount = 20;

    private const string TitleSummaryJson = @"D:\Files\Library\Metadata.AZ.TitleSummary.json";

    private const string TitleMetadataJson = @"D:\Files\Library\Metadata.AZ.Title.json";

    private const string TitleMetadataCacheDirectory = @"D:\Files\Library\Metadata.AZ.TitleCache";

    private const string TagSummaryJson = @"D:\Files\Library\Metadata.AZ.TagSummary.json";

    private const string TagVideoMetadataJson = @"D:\Files\Library\Metadata.AZ.TagVideo.json";

    private const string TagImageMetadataJson = @"D:\Files\Library\Metadata.AZ.TagImage.json";


    internal static async Task<ConcurrentDictionary<string, AZTitleSummary>> DownloadTitleSummariesAsync(ISettings settings, int firstPage = 1, int lastPage = 3043, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentQueue<int> pageIndexes = new(Enumerable.Range(firstPage, lastPage));
        ConcurrentDictionary<string, AZTitleSummary> summaries = File.Exists(TitleSummaryJson)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, AZTitleSummary>>(TitleSummaryJson, cancellationToken)
            : [];
        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                    while (pageIndexes.TryDequeue(out int pageIndex))
                    {
                        string url = $"https://www.aznude.com/browse/movies/updated/{pageIndex}.html";
                        log(url);
                        string html = await httpClient.GetStringAsync(url, token);
                        Debug.Assert(HtmlEndRegex().IsMatch(html));

                        CQ pageCQ = html;
                        pageCQ.Find(".vertical-list .vertical-list__item").ForEach(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ linkCQ = thumbnailCQ.Find("a:eq(0)");
                            string title = linkCQ.Find(".vertical-list__title").TextTrimDecode();
                            string url = linkCQ.Attr("href");
                            string image = thumbnailCQ.Find("img:eq(0)").Attr("src");
                            string video = thumbnailCQ.Find(".vertical-list__stat-item[data-tooltip='Videos']").TextTrimDecode();
                            int videoCount = video.IsNotNullOrWhiteSpace() ? int.Parse(video) : 0;
                            string picture = thumbnailCQ.Find(".vertical-list__stat-item[data-tooltip='Images']").TextTrimDecode();
                            int pictureCount = picture.IsNotNullOrWhiteSpace() ? int.Parse(picture) : 0;
                            int userRating = thumbnailCQ.Find(".vertical-list__star.vertical-list__full").Length;
                            string key = Path.GetFileNameWithoutExtension(url);
                            summaries[key] = new AZTitleSummary(title, url, image, videoCount, pictureCount, userRating);
                        });
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);
        try { }
        finally
        {
            await JsonHelper.SerializeToFileAsync(summaries, TitleSummaryJson, cancellationToken);
        }

        return summaries;
    }

    internal static async Task DownloadTitleMetadataAsync(ISettings settings, ConcurrentDictionary<string, AZTitleSummary>? titleSummaries = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        titleSummaries ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, AZTitleSummary>>(TitleSummaryJson, cancellationToken);

        ConcurrentDictionary<string, AZTitleMetadata> titleMetadata = File.Exists(TitleMetadataJson)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, AZTitleMetadata>>(TitleMetadataJson, cancellationToken)
            : [];
        int downloadedCount = 0;
        Lock @lock = new();
        Dictionary<string, string> cacheFiles = Directory
            .EnumerateFiles(TitleMetadataCacheDirectory)
            .ToDictionary(PathHelper.GetFileNameWithoutExtension);
        ConcurrentQueue<string> keys = new(titleSummaries.Keys.Except(titleMetadata.Keys));
        int totalCount = keys.Count;

        Progress progress = AnsiConsole.Progress();
        progress.AutoRefresh = true;
        progress.RefreshRate = TimeSpan.FromSeconds(1);
        await progress.StartAsync(async context =>
        {
            ProgressTask task = context.AddTask("Download progress", maxValue: totalCount);
            await Enumerable
                .Range(0, MaxDegreeOfParallelism)
                .ParallelForEachAsync(async (index, _, token) =>
                {
                    using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                    while (keys.TryDequeue(out string? key))
                    {
                        string url = $"https://www.aznude.com/view/movie/{key[..1]}/{key}.html";
                        string html;
                        if (cacheFiles.TryGetValue(key, out string? file))
                        {
                            html = await File.ReadAllTextAsync(file, token);
                            if (!HtmlEndRegex().IsMatch(html))
                            {
                                File.Delete(file);
                                cacheFiles.Remove(key);
                                try
                                {
                                    html = await Retry.FixedIntervalAsync(
                                        async () => await httpClient.GetStringAsync(url, token),
                                        isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                                        cancellationToken: token);
                                }
                                catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons)
                                {
                                    log($"{exception.StatusCode}: {url}");
                                    continue;
                                }

                                Debug.Assert(HtmlEndRegex().IsMatch(html));
                                try { }
                                finally
                                {
                                    await File.WriteAllTextAsync(Path.Combine(TitleMetadataCacheDirectory, $"{key}{Video.ImdbCacheExtension}"), html, token);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                html = await Retry.FixedIntervalAsync(
                                    async () => await httpClient.GetStringAsync(url, token),
                                    isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                                    cancellationToken: token);
                            }
                            catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons)
                            {
                                log($"{exception.StatusCode}: {url}");
                                continue;
                            }

                            Debug.Assert(HtmlEndRegex().IsMatch(html));
                            try { }
                            finally
                            {
                                await File.WriteAllTextAsync(Path.Combine(TitleMetadataCacheDirectory, $"{key}{Video.ImdbCacheExtension}"), html, token);
                            }
                        }

                        CQ pageCQ = html;
                        string titleYear = pageCQ.Find(".single-page-banner__title").TextTrimDecode();
                        CQ imageCQ = pageCQ.Find("img.single-page-banner__image");
                        string image = imageCQ.Attr("src");
                        string title = imageCQ.Attr("alt").TrimDecode();
                        Debug.Assert(titleYear.StartsWithIgnoreCase(title) || titleYear.StartsWithIgnoreCase("[email protected]"));
                        string year;
                        if (titleYear.StartsWithIgnoreCase(title))
                        {
                            year = titleYear[title.Length..].Trim().Trim('(', ')').Trim();
                        }
                        else if (titleYear.StartsWithIgnoreCase("[email protected]") && title.ContainsOrdinal("@"))
                        {
                            year = titleYear["[email protected]".Length..].Trim().Trim('(', ')').Trim();
                        }
                        else
                        {
                            Match match = Regex.Match(titleYear, @"^\(([0-9]{4})\)$");
                            year = match.Success ? match.Groups[1].Value : titleYear;
                            log($"Unexpected title year format: {title} in {titleYear} for {url}");
                        }

                        string video = pageCQ.Find(".single-page-banner__stat-item:contains('Videos')").TextTrimDecode().ReplaceIgnoreCase("Videos", string.Empty).Trim();
                        int videoCount = video.IsNotNullOrWhiteSpace() ? int.Parse(video) : 0;
                        string photo = pageCQ.Find(".single-page-banner__stat-item:contains('Photos')").TextTrimDecode().ReplaceIgnoreCase("Photos", string.Empty).Trim();
                        int imageCount = photo.IsNotNullOrWhiteSpace() ? int.Parse(photo) : 0;
                        string viewCount = pageCQ.Find(".single-page-banner__stat-item:contains('Views')").TextTrimDecode().ReplaceIgnoreCase("Views", string.Empty).Trim();
                        CQ regionCQ = pageCQ.Find(".country_text a");
                        string region = regionCQ.TextTrimDecode();
                        string regionUrl = regionCQ.Attr("href") ?? string.Empty;

                        AZCelebrate[] celebrates = pageCQ
                            .Find(".single-page__movie-wrapper")
                            .Select(celebrityDom =>
                            {
                                CQ celebrityCQ = celebrityDom.Cq();
                                CQ celebrityLinkCQ = celebrityCQ.Find("a.single-page-title");
                                string url = celebrityLinkCQ.Attr("href");
                                CQ titleCQ = celebrityCQ.Find(".single-page-title-text");
                                CQ nameCQ = titleCQ.Find("span");
                                string name = nameCQ.TextTrimDecode().TrimEnd(':').Trim();
                                nameCQ.Remove();
                                string character = titleCQ.TextTrimDecode();

                                AZVideo[] videos = celebrityCQ
                                    .Find(".media-list-item.video-list-item")
                                    .Select(videoDom =>
                                    {
                                        CQ videoCQ = videoDom.Cq();
                                        CQ videoLinkCQ = videoCQ.Find("a:eq(0)");
                                        string videoUrl = videoLinkCQ.Attr("href");
                                        string embedUrl = videoLinkCQ.Attr("eid");
                                        CQ imageCQ = videoCQ.Find("img:eq(0)");
                                        string videoImage = imageCQ.Attr("src");
                                        string videoTitle = imageCQ.Attr("alt");
                                        string videoRegion = videoCQ.Find(".video-country").TextTrimDecode();
                                        string videoDuration = videoCQ.Find(".video-timestamp").TextTrimDecode();
                                        CQ playerCQ = videoCQ.Find(".player-position div");
                                        string videoDescription = playerCQ.Attr("title");
                                        string thumb = playerCQ.Attr("thumb");
                                        string videoVideo = playerCQ.Attr("h");
                                        string videoImageHigh = playerCQ.Attr("i");
                                        return new AZVideo(videoTitle, videoUrl, videoImage, videoRegion, videoDuration, videoDescription, videoVideo, videoImageHigh, thumb, embedUrl);
                                    })
                                    .ToArray();

                                AZImage[] images = celebrityCQ
                                    .Find(".media-list-item.media-item")
                                    .Select(imageDom =>
                                    {
                                        CQ imageCQ = imageDom.Cq();
                                        CQ imageLinkCQ = imageCQ.Find("a:eq(0)");
                                        string url = imageLinkCQ.Attr("href");
                                        string imageTitle = imageLinkCQ.Attr("lightbox").Trim();
                                        int index = imageTitle.IndexOfIgnoreCase("<small>");
                                        string imageName;
                                        if (index >= 0)
                                        {
                                            imageName = imageTitle[index..].ReplaceIgnoreCase("<small>", string.Empty).ReplaceIgnoreCase("</small>", string.Empty);
                                            imageTitle = imageTitle[..index].Trim();
                                        }
                                        else
                                        {
                                            imageName = string.Empty;
                                        }
                                        string imageImage = imageCQ.Find("img:eq(0)").Attr("src");
                                        return new AZImage(imageTitle, imageName, imageImage, url);
                                    })
                                    .ToArray();

                                return new AZCelebrate(name, url, character, videos, images);
                            })
                            .ToArray();

                        titleMetadata[key] = new AZTitleMetadata(title, url, image, year, videoCount, imageCount, viewCount, region, regionUrl, celebrates);

                        int updatedDownloadedCount = Interlocked.Increment(ref downloadedCount);
                        if (updatedDownloadedCount % WriteTitleMetadataCount == 0)
                        {
                            log($"{downloadedCount} / {totalCount}");
                            task.Value = updatedDownloadedCount;
                            try { }
                            finally
                            {
                                JsonHelper.SerializeToFile(titleMetadata, TitleMetadataJson, ref @lock);
                            }
                        }
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

            task.Value = downloadedCount;
            try { }
            finally
            {
                await JsonHelper.SerializeToFileAsync(titleMetadata, TitleMetadataJson, cancellationToken);
            }
        });
    }

    //internal static async Task<ConcurrentDictionary<string, AZVideo>> DownloadVideoSummariesAsync(ISettings settings, int firstPage = 1, int lastPage = 6394, Action<string>? log = null, CancellationToken cancellationToken = default)
    //{
    //    log ??= Logger.WriteLine;

    //    ConcurrentQueue<int> pageIndexes = new(Enumerable.Range(firstPage, lastPage));
    //    ConcurrentDictionary<string, AZVideo> summaries = File.Exists(TitleSummaryJson)
    //        ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, AZVideo>>(VideoSummaryJson, cancellationToken)
    //        : [];
    //    await Enumerable
    //        .Range(0, MaxDegreeOfParallelism)
    //        .ParallelForEachAsync(
    //            async (index, _, token) =>
    //            {
    //                using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
    //                while (pageIndexes.TryDequeue(out int pageIndex))
    //                {
    //                    string url = $"https://www.aznude.com/browse/movies/updated/{pageIndex}.html";
    //                    log(url);
    //                    string html = await httpClient.GetStringAsync(url, token);
    //                    Debug.Assert(HtmlEndRegex().IsMatch(html));

    //                    CQ pageCQ = html;
    //                    pageCQ.Find(".vertical-list .vertical-list__item").ForEach(thumbnailDom =>
    //                    {
    //                        CQ thumbnailCQ = thumbnailDom.Cq();
    //                        CQ linkCQ = thumbnailCQ.Find("a:eq(0)");
    //                        string title = linkCQ.Find(".vertical-list__title").TextTrimDecode();
    //                        string url = linkCQ.Attr("href");
    //                        string image = thumbnailCQ.Find("img:eq(0)").Attr("src");
    //                        string video = thumbnailCQ.Find(".vertical-list__stat-item[data-tooltip='Videos']").TextTrimDecode();
    //                        int videoCount = video.IsNotNullOrWhiteSpace() ? int.Parse(video) : 0;
    //                        string picture = thumbnailCQ.Find(".vertical-list__stat-item[data-tooltip='Images']").TextTrimDecode();
    //                        int pictureCount = picture.IsNotNullOrWhiteSpace() ? int.Parse(picture) : 0;
    //                        int userRating = thumbnailCQ.Find(".vertical-list__star.vertical-list__full").Length;
    //                        string key = Path.GetFileNameWithoutExtension(url);
    //                        summaries[key] = new AZTitleSummary(title, url, image, videoCount, pictureCount, userRating);
    //                    });
    //                }
    //            },
    //            MaxDegreeOfParallelism,
    //            cancellationToken);

    //    await JsonHelper.SerializeToFileAsync(summaries, TitleSummaryJson, cancellationToken);
    //    return summaries;
    //}

    internal static async Task<Dictionary<string, string>> DownloadTagSummaryAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        Dictionary<string, string> tagSummaries = [];
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        await Enumerable.Range('a', 'z' - 'a' + 1)
            .Where(character => 'x' != (char)character)
            .Select(character => $"https://www.aznude.com/browse/tags/{(char)character}/1.html")
            .ForEachAsync(
                async (url, token) =>
                {
                    string html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, token),
                        isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                        cancellationToken: token);
                    CQ pageCQ = html;

                    pageCQ.Find(".vertical-list-tags a").ForEach(tagDom =>
                    {
                        CQ tagCQ = tagDom.Cq();
                        string tag = tagCQ.TextTrimDecode();
                        string url = tagCQ.Attr("href");
                        tagSummaries[tag] = url;
                    });

                    CQ nextPageCQ;
                    while ((nextPageCQ = pageCQ.Find(".section-next a:contains('Next')")).Any())
                    {
                        url = new Uri(new Uri(url), nextPageCQ.Attr("href")).ToString();
                        html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, token),
                            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                            cancellationToken: token);
                        pageCQ = html;

                        pageCQ.Find(".vertical-list-tags a").ForEach(tagDom =>
                        {
                            CQ tagCQ = tagDom.Cq();
                            string tag = tagCQ.TextTrimDecode();
                            string url = tagCQ.Attr("href");
                            tagSummaries[tag] = url;
                        });
                    }
                },
                cancellationToken);

        try { }
        finally
        {
            await JsonHelper.SerializeToFileAsync(tagSummaries, TagSummaryJson, cancellationToken);
        }

        return tagSummaries;
    }

    internal static async Task DownloadTagVideoMetadataAsync(ISettings settings, Dictionary<string, string>? tagSummaries = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        tagSummaries ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, string>>(TagSummaryJson, cancellationToken);

        ConcurrentDictionary<string, List<AZTagVideo>> tagVideoMetadata = File.Exists(TagVideoMetadataJson)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, List<AZTagVideo>>>(TagVideoMetadataJson, cancellationToken)
            : [];
        ConcurrentQueue<string> tagsToDownload = new(tagSummaries.Keys.Except(tagVideoMetadata.Keys));
        Lock @lock = new();
        int totalCount = tagsToDownload.Count;
        int downloadedCount = 0;
        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                    while (tagsToDownload.TryDequeue(out string? tag))
                    {
                        string url = $"https://www.aznude.com{tagSummaries[tag]}";
                        log(url);
                        string html;
                        try
                        {
                            html = await Retry.FixedIntervalAsync(
                                async () => await httpClient.GetStringAsync(url, token),
                                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                                cancellationToken: token);
                        }
                        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons)

                        {
                            log($"{exception.StatusCode}: {url}");
                            continue;
                        }

                        CQ pageCQ = html;

                        List<AZTagVideo> firstPageVideos = pageCQ
                            .Find(".media-list-item.video-list-item")
                            .Select(videoDom =>
                            {
                                CQ videoCQ = videoDom.Cq();
                                string url = videoCQ.Find("a:eq(0)").Attr("href");
                                CQ imageCQ = videoCQ.Find("img:eq(0)");
                                string image = imageCQ.Attr("src");
                                string description = imageCQ.Attr("alt");
                                string duration = videoCQ.Find(".video-timestamp").TextTrimDecode();
                                string region = videoCQ.Find(".video-country").TextTrimDecode();
                                string[] celebrates = videoCQ
                                    .Find(".video-celebs a")
                                    .Select(celebrateDom => new string[] { celebrateDom.TextTrimDecode(), celebrateDom.GetAttribute("href") })
                                    .Concat()
                                    .ToArray();
                                CQ titleCQ = videoCQ.Find("a.video-title");
                                string title = titleCQ.TextTrimDecode();
                                string titleUrl = titleCQ.Attr("href");
                                return new AZTagVideo(url, image, region, duration, description, title, titleUrl, celebrates);
                            })
                            .ToList();

                        CQ nextPageCQ;
                        while ((nextPageCQ = pageCQ.Find(".section-next a:contains('Next')")).Any())
                        {
                            url = new Uri(new Uri(url), nextPageCQ.Attr("href")).ToString();
                            log(url);
                            html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, token),
                                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                                cancellationToken: token);
                            pageCQ = html;

                            List<AZTagVideo> videos = pageCQ
                                .Find(".media-list-item.video-list-item")
                                .Select(videoDom =>
                                {
                                    CQ videoCQ = videoDom.Cq();
                                    string url = videoCQ.Find("a:eq(0)").Attr("href");
                                    CQ imageCQ = videoCQ.Find("img:eq(0)");
                                    string image = imageCQ.Attr("src");
                                    string description = imageCQ.Attr("alt");
                                    string duration = videoCQ.Find(".video-timestamp").TextTrimDecode();
                                    string region = videoCQ.Find(".video-country").TextTrimDecode();
                                    string[] celebrates = videoCQ
                                        .Find(".video-celebs a")
                                        .Select(celebrateDom => new string[] { celebrateDom.TextTrimDecode(), celebrateDom.GetAttribute("href") })
                                        .Concat()
                                        .ToArray();
                                    CQ titleCQ = videoCQ.Find("a.video-title");
                                    string title = titleCQ.TextTrimDecode();
                                    string titleUrl = titleCQ.Attr("href");
                                    return new AZTagVideo(url, image, region, duration, description, title, titleUrl, celebrates);
                                })
                                .ToList();
                            firstPageVideos.AddRange(videos);
                        }

                        tagVideoMetadata[tag] = firstPageVideos;
                        int updatedDownloadedCount = Interlocked.Increment(ref downloadedCount);
                        log($"{updatedDownloadedCount} / {totalCount} - {tag} with {firstPageVideos.Count} videos.");
                        if (updatedDownloadedCount % WriteTagMetadataCount == 0)
                        {
                            try { }
                            finally
                            {
                                JsonHelper.SerializeToFile(tagVideoMetadata, TagVideoMetadataJson, ref @lock);
                            }
                        }
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        try { }
        finally
        {
            await JsonHelper.SerializeToFileAsync(tagVideoMetadata, TagVideoMetadataJson, cancellationToken);
        }
    }


    internal static async Task DownloadTagImageMetadataAsync(ISettings settings, Dictionary<string, string>? tagSummaries = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        tagSummaries ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, string>>(TagSummaryJson, cancellationToken);

        ConcurrentDictionary<string, List<AZTagImage>> tagImageMetadata = File.Exists(TagImageMetadataJson)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, List<AZTagImage>>>(TagImageMetadataJson, cancellationToken)
            : [];
        ConcurrentQueue<string> tagsToDownload = new(tagSummaries.Keys.Except(tagImageMetadata.Keys));
        Lock @lock = new();
        int totalCount = tagsToDownload.Count;
        int downloadedCount = 0;
        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                    while (tagsToDownload.TryDequeue(out string? tag))
                    {
                        string url = $"https://www.aznude.com{tagSummaries[tag]}".ReplaceIgnoreCase("/vids/", "/imgs/");
                        log(url);
                        string html;
                        try
                        {
                            html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, token),
                                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                                cancellationToken: token);
                        }
                        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons)
                        {
                            log($"{exception.StatusCode}: {url}");
                            continue;
                        }

                        CQ pageCQ = html;

                        List<AZTagImage> firstPageVideos = pageCQ
                            .Find(".media-list-item.media-item")
                            .Select(imageDom =>
                            {
                                CQ imageCQ = imageDom.Cq();
                                CQ linkCQ = imageCQ.Find("a:eq(0)");
                                string url = linkCQ.Attr("href");
                                linkCQ.Html(linkCQ.Html().ReplaceIgnoreCase(""" alt=" alt=" """, """ alt=" """).ReplaceOrdinal("\"\"", "\""));
                                CQ thumbnailCQ = imageCQ.Find("img:eq(0)");
                                string image = thumbnailCQ.Attr("src");
                                string description = thumbnailCQ.Attr("alt").TrimDecode();
                                string[] celebrates = imageCQ
                                    .Find(".video-celebs a")
                                    .Select(celebrateDom => new string[] { celebrateDom.TextTrimDecode(), celebrateDom.GetAttribute("href") })
                                    .Concat()
                                    .ToArray();
                                CQ titleCQ = imageCQ.Find("a.video-title");
                                string title = titleCQ.TextTrimDecode();
                                string titleUrl = titleCQ.Attr("href");
                                return new AZTagImage(url, image, description, title, titleUrl, celebrates);
                            })
                            .ToList();

                        CQ nextPageCQ;
                        while ((nextPageCQ = pageCQ.Find(".section-next a:contains('Next')")).Any())
                        {
                            url = new Uri(new Uri(url), nextPageCQ.Attr("href")).ToString().ReplaceIgnoreCase("/vids/", "/imgs/");
                            log(url);
                            html = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, token),
                                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.UnavailableForLegalReasons },
                                cancellationToken: token);
                            pageCQ = html;

                            List<AZTagImage> videos = pageCQ
                                .Find(".media-list-item.video-list-item")
                                .Select(imageDom =>
                                {
                                    CQ imageCQ = imageDom.Cq();
                                    string url = imageCQ.Find("a:eq(0)").Attr("href");
                                    CQ thumbnailCQ = imageCQ.Find("img:eq(0)");
                                    string image = thumbnailCQ.Attr("src");
                                    string description = imageCQ.Attr("alt");
                                    string[] celebrates = imageCQ
                                        .Find(".video-celebs a")
                                        .Select(celebrateDom => new string[] { celebrateDom.TextTrimDecode(), celebrateDom.GetAttribute("href") })
                                        .Concat()
                                        .ToArray();
                                    CQ titleCQ = imageCQ.Find("a.video-title");
                                    string title = titleCQ.TextTrimDecode();
                                    string titleUrl = titleCQ.Attr("href");
                                    return new AZTagImage(url, image, description, title, titleUrl, celebrates);
                                })
                                .ToList();
                            firstPageVideos.AddRange(videos);
                        }

                        tagImageMetadata[tag] = firstPageVideos;
                        int updatedDownloadedCount = Interlocked.Increment(ref downloadedCount);
                        log($"{updatedDownloadedCount} / {totalCount} - {tag} with {firstPageVideos.Count} videos.");
                        if (updatedDownloadedCount % WriteTagMetadataCount == 0)
                        {
                            try { }
                            finally
                            {
                                JsonHelper.SerializeToFile(tagImageMetadata, TagImageMetadataJson, ref @lock);
                            }
                        }
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        try { }
        finally
        {
            await JsonHelper.SerializeToFileAsync(tagImageMetadata, TagImageMetadataJson, cancellationToken);
        }
    }

    [GeneratedRegex(@"\<\/html\>\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlEndRegex();
}