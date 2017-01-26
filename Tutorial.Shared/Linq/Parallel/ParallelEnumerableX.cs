namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public static class ParallelEnumerableX
    {
        public static void ForceParallel<TSource>(
            this IEnumerable<TSource> source, Action<TSource> action, int forcedDegreeOfParallelism)
        {
            ConcurrentQueue<TSource> queue = new ConcurrentQueue<TSource>(source);
            Thread[] threads = Enumerable
                .Range(0, Math.Min(forcedDegreeOfParallelism, queue.Count))
                .Select(_ => new Thread(() =>
                    {
                        while (queue.TryDequeue(out TSource value))
                        {
                            action(value);
                        }
                    }))
                .ToArray();
            threads.ForEach(thread => thread.Start());
            threads.ForEach(thread => thread.Join());
        }
    }
}
