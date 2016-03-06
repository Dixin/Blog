namespace Dixin.Tests.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Dixin.Linq.Parallel;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PartitioningTests
    {
        [TestMethod]
        public void BuiltInPartitioningTest()
        {
            Partitioning.Range();
            Partitioning.Chunk();
            Partitioning.Strip();
            Partitioning.StripLoadBalance();
            Partitioning.Hash();
        }

        [TestMethod]
        public void PartitionerTest()
        {
            Partitioning.Partitioner();

            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(1, valueCount);
            IEnumerable<int> partitionsSource = new IxPartitioner<int>(source).GetDynamicPartitions();
            IEnumerable<int> values = GetPartitions(partitionsSource, partitionCount).Concat().OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }

        [TestMethod]
        public void OrderablePartitionerTest()
        {
            Partitioning.IxOrderablePartitioner();

            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(0, valueCount);
            IEnumerable<KeyValuePair<long, int>> partitionsSource = new IxOrderablePartitioner<int>(source).GetOrderableDynamicPartitions();
            IEnumerable<KeyValuePair<long, int>> result = GetPartitions(partitionsSource, partitionCount).Concat();
            IOrderedEnumerable<int> indexes = result.Select(value => Convert.ToInt32(value.Key)).OrderBy(index => index);
            EnumerableAssert.AreSequentialEqual(source, indexes);
            IOrderedEnumerable<int> values = result.Select(value => value.Value).OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }

        private static IList<IList<TSource>> GetPartitions<TSource>(IEnumerable<TSource> partitionsSource, int partitionCount)
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
