namespace MediaManager.Net;

using System.Web;
using CsQuery;
using Microsoft.Playwright;

internal static class Douban
{
    internal static readonly int MaxDegreeOfParallelism = 1;

    internal static async Task<string> GetTitleAsync(IPage page, string imdbId, CancellationToken cancellationToken = default)
    {
        string searchResultHtml = await page.GetStringAsync($"https://search.douban.com/movie/subject_search?search_text={imdbId}&cat=1002", "root", cancellationToken: cancellationToken);
        CQ searchResultCQ = searchResultHtml;
        string title = searchResultCQ.Find("div.item-root div.detail div.title a").Text();
        return Regex.Replace(HttpUtility.HtmlDecode(title), @"\([0-9]{4}\)$", string.Empty).Trim();
    }
}