namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using Microsoft.FSharp.Linq.RuntimeHelpers;

    [Pure]
    public static partial class EnumerableSelectManyExtensions
    {
        // value.Sequence(hasValue) is the alias of (hasValue ? value.Sequence() : Enumerable.Empty<TSource>())
        public static IEnumerable<TSource> Sequence<TSource>(this TSource value, bool hasValue = false)
        {
            // return hasValue ? EnumerableEx.Return(value) : Enumerable.Empty<TSource>();
            if (hasValue)
            {
                yield return value;
            }
        }
    }

    public static partial class EnumerableSelectManyExtensions
    {
        public static IEnumerable<TSource> Concat<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second) => 
                new IEnumerable<TSource>[] { first, second }.SelectMany(Functions.Id);

        public static IEnumerable<TSource> Distinct<TSource>(
            /* this */ IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return source
                .SelectMany(value => value.Sequence(hashSet.Add(value)));
        }

        public static IEnumerable<TSource> Except<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return first
                .SelectMany(firstValue => firstValue.Sequence(hashSet.Add(firstValue)));
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            /* this */ IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            // return source.ToLookup(keySelector, elementSelector, comparer);
            HashSet<TKey> hashSet = new HashSet<TKey>(comparer);
            return source
                .SelectMany(value => keySelector(value).Enumerable())
                .SelectMany(key => key.Sequence(hashSet.Add(key)))
                // Microsoft.FSharp.Linq.RuntimeHelpers.Grouping<K, T>
                .SelectMany(key => new Grouping<TKey, TElement>(key, source
                    // SelectMany inside SelectMany. Time complexity is O(N * N).
                    .SelectMany(value => elementSelector(value).Sequence(comparer.Equals(key, keySelector(value))))).Enumerable());
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            /* this */ IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            ILookup<TKey, TInner> lookup = inner.ToLookup(innerKeySelector, comparer); // Lookup<TKey, TInner> cannot be created by public API.
            return outer
                .SelectMany(outerValue => resultSelector(outerValue, lookup[outerKeySelector(outerValue)]).Enumerable());
        }

        public static IEnumerable<TSource> Intersect<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return first
                .SelectMany(firstValue => firstValue.Sequence(hashSet.Remove(firstValue)));
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            /* this */ IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            ILookup<TKey, TInner> lookup = inner.ToLookup(innerKeySelector, comparer); // Lookup<TKey, TInner> cannot be created by public API.
            return outer
                .SelectMany(outerValue => lookup[outerKeySelector(outerValue)]
                    .SelectMany(innerValue => resultSelector(outerValue, innerValue).Enumerable()));
        }

        public static IEnumerable<TResult> Select4<TSource, TResult>(
            /* this */ IEnumerable<TSource> source, Func<TSource, TResult> selector) => 
                source.SelectMany(sourceValue => selector(sourceValue).Enumerable());

        public static IEnumerable<TSource> Skip<TSource>
            (/* this */ IEnumerable<TSource> source, int count) => 
                source.SelectMany((value, index) => value.Sequence(index >= count));

        public static IEnumerable<TSource> SkipWhile<TSource>(
            /* this */ IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = false;
            return source
                .SelectMany(value =>
                {
                    if (!flag && !predicate(value))
                    {
                        flag = true; // Imperative.
                    }

                    return value.Sequence(flag);
                });
        }

        public static IEnumerable<TSource> Take<TSource>
            (/* this */ IEnumerable<TSource> source, int count) => 
                source.SelectMany((value, index) => value.Sequence(index < count));

        public static IEnumerable<TSource> TakeWhile<TSource>(
            /* this */ IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = true;
            return source
                .SelectMany(value =>
                {
                    if (!predicate(value))
                    {
                        flag = false; // Imperative.
                    }

                    return value.Sequence(flag);
                });
        }

        public static IEnumerable<TSource> Union<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return new IEnumerable<TSource>[] { first, second }
                .SelectMany(Functions.Id)
                .SelectMany(value => value.Sequence(hashSet.Add(value)));
        }

        public static IEnumerable<TSource> Where<TSource>
            (/* this */ IEnumerable<TSource> source, Func<TSource, bool> predicate) => 
                source.SelectMany(value => value.Sequence(predicate(value)));

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            /* this */ IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) => 
            first.SelectMany((firstValue, index) => second
                // SelectMany inside SelectMany. Time complexity is O(N * N).
                .SelectMany((value, index2) => value.Sequence(index2 == index)), resultSelector);
    }

    public static partial class EnumerableMonadExtensions
    {
        public static IEnumerable<TSource> Concat<TSource>
            (/* this */ IEnumerable<TSource> first, IEnumerable<TSource> second) => 
                from enumerable in new IEnumerable<TSource>[] { first, second }
                from value in enumerable
                select value;

        public static IEnumerable<TSource> Distinct<TSource>(
            /* this */ IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return from value in source
                       // where hashSet.Add(value)
                   from distinct in value.Sequence(hashSet.Add(value))
                   select distinct;
        }

        public static IEnumerable<TSource> Except<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return from value in first
                       // where hashSet.Add(value)
                   from except in value.Sequence(hashSet.Add(value))
                   select except;
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            /* this */ IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            HashSet<TKey> hashSet = new HashSet<TKey>(comparer);
            return from value in source
                   let key = keySelector(value)
                   // where hashSet.Add(key)
                   from distinctKey in key.Sequence(hashSet.Add(key))
                   select new Grouping<TKey, TElement>(
                       distinctKey,
                       from value2 in source
                           // where comparer.Equals(distinctKey, keySelector(value2))
                       from element in elementSelector(value).Sequence(comparer.Equals(key, keySelector(value2)))
                       select elementSelector(value2));
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            /* this */ IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            ILookup<TKey, TInner> lookup = inner.ToLookup(innerKeySelector, comparer);
            return from outerValue in outer
                       // select resultSelector(outerValue, lookup[outerKeySelector(outerValue)])
                   from result in resultSelector(outerValue, lookup[outerKeySelector(outerValue)]).Enumerable()
                   select result;
        }

        public static IEnumerable<TSource> Intersect<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return from firstValue in first
                       // where hashSet.Remove(firstValue)
                   from intersect in firstValue.Sequence(hashSet.Remove(firstValue))
                   select intersect;
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            /* this */ IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            ILookup<TKey, TInner> lookup = inner.ToLookup(innerKeySelector, comparer); // Lookup<TKey, TInner> cannot be created by public API.
            return from outerValue in outer
                   from result in
                       (from innerValue in lookup[outerKeySelector(outerValue)]
                            // select resultSelector(outerValue, innerValue)
                        from result2 in resultSelector(outerValue, innerValue).Enumerable()
                        select result2)
                   select result;
        }

        public static IEnumerable<TResult> Select4<TSource, TResult>(
            /* this */ IEnumerable<TSource> source, Func<TSource, TResult> selector) => 
                from value in source
                // select selector(value)
                from result in selector(value).Enumerable()
                select result;

        public static IEnumerable<TSource> Skip<TSource>(/* this */ IEnumerable<TSource> source, int count)
        {
            int index = 0;
            return from value in source
                   // where index++ >= count
                   from result in value.Sequence(index++ >= count)
                   select result;
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(
            /* this */ IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = false;
            return from value in source
                   let _ = !flag && !predicate(value) && (flag = true)
                   from result in value.Sequence(flag)
                   select result;
        }

        public static IEnumerable<TSource> Take<TSource>(/* this */ IEnumerable<TSource> source, int count)
        {
            int index = 0;
            return from value in source
                       // where index++ < count
                   from result in value.Sequence(index++ < count)
                   select result;
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(
            /* this */ IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = true;
            return from value in source
                   let _ = predicate(value) || (flag = false)
                   from result in value.Sequence(flag)
                   select result;
        }

        public static IEnumerable<TSource> Union<TSource>(
            /* this */ IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return from enumerable in new IEnumerable<TSource>[] { first, second }
                   from value in enumerable
                       // where hashSet.Add(value)
                   from result in value.Sequence(hashSet.Add(value))
                   select result;
        }

        public static IEnumerable<TSource> Where<TSource>
            (/* this */ IEnumerable<TSource> source, Func<TSource, bool> predicate) => 
                from value in source
                from result in value.Sequence(predicate(value))
                select result;

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            /* this */ IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            int firstIndex = 0;
            int secondIndex = 0;
            return from firstValue in first
                   let currentFirstIndex = firstIndex++
                   let _ = secondIndex = 0
                   from secondResult in
                       (from secondValue in second
                            // where firstIndex2 == secondIndex++
                            // let secondIndex3 = secondIndex++
                        from secondResult in secondValue.Sequence(currentFirstIndex == secondIndex++)
                        select secondResult)
                   select resultSelector(firstValue, secondResult);
        }
    }
}
