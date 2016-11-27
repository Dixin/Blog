namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class Writer<T, TContent>
    {
        private readonly Lazy<Tuple<T, TContent>> lazy;

        public Writer(Func<Tuple<T, TContent>> factory, IMonoid<TContent> monoid)
        {
            this.lazy = new Lazy<Tuple<T, TContent>>(factory);
            this.Monoid = monoid;
        }

        public T Value => this.lazy.Value.Item1;

        public TContent Content => this.lazy.Value.Item2;

        public IMonoid<TContent> Monoid { get; }
    }

    public static partial class WriterExtensions
    {
        // Required by LINQ.
        public static Writer<TResult, TContent> SelectMany<TContent, TSource, TSelector, TResult>
            (this Writer<TSource, TContent> source,
                Func<TSource, Writer<TSelector, TContent>> selector,
                Func<TSource, TSelector, TResult> resultSelector) =>
                new Writer<TResult, TContent>(() =>
                    {
                        Writer<TSelector, TContent> selectorResult = selector(source.Value);
                        return resultSelector(source.Value, selectorResult.Value).Tuple(
                            source.Monoid.Multiply(source.Content, selectorResult.Content));
                    }, source.Monoid);

        // Wrap: T -> Writer<T, TContent>
        public static Writer<T, TContent> Writer<T, TContent>
            (this T value, TContent content, IMonoid<TContent> monoid) =>
                new Writer<T, TContent>(() => value.Tuple(content), monoid);
    }

    public static partial class WriterExtensions
    {
        public static Writer<TSource, IEnumerable<string>> WithLog<TSource>(this TSource value, string log) =>
            value.Writer(
                $"{DateTime.Now.ToString("o", CultureInfo.InvariantCulture)} - {log}".Enumerable(),
                new EnumerableConcatMonoid<string>());
    }

    // Impure.
    internal static partial class WriterQuery
    {
        internal static void Stack()
        {
            IEnumerable<int> stack = Enumerable.Empty<int>();
            Writer<IEnumerable<int>, IEnumerable<string>> writer =
                from lazy1 in stack.Push(1).WithLog("Push 1 to stack.")
                from lazy2 in lazy1.Value2.Push(2).WithLog("Push 2 to stack.")
                from lazy3 in lazy2.Value2.Pop().WithLog("Pop 2 from stack.")
                from stack1 in Enumerable.Range(0, 3).WithLog("Reset stack to 0, 1, 2.")
                from lazy4 in stack1.Push(4).WithLog("Push 4 to stack.")
                from lazy5 in lazy4.Value2.Pop().WithLog("Pop 4 from stack.")
                from stack2 in lazy5.Value2.WithLog("Get current stack.")
                select stack2;

            IEnumerable<int> resultStack = writer.Value;
            IEnumerable<string> logs = writer.Content;
            logs.ForEach(log => Trace.WriteLine(log));
        }
    }
}
