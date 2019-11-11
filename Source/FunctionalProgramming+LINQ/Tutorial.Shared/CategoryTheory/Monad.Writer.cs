namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public abstract class WriterBase<TContent, T>
    {
        private readonly Lazy<(TContent, T)> lazy;

        protected WriterBase(Func<(TContent, T)> writer, IMonoid<TContent> monoid)
        {
            this.lazy = new Lazy<(TContent, T)>(writer);
            this.Monoid = monoid;
        }

        public TContent Content => this.lazy.Value.Item1;

        public T Value => this.lazy.Value.Item2;

        public IMonoid<TContent> Monoid { get; }
    }

    public class Writer<TEntry, T> : WriterBase<IEnumerable<TEntry>, T>
    {
        private static readonly IMonoid<IEnumerable<TEntry>> ContentMonoid =
            new EnumerableConcatMonoid<TEntry>();

        public Writer(Func<(IEnumerable<TEntry>, T)> writer) : base(writer, ContentMonoid) { }

        public Writer(T value) : base(() => (ContentMonoid.Unit(), value), ContentMonoid) { }
    }

    public static partial class WriterExtensions
    {
        // SelectMany: (Writer<TEntry, TSource>, TSource -> Writer<TEntry, TSelector>, (TSource, TSelector) -> TResult) -> Writer<TEntry, TResult>
        public static Writer<TEntry, TResult> SelectMany<TEntry, TSource, TSelector, TResult>(
            this Writer<TEntry, TSource> source,
            Func<TSource, Writer<TEntry, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                new Writer<TEntry, TResult>(() =>
                {
                    Writer<TEntry, TSelector> result = selector(source.Value);
                    return (source.Monoid.Multiply(source.Content, result.Content), 
                        resultSelector(source.Value, result.Value));
                });

        // Wrap: TSource -> Writer<TEntry, TSource>
        public static Writer<TEntry, TSource> Writer<TEntry, TSource>(this TSource value) =>
            new Writer<TEntry, TSource>(value);

        // Select: (Writer<TEnvironment, TSource>, TSource -> TResult) -> Writer<TEnvironment, TResult>
        public static Writer<TEntry, TResult> Select<TEntry, TSource, TResult>(
            this Writer<TEntry, TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Writer<TEntry, TResult>(), (value, result) => result);
    }

    public static partial class WriterExtensions
    {
        public static Writer<string, TSource> LogWriter<TSource>(this TSource value, string log) =>
            new Writer<string, TSource>(() => (log.Enumerable(), value));

        public static Writer<string, TSource> LogWriter<TSource>(this TSource value, Func<TSource, string> logFactory) =>
            new Writer<string, TSource>(() => (logFactory(value).Enumerable(), value));
    }

    public static partial class WriterExtensions
    {
        internal static void Workflow()
        {
            Writer<string, string> query = from filePath in Console.ReadLine().LogWriter(value =>
                                               $"File path: {value}") // Writer<string, string>.
                                           from encodingName in Console.ReadLine().LogWriter(value =>
                                               $"Encoding name: {value}") // Writer<string, string>.
                                           from encoding in Encoding.GetEncoding(encodingName).LogWriter(value =>
                                               $"Encoding: {value}") // Writer<string, Encoding>.
                                           from fileContent in File.ReadAllText(filePath, encoding).LogWriter(value =>
                                               $"File content length: {value.Length}") // Writer<string, string>.
                                           select fileContent; // Define query.
            string result = query.Value; // Execute query.
            query.Content.WriteLines();
            // File path: D:\File.txt
            // Encoding name: utf-8
            // Encoding: System.Text.UTF8Encoding
            // File content length: 76138
        }
    }
}
