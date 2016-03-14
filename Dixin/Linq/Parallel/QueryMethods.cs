namespace Dixin.Linq.Parallel
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using static HelperMethods;

    internal static partial class QueryMethods
    {
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