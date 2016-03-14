namespace Dixin.Tests.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            Partitioning.Partition();

            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(1, valueCount);
            IEnumerable<int> partitionsSource = new IxPartitioner<int>(source).GetDynamicPartitions();
            IEnumerable<int> values = Partitioning.GetPartitions(partitionsSource, partitionCount).Concat().OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }
    }
}
