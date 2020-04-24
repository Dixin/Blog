namespace Examples.Linq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class EnumerableExtensions
    {
        internal static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction)
        {
            foreach (T value in source)
            {
                await asyncAction(value);
            }
        }

        public static async Task ParallelForEachAsync<T>(
            this IEnumerable<T> source, Func<T, Task> asyncAction, int? maxDegreeOfParallelism = null)
        {
            maxDegreeOfParallelism ??= Environment.ProcessorCount;
            OrderablePartitioner<T> partitioner = source is IList<T> list
                ? Partitioner.Create(list, loadBalance: true)
                : Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
            await Task.WhenAll(partitioner
                .GetPartitions(maxDegreeOfParallelism.Value)
                .Select(partition => Task.Run(async () =>
                {
                    while (partition.MoveNext())
                    {
                        await asyncAction(partition.Current);
                    }
                })));
        }
    }
}
