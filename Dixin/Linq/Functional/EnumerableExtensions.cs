namespace Dixin.Linq.Functional
{
    using System;
    using System.Collections.Generic;

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TSource> Defer<TSource>
            (this Func<IEnumerable<TSource>> factory) => new Sequence<TSource>(factory);

        public static IEnumerable<TResult> Defer<TSource, TResult>(
            this IEnumerable<TSource> source, Func<IEnumerator<TSource>, IEnumerable<TResult>> func) =>
                new Sequence<TResult>(() =>
                {
                    using (IEnumerator<TSource> iterator = source.GetEnumerator())
                    {
                        return func(iterator);
                    }
                });

        public static IEnumerable<TSource> Sequence<TSource>
            (this IEnumerator<TSource> iterator) => new Sequence<TSource>(iterator);

        public static IEnumerable<TSource> Sequence<TSource>
            (TSource value, bool hasValue = true) => hasValue ? new TSource[] { value } : Empty<TSource>();

        public static IEnumerable<TSource> Concat<TSource>(this TSource head, IEnumerable<TSource> tail) =>
            new Concat<TSource>(Sequence(head), tail);

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> initial, TSource last) =>
            new Concat<TSource>(initial, Sequence(last));

        private static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerator<TSource> iterator,
            TAccumulate accumulation,
            Func<TAccumulate, TSource, int, TAccumulate> func,
            int index) => iterator.MoveNext()
                ? iterator.Aggregate(func(accumulation, iterator.Current, index), func, checked(index + 1))
                : accumulation;

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, int, TAccumulate> func)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                return iterator.Aggregate(seed, func, 0);
            }
        }

        public static IEnumerable<TResult> DeferredAggregate<TSource, TResult>(
            this IEnumerable<TSource> source,
            IEnumerable<TResult> seed,
            Func<IEnumerable<TResult>, TSource, int, IEnumerable<TResult>> func) =>
                new Sequence<TResult>(() => source.Aggregate(seed, func));

        public static IEnumerable<TResult> DeferredAggregate<TSource, TResult>(
            this IEnumerable<TSource> source,
            IEnumerable<TResult> seed,
            Func<IEnumerable<TResult>, TSource, IEnumerable<TResult>> func) =>
                source.DeferredAggregate(seed, (accumulation, value, index) => func(accumulation, value));

        public static void Iterate<TSource>
            (this IEnumerable<TSource> source, Action<TSource, int> onNext) =>
                source.Aggregate((object)null, (@null, value, index) =>
                    {
                        onNext(value, index);
                        return @null;
                    });

        public static void Iterate<TSource>
            (this IEnumerable<TSource> source, Action<TSource> onNext) =>
                source.Iterate((value, index) => onNext(value));
    }
}
