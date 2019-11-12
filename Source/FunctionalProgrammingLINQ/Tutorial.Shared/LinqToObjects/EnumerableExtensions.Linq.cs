﻿namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

#if DEMO
    internal static partial class EnumerableExtensions
    {
        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            TSource[] array = new TSource[0];
            foreach (TSource value in source)
            {
                Array.Resize(ref array, array.Length + 1);
                array[array.Length - 1] = value;
            }
            return array;
        }
    }
#endif

    internal static partial class EnumerableExtensions
    {
        #region Conversion

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            if (source is ICollection<TSource> genericCollection)
            {
                int length = genericCollection.Count;
                if (length > 0)
                {
                    TSource[] array = new TSource[length];
                    genericCollection.CopyTo(array, 0);
                    return array;
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (iterator.MoveNext())
                    {
                        const int InitialLength = 4; // Initial array length.
                        const int MaxLength = 0x7FEFFFFF; // Max array length: Array.MaxArrayLength.
                        TSource[] array = new TSource[InitialLength];
                        array[0] = iterator.Current;
                        int usedLength = 1;

                        while (iterator.MoveNext())
                        {
                            if (usedLength == array.Length)
                            {
                                int increaseToLength = usedLength * 2; // Array is full, double its length.
                                if ((uint)increaseToLength > MaxLength)
                                {
                                    increaseToLength = MaxLength <= usedLength ? usedLength + 1 : MaxLength;
                                }
                                Array.Resize(ref array, increaseToLength);
                            }
                            array[usedLength++] = iterator.Current;
                        }
                        Array.Resize(ref array, usedLength); // Consolidate array to its actual length.
                        return array;
                    }
                }
            }
            return Array.Empty<TSource>();
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) => new List<TSource>(source);

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) =>
                source.ToDictionary(keySelector, value => value, comparer);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
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

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null) =>
                new Lookup<TKey, TElement>(comparer).AddRange(source, keySelector, elementSelector);

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) =>
                source.ToLookup(keySelector, value => value, comparer);

        #endregion

        #region Conversion

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) =>
            source; // Deferred execution.

#if DEMO
        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            foreach (object value in source)
            {
                yield return (TResult)value; // Deferred execution.
            }
        }

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            if (source is IEnumerable<TResult> genericSource)
            {
                return genericSource; // Cannot be compiled.
            }
            foreach (object value in source)
            {
                yield return (TResult)value; // Deferred execution.
            }
        }
#endif

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            IEnumerable<TResult> CastGenerator()
            {
                foreach (object value in source)
                {
                    yield return (TResult)value; // Deferred execution.
                }
            }
            return source is IEnumerable<TResult> genericSource
                ? genericSource
                : CastGenerator(); // Deferred execution.
        }

        #endregion

        #region Generation

        public static IEnumerable<TResult> Empty<TResult>() => Array.Empty<TResult>();

        public static IEnumerable<TResult> EmptyWithYield<TResult>()
        {
            yield break;
        }

        public static IEnumerable<int> Range(int start, int count)
        {
            if (count < 0 || (long)start + count - 1L > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            IEnumerable<int> RangeGenerator()
            {
                int end = start + count;
                for (int value = start; value != end; value++)
                {
                    yield return value; // Deferred execution.
                }
            }
            return RangeGenerator();
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            IEnumerable<TResult> RepeatGenerator()
            {
                for (int index = 0; index < count; index++)
                {
                    yield return element; // Deferred execution.
                }
            }
            return RepeatGenerator();
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source, TSource defaultValue = default)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    // source is not empty.
                    do
                    {
                        yield return iterator.Current; // Deferred execution.
                    }
                    while (iterator.MoveNext());
                }
                else
                {
                    // source is empty.
                    yield return defaultValue; // Deferred execution.
                }
            }
        }

        #endregion

        #region Filtering

#if DEMO
        public static IEnumerable<TSource> Where<TSource>(
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
#endif

        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate) =>
                new Generator<TSource, IEnumerator<TSource>>(
                    data: null,
                    iteratorFactory: sourceIterator => new Iterator<TSource>(
                        start: () => sourceIterator = source.GetEnumerator(),
                        moveNext: () =>
                        {
                            while (sourceIterator.MoveNext())
                            {
                                if (predicate(sourceIterator.Current))
                                {
                                    return true;
                                }
                            }
                            return false;
                        },
                        getCurrent: () => sourceIterator.Current,
                        dispose: () => sourceIterator?.Dispose(),
                        ignoreException: true,
                        resetCurrent: true));

        public static IEnumerable<TSource> Where<TSource>(
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

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
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

#if DEMO
        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value); // Deferred execution.
            }
        }
#endif

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return new Generator<TResult, IEnumerator<TSource>>(
                data: null,
                iteratorFactory: sourceIterator => new Iterator<TResult>(
                    start: () => sourceIterator = source.GetEnumerator(),
                    moveNext: () => sourceIterator.MoveNext(),
                    getCurrent: () => selector(sourceIterator.Current),
                    dispose: () => sourceIterator?.Dispose(),
                    ignoreException: true,
                    resetCurrent: true));
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            int index = -1;
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                yield return selector(value, index); // Deferred execution.
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
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

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
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

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
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

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
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

        public static IEnumerable<IGrouping<TKey, TSource>> GroupByWithToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) =>
                source.ToLookup(keySelector, comparer);

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
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

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
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

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
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

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
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

        public static IEnumerable<TResult> JoinWithToLookup<TOuter, TInner, TKey, TResult>(
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

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            using (IEnumerator<TOuter> outerIterator = outer.GetEnumerator())
            {
                if (outerIterator.MoveNext())
                {
                    Lookup<TKey, TInner> innerLookup = new Lookup<TKey, TInner>(comparer).AddRange(
                        inner, innerKeySelector, innerValue => innerValue, skipNullKey: true); // Eager evaluation.
                    if (innerLookup.Count > 0)
                    {
                        do
                        {
                            TOuter outerValue = outerIterator.Current;
                            TKey key = outerKeySelector(outerValue);
                            if (innerLookup.Contains(key))
                            {
                                foreach (TInner innerValue in innerLookup[key])
                                {
                                    yield return resultSelector(outerValue, innerValue); // Deferred execution.
                                }
                            }
                        }
                        while (outerIterator.MoveNext());
                    }
                }
            }
        }

        public static IEnumerable<TResult> GroupJoinWithToLookup<TOuter, TInner, TKey, TResult>(
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

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            using (IEnumerator<TOuter> outerIterator = outer.GetEnumerator())
            {
                if (outerIterator.MoveNext())
                {
                    Lookup<TKey, TInner> innerLookup = new Lookup<TKey, TInner>(comparer).AddRange(
                        inner, innerKeySelector, innerValue => innerValue, skipNullKey: true); // Eager evaluation.
                    do
                    {
                        TOuter outerValue = outerIterator.Current;
                        yield return resultSelector(outerValue, innerLookup[outerKeySelector(outerValue)]); // Deferred execution.
                    }
                    while (outerIterator.MoveNext());
                }
            }
        }

        #endregion

        #region Concatenation

        public static IEnumerable<TSource> Concat<TSource>(
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

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            foreach (TSource value in source)
            {
                yield return value;
            }
            yield return element;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            yield return element;
            foreach (TSource value in source)
            {
                yield return value;
            }
        }

        #endregion

        #region Set

        public static IEnumerable<TSource> Distinct<TSource>(
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

        public static IEnumerable<TSource> DistinctWithWhere<TSource>(
            this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return source.Where(hashSet.Add); // Deferred execution.
        }

        public static IEnumerable<TSource> Union<TSource>(
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

        public static IEnumerable<TSource> UnionWithWhere<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> hashSet = new HashSet<TSource>(comparer);
            return first.Where(hashSet.Add).Concat(second.Where(hashSet.Add)); // Deferred execution.
        }

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> secondHashSet = new HashSet<TSource>(comparer).AddRange(second); // Eager evaluation.
            foreach (TSource firstValue in first)
            {
                if (secondHashSet.Add(firstValue))
                {
                    yield return firstValue; // Deferred execution.
                }
            }
        }

        public static IEnumerable<TSource> IntersectWithAdd<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> secondHashSet = new HashSet<TSource>(comparer).AddRange(second); // Eager evaluation.
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

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            HashSet<TSource> secondHashSet = new HashSet<TSource>(comparer).AddRange(second); // Eager evaluation.
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

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
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

        #region Partitioning

#if DEMO
        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            foreach (TSource value in source)
            {
                if (count > 0)
                {
                    count--;
                }
                else
                {
                    yield return value;
                }
            }
        }
#endif

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (count > 0 && iterator.MoveNext())
                {
                    count--; // Comparing foreach loop, iterator.Current is not called.
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

        public static IEnumerable<TSource> SkipWhile<TSource>(
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

        public static IEnumerable<TSource> SkipWhile<TSource>(
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

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
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

        public static IEnumerable<TSource> TakeWithWhere<TSource>
            (this IEnumerable<TSource> source, int count) =>
                count > 0 ? source.Where((_, index) => index < count) : Empty<TSource>();

        public static IEnumerable<TSource> TakeWhile<TSource>(
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

        public static IEnumerable<TSource> TakeWhile<TSource>(
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

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            if (source is IList<TSource> list)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                foreach (TSource value in source)
                {
                    return value;
                }
            }
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
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

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source is IList<TSource> list)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                foreach (TSource value in source)
                {
                    return value;
                }
            }
            return default;
        }

        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    return value;
                }
            }
            return default;
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            if (source is IList<TSource> list)
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

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source is IList<TSource> list)
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

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source is IList<TSource> list)
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

            return default;
        }

        public static TSource LastOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source is IList<TSource> list)
            {
                for (int index = list.Count - 1; index >= 0; index--)
                {
                    TSource value = list[index];
                    if (predicate(value))
                    {
                        return value;
                    }
                }

                return default;
            }

            TSource last = default;
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    last = value;
                }
            }

            return last;
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source is IList<TSource> list)
            {
                return list[index];
            }
            if (index >= 0)
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
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (index >= 0)
            {
                if (source is IList<TSource> list)
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
            return default;
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            if (source is IList<TSource> list)
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

        public static TSource Single<TSource>(
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

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source is IList<TSource> list)
            {
                switch (list.Count)
                {
                    case 0:
                        return default;
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
                        return default;
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains more than one element.");
        }

        public static TSource SingleOrDefault<TSource>(
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
            return default;
        }

        #endregion

        #region Aggregation

        public static TResult Aggregate<TSource, TAccumulate, TResult>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector) =>
                resultSelector(source.Aggregate(seed, func));

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            TAccumulate accumulate = seed;
            foreach (TSource value in source)
            {
                accumulate = func(accumulate, value);
            }
            return accumulate;
        }

        public static TSource Aggregate<TSource>(
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

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case ICollection<TSource> genericCollection:
                    return genericCollection.Count;
                case ICollection collection:
                    return collection.Count;
                default:
                    int count = 0;
                    using (IEnumerator<TSource> iterator = source.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            count = checked(count + 1); // Not needed to call iterator.Current.
                        }
                    }
                    return count;
            }
        }

        public static int Count<TSource>(
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

        public static long LongCount<TSource>(this IEnumerable<TSource> source)
        {
            long count = 0L;
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    count = checked(count + 1L); // Not needed to call iterator.Current.
                }
            }
            return count;
        }

        public static long LongCount<TSource>(
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

        public static double Min(this IEnumerable<double> source)
        {
            double min;
            using (IEnumerator<double> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }
                min = iterator.Current;
                while (iterator.MoveNext())
                {
                    double value = iterator.Current;
                    if (value < min)
                    {
                        min = value;
                    }
                    else if (double.IsNaN(value))
                    {
                        return value;
                    }
                }
            }
            return min;
        }

        public static decimal Max(this IEnumerable<decimal> source)
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

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Comparer<TResult> comparer = Comparer<TResult>.Default;
            TResult min = default;
            if (min == null)
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    do
                    {
                        if (!iterator.MoveNext())
                        {
                            return min;
                        }
                        min = selector(iterator.Current);
                    }
                    while (min == null);

                    while (iterator.MoveNext())
                    {
                        TResult value = selector(iterator.Current);
                        if (value != null && comparer.Compare(value, min) < 0)
                        {
                            min = value;
                        }
                    }
                }
            }
            else
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    if (!iterator.MoveNext())
                    {
                        throw new InvalidOperationException("Sequence contains no elements.");
                    }
                    min = selector(iterator.Current);
                    while (iterator.MoveNext())
                    {
                        TResult value = selector(iterator.Current);
                        if (comparer.Compare(value, min) < 0)
                        {
                            min = value;
                        }
                    }
                }
            }
            return min;
        }


        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) =>
            source.Select(selector).Min();

        public static float Min(this IEnumerable<float> source)
        {
            float min;
            using (IEnumerator<float> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                min = iterator.Current;
                while (iterator.MoveNext())
                {
                    float value = iterator.Current;
                    if (value < min)
                    {
                        min = value;
                    }
                    else if (float.IsNaN(value))
                    {
                        return value;
                    }
                }
            }
            return min;
        }

        public static float? Min(this IEnumerable<float?> source)
        {
            float? min = null;
            using (IEnumerator<float?> iterator = source.GetEnumerator())
            {
                do
                {
                    if (!iterator.MoveNext())
                    {
                        return min;
                    }
                    min = iterator.Current;
                }
                while (!min.HasValue);

                float minValueOrDefault = min.GetValueOrDefault();
                while (iterator.MoveNext())
                {
                    float? value = iterator.Current;
                    if (value.HasValue)
                    {
                        float x = value.GetValueOrDefault();
                        if (x < minValueOrDefault)
                        {
                            minValueOrDefault = x;
                            min = value;
                        }
                        else if (float.IsNaN(x))
                        {
                            return value;
                        }
                    }
                }
            }
            return min;
        }

        public static double? Min(this IEnumerable<double?> source)
        {
            double? min = null;
            using (IEnumerator<double?> iterator = source.GetEnumerator())
            {
                do
                {
                    if (!iterator.MoveNext())
                    {
                        return min;
                    }
                    min = iterator.Current;
                }
                while (!min.HasValue);

                double minValueOrDefault = min.GetValueOrDefault();
                while (iterator.MoveNext())
                {
                    double? value = iterator.Current;
                    if (value.HasValue)
                    {
                        double x = value.GetValueOrDefault();
                        if (x < minValueOrDefault)
                        {
                            minValueOrDefault = x;
                            min = value;
                        }
                        else if (double.IsNaN(x))
                        {
                            return value;
                        }
                    }
                }
            }
            return min;
        }

        public static int Sum(this IEnumerable<int> source)
        {
            int sum = 0;
            foreach (int value in source)
            {
                sum = checked(sum + value);
            }
            return sum;
        }

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) =>
            source.Select(selector).Sum();

        public static long Sum(this IEnumerable<long> source)
        {
            long sum = 0;
            foreach (long value in source)
            {
                sum = checked(sum + value);
            }
            return sum;
        }

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) =>
            source.Select(selector).Sum();

        public static decimal Sum(this IEnumerable<decimal> source)
        {
            decimal sum = 0;
            foreach (decimal value in source)
            {
                sum += value;
            }
            return sum;
        }

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }
                decimal sum = selector(iterator.Current);
                long count = 1L;
                while (iterator.MoveNext())
                {
                    sum += selector(iterator.Current);
                    count++;
                }
                return sum / count;
            }
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) =>
            source.Select(selector).Average();

        #endregion

        #region Quantifiers

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
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

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
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

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                return iterator.MoveNext(); // Not needed to call iterator.Current.
            }
        }

        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer = null)
        {
            if (comparer == null && source is ICollection<TSource> collection)
            {
                return collection.Contains(value);
            }
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

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            if (first is ICollection<TSource> firstCollection && second is ICollection<TSource> secondCollection
                && firstCollection.Count != secondCollection.Count)
            {
                return false;
            }
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
                return !secondIterator.MoveNext();
            }
        }

        #endregion
    }
}
