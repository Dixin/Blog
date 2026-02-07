namespace MediaManager.Net;

using System.Web;
using CsQuery;

internal static class CQExtensions
{
    internal static string TextTrimDecode(this CQ cq) => HttpUtility.HtmlDecode(cq.Text().Trim());

    internal static string TextTrimDecode(this IDomObject dom) => HttpUtility.HtmlDecode(dom.TextContent.Trim());

    internal static string HtmlTrim(this CQ cq) => cq.Html().Trim();

    internal static string HtmlTrim(this IDomObject dom) => dom.InnerHTML.Trim();
}