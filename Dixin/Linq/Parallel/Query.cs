namespace Dixin.Linq.Parallel
{
    using System;
    using System.Linq;

    using Dixin.Common;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    internal static partial class Query
    {
        internal static int[] Sequential
            (int[] values) => values.Where(value => value.IsPrime()).ToArray();

        internal static int[] Parallel
            (int[] values) => values.AsParallel().Where(value => value.IsPrime()).ToArray();
    }

    public static class VisualizerHelper
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

    internal static partial class Query
    {
        internal static void Primes()
        {
            int[] values = ArrayHelper.RandomArray(0, int.MaxValue, 500000);
            VisualizerHelper.Sequential(values, value => value.IsPrime());
            VisualizerHelper.Parallel(values, value => value.IsPrime());
        }
    }
}