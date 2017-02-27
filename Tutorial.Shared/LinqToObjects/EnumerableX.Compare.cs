namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class EnumerableX
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.OrderBy(keySelector, compare.ToComparer());

        public static IOrderedEnumerable<TSource> OrderBy2<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.OrderBy(keySelector, compare.ToComparer());

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.OrderByDescending(keySelector, compare.ToComparer());

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.ThenBy(keySelector, compare.ToComparer());

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.ThenByDescending(keySelector, compare.ToComparer());
    }

    public static partial class EnumerableX
    {
        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            Func<TKey, TKey, bool> equality,
            Func<TKey, int> getHashCode = null) =>
                source.GroupBy(keySelector, elementSelector, resultSelector, equality.ToComparer(getHashCode));

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            Func<TKey, TKey, bool> equality,
            Func<TKey, int> getHashCode = null) =>
                outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, equality.ToComparer(getHashCode));

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            Func<TKey, TKey, bool> equality,
            Func<TKey, int> getHashCode = null) =>
                outer.GroupJoin(
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    equality.ToComparer(getHashCode));

        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, bool> equality,
            Func<TSource, int> getHashCode = null) =>
                source.Distinct(equality.ToComparer(getHashCode));

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equality,
            Func<TSource, int> getHashCode = null) =>
                first.Union(second, equality.ToComparer(getHashCode));

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equality,
            Func<TSource, int> getHashCode = null) =>
                first.Intersect(second, equality.ToComparer(getHashCode));

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equality,
            Func<TSource, int> getHashCode = null) =>
                first.Except(second, equality.ToComparer(getHashCode));
    }

    public static partial class EnumerableX
    {
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, TKey, bool> equality,
            Func<TKey, int> getHashCode = null) =>
                source.ToDictionary(keySelector, elementSelector, equality.ToComparer(getHashCode));

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, TKey, bool> equality,
            Func<TKey, int> getHashCode = null) =>
                source.ToLookup(keySelector, elementSelector, equality.ToComparer(getHashCode));
    }

    public static partial class EnumerableX
    {
        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            Func<TSource, TSource, bool> equality,
            Func<TSource, int> getHashCode = null) => source.Contains(value, equality.ToComparer(getHashCode));

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equality,
            Func<TSource, int> getHashCode = null) => first.SequenceEqual(second, equality.ToComparer(getHashCode));
    }

    public class ComparerWrapper<T> : IComparer<T>
    {
        private readonly Func<T, T, int> compare;

        public ComparerWrapper(Func<T, T, int> compare) => this.compare = compare;

        public int Compare(T x, T y) => this.compare(x, y);
    }

    public class EqualityComparerWrapper<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> equality;

        private readonly Func<T, int> getHashCode;

        public EqualityComparerWrapper(Func<T, T, bool> equality, Func<T, int> getHashCode = null)
        {
            this.equality = equality;
            this.getHashCode = getHashCode ?? (value => value.GetHashCode());
        }

        public bool Equals(T x, T y) => this.equality(x, y);

        public int GetHashCode(T obj) => this.getHashCode(obj);
    }

    public static partial class FuncExtensions
    {
        public static IComparer<T> ToComparer<T>(this Func<T, T, int> compare) => new ComparerWrapper<T>(compare);

        public static IEqualityComparer<T> ToComparer<T>
            (this Func<T, T, bool> equality, Func<T, int> getHashCode = null) =>
                new EqualityComparerWrapper<T>(equality, getHashCode);
    }
}
