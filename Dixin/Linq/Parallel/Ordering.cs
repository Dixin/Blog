namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    internal static partial class Partitioning
    {
        internal static void PartitionerAsOrdered()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            new IxPartitioner<int>(source).AsParallel().AsOrdered().ToArray();
            // InvalidOperationException: AsOrdered may not be used with a partitioner that is not orderable.
        }
    }

    public class IxOrderablePartitioner<TSource> : OrderablePartitioner<TSource>, IDisposable
    {
        private readonly IBuffer<KeyValuePair<long, TSource>> buffer;

        public IxOrderablePartitioner(IEnumerable<TSource> source)
            : base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: true, keysNormalized: true)
        {
            long index = -1;
            this.buffer = source
                .Select(value => new KeyValuePair<long, TSource>(Interlocked.Increment(ref index), value))
                .Share();
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
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 2).ToArray();
            ParallelQuery<int> unordered = new IxOrderablePartitioner<int>(source).AsParallel().Select(Computing);
            Trace.WriteLine(string.Join(" ", unordered)); // 2 6 1 5 3 7 0 4.
            ParallelQuery<int> ordered = new IxOrderablePartitioner<int>(source).AsParallel().AsOrdered().Select(Computing);
            Trace.WriteLine(string.Join(" ", ordered)); // 0 1 2 3 4 5 6 7.
        }
    }
}
