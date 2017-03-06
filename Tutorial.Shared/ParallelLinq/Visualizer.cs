namespace Tutorial.ParallelLinq
{
#if NETFX
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;
#else
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
#endif

#if !NETFX
    public class Markers
    {
        public static Span EnterSpan(int category, string spanName) => new Span(category, spanName);

        public static MarkerSeries CreateMarkerSeries(string markSeriesName) => new MarkerSeries(markSeriesName);
    }

    public class Span : IDisposable
    {
        private readonly int category;

        private readonly string spanName;

        private readonly DateTime start = DateTime.Now;

        public Span(int category, string spanName, string markSeriesName = null)
        {
            this.category = category;
            this.spanName = string.IsNullOrEmpty(markSeriesName) ? spanName : $@"{markSeriesName}\{spanName}";
            $"Timestamp: {this.start.ToString("o")}, thread: {Thread.CurrentThread.ManagedThreadId}, category: {this.category}, span: {this.spanName}".WriteLine();
        }

        public void Dispose()
        {
            DateTime end = DateTime.Now;
            $"Timestamp: {end.ToString("o")}, thread: {Thread.CurrentThread.ManagedThreadId}, category: {this.category}, span: {this.spanName}, duration: {end - start}".WriteLine();
        }
    }

    public class MarkerSeries
    {
        private readonly string markSeriesName;

        public MarkerSeries(string markSeriesName) => this.markSeriesName = markSeriesName;

        public Span EnterSpan(int category, string spanName) => new Span(category, spanName, markSeriesName);
    }
#endif

    public static partial class Visualizer
    {
        internal const string Parallel = nameof(Parallel);

        internal const string Sequential = nameof(Sequential);

        internal static void Visualize<TSource>(
            this IEnumerable<TSource> source, Action<TSource> action, string span = Sequential, int category = -1)
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

        internal static void Visualize<TSource>(
            this ParallelQuery<TSource> source, Action<TSource> action, string span = Parallel, int category = -2)
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
        internal static IEnumerable<TResult> Visualize<TSource, TMiddle, TResult>(
            this IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, Func<TSource, TMiddle>, IEnumerable<TResult>> query,
            Func<TSource, TMiddle> func,
            Func<TSource, string> spanFactory = null,
            string span = Sequential)
        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return query(
                source,
                value =>
                {
                    using (markerSeries.EnterSpan(
                        Thread.CurrentThread.ManagedThreadId, spanFactory?.Invoke(value) ?? value.ToString()))
                    {
                        return func(value);
                    }
                });
        }

        internal static ParallelQuery<TResult> Visualize<TSource, TMiddle, TResult>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, Func<TSource, TMiddle>, ParallelQuery<TResult>> query,
            Func<TSource, TMiddle> func,
            Func<TSource, string> spanFactory = null,
            string span = Parallel)
        {
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return query(
                source,
                value =>
                {
                    using (markerSeries.EnterSpan(
                        Thread.CurrentThread.ManagedThreadId, spanFactory?.Invoke(value) ?? value.ToString()))
                    {
                        return func(value);
                    }
                });
        }
    }

    public static partial class Visualizer
    {
        internal static TSource Visualize<TSource>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, Func<TSource, TSource, TSource>, TSource> aggregate,
            Func<TSource, TSource, TSource> func,
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

        internal static TAccumulate Visualize<TSource, TAccumulate>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, TAccumulate, Func<TAccumulate, TSource, TAccumulate>, TAccumulate> aggregate,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
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

        internal static TResult Visualize<TSource, TAccumulate, TResult>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, TAccumulate, Func<TAccumulate, TSource, TAccumulate>, Func<TAccumulate, TResult>, TResult> aggregate,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
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
