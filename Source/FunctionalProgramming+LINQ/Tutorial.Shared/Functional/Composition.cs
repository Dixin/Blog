namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static partial class Functions
    {
        internal static void OutputAsInput()
        {
            string input = "-2";
            int output1 = int.Parse(input); // string -> int
            int output2 = Math.Abs(output1); // int -> int
            double output3 = Convert.ToDouble(output2); // int -> double
            double output4 = Math.Sqrt(output3); // double -> double
        }

        // string -> double
        internal static double Composition(string input) => 
            Math.Sqrt(Convert.ToDouble(Math.Abs(int.Parse(input))));

        internal static void Compose()
        {
            Func<string, int> parse = int.Parse; // string -> int
            Func<int, int> abs = Math.Abs; // int -> int
            Func<int, double> convert = Convert.ToDouble; // int -> double
            Func<double, double> sqrt = Math.Sqrt; // double -> double

            // string -> double
            Func<string, double> composition1 = sqrt.After(convert).After(abs).After(parse);
            composition1("-2.0").WriteLine(); // 1.4142135623731

            // string -> double
            Func<string, double> composition2 = parse.Then(abs).Then(convert).Then(sqrt);
            composition2("-2.0").WriteLine(); // 1.4142135623731
        }

        internal static void Linq()
        {
            Func<IEnumerable<int>, IEnumerable<int>> where = source => Enumerable.Where(source, int32 => int32 > 0);
            Func<IEnumerable<int>, IEnumerable<int>> skip = filtered => Enumerable.Skip(filtered, 1);
            Func<IEnumerable<int>, IEnumerable<int>> take = skipped => Enumerable.Take(skipped, 2);
            IEnumerable<int> query = take(skip(where(new int[] { 4, 3, 2, 1, 0, -1 })));
            foreach (int result in query) // Execute query.
            {
                result.WriteLine();
            }
        }

        internal static void ComposeLinq()
        {
            Func<IEnumerable<int>, IEnumerable<int>> composition =
                new Func<IEnumerable<int>, IEnumerable<int>>(source => Enumerable.Where(source, int32 => int32 > 0))
                    .Then(filtered => Enumerable.Skip(filtered, 1))
                    .Then(skipped => Enumerable.Take(skipped, 2));
            IEnumerable<int> query = composition(new int[] { 4, 3, 2, 1, 0, -1 });
            foreach (int result in query) // Execute query.
            {
                result.WriteLine();
            }
        }

        // Func<TSource, bool> -> IEnumerable<TSource> -> IEnumerable<TSource>
        internal static Func<IEnumerable<TSource>, IEnumerable<TSource>> Where<TSource>(
            Func<TSource, bool> predicate) => (IEnumerable<TSource> source) => Enumerable.Where(source, predicate);

        // int -> IEnumerable<TSource> -> IEnumerable<TSource>
        internal static Func<IEnumerable<TSource>, IEnumerable<TSource>> Skip<TSource>(
            int count) => source => Enumerable.Skip(source, count);

        // int -> IEnumerable<TSource> -> IEnumerable<TSource>
        internal static Func<IEnumerable<TSource>, IEnumerable<TSource>> Take<TSource>(
            int count) => source => Enumerable.Take(source, count);

        internal static void LinqWithPartialApplication()
        {
            // IEnumerable<TSource> -> IEnumerable<TSource>
            Func<IEnumerable<int>, IEnumerable<int>> where = Where<int>(int32 => int32 > 0);
            Func<IEnumerable<int>, IEnumerable<int>> skip = Skip<int>(1);
            Func<IEnumerable<int>, IEnumerable<int>> take = Take<int>(2);

            IEnumerable<int> query = take(skip(where(new int[] { 4, 3, 2, 1, 0, -1 })));
            foreach (int result in query) // Execute query.
            {
                result.WriteLine();
            }
        }

        internal static void ComposeLinqWithPartialApplication()
        {
            Func<IEnumerable<int>, IEnumerable<int>> composition =
                Where<int>(int32 => int32 > 0)
                .Then(Skip<int>(1))
                .Then(Take<int>(2));

            IEnumerable<int> query = composition(new int[] { 4, 3, 2, 1, 0, -1 });
            foreach (int result in query) // Execute query.
            {
                result.WriteLine();
            }
        }

        internal static void Forward()
        {
            "-2"
                .Forward(int.Parse) // string -> int
                .Forward(Math.Abs) // int -> int
                .Forward(Convert.ToDouble) // int -> double
                .Forward(Math.Sqrt) // double -> double
                .Forward(Console.WriteLine); // double -> void

            // Equivalent to:
            Console.WriteLine(Math.Sqrt(Convert.ToDouble(Math.Abs(int.Parse("-2")))));
        }

        internal static void ForwardAndNullConditional(IDictionary<string, object> dictionary, string key)
        {
            object value = dictionary[key];
            DateTime? dateTime1;
            if (value != null)
            {
                dateTime1 = Convert.ToDateTime(value);
            }
            else
            {
                dateTime1 = null;
            }

            // Equivalent to:
            DateTime? dateTime2 = dictionary[key]?.Forward(Convert.ToDateTime);
        }

        internal static void ForwardLinqWithPartialApplication()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 };
            IEnumerable<int> query = source
                .Forward(Where<int>(int32 => int32 > 0))
                .Forward(Skip<int>(1))
                .Forward(Take<int>(2));
            foreach (int result in query) // Execute query.
            {
                result.WriteLine();
            }
        }

        internal static void InstanceMethodChaining(string @string)
        {
            string result = @string.TrimStart().Substring(1, 10).Replace("a", "b").ToUpperInvariant();
        }
    }
}

#if DEMO
namespace System.Collections.Generic
{
    public interface IEnumerable<out T> : IEnumerable
    {
        IEnumerator<T> GetEnumerator();
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        // (IEnumerable<TSource>, TSource -> bool) -> IEnumerable<TSource>
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        // (IEnumerable<TSource>, int) -> IEnumerable<TSource>
        public static IEnumerable<TSource> Skip<TSource>(
            this IEnumerable<TSource> source, int count);

        // (IEnumerable<TSource>, int) -> IEnumerable<TSource>
        public static IEnumerable<TSource> Take<TSource>(
            this IEnumerable<TSource> source, int count);

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }

    public static class Enumerable
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);
    }
}

namespace System.Linq
{
    public static class ParallelEnumerable
    {
        public static ParallelQuery<TSource> Where<TSource>(
            this ParallelQuery<TSource> source, Func<TSource, bool> predicate);

        public static OrderedParallelQuery<TSource> OrderBy<TSource, TKey>(
            this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector);

        public static ParallelQuery<TResult> Select<TSource, TResult>(
            this ParallelQuery<TSource> source, Func<TSource, TResult> selector);

        // Other members.
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Func<TSource, bool> predicate);

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Func<TSource, TResult> selector);

        // Other members.
    }
}
#endif
