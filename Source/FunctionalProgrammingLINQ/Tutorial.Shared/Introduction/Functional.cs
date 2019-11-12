namespace Tutorial.Introduction
{
    using System;
    using System.IO;

    internal static partial class Functional
    {
        internal static FileInfo DownloadHtml(Uri uri)
        {
            return default;
        }

        internal static FileInfo ConvertToWord(FileInfo htmlDocument, FileInfo template)
        {
            return default;
        }

        internal static void UploadToOneDrive(FileInfo file) { }

        internal static Action<Uri, FileInfo> CreateDocumentBuilder(
            Func<Uri, FileInfo> download, Func<FileInfo, FileInfo, FileInfo> convert, Action<FileInfo> upload)
        {
            return (uri, wordTemplate) =>
            {
                FileInfo htmlDocument = download(uri);
                FileInfo wordDocument = convert(htmlDocument, wordTemplate);
                upload(wordDocument);
            };
        }
    }

    internal static partial class Functional
    {
        internal static void BuildDocument(Uri uri, FileInfo template)
        {
            Action<Uri, FileInfo> buildDocument = CreateDocumentBuilder(
                DownloadHtml, ConvertToWord, UploadToOneDrive);
            buildDocument(uri, template);
        }
    }
}