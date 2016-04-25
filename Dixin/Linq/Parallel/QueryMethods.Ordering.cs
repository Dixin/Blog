namespace Dixin.Linq.Parallel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using Person = Dixin.Linq.Fundamentals.Person;

    using static HelperMethods;

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
            (int partitionCount) => Enumerable.Select(Enumerable.Range(0, partitionCount), _ => this.buffer.GetEnumerator()).ToArray();

        public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions() => this.buffer;

        public void Dispose() => this.buffer.Dispose();
    }

    internal static partial class QueryMethods
    {
        internal static void Select()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 2).ToArray();

            ParallelQuery<int> parallelSource1 = new IxPartitioner<int>(source).AsParallel();
            ParallelQuery<int> unordered1 = parallelSource1.Select(Computing);
            Trace.WriteLine(string.Join(" ", unordered1)); // 0 4 1 5 2 6 3 7.

            ParallelQuery<int> parallelSource2 = new IxOrderablePartitioner<int>(source).AsParallel();
            ParallelQuery<int> unordered2 = parallelSource2.Select(Computing);
            Trace.WriteLine(string.Join(" ", unordered2)); // 2 6 1 5 3 7 0 4.

            ParallelQuery<int> parallelSource3 = new IxOrderablePartitioner<int>(source).AsParallel();
            ParallelQuery<int> ordered = parallelSource3.AsOrdered().Select(Computing);
            Trace.WriteLine(string.Join(" ", ordered)); // 0 1 2 3 4 5 6 7.
        }
    }

    internal static partial class QueryMethods
    {
        internal static void ElementAt()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray(); // 0 ... 16.

            ParallelQuery<int> parallelSource1 = new IxPartitioner<int>(source).AsParallel();
            int unordered = parallelSource1.Select(Computing).ElementAt(source.Length / 2); // ElementAt(8).
            Trace.WriteLine(unordered); // 2.

            ParallelQuery<int> parallelSource2 = new IxOrderablePartitioner<int>(source).AsParallel();
            int ordered = parallelSource2.AsOrdered().Select(Computing).ElementAt(source.Length / 2); // ElementAt(8).
            Trace.WriteLine(ordered); // 8.
        }

        internal static void Take()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray(); // 0 ... 16.

            ParallelQuery<int> parallelSource1 = new IxPartitioner<int>(source).AsParallel();
            ParallelQuery<int> unordered = parallelSource1.Select(Computing).Take(source.Length / 2); // Take(8).
            Trace.WriteLine(string.Join(" ", unordered)); // 3 7 11 15 0 4 8 12.

            ParallelQuery<int> parallelSource2 = new IxOrderablePartitioner<int>(source).AsParallel();
            ParallelQuery<int> ordered = parallelSource2.AsOrdered().Select(Computing).Take(source.Length / 2); // Take(8).
            Trace.WriteLine(string.Join(" ", ordered)); // 0 1 2 3 4 5 6 7.
        }

        internal static void Reverse()
        {
            // https://msdn.microsoft.com/en-us/library/dd460677.aspx Reverse does nothing.
            int count = Environment.ProcessorCount * 2; // 8.

            ParallelQuery<int> parallelSource1 = ParallelEnumerable.Range(0, count);
            ParallelQuery<int> unordered = parallelSource1
                .VisualizeQuery(ParallelEnumerable.Select, Computing, nameof(unordered))
                .Reverse()
                .VisualizeQuery(ParallelEnumerable.Select, Computing, nameof(unordered));
            Trace.WriteLine(string.Join(" ", unordered)); // 1 0 3 2 5 4 7 6.

            ParallelQuery<int> parallelSource2 = ParallelEnumerable.Range(0, count);
            ParallelQuery<int> ordered = parallelSource2.AsOrdered()
                .VisualizeQuery(ParallelEnumerable.Select, Computing, nameof(ordered))
                .Reverse()
                .VisualizeQuery(ParallelEnumerable.Select, Computing, nameof(ordered));
            Trace.WriteLine(string.Join(" ", ordered)); // 7 6 5 4 3 2 1 0.
        }

        internal static void Join()
        {
            Random random = new Random();
            Func<ParallelQuery<Person>> getOuter = () => Enumerable
                .Repeat(0, 1000).Select(_ => new Person()
                    {
                        Age = random.Next(0, 100),
                        Name = Guid.NewGuid().ToString()
                    })
                .AsParallel();
            Func<ParallelQuery<Person>> getInner = () => Enumerable
                .Repeat(0, 10000).Select(_ => new Person()
                    {
                        Age = random.Next(0, 100),
                        Name = Guid.NewGuid().ToString()
                    })
                .AsParallel();

            Stopwatch stopwatch = Stopwatch.StartNew();
            getOuter()
                .Join(
                    getInner(),
                    outerPerson => outerPerson.Age,
                    innerPerson => innerPerson.Age,
                    (outerPerson, innerPerson) => new { Outer = outerPerson, Inner = innerPerson })
                .ForAll(_ => { });
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 243.

            stopwatch.Restart();
            getOuter().AsUnordered()
                .Join(
                    getInner(),
                    outerPerson => outerPerson.Age,
                    innerPerson => innerPerson.Age,
                    (outerPerson, innerPerson) => new { Outer = outerPerson, Inner = innerPerson })
                .ForAll(_ => { });
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 77.

            stopwatch.Restart();
            getOuter().AsUnordered()
                .Join(
                    getInner().AsUnordered(),
                    outerPerson => outerPerson.Age,
                    innerPerson => innerPerson.Age,
                    (outerPerson, innerPerson) => new { Outer = outerPerson, Inner = innerPerson })
                .ForAll(_ => { });
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 64.
        }
    }
}
