namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class WriterBase<TContent, T>
    {
        private readonly Lazy<Tuple<TContent, T>> lazy;

        public WriterBase(Func<Tuple<TContent, T>> writer, IMonoid<TContent> monoid)
        {
            this.lazy = new Lazy<Tuple<TContent, T>>(writer);
            this.Monoid = monoid;
        }

        public TContent Content => this.lazy.Value.Item1;

        public T Value => this.lazy.Value.Item2;

        public IMonoid<TContent> Monoid { get; }
    }

    public class Writer<TContent, T> : WriterBase<IEnumerable<TContent>, T>
    {
        public Writer(Func<Tuple<IEnumerable<TContent>, T>> writer)
            : base(writer, new EnumerableConcatMonoid<TContent>())
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

        // Wrap: T -> Writer<TContent, T>
        public static Writer<TContent, T> Writer<TContent, T>(this T value) =>
            new Writer<TContent, T>(() => new EnumerableConcatMonoid<TContent>().Unit().Tuple(value));
    }

    public static partial class WriterExtensions
    {
        public static Writer<string, TSource> LogWriter<TSource>(this TSource value, params string[] logs) =>
            new Writer<string, TSource>(() => 
                logs.Select(log => $"{DateTime.Now.ToString("o", CultureInfo.InvariantCulture)} {log}").Tuple(value));
    }

    public static partial class WriterExtensions
    {
        internal static void Stack()
        {
            IEnumerable<int> stack = Enumerable.Empty<int>();
            Writer<string, IEnumerable<int>> writer =
                from lazy1 in stack.Push(1).LogWriter("Push 1 to stack.")
                from lazy2 in lazy1.Value2.Push(2).LogWriter("Push 2 to stack.")
                from lazy3 in lazy2.Value2.Pop().LogWriter("Pop 2 from stack.")
                from stack1 in Enumerable.Range(0, 3).LogWriter("Reset stack to 0, 1, 2.")
                from lazy4 in stack1.Push(4).LogWriter("Push 4 to stack.")
                from lazy5 in lazy4.Value2.Pop().LogWriter("Pop 4 from stack.")
                from stack2 in lazy5.Value2.LogWriter("Get current stack.")
                select stack2;

            IEnumerable<int> result = writer.Value;
            IEnumerable<string> logs = writer.Content;
            logs.ForEach(log => Trace.WriteLine(log));
        }
    }
}
