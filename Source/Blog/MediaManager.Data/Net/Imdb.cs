namespace MediaManager.Net;

using System.Web;
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
    internal static readonly int MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8);

    private static async Task<(bool IsUpdated, string Html, CQ trimmedCQ)> TryUpdateAsync(this PlayWrightWrapper playWrightWrapper, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        bool isUpdated = false;
        IPage page = await playWrightWrapper.PageAsync();
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
                        throw new InvalidOperationException("Ajax error.");
                    }
                }

                log($"{spoilerButtonCount} Spoilers are waited.");
                await page.WaitForVisibleAsync(new Regex("^Spoilers$"), new PageGetByTextOptions() { Exact = true }, spoilerButtonCount, cancellationToken);
                log($"{spoilerButtonCount} Spoilers are loaded.");

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
                        throw new InvalidOperationException("Ajax error.");
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
                    await page.WaitForVisibleAsync(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }, targetSeeMoreButtonsCount, cancellationToken);
                    int actualSeeMoreButtonCount = await page.ClickOrPressAsync(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }, cancellationToken);
                    Debug.Assert(targetSeeMoreButtonsCount == actualSeeMoreButtonCount);

                    isUpdated = true;
                    await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
                    if (await page.GetByText("error fetching more data").CountAsync() > 0
                        || await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }).CountAsync() > 0)
                    {
                        throw new InvalidOperationException("Ajax error.");
                    }
                }
                else
                {
                    await page.WaitForHiddenAsync(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = SeeMoreButtonRegex() }, cancellationToken);
                }
            },
            retryingHandler: async void (sender, arg) =>
            {
                log($"Update retry count {arg.CurrentRetryCount}.");
                log(arg.LastException.ToString());
                await page.RefreshAsync(cancellationToken: cancellationToken);
            },
            cancellationToken: cancellationToken);

        if (isUpdated)
        {
            await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
            html = await page.ContentAsync();
            trimmedCQ = TrimPage(html);
        }

        if (trimmedCQ.Find("section span.ipc-see-more button:contains('Spoilers')").Any()
            || trimmedCQ.Find("section > div.ipc-signpost > div.ipc-signpost__text:contains('Spoilers')").Length != spoilersButtonsCount
            || trimmedCQ.Find("button.ipc-see-more__button:contains('all') span.ipc-btn__text:visible").Any()
            || trimmedCQ.Find("button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").Any())
        {
            throw new InvalidOperationException($"Failed to update {page.Url}");
        }

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
            string cacheHtml = await File.ReadAllTextAsync(file, cancellationToken);
            return (cacheHtml, TrimPage(cacheHtml));
        }

        if (playWrightWrapper is null)
        {
            Debug.Assert(httpClient is not null);
            string downloadedHtml = await Retry.FixedIntervalAsync(async () => await httpClient.GetStringAsync(url, cancellationToken), cancellationToken: cancellationToken);
            return (downloadedHtml, TrimPage(downloadedHtml));
        }

        await playWrightWrapper.GetStringAsync(url);
        (bool isUpdated, string html, CQ trimmedCQ) = await playWrightWrapper.TryUpdateAsync(log, cancellationToken);
        log(isUpdated ? $"{url} is updated." : $"{url} is not updated.");
        return (html, trimmedCQ);
    }

    private static CQ TrimPage(CQ pageCQ)
    {
        CQ trimmedCQ = pageCQ.Find("main .ipc-page-grid__item").Eq(0);
        CQ remove = trimmedCQ.Find("[data-testid='contribution'], [data-testid='more-from-section']");
        Debug.Assert(remove.Any());
        remove.Remove();
        return trimmedCQ;
    }

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
        string imdbHtml = hasImdbFile
            ? await File.ReadAllTextAsync(imdbFile, cancellationToken)
            : await (page?.GetStringAsync(imdbUrl, new PageGotoOptions() { Referer = "https://www.imdb.com/" }, cancellationToken)
                ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(imdbUrl, cancellationToken), cancellationToken: cancellationToken));

        if (!hasImdbFile && page is not null && !page.Url.EqualsOrdinal(imdbUrl))
        {
            log($"Redirected {imdbId} to {page.Url}");
            imdbUrl = page.Url;
            imdbId = ImdbMetadata.ImdbIdInLinkRegex().Match(imdbUrl).Value;
        }

        if (!hasImdbFile && page is not null)
        {
            ILocator knowLocator = page.GetByTestId("DidYouKnow");
            if (await knowLocator.CountAsync() > 0)
            {
                await knowLocator.ScrollIntoViewIfNeededAsync();
            }
            else
            {
                ILocator topPicksLocator = page.Locator("[data-cel-widget='DynamicFeature_TopPicks']");
                Debug.Assert(await topPicksLocator.CountAsync() > 0);
                await topPicksLocator.ScrollIntoViewIfNeededAsync();
            }

            await page.Keyboard.PressAsync("PageUp");
            await page.WaitForSelectorAsync("[data-testid='storyline-parents-guide']");
            imdbHtml = await page.ContentAsync();
        }

        CQ imdbCQ = imdbHtml;
        string json = imdbCQ.Find("script[type='application/ld+json']").Text();
        //string json2 = imdbCQ.Find("#__NEXT_DATA__").Text();
        string htmlTitle = imdbCQ.Find("title").Text().Trim();
        if (htmlTitle.StartsWithIgnoreCase("500 Error") || imdbCQ.Find("[data-testid='error-page-title']").Text().ContainsIgnoreCase("500 Error"))
        {
            throw new HttpRequestException(HttpRequestError.InvalidResponse, imdbUrl, null, HttpStatusCode.InternalServerError);
        }

        if (htmlTitle.StartsWithIgnoreCase("404 Error") || imdbCQ.Find("[data-testid='error-page-title']").Text().ContainsIgnoreCase("404 Error"))
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

        string htmlYear = imdbCQ.Find("[data-testid='hero__pageTitle']").NextAll().Find("li:eq(0)").Text().Trim();
        htmlYear = YearRegex().Match(htmlYear).Value;
        if (!YearRegex().IsMatch(htmlYear))
        {
            htmlYear = imdbCQ.Find("[data-testid='hero__pageTitle']").NextAll().Find("li:eq(1)").Text().Trim();
            htmlYear = YearRegex().Match(htmlYear).Value;
        }

        //if (htmlYear.IsNullOrWhiteSpace())
        //{
        //    htmlYear = imdbCQ.Find("""div.title_wrapper div.subtext a[title="See more release dates"]""").Text();
        //    htmlYear = YearRegex().Match(htmlYear).Value;
        //}

        if (htmlYear.IsNullOrWhiteSpace())
        {
            htmlYear = imdbCQ.Find($"a[href*='/title/{imdbId}/releaseinfo']").Select(dom => dom.TextContent.Trim()).FirstOrDefault(text => YearRegex().IsMatch(text), string.Empty);
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
                return (Key: itemCQ.Children().Eq(0), Values: itemCQ.Find("ul li"));
            })
            .Where(item => item.Values.Any())
            .ToDictionary(
                itemCQ => itemCQ.Key.Text().Trim(),
                itemCQ => itemCQ.Values
                    .Select(innerItemDom =>
                    {
                        CQ innerItemCQ = innerItemDom.Cq();
                        CQ linkCQ = innerItemCQ.Find("a");
                        CQ descriptionCQ = innerItemCQ.Find("span");
                        string[] innerItems = [];
                        if (linkCQ.Any())
                        {
                            innerItems = [linkCQ.Text().Trim(), linkCQ.Attr("href")];
                        }

                        if (descriptionCQ.Any())
                        {
                            innerItems = [.. innerItems, descriptionCQ.Text().Trim()];
                        }

                        return innerItems;
                    })
                    .ToArray());

        Dictionary<string, string[]> boxOffice = imdbCQ
            .Find("[data-testid='BoxOffice'] ul li.ipc-metadata-list__item")
            .Select(itemDom => itemDom.Cq())
            .ToDictionary(
                itemCQ => itemCQ.Find("span").Eq(0).Text().Trim(),
                itemCQ => itemCQ.Find("li").Select(innerItemDom => innerItemDom.TextContent.Trim()).ToArray());

        Dictionary<string, string[]> techSpecs = imdbCQ
            .Find("[data-testid='TechSpecs'] ul li.ipc-metadata-list__item")
            .Select(itemDom => itemDom.Cq())
            .ToDictionary(
                itemCQ => itemCQ.Find("span").Eq(0).Text().Trim(),
                itemCQ => itemCQ.Find("li").Select(innerItemDom => innerItemDom.TextContent.Trim()).ToArray());

        CQ awardsDivCQ = imdbCQ.Find("[data-testid='awards']");
        string topRated = awardsDivCQ.Find("[data-testid='award_top-rated']").Text().Trim();
        CQ awardsInfoCQ = awardsDivCQ.Find("[data-testid='award_information']");
        string[] awards = awardsInfoCQ
            .Find("a")
            .SkipLast(1)
            .Select(linkDom => linkDom.TextContent.Trim())
            .Concat(awardsInfoCQ.Find("ul li").Select(itemDom => itemDom.TextContent.Trim()))
            .ToArray();
        if (topRated.IsNotNullOrWhiteSpace())
        {
            awards = [topRated, .. awards];
        }

        bool skipAwards = awardsInfoCQ.Find("a").IsEmpty();

        CQ taglineCQ = imdbCQ.Find("[data-testid='storyline-taglines'] ul li");
        Debug.Assert(taglineCQ.Length<=1);
        string tagline = taglineCQ.Text().Trim();
        CQ storylineSectionCQ = imdbCQ.Find("[data-testid='Storyline']");
        Debug.Assert(storylineSectionCQ.Any());
        bool skipAdvisories = storylineSectionCQ.Find("a:Contains('Add content advisory')").Any()
            && storylineSectionCQ.Find("[data-testid='storyline-certificate']").IsEmpty();
        bool skipKeywords = storylineSectionCQ.Find($"a[href*='/title/{imdbId}/keywords/']:Contains('more')").IsEmpty();

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
            skipTrivia = knowSection.Find("a:Contains('Trivia')").IsEmpty();
            skipGoofs = knowSection.Find("a:Contains('Goofs')").IsEmpty();
            skipQuotes = knowSection.Find("a:Contains('Quotes')").IsEmpty();
            skipCrazyCredits = knowSection.Find("a:Contains('Crazy credits')").IsEmpty();
            skipVersions = knowSection.Find("a:Contains('Alternate versions')").IsEmpty();
            skipConnections = knowSection.Find("a:Contains('Connections')").IsEmpty();
            skipSoundtracks = knowSection.Find("a:Contains('Soundtracks')").IsEmpty();
        }

        string releasesUrl = $"{imdbUrl}releaseinfo/";
        (string releasesHtml, CQ releasesCQ) = await GetHtmlAsync(false, releasesFile, releasesUrl, playWrightWrapper, httpClient, log, cancellationToken);

        CQ allDatesCQ = releasesCQ.Find("[data-testid='sub-section-releases'] > ul > li");
        (string DateKey, string DateValue)[] allDates = allDatesCQ
            .Select(row => row.Cq())
            .Select(rowCQ => (Key: rowCQ.Children().Eq(0), Values: rowCQ.Children().Eq(1).Find("li")))
            .SelectMany(row => row.Values.Select(value => (Key: HttpUtility.HtmlDecode(row.Key.Text().Trim()), Value: HttpUtility.HtmlDecode(value.TextContent.Trim()))))
            .ToArray();
        Dictionary<string, string[]> dates = allDates
            .ToLookup(row => row.DateKey, row => row.DateValue, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray());

        CQ allTitlesCQ = releasesCQ.Find("#akas").Next();
        (string TitleKey, string TitleValue)[] allTitles = allTitlesCQ
            .Find("tr")
            .Select(row => row.Cq().Children())
            .Select(cells => (TitleKey: HttpUtility.HtmlDecode(cells.First().Text().Trim()), TitleValue: HttpUtility.HtmlDecode(cells.Last().Text().Trim()))).ToArray();
        Enumerable.Range(0, allTitles.Length).ForEach(index =>
        {
            if (allTitles[index].TitleKey.IsNullOrWhiteSpace())
            {
                allTitles[index].TitleKey = allTitles[index - 1].TitleKey;
            }
        });

        if (allTitles.IsEmpty())
        {
            allTitlesCQ = releasesCQ.Find("[data-testid='sub-section-akas'] > ul > li");
            allTitles = allTitlesCQ
                .Select(row => row.Cq())
                .Select(rowCQ => (Key: rowCQ.Children().Eq(0), Values: rowCQ.Children().Eq(1).Find("li")))
                .SelectMany(row => row.Values.Select(value => (Key: HttpUtility.HtmlDecode(row.Key.Text().Trim()), Value: HttpUtility.HtmlDecode(value.TextContent.Trim()))))
                .ToArray();
        }

        Dictionary<string, string[]> titles = allTitles
            .ToLookup(row => row.TitleKey, row => row.TitleValue, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray());

        //Debug.Assert(titles.Any()
        //    || allTitlesCQ.Text().ContainsIgnoreCase("It looks like we don't have any AKAs for this title yet.")
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
                .Text()
                .Trim();

            if (title.IsNullOrWhiteSpace())
            {
                title = imdbCQ
                     .Find("[data-testid='hero-title-block__title']")
                     .Text()
                     .Trim();
            }

            originalTitle = imdbCQ.Find("[data-testid='hero__pageTitle']").Next("div").Text().Trim();
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
                    .Text()
                    .Trim();
            }

            if (originalTitle.IsNullOrWhiteSpace())
            {
                originalTitle = imdbCQ
                    .Find("[data-testid='hero-title-block__original-title']")
                    .Text()
                    .Trim()
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
                    string releaseHtmlTitle = releasesCQ.Find("title").Text();
                    releaseHtmlTitle = releaseHtmlTitle[..releaseHtmlTitle.LastIndexOfOrdinal("(")].Trim();

                    if (!titles.TryGetValue("(original title)", out string[]? originalTitleValues))
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
                        originalTitleValues = originalTitleValues.Where(titleValue => !titleValue.EqualsOrdinal(releaseHtmlTitle)).ToArray();
                    }

                    if (originalTitleValues.Length > 1)
                    {
                        originalTitleValues = originalTitleValues.Where(titleValue => !titleValue.EqualsIgnoreCase(releaseHtmlTitle)).ToArray();
                    }

                    originalTitle = originalTitleValues.Length switch
                    {
                        1 => originalTitleValues.Single(),
                        > 1 => originalTitleValues.FirstOrDefault(titleValue => originalTitleValues.Except(EnumerableEx.Return(titleValue)).All(titleValue.ContainsIgnoreCase))
                            ?? string.Join(Video.TitleSeparator, originalTitleValues),
                        _ => string.Empty
                    };

                    if (!titles.TryGetValue("World-wide (English title)", out string[]? titleValues))
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
                        1 => titleValues.Single(),
                        > 1 => titleValues.FirstOrDefault(availableTitle => releaseHtmlTitle.EqualsOrdinal(availableTitle))
                            ?? titleValues.FirstOrDefault(availableTitle => releaseHtmlTitle.EqualsIgnoreCase(availableTitle))
                            ?? titleValues.FirstOrDefault(titleValue => titleValues.Except(EnumerableEx.Return(titleValue)).All(titleValue.ContainsIgnoreCase))
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
                        .Text()
                        .Replace("&nbsp;", string.Empty)
                        .Trim();

                    originalTitle = imdbCQ
                        .Find("div.originalTitle")
                        .Find("span")
                        .Remove()
                        .End()
                        .Text()
                        .Trim();

                    if (title.IsNullOrWhiteSpace())
                    {
                        title = imdbCQ
                            .Find("[data-testid='hero-title-block__title']")
                            .Find("#titleYear")
                            .Remove()
                            .End()
                            .Text()
                            .Trim();
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

        string mpaaRating = advisoriesCQ.Find("[data-testid='content-rating'] ul li div.ipc-metadata-list-item__content-container").First().Text().Trim();

        (string Category, string Severity, string[] Descriptions)[] advisories = advisoriesCQ
            .Find("[data-testid='content-rating']")
            .Siblings()
            .TakeWhile(sectionDom => sectionDom.GetAttribute("data-testid").IsNullOrEmpty())
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                Category: sectionCQ.Find("h3").Text().Trim(),
                Severity: sectionCQ.Find("[data-testid='severity_component']").Children().First().Text().Trim(),
                Descriptions: sectionCQ.Find("[data-testid^='sub-section-'] [data-testid='item-html']").Select(item => item.TextContent.Trim()).ToArray()
            ))
            .Where(advisory => advisory.Severity.IsNotNullOrWhiteSpace() || advisory.Descriptions.Any())
            .ToArray();

        Dictionary<string, string> certifications = advisoriesCQ
            .Find("[data-testid='certificates'] [data-testid='certificates-item']")
            .SelectMany(regionDom =>
            {
                CQ regionCQ = regionDom.Cq();
                string region = regionCQ.Find("span.ipc-metadata-list-item__label").Text().Trim();
                return regionCQ
                    .Find("ul li")
                    .Select(certificationDom =>
                    {
                        CQ certificationCQ = certificationDom.Cq();
                        CQ certificationLinkCQ = certificationCQ.Find("a");
                        string certification = certificationLinkCQ.Text().Trim();
                        string link = certificationLinkCQ.Attr("href");
                        string remark = certificationCQ.Find("span").Text().Trim();
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
                    eventCQ.Text().Trim(),
                    eventCQ.Parent().Attr("href"),
                    sectionCQ
                        .Find("li.ipc-metadata-list-summary-item")
                        .Select(itemDom =>
                        {
                            CQ itemCQ = itemDom.Cq();
                            CQ statusCQ = itemCQ.Find("a:eq(1)");
                            CQ titleCQ = statusCQ.Find("span").Remove();
                            return new ImdbAward(
                                statusCQ.Text().Trim(),
                                statusCQ.Attr("href"),
                                titleCQ.Text().Trim(),
                                itemCQ.Find("ul:eq(0)").Text().Trim(),
                                itemCQ.Find("span.ipc-expandableSection__content").Text().Trim(),
                                itemCQ
                                    .Find("ul:eq(1) li a")
                                    .Select(listItemDom =>
                                    {
                                        CQ listItemCQ = listItemDom.Cq();
                                        string name = listItemCQ.Text().Trim();
                                        string url = listItemCQ.Attr("href");
                                        string description = listItemCQ.Next().Text().Trim();
                                        return new string[] { name, url, description };
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

        Dictionary<string, ImdbConnection[]> connections = connectionsCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(
                sectionCQ => (Key: sectionCQ.Find("div.ipc-title h3.ipc-title__text").Text().Trim(),
                Value: sectionCQ
                    .Find("ul.ipc-metadata-list > li")
                    .Select(listItemDom =>
                    {
                        CQ listItemDivCQ = listItemDom.Cq().Find("ul > div");
                        CQ linkCQ = listItemDivCQ.Eq(0).Find("a");
                        return new ImdbConnection(
                            linkCQ.Text().Trim(),
                            linkCQ.Attr("href"),
                            linkCQ.Remove().End().Text().Trim(),
                            listItemDivCQ.Eq(1).Text().Trim());
                    })
                    .ToArray()))
            .Where(pair => pair.Key.IsNotNullOrWhiteSpace())
            //.TakeWhile(pair => !pair.Key.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        string crazyCreditsUrl = $"{imdbUrl}crazycredits/";
        (string crazyCreditsHtml, CQ crazyCreditsCQ) = await GetHtmlAsync(skipCrazyCredits, crazyCreditsFile, crazyCreditsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[] crazyCredits = crazyCreditsCQ
            .Find("section.ipc-page-section p.crazy-credit-text")
            .Find("a")
            .Each(linkDom => linkDom.RemoveAttribute("class"))
            .End()
            .Select(paragraphDom => paragraphDom.InnerHTML.Trim()).ToArray();

        string creditsUrl = $"{imdbUrl}fullcredits/";
        (string creditsHtml, CQ creditsCQ) = await GetHtmlAsync(false, creditsFile, creditsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, ImdbCredit[]> credits = creditsCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                Category: sectionCQ.Find("h3").Text().Trim(),
                Credits: sectionCQ
                    .Find("ul li")
                    .Select(itemDom => itemDom.Cq())
                    .Select(itemCQ => new ImdbCredit(
                        itemCQ.Find("a").Text().Trim(),
                        itemCQ.Find("a").Attr("href"),
                        itemCQ.Find(".name-credits--crew-metadata").Text().Trim()
                    ))
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
                Category: sectionCQ.Find("h3").Text().Trim(),
                Descriptions: sectionCQ
                    .Find("div.ipc-html-content-inner-div")
                    .Find("a").Each(linkDom => linkDom.RemoveAttribute("class"))
                    .End()
                    .Select(divDom => divDom.InnerHTML.Trim()).ToArray()))
            //.TakeWhile(section => !section.Category.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(section => section.Category, section => section.Descriptions);

        string keywordsUrl = $"{imdbUrl}keywords/";
        (string keywordsHtml, CQ keywordsCQ) = await GetHtmlAsync(skipKeywords, keywordsFile, keywordsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[] allKeywords = keywordsCQ.Find("#keywords_content table td div.sodatext a").Select(keyword => keyword.TextContent.Trim()).ToArray();
        if (allKeywords.IsEmpty())
        {
            keywordsCQ = keywordsHtml;
            CQ allKeywordsCQ = keywordsCQ.Find("[data-testid='sub-section'] > ul > li");
            allKeywords = allKeywordsCQ
                .Select(row => row.Cq())
                .Select(rowCQ => HttpUtility.HtmlDecode(rowCQ.Children().Eq(0).Text()))
                .ToArray();
        }

        string quotesUrl = $"{imdbUrl}quotes/";
        (string quotesHtml, CQ quotesCQ) = await GetHtmlAsync(skipQuotes, quotesFile, quotesUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[][] quotes = quotesCQ
            .Find("section.ipc-page-section div.ipc-html-content-inner-div ul")
            .Select(quoteDom => quoteDom
                .Cq().Find("li")
                .Find("a").Each(linkDom => linkDom.RemoveAttribute("class"))
                .End()
                .Select(itemDom => itemDom.InnerHTML.Trim())
                .ToArray())
            .ToArray();

        string soundtracksUrl = $"{imdbUrl}soundtrack/";
        (string soundtracksHtml, CQ soundtracksCQ) = await GetHtmlAsync(skipSoundtracks, soundtracksFile, soundtracksUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[][] soundtracks = soundtracksCQ
            .Find("section.ipc-page-section li.ipc-metadata-list__item")
            .Select(itemDom => itemDom.Cq())
            .Select(itemCQ => itemCQ
                .Find("div.ipc-html-content-inner-div")
                .Find("a").Each(linkDom => linkDom.RemoveAttribute("class"))
                .End()
                .Select(divDom => divDom.InnerHTML.Trim())
                .Prepend(itemCQ.Find("span").Eq(0).Text().Trim())
                .ToArray())
            .ToArray();

        string triviaUrl = $"{imdbUrl}trivia/";
        (string triviaHtml, CQ triviaCQ) = await GetHtmlAsync(skipTrivia, triviaFile, triviaUrl, playWrightWrapper, httpClient, log, cancellationToken);

        Dictionary<string, string[]> trivia = triviaCQ
            .Find("section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(sectionCQ => (
                category: sectionCQ.Find("h3").Text().Trim(),
                Descriptions: sectionCQ
                    .Find("div.ipc-html-content-inner-div")
                    .Find("a").Each(linkDom => linkDom.RemoveAttribute("class"))
                    .End()
                    .Select(divDom => divDom.InnerHTML.Trim())
                    .ToArray()
            ))
            //.TakeWhile(section => !section.category.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(section => section.category, section => section.Descriptions);

        string versionsUrl = $"{imdbUrl}alternateversions/";
        (string versionsHtml, CQ versionsCQ) = await GetHtmlAsync(skipVersions, versionsFile, versionsUrl, playWrightWrapper, httpClient, log, cancellationToken);

        string[] versions = versionsCQ.Find("section.ipc-page-section ul li").Select(itemDom => itemDom.TextContent.Trim()).ToArray();

        imdbMetadata = imdbMetadata with
        {
            Parent = parentMetadata,
            Year = year,
            //Regions = regions,
            //Languages = languages,
            Titles = titles,
            Title = HttpUtility.HtmlDecode(title).Trim(),
            OriginalTitle = HttpUtility.HtmlDecode(originalTitle).Trim(),
            Name = HttpUtility.HtmlDecode(imdbMetadata.Name).Trim(),
            AllKeywords = allKeywords,
            MpaaRating = mpaaRating,
            Advisories = advisories.ToLookup(advisory => advisory.Category).ToDictionary(group => group.Key, group => group.ToDictionary(advisory => advisory.Severity, advisory => advisory.Descriptions)),
            //AlsoKnownAs = alsoKnownAs,
            //Genres = imdbMetadata.Genres,
            Releases = dates,
            //Websites = websites
            //    .Distinct()
            //    .ToLookup(item => item.Text, item => item.Url)
            //    .ToDictionary(group => HttpUtility.HtmlDecode(group.Key), group => group.ToArray()),
            //FilmingLocations = locations
            //    .Distinct()
            //    .ToDictionary(item => item.Text, item => item.Url),
            //Companies = companies
            //    .Distinct()
            //    .ToLookup(item => item.Text, item => item.Url)
            //    .ToDictionary(group => HttpUtility.HtmlDecode(group.Key), group => group.ToArray()),
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
                            string[] allKeywords = keywordsCQ.Find("#keywords_content table td div.sodatext a").Select(keyword => keyword.TextContent.Trim()).ToArray();
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
                                    .Select(rowCQ => HttpUtility.HtmlDecode(rowCQ.Children().Eq(0).Text()))
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
                                : await page.GetStringAsync(advisoriesUrl, cancellationToken: cancellationToken);
                            (bool isUpdated, advisoriesHtml, CQ advisoriesCQ) = await playWrightWrapper.TryUpdateAsync(log, cancellationToken);
                            log(isUpdated ? $"{imdbId} advisories is updated." : $"{imdbId} advisories is not updated.");

                            string mpaaRating = advisoriesCQ.Find("section[data-testid='content-rating'] ul li div.ipc-metadata-list-item__content-container").First().Text().Trim();
                            (string Category, string Severity, string[] Descriptions)[] advisories = advisoriesCQ
                                .Find("div.ipc-page-grid__item > section[data-testid='content-rating']")
                                .Siblings()
                                .TakeWhile(sectionDom => sectionDom.GetAttribute("data-testid").IsNullOrEmpty())
                                .Select(sectionDom => sectionDom.Cq())
                                .Select(sectionCQ => (
                                    Category: sectionCQ.Find("h3").Text().Trim(),
                                    Severity: sectionCQ.Find("div[data-testid='severity_component']").Children().First().Text().Trim(),
                                    Descriptions: sectionCQ.Find("div[data-testid^='sub-section-'] div[data-testid='item-html']").Select(item => item.TextContent.Trim()).ToArray()
                                ))
                                .Where(advisory => advisory.Severity.IsNotNullOrWhiteSpace() || advisory.Descriptions.Any())
                                .ToArray();

                            Dictionary<string, string> certifications = advisoriesCQ
                                .Find("section[data-testid='certificates'] ul li[data-testid='certificates-item']")
                                .SelectMany(regionDom =>
                                {
                                    CQ regionCQ = regionDom.Cq();
                                    string region = regionCQ.Find("span.ipc-metadata-list-item__label").Text().Trim();
                                    return regionCQ
                                        .Find("ul li")
                                        .Select(certificationDom =>
                                        {
                                            CQ certificationCQ = certificationDom.Cq();
                                            CQ certificationLinkCQ = certificationCQ.Find("a");
                                            string certification = certificationLinkCQ.Text().Trim();
                                            string link = certificationLinkCQ.Attr("href");
                                            string remark = certificationCQ.Find("span").Text().Trim();
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