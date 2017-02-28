namespace Tutorial.Tests.ParallelLinq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Tutorial.ParallelLinq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using EnumerableAssert = Tutorial.LinqToObjects.EnumerableAssert;

    public partial class QueryMethodsTests
    {
        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.SelectWithIndex();
            QueryMethods.AsOrdered();
            QueryMethods.AsUnordered();
            QueryMethods.OrderBy();
            QueryMethods.Correctness();
            QueryMethods.JoinAsUnordered();
            QueryMethods.MergeOptionForOrder();
        }

        [TestMethod]
        public void PartitionerTest()
        {
            try
            {
                Partitioning.PartitionerAsOrdered();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void OrderablePartitionerTest()
        {
            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(0, valueCount);
            IEnumerable<KeyValuePair<long, int>> partitionsSource = new OrderableDynamicPartitioner<int>(source).GetOrderableDynamicPartitions();
            IEnumerable<KeyValuePair<long, int>> result = Partitioning.GetPartitions(partitionsSource, partitionCount).Concat();
            IOrderedEnumerable<int> indexes = result.Select(value => Convert.ToInt32(value.Key)).OrderBy(index => index);
            EnumerableAssert.AreSequentialEqual(source, indexes);
            IOrderedEnumerable<int> values = result.Select(value => value.Value).OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }
    }
}
