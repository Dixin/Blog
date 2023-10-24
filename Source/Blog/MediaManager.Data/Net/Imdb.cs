namespace Examples.Net;

using System.Web;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

internal static class Imdb
{
    internal static async Task<(
        ImdbMetadata ImdbMetadata,
        string ImdbUrl, string ImdbHtml,
        string ReleaseUrl, string ReleaseHtml,
        string KeywordsUrl, string KeywordsHtml,
        string AdvisoriesUrl, string AdvisoriesHtml,
        string ParentImdbUrl, string ParentImdbHtml,
        string ParentReleaseUrl, string ParentReleaseHtml,
        string ParentKeywordsUrl, string parentKeywordsHtml,
        string ParentAdvisoriesUrl, string ParentAdvisoriesHtml)> DownloadAsync(
        string imdbId,
        string imdbFile, string releaseFile, string keywordsFile, string advisoriesFile,
        string parentImdbFile, string parentReleaseFile, string parentKeywordsFile, string parentAdvisoriesFile,
        IWebDriver? webDriver = null)
    {
        using HttpClient? httpClient = webDriver is null ? new() : null;
        httpClient?.AddEdgeHeaders();

        string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";
        string imdbHtml = File.Exists(imdbFile)
            ? await File.ReadAllTextAsync(imdbFile)
            : webDriver is not null
                ? WebDriverHelper.GetString(ref webDriver, imdbUrl)
                : await Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(imdbUrl));
        CQ imdbCQ = imdbHtml;
        string json = imdbCQ.Find(@"script[type=""application/ld+json""]").Text();
        if (imdbCQ.Find("title").Text().Trim().StartsWithIgnoreCase("500 Error"))
        {
            throw new ArgumentException("Server side error.", nameof(imdbId));
        }

        if (imdbCQ.Find("title").Text().Trim().StartsWithIgnoreCase("404 Error"))
        {
            throw new ArgumentOutOfRangeException(nameof(imdbId));
        }

        ImdbMetadata imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
            json,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException(json);

        string parentImdbUrl = string.Empty;
        string parentImdbHtml = string.Empty;
        string parentReleaseUrl = string.Empty;
        string parentReleaseHtml = string.Empty;
        string parentKeywordsUrl = string.Empty;
        string parentKeywordsHtml = string.Empty;
        string parentAdvisoriesUrl = string.Empty;
        string parentAdvisoriesHtml = string.Empty;
        string parentHref = imdbCQ.Find(@"div.titleParent a").FirstOrDefault()?.GetAttribute("href")
            ?? imdbCQ.Find("div").FirstOrDefault(div => div.Classes.Any(@class => @class.StartsWithOrdinal("TitleBlock__SeriesParentLinkWrapper")))?.Cq().Find("a").Attr("href")
            ?? imdbCQ.Find("section.ipc-page-section > div:first > a:first").FirstOrDefault()?.GetAttribute("href")
            ?? string.Empty;
        ImdbMetadata? parentMetadata = null;
        if (parentHref.IsNotNullOrWhiteSpace())
        {
            string parentImdbId = Regex.Match(parentHref, "tt[0-9]+").Value;
            if (!parentImdbId.EqualsIgnoreCase(imdbId))
            {
                (
                    parentMetadata,
                    parentImdbUrl, parentImdbHtml,
                    parentReleaseUrl, parentReleaseHtml,
                    parentKeywordsUrl, parentKeywordsHtml,
                    parentAdvisoriesUrl, parentAdvisoriesHtml,
                    string _, string _, string _, string _, string _, string _, string _, string _
                ) = await DownloadAsync(
                    parentImdbId,
                    parentImdbFile, parentReleaseFile, parentKeywordsFile, parentAdvisoriesFile,
                    string.Empty, string.Empty, string.Empty, string.Empty,
                    webDriver);
            }
        }

        string htmlTitle = imdbCQ.Find(@"title").Text();
        string htmlTitleYear = htmlTitle.ContainsOrdinal("(")
            ? htmlTitle[(htmlTitle.LastIndexOfOrdinal("(") + 1)..htmlTitle.LastIndexOfOrdinal(")")]
            : string.Empty;
        htmlTitleYear = Regex.Match(htmlTitleYear, "[0-9]{4}").Value;

        string htmlYear = imdbCQ.Find(@"h1[data-testid='hero__pageTitle']").NextAll().Find("li:first").Text().Trim();
        htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
        if (!Regex.IsMatch(htmlYear, "[0-9]{4}"))
        {
            htmlYear = imdbCQ.Find(@"h1[data-testid='hero__pageTitle']").NextAll().Find("li:eq(1)").Text().Trim();
            htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
        }

        if (htmlYear.IsNullOrWhiteSpace())
        {
            htmlYear = imdbCQ.Find(@"div.title_wrapper div.subtext a[title=""See more release dates""]").Text();
            htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
        }

        if (htmlYear.IsNullOrWhiteSpace())
        {
            htmlYear = imdbCQ.Find(@$"ul.ipc-inline-list li a[href=""/title/{imdbId}/releaseinfo?ref_=tt_ov_rdat#releases""]").Text();
            htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
        }

        //if (htmlYear.IsNullOrWhiteSpace())
        //{
        //    htmlYear = imdbCQ.Find(@$"ul.ipc-inline-list li").Text();
        //    htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
        //}

        Debug.Assert(htmlYear.EqualsOrdinal(htmlTitleYear) || parentMetadata is not null);

        string year = imdbMetadata.Year ?? string.Empty;
        if (year.IsNullOrWhiteSpace())
        {
            year = htmlYear.IsNotNullOrWhiteSpace() ? htmlYear : imdbMetadata.YearOfLatestRelease;
        }
        else
        {
            Debug.Assert(year.EqualsOrdinal(htmlYear));
        }

        Debug.Assert(year.IsNullOrWhiteSpace() || Regex.IsMatch(htmlYear, "[0-9]{4}"));

        string[] regions = imdbCQ
                .Find("div[data-testid='title-details-section'] > ul > li")
                .FirstOrDefault(listItem => listItem.TextContent.ContainsIgnoreCase("Countries of origin") || listItem.TextContent.ContainsIgnoreCase("Country of origin"))
                ?.Cq().Find("ul li")
                .Select(region => region.TextContent.Trim())
                .ToArray()
            ?? imdbCQ
                .Find(@"ul.ipc-metadata-list li[data-testid=""title-details-origin""] ul li")
                .Select(element => new CQ(element).Text().Trim())
                .ToArray();
        if (regions.IsEmpty())
        {
            regions = imdbCQ.Find(@"a[href^=""/search/title?country_of_origin=""]").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (regions.IsEmpty())
        {
            regions = imdbCQ.Find(@"a[href^=""/search/title/?country_of_origin=""]").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
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
                .FirstOrDefault(listItem => listItem.TextContent.ContainsIgnoreCase("Language"))
                ?.Cq().Find("ul li")
                .Select(region => region.TextContent.Trim())
                .ToArray()
            ?? imdbCQ
                .Find(@"ul.ipc-metadata-list li[data-testid=""title-details-languages""] ul li")
                .Select(element => new CQ(element).Text().Trim())
                .ToArray();
        if (languages.IsEmpty())
        {
            languages = imdbCQ.Find(@"a[href^=""/search/title?title_type=feature&primary_language=""]").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (languages.IsEmpty())
        {
            languages = imdbCQ.Find(@"a[href^=""/search/title/?title_type=feature&primary_language=""]").Select(link => link.TextContent.Trim()).DistinctOrdinal().ToArray();
        }

        if (languages.IsEmpty() && parentMetadata is not null)
        {
            languages = parentMetadata.Languages;
        }

        //Debug.Assert(languages.Any() || imdbMetadata.ImdbId
        //    is "tt0226895" or "tt0398936" or "tt6922816" or "tt3061100" or "tt3877124" or "tt0219913" or "tt0108361"
        //    or "tt0133065" or "tt0173797" or "tt2617008" or "tt1764627" or "tt0225882" or "tt10540298" or "tt0195707" or "tt3807900" or "tt2946498"
        //    or "tt9395794");

        string releaseUrl = $"{imdbUrl}releaseinfo/";
        string releaseHtml = File.Exists(releaseFile)
            ? await File.ReadAllTextAsync(releaseFile)
            : webDriver is not null
                ? WebDriverHelper.GetString(ref webDriver, releaseUrl)
                : await Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(releaseUrl));
        CQ releaseCQ = releaseHtml;

        if (webDriver is not null)
        {
            CQ seeAllDatesButtonCQ = releaseCQ.Find("span.chained-see-more-button-releases button.ipc-see-more__button:contains('All')  span.ipc-btn__text:visible");
            Debug.Assert(seeAllDatesButtonCQ.Length is 0 or 1);
            if (seeAllDatesButtonCQ.Any() && webDriver.Url.EqualsIgnoreCase(releaseUrl))
            {
                Retry.FixedInterval(
                    action: () =>
                    {
                        int rowCount = webDriver.FindElements(By.CssSelector("div[data-testid='sub-section-releases']>ul>li")).Count;
                        IWebElement seeAllDatesButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("span.chained-see-more-button-releases button.ipc-see-more__button")));
                        if (!seeAllDatesButton.Displayed)
                        {
                            seeAllDatesButtonCQ = releaseCQ.Find("span.single-page-see-more-button-releases button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible");
                            seeAllDatesButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("span.single-page-see-more-button-releases button.ipc-see-more__button")));
                        }

                        Debug.Assert(seeAllDatesButtonCQ.Length == 1);
                        Debug.Assert(seeAllDatesButton.Displayed);
                        try
                        {
                            seeAllDatesButton.Click();
                        }
                        catch (Exception exception) when (exception.IsNotCritical())
                        {
                            seeAllDatesButton.SendKeys(Keys.Space);
                        }

                        new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver =>
                        {
                            releaseHtml = webDriver.PageSource;
                            releaseCQ = releaseHtml;
                            rowCount = releaseCQ.Find("div[data-testid='sub-section-releases']>ul>li").Length;
                            Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                            return driver.FindElements(By.CssSelector("div[data-testid='sub-section-releases']>ul>li")).Count == rowCount
                                && releaseCQ.Find("span.chained-see-more-button-releases button.ipc-see-more__button:contains('All')  span.ipc-btn__text:visible").IsEmpty()
                                && releaseCQ.Find("span.single-page-see-more-button-releases button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").IsEmpty();
                        });
                    },
                    retryingHandler: (sender, arg) =>
                    {
                        try
                        {
                            webDriver.Dispose();
                        }
                        finally
                        {
                            webDriver = WebDriverHelper.Start();
                        }

                        releaseHtml = webDriver.GetString(releaseUrl);
                        releaseCQ = releaseHtml;
                        seeAllDatesButtonCQ = releaseCQ.Find("span.chained-see-more-button-releases button.ipc-see-more__button:contains('All')  span.ipc-btn__text:visible");
                        Debug.Assert(seeAllDatesButtonCQ.Any());
                    });
            }

            releaseHtml = webDriver.PageSource;
        }

        releaseCQ = releaseHtml;
        CQ allDatesCQ = releaseCQ.Find("div[data-testid='sub-section-releases']>ul>li");
        (string DateKey, string DateValue)[] allDates = allDatesCQ
            .Select(row => row.Cq())
            .Select(rowCQ => (Key: rowCQ.Children().Eq(0), Values: rowCQ.Children().Eq(1).Find("li")))
            .SelectMany(row => row.Values.Select(value => (Key: HttpUtility.HtmlDecode(row.Key.Text().Trim()), Value: HttpUtility.HtmlDecode(value.TextContent.Trim()))))
            .ToArray();
        Dictionary<string, string[]> dates = allDates
            .ToLookup(row => row.DateKey, row => row.DateValue, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray());

        CQ allTitlesCQ = releaseCQ.Find("#akas").Next();
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
            CQ seeAllAkaButtonCQ = releaseCQ.Find("span.chained-see-more-button-akas button.ipc-see-more__button:contains('All') span.ipc-btn__text:visible");
            Debug.Assert(seeAllAkaButtonCQ.Length is 0 or 1);
            if (webDriver is not null)
            {
                if (seeAllAkaButtonCQ.Any() && webDriver.Url.EqualsIgnoreCase(releaseUrl))
                {
                    Retry.FixedInterval(
                        action: () =>
                        {
                            int rowCount = webDriver.FindElements(By.CssSelector("div[data-testid='sub-section-akas']>ul>li")).Count;
                            IWebElement seeAllAkaButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("span.chained-see-more-button-akas button.ipc-see-more__button")));
                            if (!seeAllAkaButton.Displayed)
                            {
                                seeAllAkaButtonCQ = releaseCQ.Find("span.single-page-see-more-button-akas button.ipc-see-more__button:contains('more')  span.ipc-btn__text:visible");
                                seeAllAkaButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.single-page-see-more-button-akas button.ipc-see-more__button")).Last());
                            }

                            Debug.Assert(seeAllAkaButtonCQ.Length == 1);
                            Debug.Assert(seeAllAkaButton.Displayed);
                            try
                            {
                                seeAllAkaButton.Click();
                            }
                            catch (Exception exception) when (exception.IsNotCritical())
                            {
                                seeAllAkaButton.SendKeys(Keys.Space);
                            }

                            new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver =>
                            {
                                releaseHtml = webDriver.PageSource;
                                releaseCQ = releaseHtml;
                                rowCount = releaseCQ.Find("div[data-testid='sub-section-akas']>ul>li").Length;
                                Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                                return driver.FindElements(By.CssSelector("div[data-testid='sub-section-akas']>ul>li")).Count == rowCount
                                    && releaseCQ.Find("span.chained-see-more-button-akas button.ipc-see-more__button:contains('All') span.ipc-btn__text:visible").IsEmpty()
                                    && releaseCQ.Find("span.single-page-see-more-button-akas button.ipc-see-more__button:contains('more')  span.ipc-btn__text:visible").IsEmpty();
                            });
                        },
                        retryingHandler: (_, _) =>
                        {
                            try
                            {
                                webDriver.Dispose();
                            }
                            finally
                            {
                                webDriver = WebDriverHelper.Start();
                            }

                            releaseHtml = webDriver.GetString(releaseUrl);
                            releaseCQ = releaseHtml;
                            CQ seeAllDatesButtonCQ = releaseCQ.Find("span.chained-see-more-button-releases button.ipc-see-more__button:contains('All')  span.ipc-btn__text:visible");
                            Debug.Assert(seeAllDatesButtonCQ.Length is 0 or 1);
                            if (seeAllDatesButtonCQ.Any() && webDriver.Url.EqualsIgnoreCase(releaseUrl))
                            {
                                int rowCount = webDriver.FindElements(By.CssSelector("div[data-testid='sub-section-releases']>ul>li")).Count;
                                IWebElement seeAllDatesButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("span.chained-see-more-button-releases button.ipc-see-more__button")));
                                if (!seeAllDatesButton.Displayed)
                                {
                                    seeAllDatesButtonCQ = releaseCQ.Find("span.single-page-see-more-button-releases button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible");
                                    seeAllDatesButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("span.single-page-see-more-button-releases button.ipc-see-more__button")));
                                }

                                Debug.Assert(seeAllDatesButtonCQ.Length == 1);
                                Debug.Assert(seeAllDatesButton.Displayed);
                                try
                                {
                                    seeAllDatesButton.Click();
                                }
                                catch (Exception exception) when (exception.IsNotCritical())
                                {
                                    seeAllDatesButton.SendKeys(Keys.Space);
                                }

                                new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver =>
                                {
                                    releaseHtml = webDriver.PageSource;
                                    releaseCQ = releaseHtml;
                                    rowCount = releaseCQ.Find("div[data-testid='sub-section-releases']>ul>li").Length;
                                    Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                                    return driver.FindElements(By.CssSelector("div[data-testid='sub-section-releases']>ul>li")).Count == rowCount
                                        && releaseCQ.Find("span.chained-see-more-button-releases button.ipc-see-more__button:contains('All')  span.ipc-btn__text:visible").IsEmpty()
                                        && releaseCQ.Find("span.single-page-see-more-button-releases button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").IsEmpty();
                                });
                            }

                            releaseHtml = webDriver.PageSource;
                        });
                }

                releaseHtml = webDriver.PageSource;
            }

            releaseCQ = releaseHtml;
            allTitlesCQ = releaseCQ.Find("div[data-testid='sub-section-akas']>ul>li");
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
        if (webDriver is not null)
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
                     .Find(@"h1[data-testid=""hero-title-block__title""]")
                     .Text()
                     .Trim();
            }

            originalTitle = imdbCQ.Find(@"h1[data-testid='hero__pageTitle']").Next("div").Text().Trim();
            if (originalTitle.StartsWithIgnoreCase("Original title: "))
            {
                originalTitle = originalTitle.Substring("Original title: ".Length).Trim();
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
                    .Find(@"div[data-testid=""hero-title-block__original-title""]")
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
                    string releaseHtmlTitle = releaseCQ.Find("title").Text();
                    releaseHtmlTitle = releaseHtmlTitle.Substring(0, releaseHtmlTitle.LastIndexOfOrdinal("(")).Trim();

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
                            ?? string.Join(TitleSeparator, originalTitleValues),
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
                        titleValues = Array.Empty<string>();
                    }

                    Debug.Assert(titleValues.Any());
                    title = titleValues.Length switch
                    {
                        1 => titleValues.Single(),
                        > 1 => titleValues.FirstOrDefault(availableTitle => releaseHtmlTitle.EqualsOrdinal(availableTitle))
                            ?? titleValues.FirstOrDefault(availableTitle => releaseHtmlTitle.EqualsIgnoreCase(availableTitle))
                            ?? titleValues.FirstOrDefault(titleValue => titleValues.Except(EnumerableEx.Return(titleValue)).All(titleValue.ContainsIgnoreCase))
                            ?? string.Join(TitleSeparator, titleValues),
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
                            .Find(@"h1[data-testid=""hero-title-block__title""]")
                            .Find("#titleYear")
                            .Remove()
                            .End()
                            .Text()
                            .Trim();
                    }

                    htmlTitle = htmlTitle.ContainsOrdinal("(")
                        ? htmlTitle.Substring(0, htmlTitle.LastIndexOfOrdinal("(")).Trim()
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

        string keywordsUrl = $"{imdbUrl}keywords/";
        string keywordsHtml = File.Exists(keywordsFile)
            ? await File.ReadAllTextAsync(keywordsFile)
            : webDriver is not null
                ? WebDriverHelper.GetString(ref webDriver, keywordsUrl)
                : await Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(keywordsUrl));
        CQ keywordsCQ = keywordsHtml;
        string[] allKeywords = keywordsCQ.Find("#keywords_content table td div.sodatext a").Select(keyword => keyword.TextContent.Trim()).ToArray();
        if (allKeywords.IsEmpty())
        {
            CQ seeAllButtonCQ = keywordsCQ.Find("span.chained-see-more-button button.ipc-see-more__button:contains('All') span.ipc-btn__text:visible");
            Debug.Assert(seeAllButtonCQ.Length is 0 or 1);
            if (webDriver is not null)
            {
                if (seeAllButtonCQ.Any() && webDriver.Url.EqualsIgnoreCase(keywordsUrl))
                {
                    int rowCount = webDriver.FindElements(By.CssSelector("div[data-testid='sub-section']>ul>li")).Count;
                    IWebElement seeAllButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.chained-see-more-button button.ipc-see-more__button")).Last());
                    if (!seeAllButton.Displayed)
                    {
                        seeAllButtonCQ = keywordsCQ.Find("span.single-page-see-more-button button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible");
                        seeAllButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.single-page-see-more-button button.ipc-see-more__button")).Last());
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

                    new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver =>
                    {
                        keywordsHtml = webDriver.PageSource;
                        keywordsCQ = keywordsHtml;
                        rowCount = keywordsCQ.Find("div[data-testid='sub-section']>ul>li").Length;
                        Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                        return driver.FindElements(By.CssSelector("div[data-testid='sub-section']>ul>li")).Count == rowCount
                            && keywordsCQ.Find("span.chained-see-more-button button.ipc-see-more__button:contains('All') span.ipc-btn__text:visible").IsEmpty()
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
        }

        string advisoriesUrl = $"{imdbUrl}parentalguide";
        string advisoriesHtml = File.Exists(advisoriesFile)
            ? await File.ReadAllTextAsync(advisoriesFile)
            : webDriver is not null
                ? WebDriverHelper.GetString(ref webDriver, advisoriesUrl)
                : await Retry.FixedIntervalAsync(async () => await httpClient!.GetStringAsync(advisoriesUrl));
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

        imdbMetadata = imdbMetadata with
        {
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
            Genres = imdbMetadata.Genres ?? Array.Empty<string>()
        };

        return (
            imdbMetadata,

            imdbUrl, imdbHtml,
            releaseUrl, releaseHtml,
            keywordsUrl, keywordsHtml,
            advisoriesUrl, advisoriesHtml,

            parentImdbUrl, parentImdbHtml,
            parentReleaseUrl, parentReleaseHtml,
            parentKeywordsUrl, parentKeywordsHtml,
            parentAdvisoriesUrl, parentAdvisoriesHtml
        );
    }

    internal const string TitleSeparator = "~";

    internal static bool TryRead(string path, [NotNullWhen(true)] out string? imdbId, [NotNullWhen(true)] out string? year, [NotNullWhen(true)] out string[]? regions, [NotNullWhen(true)] out string[]? languages, [NotNullWhen(true)] out string[]? genres)
    {
        imdbId = null;
        year = null;
        regions = null;
        languages = null;
        genres = null;

        if (!TryRead(path, out string? file))
        {
            return false;
        }

        string name = Path.GetFileNameWithoutExtension(file);
        if (name.EqualsOrdinal(Video.NotExistingFlag))
        {
            return false;
        }

        string[] info = name.Split(Video.SubtitleSeparator);
        Debug.Assert(info.Length == 5 && info[0].IsNotNullOrWhiteSpace());
        imdbId = info[0];
        year = info[1];
        regions = info[2].Split(Video.ImdbMetadataSeparator);
        languages = info[3].Split(Video.ImdbMetadataSeparator);
        genres = info[4].Split(Video.ImdbMetadataSeparator);
        return true;
    }

    internal static bool TryRead(string? path, [NotNullWhen(true)] out string? file)
    {
        if (Directory.Exists(path))
        {
            file = Directory.GetFiles(path, Video.ImdbMetadataSearchPattern, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Debug.Assert(file.IsNullOrWhiteSpace() || Path.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.SubtitleSeparator) || Path.GetFileNameWithoutExtension(file).Split(Video.SubtitleSeparator).Length == 5);
            return file.IsNotNullOrWhiteSpace();
        }

        if (path.IsNotNullOrWhiteSpace() && path.EndsWith(Video.ImdbMetadataExtension) && File.Exists(path))
        {
            file = path;
            Debug.Assert(file.IsNullOrWhiteSpace() || Path.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.SubtitleSeparator) || Path.GetFileNameWithoutExtension(file).Split(Video.SubtitleSeparator).Length == 5);
            return true;
        }

        file = null;
        return false;
    }

    internal static bool TryLoad(string? path, [NotNullWhen(true)] out ImdbMetadata? imdbMetadata)
    {
        if (TryRead(path, out string? file) && !Path.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag))
        {
            imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                File.ReadAllText(file),
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true }) ?? throw new InvalidOperationException(file);
            return true;
        }

        imdbMetadata = null;
        return false;
    }

    internal static async Task DownloadAllMoviesAsync(
        string libraryJsonPath,
        string x265JsonPath, string h264JsonPath, string preferredJsonPath, string h264720PJsonPath, string rareJsonPath, string x265XJsonPath, string h264XJsonPath,
        string cacheDirectory, string metadataDirectory,
        Func<int, Range>? getRange = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;
        Dictionary<string, TopMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        Dictionary<string, TopMetadata[]> x265XMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(x265XJsonPath))!;
        Dictionary<string, TopMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
        Dictionary<string, TopMetadata[]> h264XMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264XJsonPath))!;
        Dictionary<string, PreferredMetadata[]> preferredMetadata = JsonSerializer.Deserialize<Dictionary<string, PreferredMetadata[]>>(await File.ReadAllTextAsync(preferredJsonPath))!;
        Dictionary<string, TopMetadata[]> h264720PMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264720PJsonPath))!;
        Dictionary<string, RareMetadata> rareMetadata = JsonSerializer.Deserialize<Dictionary<string, RareMetadata>>(await File.ReadAllTextAsync(rareJsonPath))!;

        using IWebDriver webDriver = WebDriverHelper.Start();
        string[] cacheFiles = Directory.GetFiles(@cacheDirectory);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory);
        string[] imdbIds = x265Metadata.Keys
            .Concat(x265XMetadata.Keys)
            .Concat(h264Metadata.Keys)
            .Concat(h264XMetadata.Keys)
            .Concat(preferredMetadata.Keys)
            .Concat(h264720PMetadata.Keys)
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
            .Except(metadataFiles.Select(file => Path.GetFileNameWithoutExtension(file).Split("-").First()))
            .ToArray();
        int trimmedLength = imdbIds.Length;
        await imdbIds.ForEachAsync(async (imdbId, index) =>
            {
                log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                try
                {
                    await Retry.FixedIntervalAsync(async () => await Video.DownloadImdbMetadataAsync(imdbId, cacheDirectory, metadataDirectory, cacheFiles, metadataFiles, webDriver, false, true, log));
                }
                catch (ArgumentOutOfRangeException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbId}");
                }
                catch (ArgumentException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbId}");
                }
            });
    }

    internal static async Task UpdateAllMoviesAsync(
        string libraryJsonPath,
        string x265JsonPath, string h264JsonPath, string preferredJsonPath, string h264720PJsonPath, string rareJsonPath, string x265XJsonPath, string h264XJsonPath,
        string cacheDirectory, string metadataDirectory,
        Func<int, Range>? getRange = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        //Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;
        //Dictionary<string, TopMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        //Dictionary<string, TopMetadata[]> x265XMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(x265XJsonPath))!;
        //Dictionary<string, TopMetadata[]> h264Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264JsonPath))!;
        //Dictionary<string, TopMetadata[]> h264XMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264XJsonPath))!;
        //Dictionary<string, PreferredMetadata[]> preferredMetadata = JsonSerializer.Deserialize<Dictionary<string, PreferredMetadata[]>>(await File.ReadAllTextAsync(preferredJsonPath))!;
        //Dictionary<string, TopMetadata[]> h264720PMetadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(h264720PJsonPath))!;
        //Dictionary<string, RareMetadata> rareMetadata = JsonSerializer.Deserialize<Dictionary<string, RareMetadata>>(await File.ReadAllTextAsync(rareJsonPath))!;
        string[] cacheFiles = Directory.EnumerateFiles(cacheDirectory, "*.Keywords.bak.log").Order().ToArray();

        int length = cacheFiles.Length;
        if (getRange is not null)
        {
            cacheFiles = cacheFiles.Take(getRange(length)).ToArray();
        }

        int degree = 2;
        IWebDriver[] webDrivers = Enumerable.Range(0, degree).Select(i => WebDriverHelper.Start(i, keepExisting: true)).ToArray();
        webDrivers.ForEach(webDriver => webDriver.Url = "http://imdb.com");

        int index = -1;
        webDrivers
            .ParallelForEach((webDriver, i) =>
            {
                for (int currentIndex = Interlocked.Increment(ref index); currentIndex < cacheFiles.Length; currentIndex = Interlocked.Increment(ref index))
                {
                    string keyWordFile = cacheFiles[currentIndex].Replace(".Keywords.bak.log", ".Keywords.log");
                    if (File.Exists(keyWordFile))
                    {
                        break;
                    }

                    string imdbId = Path.GetFileNameWithoutExtension(keyWordFile).Split(".").First();
                    string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";
                    string keywordsUrl = $"{imdbUrl}keywords/";
                    string keywordsHtml = WebDriverHelper.GetString(ref webDriver, keywordsUrl);
                    CQ keywordsCQ = keywordsHtml;
                    string[] allKeywords = keywordsCQ.Find("#keywords_content table td div.sodatext a").Select(keyword => keyword.TextContent.Trim()).ToArray();
                    Debug.Assert(allKeywords.IsEmpty());
                    if (true)
                    {
                        CQ seeAllButtonCQ = keywordsCQ.Find("span.chained-see-more-button button.ipc-see-more__button:contains('All') span.ipc-btn__text:visible");
                        Debug.Assert(seeAllButtonCQ.Length is 0 or 1);
                        if (true)
                        {
                            if (seeAllButtonCQ.Any() && webDriver.Url.EqualsIgnoreCase(keywordsUrl))
                            {
                                int rowCount = webDriver.FindElements(By.CssSelector("div[data-testid='sub-section']>ul>li")).Count;
                                IWebElement seeAllButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.chained-see-more-button button.ipc-see-more__button")).Last());
                                if (!seeAllButton.Displayed)
                                {
                                    seeAllButtonCQ = keywordsCQ.Find("span.single-page-see-more-button button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible");
                                    seeAllButton = new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("span.single-page-see-more-button button.ipc-see-more__button")).Last());
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

                                new WebDriverWait(webDriver, WebDriverHelper.DefaultManualWait).Until(driver =>
                                {
                                    keywordsHtml = webDriver.PageSource;
                                    keywordsCQ = keywordsHtml;
                                    rowCount = keywordsCQ.Find("div[data-testid='sub-section']>ul>li").Length;
                                    Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                                    return driver.FindElements(By.CssSelector("div[data-testid='sub-section']>ul>li")).Count == rowCount
                                        && keywordsCQ.Find("span.chained-see-more-button button.ipc-see-more__button:contains('All') span.ipc-btn__text:visible").IsEmpty()
                                        && keywordsCQ.Find("span.single-page-see-more-button button.ipc-see-more__button:contains('more') span.ipc-btn__text:visible").IsEmpty();
                                });
                            }

                            keywordsHtml = webDriver.PageSource;
                        }

                        //keywordsCQ = keywordsHtml;
                        //CQ allKeywordsCQ = keywordsCQ.Find("div[data-testid='sub-section']>ul>li");
                        //allKeywords = allKeywordsCQ
                        //    .Select(row => row.Cq())
                        //    .Select(rowCQ => HttpUtility.HtmlDecode(rowCQ.Children().Eq(0).Text()))
                        //    .ToArray();

                        //CQ oldKeywordsCQ = File.ReadAllText(cacheFiles[currentIndex]);
                        //string[] oldKeywords = oldKeywordsCQ
                        //    .Select(row => row.Cq())
                        //    .Select(rowCQ => HttpUtility.HtmlDecode(rowCQ.Children().Eq(0).Text()))
                        //    .ToArray();

                        //log($"{allKeywordsCQ.Length} - {oldKeywords.Length}");
                    }

                    File.WriteAllText(keyWordFile, keywordsHtml);
                    File.WriteAllLines(keyWordFile + ".txt", allKeywords);
                }
            }, degree);

        webDrivers.ForEach(webDriver => webDriver.Dispose());
    }

    internal static async Task DownloadAllLibraryMoviesAsync(
        string libraryJsonPath,
        string cacheDirectory, string metadataDirectory,
        Func<IEnumerable<string>, IEnumerable<string>>? partition = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Dictionary<string, Dictionary<string, VideoMetadata>> libraryMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(libraryJsonPath))!;

        using IWebDriver webDriver = WebDriverHelper.Start();
        string[] cacheFiles = Directory.GetFiles(@cacheDirectory);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory);
        string[] imdbIds = libraryMetadata.Keys
            .Where(imdbId => Regex.IsMatch(imdbId, "tt[0-9]+"))
            .Order()
            .ToArray();
        int length = imdbIds.Length;
        if (partition is not null)
        {
            imdbIds = partition(imdbIds).ToArray();
        }

        imdbIds = imdbIds
            .Except(metadataFiles.Select(file => Path.GetFileNameWithoutExtension(file).Split("-").First()))
            .ToArray();
        int trimmedLength = imdbIds.Length;
        await imdbIds.ForEachAsync(async (imdbId, index) =>
            {
                log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                try
                {
                    await Video.DownloadImdbMetadataAsync(imdbId, cacheDirectory, metadataDirectory, cacheFiles, metadataFiles, webDriver, false, true, log);
                }
                catch (ArgumentOutOfRangeException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbId}");
                }
                catch (ArgumentException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbId}");
                }
            });
    }

    internal static async Task DownloadAllTVsAsync(
        string x265JsonPath,
        string[] tvDirectories, string cacheDirectory, string metadataDirectory,
        Func<int, Range>? getRange = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Dictionary<string, TopMetadata[]> x265Metadata = JsonSerializer.Deserialize<Dictionary<string, TopMetadata[]>>(await File.ReadAllTextAsync(x265JsonPath))!;
        string[] libraryImdbIds = tvDirectories.SelectMany(tvDirectory => Directory.EnumerateFiles(tvDirectory, Video.ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .Select(file => TryRead(file, out string? imdbId, out _, out _, out _, out _) ? imdbId : string.Empty)
            .Where(imdbId => imdbId.IsNotNullOrWhiteSpace())
            .ToArray();

        using IWebDriver webDriver = WebDriverHelper.Start();
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
            .Except(metadataFiles.Select(file => Path.GetFileNameWithoutExtension(file).Split("-").First()))
            .ToArray();
        int trimmedLength = imdbIds.Length;
        await imdbIds.ForEachAsync(async (imdbId, index) =>
            {
                log($"{index * 100 / trimmedLength}% - {index}/{trimmedLength} - {imdbId}");
                try
                {
                    await Video.DownloadImdbMetadataAsync(imdbId, cacheDirectory, metadataDirectory, cacheFiles, metadataFiles, webDriver, false, true, log);
                }
                catch (ArgumentOutOfRangeException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbId}");
                }
                catch (ArgumentException exception) /*when (exception.ParamName.EqualsIgnoreCase("imdbId"))*/
                {
                    log($"!!!{imdbId}");
                }
            });
    }
}