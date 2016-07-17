namespace Dixin.Office.HtmlToWord
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using CsQuery;
    using CsQuery.ExtensionMethods;

    using Dixin.IO;

    using Microsoft.FSharp.Linq.RuntimeHelpers;
    using Microsoft.Office.Interop.Word;

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
            string outputDirectory = arguments.Any() && !string.IsNullOrWhiteSpace(arguments.First())
                ? arguments.First()
                : (PathHelper.TryGetOneDriveRoot(out outputDirectory)
                    ? Path.Combine(outputDirectory, @"Share\Book")
                    : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            Trace.WriteLine(Invariant($"Output directory {outputDirectory}."));
            Html html = DownloadHtml();
            SaveDocument(html, Path.Combine(outputDirectory, Invariant($"{html.Title}.doc")));
        }

        private static Html DownloadHtml(
            string indexUrl = @"http://weblogs.asp.net/dixin/linq-via-csharp", int downloadThreadPerProcessor = 10)
        {
            using (WebClient indexClient = new WebClient())
            {
                indexClient.Encoding = Encoding.UTF8;
                Trace.WriteLine(Invariant($"Downloading {indexUrl}."));
                CQ indexPage = indexClient.DownloadString(indexUrl);

                CQ article = indexPage["article.blog-post"];
                IEnumerable<IGrouping<string, Tuple<string, string>>> chapters = article
                    .Children("ol")
                    .Children("li")
                    .Select(chapter => chapter.Cq())
                    .AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(Environment.ProcessorCount * downloadThreadPerProcessor)
                    .Select(chapter =>
                        {
                            Tuple<string, string>[] sections = chapter.Find("h2")
                                .Select(section => section.Cq().Find("a:last"))
                                .AsParallel()
                                .AsOrdered()
                                .WithDegreeOfParallelism(Environment.ProcessorCount * downloadThreadPerProcessor)
                                .Select(section =>
                                {
                                    string sectionUrl = section.Attr<string>("href");
                                    Trace.WriteLine(Invariant($"Downloading {sectionUrl}."));
                                    using (WebClient sectionClient = new WebClient())
                                    {
                                        sectionClient.Encoding = Encoding.UTF8;
                                        CQ sectionPage = sectionClient.DownloadString(sectionUrl);

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
                                        sectionArticle.Find("pre span").Css("background", string.Empty);
                                        sectionArticle.Find("p")
                                            .Select(paragraph => paragraph.Cq())
                                            .ForEach(paragraph =>
                                                    {
                                                        string paragrapgText = paragraph.Text().Trim();
                                                        if ((paragraph.Children().Length == 0 &&
                                                             string.IsNullOrWhiteSpace(paragrapgText))
                                                            ||
                                                            paragrapgText.StartsWith(
                                                                "[LinQ via C#", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            paragraph.Remove();
                                                        }
                                                    });
                                        return Tuple.Create(section.Text().Trim(), sectionArticle.Html());
                                    }
                                })
                                .ToArray();
                            return new Grouping<string, Tuple<string, string>>(
                                chapter.Find("h1").Text().Trim(),
                                sections);
                        })
                    .ToArray();

                return new Html(
                    indexPage["title"].Text().Replace("Dixin's Blog -", string.Empty).Trim(),
                    chapters);
            }
        }

        private static void SaveDocument(Html html, string outputDocument)
        {
            string tempHtmlFile = Path.ChangeExtension(Path.GetTempFileName(), "htm");
            string htmlContent = html.TransformText();
            Trace.WriteLine(Invariant($"Saving HTML as {tempHtmlFile}, {htmlContent.Length}."));
            File.WriteAllText(tempHtmlFile, htmlContent);

            string template = Path.Combine(PathHelper.ExecutingDirectory(), "Book.dot");
            ConvertDocument(
                tempHtmlFile, WdOpenFormat.wdOpenFormatWebPages,
                outputDocument, WdSaveFormat.wdFormatDocument,
                document => FormatDocument(document, html, template));
        }

        private static void FormatDocument(Document document, Html html, string template, string author = "Dixin Yan", int downloadThreadPerProcessor = 10)
        {
            document.InlineShapes
                    .OfType<InlineShape>()
                    .Where(shape => shape.Type == WdInlineShapeType.wdInlineShapeLinkedPicture)
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount * downloadThreadPerProcessor)
                    .ForAll(picture =>
                        {
                            Trace.WriteLine(Invariant($"Downloading {picture.LinkFormat.SourceFullName}"));
                            picture.LinkFormat.SavePictureWithDocument = true;
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

            document.Sections.OfType<Section>().ForEach(section =>
                {
                    range = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    range.Fields.Add(range, WdFieldType.wdFieldStyleRef, @"""Heading 1""", true);

                    section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].PageNumbers.Add(
                        WdPageNumberAlignment.wdAlignPageNumberCenter);
                });
        }

        private static void ConvertDocument(
            string inputFile, WdOpenFormat inputFormat,
            string outputFile, WdSaveFormat outputFormat,
            Action<Document> format = null,
            bool isWordVisible = false)
        {
            Application word = null;
            try
            {
                word = new Application { Visible = isWordVisible };

                Trace.WriteLine(Invariant($"Opening {inputFile} as {inputFormat}."));
                word.Documents.Open(inputFile, Format: inputFormat);
                Document document = word.Documents[inputFile];

                format?.Invoke(document);

                Trace.WriteLine(Invariant($"Saving {outputFile} as {outputFormat}"));
                document.SaveAs2(outputFile, outputFormat);
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
