namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using MediaManager.IO;
using Microsoft.Playwright;

internal static class Skin
{
    private static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 1);

    private const int WriteCount = 100;

    internal static async Task<ConcurrentDictionary<string, SkinSummary>> DownloadSummariesAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentQueue<int> pageIndexes = new(Enumerable.Range(1, 1330));
        ConcurrentDictionary<string, SkinSummary> summaries = [];
        await Enumerable.Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new();
                    IPage page = await playWrightWrapper.PageAsync();
                    while (pageIndexes.TryDequeue(out int pageIndex))
                    {
                        string url = $"https://www.mrskin.com/search/titles?page={pageIndex}&sort=most_recent";
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
                            summaries[url] = new SkinSummary(title, url, year, image, rating);
                        });
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        await JsonHelper.SerializeToFileAsync(summaries, @"D:\Files\Library\Skin.Summaries.json", cancellationToken);
        return summaries;
    }

    internal static async Task DownloadDetails(ISettings settings, ConcurrentDictionary<string, SkinSummary>? summaries = null, bool useCache = true, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        string skinMetadataCacheDirectory = @"D:\Files\Library\SkinMetadataCache";
        string skinMetadataDirectory = @"D:\Files\Library\SkinMetadata";
        string detailsFile = @"D:\Files\Library\Skin.Details.json";

        summaries ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinSummary>>(@"D:\Files\Library\Skin.Summary.json", cancellationToken);
        ConcurrentDictionary<string, SkinMetadata> details = File.Exists(detailsFile)
            ? await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, SkinMetadata>>(detailsFile, cancellationToken)
            : [];
        HashSet<string> metadataFiles = DirectoryHelper.GetFilesOrdinalIgnoreCase(skinMetadataDirectory);
        HashSet<string> cacheFiles = DirectoryHelper.GetFilesOrdinalIgnoreCase(skinMetadataCacheDirectory);
        ConcurrentQueue<string> urls = new(summaries.Keys.Except(details.Keys));
        Lock writeJsonLock = new();
        int downloadedCount = 0;
        await Enumerable
            .Range(0, MaxDegreeOfParallelism)
            .ParallelForEachAsync(async (index, _, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new();
                    while (urls.TryDequeue(out string? url))
                    {
                        details[url] = await DownloadDetailAsync(settings, url, metadataFiles, cacheFiles, useCache, playWrightWrapper, log, token);
                    }

                    if (Interlocked.Increment(ref downloadedCount) % WriteCount == 0)
                    {
                        JsonHelper.SerializeToFile(details, detailsFile, ref writeJsonLock);
                    }
                },
                MaxDegreeOfParallelism,
                cancellationToken);

        await JsonHelper.SerializeToFileAsync(details, detailsFile, cancellationToken);
    }

    private static async Task<SkinMetadata> DownloadDetailAsync(ISettings settings, string url, HashSet<string> metadataFiles, HashSet<string> cacheFiles, bool useCache, PlayWrightWrapper playWrightWrapper, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        string skinMetadataDirectory = @"D:\Files\Library\SkinMetadata";
        string skinMetadataCacheDirectory = @"D:\Files\Library\SkinMetadataCache";

        string fileName = url.Trim('/');
        string metadataFile = Path.Combine(skinMetadataDirectory, $"{fileName}{ImdbMetadata.Extension}");
        if (useCache && metadataFiles.Contains(metadataFile))
        {
            return await JsonHelper.DeserializeFromFileAsync<SkinMetadata>(metadataFile, cancellationToken);
        }

        string clipsUrl = $"https://www.mrskin.com{url}/clips";
        string clipsFile = Path.Combine(skinMetadataCacheDirectory, $"{fileName}.Clips{Video.ImdbCacheExtension}");
        string[] clipsHtml = await GetHtmlAsync(clipsUrl, clipsFile, cacheFiles, useCache, playWrightWrapper, "#clips .thumbnails", cancellationToken: cancellationToken);
        SkinClip[] clips = clipsHtml
            .Select(html =>
            {
                CQ pageCQ = CQ.CreateDocument(html);
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
                        string title = (string)mediaCQ.Data("modal-title-name");
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
                        int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                        Debug.Assert(rating > 0);
                        string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                        Debug.Assert(level.IsNotNullOrWhiteSpace());
                        string[] keywords = thumbnailCQ
                            .Find(".scene-keywords span:eq(1)")
                            .TextTrimDecode()
                            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        Debug.Assert(keywords.Any());
                        return new SkinClip(title, url, image, names, rating, level, keywords);
                    });
            })
            .Concat()
            .ToArray();

        CQ clipsCQ = CQ.CreateDocument(clipsHtml[0]);
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
        Dictionary<string, string> details = topCQ.Find(".list-details li:has(.detail-type)")
            .Select(itemDom => itemDom.Cq())
            .ToDictionary(itemCQ => itemCQ.Find(".detail-type").TextTrimDecode(), itemCQ => itemCQ.Find(".detail-info").TextTrimDecode());
        CQ blogCQ = topCQ.Find(".list-details li a:contains('Blog Posts for this Title')");
        string blogUrl = blogCQ.Attr("href");
        string blog = blogCQ.TextTrimDecode();
        int blogCount = blog.IsNullOrWhiteSpace() ? 0 : int.Parse(new Regex("[0-0]+").Match(blog).Value);
        string description = topCQ.Find(".description--expanded p").TextTrimDecode();

        string picturesUrl = $"https://www.mrskin.com{url}/pics";
        string picturesFile = Path.Combine(skinMetadataCacheDirectory, $"{fileName}.Pictures{Video.ImdbCacheExtension}");
        SkinPicture[] pictures = clipsCQ.Find("#pics_tab").IsEmpty()
            ? []
            : (await GetHtmlAsync(picturesUrl, picturesFile, cacheFiles, useCache, playWrightWrapper, "#pics .thumbnails", cancellationToken: cancellationToken))
            .Select(html =>
            {
                CQ pageCQ = CQ.CreateDocument(html);
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
                        string title = (string)mediaCQ.Data("modal-title-name");
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
                        Debug.Assert(@as.StartsWithIgnoreCase("- As "));
                        @as = @as.ReplaceIgnoreCase("- As ", string.Empty);
                        string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                        Debug.Assert(level.IsNotNullOrWhiteSpace());
                        string[] keywords = thumbnailCQ
                            .Find(".scene-keywords span:eq(1)")
                            .TextTrimDecode()
                            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        Debug.Assert(keywords.Any());
                        return new SkinPicture(title, url, image, names, @as, level, keywords);
                    });
            })
            .Concat()
            .ToArray();

        string celebratesUrl = $"https://www.mrskin.com{url}/celebs";
        string celebratesFile = Path.Combine(skinMetadataCacheDirectory, $"{fileName}.Celebrates{Video.ImdbCacheExtension}");
        SkinCelebrate[] celebrates = clipsCQ.Find("#celebs_tab").IsEmpty()
            ? []
            : (await GetHtmlAsync(
                celebratesUrl,
                celebratesFile,
                cacheFiles,
                useCache,
                playWrightWrapper,
                "#appearances",
                async page => await page.ClickOrPressAsync("button.drawer-toggle", unload: () => page.Locator(".loading-icon"), cancellationToken: cancellationToken),
                cancellationToken))
            .Select(html => CQ.CreateDocument(html)
                .Find(".tab-content .media-drawer")
                .Select(celebrateDom =>
                {
                    CQ celebrateCQ = celebrateDom.Cq();
                    string image = celebrateCQ.Find("img").Attr("src");
                    CQ linkCQ = celebrateCQ.Find("a.media-title");
                    string name = linkCQ.TextTrimDecode();
                    string url = linkCQ.Attr("href");
                    string level = celebrateCQ.Find(".appearance-character span:eq(0)").TextTrimDecode();
                    string @as = celebrateCQ.Find(".appearance-character span:eq(1)").TextTrimDecode();
                    SkinClip[] clips = celebrateCQ
                        .Find(".thumbnail.clip")
                        .Select(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                            string title = (string)mediaCQ.Data("modal-title-name");
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
                            int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                            Debug.Assert(rating > 0);
                            string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                            Debug.Assert(level.IsNotNullOrWhiteSpace());
                            string[] keywords = thumbnailCQ
                                .Find(".scene-keywords span:eq(1)")
                                .TextTrimDecode()
                                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            Debug.Assert(keywords.Any());
                            return new SkinClip(title, url, image, names, rating, level, keywords);
                        })
                        .ToArray();
                    SkinPicture[] pictures = celebrateCQ
                        .Find(".thumbnail")
                        .Select(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                            string title = (string)mediaCQ.Data("modal-title-name");
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
                            Debug.Assert(@as.StartsWithIgnoreCase("- As "));
                            @as = @as.ReplaceIgnoreCase("- As ", string.Empty);
                            string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                            Debug.Assert(level.IsNotNullOrWhiteSpace());
                            string[] keywords = thumbnailCQ
                                .Find(".scene-keywords span:eq(1)")
                                .TextTrimDecode()
                                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            Debug.Assert(keywords.Any());
                            return new SkinPicture(title, url, image, names, @as, level, keywords);
                        })
                        .ToArray();

                    return new SkinCelebrate(name, url, image, level, @as, clips, pictures);
                }))
            .Concat()
            .ToArray();

        string celebrateScenesUrl = $"https://www.mrskin.com{url}/nude_scene_guide";
        string celebrateScenesFile = Path.Combine(skinMetadataCacheDirectory, $"{fileName}.Scenes{Video.ImdbCacheExtension}");
        SkinCelebrateScenes[] celebrateScenes = clipsCQ.Find("#nude_scene_guide_tab").IsEmpty()
            ? []
            : (await GetHtmlAsync(celebrateScenesUrl, celebrateScenesFile, cacheFiles, useCache, playWrightWrapper, "#nude_scene_guide", cancellationToken: cancellationToken))
            .Select(html => CQ.CreateDocument(html)
                .Find(".tab-content .nude-scene-content-group")
                .Select(celebrateDom =>
                {
                    CQ celebrateCQ = celebrateDom.Cq();
                    CQ linkCQ = celebrateCQ.Find("a:eq(0)");
                    string name = linkCQ.TextTrimDecode();
                    string url = linkCQ.Attr("href");
                    string level = celebrateCQ.Find(".character span:eq(0)").TextTrimDecode();
                    string @as = celebrateCQ.Find(".character span:eq(1)").TextTrimDecode();
                    SkinScene[] scenes = celebrateCQ
                        .Find(".nude-scene-content")
                        .Select(sceneDom =>
                        {
                            CQ sceneCQ = sceneDom.Cq();
                            CQ linkCQ = sceneCQ.Find("a:eq(0)");
                            string title = (string)linkCQ.Data("modal-title-name");
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
                    return new SkinCelebrateScenes(name, url, level, @as, scenes);
                }))
            .Concat()
            .ToArray();

        string episodesUrl = $"https://www.mrskin.com{url}/episode_guide";
        string episodesFile = Path.Combine(skinMetadataCacheDirectory, $"{fileName}.Episodes{Video.ImdbCacheExtension}");
        SkinEpisode[] episodes = clipsCQ.Find("#episode_guide_tab").IsEmpty()
            ? []
            : (await GetHtmlAsync(episodesUrl, episodesFile, cacheFiles, useCache, playWrightWrapper, "#episode_guide", cancellationToken: cancellationToken))
            .Select(html => CQ.CreateDocument(html)
                .Find(".episode-item")
                .Select(episodeDom =>
                {
                    CQ episodeCQ = episodeDom.Cq();
                    string title = episodeCQ.Children("h2").TextTrimDecode();
                    string description = episodeCQ.Children("p").HtmlTrim();
                    SkinClip[] clips = episodeCQ
                        .Find(".thumbnail.clip")
                        .Select(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                            string title = (string)mediaCQ.Data("modal-title-name");
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
                            int rating = thumbnailCQ.Find(".star-rating i.active").Length;
                            Debug.Assert(rating > 0);
                            string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                            Debug.Assert(level.IsNotNullOrWhiteSpace());
                            string[] keywords = thumbnailCQ
                                .Find(".scene-keywords span:eq(1)")
                                .TextTrimDecode()
                                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            Debug.Assert(keywords.Any());
                            return new SkinClip(title, url, image, names, rating, level, keywords);
                        })
                        .ToArray();
                    SkinPicture[] pictures = episodeCQ
                        .Find(".thumbnail")
                        .Select(thumbnailDom =>
                        {
                            CQ thumbnailCQ = thumbnailDom.Cq();
                            CQ mediaCQ = thumbnailCQ.Find("a.media-item:eq(0)");
                            string title = (string)mediaCQ.Data("modal-title-name");
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
                            Debug.Assert(@as.StartsWithIgnoreCase("- As "));
                            @as = @as.ReplaceIgnoreCase("- As ", string.Empty);
                            string level = thumbnailCQ.Find(".scene-keywords span:eq(0)").TextTrimDecode();
                            Debug.Assert(level.IsNotNullOrWhiteSpace());
                            string[] keywords = thumbnailCQ
                                .Find(".scene-keywords span:eq(1)")
                                .TextTrimDecode()
                                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            Debug.Assert(keywords.Any());
                            return new SkinPicture(title, url, image, names, @as, level, keywords);
                        })
                        .ToArray();
                    return new SkinEpisode(title, description, clips, pictures);
                }))
            .Concat()
            .ToArray();

        SkinMetadata metadata = new(title, url, image, year, rating, ratingDescription, userRating, description, blogCount, blogUrl, details, clips, pictures, celebrates, celebrateScenes, episodes);
        await JsonHelper.SerializeToFileAsync(metadata, metadataFile, cancellationToken);
        return metadata;
    }

    private static async Task<string[]> GetHtmlAsync(string url, string file, HashSet<string> cacheFiles, bool useCache, PlayWrightWrapper playWrightWrapper, string selector, Func<IPage, ValueTask>? update = null, CancellationToken cancellationToken = default)
    {
        IPage page = await playWrightWrapper.PageAsync();
        string html;
        if (cacheFiles.Contains(file))
        {
            if (await FileHelper.TextEndsWithIgnoreCaseAsync(file, "</html>", cancellationToken: cancellationToken))
            {
                if (useCache)
                {
                    html = await File.ReadAllTextAsync(file, cancellationToken);
                }
                else
                {
                    html = await page.GetStringAsync(url, selector, cancellationToken: cancellationToken);
                    if (update is not null)
                    {
                        await update(page);
                        html = await page.ContentAsync();
                    }

                    await File.WriteAllTextAsync(file, html, cancellationToken);
                }
            }
            else
            {
                FileHelper.Recycle(file);
                cacheFiles.Remove(file);
                html = await page.GetStringAsync(url, selector, cancellationToken: cancellationToken);
                if (update is not null)
                {
                    await update(page);
                    html = await page.ContentAsync();
                }

                await File.WriteAllTextAsync(file, html, cancellationToken);
            }
        }
        else
        {
            html = await page.GetStringAsync(url, selector, cancellationToken: cancellationToken);
            if (update is not null)
            {
                await update(page);
                html = await page.ContentAsync();
            }

            await File.WriteAllTextAsync(file, html, cancellationToken);
        }

        string lastPageUrl = await page.Locator("nav.pagination .last a").TextContentAsync() ?? string.Empty;
        //CQ pageCQ = CQ.CreateDocument(html);
        //string lastPageUrl = pageCQ.Find("nav.pagination .last a").Attr("href");
        int lastPageIndex = lastPageUrl.IsNullOrWhiteSpace()
            ? 1
            : int.Parse(new Regex(@"page\=([0-9]+)", RegexOptions.IgnoreCase).Match(lastPageUrl).Groups[1].Value);

        return await AsyncEnumerable.Range(2, lastPageIndex - 1)
            .Select(async (pageIndex, _, token) =>
            {
                string html = await page.GetStringAsync($"{url}?page={pageIndex}", selector, cancellationToken: token);
                if (update is not null)
                {
                    await update(page);
                    html = await page.ContentAsync();
                }

                await File.WriteAllTextAsync(PathHelper.AddFilePostfix(file, $"{Video.Delimiter}{pageIndex}"), html, token);
                return html;
            })
            .Prepend(html)
            .ToArrayAsync(cancellationToken);
    }
}

public record SkinSummary(string Title, string Url, string Year, string Image, int Rating);

public record SkinMetadata(
    string Title, string Url, string Image, string Year,
    int Rating, string RatingDescription, string UserRating, string Description, int BlogCount, string BlogUrl,
    Dictionary<string, string> Details,
    SkinClip[] Clips,
    SkinPicture[] Pictures,
    SkinCelebrate[] Celebrates,
    SkinCelebrateScenes[] CelebScenes,
    SkinEpisode[] Episodes);

public record SkinClip(string Title, string Url, string Image, Dictionary<string, string> Names, int Rating, string Level, string[] Keywords);

public record SkinPicture(string Title, string Url, string Image, Dictionary<string, string> Names, string As, string Level, string[] Keywords);

public record SkinCelebrate(string Name, string Url, string Image, string Level, string As, SkinClip[] Clips, SkinPicture[] Pictures);

public record SkinCelebrateScenes(string Name, string Url, string Level, string As, SkinScene[] Scenes);

public record SkinScene(string Title, string Url, string Image, int Rating, string Level, string[] Keywords, string Position, string Description);

public record SkinEpisode(string Title, string Description, SkinClip[] Clips, SkinPicture[] Pictures);