namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal static partial class Partitioning
    {
        private static int Computing(int value = 0)
        {
            Enumerable.Range(0, (value + 1) * 10000000).ForEach();
            return value;
        }

        internal static void Range()
        {
            int[] array = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray();
            array.AsParallel().Visualize(value => Computing(value), nameof(Range));
        }

        internal static void Chunk()
        {
            IEnumerable<int> source = Enumerable.Range(0, (1 + 2 + 4) * 3 * Environment.ProcessorCount + 8);
            System.Collections.Concurrent.Partitioner.Create(source, EnumerablePartitionerOptions.None)
                .AsParallel().VisualizeQuery(ParallelEnumerable.Select, _ => Computing());
        }

        internal static void Strip()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            source.AsParallel().VisualizeQuery(ParallelEnumerable.Select, value => Computing(value));
        }

        internal static void StripLoadBalance()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            source.AsParallel().VisualizeQuery(ParallelEnumerable.Select, value => Computing(value % 2));
        }

        internal static void Hash()
        {
            IEnumerable<Data> source = Enumerable.Range(5, 10).Concat(Enumerable.Repeat(2, 5)).Select(value => new Data(value));
            source.AsParallel().VisualizeQuery(
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

        public override int GetHashCode() => this.Value % (Environment.ProcessorCount - 1);

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
        internal static void Partitioner()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            new IxPartitioner<int>(source).AsParallel().VisualizeQuery(ParallelEnumerable.Select, Computing);
        }
    }

    public class IxOrderablePartitioner<TSource> : OrderablePartitioner<TSource>, IDisposable
    {
        private readonly IBuffer<KeyValuePair<long, TSource>> buffer;

        public IxOrderablePartitioner(IEnumerable<TSource> source)
            : base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: true, keysNormalized: true)
        {
            long index = -1;
            this.buffer = source.Select(value => new KeyValuePair<long, TSource>(Interlocked.Increment(ref index), value)).Share();
        }

        public override bool SupportsDynamicPartitions => true;

        public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions
            (int partitionCount) => Enumerable.Range(0, partitionCount).Select(_ => this.buffer.GetEnumerator()).ToArray();

        public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions() => this.buffer;

        public void Dispose() => this.buffer.Dispose();
    }

    internal static partial class Partitioning
    {
        internal static void IxOrderablePartitioner()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            new IxOrderablePartitioner<int>(source).AsParallel().VisualizeQuery(ParallelEnumerable.Select, Computing);
        }
    }
}
