namespace MediaManager.Net;


using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Net;
using MediaManager.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

internal static class Cool
{
    private const int ChunkLength = 100;

    internal static async Task DownloadAllPostsAsync(int startPostId, int endPostId, string directory, bool isDetection = false, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= Environment.ProcessorCount * 5;
        log ??= Logger.WriteLine;

        ConcurrentQueue<int> postIds = new(Enumerable.Range(startPostId, endPostId - startPostId).Chunk(ChunkLength).Select(chunk => chunk[0]));
        await Parallel.ForEachAsync(
            Enumerable.Range(0, degreeOfParallelism.Value),
            new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = degreeOfParallelism.Value },
             async (taskIndex, token) =>
            {
                using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                while (postIds.TryDequeue(out int postId))
                {
                    int lastPostId = postId + ChunkLength - 1;
                    for (; postId <= lastPostId; postId++)
                    {
                        await DownloadPostAsync(httpClient, postId, directory, isDetection, log, token);
                    }

                    log($"===Processed {lastPostId}===");
                }
            });
    }

    internal static async Task DownloadAllPostsWithTemplateAsync(int start, int end, string directory, bool overwrite = false, int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        IEnumerable<int> postIds = overwrite
            ? Enumerable
                .Range(start, end - start + 1)
            : Enumerable
                .Range(start, end - start + 1)
                .Except(Directory
                    .EnumerateFiles(directory)
                    .Select(file => int.Parse(PathHelper.GetFileNameWithoutExtension(file))));
        ConcurrentQueue<int> queue = new(postIds);
        maxDegreeOfParallelism ??= Environment.ProcessorCount;
        await Parallel.ForEachAsync(
            Enumerable.Range(0, maxDegreeOfParallelism.Value),
            new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = maxDegreeOfParallelism.Value },
            async (index, token) =>
            {
                using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
                while (queue.TryDequeue(out int postId))
                {
                    if (postId % 1000 == 0)
                    {
                        log($"Saved {postId}.");
                    }

                    await DownloadPostWithTemplateAsync(postId, directory, httpClient, log, token);
                }
            });
    }

    internal static async Task DownloadPostAsync(HttpClient httpClient, int postId, string directory, bool isDetection = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        if (isDetection)
        {
            HttpResponseMessage response = Retry.Incremental(
                () => httpClient.Send(new HttpRequestMessage(HttpMethod.Head, $"https://www.cool18.com/bbs4/index.php?app=forum&act=threadview&tid={postId}"), HttpCompletionOption.ResponseHeadersRead, cancellationToken),
                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound });
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
        }

        string pageHtml;
        try
        {
            pageHtml = await Retry.IncrementalAsync(
                async () => await httpClient.GetStringAsync($"https://www.cool18.com/bbs4/index.php?app=forum&act=threadview&tid={postId}", cancellationToken),
                isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound },
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            log($"Skip {postId}: 404");
            return;
        }

        if (pageHtml.StartsWithIgnoreCase("<script language='javascript'>window.location='index.php';</script>")
            || pageHtml.ContainsIgnoreCase("404 Not Found"))
        {
            log($"Skip {postId}: Redirection");
            return;
        }

        string childrenJson = await Retry.IncrementalAsync(
            async () => await httpClient.GetStringAsync($"https://www.cool18.com/bbs4/index.php?app=forum&act=achildlist&tid={postId}", cancellationToken),
            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.NotFound },
            cancellationToken: cancellationToken);

        await SavePost(postId, pageHtml, childrenJson, directory, log, cancellationToken);
    }

    private static async Task SavePost(int postId, string pageHtml, string childrenJson, string directory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        CQ pageCQ = CQ.CreateDocument(pageHtml);
        pageCQ.Children(":not(head, body)").Remove();

        CQ headCQ = pageCQ.Children("head");
        headCQ.Children(":not(title)").Remove();
        headCQ.Append("""<link rel="stylesheet" href="../style/post.css" />""");
        headCQ.Append("""<script type="text/javascript" src="../script/post.js"></script>""");

        CQ bodyCQ = pageCQ.Children("body");
        bodyCQ.Each(bodyDom => bodyDom.Attributes.ToArray().ForEach(attribute => bodyDom.RemoveAttribute(attribute.Key)));

        CQ mainCQ = bodyCQ.Children(".main-content").Siblings().Remove().End();
        mainCQ.Children(":not(.title-section, .post-content, .post-list)").Remove();

        CQ titleCQ = mainCQ.Children(".title-section").Find(".tool-btn, .fa-envelope, .title-usergrade, .followbtn").Remove().End();
        CQ senderCQ = titleCQ.Find(".sender");
        string sender = senderCQ.Text();
        string name = sender[(sender.IndexOf(':') + 1)..sender.IndexOf('[')].Trim();
        string dateTime = sender[(sender.IndexOf('于') + 1)..].Trim();
        senderCQ.Html($"""<a class="title-link">{name}</a> {dateTime}</span>""");

        CQ postCQ = mainCQ.Children(".post-content");
        postCQ.Children(".content-section").Siblings().Remove().End().Children(".view_ad_incontent").Remove();
        //postCQ.Find("center:contains('最后编辑于')").NextAll(":not(.post-list)").Remove();

        CQ repliesCQ = mainCQ.Find(".post-list").Children(".list-header").Remove().End();
        if (repliesCQ.Length == 0)
        {
            mainCQ.Append("""<div class="post-list"><ul class="post-items thread-list"></ul></div>""");
            repliesCQ = mainCQ.Find(".post-list");
            Debug.Assert(repliesCQ.Length == 1);
        }

        bodyCQ.Find("iframe, link, script, .view_ad_incontent, .vote-section, .view-gift, .view_tools_box, .view_ad_bottom, .comment-section, .bottom-nav").Remove();

        bodyCQ.Find("a")
            .RemoveAttr("title").RemoveAttr("target")
            .Each(linkDom =>
            {
                string href = linkDom.GetAttribute("href");
                if (href.IsNullOrWhiteSpace())
                {
                    return;
                }

                Match match = PostIdRegex.Match(href);
                if (match.Success)
                {
                    int referencePostId = int.Parse(match.Groups[1].Value);
                    linkDom.SetAttribute("href", $"../{GetPostDirectoryName(referencePostId)}/{GetPostFileName(referencePostId)}");
                }
            });

        CQ repliesContentCQ = repliesCQ.Children("ul");
        Debug.Assert(repliesContentCQ.Length == 1);
        repliesContentCQ.Text(childrenJson);

        string html = pageCQ.Render(DomRenderingOptions.RemoveComments | DomRenderingOptions.QuoteAllAttributes);
        html = WebUtility.HtmlDecode(html);
        string file = Path.Combine(directory, GetPostDirectoryName(postId), GetPostFileName(postId));
        await File.WriteAllTextAsync(file, html, cancellationToken);
        log($"Save {postId}: {file}");
    }

    private static string GetPostDirectoryName(int postId) => $"{postId / 100_000:00000}";

    private static string GetPostFileName(int postId) => $"{postId}.htm";

    private static readonly Regex PostIdRegex = new(@"^index\.php\?app=forum&act=threadview&tid=([0-9]+)$", RegexOptions.IgnoreCase);

    internal static async Task DownloadPostWithTemplateAsync(int postId, string directory, HttpClient? httpClient = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        bool dispose = httpClient is null;
        httpClient ??= new HttpClient().AddEdgeHeaders();

        await using Stream stream = await Retry.FixedIntervalAsync(
            async () => await httpClient.GetStreamAsync($"https://www.cool18.com/bbs4/index.php?app=forum&act=threadview&tid={postId}", cancellationToken),
            cancellationToken: cancellationToken);
        //if (pageHtml.EqualsIgnoreCase("\r\n<script language='javascript'>window.location='index.php';</script>"))
        //{
        //    log($"Skip {postId}");
        //    return;
        //}

        CQ documentCQ = CQ.CreateDocument(stream);

        if (documentCQ["head script"].Text().EqualsIgnoreCase("window.location='index.php';") && documentCQ["body"].Children().IsEmpty())
        {
            log($"Skip {postId}");
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
        string postText = userTableCQ.Find("td:eq(0)").Text().Trim();
        string postDateTime = Regex.Match(postText, @"[0-9]+\-[0-9]+\-[0-9]+ [0-9]+\:[0-9]+").Value;
        string postView = Regex.Match(postText, @"已读 ([0-9]+) 次").Groups[1].Value;
        CQ parentCQ = userTableCQ.Next("p");
        string parentHtml = string.Empty;
        if (parentCQ.Any())
        {
            CQ parentLinkCQ = parentCQ.Find("a");
            string href = parentLinkCQ.Attr("href");
            string parentPostId = Regex.Match(href, @"tid=([0-9]+)$", RegexOptions.IgnoreCase).Groups[1].Value;
            string parentPostTitle = parentLinkCQ.Text().Trim();
            string text = parentLinkCQ.Elements.Single().NextSibling.NodeValue.Trim();
            Match match = Regex.Match(text, @"由 (.+) 于 ([0-9]+\-[0-9]+\-[0-9]+ [0-9]+\:[0-9]+)$");
            string parentPostUserName = match.Groups[1].Value;
            string parentPostDateTime = match.Groups[2].Value;
            parentHtml = $"""
                <div id ="parent">
                <p><a id="parentPost" href="{parentPostId}.htm">{parentPostTitle}</a></p>
                <p id="parentPostUserName">{parentPostUserName}</p>
                <p id="parentPostDateTime">{parentPostDateTime}</p>
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

            log($"{postId}: Link is removed {href}");
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
            log($"{postId}: Removed {dom.NodeName}:{dom.TextContent.Trim()}");
            dom.Remove();
        });

        preCQ
            .Find("font")
            .Where(dom => !(dom.TryGetAttribute("color", out string color) && color.EndsWithIgnoreCase("E6E6DD")))
            .ForEach(dom => dom.Cq().ReplaceWith(dom.ChildNodes));

        CQ preNonLinkChildren = preCQ.Find(":not(a)");
        if (preNonLinkChildren.Length is > 0 and < 3 && preNonLinkChildren.All(dom => dom.OuterHTML.EqualsIgnoreCase("<p></p>")))
        {
            preNonLinkChildren.Each(dom => dom.Cq().ReplaceWith("\n\n"));
        }

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
            //log($"{postId} Children {string.Join("|", groups.Select(group => $"{group.Key}*{group.Count()}"))}");

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
                    if (preChildrenCQ.All(dom => dom.NodeName.EqualsIgnoreCase("span")))
                    {
                        preChildrenCQ.Each(dom => dom.Cq().ReplaceWith($"\n\n{dom.TextContent}\n\n"));
                    }
                    else
                    {
                        preChildrenCQ.Each(dom => dom.Cq().ReplaceWith(dom.ChildNodes));
                    }

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

                List<(Range Range, int Offset, int Length)> lines = contentHtml.AsSpan().Split('\n').ToList(contentHtml.Length);

                while (true)
                {
                    (Range Range, int Offset, int Length)[] linesWithLeadingWhiteSpace = lines.Where(line => line.Length > 0 && contentHtml[line.Offset] == ' ').ToArray();
                    if (linesWithLeadingWhiteSpace.Length > 0 && linesWithLeadingWhiteSpace.Length >= lines.Count * 9 / 10)
                    {
                        contentHtml = contentHtml.Replace("\n ", "\n");
                        lines = contentHtml.AsSpan().Split('\n').ToList(contentHtml.Length);
                    }
                    else
                    {
                        break;
                    }
                }

                if (lines.Any(line => line.Length >= 500))
                {
                    if (contentHtml.ContainsOrdinal("\u3000\u3000"))
                    {
                        contentHtml = Regex.Replace(contentHtml, @"[\u3000]{2,}", "\n\n\u3000\u3000");
                        lines = contentHtml.AsSpan().Split('\n').ToList(contentHtml.Length);
                    }
                    else
                    {
                        log($"{postId}: large paragraph is not processed.");
                        //Debugger.Break();
                    }
                }

                int[] nonEmptyLinesWithoutLink = lines
                    .Where(line =>
                    {
                        if (line.Length == 0)
                        {
                            return false;
                        }

                        ReadOnlySpan<char> lineSpan = contentHtml.AsSpan(line.Range);
                        return !lineSpan.Contains("<a", StringComparison.OrdinalIgnoreCase) && !Regex.IsMatch(lineSpan, @"[\p{P}\u3000 ]{10,}");
                    })
                    .Select(line => line.Length)
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
                            .Count(line => line.Length > 2 && contentHtml[line.Offset] == '\u3000' && contentHtml[line.Offset + 1] == '\u3000');
                        if (leadingWhiteSpaceCount >= 10 || leadingWhiteSpaceCount >= trimmedNonEmptyLinesWithoutLink.Length / 5)
                        {
                            contentHtml = Regex.Replace(contentHtml, @"([^\n^>])[\n]{1,2}([^\n^<])", "$1$2");
                            contentHtml = Regex.Replace(contentHtml, @"[\u3000]{2,}", "\n\n\u3000\u3000");
                        }
                        else if (lines.Skip(1).SkipLast(1).Any(line => line.Length == 0))
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

            log($"{postId}: Link is removed {href}");
            dom.Remove();
        });
        string childrenInnerHtml = WebUtility.HtmlDecode(childrenCQ.Html());
        string templatedHtml1 = $"""
            <!DOCTYPE html>
            <html>
            <head>
            <title>{title}</title>
            <link rel="stylesheet" href="../styles/post.css" />
            <script type="text/javascript" src="../scrips/post.js"></script>
            </head>
            <body>
            {parentHtml}
            <h1>{header}</h1>
            <div id="post">
            <p id="userName">{userName}</p>
            <p><a id="userId" href="{userLink}">{userId}</a></p>
            <p id="postDateTime">{postDateTime}</p>
            <p id="postView">{postView}</p>
            <p id="postLastEditUserName">{lastEditUserName}</p>
            <p id="postLastEditDateTime">{lastEditDateTime}</p>
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
        string file = Path.Combine(directory, $"{postId}.htm");
        await File.WriteAllLinesAsync(file, [templatedHtml1, contentHtml, templatedHtml2], cancellationToken);

        if (dispose)
        {
            httpClient?.Dispose();
        }
    }

    internal static void PrintPostWthLargeParagraph(string directory, int startPostId = 0, int endPostId = int.MaxValue, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .EnumerateFiles(directory)
            .Select(file => (file, int.Parse(PathHelper.GetFileNameWithoutExtension(file))))
            .Where(file => file.Item2 >= startPostId && file.Item2 <= endPostId)
            .Select(file =>
            {
                string text = File.ReadAllText(file.file);
                int startIndex = text.IndexOfOrdinal($"<pre>{Environment.NewLine}") + $"<pre>{Environment.NewLine}".Length;
                int endIndex = text.LastIndexOfOrdinal($"{Environment.NewLine}</pre>");
                ReadOnlySpan<char> contentSpan = text.AsSpan(startIndex..endIndex);
                List<(Range Range, int Offset, int Length)> lines = contentSpan.Split('\n').ToList(text.Length);
                return (file.file, file.Item2, text, lines);
            })
            .Where(post => post.lines.Any(line => line.Length >= 1000)
                && post.lines.Where(line => line.Length == 0).Take(10).Count() < 10)
            .ForEach(post =>
            {
                Debugger.Break();
                log(post.file);
            });
    }
}
