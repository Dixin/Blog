﻿namespace Examples.Net
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using CsQuery;
    using Examples.IO;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using OpenQA.Selenium;

    internal static class Imdb
    {
        internal static async Task<(string ImdbUrl, string ImdbHtml, string ParentUrl, string ParentHtml, string ReleaseUrl, string ReleaseHtml, ImdbMetadata ImdbMetadata)> DownloadAsync(
            string imdbId, bool useCache, string imdbFile, string parentFile, string releaseFile, IWebDriver? webDriver = null)
        {
            using WebClient? webClient = webDriver is null ? new() { Encoding = Encoding.UTF8 } : null;
            webClient?.AddChromeHeaders();

            string imdbUrl = $"https://www.imdb.com/title/{imdbId}/";
            string imdbHtml = useCache && File.Exists(imdbFile)
                ? await File.ReadAllTextAsync(imdbFile)
                : await Retry.FixedIntervalAsync(async () => webDriver is not null ? await webDriver.DownloadStringAsync(imdbUrl) : await webClient!.DownloadCompressedStringAsync(imdbUrl), retryCount: 10);
            CQ imdbCQ = imdbHtml;
            string json = imdbCQ.Find(@"script[type=""application/ld+json""]").Text();
            ImdbMetadata imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                json,
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException(json);

            string parentUrl = string.Empty;
            string parentHtml = string.Empty;
            string parentHref = imdbCQ.Find(@"div.titleParent a").FirstOrDefault()?.GetAttribute("href")
                ?? imdbCQ.Find(@"div").FirstOrDefault(div => div.Classes.Any(@class => @class.StartsWith("TitleBlock__SeriesParentLinkWrapper", StringComparison.Ordinal)))?.Cq().Find("a").Attr("href")
                ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(parentHref))
            {
                string parentImdbId = Regex.Match(parentHref, "tt[0-9]+").Value;
                (parentUrl, parentHtml, _, _, _, _, imdbMetadata.Parent) = await DownloadAsync(parentImdbId, useCache, parentFile, string.Empty, releaseFile);
            }

            string htmlTitle = imdbCQ.Find(@"title").Text();
            string htmlTitleYear = htmlTitle.Contains("(", StringComparison.Ordinal)
                ? htmlTitle[(htmlTitle.LastIndexOf("(", StringComparison.Ordinal) + 1)..htmlTitle.LastIndexOf(")", StringComparison.Ordinal)]
                : string.Empty;
            imdbMetadata.Year = Regex.Match(htmlTitleYear, "[0-9]{4}").Value;
            string htmlYear = imdbCQ.Find(@"#titleYear a").Text().Trim();
            if (string.IsNullOrWhiteSpace(htmlYear))
            {
                htmlYear = imdbCQ.Find(@"div.title_wrapper div.subtext a[title=""See more release dates""]").Text();
                htmlYear = Regex.Match(htmlYear, "[0-9]{4}").Value;
            }

            Debug.Assert(string.Equals(imdbMetadata.Year, htmlYear, StringComparison.Ordinal) || imdbMetadata.ImdbId is "tt2058092");

            imdbMetadata.Regions = imdbCQ
                .Find(@"#titleDetails .txt-block")
                .Elements
                .Select(element => new CQ(element).Text().Trim())
                .FirstOrDefault(text => text.StartsWith("Country:", StringComparison.InvariantCultureIgnoreCase))
                ?.Replace("Country:", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Split('|')
                .Select(region => region.Trim())
                .ToArray() ?? Array.Empty<string>();
            if (!imdbMetadata.Regions.Any())
            {
                imdbMetadata.Regions = imdbCQ.Find(@"a[href^=""/search/title?country_of_origin=""]").Select(link => link.TextContent.Trim()).Distinct(StringComparer.Ordinal).ToArray();
            }

            if (!imdbMetadata.Regions.Any())
            {
                imdbMetadata.Regions = imdbCQ.Find(@"a[href^=""/search/title/?country_of_origin=""]").Select(link => link.TextContent.Trim()).Distinct(StringComparer.Ordinal).ToArray();
            }

            if (!imdbMetadata.Regions.Any() && imdbMetadata.Parent is not null)
            {
                imdbMetadata.Regions = imdbMetadata.Parent.Regions;
            }

            imdbMetadata.Regions = imdbMetadata.Regions
                .Select(region => region switch
                {
                    "United States" => "USA",
                    "United Kingdom" => "UK",
                    _ => region
                })
                .ToArray();

            //Debug.Assert(imdbMetadata.Regions.Any() || imdbMetadata.ImdbId is "tt0166122" or "tt6922816" or "tt12229160" or "tt6900644");

            imdbMetadata.Languages = imdbCQ
                .Find(@"#titleDetails .txt-block")
                .Elements
                .Select(element => new CQ(element).Text().Trim())
                .FirstOrDefault(text => text.StartsWith("Language:", StringComparison.InvariantCultureIgnoreCase))
                ?.Replace("Language:", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Split('|')
                .Select(region => region.Trim())
                .ToArray() ?? Array.Empty<string>();
            if (!imdbMetadata.Languages.Any())
            {
                imdbMetadata.Languages = imdbCQ.Find(@"a[href^=""/search/title?title_type=feature&primary_language=""]").Select(link => link.TextContent.Trim()).Distinct(StringComparer.Ordinal).ToArray();
            }

            if (!imdbMetadata.Languages.Any())
            {
                imdbMetadata.Languages = imdbCQ.Find(@"a[href^=""/search/title/?title_type=feature&primary_language=""]").Select(link => link.TextContent.Trim()).Distinct(StringComparer.Ordinal).ToArray();
            }

            if (!imdbMetadata.Languages.Any() && imdbMetadata.Parent is not null)
            {
                imdbMetadata.Languages = imdbMetadata.Parent.Languages;
            }

            //Debug.Assert(imdbMetadata.Languages.Any() || imdbMetadata.ImdbId 
            //    is "tt0226895" or "tt0398936" or "tt6922816" or "tt3061100" or "tt3877124" or "tt0219913" or "tt0108361"
            //    or "tt0133065" or "tt0173797" or "tt2617008" or "tt1764627" or "tt0225882" or "tt10540298" or "tt0195707" or "tt3807900" or "tt2946498"
            //    or "tt9395794");

            string releaseUrl = imdbMetadata.Parent is null ? $"{imdbUrl}releaseinfo" : $"{imdbMetadata.Parent.Link}releaseinfo";
            string releaseHtml = useCache && File.Exists(releaseFile)
                ? await File.ReadAllTextAsync(releaseFile)
                : await Retry.FixedIntervalAsync(async () => webDriver is not null ? await webDriver.DownloadStringAsync(releaseUrl) : await webClient!.DownloadStringTaskAsync(releaseUrl), retryCount: 10);
            CQ releaseCQ = releaseHtml;
            CQ allTitlesCQ = releaseCQ.Find("#akas").Next();
            (string TitleKey, string TitleValue)[] allTitles = allTitlesCQ
                .Find("tr")
                .Select(row => row.Cq().Children())
                .Select(cells => (TitleKey: HttpUtility.HtmlDecode(cells.First().Text().Trim()), TitleValue: HttpUtility.HtmlDecode(cells.Last().Text().Trim()))).ToArray();
            Enumerable.Range(0, allTitles.Length).ForEach(index =>
            {
                if (string.IsNullOrWhiteSpace(allTitles[index].TitleKey))
                {
                    allTitles[index].TitleKey = allTitles[index - 1].TitleKey;
                }
            });

            imdbMetadata.Titles = allTitles
                .ToLookup(row => row.TitleKey, row => row.TitleValue, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToArray());
            Debug.Assert(imdbMetadata.Titles.Any() || allTitlesCQ.Text().Contains("It looks like we don't have any AKAs for this title yet."));

            if (webDriver is not null)
            {
                imdbMetadata.Title = imdbCQ
                    .Find("div.title_wrapper h1")
                    .Find("#titleYear")
                    .Remove()
                    .End()
                    .Text()
                    .Trim();

                imdbMetadata.OriginalTitle = imdbCQ
                    .Find("div.originalTitle")
                    .Find("span")
                    .Remove()
                    .End()
                    .Text()
                    .Trim();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(imdbMetadata.AlternateName))
                {
                    imdbMetadata.Title = imdbMetadata.AlternateName;
                    Debug.Assert(!string.IsNullOrWhiteSpace(imdbMetadata.Name));
                    imdbMetadata.OriginalTitle = imdbMetadata.Name;
                }
                else
                {
                    if (imdbMetadata.Titles.Any())
                    {
                        string releaseHtmlTitle = releaseCQ.Find("title").Text();
                        releaseHtmlTitle = releaseHtmlTitle.Substring(0, releaseHtmlTitle.LastIndexOf("(", StringComparison.Ordinal)).Trim();

                        if (!imdbMetadata.Titles.TryGetValue("(original title)", out string[]? originalTitleValues))
                        {
                            string[] originalTitleKeys = imdbMetadata.Titles.Keys.Where(key => key.Contains("original title", StringComparison.OrdinalIgnoreCase)).ToArray();
                            if (originalTitleKeys.Length > 1)
                            {
                                originalTitleKeys = originalTitleKeys.Where(key => !key.Contains("USA", StringComparison.OrdinalIgnoreCase)).ToArray();
                            }

                            originalTitleValues = originalTitleKeys.SelectMany(key => imdbMetadata.Titles[key]).ToArray();
                        }

                        if (originalTitleValues.Length > 1)
                        {
                            originalTitleValues = originalTitleValues.Where(title => !string.Equals(title, releaseHtmlTitle, StringComparison.Ordinal)).ToArray();
                        }

                        if (originalTitleValues.Length > 1)
                        {
                            originalTitleValues = originalTitleValues.Where(title => !string.Equals(title, releaseHtmlTitle, StringComparison.OrdinalIgnoreCase)).ToArray();
                        }

                        imdbMetadata.OriginalTitle = originalTitleValues.Length switch
                        {
                            1 => originalTitleValues.Single(),
                            > 1 => originalTitleValues.FirstOrDefault(titleValue => originalTitleValues.Except(EnumerableEx.Return(titleValue)).All(otherTitle => titleValue.Contains(otherTitle, StringComparison.OrdinalIgnoreCase)))
                                ?? string.Join(TitleSeparator, originalTitleValues),
                            _ => string.Empty
                        };

                        if (!imdbMetadata.Titles.TryGetValue("World-wide (English title)", out string[]? titleValues))
                        {
                            titleValues = imdbMetadata.Titles
                                .Where(pair => pair.Key.Contains("World-wide (English title)", StringComparison.OrdinalIgnoreCase))
                                .SelectMany(pair => pair.Value)
                                .ToArray();
                        }

                        if (!titleValues.Any()
                            && !imdbMetadata.Titles.TryGetValue("USA", out titleValues)
                            && !imdbMetadata.Titles.TryGetValue("USA (working title)", out titleValues)
                            && !imdbMetadata.Titles.TryGetValue("USA (informal English title)", out titleValues)
                            && !imdbMetadata.Titles.TryGetValue("UK", out titleValues)
                            && !imdbMetadata.Titles.TryGetValue("UK (informal English title)", out titleValues)
                            && !imdbMetadata.Titles.TryGetValue("Hong Kong (English title)", out titleValues)
                            && !imdbMetadata.Titles.TryGetValue("(original title)", out titleValues))
                        {
                            titleValues = Array.Empty<string>();
                        }

                        Debug.Assert(titleValues.Any());
                        imdbMetadata.Title = titleValues.Length switch
                        {
                            1 => titleValues.Single(),
                            > 1 => titleValues.FirstOrDefault(availableTitle => string.Equals(releaseHtmlTitle, availableTitle, StringComparison.Ordinal))
                                ?? titleValues.FirstOrDefault(availableTitle => string.Equals(releaseHtmlTitle, availableTitle, StringComparison.OrdinalIgnoreCase))
                                ?? titleValues.FirstOrDefault(titleValue => titleValues.Except(EnumerableEx.Return(titleValue)).All(otherTitle => titleValue.Contains(otherTitle, StringComparison.OrdinalIgnoreCase)))
                                ?? string.Join(TitleSeparator, titleValues),
                            _ => releaseHtmlTitle
                        };

                        Debug.Assert(!string.IsNullOrWhiteSpace(imdbMetadata.Title));
                    }
                    else
                    {
                        imdbMetadata.Title = imdbCQ
                            .Find("div.title_wrapper h1")
                            .Find("#titleYear")
                            .Remove()
                            .End()
                            .Text()
                            .Replace("&nbsp;", string.Empty)
                            .Trim();

                        imdbMetadata.OriginalTitle = imdbCQ
                            .Find("div.originalTitle")
                            .Find("span")
                            .Remove()
                            .End()
                            .Text()
                            .Trim();

                        if (string.IsNullOrWhiteSpace(imdbMetadata.Title))
                        {
                            imdbMetadata.Title = imdbCQ
                                .Find(@"h1[data-testid=""hero-title-block__title""]")
                                .Find("#titleYear")
                                .Remove()
                                .End()
                                .Text()
                                .Trim();
                        }

                        htmlTitle = htmlTitle.Contains("(", StringComparison.Ordinal)
                            ? htmlTitle.Substring(0, htmlTitle.LastIndexOf("(", StringComparison.Ordinal)).Trim()
                            : htmlTitle.Replace("- IMDB", string.Empty).Trim();
                        Debug.Assert(string.Equals(imdbMetadata.Title, htmlTitle, StringComparison.Ordinal) || htmlTitle.Contains(imdbMetadata.Title, StringComparison.Ordinal));
                    }
                }
            }

            if (string.Equals(imdbMetadata.Title, imdbMetadata.OriginalTitle, StringComparison.OrdinalIgnoreCase))
            {
                imdbMetadata.OriginalTitle = string.Empty;
            }

            Debug.Assert(!string.IsNullOrWhiteSpace(imdbMetadata.Title));

            imdbMetadata.Title = HttpUtility.HtmlDecode(imdbMetadata.Title).Trim();
            imdbMetadata.OriginalTitle = HttpUtility.HtmlDecode(imdbMetadata.OriginalTitle).Trim();

            return (imdbUrl, imdbHtml, parentUrl, parentHtml, releaseUrl, releaseHtml, imdbMetadata);
        }

        internal const string TitleSeparator = "~";

        internal static bool TryLoad(string path, [NotNullWhen(true)] out ImdbMetadata? imdbMetadata)
        {
            if (Directory.Exists(path))
            {
                path = Directory.GetFiles(path, Video.JsonMetadataSearchPattern, SearchOption.TopDirectoryOnly).Single();
            }

            if (string.Equals(Path.GetFileNameWithoutExtension(path), Video.NotExistingFlag))
            {
                imdbMetadata = null;
                return false;
            }

            imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                File.ReadAllText(path),
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true }) ?? throw new InvalidOperationException(path);
            return true;
        }
    }
}