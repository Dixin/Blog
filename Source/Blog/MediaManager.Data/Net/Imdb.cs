namespace MediaManager.Net;

using System;
using System.Threading;
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

    private static async Task<bool> UpdateAsync(this IPage page, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        bool isUpdated = false;
        await Retry.FixedIntervalAsync(
            func: async () =>
            {
                ILocator seeAllButtons = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "See all" });
                ILocator seeMoreButtons = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = new Regex(@"[0-9]+ more") });
                int seeAllButtonsCount = await seeAllButtons.CountAsync();
                int seeMoreButtonsCount = await seeMoreButtons.CountAsync();
                log($"{seeAllButtonsCount} See all buttons are found.");
                if (seeAllButtonsCount > 0)
                {
                    await (await seeAllButtons.AllAsync()).Reverse().ToArray().ForEachAsync(
                        async button =>
                        {
                            try
                            {
                                await button.ClickAsync();
                            }
                            catch (Exception exception) when (exception.IsNotCritical())
                            {
                                await button.PressAsync(" ");
                            }

                            //await button.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Detached });
                        },
                        cancellationToken);
                    log($"{seeAllButtonsCount} See all buttons are clicked and waited.");
                    //await seeAllButtons.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Detached });
                    long seeAllDetachingTimestamp = Stopwatch.GetTimestamp();
                    while (await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "See all" }).CountAsync() > 0)
                    {
                        if (Stopwatch.GetElapsedTime(seeAllDetachingTimestamp) > PageHelper.DefaultManualWait)
                        {
                            throw new TimeoutException("Waiting for See all buttons to be detached timed out.");
                        }

                        await Task.Delay(PageHelper.DefaultDomWait, cancellationToken);
                    }

                    log($"{seeAllButtonsCount} See all buttons are detached.");
                    isUpdated = true;
                }

                seeMoreButtonsCount -= seeAllButtonsCount;
                long seeMoreInitializingTimestamp = Stopwatch.GetTimestamp();
                while (await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = new Regex(@"[0-9]+ more") }).CountAsync() != seeMoreButtonsCount)
                {
                    if (Stopwatch.GetElapsedTime(seeMoreInitializingTimestamp) > PageHelper.DefaultManualWait)
                    {
                        throw new TimeoutException("Waiting for See all buttons to be initialized timed out.");
                    }

                    await Task.Delay(PageHelper.DefaultDomWait, cancellationToken);
                    //string[] xx = (await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = new Regex(@"[0-9]+ more") })
                    //    .AllAsync()).Select(locator => locator.TextContentAsync().Result ?? string.Empty).ToArray();

                    //log(xx.Length.ToString());
                }

                log($"{seeMoreButtonsCount} See more buttons are found.");
                if (seeMoreButtonsCount > 0)
                {
                    await (await seeMoreButtons.AllAsync()).Reverse().ToArray().ForEachAsync(
                        async button =>
                        {
                            try
                            {
                                await button.ClickAsync();
                            }
                            catch (Exception exception) when (exception.IsNotCritical())
                            {
                                await button.PressAsync(" ");
                            }

                            //await button.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Detached });
                        },
                        cancellationToken);
                    log($"{seeMoreButtonsCount} See more buttons are clicked and waited.");
                    //await seeMoreButtons.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Detached });
                    long seeMoreDetachingTimestamp = Stopwatch.GetTimestamp();
                    while (await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { NameRegex = new Regex(@"[0-9]+ more") }).CountAsync() > 0)
                    {
                        if (Stopwatch.GetElapsedTime(seeMoreDetachingTimestamp) > PageHelper.DefaultManualWait)
                        {
                            throw new TimeoutException("Waiting for See more buttons to be detached timed out.");
                        }

                        await Task.Delay(PageHelper.DefaultDomWait, cancellationToken);
                    }
                    log($"{seeMoreButtonsCount} See more buttons are detached.");
                    isUpdated = true;
                }

                ILocator spoilerButtons = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Spoiler" });
                int spoilerButtonCount = await spoilerButtons.CountAsync();
                log($"{spoilerButtonCount} Spoiler buttons are found.");
                if (spoilerButtonCount > 0)
                {
                    await (await spoilerButtons.AllAsync()).Reverse().ToArray().ForEachAsync(
                        async button =>
                        {
                            try
                            {
                                await button.ClickAsync();
                            }
                            catch (Exception exception) when (exception.IsNotCritical())
                            {
                                await button.PressAsync(" ");
                            }

                            //await button.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Detached });
                        },
                        cancellationToken);
                    log($"{spoilerButtonCount} Spoiler buttons are clicked and waited.");
                    //await spoilerButtons.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Detached });
                    long spoilerDetachingTimestamp = Stopwatch.GetTimestamp();
                    while (await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Spoiler" }).CountAsync() > 0)
                    {
                        if (Stopwatch.GetElapsedTime(spoilerDetachingTimestamp) > PageHelper.DefaultManualWait)
                        {
                            throw new TimeoutException("Waiting for Spoiler buttons to be detached timed out.");
                        }

                        await Task.Delay(PageHelper.DefaultDomWait, cancellationToken);
                    }

                    log($"{spoilerButtonCount} Spoiler buttons are detached.");
                    isUpdated = true;

                    log($"{spoilerButtonCount} Spoilers are waited.");
                    long spoilerInitializingTimestamp = Stopwatch.GetTimestamp();
                    while (await page.GetByText(new Regex("^Spoilers$"), new PageGetByTextOptions() { Exact = true }).CountAsync() != spoilerButtonCount)
                    {
                        if (Stopwatch.GetElapsedTime(spoilerInitializingTimestamp) > PageHelper.DefaultManualWait)
                        {
                            throw new TimeoutException("Waiting for Spoilers to be loaded timed out.");
                        }

                        await Task.Delay(PageHelper.DefaultDomWait, cancellationToken);
                    }

                    log($"{spoilerButtonCount} Spoilers are loaded.");
                }
            },
            retryingHandler: async void (sender, arg) =>
            {
                log($"Update retry count {arg.CurrentRetryCount}.");
                log(arg.LastException.ToString());
                await page.RefreshAsync();
            },
            cancellationToken: cancellationToken);

        if (isUpdated)
        {
            await Task.Delay(PageHelper.DefaultNetworkWait, cancellationToken);
        }

        return isUpdated;
    }

    internal static async Task<(ImdbMetadata ImdbMetadata,
        string ImdbUrl, string AdvisoriesUrl, string ConnectionsUrl, string KeywordsUrl, string ReleasesUrl,
        string ImdbHtml, string AdvisoriesHtml, string ConnectionsHtml, string KeywordsHtml, string ReleasesHtml,
        string ParentImdbUrl, string ParentAdvisoriesUrl, string ParentConnectionsUrl, string ParentKeywordsUrl, string ParentReleasesUrl,
        string ParentImdbHtml, string ParentAdvisoriesHtml, string ParentConnectionsHtml, string parentKeywordsHtml, string ParentReleasesHtml)> DownloadAsync(
        string imdbId,
        string imdbFile, string advisoriesFile, string connectionsFile, string keywordsFile, string releasesFile,
        string parentImdbFile, string parentAdvisoriesFile, string parentConnectionsFile, string parentKeywordsFile, string parentReleasesFile,
        IPage? page, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        using HttpClient? httpClient = page is null ? new() : null;
        httpClient?.AddEdgeHeaders();

        string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";
        bool hasImdbFile = File.Exists(imdbFile);
        string imdbHtml = hasImdbFile
            ? await File.ReadAllTextAsync(imdbFile, cancellationToken)
            : await (page?.GetStringAsync(imdbUrl, new PageGotoOptions() { Referer = "https://www.imdb.com/" })
                ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(imdbUrl, cancellationToken), cancellationToken: cancellationToken));

        if (!hasImdbFile && page is not null && !page.Url.EqualsOrdinal(imdbUrl))
        {
            log($"Redirected {imdbId} to {page.Url}");
            imdbUrl = page.Url;
        }

        CQ imdbCQ = imdbHtml;
        string json = imdbCQ.Find("""script[type="application/ld+json"]""").Text();
        if (imdbCQ.Find("title").Text().Trim().StartsWithIgnoreCase("500 Error") || imdbCQ.Find("h1[data-testid='error-page-title']").Text().ContainsIgnoreCase("500 Error"))
        {
            throw new HttpRequestException(HttpRequestError.InvalidResponse, imdbUrl, null, HttpStatusCode.InternalServerError);
        }

        if (imdbCQ.Find("title").Text().Trim().StartsWithIgnoreCase("404 Error") || imdbCQ.Find("h1[data-testid='error-page-title']").Text().ContainsIgnoreCase("404 Error"))
        {
            throw new HttpRequestException(HttpRequestError.InvalidResponse, imdbUrl, null, HttpStatusCode.NotFound);
        }

        ImdbMetadata imdbMetadata = JsonHelper.Deserialize<ImdbMetadata>(json);

        string parentImdbUrl = string.Empty;
        string parentAdvisoriesUrl = string.Empty;
        string parentConnectionsUrl = string.Empty;
        string parentKeywordsUrl = string.Empty;
        string parentReleasesUrl = string.Empty;

        string parentImdbHtml = string.Empty;
        string parentAdvisoriesHtml = string.Empty;
        string parentConnectionsHtml = string.Empty;
        string parentKeywordsHtml = string.Empty;
        string parentReleasesHtml = string.Empty;

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
                    parentImdbUrl, parentAdvisoriesUrl, parentConnectionsUrl, parentKeywordsUrl, parentReleasesUrl,
                    parentImdbHtml, parentAdvisoriesHtml, parentConnectionsHtml, parentKeywordsHtml, parentReleasesHtml,
                    string _, string _, string _, string _, string _,
                    string _, string _, string _, string _, string _
                ) = await DownloadAsync(
                    parentImdbId,
                    parentImdbFile, parentAdvisoriesFile, parentConnectionsFile, parentKeywordsFile, parentReleasesFile,
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                    page, log, cancellationToken);
            }
        }

        string htmlTitle = imdbCQ.Find("title").Text();
        string htmlTitleYear = htmlTitle.ContainsOrdinal("(")
            ? htmlTitle[(htmlTitle.LastIndexOfOrdinal("(") + 1)..htmlTitle.LastIndexOfOrdinal(")")]
            : string.Empty;
        htmlTitleYear = YearRegex().Match(htmlTitleYear).Value;

        string htmlYear = imdbCQ.Find("h1[data-testid='hero__pageTitle']").NextAll().Find("li:first").Text().Trim();
        htmlYear = YearRegex().Match(htmlYear).Value;
        if (!YearRegex().IsMatch(htmlYear))
        {
            htmlYear = imdbCQ.Find("h1[data-testid='hero__pageTitle']").NextAll().Find("li:eq(1)").Text().Trim();
            htmlYear = YearRegex().Match(htmlYear).Value;
        }

        if (htmlYear.IsNullOrWhiteSpace())
        {
            htmlYear = imdbCQ.Find("""div.title_wrapper div.subtext a[title="See more release dates"]""").Text();
            htmlYear = YearRegex().Match(htmlYear).Value;
        }

        if (htmlYear.IsNullOrWhiteSpace())
        {
            htmlYear = imdbCQ.Find($"""ul.ipc-inline-list li a[href="/title/{imdbId}/releaseinfo?ref_=tt_ov_rdat#releases"]""").Text();
            htmlYear = YearRegex().Match(htmlYear).Value;
        }

        //if (htmlYear.IsNullOrWhiteSpace())
        //{
        //    htmlYear = imdbCQ.Find(@$"ul.ipc-inline-list li").Text();
        //    htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
        //}

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

        string[] regions = imdbCQ
                .Find("div[data-testid='title-details-section'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.ContainsIgnoreCase("Countries of origin") || listItem.TextContent.ContainsIgnoreCase("Country of origin"))
                ?.Cq().Find("ul li")
                .Select(region => region.TextContent.Trim())
                .ToArray()
            ?? imdbCQ
                .Find("""ul.ipc-metadata-list li[data-testid="title-details-origin"] ul li""")
                .Select(element => new CQ(element).Text().Trim())
                .ToArray();
        if (regions.IsEmpty())
        {
            regions = imdbCQ.Find("""a[href^="/search/title?country_of_origin="]""").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (regions.IsEmpty())
        {
            regions = imdbCQ.Find("""a[href^="/search/title/?country_of_origin="]""").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (regions.IsEmpty() && parentMetadata is not null)
        {
            regions = parentMetadata.Regions;
        }

        regions = regions
            .Select(region => region switch
            {
                "United States" => "USA",
                "United Kingdom" => "UK",
                _ => region
            })
            .ToArray();

        //Debug.Assert(regions.Any() || imdbMetadata.ImdbId is "tt0166122" or "tt6922816" or "tt12229160" or "tt6900644");

        string[] languages = imdbCQ
                .Find("div[data-testid='title-details-section'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Language"))
                ?.Cq().Find("ul li")
                .Select(region => region.TextContent.Trim())
                .ToArray()
            ?? imdbCQ
                .Find("""ul.ipc-metadata-list li[data-testid="title-details-languages"] ul li""")
                .Select(element => new CQ(element).Text().Trim())
                .ToArray();
        if (languages.IsEmpty())
        {
            languages = imdbCQ.Find("""a[href^="/search/title?title_type=feature&primary_language="]""").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (languages.IsEmpty())
        {
            languages = imdbCQ.Find("""a[href^="/search/title/?title_type=feature&primary_language="]""").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (languages.IsEmpty() && parentMetadata is not null)
        {
            languages = parentMetadata.Languages;
        }

        //Debug.Assert(languages.Any() || imdbMetadata.ImdbId
        //    is "tt0226895" or "tt0398936" or "tt6922816" or "tt3061100" or "tt3877124" or "tt0219913" or "tt0108361"
        //    or "tt0133065" or "tt0173797" or "tt2617008" or "tt1764627" or "tt0225882" or "tt10540298" or "tt0195707" or "tt3807900" or "tt2946498"
        //    or "tt9395794");

        (string Text, string Url)[] websites = imdbCQ.Find("div[data-testid='details-officialsites'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Official sites"))
                ?.Cq().Find("ul li")
                .Select(listItem => new CQ(listItem))
                .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                .ToArray()
            ?? imdbCQ
                .Find("""ul.ipc-metadata-list li[data-testid="details-officialsites"] ul li""")
                .Select(listItem => new CQ(listItem))
                .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                .ToArray();

        (string Text, string Url)[] locations = imdbCQ.Find("div[data-testid='title-details-filminglocations'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Filming locations"))
                ?.Cq().Find("ul li")
                .Select(listItem => new CQ(listItem))
                .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                .Select(data => (data.Text, $"https://www.imdb.com{data.Url[..data.Url.IndexOfIgnoreCase("&ref_=")]}"))
                .ToArray()
            ?? imdbCQ
                .Find("""ul.ipc-metadata-list li[data-testid="title-details-filminglocations"] ul li""")
                .Select(listItem => new CQ(listItem))
                .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: listItemCQ.Find("a").Attr("href")))
                .Select(data => (data.Text, $"https://www.imdb.com{data.Url[..data.Url.IndexOfIgnoreCase("&ref_=")]}"))
                .ToArray();

        (string Text, string Url)[] companies = imdbCQ.Find("div[data-testid='title-details-companies'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.Trim().StartsWithIgnoreCase("Production companies"))
                ?.Cq().Find("ul li")
                .Select(listItem => new CQ(listItem))
                .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: new UriBuilder(new Uri(new Uri("https://www.imdb.com"), listItemCQ.Find("a").Attr("href"))) { Query = string.Empty, Port = -1 }.ToString()))
                .ToArray()
            ?? imdbCQ
                .Find("""ul.ipc-metadata-list li[data-testid="title-details-companies"] ul li""")
                .Select(listItem => new CQ(listItem))
                .Select(listItemCQ => (Text: listItemCQ.Text().Trim(), Url: new UriBuilder(new Uri(new Uri("https://www.imdb.com"), listItemCQ.Find("a").Attr("href"))) { Query = string.Empty, Port = -1 }.ToString()))
                .ToArray();

        string releasesUrl = $"{imdbUrl}releaseinfo/";
        string releasesHtml = File.Exists(releasesFile)
            ? await File.ReadAllTextAsync(releasesFile, cancellationToken)
            : await (page?.GetStringAsync(releasesUrl)
                ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(releasesUrl, cancellationToken), cancellationToken: cancellationToken));

        if (page is not null && await page.UpdateAsync(log, cancellationToken))
        {
            releasesHtml = await page.ContentAsync();
            log($"{imdbId} releases is updated.");
        }
        else
        {
            log($"{imdbId} releases is not updated.");
        }

        CQ releasesCQ = releasesHtml;
        Debug.Assert(releasesCQ.Find("span.chained-see-more-button-releases button.ipc-see-more__button:contains('all')  span.ipc-btn__text:visible").IsEmpty()
            && releasesCQ.Find("span.single-page-see-more-button-releases button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").IsEmpty());
        CQ allDatesCQ = releasesCQ.Find("div[data-testid='sub-section-releases']>ul>li");
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
            allTitlesCQ = releasesCQ.Find("div[data-testid='sub-section-akas']>ul>li");
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
        string alsoKnownAs = string.Empty;
        if (page is not null)
        {
            alsoKnownAs = imdbCQ
                .Find("div[data-testid='title-details-section'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.ContainsIgnoreCase("Also known as"))
                ?.Cq().Find("ul li")
                .Select(region => region.TextContent.Trim())
                .Single() ?? string.Empty;

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
                     .Find("""h1[data-testid="hero-title-block__title"]""")
                     .Text()
                     .Trim();
            }

            originalTitle = imdbCQ.Find("h1[data-testid='hero__pageTitle']").Next("div").Text().Trim();
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
                    .Find("""div[data-testid="hero-title-block__original-title"]""")
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
                            .Find("""h1[data-testid="hero-title-block__title"]""")
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
        string advisoriesHtml = File.Exists(advisoriesFile)
            ? await File.ReadAllTextAsync(advisoriesFile, cancellationToken)
            : await (page?.GetStringAsync(advisoriesUrl)
                ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(advisoriesUrl, cancellationToken), cancellationToken: cancellationToken));
        CQ advisoriesCQ = advisoriesHtml;
        string mpaaRating = advisoriesCQ.Find("section[data-testid='content-rating'] ul li div.ipc-metadata-list-item__content-container").First().Text().Trim();
        CQ spoilersButtonsCQ = advisoriesCQ.Find("section span.ipc-see-more button:contains('Spoilers')");

        if (page is not null && await page.UpdateAsync(log, cancellationToken))
        {
            advisoriesHtml = await page.ContentAsync();
            advisoriesCQ = advisoriesHtml;
            log($"{imdbId} advisories is updated.");
        }
        else
        {
            log($"{imdbId} advisories is not updated.");
        }

        Debug.Assert(advisoriesCQ.Find("section span.ipc-see-more button:contains('Spoilers')").IsEmpty()
            && advisoriesCQ.Find("section > div.ipc-signpost > div.ipc-signpost__text:contains('Spoilers')").Length == spoilersButtonsCQ.Length);

        ImdbAdvisory[] advisories = advisoriesCQ
            .Find("div.ipc-page-grid__item > section[data-testid='content-rating']")
            .Siblings()
            .TakeWhile(sectionDom => sectionDom.GetAttribute("data-testid").IsNullOrEmpty())
            .Select(sectionDom =>
            {
                CQ sectionCQ = sectionDom.Cq();
                string category = sectionCQ.Find("h3").Text().Trim();
                string severity = sectionCQ.Find("div[data-testid='severity_component']").Children().First().Text().Trim();
                string[] details = sectionCQ.Find("div[data-testid^='sub-section-'] div[data-testid='item-html']").Select(item => item.TextContent.Trim()).ToArray();
                return new ImdbAdvisory(category, severity, details);
            })
            .Where(advisory => advisory.Severity.IsNotNullOrWhiteSpace() || advisory.Details.Any())
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

        string connectionsUrl = $"{imdbUrl}movieconnections/";
        string connectionsHtml = File.Exists(connectionsUrl)
            ? await File.ReadAllTextAsync(connectionsFile, cancellationToken)
            : await (page?.GetStringAsync(connectionsUrl) ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(connectionsUrl, cancellationToken), cancellationToken: cancellationToken));

        if (page is not null && await page.UpdateAsync(log, cancellationToken))
        {
            connectionsHtml = await page.ContentAsync();
            log($"{imdbId} connections is updated.");
        }
        else
        {
            log($"{imdbId} connections is not updated.");
        }

        CQ connectionsCQ = connectionsHtml;
        Dictionary<string, ImdbConnection[]> connections = connectionsCQ
            .Find("div.ipc-page-grid__item section.ipc-page-section")
            .Select(sectionDom => sectionDom.Cq())
            .Select(
                sectionCQ => (Key: sectionCQ.Find("div.ipc-title h3.ipc-title__text").Text().Trim(),
                Value: sectionCQ
                    .Find("ul.ipc-metadata-list > li")
                    .Select(listItemDom =>
                    {
                        CQ listItemDivCQ = listItemDom.Cq().Find("ul > div");
                        return new ImdbConnection(
                            listItemDivCQ.Eq(0).Text().Trim(),
                            listItemDivCQ.Eq(0).Find("a").Attr("href"),
                            listItemDivCQ.Eq(1).Text().Trim());
                    })
                    .ToArray()))
            .Where(pair => pair.Key.IsNotNullOrWhiteSpace())
            .TakeWhile(pair => !pair.Key.ContainsIgnoreCase("Contribute to this page"))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        string keywordsUrl = $"{imdbUrl}keywords/";
        string keywordsHtml = File.Exists(keywordsFile)
            ? await File.ReadAllTextAsync(keywordsFile, cancellationToken)
            : await (page?.GetStringAsync(keywordsUrl) ?? Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(keywordsUrl, cancellationToken), cancellationToken: cancellationToken));
        if (page is not null && await page.UpdateAsync(log, cancellationToken))
        {
            keywordsHtml = await page.ContentAsync();
            log($"{imdbId} keywords is updated.");
        }
        else
        {
            log($"{imdbId} keywords is not updated.");
        }

        CQ keywordsCQ = keywordsHtml;
        string[] allKeywords = keywordsCQ.Find("#keywords_content table td div.sodatext a").Select(keyword => keyword.TextContent.Trim()).ToArray();
        if (allKeywords.IsEmpty())
        {
            keywordsCQ = keywordsHtml;
            CQ allKeywordsCQ = keywordsCQ.Find("div[data-testid='sub-section']>ul>li");
            allKeywords = allKeywordsCQ
                .Select(row => row.Cq())
                .Select(rowCQ => HttpUtility.HtmlDecode(rowCQ.Children().Eq(0).Text()))
                .ToArray();
        }

        imdbMetadata = imdbMetadata with
        {
            Link = imdbUrl,
            Parent = parentMetadata,
            Year = year,
            Regions = regions,
            Languages = languages,
            Titles = titles,
            Title = HttpUtility.HtmlDecode(title).Trim(),
            OriginalTitle = HttpUtility.HtmlDecode(originalTitle).Trim(),
            Name = HttpUtility.HtmlDecode(imdbMetadata.Name).Trim(),
            AllKeywords = allKeywords,
            MpaaRating = mpaaRating,
            Advisories = advisories.ToLookup(advisory => advisory.Category).ToDictionary(group => group.Key, group => group.ToArray()),
            AlsoKnownAs = alsoKnownAs,
            Genres = imdbMetadata.Genres,
            Releases = dates,
            Websites = websites
                .Distinct()
                .ToLookup(item => item.Text, item => item.Url)
                .ToDictionary(group => HttpUtility.HtmlDecode(group.Key), group => group.ToArray()),
            FilmingLocations = locations
                .Distinct()
                .ToDictionary(item => item.Text, item => item.Url),
            Companies = companies
                .Distinct()
                .ToLookup(item => item.Text, item => item.Url)
                .ToDictionary(group => HttpUtility.HtmlDecode(group.Key), group => group.ToArray()),
            Certifications = certifications,
            Connections = connections
        };

        return (
            imdbMetadata,

            imdbUrl, advisoriesUrl, connectionsUrl, keywordsUrl, releasesUrl,
            imdbHtml, advisoriesHtml, connectionsHtml, keywordsHtml, releasesHtml,

            parentImdbUrl, parentAdvisoriesUrl, parentConnectionsUrl, parentKeywordsUrl, parentReleasesUrl,
            parentImdbHtml, parentAdvisoriesHtml, parentConnectionsHtml, parentKeywordsHtml, parentReleasesHtml
        );
    }

    internal static async Task DownloadAllMoviesAsync(ISettings settings, Func<int, Range>? getRange = null, int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        maxDegreeOfParallelism ??= MaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        WebDriverHelper.DisposeAll();

        ILookup<string, string> top = (await File.ReadAllLinesAsync(settings.TopDatabase, cancellationToken))
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
                    using IPlaywright playwright = await Playwright.CreateAsync();
                    await using IBrowser browser = await playwright.Chromium.LaunchAsync();
                    IPage page = await browser.NewPageAsync();
                    IResponse? response = await page.GotoAsync("https://www.imdb.com/");
                    Debug.Assert(response is not null && response.Ok);
                    while (imdbIdQueue.TryDequeue(out string? imdbId))
                    {
                        int index = trimmedLength - imdbIdQueue.Count;
                        log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                        try
                        {
                            await Retry.FixedIntervalAsync(
                                async () => await Video.DownloadImdbMetadataAsync(imdbId, settings.MovieMetadataDirectory, settings.MovieMetadataCacheDirectory, metadataFiles, cacheFiles, page, overwrite: false, useCache: true, log: log, token),
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
                    using WebDriverWrapper webDriver = new(() => WebDriverHelper.Start(webDriverIndex, keepExisting: true), "http://imdb.com");
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
                            string advisoriesUrl = $"{imdbUrl}parentalguide";
                            string advisoriesHtml = await webDriver.GetStringAsync(advisoriesUrl, cancellationToken: token);
                            if (true)
                            {
                                CQ parentalGuideCQ = advisoriesHtml;
                                string mpaaRating = parentalGuideCQ.Find("#mpaa-rating td").Last().Text().Trim();
                                ImdbAdvisory[] advisories = parentalGuideCQ
                                    .Find("#main section.content-advisories-index section")
                                    .Where(section => section.Id.StartsWithIgnoreCase("advisory-"))
                                    .Select(section =>
                                    {
                                        CQ sectionCQ = section.Cq();
                                        string category = sectionCQ.Find("h4").Text().Trim();
                                        string severity = sectionCQ.Find("li.advisory-severity-vote span.ipl-status-pill").Text().Trim();
                                        string[] details = sectionCQ.Find("li.ipl-zebra-list__item").Select(li => li.ChildNodes.First().NodeValue.Trim()).ToArray();
                                        return new ImdbAdvisory(category, severity, details);
                                    })
                                    .Where(advisory => advisory.Severity.IsNotNullOrWhiteSpace() || advisory.Details.Any())
                                    .ToArray();
                                Dictionary<string, ImdbAdvisory[]> result = new()
                                {
                                    [mpaaRating] = advisories
                                };
                                Dictionary<string, string> certifications = parentalGuideCQ
                                    .Find("#certificates li.ipl-inline-list__item")
                                    .Select(certificationDom => (
                                        Certification: Regex.Replace(certificationDom.TextContent.Trim(), @"\s+", " "),
                                        Link: certificationDom.Cq().Find("a").Attr("href").Trim()))
                                    .DistinctBy(certification => certification.Certification)
                                    .Do(certification => log(certification.Certification))
                                    .ToDictionary(certification => certification.Certification, certification => certification.Link);
                                await FileHelper.WriteTextAsync(advisoriesFile, advisoriesHtml, cancellationToken: token);
                                await JsonHelper.SerializeToFileAsync(result, $"{advisoriesFile}.txt", token);
                                await JsonHelper.SerializeToFileAsync(certifications, $"{advisoriesFile}.Certifications.txt", token);
                            }
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

        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Chromium.LaunchAsync();
        IPage page = await browser.NewPageAsync();
        IResponse? response = await page.GotoAsync("https://www.imdb.com/");
        Debug.Assert(response is not null && response.Ok);
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
                        async () => await Video.DownloadImdbMetadataAsync(imdbId, metadataDirectory, cacheDirectory, metadataFiles, cacheFiles, page, overwrite: false, useCache: true, log: log, cancellationToken: cancellationToken),
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

        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Chromium.LaunchAsync();
        IPage page = await browser.NewPageAsync();
        IResponse? response = await page.GotoAsync("https://www.imdb.com/");
        Debug.Assert(response is not null && response.Ok);

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
                        async () => await Video.DownloadImdbMetadataAsync(imdbId, metadataDirectory, cacheDirectory, metadataFiles, cacheFiles, page, overwrite: false, useCache: true, log: log, cancellationToken: cancellationToken),
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
}