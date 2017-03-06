namespace Dixin.Office.HtmlToWord
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using CsQuery;
    using CsQuery.ExtensionMethods;

    using Dixin.IO;

    using Microsoft.FSharp.Linq.RuntimeHelpers;
    using Microsoft.Office.Interop.Word;

    using Task = System.Threading.Tasks.Task;

    using static System.FormattableString;

    internal partial class Html
    {
        internal Html(string title, IEnumerable<IGrouping<string, Tuple<string, string>>> chapters)
        {
            this.Title = title;
            this.Chapters = chapters;
        }

        internal string Title { get; }

        internal IEnumerable<IGrouping<string, Tuple<string, string>>> Chapters { get; }
    }

    internal static class Program
    {
        private static void Main(string[] arguments)
        {
            BuildDocumentsAsync(arguments.FirstOrDefault()).Wait();
        }

        private static async Task BuildDocumentsAsync(string outputDirectory, string oneDriveDirectory = @"Share\Book")
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                outputDirectory = PathHelper.TryGetOneDriveRoot(out outputDirectory)
                    ? Path.Combine(outputDirectory, oneDriveDirectory)
                    : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            Trace.WriteLine(Invariant($"Output directory {outputDirectory}."));
            Html html = await DownloadHtmlAsync();
            await SaveDocumentsAsync(html, Path.Combine(outputDirectory, Invariant($"{html.Title}.doc")), Path.Combine(outputDirectory, Invariant($"{html.Title}.pdf")));
        }

        private static async Task<Html> DownloadHtmlAsync(
            string indexUrl = @"http://weblogs.asp.net/dixin/linq-via-csharp")
        {
            using (WebClient indexClient = new WebClient())
            {
                indexClient.Encoding = Encoding.UTF8;
                Trace.WriteLine(Invariant($"Downloading {indexUrl}."));
                CQ indexPage = await indexClient.DownloadStringTaskAsync(indexUrl);

                CQ article = indexPage["article.blog-post"];
                IGrouping<string, Tuple<string, string>>[] chapters = await Task.WhenAll(article
                    .Children("ol")
                    .Children("li")
                    .Select(chapter => chapter.Cq())
                    .AsParallel()
                    .AsOrdered()
                    .Select(async chapter =>
                    {
                        Tuple<string, string>[] sections = await Task.WhenAll(chapter.Find("h2")
                            .Select(section => section.Cq().Find("a:last"))
                            .AsParallel()
                            .AsOrdered()
                            .Select(async section =>
                            {
                                string sectionUrl = section.Attr<string>("href");
                                Trace.WriteLine(Invariant($"Downloading {sectionUrl}."));
                                using (WebClient sectionClient = new WebClient())
                                {
                                    sectionClient.Encoding = Encoding.UTF8;
                                    CQ sectionPage = await sectionClient.DownloadStringTaskAsync(sectionUrl);

                                    CQ sectionArticle = sectionPage["article.blog-post"];
                                    sectionArticle.Children("header").Remove();
                                    Enumerable
                                        .Range(1, 7)
                                        .Reverse()
                                        .ForEach(i => sectionArticle
                                            .Find(Invariant($"h{i}")).Contents().Unwrap()
                                            .Wrap(Invariant($"<h{i + 2}/>"))
                                            .Parent()
                                            .Find("a").Contents().Unwrap());
                                    sectionArticle.Find("p")
                                        .Select(paragraph => paragraph.Cq())
                                        .ForEach(paragraph =>
                                        {
                                            string paragraphText = paragraph.Text().Trim();
                                            if ((paragraph.Children().Length == 0
                                                && string.IsNullOrWhiteSpace(paragraphText))
                                                || paragraphText.StartsWith("[LinQ via C#", StringComparison.OrdinalIgnoreCase))
                                            {
                                                paragraph.Remove();
                                            }
                                        });
                                    return Tuple.Create(section.Text().Trim(), sectionArticle.Html());
                                }
                            }));
                        return new Grouping<string, Tuple<string, string>>(
                            chapter.Find("h1").Text().Trim(),
                            sections);
                    }));

                return new Html(
                    indexPage["title"].Text().Replace("Dixin's Blog -", string.Empty).Trim(),
                    chapters);
            }
        }

        private static async Task SaveDocumentsAsync(Html html, string outputDocument, string exportDocument)
        {
            string tempHtmlFile = Path.ChangeExtension(Path.GetTempFileName(), "htm");
            string htmlContent = html.TransformText();
            Trace.WriteLine(Invariant($"Saving HTML as {tempHtmlFile}, {htmlContent.Length}."));
            using (StreamWriter writer = new StreamWriter(new FileStream(
                path: tempHtmlFile, mode: FileMode.Create, access: FileAccess.Write,
                share: FileShare.Read, bufferSize: 4096, useAsync: true)))
            {
                await writer.WriteAsync(htmlContent);
            }

            string template = Path.Combine(PathHelper.ExecutingDirectory(), "Book.dot");
            ConvertDocument(
                tempHtmlFile, WdOpenFormat.wdOpenFormatWebPages,
                outputDocument, WdSaveFormat.wdFormatDocument,
                exportDocument, WdExportFormat.wdExportFormatPDF,
                document => FormatDocument(document, html, template));
        }

        private static void FormatDocument(Document document, Html html, string template, string author = "Dixin Yan")
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
