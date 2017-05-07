namespace Tutorial.ParallelLinq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public static partial class ParallelEnumerableX
    {
        public static void ForAll<TSource>(this ParallelQuery<TSource> source) => source.ForAll(value => { });
    }

    public static partial class ParallelEnumerableX
    {
        public static void ForceParallel<TSource>(
            this IEnumerable<TSource> source, Action<TSource> action, int forcedDegreeOfParallelism)
        {
            if (forcedDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(forcedDegreeOfParallelism));
            }

            IList<IEnumerator<TSource>> partitions = Partitioner
                .Create(source, EnumerablePartitionerOptions.NoBuffering) // Stripped partitioning.
                .GetPartitions(forcedDegreeOfParallelism);
            using (CountdownEvent countdownEvent = new CountdownEvent(forcedDegreeOfParallelism))
            {
                partitions.ForEach(partition => new Thread(() =>
                {
                    try
                    {
                        using (partition)
                        {
                            while (partition.MoveNext())
                            {
                                action(partition.Current);
                            }
                        }
                    }
                    finally 
                    {
                        countdownEvent.Signal();
                    }
                }).Start());
                countdownEvent.Wait();
            }
        }
    }
}
