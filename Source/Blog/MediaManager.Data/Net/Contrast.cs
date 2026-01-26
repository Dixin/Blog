namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.Net;
using System.Linq;
using MediaManager.IO;

internal static class Contrast
{
    internal static async Task DownloadAsync(ISettings settings, int pageCount, Action<string>? log = null, CancellationToken cancellationToken = default)
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
}