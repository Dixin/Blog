namespace MediaManager.Net;

using System;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Net;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MemoryExtensions = System.MemoryExtensions;

internal static class Cool
{
    internal static async Task DownloadAllThreadsAsync(int start, int end, string directory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(
            Enumerable.Range(start, end + 1 - start),
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
    internal static async Task DownloadAllThreadsWithTemplateAsync(int start, int end, string directory, bool overwrite = false, int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        IEnumerable<int> threadIds = overwrite
            ? Enumerable
                .Range(start, end - start + 1)
            : Enumerable
                .Range(start, end - start + 1)
                .Except(Directory
                    .EnumerateFiles(directory)
                    .Select(file => int.Parse(PathHelper.GetFileNameWithoutExtension(file))));
        ConcurrentQueue<int> queue = new(threadIds);
        maxDegreeOfParallelism ??= Environment.ProcessorCount;
        await Parallel.ForEachAsync(
            Enumerable.Range(0, maxDegreeOfParallelism.Value),
            new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = maxDegreeOfParallelism.Value },
            async (index, token) =>
            {
                using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                while (queue.TryDequeue(out int threadId))
                {
                    if (threadId % 1000 == 0)
                    {
                        log($"Saved {threadId}.");
                    }

                    await DownloadThreadWithTemplateAsync(threadId, directory, httpClient, log, token);
                }
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
        headCQ.Append("""<link rel="stylesheet" href="../style/thread.css" />""");
        headCQ.Append("""<script type="text/javascript" src="../script/thread.js"></script>""");

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

    internal static async Task DownloadThreadWithTemplateAsync(int threadId, string directory, HttpClient? httpClient = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        bool dispose = httpClient is null;
        httpClient ??= new HttpClient().AddEdgeHeaders();

        await using Stream stream = await Retry.FixedIntervalAsync(
            async () => await httpClient.GetStreamAsync($"https://www.cool18.com/bbs4/index.php?app=forum&act=threadview&tid={threadId}", cancellationToken),
            cancellationToken: cancellationToken);
        //if (pageHtml.EqualsIgnoreCase("\r\n<script language='javascript'>window.location='index.php';</script>"))
        //{
        //    log($"Skip {threadId}");
        //    return;
        //}

        CQ documentCQ = CQ.CreateDocument(stream);

        if (documentCQ["head script"].Text().EqualsIgnoreCase("window.location='index.php';") && documentCQ["body"].Children().IsEmpty())
        {
            log($"Skip {threadId}");
            return;
        }

        string title = documentCQ.Find("head title").Text().Trim().Replace(" - cool18.com", string.Empty);
        CQ headerCQ = documentCQ.Find("body table:eq(0) td.show_content center:eq(0)");
        string header = headerCQ.Text().Trim();
        CQ userTableCQ = headerCQ.Next("table.tab5");
        CQ userCQ = userTableCQ.Find("td:eq(0) a:eq(0)");
        string userName = userCQ.Text().Trim();
        string userLink = userTableCQ.Find("td:eq(2) a").Attr("href").Trim();
        string userId = WebUtility.UrlDecode(Regex.Match(userLink, "uname=(.+)$", RegexOptions.IgnoreCase).Groups[1].Value);
        string threadText = userTableCQ.Find("td:eq(0)").Text().Trim();
        string threadDateTime = Regex.Match(threadText, @"[0-9]+\-[0-9]+\-[0-9]+ [0-9]+\:[0-9]+").Value;
        string threadView = Regex.Match(threadText, @"已读 ([0-9]+) 次").Groups[1].Value;
        CQ parentCQ = userTableCQ.Next("p");
        string parentHtml = string.Empty;
        if (parentCQ.Any())
        {
            CQ parentLinkCQ = parentCQ.Find("a");
            string href = parentLinkCQ.Attr("href");
            string parentThreadId = Regex.Match(href, @"tid=([0-9]+)$", RegexOptions.IgnoreCase).Groups[1].Value;
            string parentThreadTitle = parentLinkCQ.Text().Trim();
            string text = parentLinkCQ.Elements.Single().NextSibling.NodeValue.Trim();
            Match match = Regex.Match(text, @"由 (.+) 于 ([0-9]+\-[0-9]+\-[0-9]+ [0-9]+\:[0-9]+)$");
            string parentThreadUserName = match.Groups[1].Value;
            string parentThreadDateTime = match.Groups[2].Value;
            parentHtml = $"""
                <div id ="parent">
                <p><a id="parentThread" href="{parentThreadId}.htm">{parentThreadTitle}</a></p>
                <p id="parentThreadUserName">{parentThreadUserName}</p>
                <p id="parentThreadDateTime">{parentThreadDateTime}</p>
                </div>
                """;
        }

        CQ preCQ = userTableCQ.NextAll("pre");
        //preCQ.Find("font:contains('cool18.com'), font:contains('6park.com')").Remove();
        preCQ.Find("a").Each(dom =>
        {
            dom.RemoveAttribute("target");
            dom.TextContent = dom.TextContent.Replace('\u3000', ' ');
            string href = dom.GetAttribute("href");
            if (href.IsNullOrWhiteSpace())
            {
                return;
            }

            Match match = Regex.Match(href, @"(cool18|6park).+[tc]id=([0-9]+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                dom.SetAttribute("href", $"{match.Groups[2].Value}.htm");
                return;
            }

            match = Regex.Match(href, @"/([0-9]+)\.html$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                dom.SetAttribute("href", $"{match.Groups[1].Value}.htm");
                return;
            }

            log($"{threadId}: Link is removed {href}");
            dom.Remove();
        });

        string lastEditUserName = string.Empty;
        string lastEditDateTime = string.Empty;

        //preCQ.Find("b, i, u").Where(dom => dom.TextContent.IsNullOrWhiteSpace()).ForEach(dom => dom.Remove());
        preCQ.Find("b, i, u, img").Each(dom => dom.Cq().ReplaceWith(dom.ChildNodes));
        preCQ.Find("*").Each(dom =>
        {
            string text = dom.TextContent;
            if (Regex.IsMatch(text, @"\s*评分完成.+加.+银元[\p{P}]*\s*"))
            {
                dom.Remove();
                return;
            }

            Match match = Regex.Match(text, ".主:(.+)于([0-9]+_[0-9]+_[0-9]+ [0-9]+:[0-9]+:[0-9]+)编辑");
            if (match.Success)
            {
                lastEditUserName = match.Groups[1].Value;
                lastEditDateTime = match.Groups[2].Value.Replace("_", "-");
                dom.Remove();
                return;
            }

            match = Regex.Match(text, @"本贴由\[(.+)\]最后编辑于: ([0-9]+)日/([0-9]+)月/([0-9]+) ([0-9]+)时([0-9]+)分([0-9]+)秒");
            if (match.Success)
            {
                lastEditUserName = match.Groups[1].Value;
                lastEditDateTime = match.Result("$4-$3-$2 $5:$6:$7");
                dom.Remove();
            }
        });

        preCQ.Find("*[style*='E6E6DD']").Each(dom =>
        {
            log($"{threadId}: Removed {dom.NodeName}:{dom.TextContent.Trim()}");
            dom.Remove();
        });

        string contentHtml = WebUtility.HtmlDecode(preCQ.Html())
            .Replace("\t", "\u3000\u3000")
            .Replace("\n    ", "\n\u3000\u3000")
            .Replace("\n??", "\n\u3000\u3000");

        int indexOfBodyEnd = contentHtml.IndexOfIgnoreCase("<!--bodyend-->");
        if (indexOfBodyEnd >= 0)
        {
            contentHtml = contentHtml[..indexOfBodyEnd];
            preCQ.Html(contentHtml);
        }

        //preCQ.Find("font[color*='E6E6DD']").Each(dom => Debug.Assert(dom.TextContent.Trim() is "cool18.com" or "www.6park.com"));

        if (contentHtml.IsNullOrWhiteSpace())
        {
            contentHtml = string.Empty;
        }
        else
        {
            //IGrouping<string, IDomElement>[] groups = preCQ.Single().ChildElements.GroupBy(element => element.NodeName.ToLowerInvariant()).OrderByDescending(group => group.Count()).ToArray();
            //log($"{threadId} Children {string.Join("|", groups.Select(group => $"{group.Key}*{group.Count()}"))}");

            if (contentHtml.AllIndexesOfOrdinal("<").Any(index => index + 1 < contentHtml.Length && contentHtml[index + 1] > 'a'))
            {
                StringBuilder contentHtmlBuilder = new(contentHtml);
                contentHtmlBuilder = contentHtmlBuilder
                    .TrimStart("\n ")
                    .TrimEnd()
                    .Replace("\n", string.Empty);
                contentHtml = contentHtmlBuilder.ToString();
                contentHtml = Regex.Replace(contentHtml, @"<font color=""[#]?E6E6DD"">[^<]*</font>", "\n\n", RegexOptions.IgnoreCase);
                contentHtml = Regex.Replace(contentHtml, @">(\s*[\p{P}\u3000 ]{10,})", ">\n\n$1");
                contentHtml = Regex.Replace(contentHtml, @"(\s*[\p{P}\u3000 ]{10,})<", "$1\n\n<");
                contentHtmlBuilder = contentHtmlBuilder.Clear().Append(contentHtml);

                int paragraphCount = preCQ.Children("p").Length;
                int lineBreakCount = preCQ.Children("br").Length;
                if (paragraphCount > 0 && lineBreakCount == 0 || paragraphCount >= lineBreakCount * 10 || paragraphCount >= 10 && lineBreakCount <= 5)
                {
                    contentHtmlBuilder = contentHtmlBuilder
                        .Replace("<p>", "\n\n")
                        .Replace("</p>", "\n\n")
                        .Replace("<br />", "\n");
                }
                else if (paragraphCount == 0 && lineBreakCount > 0)
                {
                    contentHtmlBuilder = contentHtmlBuilder
                        .Replace("<br />", "\n\n");
                }
                else
                {
                    contentHtmlBuilder = contentHtmlBuilder
                        .Replace("<p>", "\n\n")
                        .Replace("</p>", "\n\n")
                        .Replace("<br />", "\n");
                    //Debugger.Break();
                }

                contentHtml = contentHtmlBuilder.TrimStart("\n ").TrimEnd().ToString();
            }

            if (contentHtml.IsNotNullOrWhiteSpace()
                && contentHtml.AllIndexesOfOrdinal("<").Any(index => index + 1 < contentHtml.Length && contentHtml[index + 1] > 'a'))
            {
                preCQ.Html(contentHtml);
                CQ preChildrenCQ = preCQ.Find(":not(a)");
                if (preChildrenCQ.Any())
                {
                    preChildrenCQ.Each(dom => dom.Cq().ReplaceWith(dom.ChildNodes));
                    //Debug.Assert(preCQ.Find(":not(a)").IsEmpty());
                    contentHtml = WebUtility.HtmlDecode(preCQ.Html())
                        .TrimStart(' ', '\n')
                        .TrimEnd();
                }
            }

            Debug.Assert(preCQ.Html(contentHtml).Find(":not(a)").IsEmpty());
            if (contentHtml.IsNullOrWhiteSpace())
            {
                contentHtml = string.Empty;
            }
            else
            {
                contentHtml = Regex.Replace(contentHtml, @"\n[\p{P}\u3000 ]{10,}", "\n$0");
                contentHtml = Regex.Replace(contentHtml, @"[\p{P}\u3000 ]{10,}\n", "$0\n");
                contentHtml = Regex.Replace(contentHtml, @"[\p{P}\u3000 ]{20,}", "\n\n$0\n\n");
                contentHtml = Regex.Replace(contentHtml, "http://[a-z]+.6park.com/bbs4/messages/([0-9]+).html", "$1.htm", RegexOptions.IgnoreCase);
                contentHtml = Regex.Replace(contentHtml, "https://www.cool18.com/bbs4/index.php?app=forum&act=threadview&tid=([0-9]+)", "$1.htm", RegexOptions.IgnoreCase);
                contentHtml = Regex.Replace(contentHtml, @"([^\n])\n\u3000\u3000", "$1\n\n\u3000\u3000");
                contentHtml = Regex.Replace(contentHtml, @"\n[\s-[\n]]{4}(\S)", "\n\n\u3000\u3000$1");
                contentHtml = Regex.Replace(contentHtml, @"\n    ([^ ])", "\n\n\u3000\u3000$1");
                contentHtml = Regex.Replace(contentHtml, @"\n    ([^ ])", "\n\n\u3000\u3000$1");
                contentHtml = contentHtml
                    .ReplaceIgnoreCase("<a", "\n\n<a")
                    .ReplaceIgnoreCase("a>", "a>\n\n")
                    .TrimEnd();

                int lastLineBreakIndex = contentHtml.LastIndexOf('\n');
                string lastLine = lastLineBreakIndex >= 0 ? contentHtml[(lastLineBreakIndex + 1)..] : contentHtml;
                Match match = Regex.Match(lastLine, ".主:(.+)于([0-9]+_[0-9]+_[0-9]+ [0-9]+:[0-9]+:[0-9]+)编辑");
                if (match.Success)
                {
                    lastEditUserName = match.Groups[1].Value;
                    lastEditDateTime = match.Groups[2].Value.Replace("_", "-");
                    contentHtml = lastLineBreakIndex >= 0 ? contentHtml[..lastLineBreakIndex] : contentHtml[..match.Index];
                }
                else
                {
                    match = Regex.Match(lastLine, @"本贴由\[(.+)\]最后编辑于: ([0-9]+)日/([0-9]+)月/([0-9]+) ([0-9]+)时([0-9]+)分([0-9]+)秒");
                    if (match.Success)
                    {
                        lastEditUserName = match.Groups[1].Value;
                        lastEditDateTime = match.Result("$4-$3-$2 $5:$6:$7");
                        contentHtml = lastLineBreakIndex >= 0 ? contentHtml[..lastLineBreakIndex] : contentHtml[..match.Index];
                    }
                }

                MemoryExtensions.SpanSplitEnumerator<char> linesEnumerator = contentHtml.AsSpan().Split('\n');
                List<(Range, (int Offset, int Length))> lines = new();
                while (linesEnumerator.MoveNext())
                {
                    lines.Add((linesEnumerator.Current, linesEnumerator.Current.GetOffsetAndLength(contentHtml.Length)));
                }

                if (lines.Any(line => line.Item2.Length >= 500))
                {
                    if (contentHtml.ContainsOrdinal("\u3000\u3000"))
                    {
                        contentHtml = Regex.Replace(contentHtml, @"[\u3000]{2,}", "\n\n\u3000\u3000");

                        linesEnumerator = contentHtml.AsSpan().Split('\n');
                        lines.Clear();
                        while (linesEnumerator.MoveNext())
                        {
                            lines.Add((linesEnumerator.Current, linesEnumerator.Current.GetOffsetAndLength(contentHtml.Length)));
                        }
                    }
                    else
                    {
                        log($"{threadId}: large paragraph is not processed.");
                        ;
                        //Debugger.Break();
                    }
                }

                int[] nonEmptyLinesWithoutLink = lines
                    .Where(line =>
                    {
                        if (line.Item2.Length == 0)
                        {
                            return false;
                        }

                        ReadOnlySpan<char> lineSpan = contentHtml.AsSpan(line.Item1);
                        return !lineSpan.Contains("<a", StringComparison.OrdinalIgnoreCase) && !Regex.IsMatch(lineSpan, @"[\p{P}\u3000 ]{10,}");
                    })
                    .Select(line => line.Item2.Length)
                    .OrderDescending()
                    .ToArray();
                if (nonEmptyLinesWithoutLink.Length > 1)
                {
                    (int sameLength, int sameLengthCount) = nonEmptyLinesWithoutLink
                        .Skip(1)
                        .GroupBy(length => length)
                        .Select(group => (Length: group.Key, Count: group.Count()))
                        .MaxBy(group => group.Count);
                    int[] trimmedNonEmptyLinesWithoutLink = nonEmptyLinesWithoutLink.Skip(int.Max(1, nonEmptyLinesWithoutLink.Length / 100)).ToArray();
                    if (trimmedNonEmptyLinesWithoutLink.All(length => length <= 50) && trimmedNonEmptyLinesWithoutLink.Any(length => length >= 35)
                        || sameLengthCount > 1 && sameLengthCount >= nonEmptyLinesWithoutLink.Length / 5 && trimmedNonEmptyLinesWithoutLink.All(length => sameLength + 15 >= length))
                    {
                        int leadingWhiteSpaceCount = lines
                            .Count(line => line.Item2.Length > 2 && contentHtml[line.Item2.Offset] == '\u3000' && contentHtml[line.Item2.Offset + 1] == '\u3000');
                        if (leadingWhiteSpaceCount >= 10 || leadingWhiteSpaceCount >= trimmedNonEmptyLinesWithoutLink.Length / 5)
                        {
                            contentHtml = Regex.Replace(contentHtml, @"([^\n^>])[\n]{1,2}([^\n^<])", "$1$2");
                            contentHtml = Regex.Replace(contentHtml, @"[\u3000]{2,}", "\n\n\u3000\u3000");
                        }
                        else if (lines.Skip(1).SkipLast(1).Any(line => line.Item2.Length == 0))
                        {
                            contentHtml = Regex.Replace(contentHtml, @"([^\n^>])\n([^\n^<])", "$1$2");
                        }
                        else
                        {
                            //Debugger.Break();
                        }
                    }
                }

                contentHtml = Regex.Replace(contentHtml, @"[\s-[\n]]+\n", "\n");
                //contentHtml = Regex.Replace(contentHtml, @"\n([\p{P}\u3000 ]{0,10})[\n]+([\p{P}\u3000 ]{0,10})\n", "\n$1$2\n");
                contentHtml = Regex.Replace(contentHtml, "[\n]{3,}", "\n\n")
                    .TrimStart(' ', '\n')
                    .TrimEnd();
            }
        }

        //contentHtml = Regex.Replace(contentHtml, """<font color="[0-9A-Za-z]+">cool18.com</font>""", $"{Environment.NewLine}{Environment.NewLine}");
        //contentHtml = Regex.Replace(contentHtml, """<font color="[0-9A-Za-z]+"> www.6park.com</font>""", $"{Environment.NewLine}{Environment.NewLine}");
        //new StringBuilder(contentHtml).Replace("<p>", string.Empty)
        CQ childrenCQ = documentCQ["body > table:eq(1) ul:eq(0)"];
        childrenCQ.Find("a").Each(dom =>
        {
            dom.RemoveAttribute("target");
            dom.TextContent = dom.TextContent.Replace('\u3000', ' ');
            string href = dom.GetAttribute("href");
            if (href.IsNullOrWhiteSpace())
            {
                return;
            }

            Match match = Regex.Match(href, @"tid=([0-9]+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                dom.SetAttribute("href", $"{match.Groups[1].Value}.htm");
                return;
            }

            match = Regex.Match(href, @"/([0-9]+)\.html$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                dom.SetAttribute("href", $"{match.Groups[1].Value}.htm");
                return;
            }

            log($"{threadId}: Link is removed {href}");
            dom.Remove();
        });
        string childrenInnerHtml = WebUtility.HtmlDecode(childrenCQ.Html());
        string templatedHtml1 = $"""
            <!DOCTYPE html>
            <html>
            <head>
            <title>{title}</title>
            <link rel="stylesheet" href="../styles/thread.css" />
            <script type="text/javascript" src="../scrips/thread.js"></script>
            </head>
            <body>
            {parentHtml}
            <h1>{header}</h1>
            <div id="thread">
            <p id="userName">{userName}</p>
            <p><a id="userId" href="{userLink}">{userId}</a></p>
            <p id="threadDateTime">{threadDateTime}</p>
            <p id="threadView">{threadView}</p>
            <p id="threadLastEditUserName">{lastEditUserName}</p>
            <p id="threadLastEditDateTime">{lastEditDateTime}</p>
            </div>
            <pre>
            """;

        string templatedHtml2 = $"""
            </pre>
            <ul id="children">
            {childrenInnerHtml}
            </ul>
            </body>
            </html>
            """;

        //html = WebUtility.HtmlDecode(html);
        string file = Path.Combine(directory, $"{threadId}.htm");
        await File.WriteAllLinesAsync(file, [templatedHtml1, contentHtml, templatedHtml2], cancellationToken);

        if (dispose)
        {
            httpClient?.Dispose();
        }
    }

    internal static void PrintThreadWthLargeParagraph(string directory, int startThreadId = 0, int endThreadId = int.MaxValue, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .EnumerateFiles(directory)
            .Select(file=>(file, int.Parse(PathHelper.GetFileNameWithoutExtension(file))))
            .Where(file => file.Item2 >= startThreadId && file.Item2 <= endThreadId)
            .Select(file =>
            {
                string text = File.ReadAllText(file.file);
                int startIndex = text.IndexOfOrdinal($"<pre>{Environment.NewLine}") + $"<pre>{Environment.NewLine}".Length;
                int endIndex = text.LastIndexOfOrdinal($"{Environment.NewLine}</pre>");
                ReadOnlySpan<char> contentSpan = text.AsSpan(startIndex..endIndex);
                MemoryExtensions.SpanSplitEnumerator<char> lineEnumerator = contentSpan.Split('\n');
                List<(Range, (int Offset, int Length))> lines = new();
                while (lineEnumerator.MoveNext())
                {
                    lines.Add((lineEnumerator.Current, lineEnumerator.Current.GetOffsetAndLength(contentSpan.Length)));
                }

                return (file.file, file.Item2, text, lines);
            })
            .Where(thread => thread.lines.Any(line => line.Item2.Length >= 1000)
                && thread.lines.Where(line => line.Item2.Length == 0).Take(10).Count() < 10)
            .ForEach(thread =>
            {
                Debugger.Break();
                log(thread.file);
            });
    }
}
