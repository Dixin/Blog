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
                source.OrderBy(keySelector, ToComparer(compare));

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.OrderByDescending(keySelector, ToComparer(compare));

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.ThenBy(keySelector, ToComparer(compare));

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> compare) =>
                source.ThenByDescending(keySelector, ToComparer(compare));
    }

    public static partial class EnumerableX
    {
        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            Func<TKey, TKey, bool> equals,
            Func<TKey, int> getHashCode = null) =>
                source.GroupBy(keySelector, elementSelector, resultSelector, ToEqualityComparer(equals, getHashCode));

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            Func<TKey, TKey, bool> equals,
            Func<TKey, int> getHashCode = null) =>
                outer.Join(
                    inner, 
                    outerKeySelector, 
                    innerKeySelector, 
                    resultSelector, 
                    ToEqualityComparer(equals, getHashCode));

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            Func<TKey, TKey, bool> equals,
            Func<TKey, int> getHashCode = null) =>
                outer.GroupJoin(
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    ToEqualityComparer(equals, getHashCode));

        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, bool> equals,
            Func<TSource, int> getHashCode = null) =>
                source.Distinct(ToEqualityComparer(equals, getHashCode));

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equals,
            Func<TSource, int> getHashCode = null) =>
                first.Union(second, ToEqualityComparer(equals, getHashCode));

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equals,
            Func<TSource, int> getHashCode = null) =>
                first.Intersect(second, ToEqualityComparer(equals, getHashCode));

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equals,
            Func<TSource, int> getHashCode = null) =>
                first.Except(second, ToEqualityComparer(equals, getHashCode));
    }

    public static partial class EnumerableX
    {
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, TKey, bool> equals,
            Func<TKey, int> getHashCode = null) =>
                source.ToDictionary(keySelector, elementSelector, ToEqualityComparer(equals, getHashCode));

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, TKey, bool> equals,
            Func<TKey, int> getHashCode = null) =>
                source.ToLookup(keySelector, elementSelector, ToEqualityComparer(equals, getHashCode));
    }

    public static partial class EnumerableX
    {
        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            Func<TSource, TSource, bool> equals,
            Func<TSource, int> getHashCode = null) => 
                source.Contains(value, ToEqualityComparer(equals, getHashCode));

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> equals,
            Func<TSource, int> getHashCode = null) => 
                first.SequenceEqual(second, ToEqualityComparer(equals, getHashCode));
    }

    public static partial class EnumerableX
    {
        private static IComparer<T> ToComparer<T>(Func<T, T, int> compare) =>
            Comparer<T>.Create(new Comparison<T>(compare));

        private static IEqualityComparer<T> ToEqualityComparer<T>(
            Func<T, T, bool> equals, Func<T, int> getHashCode = null) =>
                new EqualityComparerWrapper<T>(equals, getHashCode);
#if NETFX
        // HashIdentity.FromFunctions<T>(
        //    new Converter<T, int>(getHashCode ?? (value => value.GetHashCode())),
        //    new Converter<T, FSharpFunc<T, bool>>(value1 => new Converter<T, bool>(value2 => equals(value1, value2))));
#endif
    }

    // Microsoft.FSharp.Collections.HashIdentity.FromFunctions@32<T>
    public class EqualityComparerWrapper<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> equals;

        private readonly Func<T, int> getHashCode;

        public EqualityComparerWrapper(Func<T, T, bool> equals, Func<T, int> getHashCode = null)
        {
            this.equals = equals;
            this.getHashCode = getHashCode ?? (value => value.GetHashCode());
        }

        public bool Equals(T x, T y) => this.equals(x, y);

        public int GetHashCode(T obj) => this.getHashCode(obj);
    }
}
