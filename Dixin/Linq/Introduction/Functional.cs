namespace Dixin.Linq.Introduction
{
    using System;
    using System.IO;

    internal static partial class Functional
    {
        internal static string DownloadWebContent(string url)
        {
            throw new NotImplementedException();
        }

        internal static FileInfo ConvertToWord(string html)
        {
            throw new NotImplementedException();
        }

        internal static void UploadToOneDrive(FileInfo file)
        {
            throw new NotImplementedException();
        }

        internal static Action<string> GetDocumentBuilder(
            Func<string, string> download, Func<string, FileInfo> convert, Action<FileInfo> upload)
        {
            return url =>
                {
                    string html = download(url);
                    FileInfo word = convert(html);
                    upload(word);
                };
        }
    }

    internal static partial class Functional
    {
        internal static void BuildDocument()
        {
            Action<string> buildDocument = GetDocumentBuilder(
                DownloadWebContent, ConvertToWord, UploadToOneDrive);
            buildDocument("https://weblogs.asp.net/dixin/linq-via-csharp");
        }
    }
}