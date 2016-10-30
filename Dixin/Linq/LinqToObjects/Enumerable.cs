#if DEMO
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Linq
{
    public static class EnumerableEx
    {
        public static IEnumerable<TSource> DistinctUntilChanged<TSource>(this IEnumerable<TSource> source);

        public static IEnumerable<TSource> DistinctUntilChanged<TSource>(
            this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer);

        public static IEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer);

        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count);

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count);

        public static TSource Min<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer);

        public static TSource Max<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer);

        public static IList<TSource> MinBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IList<TSource> MinBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer);

        public static IList<TSource> MinBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IList<TSource> MinBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer);

        public static IEnumerable<IList<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, int count);

        public static IEnumerable<IList<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, int count, int skip);

        IEnumerable<TResult> Share<TSource, TResult>(
            this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector);

        public static IBuffer<TSource> Publish<TSource>(this IEnumerable<TSource> source);

        public static IEnumerable<TResult> Publish<TSource, TResult>(
            this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector);

        public static IBuffer<TSource> Memoize<TSource>(this IEnumerable<TSource> source);

        public static IEnumerable<TResult> Memoize<TSource, TResult>(
            this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector);

        public static IBuffer<TSource> Memoize<TSource>(
            this IEnumerable<TSource> source, int readerCount);

        public static IEnumerable<TResult> Memoize<TSource, TResult>(
            this IEnumerable<TSource> source, int readerCount, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector);
    }

    public static class Enumerable
    {
        public static IEnumerable<TResult> Empty<TResult>();

        public static IEnumerable<int> Range(int start, int count);

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count);

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector);

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector);

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source, Func<TSource,
            IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector);

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector);

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source);

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector);

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector);

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer);

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer);

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source);

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source,
            TSource defaultValue);

        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source);

        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer);

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer);

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer);

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer);

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector);

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count);

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count);

        public static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate);

        public static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int, bool> predicate);

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source);

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source);

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source);

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source);

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);


        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector);

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector);


        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer);

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer);

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer);

        public static TSource First<TSource>(this IEnumerable<TSource> source);

        public static TSource Last<TSource>(this IEnumerable<TSource> source);

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source);

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static TSource Single<TSource>(this IEnumerable<TSource> source);

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source);

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index);

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index);

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector);

        public static int Count<TSource>(this IEnumerable<TSource> source);

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static long LongCount<TSource>(this IEnumerable<TSource> source);

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static int Min(this IEnumerable<int> source);

        public static int Max(this IEnumerable<int> source);

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector);

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector);

        // int?, long, long?, double, double?, float, float?, decimal, decimal?.

        public static TSource Min<TSource>(this IEnumerable<TSource> source);

        public static TSource Max<TSource>(this IEnumerable<TSource> source);

        public static int Sum(this IEnumerable<int> source);

        public static double Average(this IEnumerable<int> source);

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector);

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector);

        // int?, long, long?, float, float?, double, double?, decimal, decimal.

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static bool Any<TSource>(this IEnumerable<TSource> source);

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value);

        public static bool Contains<TSource>(
            this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer);

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer);

    }
}

namespace System.Collections.Generic
{
    public class Dictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, 
        IDictionary<TKey, TValue>, IDictionary, ICollection<KeyValuePair<TKey, TValue>>, ICollection, 
        IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, 
        ISerializable, IDeserializationCallback
    {
    }
}

namespace System.Linq
{
    public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        IEnumerable<TElement> this[TKey key] { get; }

        int Count { get; }

        bool Contains(TKey key);
    }
}

namespace System.Collections
{
    public interface ICollection : IEnumerable
    {
        int Count { get; }

        object SyncRoot { get; }

        bool IsSynchronized { get; }

        void CopyTo(Array array, int index);
    }
}

namespace System.Collections.Generic
{
    public interface ICollection<T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }

        bool IsReadOnly { get; }

        void Add(T item);

        void Clear();

        bool Contains(T item);

        void CopyTo(T[] array, int arrayIndex);

        bool Remove(T item);
    }
}

namespace System.Collections.Generic
{
    public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        T this[int index] { get; set; }

        int IndexOf(T item);
        void Insert(int index, T item);
        void RemoveAt(int index);
    }
}

#endif