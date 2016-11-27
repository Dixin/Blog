namespace Dixin.Linq.CategoryTheory
{
    using System;

    // Reader<TContext, T> is alias of Func<TContext, T>
    public delegate T Reader<in TContext, out T>(TContext context);

    public static partial class ReaderExtensions
    {
        public static Reader<TContext, TResult> SelectMany<TContext, TSource, TSelector, TResult>(
            this Reader<TContext, TSource> source,
            Func<TSource, Reader<TContext, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                context =>
                {
                    TSource value = source(context);
                    return resultSelector(value, selector(value)(context));
                };

        // Wrap: T -> Reader<TContext, T>
        public static Reader<TContext, TSource> Reader<TContext, TSource>(this TSource value) => context => value;
    }

    public static partial class ReaderExtensions
    {
        public static Reader<TContext, TContext> Context<TContext>() => Functions.Id;

        // Select: (TSource -> TResult) -> (Reader<TContext, TSource> -> Reader<TContext, TResult>)
        public static Reader<TContext, TResult> Select<TContext, TSource, TResult>(
            this Reader<TContext, TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Reader<TContext, TResult>(), Functions.False);
    }

    internal static partial class ReaderQuery
    {
        internal static void HtmlToWord()
        {
            var query = // Reader<Settings, (string, string)>
                from htmlContent in new Reader<Settings, string>(settings => DownloadHtmlContent(settings.BlogUrl))
                from htmlFilePath in new Reader<Settings, string>(settings => SaveHtmlToFile(
                    settings.TempPath, htmlContent))
                from wordFilePath in new Reader<Settings, string>(settings => ConvertHtmlFileToWordFile(
                    settings.WordTemplatePath, htmlFilePath))
                from wordFileUrl in new Reader<Settings, string>(settings => UploadFileToOneDrive(
                    wordFilePath, settings.OneDrivePath))
                select new { Local = wordFilePath, Remote = wordFileUrl }; // Define query.
            var result = // (string, string)
                query(Settings.Default); // Execute query.
        }

        private static string DownloadHtmlContent(string url) => default(string);

        private static string SaveHtmlToFile(string connectionString, string html) => default(string);

        private static string ConvertHtmlFileToWordFile(string wordTemplatePath, string htmlFilePath) => default(string);

        private static string UploadFileToOneDrive(string filePath, string oneDrivePath) => default(string);
    }

    //public interface IRepository
}
