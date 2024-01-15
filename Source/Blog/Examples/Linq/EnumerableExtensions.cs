namespace Examples.Linq;

public static class EnumerableExtensions
{
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        foreach (T value in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(value);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        int index = -1;
        foreach (T value in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(value, checked(++index));
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerator<T> iterator, Func<T, int, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        int index = -1;
        while (iterator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(iterator.Current, checked(++index));
        }
    }

    private static readonly int DefaultMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 512);

    public static void ParallelForEach<T>(
        this IEnumerable<T> source, Action<T, long> asyncAction, int? maxDegreeOfParallelism = null, CancellationToken cancellationToken = default) =>
        Parallel.ForEach(
            source,
            new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism, CancellationToken = cancellationToken },
            (value, _, index) => asyncAction(value, index));

    public static Task ParallelForEachAsync<T>(
        this IEnumerable<T> source, Func<T, int, ValueTask> asyncAction, int? maxDegreeOfParallelism = null, CancellationToken cancellationToken = default)
    {
        int index = -1;
        return Parallel.ForEachAsync(source,
            new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism, CancellationToken = cancellationToken },
            (value, _) =>
            {
                int currentIndex = Interlocked.Increment(ref index);
                if (currentIndex < 0)
                {
                    throw new OverflowException();
                }

                return asyncAction(value, currentIndex);
            });
        //maxDegreeOfParallelism ??= DefaultMaxDegreeOfParallelism;
        //if (maxDegreeOfParallelism <= 0)
        //{
        //    throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
        //}

        //int index = -1;
        //if (maxDegreeOfParallelism == 1)
        //{
        //    foreach (T value in source)
        //    {
        //        await asyncAction(value, checked(++index));
        //    }

        //    return;
        //}

        //OrderablePartitioner<T> partitioner = source is IList<T> list
        //    ? Partitioner.Create(list, loadBalance: true)
        //    : Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
        //await Task.WhenAll(partitioner
        //    .GetPartitions(maxDegreeOfParallelism.Value)
        //    .Select(async partition =>
        //    {
        //        while (partition.MoveNext())
        //        {
        //            await asyncAction(partition.Current, Interlocked.Increment(ref index));
        //        }
        //    }));
    }

    public static Task ForEachAsync<T>(this ParallelQuery<T> source, Func<T, Task> asyncAction)
    {
        return Task.WhenAll(source.Select(asyncAction).ToArray());
    }

    public static Task ParallelForEachAsync<T>(
        this IEnumerable<T> source, Func<T, ValueTask> asyncAction, int? maxDegreeOfParallelism = null, CancellationToken cancellationToken = default)
    {
        return Parallel.ForEachAsync(
            source,
            new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism, CancellationToken = cancellationToken },
            (value, cancellationToken) => asyncAction(value));
        //maxDegreeOfParallelism ??= DefaultMaxDegreeOfParallelism;
        //if (maxDegreeOfParallelism <= 0)
        //{
        //    throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
        //}

        //if (maxDegreeOfParallelism == 1)
        //{
        //    foreach (T value in source)
        //    {
        //        await asyncAction(value);
        //    }

        //    return;
        //}

        //OrderablePartitioner<T> partitioner = source is IList<T> list
        //    ? Partitioner.Create(list, loadBalance: true)
        //    : Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
        //await Task.WhenAll(partitioner
        //    .GetPartitions(maxDegreeOfParallelism.Value)
        //    .Select(async partition =>
        //    {
        //        while (partition.MoveNext())
        //        {
        //            await asyncAction(partition.Current);
        //        }
        //    }));
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) where T : class
    {
        return source.Where(value => value is not null)!; // Equivalent to: source.Where(value => value is not null).Select(value => value!).
    }

    public static ParallelQuery<T> NotNull<T>(this ParallelQuery<T?> source) where T : class
    {
        return source.Where(value => value is not null)!; // Equivalent to: source.Where(value => value is not null).Select(value => value!).
    }

    public static void ForEach<T>(this IEnumerator<T> source, Action<T> action)
    {
        while (source.MoveNext())
        {
            action(source.Current);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Func<T, bool> action)
    {
        foreach (T value in source)
        {
            if (!action(value))
            {
                break;
            }
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerator<T> source, Func<T, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        while (source.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(source.Current);
        }
    }
}