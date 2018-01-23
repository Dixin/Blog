namespace Tutorial.CategoryTheory
{
    using System;
    using System.IO;

    using Microsoft.FSharp.Core;

    // Reader: TEnvironment -> T
    public delegate T Reader<in TEnvironment, out T>(TEnvironment environment);

    public static partial class ReaderExtensions
    {
        // SelectMany: (Reader<TEnvironment, TSource>, TSource -> Reader<TEnvironment, TSelector>, (TSource, TSelector) -> TResult) -> Reader<TEnvironment, TResult>
        public static Reader<TEnvironment, TResult> SelectMany<TEnvironment, TSource, TSelector, TResult>(
            this Reader<TEnvironment, TSource> source,
            Func<TSource, Reader<TEnvironment, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                environment =>
                {
                    TSource value = source(environment);
                    return resultSelector(value, selector(value)(environment));
                };

        // Wrap: TSource -> Reader<TEnvironment, TSource>
        public static Reader<TEnvironment, TSource> Reader<TEnvironment, TSource>(this TSource value) =>
            environment => value;

        // Select: (Reader<TEnvironment, TSource>, TSource -> TResult) -> Reader<TEnvironment, TResult>
        public static Reader<TEnvironment, TResult> Select<TEnvironment, TSource, TResult>(
            this Reader<TEnvironment, TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Reader<TEnvironment, TResult>(), (value, result) => result);
    }

    internal interface IConfiguration { }

    public static partial class ReaderExtensions
    {
        private static Reader<IConfiguration, FileInfo> DownloadHtml(Uri uri) =>
            configuration => default;

        private static Reader<IConfiguration, FileInfo> ConverToWord(FileInfo htmlDocument, FileInfo template) =>
            configuration => default;

        private static Reader<IConfiguration, Unit> UploadToOneDrive(FileInfo file) =>
            configuration => default;

        internal static void Workflow(IConfiguration configuration, Uri uri, FileInfo template)
        {
            Reader<IConfiguration, (FileInfo, FileInfo)> query =
                from htmlDocument in DownloadHtml(uri) // Reader<IConfiguration, FileInfo>.
                from wordDocument in ConverToWord(htmlDocument, template) // Reader<IConfiguration, FileInfo>.
                from unit in UploadToOneDrive(wordDocument) // Reader<IConfiguration, Unit>.
                select (htmlDocument, wordDocument); // Define query.
            (FileInfo, FileInfo) result = query(configuration); // Execute query.
        }
    }
}
