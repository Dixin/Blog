namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.FSharp.Linq.RuntimeHelpers;

    internal static partial class EnumerableExtensions
    {
        internal static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            ICollection<TSource> genericCollection = source as ICollection<TSource>;
            if (genericCollection != null)
            {
                TSource[] array = new TSource[genericCollection.Count];
                genericCollection.CopyTo(array, 0);
                return array;
            }

            ICollection collection = source as ICollection;
            if (collection != null)
            {
                TSource[] array = new TSource[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }

            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext()) // source is empty.
                {
                    return new TSource[0];
                }

                const int initialLength = 4;
                TSource[] array = new TSource[initialLength];
                int length = 0;
                do
                {
                    if (array.Length == length)
                    {
                        Array.Resize(ref array, checked(length * 2)); // Doubles size when full.
                    }

                    array[length++] = iterator.Current;
                }
                while (iterator.MoveNext());

                Array.Resize(ref array, length); // Finalizes size when done.
                return array;
            }
        }
    }

    internal static partial class EnumerableExtensions
    {
        #region Conversion

        internal static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) => new List<TSource>(source);

        internal static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource value in source)
            {
                dictionary.Add(keySelector(value), elementSelector(value));
            }

            return dictionary;
        }

        internal static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector = null,
            IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, List<TElement>> groupsWithNonNullKey = new Dictionary<TKey, List<TElement>>(comparer);
            List<TElement> groupWithNullKey = new List<TElement>();
            foreach (TSource value in source)
            {
                TKey key = keySelector(value);
                if (key == null)
                {
                    groupWithNullKey.Add(elementSelector(value));
                }
                else
                {
                    if (!groupsWithNonNullKey.ContainsKey(key))
                    {
                        groupsWithNonNullKey.Add(key, new List<TElement>());
                    }

                    groupsWithNonNullKey[key].Add(elementSelector(value));
                }
            }

            return new Lookup<TKey, TElement>(
                groupsWithNonNullKey.ToDictionary(
                    group => group.Key,
                    group => new Grouping<TKey, TElement>(group.Key, group.Value) as IGrouping<TKey, TElement>,
                    comparer),
                new Grouping<TKey, TElement>(default(TKey), groupWithNullKey),
                groupWithNullKey.Count > 0);
        }

        internal static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) => source.ToLookup(keySelector, value => value, comparer);

        #endregion

        #region Conversion

        internal static IEnumerable<TSource> AsEnumerable<TSource>
            (this IEnumerable<TSource> source) => source; // Deferred execution.

#if DEMO
        internal static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            IEnumerable<TResult> generic = source as IEnumerable<TResult>;
            if (generic != null)
            {
                return generic; // Deferred execution.
            }

            foreach (object value in source)
            {
                yield return (TResult)value; // Deferred execution.
            }
        }
#endif

        internal static IEnumerable<TResult> Cast<TResult>
            (this IEnumerable source) => source as IEnumerable<TResult> ?? source.CastGenerator<TResult>();  // Deferred execution.

        internal static IEnumerable<TResult> CastGenerator<TResult>(this IEnumerable source)
        {
            foreach (object value in source)
            {
                yield return (TResult)value; // Deferred execution.
            }
        }

        #endregion

        #region Generation

        internal static IEnumerable<TResult> Empty<TResult>() => EmptyArray<TResult>.Cache;

        internal static IEnumerable<TResult> Empty2<TResult>()
        {
            yield break;
        }

        internal static IEnumerable<int> Range(int start, int count)
        {
            for (int value = start; value < start + count; value++)
            {
                yield return value; // Deferred execution.
            }
        }

        internal static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return element; // Deferred execution.
            }
        }

        internal static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source, TSource defaultValue = default(TSource))
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    do
                    {
                        yield return iterator.Current; // Deferred execution.
                    }
                    while (iterator.MoveNext());
                }
                else // source is empty.
                {
                    yield return defaultValue; // Deferred execution.
                }
            }
        }

        #endregion

        #region Filtering

        internal static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int index = -1;
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                if (predicate(value, index))
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
        {
            foreach (object value in source)
            {
                if (value is TResult)
                {
                    yield return (TResult)value; // Deferred execution.
                }
            }
        }

        #endregion

        #region Mapping

        internal static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value); // Deferred execution.
            }
        }

        internal static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            int index = -1;
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                yield return selector(value, index); // Deferred execution.
            }
        }

        internal static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TResult>> selector)
        {
            foreach (TSource value in source)
            {
                foreach (TResult result in selector(value))
                {
                    yield return result; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            foreach (TSource sourceValue in source)
            {
                foreach (TCollection collectionValue in collectionSelector(sourceValue))
                {
                    yield return resultSelector(sourceValue, collectionValue); // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TResult>> selector)
        {
            int index = -1;
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                foreach (TResult result in selector(value, index))
                {
                    yield return result; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            int index = -1;
            foreach (TSource sourceValue in source)
            {
                index = checked(index + 1);
                foreach (TCollection collectionValue in collectionSelector(sourceValue, index))
                {
                    yield return resultSelector(sourceValue, collectionValue); // Deferred execution.
                }
            }
        }

        #endregion

        #region Grouping

        internal static IEnumerable<IGrouping<TKey, TSource>> GroupByWithToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) => source.ToLookup(keySelector, comparer);

        internal static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null)
        {
            ILookup<TKey, TSource> lookup = source.ToLookup(keySelector, comparer); // Eager evaluation.
            foreach (IGrouping<TKey, TSource> group in lookup)
            {
                yield return group; // Deferred execution.
            }
        }

        internal static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            ILookup<TKey, TElement> lookup = source.ToLookup(keySelector, elementSelector, comparer); // Eager evaluation.
            foreach (IGrouping<TKey, TElement> group in lookup)
            {
                yield return group; // Deferred execution.
            }
        }

        internal static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            ILookup<TKey, TSource> lookup = source.ToLookup(keySelector, comparer); // Eager evaluation.
            foreach (IGrouping<TKey, TSource> group in lookup)
            {
                yield return resultSelector(group.Key, group); // Deferred execution.
            }
        }

        internal static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            ILookup<TKey, TElement> lookup = source.ToLookup(keySelector, elementSelector, comparer); // Eager evaluation.
            foreach (IGrouping<TKey, TElement> group in lookup)
            {
                yield return resultSelector(group.Key, group); // Deferred execution.
            }
        }

        #endregion

        #region Join

        internal static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            ILookup<TKey, TInner> innerLookup = inner.ToLookup(innerKeySelector, comparer); // Eager evaluation.
            foreach (TOuter outerValue in outer)
            {
                TKey key = outerKeySelector(outerValue);
                if (innerLookup.Contains(key))
                {
                    foreach (TInner innerValue in innerLookup[key])
                    {
                        yield return resultSelector(outerValue, innerValue); // Deferred execution.
                    }
                }
            }
        }

        internal static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            ILookup<TKey, TInner> innerLookup = inner.ToLookup(innerKeySelector, comparer); // Eager evaluation.
            foreach (TOuter outerValue in outer)
            {
                yield return resultSelector(outerValue, innerLookup[outerKeySelector(outerValue)]); // Deferred execution.
            }
        }

        #endregion

        #region Concatenation

        internal static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            foreach (TSource value in first)
            {
                yield return value; // Deferred execution.
            }

            foreach (TSource value in second)
            {
                yield return value; // Deferred execution.
            }
        }

        #endregion

        #region Set

        internal static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            foreach (TSource value in source)
            {
                if (hashSet.Add(value))
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> DistinctWithWhere<TSource>(
            this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return source.Where(hashSet.Add); // Deferred execution.
        }

        internal static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            foreach (TSource firstValue in first)
            {
                if (hashSet.Add(firstValue))
                {
                    yield return firstValue; // Deferred execution.
                }
            }

            foreach (TSource secondValue in second)
            {
                if (hashSet.Add(secondValue))
                {
                    yield return secondValue; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> UnionWithWhere<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return first.Where(hashSet.Add).Concat(second.Where(hashSet.Add)); // Deferred execution.
        }

        internal static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            HashSet<TSource> secondHashSet = new HashSet<TSource>(comparer);
            foreach (TSource secondValue in second)
            {
                secondHashSet.Add(secondValue); // Eager evaluation.
            }

            // return first.Where(secondHashSet.Add);
            foreach (TSource firstValue in first)
            {
                if (secondHashSet.Add(firstValue))
                {
                    yield return firstValue; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> IntersectWithAdd<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> secondHashSet = new HashSet<TSource>(comparer);
            foreach (TSource secondValue in second)
            {
                secondHashSet.Add(secondValue); // Eager evaluation.
            }

            HashSet<TSource> firstHashSet = new HashSet<TSource>(comparer);
            foreach (TSource firstValue in first)
            {
                if (secondHashSet.Add(firstValue))
                {
                    firstHashSet.Add(firstValue);
                }
                else if (firstHashSet.Add(firstValue))
                {
                    yield return firstValue; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> secondHashSet = new HashSet<TSource>(comparer);
            foreach (TSource secondValue in second)
            {
                secondHashSet.Add(secondValue); // Eager evaluation.
            }

            // return first.Where(secondHashSet.Remove);
            foreach (TSource firstValue in first)
            {
                if (secondHashSet.Remove(firstValue))
                {
                    yield return firstValue; // Deferred execution.
                }
            }
        }

        #endregion

        #region Convolution

        internal static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
        {
            using (IEnumerator<TFirst> firstIterator = first.GetEnumerator())
            using (IEnumerator<TSecond> secondIterator = second.GetEnumerator())
            {
                while (firstIterator.MoveNext() && secondIterator.MoveNext())
                {
                    yield return resultSelector(firstIterator.Current, secondIterator.Current); // Deferred execution.
                }
            }
        }

        #endregion

        #region Partioning

        internal static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (count > 0 && iterator.MoveNext())
                {
                    count--; // Comparing foreach loop, iterator.Current is never called.
                }

                if (count <= 0)
                {
                    while (iterator.MoveNext())
                    {
                        yield return iterator.Current; // Deferred execution.
                    }
                }
            }
        }

        internal static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool skip = true;
            foreach (TSource value in source)
            {
                if (skip && !predicate(value))
                {
                    skip = false;
                }

                if (!skip)
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int index = -1;
            bool skip = true;
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                if (skip && !predicate(value, index))
                {
                    skip = false;
                }

                if (!skip)
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        internal static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (count > 0)
            {
                foreach (TSource value in source)
                {
                    yield return value; // Deferred execution.
                    if (--count == 0)
                    {
                        break;
                    }
                }
            }
        }

        internal static IEnumerable<TSource> TakeWithWhere<TSource>
            (this IEnumerable<TSource> source, int count) => 
                count > 0 ? source.Where((_, index) => index < count) : Empty<TSource>();

        internal static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (!predicate(value))
                {
                    break;
                }

                yield return value; // Deferred execution.
            }
        }

        internal static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int index = -1;
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                if (!predicate(value, index))
                {
                    break;
                }

                yield return value; // Deferred execution.
            }
        }

        #endregion

        #region Element

        internal static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (iterator.MoveNext())
                    {
                        return iterator.Current;
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains no elements.");
        }

        internal static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    return value;
                }
            }

            throw new InvalidOperationException("Sequence contains no matching element.");
        }

        internal static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (iterator.MoveNext())
                    {
                        return iterator.Current;
                    }
                }
            }

            return default(TSource);
        }

        internal static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    return value;
                }
            }

            return default(TSource);
        }

        internal static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (iterator.MoveNext())
                    {
                        TSource last;
                        do
                        {
                            last = iterator.Current;
                        }
                        while (iterator.MoveNext());
                        return last;
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains no elements.");
        }

        internal static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                for (int index = list.Count - 1; index >= 0; index--)
                {
                    TSource value = list[index];
                    if (predicate(value))
                    {
                        return value;
                    }
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        TSource last = iterator.Current;
                        if (predicate(last))
                        {
                            while (iterator.MoveNext())
                            {
                                TSource value = iterator.Current;
                                if (predicate(value))
                                {
                                    last = value;
                                }
                            }

                            return last;
                        }
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains no matching element.");
        }

        internal static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (iterator.MoveNext())
                    {
                        TSource last;
                        do
                        {
                            last = iterator.Current;
                        }
                        while (iterator.MoveNext());
                        return last;
                    }
                }
            }

            return default(TSource);
        }

        internal static TSource LastOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                for (int index = list.Count - 1; index >= 0; index--)
                {
                    TSource value = list[index];
                    if (predicate(value))
                    {
                        return value;
                    }
                }

                return default(TSource);
            }

            TSource last = default(TSource);
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    last = value;
                }
            }

            return last;
        }

        internal static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                return list[index];
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (index-- == 0)
                    {
                        return iterator.Current;
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        internal static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (index >= 0)
            {
                IList<TSource> list = source as IList<TSource>;
                if (list != null)
                {
                    if (index < list.Count)
                    {
                        return list[index];
                    }
                }
                else
                {
                    using (IEnumerator<TSource> iterator = source.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (index-- == 0)
                            {
                                return iterator.Current;
                            }
                        }
                    }
                }
            }

            return default(TSource);
        }

        internal static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                switch (list.Count)
                {
                    case 0:
                        throw new InvalidOperationException("Sequence contains no elements.");
                    case 1:
                        return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (!iterator.MoveNext()) // source is empty.
                    {
                        throw new InvalidOperationException("Sequence contains no elements.");
                    }

                    TSource first = iterator.Current;
                    if (!iterator.MoveNext())
                    {
                        return first;
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains more than one element.");
        }

        internal static TSource Single<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    TSource value = iterator.Current;
                    if (predicate(value))
                    {
                        while (iterator.MoveNext())
                        {
                            if (predicate(iterator.Current))
                            {
                                throw new InvalidOperationException("Sequence contains more than one matching element.");
                            }
                        }

                        return value;
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains no matching element.");
        }

        internal static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                switch (list.Count)
                {
                    case 0:
                        return default(TSource);
                    case 1:
                        return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (iterator.MoveNext())
                    {
                        TSource first = iterator.Current;
                        if (!iterator.MoveNext())
                        {
                            return first;
                        }
                    }
                    else
                    {
                        return default(TSource);
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains more than one element.");
        }

        internal static TSource SingleOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    TSource value = iterator.Current;
                    if (predicate(value))
                    {
                        while (iterator.MoveNext())
                        {
                            if (predicate(iterator.Current))
                            {
                                throw new InvalidOperationException("Sequence contains more than one matching element.");
                            }
                        }

                        return value;
                    }
                }
            }
            return default(TSource);
        }

        #endregion

        #region Aggregation

        internal static TResult Aggregate<TSource, TAccumulate, TResult>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector)
        {
            TAccumulate accumulate = seed;
            foreach (TSource value in source)
            {
                accumulate = func(accumulate, value);
            }

            return resultSelector(accumulate);
        }

        internal static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            TAccumulate accumulate = seed;
            foreach (TSource value in source)
            {
                accumulate = func(accumulate, value);
            }

            return accumulate;
        }

        internal static TSource Aggregate<TSource>(
            this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                TSource accumulate = iterator.Current;
                while (iterator.MoveNext())
                {
                    accumulate = func(accumulate, iterator.Current);
                }

                return accumulate;
            }
        }

        internal static int Count<TSource>(this IEnumerable<TSource> source)
        {
            ICollection<TSource> genericCollection = source as ICollection<TSource>;
            if (genericCollection != null)
            {
                return genericCollection.Count;
            }

            ICollection collection = source as ICollection;
            if (collection != null)
            {
                return collection.Count;
            }

            int count = 0;
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    count = checked(count + 1); // Comparing foreach loop, iterator.Current is never called.
                }
            }

            return count;
        }

        internal static int Count<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int count = 0;
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    count = checked(count + 1);
                }
            }
            return count;
        }

        internal static long LongCount<TSource>(this IEnumerable<TSource> source)
        {
            long count = 0L;
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    count = checked(count + 1L); // Comparing foreach loop, iterator.Current is never called.
                }
            }

            return count;
        }

        internal static long LongCount<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            long count = 0L;
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    count = checked(count + 1L);
                }
            }
            return count;
        }

        internal static decimal Min(this IEnumerable<decimal> source)
        {
            decimal min;
            using (IEnumerator<decimal> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                min = iterator.Current;
                while (iterator.MoveNext())
                {
                    decimal value = iterator.Current;
                    if (value < min)
                    {
                        min = value;
                    }
                }
            }

            return min;
        }

        internal static decimal Min<TSource>
            (this IEnumerable<TSource> source, Func<TSource, decimal> selector) => source.Select(selector).Min();

        internal static decimal Max(this IEnumerable<decimal> source)
        {
            decimal max;
            using (IEnumerator<decimal> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                max = iterator.Current;
                while (iterator.MoveNext())
                {
                    decimal value = iterator.Current;
                    if (value > max)
                    {
                        max = value;
                    }
                }
            }

            return max;
        }

        internal static decimal Max<TSource>
            (this IEnumerable<TSource> source, Func<TSource, decimal> selector) => source.Select(selector).Max();

        internal static decimal Sum(this IEnumerable<decimal> source)
        {
            decimal sum = 0;
            foreach (decimal value in source)
            {
                sum += value;
            }

            return sum;
        }

        internal static decimal Sum<TSource>
            (this IEnumerable<TSource> source, Func<TSource, decimal> selector) => source.Select(selector).Sum();

        internal static decimal Average(this IEnumerable<decimal> source)
        {
            using (IEnumerator<decimal> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                decimal sum = iterator.Current;
                long count = 1L;
                while (iterator.MoveNext())
                {
                    sum += iterator.Current;
                    count++;
                }

                return sum / count;
            }
        }

        internal static decimal Average<TSource>
            (this IEnumerable<TSource> source, Func<TSource, decimal> selector) => source.Select(selector).Average();

        #endregion

        #region Quantifiers

        internal static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (!predicate(value))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext()) // Comparing foreach loop, iterator.Current is never called.
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            foreach (TSource sourceValue in source)
            {
                if (comparer.Equals(sourceValue, value))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Equality

        internal static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            using (IEnumerator<TSource> firstIterator = first.GetEnumerator())
            using (IEnumerator<TSource> secondIterator = second.GetEnumerator())
            {
                while (firstIterator.MoveNext())
                {
                    if (!secondIterator.MoveNext() || !comparer.Equals(firstIterator.Current, secondIterator.Current))
                    {
                        return false;
                    }
                }

                if (secondIterator.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
