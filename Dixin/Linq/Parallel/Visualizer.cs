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

        internal static void Visualize<T>(this IEnumerable<T> source, Action<T> action, string span = Sequential, int category = 0)
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

        internal static void Visualize<T>(this ParallelQuery<T> source, Action<T> action, string span = Parallel, int category = 1)
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
        internal static void VisualizeQuery<T, TResult>(
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

        internal static void VisualizeQuery<T, TResult>(
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
    }
}
