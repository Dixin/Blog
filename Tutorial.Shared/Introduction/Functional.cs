namespace Tutorial.Introduction
{
    using System;
    using System.IO;

    internal static partial class Functional
    {
        internal static string DownloadWebContent(string uri)
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
            return uri =>
                {
                    string html = download(uri);
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