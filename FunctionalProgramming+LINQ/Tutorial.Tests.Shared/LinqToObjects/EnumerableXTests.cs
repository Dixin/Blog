namespace Tutorial.Tests.LinqToObjects
{
    using System;
    using System.Collections.Generic;

    using Tutorial.LinqToObjects;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Enumerable = System.Linq.Enumerable;
    using EnumerableEx = System.Linq.EnumerableEx;

    [TestClass]
    public class EnumerableXTests
    {
        [TestMethod]
        public void CatchTest()
        {
            EnumerableEx
                .Throw<int>(new OperationCanceledException())
                .Catch<int, OperationCanceledException>(exception => Assert.IsNotNull(exception))
                .ForEach();

            try
            {
                EnumerableEx
                    .Throw<int>(new OperationCanceledException())
                    .Catch<int, OperationCanceledException>(exception =>
                        {
                            Assert.IsNotNull(exception);
                            return false;
                        })
                    .ForEach();
                Assert.Fail();
            }
            catch (OperationCanceledException exception)
            {
                Assert.IsNotNull(exception);
            }
        }

        [TestMethod]
        public void RetryTest()
        {
            int count = 0;
            int[] retry = EnumerableX.Create(() =>
                {
                    count++;
                    if (count < 5)
                    {
                        throw new OperationCanceledException();
                    }
                    return count;
                })
                .Take(2)
                .Retry<int, OperationCanceledException>(5)
                .ToArray();
            EnumerableAssert.AreSequentialEqual(new int[] { 5, 6 }, retry);
        }

        [TestMethod]
        public void InsertTest()
        {
            try
            {
                int[] insert = Enumerable.Range(0, 5).Insert(-1, 5).ToArray();
                Assert.Fail(string.Join(", ", insert.Select(int32 => int32.ToString())));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("index", exception.ParamName);
            }
            EnumerableAssert.AreSequentialEqual(new int[] { 5, 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).Insert(0, 5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 5, 1, 2, 3, 4 }, Enumerable.Range(0, 5).Insert(1, 5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 5, 2, 3, 4 }, Enumerable.Range(0, 5).Insert(2, 5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 5, 3, 4 }, Enumerable.Range(0, 5).Insert(3, 5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 5, 4 }, Enumerable.Range(0, 5).Insert(4, 5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4, 5 }, Enumerable.Range(0, 5).Insert(5, 5));
            try
            {
                int[] insert = Enumerable.Range(0, 5).Insert(6, 5).ToArray();
                Assert.Fail(string.Join(", ", insert.Select(int32 => int32.ToString())));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("index", exception.ParamName);
            }
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            try
            {
                int[] removeAt = Enumerable.Range(0, 5).RemoveAt(-1).ToArray();
                Assert.Fail(string.Join(", ", removeAt.Select(int32 => int32.ToString())));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("index", exception.ParamName);
            }
            EnumerableAssert.AreSequentialEqual(new int[] { 1, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAt(0));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAt(1));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 3, 4 }, Enumerable.Range(0, 5).RemoveAt(2));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 4 }, Enumerable.Range(0, 5).RemoveAt(3));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3 }, Enumerable.Range(0, 5).RemoveAt(4));
            try
            {
                int[] removeAt = Enumerable.Range(0, 5).RemoveAt(5).ToArray();
                Assert.Fail(string.Join(", ", removeAt.Select(int32 => int32.ToString())));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("index", exception.ParamName);
            }
            try
            {
                int[] removeAt = Enumerable.Range(0, 5).RemoveAt(6).ToArray();
                Assert.Fail(string.Join(", ", removeAt.Select(int32 => int32.ToString())));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("index", exception.ParamName);
            }
        }

        [TestMethod]
        public void RemoveTest()
        {
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).Remove(-1));
            EnumerableAssert.AreSequentialEqual(new int[] { 1, 2, 3, 4 }, Enumerable.Range(0, 5).Remove(0));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 2, 3, 4 }, Enumerable.Range(0, 5).Remove(1));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 3, 4 }, Enumerable.Range(0, 5).Remove(2));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 4 }, Enumerable.Range(0, 5).Remove(3));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3 }, Enumerable.Range(0, 5).Remove(4));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).Remove(5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).Remove(6));
        }

        [TestMethod]
        public void RemoveAllTest()
        {
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAll(-1));
            EnumerableAssert.AreSequentialEqual(new int[] { 1, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAll(0));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAll(1));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 3, 4 }, Enumerable.Range(0, 5).RemoveAll(2));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 4 }, Enumerable.Range(0, 5).RemoveAll(3));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3 }, Enumerable.Range(0, 5).RemoveAll(4));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAll(5));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 4 }, Enumerable.Range(0, 5).RemoveAll(6));
            EnumerableAssert.AreSequentialEqual(new int[] { }, Enumerable.Repeat(0, 5).RemoveAll(0));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 0, 0, 0, 0 }, Enumerable.Repeat(0, 5).RemoveAll(1));
            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3, 0, 1, 2, 3 }, Enumerable.Range(0, 5).Concat(Enumerable.Range(0, 5)).RemoveAll(4));
            EnumerableAssert.AreSequentialEqual(new int[] { 1, 2, 3, 4, 1, 2, 3, 4 }, Enumerable.Range(0, 5).Concat(Enumerable.Range(0, 5)).RemoveAll(0));
        }

        [TestMethod]
        public void IndexOfTest()
        {
            Assert.AreEqual(-1, Enumerable.Range(0, 5).IndexOf(-1));
            Assert.AreEqual(-1, Enumerable.Range(0, 5).IndexOf(6));
            Assert.AreEqual(0, Enumerable.Range(0, 5).IndexOf(0));
            Assert.AreEqual(1, Enumerable.Range(0, 5).IndexOf(1));
            Assert.AreEqual(-1, Enumerable.Repeat("a", 5).IndexOf("A"));
            Assert.AreEqual(0, Enumerable.Repeat("a", 5).IndexOf("A", comparer: StringComparer.OrdinalIgnoreCase));
            Assert.AreEqual(2, Enumerable.Repeat("a", 5).IndexOf("A", 2, comparer: StringComparer.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void LastIndexOfTest()
        {
            Assert.AreEqual(-1, Enumerable.Range(0, 5).LastIndexOf(-1));
            Assert.AreEqual(-1, Enumerable.Range(0, 5).LastIndexOf(6));
            Assert.AreEqual(0, Enumerable.Range(0, 5).LastIndexOf(0));
            Assert.AreEqual(1, Enumerable.Range(0, 5).LastIndexOf(1));
            Assert.AreEqual(4, Enumerable.Repeat(0, 5).LastIndexOf(0));
            Assert.AreEqual(6, Enumerable.Repeat(0, 5).Concat(Enumerable.Range(0, 5)).LastIndexOf(1));
            Assert.AreEqual(-1, Enumerable.Repeat("a", 5).LastIndexOf("A"));
            Assert.AreEqual(4, Enumerable.Repeat("a", 5).LastIndexOf("A", comparer: StringComparer.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PercentileExclusiveTest()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5).ToArray();
            try
            {
                double percentile = source.PercentileExclusive(int32 => int32, 0);
                Assert.Fail($"{percentile}");
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("percentile", exception.ParamName);
            }
            Assert.AreEqual(0.2, source.PercentileExclusive(int32 => int32, 0.2), 0.000000001);
            Assert.AreEqual(2, source.PercentileExclusive(int32 => int32, 0.5), 0.000000001);
            Assert.AreEqual(2.6, source.PercentileExclusive(int32 => int32, 0.6), 0.000000001);
            Assert.AreEqual(3.98, source.PercentileExclusive(int32 => int32, 0.83), 0.000000001);
            try
            {
                double percentile = source.PercentileExclusive(int32 => int32, 0.84);
                Assert.Fail($"{percentile}");
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("percentile", exception.ParamName);
            }
            try
            {
                double percentile = source.PercentileExclusive(int32 => int32, 0.95);
                Assert.Fail($"{percentile}");
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Assert.AreEqual("percentile", exception.ParamName);
            }
        }

        [TestMethod]
        public void PercentileInclusiveTest()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5).ToArray();
            Assert.AreEqual(0, source.PercentileInclusive(int32 => int32, 0));
            Assert.AreEqual(0.8, source.PercentileInclusive(int32 => int32, 0.2));
            Assert.AreEqual(2, source.PercentileInclusive(int32 => int32, 0.5));
            Assert.AreEqual(2.4, source.PercentileInclusive(int32 => int32, 0.6));
            Assert.AreEqual(3.32, source.PercentileInclusive(int32 => int32, 0.83));
            Assert.AreEqual(3.36, source.PercentileInclusive(int32 => int32, 0.84));
            Assert.AreEqual(3.8, source.PercentileInclusive(int32 => int32, 0.95));
        }

        [TestMethod]
        public void PercentileTest()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5).ToArray();
            Assert.AreEqual(0, source.Percentile(int32 => int32, 0));
            Assert.AreEqual(0.8, source.Percentile(int32 => int32, 0.2));
            Assert.AreEqual(2, source.Percentile(int32 => int32, 0.5));
            Assert.AreEqual(2.4, source.Percentile(int32 => int32, 0.6));
            Assert.AreEqual(3.32, source.Percentile(int32 => int32, 0.83));
            Assert.AreEqual(3.36, source.Percentile(int32 => int32, 0.84));
            Assert.AreEqual(3.8, source.Percentile(int32 => int32, 0.95));
        }

        [TestMethod]
        public void VariancePopulationTest()
        {
            Assert.AreEqual(8.25, Enumerable.Range(0, 10).VariancePopulation(int32 => int32));
        }

        [TestMethod]
        public void VarianceSampleTest()
        {
            Assert.AreEqual(9.166666667, Enumerable.Range(0, 10).VarianceSample(int32 => int32), 0.000000001);
        }

        [TestMethod]
        public void VarianceTest()
        {
            Assert.AreEqual(9.166666667, Enumerable.Range(0, 10).Variance(int32 => int32), 0.000000001);
        }
    }
}
