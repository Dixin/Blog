namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public abstract class WriterBase<TContent, T>
    {
        private readonly Lazy<TContent, T> lazy;

        protected WriterBase(Func<Tuple<TContent, T>> writer, IMonoid<TContent> monoid)
        {
            this.lazy = new Lazy<TContent, T>(writer);
            this.Monoid = monoid;
        }

        public TContent Content => this.lazy.Value1;

        public T Value => this.lazy.Value2;

        public IMonoid<TContent> Monoid { get; }
    }

    public class Writer<TContent, T> : WriterBase<IEnumerable<TContent>, T>
    {
        private static readonly IMonoid<IEnumerable<TContent>> ContentMonoid = 
            new EnumerableConcatMonoid<TContent>();

        public Writer(Func<Tuple<IEnumerable<TContent>, T>> writer) : base(writer, ContentMonoid)
        {
        }

        public Writer(T value) : base(() => ContentMonoid.Unit().Tuple(value), ContentMonoid)
        {
        }
    }

    public static partial class WriterExtensions
    {
        public static Writer<TContent, TResult> SelectMany<TContent, TSource, TSelector, TResult>(
            this Writer<TContent, TSource> source,
            Func<TSource, Writer<TContent, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                new Writer<TContent, TResult>(() =>
                {
                    Writer<TContent, TSelector> result = selector(source.Value);
                    return source.Monoid.Multiply(source.Content, result.Content).Tuple(
                        resultSelector(source.Value, result.Value));
                });

        // Wrap: TSource -> Writer<TContent, TSource>
        public static Writer<TContent, TSource> Writer<TContent, TSource>(this TSource value) =>
            new Writer<TContent, TSource>(value);
    }

    public static partial class WriterExtensions
    {
        public static Writer<string, TSource> LogWriter<TSource>(this TSource value, params string[] logs) =>
            new Writer<string, TSource>(() =>
                logs.Select(log => $"{DateTime.Now.ToString("o", CultureInfo.InvariantCulture)} {log}").Tuple(value));
    }

    public static partial class EnumerableExtensions
    {
        public static Tuple<T, IEnumerable<T>> Pop<T>(this IEnumerable<T> source)
        {
            source = source.Share();
            return source.First().Tuple(source);
        }

        public static Tuple<T, IEnumerable<T>> Push<T>(this IEnumerable<T> source, T value) =>
            value.Tuple(source.Concat(value.Enumerable()));
    }

    public static partial class WriterExtensions
    {
        internal static void Stack()
        {
            IEnumerable<int> stack = Enumerable.Empty<int>();
            Writer<string, IEnumerable<int>> query =
                from lazy1 in stack.Push(1).LogWriter("Push 1 to stack.")
                from lazy2 in lazy1.Item2.Push(2).LogWriter("Push 2 to stack.")
                from lazy3 in lazy2.Item2.Pop().LogWriter("Pop 2 from stack.")
                from stack1 in Enumerable.Range(0, 3).LogWriter("Reset stack to 0, 1, 2.")
                from lazy4 in stack1.Push(4).LogWriter("Push 4 to stack.")
                from lazy5 in lazy4.Item2.Pop().LogWriter("Pop 4 from stack.")
                from stack2 in lazy5.Item2.LogWriter("Get current stack.")
                select stack2; // Define query.

            IEnumerable<int> result = query.Value; // Execute query.
            IEnumerable<string> logs = query.Content;
            logs.ForEach(log => Trace.WriteLine(log));
        }
    }
}
