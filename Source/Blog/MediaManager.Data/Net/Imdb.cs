namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using MediaManager.IO;
using Microsoft.Playwright;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;

internal static partial class Imdb
{
    internal static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 12);

    private const string PageSelector = "main .ipc-page-grid__item";

    private static async Task<(bool IsUpdated, string Html, CQ TrimmedCQ)> TryUpdateAsync(this PlayWrightWrapper playWrightWrapper, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        IPage page = await playWrightWrapper.PageAsync();
        string url = page.Url;
        bool isUpdated = false;
        string html = await page.ContentAsync();
        CQ trimmedCQ = TrimPage(html);
        int spoilersButtonsCount = trimmedCQ.Find("button:contains('Spoilers')").Length;

        await Retry.FixedIntervalAsync(
            func: async () =>
            {
                await page.ClickOrPressAsync("button", new PageLocatorOptions() { HasText = "Don't prompt me" }, cancellationToken);

                int spoilerButtonCount = await page.ClickOrPressAsync(AriaRole.Button, new PageGetByRoleOptions() { Name = "Spoiler" }, cancellationToken);
                log($"{spoilerButtonCount} Spoiler buttons are found.");
                if (spoilerButtonCount > 0)
                {
                    isUpdated = true;
                    await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
                    if (await page.GetByText("error fetching more data").CountAsync() > 0
                        || await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Spoiler" }).CountAsync() > 0)
                    {
                        throw new HttpRequestException(HttpRequestError.InvalidResponse, $"Ajax error {url}", null, HttpStatusCode.InternalServerError);
                    }

                    log($"{spoilerButtonCount} Spoilers are waited.");
                    await page.WaitForCountAsync(new Regex("^Spoilers$"), new PageGetByTextOptions() { Exact = true }, spoilerButtonCount, cancellationToken);
                    log($"{spoilerButtonCount} Spoilers are loaded.");
                }

                ILocator seeMoreButtons = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() });
                int targetSeeMoreButtonsCount = await seeMoreButtons.CountAsync();
                int seeAllButtonsCount = await page.ClickOrPressAsync(AriaRole.Button, new PageGetByRoleOptions() { Name = "See all" }, cancellationToken);
                log($"{seeAllButtonsCount} See all buttons are found.");
                if (seeAllButtonsCount > 0)
                {
                    isUpdated = true;
                    await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
                    if (await page.GetByText("error fetching more data").CountAsync() > 0
                        || await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "See all" }).CountAsync() > 0)
                    {
                        throw new HttpRequestException(HttpRequestError.InvalidResponse, $"Ajax error {url}", null, HttpStatusCode.InternalServerError);
                    }
                }

                targetSeeMoreButtonsCount -= seeAllButtonsCount;
                if (targetSeeMoreButtonsCount < 0)
                {
                    targetSeeMoreButtonsCount = 0;
                }

                log($"{targetSeeMoreButtonsCount} See more buttons are found.");
                if (targetSeeMoreButtonsCount > 0)
                {
                    await page.WaitForCountAsync(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }, targetSeeMoreButtonsCount, cancellationToken);
                    int actualSeeMoreButtonCount = await page.ClickOrPressAsync(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }, cancellationToken);
                    Debug.Assert(targetSeeMoreButtonsCount == actualSeeMoreButtonCount);

                    isUpdated = true;
                    await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
                    if (await page.GetByText("error fetching more data").CountAsync() > 0
                        || await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }).CountAsync() > 0)
                    {
                        throw new HttpRequestException(HttpRequestError.InvalidResponse, $"Ajax error {url}", null, HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    await page.WaitForNoneAsync(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }, cancellationToken);
                }

                if (isUpdated)
                {
                    await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
                    html = await page.ContentAsync();
                    trimmedCQ = TrimPage(html);
                }

                if (trimmedCQ.Find("section span.ipc-see-more button:contains('Spoilers')").Any()
                    || trimmedCQ.Find("section > div.ipc-signpost > div.ipc-signpost__text:contains('Spoilers')").Length != spoilersButtonsCount
                    || trimmedCQ.Find("button.ipc-see-more__button:contains('all') span.ipc-btn__text:visible").Any()
                    || trimmedCQ.Find("button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").Any()
                    || trimmedCQ.Find(":contains('error fetching more data')").Any())
                {
                    throw new HttpRequestException(HttpRequestError.InvalidResponse, $"Ajax error {url}", null, HttpStatusCode.InternalServerError);
                }
            },
            retryingHandler: void (sender, arg) =>
            {
                log($"Update retry count {arg.CurrentRetryCount}.");
                log(arg.LastException.ToString());
                if (arg.LastException is HttpRequestException { StatusCode: HttpStatusCode.InternalServerError, HttpRequestError: HttpRequestError.InvalidResponse })
                {
                    page = playWrightWrapper.RestartAsync(cancellationToken: cancellationToken).Result;
                    html = page.GetStringAsync(url, PageSelector, cancellationToken: cancellationToken).Result;
                }
                else
                {
                    html = page.RefreshAsync(PageSelector, cancellationToken: cancellationToken).Result;
                }

                isUpdated = false;
                trimmedCQ = TrimPage(html);
                spoilersButtonsCount = trimmedCQ.Find("button:contains('Spoilers')").Length;
            },
            cancellationToken: cancellationToken);

        return (isUpdated, html, trimmedCQ);
    }

    private static async Task<(string Html, CQ CQ)> GetHtmlAsync(bool skip, string file, string url, PlayWrightWrapper? playWrightWrapper, HttpClient? httpClient, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        if (skip)
        {
            return (string.Empty, string.Empty);
        }

        if (File.Exists(file))
        {
            string cachedHtml = await File.ReadAllTextAsync(file, cancellationToken);
            if (!cachedHtml.EndsWithIgnoreCase("</html>"))
            {
                FileHelper.Recycle(file);
                throw new InvalidDataException($"Invalid cached HTML: {file}.");
            }

            return (cachedHtml.ThrowIfNullOrWhiteSpace(), TrimPage(cachedHtml));
        }

        if (playWrightWrapper is null)
        {
            Debug.Assert(httpClient is not null);
            string downloadedHtml = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, cancellationToken), cancellationToken: cancellationToken);
            return (downloadedHtml, TrimPage(downloadedHtml));
        }

        IPage page = await playWrightWrapper.PageAsync();
        await page.GetStringAsync(url, PageSelector, cancellationToken: cancellationToken);
        (bool isUpdated, string html, CQ trimmedCQ) = await playWrightWrapper.TryUpdateAsync(log, cancellationToken);
        log(isUpdated ? $"{url} is updated." : $"{url} is not updated.");
        return (html, trimmedCQ);
    }

    private static CQ TrimPage(CQ pageCQ) => pageCQ
        .Find(PageSelector).Eq(0)
        .Find("[data-testid='contribution'], [data-testid='more-from-section']")
        .Remove()
        .End();

    internal static async Task<(ImdbMetadata ImdbMetadata,
        string ImdbUrl, string ImdbHtml,
        string AdvisoriesUrl, string AdvisoriesHtml,
        string AwardsUrl, string AwardsHtml,
        string ConnectionsUrl, string ConnectionsHtml,
        string CrazyCreditsUrl, string CrazyCreditsHtml,
        string CreditsUrl, string CreditsHtml,
        string GoofsUrl, string GoofsHtml,
        string KeywordsUrl, string KeywordsHtml,
        string QuotesUrl, string QuotesHtml,
        string ReleasesUrl, string ReleasesHtml,
        string SoundtracksUrl, string SoundtracksHtml,
        string TriviaUrl, string TriviaHtml,
        string VersionsUrl, string VersionsHtml,

        string ParentImdbUrl, string ParentImdbHtml,
        string ParentAdvisoriesUrl, string ParentAdvisoriesHtml,
        string ParentAwardsUrl, string ParentAwardsHtml,
        string ParentConnectionsUrl, string ParentConnectionsHtml,
        string parentCrazyCreditsUrl, string parentCrazyCreditsHtml,
        string ParentCreditsUrl, string ParentCreditsHtml,
        string ParentGoofsUrl, string ParentGoofsHtml,
        string ParentKeywordsUrl, string parentKeywordsHtml,
        string ParentQuotesUrl, string ParentQuotesHtml,
        string ParentReleasesUrl, string ParentReleasesHtml,
        string ParentSoundtracksUrl, string ParentSoundtracksHtml,
        string ParentTriviaUrl, string ParentTriviaHtml,
        string ParentVersionsUrl, string ParentVersionsHtml
        )> DownloadAsync(
        string imdbId,
        string imdbFile, string advisoriesFile, string awardsFile, string connectionsFile, string crazyCreditsFile, string creditsFile, string goofsFile, string keywordsFile, string quotesFile, string releasesFile, string soundtracksFile, string triviaFile, string versionsFile,
        string parentImdbFile, string parentAdvisoriesFile, string parentAwardsFile, string parentConnectionsFile, string parentCrazyCreditsFile, string parentCreditsFile, string parentGoofsFile, string parentKeywordsFile, string parentQuotesFile, string parentReleasesFile, string parentSoundtracksFile, string parentTriviaFile, string parentVersionsFile,
        PlayWrightWrapper? playWrightWrapper, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        IPage? page = playWrightWrapper is null ? null : await playWrightWrapper.PageAsync();
        using HttpClient? httpClient = page is null ? new() : null;
        httpClient?.AddEdgeHeaders();

        string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";
        bool hasImdbFile = File.Exists(imdbFile);
        string imdbHtml;
        if (hasImdbFile)
        {
            imdbHtml = (await File.ReadAllTextAsync(imdbFile, cancellationToken)).ThrowIfNullOrWhiteSpace();
            if (!imdbHtml.EndsWithIgnoreCase("</html>"))
            {
                FileHelper.Recycle(imdbFile);
                throw new InvalidDataException($"Invalid cached HTML: {imdbFile}.");
            }
        }
        else
        {
            imdbHtml = await (page?.GetStringAsync(imdbUrl, PageSelector, new PageGotoOptions() { Referer = "https://www.imdb.com/" }, cancellationToken)
                ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(imdbUrl, cancellationToken), cancellationToken: cancellationToken));
        }

        if (!hasImdbFile && page is not null && !page.Url.EqualsOrdinal(imdbUrl))
        {
            log($"Redirected {imdbId} to {page.Url}");
            imdbUrl = page.Url;
            imdbId = ImdbMetadata.ImdbIdSubstringRegex().Match(imdbUrl).Value;
            Debug.Assert(imdbId.IsImdbId());
        }

        if (!hasImdbFile && page is not null && playWrightWrapper is not null)
        {
            await Retry.FixedIntervalAsync(
                async () =>
                {
                    ILocator knowLocator = page.GetByTestId("DidYouKnow");
                    if (await knowLocator.CountAsync() > 0)
                    {
                        await knowLocator.ScrollIntoViewIfNeededAsync();
                    }
                    else
                    {
                        ILocator topPicksLocator = await page.WaitForCountAsync("[data-cel-widget='DynamicFeature_TopPicks']", locatorCount: 1, cancellationToken: cancellationToken);
                        await topPicksLocator.ScrollIntoViewIfNeededAsync();
                    }

                    await page.Keyboard.PressAsync("PageUp");
                    await page.WaitForSelectorAsync("[data-testid='storyline-parents-guide']");
                    imdbHtml = await page.ContentAsync();
                },
                retryingHandler: (sender, args) =>
                {
                    if (args.LastException is TimeoutException)
                    {
                        page = playWrightWrapper.RestartAsync(cancellationToken: cancellationToken).Result;
                        imdbHtml = page.GetStringAsync(imdbUrl, PageSelector, cancellationToken: cancellationToken).Result;
                    }
                },
                cancellationToken: cancellationToken);
        }

        CQ imdbCQ = imdbHtml;
        string json = imdbCQ.Find("script[type='application/ld+json']").Text();
        //string json2 = imdbCQ.Find("#__NEXT_DATA__").Text();
        string htmlTitle = imdbCQ.Find("title").TextTrimDecode();
        if (htmlTitle.StartsWithIgnoreCase("500 Error") || imdbCQ.Find("[data-testid='error-page-title']").TextTrimDecode().ContainsIgnoreCase("500 Error"))
        {
            throw new HttpRequestException(HttpRequestError.InvalidResponse, imdbUrl, null, HttpStatusCode.InternalServerError);
        }

        if (htmlTitle.StartsWithIgnoreCase("404 Error") || imdbCQ.Find("[data-testid='error-page-title']").TextTrimDecode().ContainsIgnoreCase("404 Error"))
        {
            throw new HttpRequestException(HttpRequestError.InvalidResponse, imdbUrl, null, HttpStatusCode.NotFound);
        }

        ImdbMetadata imdbMetadata = JsonHelper.Deserialize<ImdbMetadata>(json);

        string parentImdbUrl = string.Empty;
        string parentAdvisoriesUrl = string.Empty;
        string parentAwardsUrl = string.Empty;
        string parentConnectionsUrl = string.Empty;
        string parentCrazyCreditsUrl = string.Empty;
        string parentCreditsUrl = string.Empty;
        string parentGoofsUrl = string.Empty;
        string parentKeywordsUrl = string.Empty;
        string parentQuotesUrl = string.Empty;
        string parentReleasesUrl = string.Empty;
        string parentSoundtracksUrl = string.Empty;
        string parentTriviaUrl = string.Empty;
        string parentVersionsUrl = string.Empty;

        string parentImdbHtml = string.Empty;
        string parentAdvisoriesHtml = string.Empty;
        string parentAwardsHtml = string.Empty;
        string parentConnectionsHtml = string.Empty;
        string parentCrazyCreditsHtml = string.Empty;
        string parentCreditsHtml = string.Empty;
        string parentGoofsHtml = string.Empty;
        string parentKeywordsHtml = string.Empty;
        string parentQuotesHtml = string.Empty;
        string parentReleasesHtml = string.Empty;
        string parentSoundtracksHtml = string.Empty;
        string parentTriviaHtml = string.Empty;
        string parentVersionsHtml = string.Empty;

        imdbCQ = imdbCQ.Find("main");
        string parentHref = imdbCQ.Find("div.titleParent a").FirstOrDefault()?.GetAttribute("href")
            ?? imdbCQ.Find("div").FirstOrDefault(div => div.Classes.Any(@class => @class.StartsWithOrdinal("TitleBlock__SeriesParentLinkWrapper")))?.Cq().Find("a").Attr("href")
            ?? imdbCQ.Find("section.ipc-page-section > div:first > a:first").FirstOrDefault()?.GetAttribute("href")
            ?? string.Empty;
        ImdbMetadata? parentMetadata = null;
        if (parentHref.IsNotNullOrWhiteSpace())
        {
            string parentImdbId = ImdbMetadata.ImdbIdSubstringRegex().Match(parentHref).Value;
            if (!parentImdbId.EqualsIgnoreCase(imdbId))
            {
                (
                    parentMetadata,
                    parentImdbUrl, parentImdbHtml,
                    parentAdvisoriesUrl, parentAdvisoriesHtml,
                    parentAwardsUrl, parentAwardsHtml,
                    parentConnectionsUrl, parentConnectionsHtml,
                    parentCrazyCreditsUrl, parentCrazyCreditsHtml,
                    parentCreditsUrl, parentCreditsHtml,
                    parentGoofsUrl, parentGoofsHtml,
                    parentKeywordsUrl, parentKeywordsHtml,
                    parentQuotesUrl, parentQuotesHtml,
                    parentReleasesUrl, parentReleasesHtml,
                    parentSoundtracksUrl, parentSoundtracksHtml,
                    parentTriviaUrl, parentTriviaHtml,
                    parentVersionsUrl, parentVersionsHtml,

                    _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _
                ) = await DownloadAsync(
                    parentImdbId,
                    parentImdbFile, parentAdvisoriesFile, parentAwardsFile, parentConnectionsFile, parentCrazyCreditsFile, parentCreditsFile, parentGoofsFile, parentKeywordsFile, parentQuotesFile, parentReleasesFile, parentSoundtracksFile, parentTriviaFile, parentVersionsFile,
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                    playWrightWrapper, log, cancellationToken);
            }
        }

        string htmlTitleYear = htmlTitle.ContainsOrdinal("(")
            ? htmlTitle[(htmlTitle.LastIndexOfOrdinal("(") + 1)..htmlTitle.LastIndexOfOrdinal(")")]
            : string.Empty;
        htmlTitleYear = YearRegex().Match(htmlTitleYear).Value;

        string htmlYear = imdbCQ.Find("[data-testid='hero__pageTitle']").NextAll().Find("li:eq(0)").TextTrimDecode();
        htmlYear = YearRegex().Match(htmlYear).Value;
        if (!YearRegex().IsMatch(htmlYear))
        {
            htmlYear = imdbCQ.Find("[data-testid='hero__pageTitle']").NextAll().Find("li:eq(1)").TextTrimDecode();
            htmlYear = YearRegex().Match(htmlYear).Value;
        }

        //if (htmlYear.IsNullOrWhiteSpace())
        //{
        //    htmlYear = imdbCQ.Find("""div.title_wrapper div.subtext a[title="See more release dates"]""").TextTrimDecode();
        //    htmlYear = YearRegex().Match(htmlYear).Value;
        //}

        if (htmlYear.IsNullOrWhiteSpace())
        {
            htmlYear = imdbCQ.Find($"a[href*='/title/{imdbId}/releaseinfo']").Select(dom => dom.TextTrimDecode()).FirstOrDefault(text => YearRegex().IsMatch(text), string.Empty);
        }

        Debug.Assert(htmlYear.EqualsOrdinal(htmlTitleYear) || parentMetadata is not null);

        string year = imdbMetadata.Year;
        if (year.IsNullOrWhiteSpace())
        {
            year = htmlYear.IsNotNullOrWhiteSpace() ? htmlYear : imdbMetadata.YearOfLatestRelease;
        }
        else
        {
            Debug.Assert(year.EqualsOrdinal(htmlYear));
        }

        Debug.Assert(year.IsNullOrWhiteSpace() || YearRegex().IsMatch(htmlYear));

        Dictionary<string, string[][]> details = imdbCQ
            .Find("[data-testid='title-details-section'] > ul > li")
            .Select(itemDom =>
            {
                CQ itemCQ = itemDom.Cq();
                return (Key: itemCQ.Children().Eq(0).TextTrimDecode(), Values: itemCQ.Find("ul li"));
            })
            .Where(item => item.Values.Any())
            .ToDictionary(
                itemCQ => itemCQ.Key,
                itemCQ => itemCQ.Values
                    .Select(innerItemDom =>
                    {
                        CQ innerItemCQ = innerItemDom.Cq();
                        CQ linkCQ = innerItemCQ.Find("a");
                        string text = linkCQ.TextTrimDecode();
                        string url = linkCQ.Attr("href");
                        CQ descriptionCQ = innerItemCQ.Find("span");
                        string[] innerItems = [];
                        if (linkCQ.Any())
                        {
                            innerItems = [text, url];
                        }

                        if (descriptionCQ.Any())
                        {
                            innerItems = [.. innerItems, descriptionCQ.TextTrimDecode()];
                        }

                        return innerItems;
                    })
                    .ToArray());

        Dictionary<string, string[]> boxOffice = imdbCQ
            .Find("[data-testid='BoxOffice'] ul li.ipc-metadata-list__item")
            .Select(itemDom => itemDom.Cq())
            .ToDictionary(
                itemCQ => itemCQ.Find("span").Eq(0).TextTrimDecode(),
                itemCQ => itemCQ.Find("li").Select(innerItemDom => innerItemDom.TextTrimDecode()).ToArray());

        Dictionary<string, string[]> techSpecs = imdbCQ
            .Find("[data-testid='TechSpecs'] ul li.ipc-metadata-list__item")
            .Select(itemDom => itemDom.Cq())
            .ToDictionary(
                itemCQ => itemCQ.Find("span").Eq(0).TextTrimDecode(),
                itemCQ => itemCQ.Find("li").Select(innerItemDom => innerItemDom.TextTrimDecode()).ToArray());

        CQ awardsDivCQ = imdbCQ.Find("[data-testid='awards']");
        string topRated = awardsDivCQ.Find("[data-testid='award_top-rated']").TextTrimDecode();
        CQ awardsInfoCQ = awardsDivCQ.Find("[data-testid='award_information']");
        string[] awards = awardsInfoCQ
            .Find("a")
            .SkipLast(1)
            .Select(linkDom => linkDom.TextTrimDecode())
            .Concat(awardsInfoCQ.Find("ul li").Select(itemDom => itemDom.TextTrimDecode()))
            .ToArray();
        if (topRated.IsNotNullOrWhiteSpace())
        {
            awards = [topRated, .. awards];
        }

        bool skipAwards = awardsInfoCQ.Find("a").IsEmpty();

        CQ taglineCQ = imdbCQ.Find("[data-testid='storyline-taglines'] ul li");
        Debug.Assert(taglineCQ.Length <= 1);
        string tagline = taglineCQ.TextTrimDecode();
        CQ storylineSectionCQ = imdbCQ.Find("[data-testid='Storyline']");
        Debug.Assert(storylineSectionCQ.Any());
        bool skipAdvisories = storylineSectionCQ.Find("a:contains('Add content advisory')").Any()
            && storylineSectionCQ.Find("[data-testid='storyline-certificate']").IsEmpty();
        bool skipKeywords = storylineSectionCQ.Find($"a[href*='/title/{imdbId}/keywords/']:contains('more')").IsEmpty();

        bool skipTrivia = true;
        bool skipGoofs = true;
        bool skipQuotes = true;
        bool skipCrazyCredits = true;
        bool skipVersions = true;
        bool skipConnections = true;
        bool skipSoundtracks = true;
        CQ knowSection = imdbCQ.Find("[data-testid='DidYouKnow']");
        if (knowSection.Any())
        {
            skipTrivia = knowSection.Find("a:contains('Trivia')").IsEmpty();
            skipGoofs = knowSection.Find("a:contains('Goofs')").IsEmpty();
            skipQuotes = knowSection.Find("a:contains('Quotes')").IsEmpty();
            skipCrazyCredits = knowSection.Find("a:contains('Crazy credits')").IsEmpty();
            skipVersions = knowSection.Find("a:contains('Alternate versions')").IsEmpty();
            skipConnections = knowSection.Find("a:contains('Connections')").IsEmpty();
            skipSoundtracks = knowSection.Find("a:contains('Soundtracks')").IsEmpty();
        }

        string releasesUrl = $"{imdbUrl}releaseinfo/";
        (string releasesHtml, CQ releasesCQ) = await GetHtmlAsync(false, releasesFile, releasesUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[][] releaseDates = releasesCQ
            .Find("[data-testid='sub-section-releases'] [data-testid='list-item']")
            .SelectMany(rowDom =>
            {
                CQ rowCQ = rowDom.Cq();
                CQ regionCQ = rowCQ.Find("a[href*='/calendar/?region=']");
                string region = regionCQ.TextTrimDecode();
                string url = regionCQ.Attr("href");
                return rowCQ
                    .Find("ul li")
                    .Select(innerItemDom =>
                    {
                        CQ innerItemCQ = innerItemDom.Cq();
                        string date = innerItemCQ.Find("span:eq(0)").TextTrimDecode();
                        string description = innerItemCQ.Find("span:eq(1)").TextTrimDecode();
                        return description.IsNotNullOrWhiteSpace()
                            ? new string[] { region, url, date, description }
                            : [region, url, date];
                    });
            })
            .ToArray();

        (string Region, string Title, string Description)[] allTitles = releasesCQ
            .Find("#akas")
            .Next()
            .Find("tr")
            .Select(row => row.Cq().Children())
            .Select(cells => (
                Region: cells.First().TextTrimDecode(),
                Title: cells.Last().TextTrimDecode(),
                Description: string.Empty
            ))
            .ToArray();
        Enumerable
            .Range(0, allTitles.Length)
            .ForEach(index =>
            {
                if (allTitles[index].Region.IsNullOrWhiteSpace())
                {
                    allTitles[index].Region = allTitles[index - 1].Region;
                }
            });

        if (allTitles.IsEmpty())
        {
            allTitles = releasesCQ
                .Find("[data-testid='sub-section-akas'] [data-testid='list-item']")
                .SelectMany(rowDom =>
                {
                    CQ rowCQ = rowDom.Cq();
                    string region = rowCQ.Find("span:eq(0)").TextTrimDecode();
                    return rowCQ
                        .Find("ul li")
                        .Select(innerItemDom =>
                        {
                            CQ innerItemCQ = innerItemDom.Cq();
                            return (
                                Region: region,
                                Title: innerItemCQ.Find("span:eq(0)").TextTrimDecode(),
                                Description: innerItemCQ.Find("span:eq(1)").TextTrimDecode()
                            );
                        });
                })
                .ToArray();
        }

        Dictionary<string, string[][]> titles = allTitles
            .ToLookup(
                row => row.Region,
                row => row.Description.IsNotNullOrWhiteSpace() ? new string[] { row.Title, row.Description } : [row.Title],
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray());

        //Debug.Assert(titles.Any()
        //    || allTitlesCQ.TextTrimDecode().ContainsIgnoreCase("It looks like we don't have any AKAs for this title yet.")
        //    || imdbId is "tt10562876" or "tt13734388" or "tt11485640" or "tt11127510" or "tt11127706" or "tt11423284" or "tt20855690");

        string title;
        string originalTitle;
        //string alsoKnownAs = string.Empty;
        if (page is not null)
        {
            title = imdbCQ
                .Find("section.ipc-page-section--bp-xs h1")
                .Find("#titleYear")
                .Remove()
                .End()
                .TextTrimDecode();

            if (title.IsNullOrWhiteSpace())
            {
                title = imdbCQ
                    .Find("[data-testid='hero-title-block__title']")
                    .TextTrimDecode();
            }

            originalTitle = imdbCQ.Find("[data-testid='hero__pageTitle']").Next("div").TextTrimDecode();
            if (originalTitle.StartsWithIgnoreCase("Original title: "))
            {
                originalTitle = originalTitle["Original title: ".Length..].Trim();
            }

            if (originalTitle.IsNullOrWhiteSpace())
            {
                originalTitle = imdbCQ
                    .Find("div.originalTitle")
                    .Find("span")
                    .Remove()
                    .End()
                    .TextTrimDecode();
            }

            if (originalTitle.IsNullOrWhiteSpace())
            {
                originalTitle = imdbCQ
                    .Find("[data-testid='hero-title-block__original-title']")
                    .TextTrimDecode()
                    .Replace("Original title: ", string.Empty);
            }

            //if (originalTitle.IsNullOrWhiteSpace())
            //{
            //    originalTitle = alsoKnownAs;
            //}
        }
        else
        {
            if (imdbMetadata.AlternateName.IsNotNullOrWhiteSpace())
            {
                title = imdbMetadata.AlternateName;
                Debug.Assert(imdbMetadata.Name.IsNotNullOrWhiteSpace());
                originalTitle = imdbMetadata.Name;
            }
            else
            {
                if (titles.Any())
                {
                    string releaseHtmlTitle = releasesCQ.Find("title").TextTrimDecode();
                    releaseHtmlTitle = releaseHtmlTitle[..releaseHtmlTitle.LastIndexOfOrdinal("(")].Trim();

                    if (!titles.TryGetValue("(original title)", out string[][]? originalTitleValues))
                    {
                        string[] originalTitleKeys = titles.Keys.Where(key => key.ContainsIgnoreCase("original title")).ToArray();
                        if (originalTitleKeys.Length > 1)
                        {
                            originalTitleKeys = originalTitleKeys.Where(key => !key.ContainsIgnoreCase("USA")).ToArray();
                        }

                        originalTitleValues = originalTitleKeys.SelectMany(key => titles[key]).ToArray();
                    }

                    if (originalTitleValues.Length > 1)
                    {
                        originalTitleValues = originalTitleValues.Where(titleValue => !titleValue.First().EqualsOrdinal(releaseHtmlTitle)).ToArray();
                    }

                    if (originalTitleValues.Length > 1)
                    {
                        originalTitleValues = originalTitleValues.Where(titleValue => !titleValue.First().EqualsIgnoreCase(releaseHtmlTitle)).ToArray();
                    }

                    originalTitle = originalTitleValues.Length switch
                    {
                        1 => originalTitleValues.Select(item => item.First()).Single(),
                        > 1 => originalTitleValues.Select(item => item.First()).FirstOrDefault(titleValue => originalTitleValues.Select(item => item.First()).Except(EnumerableEx.Return(titleValue)).All(titleValue.ContainsIgnoreCase))
                            ?? string.Join(Video.TitleSeparator, originalTitleValues),
                        _ => string.Empty
                    };

                    if (!titles.TryGetValue("World-wide (English title)", out string[][]? titleValues))
                    {
                        titleValues = titles
                            .Where(pair => pair.Key.ContainsIgnoreCase("World-wide (English title)"))
                            .SelectMany(pair => pair.Value)
                            .ToArray();
                    }

                    if (titleValues.IsEmpty()
                        && !titles.TryGetValue("USA", out titleValues)
                        && !titles.TryGetValue("USA (working title)", out titleValues)
                        && !titles.TryGetValue("USA (informal English title)", out titleValues)
                        && !titles.TryGetValue("UK", out titleValues)
                        && !titles.TryGetValue("UK (informal English title)", out titleValues)
                        && !titles.TryGetValue("Hong Kong (English title)", out titleValues)
                        && !titles.TryGetValue("(original title)", out titleValues))
                    {
                        titleValues = [];
                    }

                    Debug.Assert(titleValues.Any());
                    title = titleValues.Length switch
                    {
                        1 => titleValues.Select(item => item.First()).Single(),
                        > 1 => titleValues.Select(item => item.First()).FirstOrDefault(availableTitle => releaseHtmlTitle.EqualsOrdinal(availableTitle))
                            ?? titleValues.Select(item => item.First()).FirstOrDefault(availableTitle => releaseHtmlTitle.EqualsIgnoreCase(availableTitle))
                            ?? titleValues.Select(item => item.First()).FirstOrDefault(titleValue => titleValues.Select(item => item.First()).Except(EnumerableEx.Return(titleValue)).All(titleValue.ContainsIgnoreCase))
                            ?? string.Join(Video.TitleSeparator, titleValues),
                        _ => releaseHtmlTitle
                    };

                    Debug.Assert(title.IsNotNullOrWhiteSpace());
                }
                else
                {
                    title = imdbCQ
                        .Find("div.title_wrapper h1")
                        .Find("#titleYear")
                        .Remove()
                        .End()
                        .TextTrimDecode()
                        .Replace(" ", string.Empty);

                    originalTitle = imdbCQ
                        .Find("div.originalTitle")
                        .Find("span")
                        .Remove()
                        .End()
                        .TextTrimDecode();

                    if (title.IsNullOrWhiteSpace())
                    {
                        title = imdbCQ
                            .Find("[data-testid='hero-title-block__title']")
                            .Find("#titleYear")
                            .Remove()
                            .End()
                            .TextTrimDecode();
                    }

                    htmlTitle = htmlTitle.ContainsOrdinal("(")
                        ? htmlTitle[..htmlTitle.LastIndexOfOrdinal("(")].Trim()
                        : htmlTitle.Replace("- IMDB", string.Empty).Trim();
                    Debug.Assert(title.EqualsOrdinal(htmlTitle) || htmlTitle.ContainsOrdinal(title));
                }
            }
        }

        if (title.EqualsIgnoreCase(originalTitle))
        {
            originalTitle = string.Empty;
        }

        Debug.Assert(title.IsNotNullOrWhiteSpace());

        string advisoriesUrl = $"{imdbUrl}parentalguide/";
        (string advisoriesHtml, CQ advisoriesCQ) = await GetHtmlAsync(skipAdvisories, advisoriesFile, advisoriesUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string mpaaRating = advisoriesCQ.Find("[data-testid='content-rating'] ul li div.ipc-metadata-list-item__content-container").First().TextTrimDecode();

        Dictionary<string, Dictionary<string, string[]>> advisories = advisoriesCQ
            .Find("[data-testid='content-rating']")
            .Siblings()
            .TakeWhile(sectionDom => sectionDom.GetAttribute("data-testid").IsNullOrEmpty())
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                Category: sectionCQ.Find("h3").TextTrimDecode(),
                Severity: sectionCQ.Find("[data-testid='severity_component']").Children().First().TextTrimDecode(),
                Descriptions: sectionCQ
                    .Find("[data-testid^='sub-section-'] [data-testid='item-html']")
                    .Select(item => item.TextTrimDecode())
                    .ToArray()
            ))
            .Where(advisory => advisory.Severity.IsNotNullOrWhiteSpace() || advisory.Descriptions.Any())
            .ToLookup(advisory => advisory.Category)
            .ToDictionary(group => group.Key, group => group.ToDictionary(advisory => advisory.Severity, advisory => advisory.Descriptions));

        Dictionary<string, string> certifications = advisoriesCQ
            .Find("[data-testid='certificates'] [data-testid='certificates-item']")
            .SelectMany(regionDom =>
            {
                CQ regionCQ = regionDom.Cq();
                string region = regionCQ.Find("span.ipc-metadata-list-item__label").TextTrimDecode();
                return regionCQ
                    .Find("ul li")
                    .Select(certificationDom =>
                    {
                        CQ certificationCQ = certificationDom.Cq();
                        CQ certificationLinkCQ = certificationCQ.Find("a");
                        string certification = certificationLinkCQ.TextTrimDecode();
                        string link = certificationLinkCQ.Attr("href");
                        string remark = certificationCQ.Find("span").TextTrimDecode();
                        return (
                            Certification: $"{region}:{certification}{(remark.IsNullOrWhiteSpace() ? string.Empty : $" ({remark})")}",
                            Link: link
                        );
                    });
            })
            .DistinctBy(certification => certification.Certification)
            .ToDictionary(certification => certification.Certification, certification => certification.Link);

        string awardsUrl = $"{imdbUrl}awards/";
        (string awardsHtml, CQ awardsCQ) = await GetHtmlAsync(skipAwards, awardsFile, awardsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        ImdbAwards[] allAwards = awardsCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom =>
            {
                CQ sectionCQ = sectionDom.Cq();
                CQ eventCQ = sectionCQ.Find("h3");
                return new ImdbAwards(
                    eventCQ.TextTrimDecode(),
                    eventCQ.Parent().Attr("href"),
                    sectionCQ
                        .Find("[data-testid='list-item']")
                        .Select(itemDom =>
                        {
                            CQ itemCQ = itemDom.Cq();
                            CQ statusCQ = itemCQ.Find("a[href*='/event/ev']");
                            CQ titleCQ = statusCQ.Find("span").Remove();
                            return new ImdbAward(
                                statusCQ.TextTrimDecode(),
                                statusCQ.Attr("href"),
                                titleCQ.TextTrimDecode(),
                                itemCQ.Find("ul:eq(0)").TextTrimDecode(),
                                itemCQ.Find("span.ipc-expandableSection__content").TextTrimDecode(),
                                itemCQ
                                    .Find("ul:eq(1) li a")
                                    .Select(listItemDom =>
                                    {
                                        CQ listItemCQ = listItemDom.Cq();
                                        string name = listItemCQ.TextTrimDecode();
                                        string url = listItemCQ.Attr("href");
                                        CQ descriptionCQ = listItemCQ.Next();
                                        return descriptionCQ.Any()
                                            ? new string[] { name, url, descriptionCQ.TextTrimDecode() }
                                            : [name, url];
                                    })
                                    .ToArray()
                            );
                        })
                        .ToArray()
                );
            })
            .ToArray();

        string connectionsUrl = $"{imdbUrl}movieconnections/";
        (string connectionsHtml, CQ connectionsCQ) = await GetHtmlAsync(skipConnections, connectionsFile, connectionsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, string[][]> connections = connectionsCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                Key: sectionCQ.Find("h3").TextTrimDecode(),
                Value: sectionCQ
                    .Find("[data-testid='list-item']")
                    .Select(listItemDom =>
                    {
                        CQ listItemCQ = listItemDom.Cq();
                        CQ linkCQ = listItemCQ.Find("a").Remove();
                        CQ paragraphCQ = listItemCQ.Find("p");
                        CQ description = listItemCQ.Find(".ipc-html-content-inner-div:eq(1)");
                        return description.Any()
                            ? new string[] { linkCQ.TextTrimDecode(), linkCQ.Attr("href"), paragraphCQ.TextTrimDecode(), description.TextTrimDecode() }
                            : [linkCQ.TextTrimDecode(), linkCQ.Attr("href"), paragraphCQ.TextTrimDecode()];
                    })
                    .ToArray()))
            //.Where(pair => pair.Key.IsNotNullOrWhiteSpace())
            //.TakeWhile(pair => !pair.Key.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        string crazyCreditsUrl = $"{imdbUrl}crazycredits/";
        (string crazyCreditsHtml, CQ crazyCreditsCQ) = await GetHtmlAsync(skipCrazyCredits, crazyCreditsFile, crazyCreditsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[] crazyCredits = crazyCreditsCQ
            .Find("[data-testid='item-html'] .crazy-credit-text")
            .Find("*")
            .Each(linkDom => linkDom.RemoveAttribute("class"))
            .End()
            .Select(paragraphDom => paragraphDom.HtmlTrim()).ToArray();

        string creditsUrl = $"{imdbUrl}fullcredits/";
        (string creditsHtml, CQ creditsCQ) = await GetHtmlAsync(false, creditsFile, creditsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, string[][]> credits = creditsCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                Category: sectionCQ.Find("h3").TextTrimDecode(),
                Credits: sectionCQ
                    .Find("[data-testid='name-credits-list-item']")
                    .Select(itemDom =>
                    {
                        CQ itemCQ = itemDom.Cq();
                        CQ nameLinkCQ = itemCQ.Find("a[href*='/name/nm']:last");
                        Debug.Assert(nameLinkCQ.Length == 1);
                        CQ characterLinkCQ = itemCQ.Find($"a[href*='/title/{imdbId}/characters/nm']");
                        string description = itemCQ.Find("span").Select(spanDom => spanDom.TextTrimDecode()).SingleOrDefault(text => !text.EqualsOrdinal("/") && text.IsNotNullOrWhiteSpace(), string.Empty);
                        IEnumerable<string> result = new string[] { nameLinkCQ.TextTrimDecode(), nameLinkCQ.Attr("href") }
                            .Concat(characterLinkCQ.SelectMany(characterLinkDom => new string[] { characterLinkDom.TextTrimDecode(), characterLinkDom.GetAttribute("href") }));
                        if (description.IsNotNullOrWhiteSpace())
                        {
                            result = result.Append(description);
                        }

                        return result.ToArray();
                    })
                    .ToArray()
            ))
            //.TakeWhile(section => !section.Category.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(section => section.Category, section => section.Credits);

        string goofsUrl = $"{imdbUrl}goofs/";
        (string goofsHtml, CQ goofsCQ) = await GetHtmlAsync(skipGoofs, goofsFile, goofsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, string[]> goofs = goofsCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                Category: sectionCQ.Find("h3").TextTrimDecode(),
                Descriptions: sectionCQ
                    .Find("[data-testid='item-html'] .ipc-html-content-inner-div")
                    .Find("*")
                    .Each(linkDom => linkDom.RemoveAttribute("class"))
                    .End()
                    .Select(divDom => divDom.HtmlTrim()).ToArray()))
            //.TakeWhile(section => !section.Category.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(section => section.Category, section => section.Descriptions);

        string keywordsUrl = $"{imdbUrl}keywords/";
        (string keywordsHtml, CQ keywordsCQ) = await GetHtmlAsync(skipKeywords, keywordsFile, keywordsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, string> allKeywords = keywordsCQ
            .Find("#keywords_content table td div.sodatext a")
            .Select(keyword => keyword.TextTrimDecode())
            .ToDictionary(keyword => keyword, keyword => string.Empty);
        if (allKeywords.IsEmpty())
        {
            keywordsCQ = keywordsHtml;
            allKeywords = keywordsCQ
                .Find("[data-testid='sub-section'] [data-testid='list-summary-item']")
                .Select(rowDom => rowDom.Cq().Find("a[href*='/search/title/?keywords=']"))
                .ToLookup(linkCQ => linkCQ.TextTrimDecode(), linkCQ => linkCQ.Attr("href"))
                .ToDictionary(
                    group => group.Key, 
                    group => group.Distinct(StringComparer.OrdinalIgnoreCase).Single());
        }

        string quotesUrl = $"{imdbUrl}quotes/";
        (string quotesHtml, CQ quotesCQ) = await GetHtmlAsync(skipQuotes, quotesFile, quotesUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[][] quotes = quotesCQ
            .Find("[data-testid='item-html']")
            .Select(quoteDom => quoteDom
                .Cq().Find("ul li")
                .Find("*")
                .Each(linkDom => linkDom.RemoveAttribute("class"))
                .End()
                .Select(itemDom => itemDom.HtmlTrim())
                .ToArray())
            .ToArray();

        string soundtracksUrl = $"{imdbUrl}soundtrack/";
        (string soundtracksHtml, CQ soundtracksCQ) = await GetHtmlAsync(skipSoundtracks, soundtracksFile, soundtracksUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[][] soundtracks = soundtracksCQ
            .Find("[data-testid='list-item']")
            .Select(itemDom => itemDom.Cq())
            .Select(itemCQ => itemCQ
                .Find(".ipc-html-content-inner-div")
                .Find("*")
                .Each(linkDom => linkDom.RemoveAttribute("class"))
                .End()
                .Select(divDom => divDom.HtmlTrim())
                .Prepend(itemCQ.Find("span:eq(0)").TextTrimDecode())
                .ToArray())
            .ToArray();

        string triviaUrl = $"{imdbUrl}trivia/";
        (string triviaHtml, CQ triviaCQ) = await GetHtmlAsync(skipTrivia, triviaFile, triviaUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, string[]> trivia = triviaCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                category: sectionCQ.Find("h3").TextTrimDecode(),
                Descriptions: sectionCQ
                    .Find("[data-testid='item-html'] .ipc-html-content-inner-div")
                    .Find("*")
                    .Each(linkDom => linkDom.RemoveAttribute("class"))
                    .End()
                    .Select(divDom => divDom.HtmlTrim())
                    .ToArray()
            ))
            //.TakeWhile(section => !section.category.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(section => section.category, section => section.Descriptions);

        string versionsUrl = $"{imdbUrl}alternateversions/";
        (string versionsHtml, CQ versionsCQ) = await GetHtmlAsync(skipVersions, versionsFile, versionsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[] versions = versionsCQ
            .Find("[data-testid='list-item'] .ipc-html-content-inner-div")
            .Find("*")
            .Each(linkDom => linkDom.RemoveAttribute("class"))
            .End()
            .Select(itemDom => itemDom.HtmlTrim())
            .ToArray();

        imdbMetadata = imdbMetadata with
        {
            Parent = parentMetadata,
            Year = year,
            Titles = titles,
            Title = title.TrimDecode(),
            OriginalTitle = originalTitle.TrimDecode(),
            Name = imdbMetadata.Name.TrimDecode(),
            AllKeywords = allKeywords,
            MpaaRating = mpaaRating,
            Advisories = advisories,
            ReleaseDates = releaseDates,
            Certifications = certifications,
            Connections = connections,
            AlternateVersions = versions,
            Soundtracks = soundtracks,
            CrazyCredits = crazyCredits,
            Credits = credits,
            Goofs = goofs,
            Quotes = quotes,
            Trivia = trivia,
            BoxOffice = boxOffice,
            TechSpecs = techSpecs,
            Awards = awards,
            AllAwards = allAwards,
            Details = details,
            Tagline = tagline
        };

        return (
            imdbMetadata,

            imdbUrl, imdbHtml,
            advisoriesUrl, advisoriesHtml,
            awardsUrl, awardsHtml,
            connectionsUrl, connectionsHtml,
            crazyCreditsUrl, crazyCreditsHtml,
            creditsUrl, creditsHtml,
            goofsUrl, goofsHtml,
            keywordsUrl, keywordsHtml,
            quotesUrl, quotesHtml,
            releasesUrl, releasesHtml,
            soundtracksUrl, soundtracksHtml,
            triviaUrl, triviaHtml,
            versionsUrl, versionsHtml,

            parentImdbUrl, parentImdbHtml,
            parentAdvisoriesUrl, parentAdvisoriesHtml,
            parentAwardsUrl, parentAwardsHtml,
            parentConnectionsUrl, parentConnectionsHtml,
            parentCrazyCreditsUrl, parentCrazyCreditsHtml,
            parentCreditsUrl, parentCreditsHtml,
            parentGoofsUrl, parentGoofsHtml,
            parentKeywordsUrl, parentKeywordsHtml,
            parentQuotesUrl, parentQuotesHtml,
            parentReleasesUrl, parentReleasesHtml,
            parentSoundtracksUrl, parentSoundtracksHtml,
            parentTriviaUrl, parentTriviaHtml,
            parentVersionsUrl, parentVersionsHtml
        );
    }

    internal static async Task DownloadAllMoviesAsync(ISettings settings, Func<int, Range>? getRange = null, int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        maxDegreeOfParallelism ??= MaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        WebDriverHelper.DisposeAll();

        ILookup<string, string> top = (await File.ReadAllLinesAsync(settings.TopMetadata, cancellationToken))
            .AsParallel()
            .Where(line => (line.ContainsIgnoreCase("|movies_x265|") || line.ContainsIgnoreCase("|movies|"))
                && (line.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopEnglishKeyword}") || line.ContainsIgnoreCase($"{Video.VersionSeparator}{settings.TopForeignKeyword}")))
            .Select(line => line.Split('|'))
            .Do(cells => Debug.Assert(string.IsNullOrEmpty(cells[^2]) || cells[^2].IsImdbId()))
            .Do(cells => Debug.Assert(cells[1].ContainsIgnoreCase($"-{settings.TopEnglishKeyword}") || cells[1].ContainsIgnoreCase($"-{settings.TopForeignKeyword}")))
            .ToLookup(cells => cells[^2], cells => cells[1]);

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> libraryMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264Metadata = await settings.LoadMovieTopH264MetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264720PMetadata = await settings.LoadMovieTopH264720PMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, List<PreferredMetadata>> preferredMetadata = await settings.LoadMoviePreferredMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> x265XMetadata = await settings.LoadMovieTopX265XMetadataAsync(cancellationToken);
        Dictionary<string, TopMetadata[]> h264XMetadata = await settings.LoadMovieTopH264XMetadataAsync(cancellationToken);
        //Dictionary<string, RareMetadata> rareMetadata = await JsonHelper.DeserializeFromFileAsync<Dictionary<string, RareMetadata>>(rareJsonPath);

        string[] cacheFiles = Directory.GetFiles(settings.MovieMetadataCacheDirectory);
        string[] metadataFiles = Directory.GetFiles(settings.MovieMetadataDirectory);
        string[] imdbIds = x265Metadata.Keys
            .Concat(x265XMetadata.Keys)
            .Concat(h264Metadata.Keys)
            .Concat(h264XMetadata.Keys)
            .Concat(preferredMetadata.Keys)
            .Concat(h264720PMetadata.Keys)
            .Concat(top.Select(group => group.Key))
            .Except(libraryMetadata.Keys)
            //.Except(rareMetadata
            //    .SelectMany(rare => Regex
            //        .Matches(rare.Value.Content, @"imdb\.com/title/(tt[0-9]+)")
            //        .Where(match => match.Success)
            //        .Select(match => match.Groups[1].Value)))
            .Distinct()
            .Order()
            .ToArray();
        int length = imdbIds.Length;
        if (getRange is not null)
        {
            imdbIds = imdbIds.Take(getRange(length)).ToArray();
        }

        imdbIds = imdbIds
            .Except(metadataFiles.Select(file => file.GetImdbId()))
            .ToArray();
        int trimmedLength = imdbIds.Length;
        ConcurrentQueue<string> imdbIdQueue = new(imdbIds);
        ConcurrentBag<string> imdbIdWithErrors = [];
        await Enumerable
            .Range(0, maxDegreeOfParallelism.Value)
            .ParallelForEachAsync(
                async (webDriverIndex, _, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    await using PlayWrightWrapper playWrightWrapper = new("https://www.imdb.com/");

                    while (imdbIdQueue.TryDequeue(out string? imdbId))
                    {
                        int index = trimmedLength - imdbIdQueue.Count;
                        log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                        try
                        {
                            await Retry.FixedIntervalAsync(
                                async () => await Video.DownloadImdbMetadataAsync(imdbId, settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, metadataFiles, cacheFiles, playWrightWrapper, overwrite: false, useCache: true, log: log, token),
                                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.InternalServerError }, cancellationToken: token);
                        }
                        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
                        {
                            log($"!!!{imdbId} {exception}");
                            imdbIdWithErrors.Add(imdbId);
                        }
                    }
                },
                maxDegreeOfParallelism,
                cancellationToken);

        imdbIdWithErrors.Prepend("IMDB ids with errors:").ForEach(log);
    }

    internal static async Task UpdateAllMoviesKeywordsAsync(ISettings settings, Func<int, Range>? getRange = null, int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        maxDegreeOfParallelism ??= MaxDegreeOfParallelism;

        WebDriverHelper.DisposeAll();
        string[] cacheFiles = Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, "*.Keywords.log").Order().ToArray();
        HashSet<string> keywordsFiles = new(Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, "*.Keywords.log.txt"), StringComparer.OrdinalIgnoreCase);
        int length = cacheFiles.Length;
        if (getRange is not null)
        {
            cacheFiles = cacheFiles.Take(getRange(length)).ToArray();
        }

        ConcurrentQueue<string> cacheFilesQueue = new(cacheFiles.Where(keywordFile => !keywordsFiles.Contains($"{keywordFile}.txt")));
        int totalDownloadCount = cacheFilesQueue.Count;
        log($"Download {totalDownloadCount}.");
        await Enumerable
            .Range(0, maxDegreeOfParallelism.Value)
            .ParallelForEachAsync(
                async (webDriverIndex, i, token) =>
                {
                    using WebDriverWrapper webDriver = new(() => WebDriverHelper.Start(webDriverIndex, keepExisting: true), "http://imdb.com");
                    while (cacheFilesQueue.TryDequeue(out string? keywordFile))
                    {
                        if (File.Exists($"{keywordFile}.txt"))
                        {
                            continue;
                        }

                        await Retry.FixedIntervalAsync(async () =>
                        {
                            string imdbId = PathHelper.GetFileNameWithoutExtension(keywordFile).Split(Video.Delimiter).First();
                            string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";
                            string keywordsUrl = $"{imdbUrl}keywords/";
                            string keywordsHtml = await webDriver.GetStringAsync(keywordsUrl, cancellationToken: token);
                            CQ keywordsCQ = keywordsHtml;
                            string[] allKeywords = keywordsCQ.Find("#keywords_content table td div.sodatext a").Select(keyword => keyword.TextTrimDecode()).ToArray();
                            Debug.Assert(allKeywords.IsEmpty());
                            if (true)
                            {
                                CQ seeAllButtonCQ = keywordsCQ.Find("span.chained-see-more-button button.ipc-see-more__button:contains('all') span.ipc-btn__text:visible");
                                Debug.Assert(seeAllButtonCQ.Length is 0 or 1);
                                if (true)
                                {
                                    if (seeAllButtonCQ.Any() && webDriver.Url.EqualsIgnoreCase(keywordsUrl))
                                    {
                                        int rowCount = webDriver.FindElements(By.CssSelector("div[data-testid='sub-section']>ul>li")).Count;
                                        IWebElement seeAllButton = webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.chained-see-more-button button.ipc-see-more__button")).Last());
                                        if (!seeAllButton.Displayed)
                                        {
                                            seeAllButtonCQ = keywordsCQ.Find("span.single-page-see-more-button button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible");
                                            seeAllButton = webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.single-page-see-more-button button.ipc-see-more__button")).Last());
                                        }

                                        Debug.Assert(seeAllButtonCQ.Length == 1);
                                        Debug.Assert(seeAllButton.Displayed);
                                        try
                                        {
                                            Retry.FixedInterval(seeAllButton.Click);
                                        }
                                        catch (Exception exception) when (exception.IsNotCritical())
                                        {
                                            seeAllButton.SendKeys(Keys.Space);
                                        }

                                        webDriver.Wait(WebDriverHelper.DefaultManualWait).Until(driver =>
                                        {
                                            keywordsHtml = webDriver.PageSource;
                                            keywordsCQ = keywordsHtml;
                                            rowCount = keywordsCQ.Find("div[data-testid='sub-section']>ul>li").Length;
                                            Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                                            return driver.FindElements(By.CssSelector("div[data-testid='sub-section']>ul>li")).Count == rowCount
                                                && keywordsCQ.Find("span.chained-see-more-button button.ipc-see-more__button:contains('all') span.ipc-btn__text:visible").IsEmpty()
                                                && keywordsCQ.Find("span.single-page-see-more-button button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").IsEmpty();
                                        });
                                    }

                                    keywordsHtml = webDriver.PageSource;
                                }

                                keywordsCQ = keywordsHtml;
                                CQ allKeywordsCQ = keywordsCQ.Find("div[data-testid='sub-section']>ul>li");
                                allKeywords = allKeywordsCQ
                                    .Select(row => row.Cq())
                                    .Select(rowCQ => rowCQ.Children().Eq(0).TextTrimDecode())
                                    .ToArray();

                                await FileHelper.WriteTextAsync(keywordFile, keywordsHtml, cancellationToken: token);
                                await FileHelper.WriteTextAsync($"{keywordFile}.txt", string.Join(Environment.NewLine, allKeywords), cancellationToken: token);
                            }
                        }, cancellationToken: token);

                        log($"{cacheFilesQueue.Count} of {totalDownloadCount} to download.");
                    }
                },
                maxDegreeOfParallelism,
                cancellationToken);
    }

    internal static async Task UpdateAllMoviesAdvisoriesAsync(ISettings settings, Func<int, Range>? getRange = null, int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        maxDegreeOfParallelism ??= MaxDegreeOfParallelism;

        WebDriverHelper.DisposeAll();

        HashSet<string> advisoriesFiles = new(Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, "*.Advisories.log.txt"), StringComparer.OrdinalIgnoreCase);
        HashSet<string> certificationsFiles = new(Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, "*.Advisories.log.Certifications.txt"), StringComparer.OrdinalIgnoreCase);
        string[] cacheFiles = Directory.EnumerateFiles(settings.MovieMetadataCacheDirectory, "*.Advisories.log")
            .Where(advisoriesFile => !(advisoriesFiles.Contains($"{advisoriesFile}.txt") && certificationsFiles.Contains($"{advisoriesFile}.Certifications.txt")))
            .Order()
            .ToArray();

        int length = cacheFiles.Length;
        ConcurrentQueue<string> cacheFilesQueue = new(getRange is not null ? cacheFiles.Take(getRange(length)) : cacheFiles);
        int totalDownloadCount = cacheFilesQueue.Count;
        log($"Download {totalDownloadCount}.");
        await Enumerable
            .Range(0, maxDegreeOfParallelism.Value)
            .ParallelForEachAsync(
                async (webDriverIndex, i, token) =>
                {
                    await using PlayWrightWrapper playWrightWrapper = new("http://imdb.com");
                    IPage page = await playWrightWrapper.PageAsync();
                    while (cacheFilesQueue.TryDequeue(out string? advisoriesFile))
                    {
                        if (File.Exists($"{advisoriesFile}.txt") && File.Exists($"{advisoriesFile}.Certifications.txt"))
                        {
                            continue;
                        }

                        await Retry.FixedIntervalAsync(async () =>
                        {
                            string imdbId = PathHelper.GetFileNameWithoutExtension(advisoriesFile).Split(Video.Delimiter).First();
                            string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";

                            string advisoriesUrl = $"{imdbUrl}parentalguide/";
                            string advisoriesHtml = File.Exists(advisoriesFile)
                                ? await File.ReadAllTextAsync(advisoriesFile, cancellationToken)
                                : await page.GetStringAsync(advisoriesUrl, PageSelector, cancellationToken: cancellationToken);
                            (bool isUpdated, advisoriesHtml, CQ advisoriesCQ) = await playWrightWrapper.TryUpdateAsync(log, cancellationToken);
                            log(isUpdated ? $"{imdbId} advisories is updated." : $"{imdbId} advisories is not updated.");

                            string mpaaRating = advisoriesCQ.Find("section[data-testid='content-rating'] ul li div.ipc-metadata-list-item__content-container").First().TextTrimDecode();
                            (string Category, string Severity, string[] Descriptions)[] advisories = advisoriesCQ
                                .Find("div.ipc-page-grid__item > section[data-testid='content-rating']")
                                .Siblings()
                                .TakeWhile(sectionDom => sectionDom.GetAttribute("data-testid").IsNullOrEmpty())
                                .Select(sectionDom => sectionDom.Cq())
                                .Select(sectionCQ => (
                                    Category: sectionCQ.Find("h3").TextTrimDecode(),
                                    Severity: sectionCQ.Find("div[data-testid='severity_component']").Children().First().TextTrimDecode(),
                                    Descriptions: sectionCQ.Find("div[data-testid^='sub-section-'] div[data-testid='item-html']").Select(item => item.TextTrimDecode()).ToArray()
                                ))
                                .Where(advisory => advisory.Severity.IsNotNullOrWhiteSpace() || advisory.Descriptions.Any())
                                .ToArray();

                            Dictionary<string, string> certifications = advisoriesCQ
                                .Find("section[data-testid='certificates'] ul li[data-testid='certificates-item']")
                                .SelectMany(regionDom =>
                                {
                                    CQ regionCQ = regionDom.Cq();
                                    string region = regionCQ.Find("span.ipc-metadata-list-item__label").TextTrimDecode();
                                    return regionCQ
                                        .Find("ul li")
                                        .Select(certificationDom =>
                                        {
                                            CQ certificationCQ = certificationDom.Cq();
                                            CQ certificationLinkCQ = certificationCQ.Find("a");
                                            string certification = certificationLinkCQ.TextTrimDecode();
                                            string link = certificationLinkCQ.Attr("href");
                                            string remark = certificationCQ.Find("span").TextTrimDecode();
                                            return (
                                                Certification: $"{region}:{certification}{(remark.IsNullOrWhiteSpace() ? string.Empty : $" ({remark})")}",
                                                Link: link
                                            );
                                        });
                                })
                                .DistinctBy(certification => certification.Certification)
                                .ToDictionary(certification => certification.Certification, certification => certification.Link);

                            Dictionary<string, (string Category, string Severity, string[] Descriptions)[]> result = new() { { mpaaRating, advisories } };
                            await FileHelper.WriteTextAsync(advisoriesFile, advisoriesHtml, cancellationToken: token);
                            await JsonHelper.SerializeToFileAsync(result, $"{advisoriesFile}.txt", token);
                            await JsonHelper.SerializeToFileAsync(certifications, $"{advisoriesFile}.Certifications.txt", token);
                        }, cancellationToken: token);

                        log($"{cacheFilesQueue.Count} of {totalDownloadCount} to download.");
                    }
                },
                maxDegreeOfParallelism,
                cancellationToken);
    }

    internal static async Task DownloadAllLibraryMoviesAsync(
        ISettings settings,
        string cacheDirectory, string metadataDirectory,
        Func<IEnumerable<string>, IEnumerable<string>>? partition = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> libraryMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);

        await using PlayWrightWrapper playWrightWrapper = new("https://www.imdb.com/");

        string[] cacheFiles = Directory.GetFiles(@cacheDirectory);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory);
        string[] imdbIds = libraryMetadata.Keys
            .Where(imdbId => imdbId.IsImdbId())
            .Order()
            .ToArray();
        //int length = imdbIds.Length;
        if (partition is not null)
        {
            imdbIds = partition(imdbIds).ToArray();
        }

        imdbIds = imdbIds
            .Except(metadataFiles.Select(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty))
            .ToArray();
        int trimmedLength = imdbIds.Length;
        await imdbIds.ForEachAsync(
            async (imdbId, index) =>
            {
                log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                try
                {
                    await Retry.FixedIntervalAsync(
                        async () => await Video.DownloadImdbMetadataAsync(imdbId, metadataDirectory, cacheDirectory, metadataFiles, cacheFiles, playWrightWrapper, overwrite: false, useCache: true, log: log, cancellationToken: cancellationToken),
                        isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.InternalServerError },
                        cancellationToken: cancellationToken);
                }
                catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
                {
                    log($"!!!{imdbId} {exception.ToString()}");
                }
            },
            cancellationToken);
    }

    internal static async Task DownloadAllTVsAsync(
        ISettings settings,
        string[] tvDirectories, string cacheDirectory, string metadataDirectory,
        Func<int, Range>? getRange = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        Dictionary<string, TopMetadata[]> x265Metadata = await settings.LoadMovieTopX265MetadataAsync(cancellationToken);
        string[] libraryImdbIds = tvDirectories.SelectMany(tvDirectory => Directory.EnumerateFiles(tvDirectory, Video.ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .Select(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty)
            .Where(imdbId => imdbId.IsNotNullOrWhiteSpace())
            .ToArray();

        await using PlayWrightWrapper playWrightWrapper = new("https://www.imdb.com/");

        string[] cacheFiles = Directory.GetFiles(cacheDirectory);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory);
        string[] imdbIds = x265Metadata.Keys
            .Distinct()
            .Except(libraryImdbIds)
            .Order()
            .ToArray();
        if (getRange is not null)
        {
            int length = imdbIds.Length;
            imdbIds = imdbIds.Take(getRange(length)).ToArray();
        }

        imdbIds = imdbIds
            .Except(metadataFiles.Select(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty))
            .ToArray();
        int trimmedLength = imdbIds.Length;
        await imdbIds.ForEachAsync(
            async (imdbId, index) =>
            {
                log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                try
                {
                    await Retry.FixedIntervalAsync(
                        async () => await Video.DownloadImdbMetadataAsync(imdbId, metadataDirectory, cacheDirectory, metadataFiles, cacheFiles, playWrightWrapper, overwrite: false, useCache: true, log: log, cancellationToken: cancellationToken),
                        isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.InternalServerError },
                        cancellationToken: cancellationToken);
                }
                catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
                {
                    log($"!!!{imdbId} {exception}");
                }
            },
            cancellationToken: cancellationToken);
    }

    [GeneratedRegex("[0-9]{4}")]
    private static partial Regex YearRegex();

    [GeneratedRegex("[0-9]+ more")]
    private static partial Regex SeeMoreButtonRegex();
}