namespace MediaManager.Net;

using System.Web;
using CsQuery;
using Examples.Net;
using OpenQA.Selenium;

internal static class Douban
{
    internal static async Task<string> GetTitleAsync(IWebDriver webDriver, string imdbId)
    {
        string searchResultHtml = await webDriver.GetStringAsync($"https://search.douban.com/movie/subject_search?search_text={imdbId}&cat=1002");
        CQ searchResultCQ = searchResultHtml;
        string title = searchResultCQ.Find("div.item-root div.detail div.title a").Text();
        return Regex.Replace(HttpUtility.HtmlDecode(title), @"\([0-9]{4}\)$", string.Empty).Trim();
    }
}