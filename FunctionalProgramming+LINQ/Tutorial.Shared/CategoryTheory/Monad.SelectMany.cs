namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.FSharp.Linq.RuntimeHelpers;

    public static partial class EnumerableSelectManyExtensions
    {
        // value.Sequence(hasValue) is the alias of (hasValue ? value.Sequence() : Enumerable.Empty<TSource>())
        internal static IEnumerable<TSource> ToEnumerable<TSource>(this TSource value, bool hasValue = false)
        {
            // return hasValue ? new TSource[] { value } : Enumerable.Empty<TSource>();
            if (hasValue)
            {
                yield return value;
            }
        }
    }

    public static partial class EnumerableSelectManyExtensions
    {
        public static IEnumerable<TSource> Concat<TSource>(
            IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                new IEnumerable<TSource>[] { first, second }.SelectMany(Functions.Id);

        public static IEnumerable<TSource> Distinct<TSource>(
            IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = null;
            return source
                .SelectMany(value => (hashSet ?? (hashSet = new HashSet<TSource>(comparer))).Add(value)
                    ? new TSource[] { value } : new TSource[0]);
        }

        public static IEnumerable<TSource> Except<TSource>(
            IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = null;
            return first
                .SelectMany(firstValue => (hashSet ?? (hashSet = new HashSet<TSource>(second, comparer))).Add(firstValue)
                    ? new TSource[] { firstValue } : new TSource[0]);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            // return source.ToLookup(keySelector, elementSelector, comparer);
            HashSet<TKey> hashSet = new HashSet<TKey>(comparer);
            return source
                .SelectMany(value => keySelector(value).Enumerable())
                .SelectMany(key => hashSet.Add(key) ? new TKey[] { key } : new TKey[0])
                .SelectMany(key => new Grouping<TKey, TElement>(key, source
                    // SelectMany inside SelectMany. Time complexity is O(N * N).
                    .SelectMany(value => elementSelector(value).ToEnumerable(comparer.Equals(key, keySelector(value))))).Enumerable());
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
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
            IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return first
                .SelectMany(firstValue => firstValue.ToEnumerable(hashSet.Remove(firstValue)));
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
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
            IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(sourceValue => selector(sourceValue).Enumerable());

        public static IEnumerable<TSource> Skip<TSource>
            (IEnumerable<TSource> source, int count) =>
                source.SelectMany((value, index) => value.ToEnumerable(index >= count));

        public static IEnumerable<TSource> SkipWhile<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = false;
            return source
                .SelectMany(value =>
                {
                    if (!flag && !predicate(value))
                    {
                        flag = true; // Imperative.
                    }

                    return value.ToEnumerable(flag);
                });
        }

        public static IEnumerable<TSource> Take<TSource>
            (IEnumerable<TSource> source, int count) =>
                source.SelectMany((value, index) => value.ToEnumerable(index < count));

        public static IEnumerable<TSource> TakeWhile<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = true;
            return source
                .SelectMany(value =>
                {
                    if (!predicate(value))
                    {
                        flag = false; // Imperative.
                    }

                    return value.ToEnumerable(flag);
                });
        }

        public static IEnumerable<TSource> Union<TSource>(
            IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return new IEnumerable<TSource>[] { first, second }
                .SelectMany(Functions.Id)
                .SelectMany(value => value.ToEnumerable(hashSet.Add(value)));
        }

        public static IEnumerable<TSource> Where<TSource>
            (IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                source.SelectMany(value => value.ToEnumerable(predicate(value)));

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) =>
            first.SelectMany((firstValue, index) => second
                // SelectMany inside SelectMany. Time complexity is O(N * N).
                .SelectMany((value, index2) => value.ToEnumerable(index2 == index)), resultSelector);
    }

    public static partial class EnumerableMonadExtensions
    {
        public static IEnumerable<TSource> Concat<TSource>
            (IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                from enumerable in new IEnumerable<TSource>[] { first, second }
                from value in enumerable
                select value;

        public static IEnumerable<TSource> Distinct<TSource>(
            IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return from value in source
                       // where hashSet.Add(value)
                   from distinct in value.ToEnumerable(hashSet.Add(value))
                   select distinct;
        }

        public static IEnumerable<TSource> Except<TSource>(
            IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return from value in first
                       // where hashSet.Add(value)
                   from except in value.ToEnumerable(hashSet.Add(value))
                   select except;
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            HashSet<TKey> hashSet = new HashSet<TKey>(comparer);
            return from value in source
                   let key = keySelector(value)
                   // where hashSet.Add(key)
                   from distinctKey in key.ToEnumerable(hashSet.Add(key))
                   select new Grouping<TKey, TElement>(
                       distinctKey,
                       from value2 in source
                           // where comparer.Equals(distinctKey, keySelector(value2))
                       from element in elementSelector(value).ToEnumerable(comparer.Equals(key, keySelector(value2)))
                       select elementSelector(value2));
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
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
            IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(second, comparer);
            return from firstValue in first
                       // where hashSet.Remove(firstValue)
                   from intersect in firstValue.ToEnumerable(hashSet.Remove(firstValue))
                   select intersect;
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
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
            IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                from value in source
                    // select selector(value)
                from result in selector(value).Enumerable()
                select result;

        public static IEnumerable<TSource> Skip<TSource>(IEnumerable<TSource> source, int count)
        {
            int index = 0;
            return from value in source
                       // where index++ >= count
                   from result in value.ToEnumerable(index++ >= count)
                   select result;
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = false;
            return from value in source
                   let _ = !flag && !predicate(value) && (flag = true)
                   from result in value.ToEnumerable(flag)
                   select result;
        }

        public static IEnumerable<TSource> Take<TSource>(IEnumerable<TSource> source, int count)
        {
            int index = 0;
            return from value in source
                       // where index++ < count
                   from result in value.ToEnumerable(index++ < count)
                   select result;
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool flag = true;
            return from value in source
                   let _ = predicate(value) || (flag = false)
                   from result in value.ToEnumerable(flag)
                   select result;
        }

        public static IEnumerable<TSource> Union<TSource>(
            IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return from enumerable in new IEnumerable<TSource>[] { first, second }
                   from value in enumerable
                       // where hashSet.Add(value)
                   from result in value.ToEnumerable(hashSet.Add(value))
                   select result;
        }

        public static IEnumerable<TSource> Where<TSource>
            (IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                from value in source
                from result in value.ToEnumerable(predicate(value))
                select result;

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
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
                        from secondResult in secondValue.ToEnumerable(currentFirstIndex == secondIndex++)
                        select secondResult)
                   select resultSelector(firstValue, secondResult);
        }
    }
}
