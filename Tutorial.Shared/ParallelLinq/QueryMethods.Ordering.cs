namespace Tutorial.ParallelLinq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using static Functions;

    using Stopwatch = System.Diagnostics.Stopwatch;

    internal static partial class QueryMethods
    {
        internal static void SelectWithIndex() => 
            new StaticPartitioner<int>(Enumerable.Range(0, Environment.ProcessorCount * 2))
                .AsParallel()
                .Select((value, index) => $"[{index}]={value}")
                .WriteLines(); // [0]=0 [1]=2 [2]=4 [3]=5 [4]=6 [5]=1 [6]=3 [7]=7

        internal static void AsOrdered()
        {
            Enumerable
                .Range(0, Environment.ProcessorCount * 2)
                .AsParallel()
                .Select(value => value + Compute())
                .WriteLines(); // 3 1 2 0 4 5 6 7

            Enumerable
                .Range(0, Environment.ProcessorCount * 2)
                .AsParallel()
                .AsOrdered()
                .Select(value => value + Compute())
                .WriteLines(); // 0 1 2 3 4 5 6 7
        }

        internal static void AsUnordered()
        {
            Random random = new Random();
            Model[] source = Enumerable
                .Range(0, Environment.ProcessorCount * 10_000)
                .Select(_ => new Model(name: Guid.NewGuid().ToString(), weight: random.Next(1, 100)))
                .ToArray();

            Stopwatch stopwatch = Stopwatch.StartNew();
            source
                .AsParallel()
                .GroupBy(model => model.Weight, model => model.Name)
                .ForAll();
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.WriteLine(); // 35.

            stopwatch.Restart();
            source
                .AsParallel()
                .AsUnordered()
                .GroupBy(model => model.Weight, model => model.Name)
                .ForAll();
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.WriteLine(); // 2.
        }

        internal static void OrderBy()
        {
            Enumerable
                .Range(0, Environment.ProcessorCount * 2)
                .AsParallel()
                .Select(value => value) // Order is not persisted.
                .WriteLines(); // 3 1 2 0 4 5 6 7

            Enumerable
                .Range(0, Environment.ProcessorCount * 2)
                .AsParallel()
                .Select(value => value) // Order is not persisted.
                .OrderBy(value => value) // Order is introduced.
                .Select(value => value) // Order is persisted.
                .WriteLines(); // 3 1 2 0 4 5 6 7
        }

        internal static void Correctness()
        {
            int count = Environment.ProcessorCount * 4;
            int[] source = Enumerable.Range(0, count).ToArray(); // 0 ... 15.

            int elementAt = new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .ElementAt(count / 2).WriteLine() // Expected: 8, 
                .WriteLine(); // Actual: 2.

            int first = new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .First() // Expected: 0.
                .WriteLine(); // Actual: 3.

            int last = new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .Last() // Expected: 15.
                .WriteLine(); // Actual: 13.

            new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .Take(count / 2) // Expected: 0 ... 7.
                .WriteLines(); // Actual: 3 2 5 7 10 11 14 15.

            new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .Skip(count / 2) // Expected: 8 ... 15.
                .WriteLines(); // Actual: 3 0 7 5 11 10 15 14.

            new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .TakeWhile(value => value <= count / 2) // Expected: 0 ... 7.
                .WriteLines(); // Actual: 3 5 8.

            new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .SkipWhile(value => value <= count / 2) // Expected: 9 ... 15.
                .WriteLines(); // Actual: 1 3 2 13 5 7 6 11 9 10 15 12 14.

            new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .Reverse() // Expected: 15 ... 0.
                .WriteLines(); // Actual: 12 8 4 2 13 9 5 1 14 10 6 0 15 11 7 3.

            bool sequentialEqual = new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .SequenceEqual(new StaticPartitioner<int>(source).AsParallel()); // Expected: True.
            sequentialEqual.WriteLine(); // Actual: False.

            new StaticPartitioner<int>(source).AsParallel().Select(value => value + Compute())
                .Zip(
                    second: new StaticPartitioner<int>(source).AsParallel(),
                    resultSelector: (a, b) => $"({a}, {b})") // Expected: (0, 0) ... (15, 15).
                .WriteLines(); // Actual: (3, 8) (0, 12) (1, 0) (2, 4) (6, 9) (7, 13) ...
        }
    }

    public class Model
    {
        public Model(string name, int weight)
        {
            this.Name = name;
            this.Weight = weight;
        }

        public string Name { get; }

        public int Weight { get; }
    }

    internal static partial class QueryMethods
    {
        internal static void JoinAsUnordered()
        {
            int count = Environment.ProcessorCount * 10_000;
            Random random = new Random();
            ParallelQuery<Model> outer = Enumerable
                .Repeat(0, count)
                .Select(_ => new Model(name: Guid.NewGuid().ToString(), weight: random.Next(1, 100)))
                .AsParallel();
            ParallelQuery<Model> inner = Enumerable
                .Repeat(0, count)
                .Select(_ => new Model(name: Guid.NewGuid().ToString(), weight: random.Next(1, 100)))
                .AsParallel();

            Stopwatch stopwatch = Stopwatch.StartNew();
            outer
                .Join(
                    inner,
                    outerPerson => (outerPerson.Weight, outerPerson.Name.Substring(0, 2)),
                    innerPerson => (innerPerson.Weight + 1, innerPerson.Name.Substring(2, 4)),
                    (outerPerson, innerPerson) => new { Outer = outerPerson, Inner = innerPerson })
                .ForAll();
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.WriteLine(); // 243

            stopwatch.Restart();
            outer.AsUnordered()
                .Join(
                    inner,
                    outerPerson => (outerPerson.Weight, outerPerson.Name.Substring(0, 2)),
                    innerPerson => (innerPerson.Weight + 1, innerPerson.Name.Substring(2, 4)),
                    (outerPerson, innerPerson) => new { Outer = outerPerson, Inner = innerPerson })
                .ForAll();
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.WriteLine(); // 77

            stopwatch.Restart();
            outer.AsUnordered()
                .Join(
                    inner.AsUnordered(),
                    outerPerson => (outerPerson.Weight, outerPerson.Name.Substring(0, 2)),
                    innerPerson => (innerPerson.Weight + 1, innerPerson.Name.Substring(2, 4)),
                    (outerPerson, innerPerson) => new { Outer = outerPerson, Inner = innerPerson })
                .ForAll();
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.WriteLine(); // 64
        }

        internal static void MergeOptionForOrder()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 1_000_000).ToArray();
            List<int> fullyBuffered = new List<int>();
            source
                .AsParallel()
                .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                .ForEach(value => fullyBuffered.Add(value));
            fullyBuffered.SequenceEqual(source).WriteLine(); // True

            List<int> notBuffered = new List<int>();
            source
                .AsParallel()
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .ForEach(value => notBuffered.Add(value));
            notBuffered.SequenceEqual(source).WriteLine(); // False
        }
    }

    public class OrderableDynamicPartitioner<TSource> : OrderablePartitioner<TSource>
    {
        private readonly IBuffer<KeyValuePair<long, TSource>> buffer;

        public OrderableDynamicPartitioner(IEnumerable<TSource> source)
            : base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: true, keysNormalized: true)
        {
            long index = -1;
            this.buffer = source
                .Select(value => new KeyValuePair<long, TSource>(Interlocked.Increment(ref index), value))
                .Share();
        }

        public override bool SupportsDynamicPartitions => true;

        public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions
            (int partitionCount) => Enumerable
                .Range(0, partitionCount)
                .Select(_ => this.buffer.GetEnumerator())
                .ToArray();

        public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions() => this.buffer;
    }

    internal static partial class Partitioning
    {
        internal static void PartitionerAsOrdered()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 2).ToArray();
            new OrderableDynamicPartitioner<int>(source)
                .AsParallel()
                .Select(value => value + Compute())
                .WriteLines(); // 1 0 5 3 4 6 2 7

            new OrderableDynamicPartitioner<int>(source)
                .AsParallel()
                .AsOrdered()
                .Select(value => value + Compute())
                .WriteLines(); // 0 ... 7

            new DynamicPartitioner<int>(source)
                .AsParallel()
                .AsOrdered()
                .Select(value => value + Compute())
                .WriteLines();
            // InvalidOperationException: AsOrdered may not be used with a partitioner that is not orderable.
        }
    }
}

#if DEMO
namespace System.Collections.Concurrent
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class OrderablePartitioner<TSource> : Partitioner<TSource>
    {
        protected OrderablePartitioner(bool keysOrderedInEachPartition, bool keysOrderedAcrossPartitions, bool keysNormalized)
        {
            this.KeysOrderedInEachPartition = keysOrderedInEachPartition;
            this.KeysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
            this.KeysNormalized = keysNormalized;
        }

        public bool KeysNormalized { get; }

        public bool KeysOrderedInEachPartition { get; }

        public bool KeysOrderedAcrossPartitions { get; }

        public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);

        public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
        {
            throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");
        }

        public override IList<IEnumerator<TSource>> GetPartitions
            (int partitionCount) => this.GetOrderablePartitions(partitionCount)
                .Select(partition => new EnumeratorDropIndices(partition))
                .ToArray();


        public override IEnumerable<TSource> GetDynamicPartitions
            () => new EnumerableDropIndices(this.GetOrderableDynamicPartitions());

        private class EnumerableDropIndices : IEnumerable<TSource>
        {
            private readonly IEnumerable<KeyValuePair<long, TSource>> source;

            public EnumerableDropIndices(IEnumerable<KeyValuePair<long, TSource>> source)
            {
                this.source = source;
            }

            public IEnumerator<TSource> GetEnumerator() => new EnumeratorDropIndices(this.source.GetEnumerator());

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        private class EnumeratorDropIndices : IEnumerator<TSource>
        {
            private readonly IEnumerator<KeyValuePair<long, TSource>> source;

            public TSource Current => this.source.Current.Value;

            object IEnumerator.Current => this.Current;

            public EnumeratorDropIndices(IEnumerator<KeyValuePair<long, TSource>> source)
            {
                this.source = source;
            }

            public bool MoveNext() => this.source.MoveNext();

            public void Dispose() => this.source.Dispose();

            public void Reset() => this.source.Reset();
        }
    }
}
#endif
