namespace Examples.Linq;

using Examples.Common;

public static class EnumerableExtensions
{
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, ValueTask> asyncAction, CancellationToken cancellationToken = default) =>
        await source.ForEachAsync((T value, CancellationToken token) => asyncAction(value), cancellationToken);

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> asyncAction, CancellationToken cancellationToken = default)
    {
        foreach (T value in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(value, cancellationToken);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, ValueTask> asyncAction, CancellationToken cancellationToken = default) =>
        await source.ForEachAsync((value, index, token) => asyncAction(value, index), cancellationToken);

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, CancellationToken, ValueTask> asyncAction, CancellationToken cancellationToken = default)
    {
        int index = -1;
        foreach (T value in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(value, checked(++index), cancellationToken);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, CancellationToken, ValueTask<bool>> asyncAction, CancellationToken cancellationToken = default)
    {
        int index = -1;
        foreach (T value in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await asyncAction(value, checked(++index), cancellationToken))
            {
                break;
            }
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerator<T> iterator, Func<T, int, CancellationToken, ValueTask> asyncAction, CancellationToken cancellationToken = default)
    {
        int index = -1;
        while (iterator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(iterator.Current, checked(++index), cancellationToken);
        }
    }

    private static readonly int DefaultMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 512);

    public static void ParallelForEach<T>(
        this IEnumerable<T> source, Action<T, long> action, int? maxDegreeOfParallelism = null, CancellationToken cancellationToken = default) =>
        Parallel.ForEach(
            source,
            new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism, CancellationToken = cancellationToken },
            (value, state, index) => action(value, index));

    public static async Task ParallelForEachAsync<T>(
        this IEnumerable<T> source, Func<T, int, ValueTask> asyncAction, int? maxDegreeOfParallelism = null, CancellationToken cancellationToken = default) =>
        await source.ParallelForEachAsync((value, index, token) => asyncAction(value, index), maxDegreeOfParallelism, cancellationToken);

    public static async Task ParallelForEachAsync<T>(
        this IEnumerable<T> source, Func<T, int, CancellationToken, ValueTask> asyncAction, int? maxDegreeOfParallelism = null, CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(
            source.Select((value, index) => (value, index)),
            new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism, CancellationToken = cancellationToken },
            (value, token) => asyncAction(value.value, value.index, token));
        //maxDegreeOfParallelism ??= DefaultMaxDegreeOfParallelism;
        //if (maxDegreeOfParallelism <= 0)
        //{
        //    throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), maxDegreeOfParallelism, string.Empty);
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
        //    throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), maxDegreeOfParallelism, string.Empty);
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

    public static async Task ForEachAsync<T>(this IEnumerator<T> source, Func<T, CancellationToken, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        while (source.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncAction(source.Current, cancellationToken);
        }
    }

    public static HashSet<string> ToHashSetOrdinalIgnoreCase(this IEnumerable<string> source) => 
        source.ThrowIfNull().ToHashSet(StringComparer.OrdinalIgnoreCase);

    extension<TSource>(IEnumerable<TSource>)
    {
        public static IEnumerable<TSource> operator +(IEnumerable<TSource> left, IEnumerable<TSource> right) => left.Concat(right);
    }
}