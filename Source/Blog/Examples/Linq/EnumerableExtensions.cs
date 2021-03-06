﻿namespace Examples.Linq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction)
        {
            foreach (T value in source)
            {
                await asyncAction(value);
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> asyncAction)
        {
            int index = -1;
            foreach (T value in source)
            {
                await asyncAction(value, checked(++index));
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerator<T> iterator, Func<T, int, Task> asyncAction)
        {
            int index = -1;
            while (iterator.MoveNext())
            {
                await asyncAction(iterator.Current, checked(++index));
            }
        }

        private static readonly int DefaultMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 512);

        public static async Task ParallelForEachAsync<T>(
            this IEnumerable<T> source, Func<T, int, Task> asyncAction, int? maxDegreeOfParallelism = null)
        {
            maxDegreeOfParallelism ??= DefaultMaxDegreeOfParallelism;
            if (maxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }

            int index = -1;
            if (maxDegreeOfParallelism == 1)
            {
                foreach (T value in source)
                {
                    await asyncAction(value, checked(++index));
                }

                return;
            }

            OrderablePartitioner<T> partitioner = source is IList<T> list
                ? Partitioner.Create(list, loadBalance: true)
                : Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
            await Task.WhenAll(partitioner
                .GetPartitions(maxDegreeOfParallelism.Value)
                .Select(async partition =>
                {
                    while (partition.MoveNext())
                    {
                        await asyncAction(partition.Current, Interlocked.Increment(ref index));
                    }
                }));
        }

        public static async Task ForEachAsync<T>(this ParallelQuery<T> source, Func<T, Task> asyncAction)
        {
            await Task.WhenAll(source.Select(asyncAction).ToArray());
        }

        public static async Task ParallelForEachAsync<T>(
            this IEnumerable<T> source, Func<T, Task> asyncAction, int? maxDegreeOfParallelism = null)
        {
            maxDegreeOfParallelism ??= DefaultMaxDegreeOfParallelism;
            if (maxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }

            if (maxDegreeOfParallelism == 1)
            {
                foreach (T value in source)
                {
                    await asyncAction(value);
                }

                return;
            }

            OrderablePartitioner<T> partitioner = source is IList<T> list
                ? Partitioner.Create(list, loadBalance: true)
                : Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
            await Task.WhenAll(partitioner
                .GetPartitions(maxDegreeOfParallelism.Value)
                .Select(async partition =>
                {
                    while (partition.MoveNext())
                    {
                        await asyncAction(partition.Current);
                    }
                }));
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) where T : class
        {
            return source.Where(value => value is not null)!; // Equivalent to: source.Where(value => value is not null).Select(value => value!).
        }

        public static ParallelQuery<T> NotNull<T>(this ParallelQuery<T?> source) where T : class
        {
            return source.Where(value => value is not null)!; // Equivalent to: source.Where(value => !(value is null)).Select(value => value!).
        }

        public static void ForEach<T>(this IEnumerator<T> source, Action<T> action)
        {
            while (source.MoveNext())
            {
                action(source.Current);
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerator<T> source, Func<T, Task> asyncAction)
        {
            while (source.MoveNext())
            {
                await asyncAction(source.Current);
            }
        }
    }
}
