namespace MediaManager.Net;

using System.Web;
using CsQuery;

internal static class Douban
{
    internal static readonly int MaxDegreeOfParallelism = int.Min(4, Environment.ProcessorCount);

    internal static async Task<string> GetTitleAsync(WebDriverWrapper webDriver, string imdbId, CancellationToken cancellationToken = default)
    {
        string searchResultHtml = await webDriver.GetStringAsync($"https://search.douban.com/movie/subject_search?search_text={imdbId}&cat=1002", cancellationToken: cancellationToken);
        CQ searchResultCQ = searchResultHtml;
        string title = searchResultCQ.Find("div.item-root div.detail div.title a").Text();
        return Regex.Replace(HttpUtility.HtmlDecode(title), @"\([0-9]{4}\)$", string.Empty).Trim();
    }
}