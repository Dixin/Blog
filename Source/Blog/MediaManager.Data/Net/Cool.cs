namespace MediaManager.Net;

using CsQuery;
using Examples.Common;
using Examples.Net;
using MediaManager.IO;

internal static class Cool
{
    internal static async Task DownloadAllThreadsAsync(int start, int end, string directory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(
            Enumerable.Range(start, end - start + 1),
            new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
            async (threadId, token) =>
            {
                if (threadId % 1000 == 0)
                {
                    log($"Saved {threadId}.");
                }

                await DownloadThreadAsync(threadId, directory, log, token);
            });
    }

    internal static async Task DownloadThreadAsync(int threadId, string directory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        string pageHtml = await httpClient.GetStringAsync($"https://www.cool18.com/bbs4/index.php?app=forum&act=threadview&tid={threadId}", cancellationToken);
        if (pageHtml.EqualsIgnoreCase("\r\n<script language='javascript'>window.location='index.php';</script>"))
        {
            log($"Skip {threadId}");
            return;
        }

        CQ pageCQ = CQ.CreateDocument(pageHtml);
        pageCQ.Children(":not(head, body)").Remove();

        CQ headCQ = pageCQ.Children("head");
        headCQ.Children(":not(title)").Remove();
        headCQ.Append("""<link rel="stylesheet" href="../styles/thread.css" />""");
        headCQ.Append("""<script type="text/javascript" src="../scrips/thread.js"></script>""");


        CQ bodyCQ = pageCQ.Children("body");
        bodyCQ.Children(":not(table)").Remove();
        bodyCQ.RemoveAttr("leftmargin").RemoveAttr("topmargin").RemoveAttr("marginwidth").RemoveAttr("marginheight").RemoveAttr("bgcolor");

        CQ table1CQ = bodyCQ.Children("table").Eq(0);
        table1CQ.Children("tbody").Children("tr").Where((dom, index) => index != 1).ToArray().ForEach(dom => dom.Remove());
        table1CQ.Find("tr table td.w5").Remove();
        table1CQ.Find("pre").Next("center").Remove();

        CQ table2CQ = bodyCQ.Children("table").Eq(1);
        table2CQ.Children("tbody").Children("tr").Children("td").Children(":not(ul)").Remove();
        if (table2CQ.Text().IsNullOrWhiteSpace())
        {
            table2CQ.Remove();
        }

        bodyCQ.Find("font:contains('cool18.com'), font:contains('6park.com'), button").Remove();
        bodyCQ.Find("table").RemoveAttr("border").RemoveAttr("align").RemoveAttr("cellpadding").RemoveAttr("cellspacing").RemoveAttr("width");
        bodyCQ.Find("tr").RemoveAttr("bgcolor");
        bodyCQ.Find("td").RemoveAttr("height");

        bodyCQ.Find("a[href^='index.php?app=forum&act=threadview&tid=']").Each(dom =>
        {
            string href = dom.GetAttribute("href");
            href = Regex.Match(href, @"^index\.php\?app=forum&act=threadview&tid=([0-9]+)$").Groups[1].Value;
            dom.SetAttribute("href", $"{href}.htm");
        });

        string html = pageCQ.Render(DomRenderingOptions.RemoveComments | DomRenderingOptions.QuoteAllAttributes);
        html = WebUtility.HtmlDecode(html);
        await File.WriteAllTextAsync(Path.Combine(directory, $"{threadId}.htm"), html, cancellationToken);
    }
}
