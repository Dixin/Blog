namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using static HelperMethods;

    internal static partial class Partitioning
    {
        internal static void Range()
        {
            int[] array = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray();
            array.AsParallel().Visualize(value => Computing(value), nameof(Range));
        }

        internal static void Chunk()
        {
            IEnumerable<int> source = Enumerable.Range(0, (1 + 2 + 4) * 3 * Environment.ProcessorCount + 8);
            Partitioner.Create(source, EnumerablePartitionerOptions.None)
                .AsParallel().Visualize(ParallelEnumerable.Select, _ => Computing());
        }

        internal static void Strip()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            source.AsParallel().Visualize(ParallelEnumerable.Select, value => Computing(value));
        }

        internal static void StripLoadBalance()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            source.AsParallel().Visualize(ParallelEnumerable.Select, value => Computing(value % 2));
        }

        internal static void Hash()
        {
            IEnumerable<Data> source = Enumerable.Range(5, 10).Concat(Enumerable.Repeat(2, 5)).Select(value => new Data(value));
            source.AsParallel().Visualize(
                (parallelQuery, elementSelector) => parallelQuery
                    .GroupBy(value => value, elementSelector)
                    .Select(group => group.Key),
                value => Computing(value.Value)); // elementSelector
            // Equivalent to:
            // string span = nameof(Parallel);
            // using (Markers.EnterSpan(Thread.CurrentThread.ManagedThreadId, span))
            // {
            //    MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            //    source.AsParallel()
            //        .GroupBy(
            //            value => value, // Key selector.
            //            value => // Value selector.
            //            {
            //                using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
            //                {
            //                    Computing();
            //                }
            //
            //                return value;
            //            })
            //        .ForAll(_ => { });
            // }
        }
    }

    internal struct Data
    {
        internal Data(int value)
        {
            this.Value = value;
        }

        internal int Value { get; }

        public override int GetHashCode() => 
            this.Value % (Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1);

        public override bool Equals(object obj) => obj is Data && this.GetHashCode() == ((Data)obj).GetHashCode();

        public override string ToString() => this.Value.ToString();
    }

    public class IxPartitioner<TSource> : Partitioner<TSource>, IDisposable
    {
        private readonly IBuffer<TSource> buffer;

        public IxPartitioner(IEnumerable<TSource> source)
        {
            this.buffer = source.Share();
        }

        public override bool SupportsDynamicPartitions => true;

        public override IList<IEnumerator<TSource>> GetPartitions
            (int partitionCount) => Enumerable.Range(0, partitionCount).Select(_ => this.buffer.GetEnumerator()).ToArray();

        public override IEnumerable<TSource> GetDynamicPartitions() => this.buffer;

        public void Dispose() => this.buffer.Dispose();
    }

    internal static partial class Partitioning
    {
        internal static void Partition()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            new IxPartitioner<int>(source).AsParallel().Visualize(ParallelEnumerable.Select, Computing);
        }

        internal static IList<IList<TSource>> GetPartitions<TSource>(IEnumerable<TSource> partitionsSource, int partitionCount)
        {
            List<TSource>[] partitions = Enumerable.Range(0, partitionCount).Select(_ => new List<TSource>()).ToArray();
            Thread[] partitioningThreads = Enumerable
                .Range(0, partitionCount)
                .Select(_ => partitionsSource.GetEnumerator())
                .Select((partitionIterator, partitionIndex) => new Thread(() =>
                {
                    List<TSource> partition = partitions[partitionIndex];
                    using (partitionIterator)
                    {
                        while (partitionIterator.MoveNext())
                        {
                            partition.Add(partitionIterator.Current);
                        }
                    }
                }))
                .ToArray();
            partitioningThreads.ForEach(thread => thread.Start());
            partitioningThreads.ForEach(thread => thread.Join());
            return partitions;
        }
    }
}
