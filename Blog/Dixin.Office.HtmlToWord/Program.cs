namespace Dixin.Office.HtmlToWord
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using CsQuery;
    using CsQuery.ExtensionMethods;

    using Dixin.IO;

    using Microsoft.Office.Interop.Word;

    using Task = System.Threading.Tasks.Task;

    using static System.FormattableString;

    internal partial class AllHtml
    {
        internal AllHtml(string title, IEnumerable<(string, List<(string Title, CQ Content)>)> chapters)
        {
            this.Title = title;
            this.Chapters = chapters;
        }

        internal string Title { get; }

        internal IEnumerable<(string Title, List<(string Title, CQ Content)> Sections)> Chapters { get; }
    }

    internal partial class ChapterHtml
    {
        internal ChapterHtml(string title, IEnumerable<(string Title, CQ Content)> sections)
        {
            this.Title = title;
            this.Sections = sections;
        }

        internal string Title { get; }

        public IEnumerable<(string Title, CQ Content)> Sections { get; }
    }

    internal partial class SectionHtml
    {
        internal SectionHtml(string title, CQ content)
        {
            this.Title = title;
            this.Content = content;
        }

        internal string Title { get; }

        public CQ Content { get; }
    }

    internal static class Program
    {
        private static async Task Main(string[] arguments) =>
            await BuildDocumentsAsync(arguments.FirstOrDefault());

        private static async Task BuildDocumentsAsync(string outputDirectory, string oneDriveDirectory = @"Works\Book\Apress")
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                outputDirectory = PathHelper.TryGetOneDriveRoot(out outputDirectory)
                    ? Path.Combine(outputDirectory, oneDriveDirectory)
                    : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            string htmlOutputDirectory = Path.Combine(outputDirectory, "Html");
            Trace.WriteLine(Invariant($"HTML output directory {outputDirectory}."));
            AllHtml html = await DownloadHtmlAsync();
            await html.Chapters.ForEachAsync(async (chapter, chapterIndex) =>
            {
                ChapterHtml partHtml = new ChapterHtml(chapter.Title, chapter.Sections);
                await SaveAsync(partHtml.TransformText(), Path.Combine(htmlOutputDirectory, $"{chapterIndex + 1}. {chapter.Title.Replace("/", "-").Replace(":", " -")}.html"));
                await chapter.Sections
                    .Select(section => new SectionHtml(section.Title, section.Content))
                    .ForEachAsync(async (section, sectionIndex) =>
                    {
                        // Format and download img.
                        await section.Content.Find("img")
                            .ForEachAsync(async image =>
                            {
                                string uri = image.GetAttribute("src");
                                string localPath = Path.Combine("images", string.Join("-", uri.Split('/').Reverse().Take(2).Reverse()));
                                image.SetAttribute("src", localPath);
                                using (WebClient webClient = new WebClient())
                                {
                                    Trace.WriteLine($"Downloading image {uri} to {localPath}.");
                                    await webClient.DownloadFileTaskAsync(uri, Path.Combine(htmlOutputDirectory, localPath));
                                }
                            });
                        await SaveAsync(
                            section.TransformText(),
                            Path.Combine(htmlOutputDirectory, $"{chapterIndex + 1}.{sectionIndex + 1}. {section.Title.Replace("/", "-").Replace(":", " -")}.html"));
                    });
            });
            await SaveDocumentsAsync(htmlOutputDirectory, html, Path.Combine(outputDirectory, Invariant($"{html.Title}.doc")), Path.Combine(outputDirectory, Invariant($"{html.Title}.pdf")));
        }

        private static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action) => await Task.WhenAll(source.Select(action));

        private static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> action) => await Task.WhenAll(source.Select(action));

        private static async Task SaveAsync(string text, string htmlFile)
        {
            Trace.WriteLine(Invariant($"Saving HTML as {htmlFile}, {text.Length}."));
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(
                    path: htmlFile, mode: FileMode.Create, access: FileAccess.Write,
                    share: FileShare.Read, bufferSize: 4096, useAsync: true)))
                {
                    await writer.WriteAsync(text);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(htmlFile);
                Trace.WriteLine(exception);
                throw;
            }
        }

        private static async Task<AllHtml> DownloadHtmlAsync(
            string indexUrl = @"http://weblogs.asp.net/dixin/linq-via-csharp")
        {
            using (WebClient indexClient = new WebClient())
            {
                indexClient.Encoding = Encoding.UTF8;
                Trace.WriteLine(Invariant($"Downloading {indexUrl}."));
                CQ indexPageCq = await indexClient.DownloadStringTaskAsync(indexUrl);

                CQ indexContentCq = indexPageCq["article.blog-post"];
                (string Title, List<(string Title, CQ Content)> Sections)[] chapters = await Task.WhenAll(indexContentCq
                    .Children("ol")
                    .Children("li")
                    .Select(part => part.Cq())
                    .AsParallel()
                    .AsOrdered()
                    .Select(async categoryCq =>
                    {
                        (string Title, CQ Content)[] articles = await Task.WhenAll(categoryCq.Find("h2")
                            .Select(articleLink => articleLink.Cq().Find("a:last"))
                            .AsParallel()
                            .AsOrdered()
                            .Select(async articleLinkCq =>
                            {
                                string articleUri = articleLinkCq.Attr<string>("href");
                                string articleTitle = articleLinkCq.Text().Trim();

                                Trace.WriteLine(Invariant($"Downloading [{articleTitle}] {articleUri}."));
                                using (WebClient articleClient = new WebClient())
                                {
                                    articleClient.Encoding = Encoding.UTF8;
                                    CQ articleCq;
                                    try
                                    {
                                        articleCq = await articleClient.DownloadStringTaskAsync(articleUri);
                                    }
                                    catch (Exception exception)
                                    {
                                        Trace.WriteLine($"Failed to download {articleUri}");
                                        Trace.WriteLine(exception);
                                        throw;
                                    }
                                    CQ articleContentCq = articleCq["article.blog-post"];
                                    articleContentCq.Children("header").Remove();

                                    return (Title: articleTitle, Content: FormatArticleContent(articleTitle, articleContentCq));
                                }
                            }));
                        return (categoryCq.Find("h1").Text().Trim(), articles.ToList());
                    }));

                return new AllHtml(
                    indexPageCq["title"].Text().Replace("Dixin's Blog -", string.Empty).Trim(),
                    chapters);
            }
        }

        private static readonly Regex allowedTag = new Regex("^(p|h[1-9]|pre|blockquote|table|img|ul|ol)$", RegexOptions.IgnoreCase);

        private static readonly Regex allowedParagraphTag = new Regex("^(a|sub|sup|img)$", RegexOptions.IgnoreCase);

        private static readonly Regex allowedSpanParentTag = new Regex("^(pre|span)$", RegexOptions.IgnoreCase);

        private static CQ FormatArticleContent(string articleTitle, CQ articleContentCq)
        {
            // Format h1 - h7 to h3 - h9.
            Enumerable
                .Range(1, 7)
                .Reverse()
                .ForEach(i => articleContentCq
                    .Find(Invariant($"h{i}")).Contents().Unwrap()
                    .Wrap(Invariant($"<h{i + 2}/>"))
                    .Parent()
                    .Find("a").Contents().Unwrap());
            // Format p.
            articleContentCq.Find("p")
                .Select(paragraph => paragraph.Cq())
                .ForEach(paragraphCq =>
                {
                    string paragraphText = paragraphCq.Text().Trim();
                    paragraphCq.Children()
                        .Where(child => !allowedParagraphTag.IsMatch(child.NodeName))
                        .Select(child => child.Cq())
                        .ForEach(childCq =>
                        {
                            Trace.WriteLine($"[{articleTitle}] [Paragraph has child elements {childCq[0].NodeName}]: {paragraphCq.Html()}");
                            childCq.ReplaceWith(childCq.Text());
                        });

                    CQ imageCq = paragraphCq.Find("img");
                    if (imageCq.Any())
                    {
                        paragraphCq.Children().ReplaceWith(imageCq);
                    }
                    else
                    {
                        paragraphCq.Text(paragraphText);
                        if (string.IsNullOrWhiteSpace(paragraphText)
                            || paragraphText.StartsWith("[LinQ via C#", StringComparison.OrdinalIgnoreCase))
                        {
                            paragraphCq.Remove();
                        }
                    }
                });
            articleContentCq.Find("img").RemoveAttr("style").RemoveAttr("width").RemoveAttr("height").RemoveAttr("border").RemoveAttr("class");
            articleContentCq.Find("table").RemoveAttr("style").RemoveAttr("width").RemoveAttr("height").RemoveAttr("border").RemoveAttr("cellspacing").RemoveAttr("cellpadding").RemoveAttr("class");
            articleContentCq.Find("tr").RemoveProp("class");
            articleContentCq.Find("td").RemoveAttr("style").RemoveAttr("width").RemoveAttr("height").RemoveAttr("border").RemoveAttr("valign").RemoveAttr("class");
            articleContentCq.Find("pre").RemoveClass("code").AddClass("csharp");
            // Cleanup direct child elements.
            articleContentCq.Children()
                .Where(child => !allowedTag.IsMatch(child.NodeName))
                .ForEach(child =>
                {
                    Trace.WriteLine($"[{articleTitle}] [{child.NodeName} is not allowed]: {child.OuterHTML}");
                    child.Cq().Remove();
                });
            // Cleanup span.
            articleContentCq.Find("span")
                .Select(span => (Span: span, Parent: span.ParentNode))
                .Where(spanAndParent => !allowedSpanParentTag.IsMatch(spanAndParent.Parent.NodeName))
                .ForEach(spanAndParent =>
                {
                    Trace.WriteLine($"[{articleTitle}] [{spanAndParent.Span.NodeName} is not allowed]: {spanAndParent.Parent.OuterHTML}");
                    spanAndParent.Span.Cq().ReplaceWith(spanAndParent.Span.InnerText);
                });
            // Cleanup strong b u i
            articleContentCq.Find("strong, b, u, i").ForEach(element => element.Cq().ReplaceWith(element.InnerText));

            return articleContentCq;
        }

        private static async Task SaveDocumentsAsync(string directory, AllHtml html, string outputDocument, string exportDocument)
        {
            string tempHtmlFile = Path.Combine(directory, "All.htm");
            string htmlContent = html.TransformText();
            Trace.WriteLine(Invariant($"Saving HTML as {tempHtmlFile}, {htmlContent.Length}."));
            using (StreamWriter writer = new StreamWriter(new FileStream(
                path: tempHtmlFile, mode: FileMode.Create, access: FileAccess.Write,
                share: FileShare.Read, bufferSize: 4096, useAsync: true)))
            {
                await writer.WriteAsync(htmlContent);
            }

            string template = Path.Combine(PathHelper.ExecutingDirectory(), "Word", "Book.dot");
            ConvertDocument(
                tempHtmlFile, WdOpenFormat.wdOpenFormatWebPages,
                outputDocument, WdSaveFormat.wdFormatDocument,
                exportDocument, WdExportFormat.wdExportFormatPDF,
                document => FormatDocument(document, html, template));
        }

        private static void FormatDocument(Document document, AllHtml html, string template, string author = "Dixin Yan")
        {
            document.InlineShapes
                .Cast<InlineShape>()
                .Where(shape => shape.Type == WdInlineShapeType.wdInlineShapeLinkedPicture)
                .AsParallel()
                .ForAll(picture =>
                {
                    Trace.WriteLine(Invariant($"Downloading {picture.LinkFormat.SourceFullName}"));
                    picture.LinkFormat.SavePictureWithDocument = true;
                    if (picture.Width > 470)
                    {
                        picture.Width = 470;
                    }
                });

            Trace.WriteLine(Invariant($"Applying template {template}"));
            document.set_AttachedTemplate(template);
            document.UpdateStyles();

            Range range = document.Range(document.Content.Start, document.Content.Start);

            document.TablesOfContents.Add(range);

            TableOfContents table = document.TablesOfContents.Add(range, LowerHeadingLevel: 1);

            Trace.WriteLine(Invariant($"Adding title {html.Title}"));
            Paragraph titleParagraph = document.Paragraphs.Add(range);
            titleParagraph.Range.Text = Invariant($"{html.Title}{Environment.NewLine}");
            range.set_Style("Title");

            Trace.WriteLine(Invariant($"Adding author {author}"));
            range = document.Range(table.Range.Start, table.Range.Start);
            Paragraph authorParagraph = document.Paragraphs.Add(range);
            authorParagraph.Range.Text = Invariant($"{author}{Environment.NewLine}");
            range.set_Style("Author");

            range = document.Range(table.Range.End, table.Range.End);
            range.InsertBreak(WdBreakType.wdPageBreak);

            document.Sections.Cast<Section>().ForEach(section =>
            {
                range = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                range.Fields.Add(range, WdFieldType.wdFieldStyleRef, @"""Heading 1""", true);

                section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].PageNumbers.Add(
                    WdPageNumberAlignment.wdAlignPageNumberCenter);
            });
        }

        private static void ConvertDocument(
            string openFile, WdOpenFormat openFormat,
            string saveFile, WdSaveFormat saveFormat,
            string exportFile, WdExportFormat exportFormat,
            Action<Document> format = null,
            bool isWordVisible = false)
        {
            Application word = null;
            try
            {
                word = new Application { Visible = isWordVisible };

                Trace.WriteLine(Invariant($"Opening {openFile} as {openFormat}."));
                word.Documents.Open(openFile, Format: openFormat);
                Document document = word.Documents[openFile];

                format?.Invoke(document);

                Trace.WriteLine(Invariant($"Saving {saveFile} as {saveFormat}"));
                document.SaveAs2(saveFile, saveFormat);
                Trace.WriteLine(Invariant($"Exporting {exportFile} as {exportFormat}"));
                document.ExportAsFixedFormat(exportFile, exportFormat, CreateBookmarks: WdExportCreateBookmarks.wdExportCreateHeadingBookmarks, OptimizeFor: WdExportOptimizeFor.wdExportOptimizeForOnScreen);
            }
            finally
            {
                word?.Documents?.Close();
                word?.Quit();
            }
        }

#if DEMO
        private static byte[] HtmlToWord(string html, string fileName)
        {
            using (MemoryStream memoryStream = new MemoryStream())

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(
                memoryStream, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.MainDocumentPart;
                if (mainPart == null)
                {
                    mainPart = wordDocument.AddMainDocumentPart();
                    new Document(new Body()).Save(mainPart);
                }

                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.ImageProcessing = ImageProcessing.AutomaticDownload;
                Body body = mainPart.Document.Body;

                IList<OpenXmlCompositeElement> paragraphs = converter.Parse(html);
                body.Append(paragraphs);

                mainPart.Document.Save();
                return memoryStream.ToArray();
            }
        }
#endif
    }
}
