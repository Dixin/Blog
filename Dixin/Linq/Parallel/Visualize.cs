namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    public static partial class Visualize
    {
        internal static void Sequential<T>
            (T[] values, Action<T> action) => Run(values, action, false, nameof(Sequential));

        internal static void Parallel<T>
            (T[] values, Action<T> action) => Run(values, action, true, nameof(Parallel));

        internal static void Run<T>(T[] values, Action<T> action, bool parallel, string span)
        {
            int category = Convert.ToInt32(parallel);
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                if (parallel)
                {
                    values.AsParallel().ForAll(value =>
                        {
                            using (markerSeries.EnterSpan(category, value.ToString()))
                            {
                                action(value);
                            }
                        });
                }
                else
                {
                    values.ForEach(value =>
                        {
                            using (markerSeries.EnterSpan(category, value.ToString()))
                            {
                                action(value);
                            }
                        });
                }
            }
        }
    }

    public static partial class Visualize
    {
        internal static void Sequential<T, TResult>(
            IEnumerable<T> source, Func<IEnumerable<T>, Func<T, TResult>, IEnumerable<T>> query, Func<T, TResult> func)
        {
            int category = 0;
            string span = nameof(Sequential);
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                query(source, value =>
                    {
                        using (markerSeries.EnterSpan(category, value.ToString()))
                        {
                            return func(value);
                        }
                    }).ForEach();
            }
        }

        internal static void Parallel<T, TResult>(
            ParallelQuery<T> source, Func<ParallelQuery<T>, Func<T, TResult>, ParallelQuery<T>> query, Func<T, TResult> func)
        {
            int category = 0;
            string span = nameof(Parallel);
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                query(source, value =>
                {
                    using (markerSeries.EnterSpan(category, value.ToString()))
                    {
                        return func(value);
                    }
                }).ForAll(_ => { });
            }
        }
    }
}
