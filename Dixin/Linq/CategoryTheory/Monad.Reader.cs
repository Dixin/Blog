namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;

    using Microsoft.FSharp.Core;

    // Reader<TEnvironment, T> is alias of Func<TEnvironment, T>
    public delegate T Reader<in TEnvironment, out T>(TEnvironment environment);

    [Pure]
    public static partial class ReaderExtensions
    {
        // Required by LINQ.
        public static Reader<TEnvironment, TResult> SelectMany<TEnvironment, TSource, TSelector, TResult>
            (this Reader<TEnvironment, TSource> source,
                Func<TSource, Reader<TEnvironment, TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector) =>
                environment =>
                    {
                        TSource sourceResult = source(environment);
                        return resultSelector(sourceResult, selector(sourceResult)(environment));
                    };

        // Not required, just for convenience.
        public static Reader<TEnvironment, TResult> SelectMany<TEnvironment, TSource, TResult>
            (this Reader<TEnvironment, TSource> source,
                Func<TSource, Reader<TEnvironment, TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class ReaderExtensions
    {
        // μ: Reader<TEnvironment, Reader<TEnvironment, T>> => Reader<TEnvironment, T>
        public static Reader<TEnvironment, TResult> Flatten<TEnvironment, TResult>
            (Reader<TEnvironment, Reader<TEnvironment, TResult>> source) => source.SelectMany(Functions.Id);

        // η: T -> Reader<TEnvironment, T>
        public static Reader<TEnvironment, T> Reader<TEnvironment, T>
            (this T value) => environment => value;

        // φ: Lazy<Reader<TEnvironment, T1>, Reader<TEnvironment, T2>> => Reader<TEnvironment, Defer<T1, T2>>
        public static Reader<TEnvironment, Lazy<T1, T2>> Binary<TEnvironment, T1, T2>
            (this Lazy<Reader<TEnvironment, T1>, Reader<TEnvironment, T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Reader<TEnvironment, TUnit>
        public static Reader<TEnvironment, Unit> Unit<TEnvironment>
            (Unit unit) => unit.Reader<TEnvironment, Unit>();

        // Select: (TSource -> TResult) -> (Reader<TEnvironment, TSource> -> Reader<TEnvironment, TResult>)
        public static Reader<TEnvironment, TResult> Select<TEnvironment, TSource, TResult>
            (this Reader<TEnvironment, TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Reader<TEnvironment, TResult>());
    }

    // Impure.
    internal static partial class ReaderQuery
    {
        internal static void ProcessSettings()
        {
            Reader<Settings, string> query =
                // 1. Use settings.
                from html in new Reader<Settings, string>(settings => DownloadString(settings.BlogUrl))
                // 2. Use settings.
                from _ in new Reader<Settings, Unit>(settings => SaveToDatabase(settings.ConnectionString, html))
                // 3. Update settings.
                from __ in new Reader<Settings, Settings>(settings => UpdateSettings(settings))
                // 4. Use settings. Here settings are updated.
                from ___ in new Reader<Settings, Unit>(settings => ListenToPort(settings.Port))
                select html;
            string result = query(Settings.Default);
        }

        private static string DownloadString(string url) => null;

        private static Unit SaveToDatabase(string connectionString, string html) => null;

        private static Settings UpdateSettings(Settings settings)
        {
            settings["Port"] = 80;
            settings.Save();
            return settings;
        }

        private static Unit ListenToPort(int port) => null;
    }
}
