namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    using static HelperMethods;

    internal static partial class QueryMethods
    {
        private static readonly Assembly mscorlib = typeof(object).Assembly;

        internal static void OptInOutParallel()
        {
            IEnumerable<string> obsoleteTypes = mscorlib.ExportedTypes // Return IEnumerable<Type>.
                .AsParallel() // Return ParallelQuery<Type>.
                .Where(type => type.GetCustomAttribute<ObsoleteAttribute>() != null) // ParallelEnumerable.Where.
                .Select(type => type.FullName) // ParallelEnumerable.Select.
                .AsSequential() // Return IEnumerable<Type>.
                .OrderBy(name => name); // Enumerable.OrderBy.
            obsoleteTypes.ForEach(name => Trace.WriteLine(name));
        }
    }

    internal static partial class QueryMethods
    {
        internal static void QueryExpression()
        {
            IEnumerable<string> obsoleteTypes =
                from name in
                    (from type in mscorlib.ExportedTypes.AsParallel()
                     where type.GetCustomAttribute<ObsoleteAttribute>() != null
                     select type.FullName).AsSequential()
                orderby name
                select name;
            obsoleteTypes.ForEach(name => Trace.WriteLine(name));
        }

        internal static void ForEachForAll()
        {
            Enumerable
                .Range(0, Environment.ProcessorCount * 2)
                .ForEach(value => Trace.WriteLine(value)); // 0 1 2 3 4 5 6 7

            ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .ForAll(value => Trace.WriteLine(value)); // 2 6 4 0 5 3 7 1
        }

        internal static void ForEachForAllTimeSpans()
        {
            string sequentialTimeSpanName = nameof(EnumerableEx.ForEach);
            // Draw a timespan for the entire sequential LINQ query execution, with text label "ForEach".
            using (Markers.EnterSpan(0, sequentialTimeSpanName))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(sequentialTimeSpanName);
                Enumerable.Range(0, Environment.ProcessorCount * 2).ForEach(value =>
                {
                    // Draw a sub timespan for each action execution, with each value as text label.
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        // Extends the action execution to a more visible timespan.
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        Trace.WriteLine(value);
                    }
                });
            }

            string parallelTimeSpanName = nameof(ParallelEnumerable.ForAll);
            // Draw a timespan for the entire parallel LINQ query execution, with text label "ForAll".
            using (Markers.EnterSpan(1, parallelTimeSpanName))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(parallelTimeSpanName);
                ParallelEnumerable.Range(0, Environment.ProcessorCount * 2).ForAll(value =>
                {
                    // Draw a sub timespan for each action execution, with each value as text label.
                    using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                    {
                        // Extends the action execution to a more visible timespan.
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        Trace.WriteLine(value);
                    }
                });
            }
        }

        internal static void VisualizeForEachForAll()
        {
            Enumerable
                .Range(0, Environment.ProcessorCount * 2)
                .Visualize(value =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    Trace.WriteLine(value);
                });

            ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .Visualize(value =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    Trace.WriteLine(value);
                });
        }

        internal static void Cancel()
        {
            CancellationTokenSource cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            try
            {
                ParallelEnumerable.Range(0, Environment.ProcessorCount * 4)
                    .WithCancellation(cancellation.Token)
                    .Select(Computing)
                    .ForAll(value => Trace.WriteLine(value));
            }
            catch (OperationCanceledException exception)
            {
                Trace.WriteLine(exception);
            }
            // 0 4 1 2 5 3 6 7 
            // OperationCanceledException: The query has been canceled via the token supplied to WithCancellation.
        }

        internal static void DegreeOfParallelism()
        {
            int count = Environment.ProcessorCount * 20;
            ParallelEnumerable.Range(0, count).WithDegreeOfParallelism(count).Visualize(value => Computing());
        }

        internal static void Merge()
        {
            int count = Environment.ProcessorCount * 2; // 8.

            Stopwatch stopwatch = Stopwatch.StartNew();
            ParallelQuery<int> notBuffered = ParallelEnumerable.Range(0, count)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select(Computing);
            notBuffered.ForEach(value => Trace.WriteLine($"{value}:{stopwatch.ElapsedMilliseconds}"));
            // 0:154 2:607 1:717 4:841 3:971 6:1050 5:1277 7:1614

            stopwatch.Restart();
            ParallelQuery<int> fullyBuffered = ParallelEnumerable.Range(0, count)
                .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                .Select(Computing);
            fullyBuffered.ForEach(value => Trace.WriteLine($"{value}:{stopwatch.ElapsedMilliseconds}"));
            // 0:1082 1:1083 2:1083 3:1084 4:1084 5:1085 6:1085 7:1085
        }

        // http://blogs.msdn.com/b/pfxteam/archive/2009/10/31/9915569.aspx
        public static void ExecutionMode()
        {
            int sum1 = ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Visualize(ParallelEnumerable.Aggregate, (accumulate, value) => accumulate + Computing(value));
            Trace.WriteLine(sum1);

            int sum2 = ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Visualize(ParallelEnumerable.Aggregate, 0, (accumulate, value) => accumulate + Computing(value));
            Trace.WriteLine(sum2);

            int sum3 = ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Visualize(
                    ParallelEnumerable.Aggregate, 0, (accumulate, value) => accumulate + Computing(value), result => result);
            Trace.WriteLine(sum3);
        }

        // http://blogs.msdn.com/b/pfxteam/archive/2008/01/22/7211660.aspx
        public static void Aggregate()
        {
            int sumOfSquares1 = ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .Visualize(
                    ParallelEnumerable.Aggregate,
                    0, // () => 0,
                    (accumulate, value) => accumulate + value * value + Computing(), // Computing() returns 0.
                    (accumulates, accumulate) => accumulates + accumulate + Computing(), // Computing() returns 0.
                    result => result);
            Trace.WriteLine(sumOfSquares1);

            int sumOfSquares2 = ParallelEnumerable
                .Range(0, Environment.ProcessorCount * 2)
                .Visualize(
                    ParallelEnumerable.Aggregate,
                    () => 0, // 0,
                    (accumulate, value) => accumulate + value * value + Computing(), // Computing() returns 0.
                    (accumulates, accumulate) => accumulates + accumulate + Computing(), // Computing() returns 0.
                    result => result);
            Trace.WriteLine(sumOfSquares2);
        }
    }
}

#if DEMO
namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second);
    }

    public static class ParallelEnumerable
    {
        public static ParallelQuery<TSource> Concat<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second);
    }
}
#endif

#if DEMO
namespace System.Linq
{
    using System.Collections.Generic;

    public static class EnumerableEx
    {
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext);
    }

    public static class ParallelEnumerable
    {
        public static void ForAll<TSource>(this ParallelQuery<TSource> source, Action<TSource> action);
    }
}
#endif

#if DEMO
namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    public static class Markers
    {
        public static Span EnterSpan(int category, string text);
    }

    public class MarkerSeries
    {
        public static Span EnterSpan(int category, string text);
    }
}
#endif