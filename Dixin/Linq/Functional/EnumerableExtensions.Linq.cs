namespace Dixin.Linq.Functional
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.FSharp.Linq.RuntimeHelpers;

    public static partial class EnumerableExtensions
    {
        public static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func) =>
                source.Aggregate(seed, (accumulate, value, index) => func(accumulate, value));

        public static TResult Aggregate<TSource, TAccumulate, TResult>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector) => resultSelector(source.Aggregate(seed, func));

        public static TSource Aggregate<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, TSource> func)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                return iterator.Aggregate(
                    iterator.Current, (accumulate, value, index) => func(accumulate, value), 1);
            }
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            !source.Any(value => !predicate(value));

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Any<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate).Any();

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) => source;

        private static IEnumerable<TResult> Cast<TResult>(this IEnumerator iterator) => iterator.MoveNext()
            ? ((TResult)iterator.Current).Concat(iterator.Cast<TResult>())
            : Empty<TResult>();

        public static IEnumerable<TResult> Cast<TResult>
            (this IEnumerable source) => Defer(() => source.GetEnumerator().Cast<TResult>());

        public static IEnumerable<TSource> Concat<TSource>
            (this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                new Concat<TSource>(first, second);

        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer = null) => source.Any(
                sourceValue => (comparer ?? EqualityComparer<TSource>.Default).Equals(sourceValue, value));

        public static int Count<TSource>(this IEnumerable<TSource> source)
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

            return source.Aggregate(0, (count, value) => checked(count + 1));
        }

        public static int Count<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate).Count();

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source,
            TSource defaultValue = default(TSource)) => source.Defer(iterator =>
                iterator.MoveNext()
                    ? iterator.Current.Concat(iterator.Sequence())
                    : Sequence(defaultValue));

        public static IEnumerable<TSource> Distinct<TSource>
            (this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null) => Defer(() =>
                {
                    LinqToObjects.HashSet<TSource> hashSet = new LinqToObjects.HashSet<TSource>(comparer);
                    return source.Where(hashSet.Add);
                });

        private static Tuple<bool, TSource> ElementAt<TSource>(
            this IEnumerator<TSource> iterator,
            int targetIndex,
            int lastIndex)
        {
            if (iterator.MoveNext())
            {
                int currentIndex = lastIndex + 1;
                return currentIndex == targetIndex
                    ? Tuple.Create(true, iterator.Current)
                    : iterator.ElementAt(targetIndex, currentIndex);
            }

            return Tuple.Create(false, default(TSource));
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
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
                Tuple<bool, TSource> result = iterator.ElementAt(index, -1);
                if (result.Item1)
                {
                    return result.Item2;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
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
                        return iterator.ElementAt(index, -1).Item2;
                    }
                }
            }

            return default(TSource);
        }

        public static IEnumerable<TResult> Empty<TResult>() => Array.Empty<TResult>();

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null) => Defer(() =>
                {
                    comparer = comparer ?? EqualityComparer<TSource>.Default;
                    LinqToObjects.HashSet<TSource> hashSet = new LinqToObjects.HashSet<TSource>(comparer);
                    foreach (TSource secondValue in second)
                    {
                        hashSet.Add(secondValue);
                    }

                    return first.Where(hashSet.Add);
                });

        public static TSource First<TSource>(this IEnumerable<TSource> source)
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

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            source.Where(predicate).First();

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
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

        public static TSource FirstOrDefault<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                source.Where(predicate).FirstOrDefault();

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null) => Defer(() =>
                source.ToLookup(keySelector, elementSelector, comparer));

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) =>
                source.GroupBy(keySelector, value => value, comparer);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null) => source
                .GroupBy(keySelector, elementSelector, comparer)
                .Select(group => resultSelector(group.Key, group));

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null) =>
                source.GroupBy(keySelector, value => value, resultSelector, comparer);

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null) => Defer(() =>
                {
                    ILookup<TKey, TInner> lookup = inner.ToLookup(innerKeySelector, comparer);
                    return outer.Select(value => resultSelector(value, lookup[outerKeySelector(value)]));
                });

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null) => Defer(() =>
                {
                    LinqToObjects.HashSet<TSource> hashSet = new LinqToObjects.HashSet<TSource>(comparer);
                    second.Iterate(secondValue => hashSet.Add(secondValue));
                    return first.Where(hashSet.Remove);
                });

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null) => Defer(() =>
                {
                    ILookup<TKey, TInner> innerLookup = inner.ToLookup(innerKeySelector, comparer);
                    return outer
                        .Select(outerValue => new
                        {
                            Key = outerKeySelector(outerValue),
                            Value = outerValue
                        })
                        .Where(outerValue => innerLookup.Contains(outerValue.Key))
                        .Select(outerValue => innerLookup[outerValue.Key]
                            .Select(innerValue => resultSelector(outerValue.Value, innerValue)))
                        .Aggregate(Empty<TResult>(), (concat, value) => concat.Concat(value));
                });

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
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
                var last = source.Aggregate(
                    new { HasValue = false, Value = default(TSource) },
                    (_, value) => new { HasValue = true, Value = value });
                if (last.HasValue)
                {
                    return last.Value;
                }
            }

            throw new InvalidOperationException("Sequence contains no elements.");
        }

        public static TSource Last<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate).Last();

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                return count > 0 ? list[count - 1] : default(TSource);
            }

            return source.Aggregate(default(TSource), (last, value) => value);
        }

        public static TSource LastOrDefault<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                source.Where(predicate).LastOrDefault();

        public static IEnumerable<TResult> OfType<TResult>
            (this IEnumerable source) =>
                source.Cast<object>().Where(value => value is TResult).Select(value => (TResult)value);

        public static IEnumerable<int> Range
            (int start, int count) => count > 0 ? start.Concat(Range(checked(start + 1), count - 1)) : Empty<int>();

        public static IEnumerable<TResult> Repeat<TResult>
            (TResult element, int count) => count > 0
                ? element.Concat(Repeat(element, count - 1))
                : Empty<TResult>();

        public static IEnumerable<TSource> Reverse<TSource>
            (this IEnumerable<TSource> source) =>
                source.DeferredAggregate(Empty<TSource>(), (reverse, value) => value.Concat(reverse));

        public static IEnumerable<TResult> Select<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) =>
                source.DeferredAggregate(Empty<TResult>(), (select, value, index) => select.Concat(selector(value, index)));

        public static IEnumerable<TResult> Select<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                source.Select((value, index) => selector(value));

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TResult>> selector) => source
                .Select(selector)
                .DeferredAggregate(Empty<TResult>(), (concat, value) => concat.Concat(value));

        public static IEnumerable<TResult> SelectMany<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
                source.SelectMany((value, index) => selector(value));

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector) => source
                .Select((sourceValue, index) => collectionSelector(sourceValue, index)
                    .Select(collectionValue => resultSelector(sourceValue, collectionValue)))
                .DeferredAggregate(Empty<TResult>(), (concat, value) => concat.Concat(value));

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector) =>
                source.SelectMany((value, index) => collectionSelector(value), resultSelector);

        private static bool SequenceEqual<TSource>(
            this IEnumerator<TSource> first,
            IEnumerator<TSource> second,
            IEqualityComparer<TSource> comparer) => first.MoveNext()
                ? second.MoveNext() && comparer.Equals(first.Current, second.Current) &&
                  first.SequenceEqual(second, comparer)
                : !second.MoveNext();

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            using (IEnumerator<TSource> firstItoerator = first.GetEnumerator())
            using (IEnumerator<TSource> secondIterator = second.GetEnumerator())
            {
                return firstItoerator.SequenceEqual(secondIterator, comparer);
            }
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count == 0)
                {
                    throw new InvalidOperationException("Sequence contains no elements.");
                }

                if (count == 1)
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
                        TSource first = iterator.Current;
                        if (!iterator.MoveNext())
                        {
                            return first;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Sequence contains no elements.");
                    }
                }
            }

            throw new InvalidOperationException("Sequence contains more than one element.");
        }

        public static TSource Single<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate).Single();

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count == 0)
                {
                    return default(TSource);
                }

                if (count == 1)
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

        public static TSource SingleOrDefault<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                source.Where(predicate).SingleOrDefault();

        public static IEnumerable<TSource> Skip<TSource>
            (this IEnumerable<TSource> source, int count) => Defer(() => source.Aggregate(
                 new { Result = Empty<TSource>(), Count = count },
                 (current, value) => current.Count > 0
                     ? new { Result = current.Result, Count = current.Count - 1 }
                     : new { Result = current.Result.Concat(value), Count = current.Count }).Result);

        public static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => Defer(() => source.Aggregate(
                new { Result = Empty<TSource>(), Take = false },
                (current, value, index) => current.Take
                    ? new { Result = current.Result.Concat(value), Take = true }
                    : (predicate(value, index)
                        ? new { Result = current.Result, Take = false }
                        : new { Result = current.Result.Concat(value), Take = true })).Result);

        public static IEnumerable<TSource> SkipWhile<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                source.SkipWhile((value, index) => predicate(value));

        private static IEnumerable<TSource> Take<TSource>
            (this IEnumerator<TSource> iterator, int count) => count > 0 && iterator.MoveNext()
                ? iterator.Current.Concat(iterator.Take(count - 1))
                : Empty<TSource>();

        public static IEnumerable<TSource> Take<TSource>
            (this IEnumerable<TSource> source, int count) =>
                source.Defer(iterator => iterator.Take(count));

        private static IEnumerable<TSource> TakeWhile<TSource>
            (this IEnumerator<TSource> iterator, Func<TSource, int, bool> predicate, int index) => iterator.MoveNext()
                ? (predicate(iterator.Current, index)
                    ? iterator.Current.Concat(iterator.TakeWhile(predicate, index + 1))
                    : Empty<TSource>())
                : Empty<TSource>();

        public static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) =>
                source.Defer(iterator => iterator.TakeWhile(predicate, 0));

        public static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate) =>
                source.TakeWhile((value, index) => predicate(value));

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
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

            const int InitialLength = 4;
            var result = source.Aggregate(
                new { Length = 0, Array = new TSource[InitialLength] },
                (current, value) =>
                {
                    int length = current.Length;
                    TSource[] array = current.Array;
                    if (length == array.Length)
                    {
                        Array.Resize(ref array, checked(length * 2)); // Doubles size when full.
                        array[length] = value;
                        return new { Length = length + 1, Array = array };
                    }

                    array[length] = value;
                    return new { Length = length + 1, Array = array };
                });

            TSource[] resultArray = result.Array;
            Array.Resize(ref resultArray, result.Length); // Finalizes size when done.
            return resultArray;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
            source.Iterate(value => dictionary.Add(keySelector(value), elementSelector(value)));
            return dictionary;
        }

        public static List<TSource> ToList<TSource>
            (this IEnumerable<TSource> source) => new List<TSource>(source);

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, List<TElement>> groupsWithNonNullKey = new Dictionary<TKey, List<TElement>>(comparer);
            List<TElement> groupWithNullKey = new List<TElement>();
            source.Iterate(value =>
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
                });

            return new Lookup<TKey, TElement>(
                groupsWithNonNullKey.ToDictionary(
                    group => group.Key,
                    group => new Grouping<TKey, TElement>(group.Key, group.Value) as IGrouping<TKey, TElement>,
                    comparer),
                new Grouping<TKey, TElement>(default(TKey), groupWithNullKey),
                groupWithNullKey.Count > 0);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null) => source.ToLookup(keySelector, value => value, comparer);

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer = null) => Defer(() =>
                {
                    LinqToObjects.HashSet<TSource> hashSet = new LinqToObjects.HashSet<TSource>(comparer);
                    return first.Where(hashSet.Add).Concat(second.Where(hashSet.Add));
                });

        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => source.DeferredAggregate(
                Empty<TSource>(),
                (filter, value, index) => predicate(value, index) ? filter.Concat(value) : filter);

        public static IEnumerable<TSource> Where<TSource>
            (this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                source.Where((value, index) => predicate(value));

        private static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerator<TFirst> first,
            IEnumerator<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector) => first.MoveNext() && second.MoveNext()
                ? resultSelector(first.Current, second.Current).Concat(Zip(first, second, resultSelector))
                : Empty<TResult>();


        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector) => Defer(() =>
                {
                    using (IEnumerator<TFirst> firstIterator = first.GetEnumerator())
                    using (IEnumerator<TSecond> secondIterator = second.GetEnumerator())
                    {
                        return firstIterator.Zip(secondIterator, resultSelector);
                    }
                });
    }
}
