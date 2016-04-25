namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    public static partial class Visualizer
    {
        internal const string Parallel = nameof(Parallel);

        internal const string Sequential = nameof(Sequential);

        internal static void Visualize<T>(
            this IEnumerable<T> source, Action<T> action, string span = Sequential, int category = 0)
        {
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                source.ForEach(value =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        action(value);
                    }
                });
            }
        }

        internal static void Visualize<T>(
            this ParallelQuery<T> source, Action<T> action, string span = Parallel, int category = 1)
        {
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                source.ForAll(value =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        action(value);
                    }
                });
            }
        }
    }

    public static partial class Visualizer
    {
        internal static void Visualize<T, TResult>(
            this IEnumerable<T> source, Func<IEnumerable<T>, Func<T, TResult>, IEnumerable<T>> query, Func<T, TResult> func, string span = Sequential, int category = 0)
        {
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                query(
                    source,
                    value =>
                        {
                            using (markerSeries.EnterSpan(category, value.ToString()))
                            {
                                return func(value);
                            }
                        })
                    .ForEach();
            }
        }

        internal static void Visualize<T, TResult>(
            this ParallelQuery<T> source, Func<ParallelQuery<T>, Func<T, TResult>, ParallelQuery<T>> query, Func<T, TResult> func, string span = Parallel, int category = 1)
        {
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                query(
                    source,
                    value =>
                        {
                            using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                            {
                                return func(value);
                            }
                        })
                .ForAll(_ => { });
            }
        }

        internal static ParallelQuery<T> VisualizeQuery<T, TResult>(
            this ParallelQuery<T> source, Func<ParallelQuery<T>, Func<T, TResult>, ParallelQuery<T>> query, Func<T, TResult> func, string span = Parallel)
        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return query(
                source,
                value =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        return func(value);
                    }
                });
        }
    }

    public static partial class Visualizer
    {
        internal static T Visualize<T>(
            this ParallelQuery<T> source,
            Func<ParallelQuery<T>, Func<T, T, T>, T> aggregate,
            Func<T, T, T> func,
            string span = nameof(ParallelEnumerable.Aggregate))
        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return aggregate(
                source,
                (accumulate, value) =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, $"{accumulate} {value}"))
                    {
                        return func(accumulate, value);
                    }
                });
        }

        internal static TAccumulate Visualize<T, TAccumulate>(
            this ParallelQuery<T> source,
            Func<ParallelQuery<T>, TAccumulate, Func<TAccumulate, T, TAccumulate>, TAccumulate> aggregate,
            TAccumulate seed,
            Func<TAccumulate, T, TAccumulate> func,
            string span = nameof(ParallelEnumerable.Aggregate))
        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return aggregate(
                source,
                seed,
                (accumulate, value) =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        return func(accumulate, value);
                    }
                });
        }

        internal static TResult Visualize<T, TAccumulate, TResult>(
            this ParallelQuery<T> source,
            Func<ParallelQuery<T>, TAccumulate, Func<TAccumulate, T, TAccumulate>, Func<TAccumulate, TResult>, TResult> aggregate,
            TAccumulate seed,
            Func<TAccumulate, T, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector,
            string span = nameof(ParallelEnumerable.Aggregate))
        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return aggregate(
                source,
                seed,
                (accumulate, value) =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        return func(accumulate, value);
                    }
                },
                resultSelector);
        }

        internal static TResult Visualize<TSource, TAccumulate, TResult>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, TAccumulate, Func<TAccumulate, TSource, TAccumulate>, Func<TAccumulate, TAccumulate, TAccumulate>, Func<TAccumulate, TResult>, TResult> aggregate,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> updateAccumulatorFunc,
            Func<TAccumulate, TAccumulate, TAccumulate> combineAccumulatorsFunc,
            Func<TAccumulate, TResult> resultSelector,
            string span = nameof(ParallelEnumerable.Aggregate))

        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return aggregate(
                source,
                seed,
                (accumulate, value) =>
                    {
                        using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, $"{accumulate} {value}"))
                        {
                            return updateAccumulatorFunc(accumulate, value);
                        }
                    },
                (accumulates, accumulate) =>
                    {
                        using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, $"{accumulates} {accumulate}"))
                        {
                            return combineAccumulatorsFunc(accumulates, accumulate);
                        }
                    },
                resultSelector);
        }

        internal static TResult Visualize<TSource, TAccumulate, TResult>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, Func<TAccumulate>, Func<TAccumulate, TSource, TAccumulate>, Func<TAccumulate, TAccumulate, TAccumulate>, Func<TAccumulate, TResult>, TResult> aggregate,
            Func<TAccumulate> seedFactory,
            Func<TAccumulate, TSource, TAccumulate> updateAccumulatorFunc,
            Func<TAccumulate, TAccumulate, TAccumulate> combineAccumulatorsFunc,
            Func<TAccumulate, TResult> resultSelector,
            string span = nameof(ParallelEnumerable.Aggregate))

        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return aggregate(
                source,
                seedFactory,
                (accumulate, value) =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, $"{accumulate} {value}"))
                    {
                        return updateAccumulatorFunc(accumulate, value);
                    }
                },
                (accumulates, accumulate) =>
                {
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, $"{accumulates} {accumulate}"))
                    {
                        return combineAccumulatorsFunc(accumulates, accumulate);
                    }
                },
                resultSelector);
        }
    }
}
