namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Net;
using MediaManager.IO;

internal static class Contrast
{
    internal static async Task DownloadFromGalaxyAsync(ISettings settings, int pageCount, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ContrastMetadata[] metadata = await AsyncEnumerable
            .Range(0, pageCount)
            .Select(pageIndex => $"{settings.TVContrastUrl}{pageIndex}")
            .SelectMany(async (pageUrl, token) =>
            {
                using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                CQ cq = await httpClient.GetStringAsync(pageUrl, token);
                return cq
                    .Find("div.tgxtable div.tgxtablerow")
                    .Select(rowDom =>
                    {
                        CQ rowCellsCQ = rowDom.Cq().Find("div.tgxtablecell");
                        CQ linkCQ = rowCellsCQ.Eq(1).Find("a").Eq(0);
                        string title = linkCQ.Attr("title");
                        string link = linkCQ.Attr("href");
                        CQ imdbCQ = rowCellsCQ.Eq(1).Find("a").Eq(1);
                        string imdbLink = imdbCQ.Attr("href");
                        string imdbId = imdbLink.IsNotNullOrWhiteSpace() ? ImdbMetadata.ImdbIdSubstringRegex().Match(imdbLink).Value : string.Empty;
                        string torrent = rowCellsCQ.Eq(2).Find("a").Eq(0).Attr("href");
                        string magnet = rowCellsCQ.Eq(2).Find("a").Eq(1).Attr("href");
                        string uploader = rowCellsCQ.Eq(4).Text();
                        string size = rowCellsCQ.Eq(5).Text();
                        string seed = rowCellsCQ.Eq(8).Find("font").Eq(0).Text();
                        string leech = rowCellsCQ.Eq(8).Find("font").Eq(1).Text();
                        string dateAdded = rowCellsCQ.Eq(9).Text();
                        string rowMouseOver = rowDom.Cq().Attr("onmouseover");
                        string rowMouseOverHtml = rowMouseOver[rowMouseOver.IndexOfOrdinal("<")..(rowMouseOver.LastIndexOfOrdinal(">") + 1)].ReplaceOrdinal(@"\", string.Empty);
                        string image = rowMouseOverHtml[rowMouseOverHtml.IndexOfOrdinal("https://")..rowMouseOverHtml.LastIndexOfOrdinal("'")];
                        return new ContrastMetadata(
                            link, title, string.Empty, [], image, imdbId,
                            dateAdded, size, int.Parse(seed), int.Parse(leech), uploader, torrent, magnet);
                    });
            })
            .ToArrayAsync(cancellationToken);

        await JsonHelper.SerializeToFileAsync(metadata, settings.TVContrastMetadata, cancellationToken);
    }

    internal static async Task DownloadFromContrast(ISettings settings, string htmlFile, CancellationToken cancellationToken = default)
    {
        CQ tableCQ = CQ.CreateFragment(await File.ReadAllTextAsync(htmlFile, cancellationToken));
        ContrastMetadata[] downloaded = tableCQ.Children("tbody.table-group-divider").Children("tr")
            .Select(rowDom =>
            {
                CQ rowCQ = rowDom.Cq();
                string imdbUrl = rowCQ.Children("td").Eq(1).Find("a").Attr("href");
                string imdbId = imdbUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                string link = rowCQ.Children("td").Eq(2).Find("a").Attr("href");
                string title = link.ReplaceIgnoreCase("preview?title=", string.Empty);
                string image = (string)rowCQ.Children("td").Eq(2).Find("a").Data("poster");
                image = "https://image.tmdb.org/t/p/" + image;
                string magnetUrl = rowCQ.Children("td").Eq(4).Find("div").Attr("onclick").ReplaceIgnoreCase("copyToClipboard('", "").TrimEnd(')').TrimEnd('\'');
                DateTime dateTime = DateTime.ParseExact(rowCQ.Children("td").Eq(5).Text().Trim(), "dd-MM-yyyy (HH:mm)", null);
                string seed = rowCQ.Children("td").Eq(2).Find("span.seeders").Text().Trim();
                string leechers = rowCQ.Children("td").Eq(2).Find("span.leechers").Text().Trim();
                string size = rowCQ.Children("td").Eq(2).Find("span.badge").Eq(0).Text().Trim();
                return new ContrastMetadata(link, title, "", [], image, imdbId, dateTime.ToString("yyyy-MM-dd HH:mm:ss"), size, int.Parse(seed), int.Parse(leechers), "Kontrast.top", "", magnetUrl);
            })
            .ToArray();
        ContrastMetadata[] existing = await JsonHelper.DeserializeFromFileAsync<ContrastMetadata[]>(settings.TVContrastMetadata, cancellationToken);
        ContrastMetadata[] all = existing.UnionBy(downloaded, metadata => metadata.Title, StringComparer.OrdinalIgnoreCase).ToArray();
        ContrastMetadata[] existingDuplicate = existing.IntersectBy(downloaded.Select(metadata => metadata.Title), metadata => metadata.Title, StringComparer.OrdinalIgnoreCase).ToArray();
        ContrastMetadata[] downloadedDuplicate = downloaded.IntersectBy(existing.Select(metadata => metadata.Title), metadata => metadata.Title, StringComparer.OrdinalIgnoreCase).ToArray();
        Debug.Assert(existingDuplicate.Length == downloadedDuplicate.Length);
        Debug.Assert(existingDuplicate.Select(metadata => MagnetUri.Parse(metadata.Magnet).ExactTopic.ToUpperInvariant()).Order().SequenceEqual(
            downloadedDuplicate.Select(metadata => MagnetUri.Parse(metadata.Magnet).ExactTopic.ToUpperInvariant()).Order()));
        Debug.Assert(all.Length == downloaded.Length + existing.Length - existingDuplicate.Length);

        await JsonHelper.SerializeToFileAsync(all, settings.TVContrastMetadata, cancellationToken);
    }

    internal static async Task PrintTVVersionsAsync(ISettings settings, string[][]? tvDrives, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        tvDrives ??= [
            [
                settings.TVMainstream,
            ],
            [
                settings.TV4KHdr,
                settings.TVControversial,
                settings.TVDocumentary,
                settings.TVMainstreamChinese,
                settings.TVMainstreamOverflow,
            ]
        ];

        ContrastMetadata[] existing = await JsonHelper.DeserializeFromFileAsync<ContrastMetadata[]>(settings.TVContrastMetadata, cancellationToken);
        Dictionary<string, (string SeasonNumber, ContrastMetadata Metadata)[]> result = existing
            .Where(metadata => metadata.Title.ContainsIgnoreCase(settings.ContrastKeyword))
            .Select(metadata => (metadata, Match: Regex.Match(metadata.Title, @"\.(S[0-9]{2})\.")))
            .Where(metadata => metadata.Match.Success)
            .GroupBy(
                metadata => metadata.metadata.ImdbId,
                metadata => (SeasonNumber: metadata.Match.Groups[1].Value, Metadata: metadata.metadata))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(metadata => metadata.SeasonNumber).ToArray());

        tvDrives
            .SelectMany(directories => directories)
            .SelectMany(directory => Video.EnumerateDirectories(directory))
            .Select(tv =>
            {
                string? imdbId = ImdbMetadata.TryRead(tv, out imdbId, out _, out _, out _, out _) ? imdbId : string.Empty;
                return (tv, imdbId, Seasons: Directory.GetDirectories(tv, "Season *"));
            })
            .Where(tv => tv.imdbId.IsNotNullOrWhiteSpace())
            .ForEach(tv =>
            {
                if (!result.TryGetValue(tv.imdbId, out (string SeasonNumber, ContrastMetadata Metadata)[]? contrastSeasons))
                {
                    return;
                }

                (string SeasonNumber, string Season, bool IsTopOrKontrast)[] localSeasons = tv
                    .Seasons
                    .Select(season =>
                    {
                        string seasonNumber = PathHelper.GetFileNameWithoutExtension(season);
                        Debug.Assert(Regex.IsMatch(seasonNumber, "^Season [0-9]{2}$"));
                        seasonNumber = seasonNumber.Replace("Season ", "S");
                        return (
                            SeasonNumber: seasonNumber,
                            Season: season,
                            IsTopOrKontrast: Directory.EnumerateFiles(season).Count(file => file.IsVideo() && (file.ContainsIgnoreCase(settings.ContrastKeyword) || file.ContainsIgnoreCase(settings.TopEnglishKeyword))) > 1
                        );
                    })
                    .OrderBy(season => season.SeasonNumber)
                    .ToArray();

                localSeasons
                    .Where(localSeason => !localSeason.IsTopOrKontrast)
                    .ForEach(localSeason =>
                    {
                        (string SeasonNumber, ContrastMetadata Metadata)[] matches = contrastSeasons.Where(contrastSeason => contrastSeason.SeasonNumber.EqualsIgnoreCase(localSeason.SeasonNumber)).ToArray();
                        if (matches.Any())
                        {
                            matches.Select(contrastSeason => contrastSeason.Metadata.Title).Prepend(localSeason.Season).Append(string.Empty).ForEach(log);
                        }
                    });

                string localMaxSeasonNumber = localSeasons.Last().SeasonNumber;
                (string SeasonNumber, ContrastMetadata Metadata)[] additionalSeasons = contrastSeasons
                    .Where(contrastSeason => contrastSeason.SeasonNumber.CompareTo(localMaxSeasonNumber, StringComparison.OrdinalIgnoreCase) > 0)
                    .ToArray();
                if (additionalSeasons.Any())
                {
                    additionalSeasons.Select(contrastSeason => contrastSeason.Metadata.Title).Prepend(tv.tv).Append(string.Empty).ForEach(log);
                }
            });
    }
}