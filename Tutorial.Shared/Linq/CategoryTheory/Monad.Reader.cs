namespace Dixin.Linq.CategoryTheory
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

    internal interface IConfiguration
    {
    }

    public static partial class ReaderExtensions
    {
        private static Reader<IConfiguration, Stream> DownloadHtmlContent(string uri) => 
            configuration => default(Stream); // Return download stram.

        private static Reader<IConfiguration, string> SaveHtmlToFile(Stream htmlStream) =>
            configuration => default(string); // Return saved HTML file path.

        private static Reader<IConfiguration, string> ConvertHtmlFileToWordFile(string htmlFilePath, string wordTemplatePath) =>
            configuration => default(string); // Return converted Word file path.

        private static Reader<IConfiguration, Unit> UploadFileToOneDrive(string localFilePath) =>
            configuration => default(Unit); // Return void.

        internal static void Workflow(IConfiguration configuration, string uri, string wordTemplatePath)
        {
            Reader<IConfiguration, string> query = 
                from htmlContent in DownloadHtmlContent(uri) // Reader<IConfiguration, Steam>.
                from htmlFile in SaveHtmlToFile(htmlContent) // Reader<IConfiguration, string>.
                from wordFile in ConvertHtmlFileToWordFile(htmlFile, wordTemplatePath) // Reader<IConfiguration, string>.
                from unit in UploadFileToOneDrive(wordFile) // Reader<IConfiguration, Unit>.
                select wordFile; // Define query.
            string result = query(configuration); // Execute query.
        }
    }
}
