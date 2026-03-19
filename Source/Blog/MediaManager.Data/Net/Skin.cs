namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using MediaManager.IO;
using Microsoft.Playwright;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Skin
{
    private static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 12);

    private const int WriteCount = 100;

    private const string BaseUrl = "https://www.mrskin.com";

    private const string DirectoryMetadataMediaCache = @"D:\Files\Library\Metadata.Skin.Media.Cache";

    private const string DirectoryMetadataCelebrities = @"D:\Files\Library\Metadata.Skin.Celebrities";

    private const string DirectoryMetadataCelebritiesCache = @"D:\Files\Library\Metadata.Skin.Celebrities.Cache";

    private const string DirectoryMetadataMedia = @"D:\Files\Library\Metadata.Skin.Media";

    private const string FileMetadataMedia = @"D:\Files\Library\Metadata.Skin.Media.json";

    private const string FileMetadataCelebrities = @"D:\Files\Library\Metadata.Skin.Celebrities.json";

    private const string FileMetadataMediaSummaries = @"D:\Files\Library\Metadata.Skin.Media.Summaries.json";

    private const string FileMetadataCelebritySummaries = @"D:\Files\Library\Metadata.Skin.Celebrities.Summaries.json";

    private const string FileCookie = @"D:\Files\Library\Metadata.Skin.Cookie.json";

    private static Lock writeLock = new();

    internal static async Task<ConcurrentDictionary<string, SkinMediaSummary>> DownloadMediaSummariesAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, SkinMediaSummary> summaries = File.Exists(FileMetadataMediaSummaries)
            ? JsonHelper.DeserializeFromFile<ConcurrentDictionary<string, SkinMediaSummary>>(FileMetadataMediaSummaries)
            : [];
        ConcurrentQueue<int> pageNumbers = new(Enumerable.Range(1, 5));
        await Enumerable.Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new();
                    IPage page = await playWrightWrapper.PageAsync();
                    while (pageNumbers.TryDequeue(out int pageNumber))
                    {
                        string url = $"{BaseUrl}/search/titles?page={pageNumber}&sort=most_recent";
                        log(url);
                        string html = await page.GetStringAsync(url, "#search-results", cancellationToken: token);
                        if (await page.ClickOrPressAsync("#age-gate-agree a", cancellationToken: token) > 0)
                        {
                            html = await page.ContentAsync();
                        }

                        CQ pageCQ = html;
                        pageCQ.Find("#search-results .thumbnail-column").ForEach(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ linkCQ = thumbnailCQ.Find(".caption a:eq(0)");
                            string url = linkCQ.Attr("href");
                            string image = thumbnailCQ.Find("img:eq(0)").Attr("src");
                            string title = linkCQ.Attr("title");
                            string year = thumbnailCQ.Find(".caption .text-muted").TextTrimDecode().TrimStart('(').TrimEnd(')');
                            int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                            summaries[url] = new SkinMediaSummary(title, url, year, image, rating);
                        });
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        await JsonHelper.SerializeToFileAsync(summaries, FileMetadataMediaSummaries, cancellationToken: cancellationToken);
        return summaries;
    }

    internal static async Task<ConcurrentDictionary<string, SkinCelebritySummary>> DownloadCelebritySummariesAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, SkinCelebritySummary> summaries = File.Exists(FileMetadataCelebritySummaries)
            ? JsonHelper.DeserializeFromFile<ConcurrentDictionary<string, SkinCelebritySummary>>(FileMetadataCelebritySummaries)
            : [];
        ConcurrentQueue<int> pageNumbers = new(Enumerable.Range(1, 1054));
        await Enumerable.Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new();
                    IPage page = await playWrightWrapper.PageAsync();
                    while (pageNumbers.TryDequeue(out int pageNumber))
                    {
                        string url = $"{BaseUrl}/search/celebs?page={pageNumber}&sort=most_recent";
                        log(url);
                        string html = await page.GetStringAsync(url, "#search-results", cancellationToken: token);
                        if (await page.ClickOrPressAsync("#age-gate-agree a", cancellationToken: token) > 0)
                        {
                            html = await page.ContentAsync();
                        }

                        CQ pageCQ = html;
                        pageCQ.Find("#search-results .thumbnail-column").ForEach(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ linkCQ = thumbnailCQ.Find(".caption a:eq(0)");
                            string url = linkCQ.Attr("href");
                            string image = thumbnailCQ.Find("img:eq(0)").Attr("src");
                            string name = linkCQ.TextTrimDecode();
                            string rating = thumbnailCQ
                                .Find(".star-rating-simple")
                                .Attr("class")
                                .Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Last()[^1..];
                            summaries[url] = new SkinCelebritySummary(name, url, image, int.Parse(rating));
                        });
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        await JsonHelper.SerializeToFileAsync(summaries, FileMetadataCelebritySummaries, cancellationToken: cancellationToken);
        return summaries;
    }

    internal static async Task DownloadMediaMetadataAsync(ISettings settings, ConcurrentDictionary<string, SkinMediaSummary>? summaries = null, bool useCache = true, Func<int, Range>? getRange = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        summaries ??= File.Exists(FileMetadataMediaSummaries)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinMediaSummary>>(FileMetadataMediaSummaries, cancellationToken)
            : [];

        Dictionary<string, string> cacheFiles = Directory
            .EnumerateFiles(DirectoryMetadataMediaCache)
            .ToDictionary(file => $"/{PathHelper.GetFileNameWithoutExtension(file)}");
        ConcurrentQueue<string> urls = new(summaries.Keys.Except(cacheFiles.Keys));
        if (getRange is not null)
        {
            urls = new(urls.Take(getRange(urls.Count)));
        }

        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new();
                    IPage page = await playWrightWrapper.PageAsync();
                    while (urls.TryDequeue(out string? url))
                    {
                        string html = string.Empty;
                        try
                        {
                            html = await Retry.FixedIntervalAsync(
                                async () => await page.GetStringAsync($"{BaseUrl}{url}", "#titleShowView", cancellationToken: token),
                                cancellationToken: token);
                        }
                        catch (HttpRequestException exception)
                        {
                            log($"Failed to download {url}: {exception.StatusCode} {exception.HttpRequestError} {exception.Message}");
                        }
                        catch (Exception exception)
                        {
                            log($"Failed to download {url}: {exception}");
                        }
                        finally
                        {
                            if (html.IsNotNullOrWhiteSpace())
                            {
                                await FileHelper.WriteTextAsync(Path.Combine(DirectoryMetadataMediaCache, $"{url.Trim('/')}{Video.ImdbCacheExtension}"), html, cancellationToken: token);
                            }
                        }
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);
    }

    internal static async Task DownloadMemberMediaMetadataAsync(ISettings settings, ConcurrentDictionary<string, SkinMediaSummary>? summaries = null, bool useCache = true, Func<int, Range>? getRange = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        summaries ??= File.Exists(FileMetadataMediaSummaries)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinMediaSummary>>(FileMetadataMediaSummaries, cancellationToken)
            : [];

        ConcurrentDictionary<string, SkinMediaMetadata> metadata = File.Exists(FileMetadataMedia)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinMediaMetadata>>(FileMetadataMedia, cancellationToken)
            : [];
        HashSet<string> metadataFiles = DirectoryHelper.GetFilesOrdinalIgnoreCase(DirectoryMetadataMedia);
        HashSet<string> cacheFiles = DirectoryHelper.GetFilesOrdinalIgnoreCase(DirectoryMetadataMediaCache);
        ConcurrentQueue<string> urls = new(summaries.Keys.Except(metadata.Keys).Order());
        if (getRange is not null)
        {
            urls = new(urls.Take(getRange(urls.Count)));
        }

        int downloadedCount = 0;
        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new(cookieFile: FileCookie);
                    while (urls.TryDequeue(out string? url))
                    {
                        metadata[url] = await DownloadMemberMediaMetadataAsync(url, metadataFiles, cacheFiles, useCache, playWrightWrapper, log, token);
                    }

                    if (Interlocked.Increment(ref downloadedCount) % WriteCount == 0)
                    {
                        JsonHelper.SerializeToFile(metadata, FileMetadataMedia, ref writeLock);
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        await JsonHelper.SerializeToFileAsync(metadata, FileMetadataMedia, cancellationToken: cancellationToken);
    }

    internal static async Task DownloadMemberCelebrityMetadataAsync(ISettings settings, ConcurrentDictionary<string, SkinCelebritySummary>? summaries = null, bool useCache = true, Func<int, Range>? getRange = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        summaries ??= File.Exists(FileMetadataCelebritySummaries)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinCelebritySummary>>(FileMetadataCelebritySummaries, cancellationToken)
            : [];

        ConcurrentDictionary<string, SkinCelebrityMetadata> metadata = File.Exists(FileMetadataCelebrities)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinCelebrityMetadata>>(FileMetadataCelebrities, cancellationToken)
            : [];
        HashSet<string> metadataFiles = DirectoryHelper.GetFilesOrdinalIgnoreCase(DirectoryMetadataCelebrities);
        HashSet<string> cacheFiles = DirectoryHelper.GetFilesOrdinalIgnoreCase(DirectoryMetadataCelebritiesCache);
        ConcurrentQueue<string> urls = new(summaries
            .Keys
            .Except(metadata.Keys)
            .Order());
        if (getRange is not null)
        {
            urls = new(urls.Take(getRange(urls.Count)));
        }

        int downloadedCount = 0;
        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new(cookieFile: FileCookie);
                    while (urls.TryDequeue(out string? url))
                    {
                        metadata[url] = await Retry.FixedIntervalAsync(
                            async () => await DownloadMemberCelebrityMetadataAsync(url, metadataFiles, cacheFiles, useCache, playWrightWrapper, log, token),
                            cancellationToken: token);
                    }

                    if (Interlocked.Increment(ref downloadedCount) % WriteCount == 0)
                    {
                        JsonHelper.SerializeToFile(metadata, FileMetadataCelebrities, ref writeLock);
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        await JsonHelper.SerializeToFileAsync(metadata, FileMetadataCelebrities, cancellationToken: cancellationToken);
    }

    private static async Task<SkinMediaMetadata> DownloadMemberMediaMetadataAsync(string url, HashSet<string> metadataFiles, HashSet<string> cacheFiles, bool useCache, PlayWrightWrapper playWrightWrapper, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        string fileName = url.Trim('/');
        Debug.Assert(!fileName.ContainsOrdinal(Video.Delimiter));
        string metadataFile = Path.Combine(DirectoryMetadataMedia, $"{fileName}{ImdbMetadata.Extension}");
        if (useCache && metadataFiles.Contains(metadataFile))
        {
            return await JsonHelper.DeserializeFromFileAsync<SkinMediaMetadata>(metadataFile, cancellationToken);
        }

        string clipsUrl = $"{BaseUrl}{url}/clips";
        string clipsFile = Path.Combine(DirectoryMetadataMediaCache, $"{fileName}{Video.Delimiter}Clips{Video.ImdbCacheExtension}");
        (string Content, string File)[] clipHtmls = await GetHtmlsAsync(clipsUrl, clipsFile, cacheFiles, useCache, playWrightWrapper, "#clips .thumbnails", cancellationToken: cancellationToken);
        foreach ((string Content, string File) html in clipHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
        {
            FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
        }

        SkinMediaClip[] clips = clipHtmls
            .Select(html =>
            {
                CQ pageCQ = CQ.CreateDocument(html.Content);
                CQ thumbnailsCQ = pageCQ.Find(".tab-content .thumbnails");
                CQ advertisementCQ = thumbnailsCQ.Children(".col-xs-12");
                Debug.Assert(advertisementCQ.Length is 0 or 1 or 2);
                advertisementCQ.Remove();

                return thumbnailsCQ
                    .Find(".thumbnail")
                    .Select(thumbnailDom =>
                    {
                        CQ thumbnailCQ = thumbnailDom.Cq();
                        CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                        string title = mediaCQ.Attr("data-modal-title-name");
                        Debug.Assert(title.IsNotNullOrWhiteSpace());
                        string url = mediaCQ.Attr("href");
                        Debug.Assert(url.IsNotNullOrWhiteSpace());
                        string image = thumbnailCQ.Find("img").Attr("src");
                        Debug.Assert(image.IsNotNullOrWhiteSpace());
                        Dictionary<string, string> names = thumbnailCQ
                            .Find(".title a")
                            .Select(linkDom => linkDom.Cq())
                            .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                        //Debug.Assert(names.Any());
                        int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                        Debug.Assert(rating > 0);
                        string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                        Debug.Assert(level.IsNotNullOrWhiteSpace());
                        string[] keywords = thumbnailCQ
                            .Find(".scene-keywords span:eq(1)")
                            .TextTrimDecode()
                            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        return new SkinMediaClip(title, url, image, names, rating, level, keywords);
                    });
            })
            .Concat()
            .ToArray();

        CQ clipsCQ = CQ.CreateDocument(clipHtmls.First().Content);
        CQ topCQ = clipsCQ.Find(".top-details-container");
        string image = topCQ.Find("img.media-object").Attr("src");
        CQ titleCQ = topCQ.Find("h1");
        CQ yearCQ = titleCQ.Children("span");
        string year = yearCQ.TextTrimDecode().TrimStart('(').TrimEnd(')');
        yearCQ.Remove();
        string title = titleCQ.TextTrimDecode();
        int rating = topCQ.Find(".star-rating i.active").Length;
        string ratingDescription = topCQ.Find(".rating-string").TextTrimDecode();
        string userRating = topCQ.Find(".rating-number").TextTrimDecode();
        Dictionary<string, string[]> details = topCQ.Find(".list-details li:has(.detail-type)")
            .Select(itemDom => itemDom.Cq())
            .ToLookup(itemCQ => itemCQ.Find(".detail-type").TextTrimDecode().TrimEnd(':', ' '), itemCQ => itemCQ.Find(".detail-info").TextTrimDecode())
            .ToDictionary(group => group.Key, group => group.ToArray());
        CQ blogCQ = topCQ.Find(".list-details li a:contains('Blog Post')");
        string blogUrl = blogCQ.Attr("href");
        string blog = blogCQ.TextTrimDecode();
        Debug.Assert(blog.IsNullOrWhiteSpace() || blog.ContainsIgnoreCase("Blog Post for this Title") || blog.ContainsIgnoreCase("Blog Posts for this Title"));
        int blogCount = blog.IsNullOrWhiteSpace() ? 0 : int.Parse(new Regex("[0-9]+").Match(blog).Value);
        string description = topCQ.Find(".description--expanded p").TextTrimDecode();

        string picturesUrl = $"{BaseUrl}{url}/pics";
        string picturesFile = Path.Combine(DirectoryMetadataMediaCache, $"{fileName}{Video.Delimiter}Pictures{Video.ImdbCacheExtension}");
        SkinMediaPicture[] pictures;
        if (clipsCQ.Find("#pics_tab").IsEmpty())
        {
            pictures = [];
        }
        else
        {
            (string Content, string File)[] pictureHtmls = await GetHtmlsAsync(
                picturesUrl, picturesFile, cacheFiles, useCache, playWrightWrapper, "#pics .thumbnails", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in pictureHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            pictures = pictureHtmls
                .Select(html =>
                {
                    CQ pageCQ = CQ.CreateDocument(html.Content);
                    CQ thumbnailsCQ = pageCQ.Find(".tab-content .thumbnails");
                    CQ advertisementCQ = thumbnailsCQ.Children(".col-xs-12");
                    Debug.Assert(advertisementCQ.Length is 0 or 1);
                    advertisementCQ.Remove();

                    return thumbnailsCQ
                        .Find(".thumbnail")
                        .Select(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                            string title = mediaCQ.Attr("data-modal-title-name");
                            Debug.Assert(title.IsNotNullOrWhiteSpace());
                            string url = mediaCQ.Attr("href");
                            Debug.Assert(url.IsNotNullOrWhiteSpace());
                            string image = thumbnailCQ.Find("img").Attr("src");
                            Debug.Assert(image.IsNotNullOrWhiteSpace());
                            Dictionary<string, string> names = thumbnailCQ
                                .Find(".title a")
                                .Select(linkDom => linkDom.Cq())
                                .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                            Debug.Assert(names.Any());
                            string @as = thumbnailCQ.Find(".role-type").TextTrimDecode();
                            if (@as.StartsWithIgnoreCase("- As "))
                            {
                                @as = @as["- As ".Length..];
                            }
                            else
                            {
                                Debug.Assert(@as.IsNullOrWhiteSpace());
                                @as = string.Empty;
                            }

                            string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                            Debug.Assert(level.IsNotNullOrWhiteSpace());
                            string[] keywords = thumbnailCQ
                                .Find(".scene-keywords span:eq(1)")
                                .TextTrimDecode()
                                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            return new SkinMediaPicture(title, url, image, names, @as, level, keywords);
                        });
                })
                .Concat()
                .ToArray();
        }

        string celebritiesUrl = $"{BaseUrl}{url}/celebs";
        string celebritiesFile = Path.Combine(DirectoryMetadataMediaCache, $"{fileName}{Video.Delimiter}Celebrities{Video.ImdbCacheExtension}");
        SkinCelebrity[] celebrities;
        if (clipsCQ.Find("#celebs_tab").IsEmpty())
        {
            celebrities = [];
        }
        else
        {
            (string Content, string File)[] celebrityHtmls = await GetHtmlsAsync(
                celebritiesUrl,
                celebritiesFile,
                cacheFiles,
                useCache,
                playWrightWrapper,
                "#appearances",
                async page => await page.ClickOrPressAsync("button.drawer-toggle", unload: () => page.Locator(".loading-icon"), cancellationToken: cancellationToken),
                cancellationToken);
            foreach ((string Content, string File) html in celebrityHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            celebrities = celebrityHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".tab-content > .tab-pane > .appearances-tab-pane")
                    .Select(celebrityDom =>
                    {
                        CQ celebrityCQ = celebrityDom.Cq();
                        string image = celebrityCQ.Find("img").Attr("src");
                        CQ linkCQ = celebrityCQ.Find("a.media-title");
                        string name = linkCQ.TextTrimDecode();
                        string url = linkCQ.Attr("href");
                        string level = celebrityCQ.Find(".appearance-character span:eq(0)").TextTrimDecode();
                        string @as = celebrityCQ.Find(".appearance-character span:eq(1)").TextTrimDecode();
                        SkinMediaClip[] clips = celebrityCQ
                            .Find(".thumbnail.clip")
                            .Select(thumbnailDom =>
                            {
                                CQ thumbnailCQ = thumbnailDom.Cq();
                                CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                                string title = mediaCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = mediaCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = thumbnailCQ.Find("img").Attr("src");
                                Debug.Assert(image.IsNotNullOrWhiteSpace());
                                Dictionary<string, string> names = thumbnailCQ
                                    .Find(".title a")
                                    .Select(linkDom => linkDom.Cq())
                                    .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                                //Debug.Assert(names.Any());
                                int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                                Debug.Assert(rating > 0);
                                string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                Debug.Assert(level.IsNotNullOrWhiteSpace());
                                string[] keywords = thumbnailCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                return new SkinMediaClip(title, url, image, names, rating, level, keywords);
                            })
                            .ToArray();
                        SkinMediaPicture[] pictures = celebrityCQ
                            .Find(".thumbnail.pic")
                            .Select(thumbnailDom =>
                            {
                                CQ thumbnailCQ = thumbnailDom.Cq();
                                CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                                string title = mediaCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = mediaCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = thumbnailCQ.Find("img").Attr("src");
                                Debug.Assert(image.IsNotNullOrWhiteSpace());
                                Dictionary<string, string> names = thumbnailCQ
                                    .Find(".title a")
                                    .Select(linkDom => linkDom.Cq())
                                    .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                                //Debug.Assert(names.Any());
                                string @as = thumbnailCQ.Find(".role-type").TextTrimDecode();
                                if (@as.StartsWithIgnoreCase("- As "))
                                {
                                    @as = @as["- As ".Length..];
                                }
                                else
                                {
                                    Debug.Assert(@as.IsNullOrWhiteSpace());
                                    @as = string.Empty;
                                }

                                string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                Debug.Assert(level.IsNotNullOrWhiteSpace());
                                string[] keywords = thumbnailCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                return new SkinMediaPicture(title, url, image, names, @as, level, keywords);
                            })
                            .ToArray();

                        return new SkinCelebrity(name, url, image, level, @as, clips, pictures);
                    }))
                .Concat()
                .ToArray();
        }

        string celebrityScenesUrl = $"{BaseUrl}{url}/nude_scene_guide";
        string celebrityScenesFile = Path.Combine(DirectoryMetadataMediaCache, $"{fileName}{Video.Delimiter}Scenes{Video.ImdbCacheExtension}");
        SkinCelebrityScenes[] celebrityScenes;
        if (clipsCQ.Find("#nude_scene_guide_tab").IsEmpty())
        {
            celebrityScenes = [];
        }
        else
        {
            (string Content, string File)[] celebritySceneHtmls = await GetHtmlsAsync(
                celebrityScenesUrl, celebrityScenesFile, cacheFiles, useCache, playWrightWrapper, "#nude_scene_guide", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in celebritySceneHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            celebrityScenes = celebritySceneHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".tab-content .nude-scene-content-group")
                    .Select(celebrityDom =>
                    {
                        CQ celebrityCQ = celebrityDom.Cq();
                        CQ linkCQ = celebrityCQ.Find("a:eq(0)");
                        string name = linkCQ.TextTrimDecode();
                        string url = linkCQ.Attr("href");
                        string level = celebrityCQ.Find(".character span:eq(0)").TextTrimDecode();
                        string @as = celebrityCQ.Find(".character span:eq(1)").TextTrimDecode();
                        SkinScene[] scenes = celebrityCQ
                            .Find(".nude-scene-content")
                            .Select(sceneDom =>
                            {
                                CQ sceneCQ = sceneDom.Cq();
                                CQ linkCQ = sceneCQ.Find("a:eq(0)");
                                string title = linkCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = linkCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = sceneCQ.Find("img").Attr("src");
                                int rating = sceneCQ.Find(".star-rating i.active").Length;
                                string level = sceneCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                string[] keywords = sceneCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                CQ descriptionCQ = sceneCQ.Find(".scene-description");
                                CQ positionCQ = descriptionCQ.Find("span:eq(0)");
                                string position = positionCQ.TextTrimDecode();
                                positionCQ.Remove();
                                string description = descriptionCQ.TextTrimDecode();
                                return new SkinScene(title, url, image, rating, level, keywords, position, description);
                            })
                            .ToArray();
                        return new SkinCelebrityScenes(name, url, level, @as, scenes);
                    }))
                .Concat()
                .ToArray();
        }

        string episodesUrl = $"{BaseUrl}{url}/episode_guide";
        string episodesFile = Path.Combine(DirectoryMetadataMediaCache, $"{fileName}{Video.Delimiter}Episodes{Video.ImdbCacheExtension}");
        SkinEpisode[] episodes;
        if (clipsCQ.Find("#episode_guide_tab").IsEmpty())
        {
            episodes = [];
        }
        else
        {
            (string Content, string File)[] episodeHtmls = await GetHtmlsAsync(
                episodesUrl, episodesFile, cacheFiles, useCache, playWrightWrapper, "#episode_guide", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in episodeHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            episodes = episodeHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".episode-item")
                    .Select(episodeDom =>
                    {
                        CQ episodeCQ = episodeDom.Cq();
                        string title = episodeCQ.Children("h2").TextTrimDecode();
                        string description = string.Join(Environment.NewLine, episodeCQ.Children("p.MsoNormal").Select(dom => dom.TextTrimDecode()));
                        SkinMediaClip[] clips = episodeCQ
                            .Find(".thumbnail.clip")
                            .Select(thumbnailDom =>
                            {
                                CQ thumbnailCQ = thumbnailDom.Cq();
                                CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                                string title = mediaCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = mediaCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = thumbnailCQ.Find("img").Attr("src");
                                Debug.Assert(image.IsNotNullOrWhiteSpace());
                                Dictionary<string, string> names = thumbnailCQ
                                    .Find(".title a")
                                    .Select(linkDom => linkDom.Cq())
                                    .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                                //Debug.Assert(names.Any());
                                int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                                Debug.Assert(rating > 0);
                                string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                Debug.Assert(level.IsNotNullOrWhiteSpace());
                                string[] keywords = thumbnailCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                return new SkinMediaClip(title, url, image, names, rating, level, keywords);
                            })
                            .ToArray();
                        SkinMediaPicture[] pictures = episodeCQ
                            .Find(".thumbnail")
                            .Select(thumbnailDom =>
                            {
                                CQ thumbnailCQ = thumbnailDom.Cq();
                                CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                                string title = mediaCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = mediaCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = thumbnailCQ.Find("img").Attr("src");
                                Debug.Assert(image.IsNotNullOrWhiteSpace());
                                Dictionary<string, string> names = thumbnailCQ
                                    .Find(".title a")
                                    .Select(linkDom => linkDom.Cq())
                                    .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                                //Debug.Assert(names.Any());
                                string @as = thumbnailCQ.Find(".role-type").TextTrimDecode();
                                if (@as.StartsWithIgnoreCase("- As "))
                                {
                                    @as = @as["- As ".Length..];
                                }
                                else
                                {
                                    Debug.Assert(@as.IsNullOrWhiteSpace());
                                    @as = string.Empty;
                                }

                                string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                Debug.Assert(level.IsNotNullOrWhiteSpace());
                                string[] keywords = thumbnailCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                return new SkinMediaPicture(title, url, image, names, @as, level, keywords);
                            })
                            .ToArray();
                        return new SkinEpisode(title, description, clips, pictures);
                    }))
                .Concat()
                .ToArray();
        }

        SkinMediaMetadata metadata = new(title, url, image, year, rating, ratingDescription, userRating, description, blogCount, blogUrl, details, clips, pictures, celebrities, celebrityScenes, episodes);
        JsonHelper.SerializeToFile(metadata, metadataFile, ref writeLock, true);
        return metadata;
    }

    private static async Task<SkinCelebrityMetadata> DownloadMemberCelebrityMetadataAsync(string url, HashSet<string> metadataFiles, HashSet<string> cacheFiles, bool useCache, PlayWrightWrapper playWrightWrapper, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        string fileName = url.Trim('/');
        Debug.Assert(!fileName.ContainsOrdinal(Video.Delimiter));
        string metadataFile = Path.Combine(DirectoryMetadataCelebrities, $"{fileName}{ImdbMetadata.Extension}");
        if (useCache && metadataFiles.Contains(metadataFile))
        {
            return await JsonHelper.DeserializeFromFileAsync<SkinCelebrityMetadata>(metadataFile, cancellationToken);
        }

        string clipsUrl = $"{BaseUrl}{url}/clips";
        string clipsFile = Path.Combine(DirectoryMetadataCelebritiesCache, $"{fileName}{Video.Delimiter}Clips{Video.ImdbCacheExtension}");
        (string Content, string File)[] clipHtmls = await GetHtmlsAsync(clipsUrl, clipsFile, cacheFiles, useCache, playWrightWrapper, "#clips .thumbnails", cancellationToken: cancellationToken);
        foreach ((string Content, string File) html in clipHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
        {
            FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
        }

        SkinCelebrityClip[] clips = clipHtmls
            .Select(html =>
            {
                CQ pageCQ = CQ.CreateDocument(html.Content);
                CQ thumbnailsCQ = pageCQ.Find(".tab-content .thumbnails");
                CQ advertisementCQ = thumbnailsCQ.Children(".col-xs-12");
                Debug.Assert(advertisementCQ.Length is 0 or 1 or 2);
                advertisementCQ.Remove();

                return thumbnailsCQ
                    .Find(".thumbnail")
                    .Select(thumbnailDom =>
                    {
                        CQ thumbnailCQ = thumbnailDom.Cq();
                        CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                        string title = mediaCQ.Attr("data-modal-title-name");
                        Debug.Assert(title.IsNotNullOrWhiteSpace());
                        string url = mediaCQ.Attr("href");
                        Debug.Assert(url.IsNotNullOrWhiteSpace());
                        string image = thumbnailCQ.Find("img").Attr("src");
                        Debug.Assert(image.IsNotNullOrWhiteSpace());
                        string year = thumbnailCQ.Find(".caption a.title").TextTrimDecode();
                        if (year.StartsWithIgnoreCase(title))
                        {
                            year = year[title.Length..];
                        }

                        Match match = Regex.Match(year, @" \(([0-9\-]{4,})\)$");
                        year = match.Success ? match.Groups[1].Value : string.Empty;
                        int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                        Debug.Assert(rating > 0);
                        string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                        Debug.Assert(level.IsNotNullOrWhiteSpace());
                        string[] keywords = thumbnailCQ
                            .Find(".scene-keywords span:eq(1)")
                            .TextTrimDecode()
                            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        return new SkinCelebrityClip(title, url, image, year, rating, level, keywords);
                    });
            })
            .Concat()
            .ToArray();

        CQ clipsCQ = CQ.CreateDocument(clipHtmls.First().Content);
        CQ topCQ = clipsCQ.Find(".top-details-container");
        string image = topCQ.Find("img.media-object").Attr("src");
        CQ titleCQ = topCQ.Find("h1");
        CQ yearCQ = titleCQ.Children("span");
        string year = yearCQ.TextTrimDecode().TrimStart('(').TrimEnd(')');
        yearCQ.Remove();
        string title = titleCQ.TextTrimDecode();
        int rating = topCQ.Find(".star-rating i.active").Length;
        string ratingDescription = topCQ.Find(".rating-string").TextTrimDecode();
        string userRating = topCQ.Find(".rating-number").TextTrimDecode();
        Dictionary<string, string[]> details = topCQ.Find(".list-details li:has(.detail-type)")
            .Select(itemDom => itemDom.Cq())
            .ToLookup(itemCQ => itemCQ.Find(".detail-type").TextTrimDecode().TrimEnd(':', ' '), itemCQ => itemCQ.Find(".detail-info").TextTrimDecode())
            .ToDictionary(group => group.Key, group => group.ToArray());
        CQ blogCQ = topCQ.Find(".list-details li a:contains('Blog Post')");
        string blogUrl = blogCQ.Attr("href");
        string blog = blogCQ.TextTrimDecode();
        Debug.Assert(blog.IsNullOrWhiteSpace() || blog.ContainsIgnoreCase("Blog Post for this celebrity") || blog.ContainsIgnoreCase("Blog Posts for this celebrity"));
        int blogCount = blog.IsNullOrWhiteSpace() ? 0 : int.Parse(new Regex("[0-9]+").Match(blog).Value);
        string description = topCQ.Find(".description--expanded p").TextTrimDecode();

        string picturesUrl = $"{BaseUrl}{url}/pics";
        string picturesFile = Path.Combine(DirectoryMetadataCelebritiesCache, $"{fileName}{Video.Delimiter}Pictures{Video.ImdbCacheExtension}");
        SkinCelebrityPicture[] pictures;
        if (clipsCQ.Find("#pics_tab").IsEmpty())
        {
            pictures = [];
        }
        else
        {
            (string Content, string File)[] pictureHtmls = await GetHtmlsAsync(
                picturesUrl, picturesFile, cacheFiles, useCache, playWrightWrapper, "#pics .thumbnails", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in pictureHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            pictures = pictureHtmls
                .Select(html =>
                {
                    CQ pageCQ = CQ.CreateDocument(html.Content);
                    CQ thumbnailsCQ = pageCQ.Find(".tab-content .thumbnails");
                    CQ advertisementCQ = thumbnailsCQ.Children(".col-xs-12");
                    Debug.Assert(advertisementCQ.Length is 0 or 1);
                    advertisementCQ.Remove();

                    return thumbnailsCQ
                        .Find(".thumbnail")
                        .Select(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                            string title = mediaCQ.Attr("data-modal-title-name");
                            Debug.Assert(title.IsNotNullOrWhiteSpace());
                            string url = mediaCQ.Attr("href");
                            Debug.Assert(url.IsNotNullOrWhiteSpace());
                            string image = thumbnailCQ.Find("img").Attr("src");
                            Debug.Assert(image.IsNotNullOrWhiteSpace());
                            string year = thumbnailCQ.Find(".caption a.title").TextTrimDecode();
                            if (year.StartsWithIgnoreCase(title))
                            {
                                year = year[title.Length..];
                            }

                            Match match = Regex.Match(year, @" \(([0-9\-]{4,})\)$");
                            year = match.Success ? match.Groups[1].Value : string.Empty;
                            string @as = thumbnailCQ.Find(".role-type").TextTrimDecode();
                            if (@as.StartsWithIgnoreCase("- As "))
                            {
                                @as = @as["- As ".Length..];
                            }
                            else
                            {
                                Debug.Assert(@as.IsNullOrWhiteSpace());
                                @as = string.Empty;
                            }

                            string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                            Debug.Assert(level.IsNotNullOrWhiteSpace());
                            string[] keywords = thumbnailCQ
                                .Find(".scene-keywords span:eq(1)")
                                .TextTrimDecode()
                                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            return new SkinCelebrityPicture(title, url, image, year, @as, level, keywords);
                        });
                })
                .Concat()
                .ToArray();
        }

        string celebritiesUrl = $"{BaseUrl}{url}/titles";
        string celebritiesFile = Path.Combine(DirectoryMetadataCelebritiesCache, $"{fileName}{Video.Delimiter}Titles{Video.ImdbCacheExtension}");
        SkinTitle[] celebrities;
        if (clipsCQ.Find("#titles_tab").IsEmpty())
        {
            celebrities = [];
        }
        else
        {
            (string Content, string File)[] celebrityHtmls = await GetHtmlsAsync(
                celebritiesUrl,
                celebritiesFile,
                cacheFiles,
                useCache,
                playWrightWrapper,
                "#appearances",
                async page => await page.ClickOrPressAsync("button.drawer-toggle", unload: () => page.Locator(".loading-icon"), cancellationToken: cancellationToken),
                cancellationToken);
            foreach ((string Content, string File) html in celebrityHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            celebrities = celebrityHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".tab-content > .tab-pane > .appearances-tab-pane")
                    .Select(celebrityDom =>
                    {
                        CQ celebrityCQ = celebrityDom.Cq();
                        string image = celebrityCQ.Find("img").Attr("src");
                        CQ linkCQ = celebrityCQ.Find("a.media-title");
                        CQ yearCQ = linkCQ.Find(".text-muted");
                        string year = yearCQ.TextTrimDecode().TrimStart('(').TrimEnd(')');
                        yearCQ.Remove();
                        string name = linkCQ.TextTrimDecode();
                        string url = linkCQ.Attr("href");
                        string level = celebrityCQ.Find(".appearance-character span:eq(0)").TextTrimDecode();
                        string @as = celebrityCQ.Find(".appearance-character span:eq(1)").TextTrimDecode();
                        SkinMediaClip[] clips = celebrityCQ
                            .Find(".thumbnail.clip")
                            .Select(thumbnailDom =>
                            {
                                CQ thumbnailCQ = thumbnailDom.Cq();
                                CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                                string title = mediaCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = mediaCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = thumbnailCQ.Find("img").Attr("src");
                                Debug.Assert(image.IsNotNullOrWhiteSpace());
                                Dictionary<string, string> names = thumbnailCQ
                                    .Find(".title a")
                                    .Select(linkDom => linkDom.Cq())
                                    .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                                //Debug.Assert(names.Any());
                                int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                                Debug.Assert(rating > 0);
                                string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                Debug.Assert(level.IsNotNullOrWhiteSpace());
                                string[] keywords = thumbnailCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                return new SkinMediaClip(title, url, image, names, rating, level, keywords);
                            })
                            .ToArray();
                        SkinMediaPicture[] pictures = celebrityCQ
                            .Find(".thumbnail.pic")
                            .Select(thumbnailDom =>
                            {
                                CQ thumbnailCQ = thumbnailDom.Cq();
                                CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                                string title = mediaCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = mediaCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = thumbnailCQ.Find("img").Attr("src");
                                Debug.Assert(image.IsNotNullOrWhiteSpace());
                                Dictionary<string, string> names = thumbnailCQ
                                    .Find(".title a")
                                    .Select(linkDom => linkDom.Cq())
                                    .ToDictionary(linkCQ => linkCQ.Attr("href"), linkCQ => linkCQ.TextTrimDecode());
                                //Debug.Assert(names.Any());
                                string @as = thumbnailCQ.Find(".role-type").TextTrimDecode();
                                if (@as.StartsWithIgnoreCase("- As "))
                                {
                                    @as = @as["- As ".Length..];
                                }
                                else
                                {
                                    Debug.Assert(@as.IsNullOrWhiteSpace());
                                    @as = string.Empty;
                                }

                                string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                Debug.Assert(level.IsNotNullOrWhiteSpace());
                                string[] keywords = thumbnailCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                return new SkinMediaPicture(title, url, image, names, @as, level, keywords);
                            })
                            .ToArray();

                        return new SkinTitle(name, url, year, image, level, @as, clips, pictures);
                    }))
                .Concat()
                .ToArray();
        }

        string celebrityScenesUrl = $"{BaseUrl}{url}/nude_scene_guide";
        string celebrityScenesFile = Path.Combine(DirectoryMetadataCelebritiesCache, $"{fileName}{Video.Delimiter}Scenes{Video.ImdbCacheExtension}");
        SkinTitleScenes[] celebrityScenes;
        if (clipsCQ.Find("#nude_scene_guide_tab").IsEmpty())
        {
            celebrityScenes = [];
        }
        else
        {
            (string Content, string File)[] celebritySceneHtmls = await GetHtmlsAsync(
                celebrityScenesUrl, celebrityScenesFile, cacheFiles, useCache, playWrightWrapper, "#nude_scene_guide", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in celebritySceneHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            celebrityScenes = celebritySceneHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".tab-content .nude-scene-content-group")
                    .Select(celebrityDom =>
                    {
                        CQ celebrityCQ = celebrityDom.Cq();
                        CQ linkCQ = celebrityCQ.Find("a:eq(0)");
                        string name = linkCQ.TextTrimDecode();
                        string url = linkCQ.Attr("href");
                        string level = celebrityCQ.Find(".character span:eq(0)").TextTrimDecode();
                        string @as = celebrityCQ.Find(".character span:eq(1)").TextTrimDecode();
                        SkinScene[] scenes = celebrityCQ
                            .Find(".nude-scene-content")
                            .Select(sceneDom =>
                            {
                                CQ sceneCQ = sceneDom.Cq();
                                CQ linkCQ = sceneCQ.Find("a:eq(0)");
                                string title = linkCQ.Attr("data-modal-title-name");
                                Debug.Assert(title.IsNotNullOrWhiteSpace());
                                string url = linkCQ.Attr("href");
                                Debug.Assert(url.IsNotNullOrWhiteSpace());
                                string image = sceneCQ.Find("img").Attr("src");
                                int rating = sceneCQ.Find(".star-rating i.active").Length;
                                string level = sceneCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                                string[] keywords = sceneCQ
                                    .Find(".scene-keywords span:eq(1)")
                                    .TextTrimDecode()
                                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                CQ descriptionCQ = sceneCQ.Find(".scene-description");
                                CQ positionCQ = descriptionCQ.Find("span:eq(0)");
                                string position = positionCQ.TextTrimDecode();
                                positionCQ.Remove();
                                string description = descriptionCQ.TextTrimDecode();
                                return new SkinScene(title, url, image, rating, level, keywords, position, description);
                            })
                            .ToArray();
                        return new SkinTitleScenes(name, url, level, @as, scenes);
                    }))
                .Concat()
                .ToArray();
        }

        string playlistsUrl = $"{BaseUrl}{url}/playlists";
        string playlistsFile = Path.Combine(DirectoryMetadataCelebritiesCache, $"{fileName}{Video.Delimiter}Playlists{Video.ImdbCacheExtension}");
        SkinPlaylist[] playlists;
        if (clipsCQ.Find("#playlists_tab").IsEmpty())
        {
            playlists = [];
        }
        else
        {
            (string Content, string File)[] playlistHtmls = await GetHtmlsAsync(
                playlistsUrl, playlistsFile, cacheFiles, useCache, playWrightWrapper, "#playlists", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in playlistHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            playlists = playlistHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".playlist")
                    .Select(playlistDom =>
                    {
                        CQ playlistCQ = playlistDom.Cq();
                        CQ linkCQ = playlistCQ.Find(".caption a:eq(0)");
                        string url = linkCQ.Attr("href");
                        string image = playlistCQ.Find("img:eq(0)").Attr("src");
                        string title = linkCQ.TextTrimDecode();
                        string duration = playlistCQ.Find(".caption small").TextTrimDecode();
                        Match match = Regex.Match(duration, @" \(([0-9]+) Scenes\)");
                        int count;
                        if (match.Success)
                        {
                            count = int.Parse(match.Groups[1].Value);
                            duration = duration[..match.Index].Trim();
                        }
                        else
                        {
                            count = 0;
                        }

                        return new SkinPlaylist(title, url, image, duration, count);
                    }))
                .Concat()
                .ToArray();
        }

        string videosUrl = $"{BaseUrl}{url}/original_videos";
        string videosFile = Path.Combine(DirectoryMetadataCelebritiesCache, $"{fileName}{Video.Delimiter}Videos{Video.ImdbCacheExtension}");
        SkinVideo[] videos;
        if (clipsCQ.Find("#original_videos_tab").IsEmpty())
        {
            videos = [];
        }
        else
        {
            (string Content, string File)[] videoHtmls = await GetHtmlsAsync(
                videosUrl, videosFile, cacheFiles, useCache, playWrightWrapper, "#original_videos", cancellationToken: cancellationToken);
            foreach ((string Content, string File) html in videoHtmls.Where(html => html.File.IsNotNullOrWhiteSpace()))
            {
                FileHelper.WriteText(html.File, html.Content, ref writeLock, true);
            }

            videos = videoHtmls
                .Select(html => CQ.CreateDocument(html.Content)
                    .Find(".thumbnail")
                    .Select(videoDom =>
                    {
                        CQ videoCQ = videoDom.Cq();
                        CQ linkCQ = videoCQ.Find(".caption a:eq(0)");
                        string url = linkCQ.Attr("href");
                        string image = videoCQ.Find("img:eq(0)").Attr("src");
                        string title = linkCQ.TextTrimDecode();
                        string[] keywords = videoCQ
                            .Find(".caption .text-muted")
                            .Select(dom => dom.TextTrimDecode())
                            .Where(text => text.IsNotNullOrWhiteSpace())
                            .ToArray();

                        return new SkinVideo(title, url, image, keywords);
                    }))
                .Concat()
                .ToArray();
        }

        SkinCelebrityMetadata metadata = new(
            title, url, image, year, rating, ratingDescription, userRating, description, blogCount, blogUrl, details,
            clips, pictures, celebrities, celebrityScenes, playlists, videos);
        JsonHelper.SerializeToFile(metadata, metadataFile, ref writeLock, true);
        return metadata;
    }

    private static async Task<(string Content, string File)[]> GetHtmlsAsync(string url, string fileInitial, HashSet<string> cacheFiles, bool useCache, PlayWrightWrapper playWrightWrapper, string selector, Func<IPage, ValueTask>? update = null, CancellationToken cancellationToken = default)
    {
        IPage page = await playWrightWrapper.PageAsync();
        (string Content, string File) firstHtml;
        string firstFile = PathHelper.AddFilePostfix(fileInitial, $"{Video.Delimiter}1");
        if (cacheFiles.Contains(firstFile))
        {
            if (await FileHelper.TextEndsWithIgnoreCaseAsync(firstFile, "</html>", cancellationToken: cancellationToken))
            {
                if (useCache)
                {
                    firstHtml = (await File.ReadAllTextAsync(firstFile, cancellationToken), string.Empty);
                }
                else
                {
                    firstHtml = (await page.GetStringAsync(url, selector, cancellationToken: cancellationToken), firstFile);
                    if (update is not null)
                    {
                        await update(page);
                        firstHtml = (await page.ContentAsync(), firstFile);
                    }
                }
            }
            else
            {
                FileHelper.Recycle(firstFile);
                cacheFiles.Remove(firstFile);
                firstHtml = (await page.GetStringAsync(url, selector, cancellationToken: cancellationToken), firstFile);
                if (update is not null)
                {
                    await update(page);
                    firstHtml = (await page.ContentAsync(), firstFile);
                }
            }
        }
        else
        {
            firstHtml = (await page.GetStringAsync(url, selector, cancellationToken: cancellationToken), firstFile);
            if (update is not null)
            {
                await update(page);
                firstHtml = (await page.ContentAsync(), firstFile);
            }
        }

        //ILocator? paginationLocator = (await page.Locator("nav.pagination .last a").AllAsync()).SingleOrDefault();
        //string lastPageUrl = paginationLocator is null ? string.Empty : await paginationLocator.TextContentAsync() ?? string.Empty;
        CQ pageCQ = CQ.CreateDocument(firstHtml.Content);
        string lastPageUrl = pageCQ.Find("nav.pagination .last a").Attr("href");
        int lastPageNumber = lastPageUrl.IsNullOrWhiteSpace()
            ? 1
            : int.Parse(new Regex(@"page\=([0-9]+)", RegexOptions.IgnoreCase).Match(lastPageUrl).Groups[1].Value);

        return await AsyncEnumerable
            .Range(2, lastPageNumber - 1)
            .Select(async (pageNumber, _, token) =>
            {
                url = $"{url}?page={pageNumber}";
                string file = PathHelper.AddFilePostfix(fileInitial, $"{Video.Delimiter}{pageNumber}");
                (string Content, string File) html;
                if (cacheFiles.Contains(file))
                {
                    if (await FileHelper.TextEndsWithIgnoreCaseAsync(file, "</html>", cancellationToken: token))
                    {
                        if (useCache)
                        {
                            html = (await File.ReadAllTextAsync(file, token), string.Empty);
                        }
                        else
                        {
                            html = (await page.GetStringAsync(url, selector, cancellationToken: token), file);
                            if (update is not null)
                            {
                                await update(page);
                                html = (await page.ContentAsync(), file);
                            }
                        }
                    }
                    else
                    {
                        FileHelper.Recycle(file);
                        cacheFiles.Remove(file);
                        html = (await page.GetStringAsync(url, selector, cancellationToken: token), file);
                        if (update is not null)
                        {
                            await update(page);
                            html = (await page.ContentAsync(), file);
                        }
                    }
                }
                else
                {
                    html = (await page.GetStringAsync(url, selector, cancellationToken: token), file);
                    if (update is not null)
                    {
                        await update(page);
                        html = (await page.ContentAsync(), file);
                    }
                }

                return html;
            })
            .Prepend(firstHtml)
            .ToArrayAsync(cancellationToken);
    }
}