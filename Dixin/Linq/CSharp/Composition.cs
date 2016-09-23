namespace Dixin.Linq.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal static partial class Functions
    {
        internal static void OutputAsInput()
        {
            double input = -2D;
            double middle = Math.Abs(input);
            double output = Math.Sqrt(middle);
        }

        internal static double AbsSqrt(double @double) => Math.Sqrt(Math.Abs(@double));

        internal static void Composite()
        {
            Func<double, double> sqrt = Math.Sqrt;
            Func<double, double> abs = Math.Abs;

            Func<double, double> absSqrt1 = sqrt.After(abs);
            Trace.WriteLine(absSqrt1(-2D));

            Func<double, double> absSqrt2 = abs.Before(sqrt);
            Trace.WriteLine(absSqrt2(-2D));
        }

        internal static void CompositeLinq()
        {
            Func<IEnumerable<int>, IEnumerable<double>> filterSortMap =
                new Func<IEnumerable<int>, IEnumerable<int>>(source => Enumerable.Where(source, int32 => int32 > 0))
                    .Before(filtered => Enumerable.OrderBy(filtered, int32 => int32))
                    .Before(sorted => Enumerable.Select(sorted, int32 => Math.Sqrt(int32)));
            IEnumerable<double> query = filterSortMap(new int[] { 4, 3, 2, 1, 0, -1 });
            foreach (double result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }

        internal static void Forward()
        {
            (-2D)
                .Forward(Math.Abs)
                .Forward(Math.Sqrt)
                .ToString(CultureInfo.InvariantCulture)
                .Forward(Console.WriteLine);

            // Equivalent to:
            Console.WriteLine(Math.Sqrt(Math.Abs(-2D)).ToString(CultureInfo.InvariantCulture));
        }

        internal static void ForwardLinq()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 };
            IEnumerable<double> query = source
                .Forward(Enumerable.Where, new Func<int, bool>(int32 => int32 > 0))
                .Forward(Enumerable.OrderBy, new Func<int, int>(int32 => int32))
                .Forward(Enumerable.Select, new Func<int, double>(int32 => Math.Sqrt(int32)));
            foreach (double result in query)
            {
                Trace.WriteLine(result);
            }
        }

        internal static Func<Func<int, bool>, IEnumerable<int>, IEnumerable<int>> filter =
            (predicate, source) => Enumerable.Where(source, predicate);

        internal static Func<Func<int, int>, IEnumerable<int>, IEnumerable<int>> sort =
            (keySelector, source) => Enumerable.OrderBy(source, keySelector);

        internal static Func<Func<int, double>, IEnumerable<int>, IEnumerable<double>> map =
            (selector, source) => Enumerable.Select(source, selector);

        internal static void CompositeAndPartialApply()
        {
            Func<IEnumerable<int>, IEnumerable<double>> filterSortMap =
                filter.Partial(int32 => int32 > 0)
                    .Before(sort.Partial(int32 => int32))
                    .Before(map.Partial(int32 => Math.Sqrt(int32)));
            IEnumerable<double> query = filterSortMap(new int[] { 4, 3, 2, 1, 0, -1 });
            foreach (double result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }

        internal static void ForwardWithPartialApply()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 };
            IEnumerable<double> query = source
                .Forward(filter.Partial(int32 => int32 > 0))
                .Forward(sort.Partial(int32 => int32))
                .Forward(map.Partial(int32 => Math.Sqrt(int32)));
            foreach (double result in query)
            {
                Trace.WriteLine(result);
            }
        }

        internal static void ForwardAndNullConditional(IDictionary<string, object> dictionary, string key)
        {
            object valueObject = dictionary[key];
            DateTime? dateTime1;
            if (valueObject != null)
            {
                dateTime1 = Convert.ToDateTime(valueObject);
            }
            else
            {
                dateTime1 = null;
            }

            // Equivalent to:
            DateTime? creationDate2 = dictionary[key]?.Forward(Convert.ToDateTime);
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
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);

        // Other members...
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

        // Other members...
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Func<TSource, bool> predicate);

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Func<TSource, TResult> selector);

        // Other members...
    }
}
#endif
