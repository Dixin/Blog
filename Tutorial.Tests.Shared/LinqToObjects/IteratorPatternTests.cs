namespace Tutorial.Tests.LinqToObjects
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToObjects;

    using Enumerable = System.Linq.Enumerable;
    using EnumerableEx = System.Linq.EnumerableEx;

    [TestClass]
    public class IteratorPatternTests
    {
        [TestMethod]
        public void EnumerableTest()
        {
            EnumerableAssert.AreSequentialEqual(
                EnumerableEx.Return(1),
                IteratorPattern.FromValue(1));

            object value = new object();
            EnumerableAssert.AreSequentialEqual(
                EnumerableEx.Return(value),
                IteratorPattern.FromValue(value));
        }

        [TestMethod]
        public void RepeatTest()
        {
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Repeat(1, 5),
                IteratorPattern.Repeat(1, 5));
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Repeat(1, 0),
                IteratorPattern.Repeat(1, 0));
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Repeat(1, 1),
                IteratorPattern.Repeat(1, 1));

            object value = new object();
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Repeat(value, 10),
                IteratorPattern.Repeat(value, 10));
        }

        [TestMethod]
        public void SelectTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Select(enumerable, x => x.ToString()),
                IteratorPattern.Select(enumerable, x => x.ToString()));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Select(enumerable, x => x.ToString()),
                IteratorPattern.Select(enumerable, x => x.ToString()));
        }

        [TestMethod]
        public void WhereTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Where(enumerable, x => x > 0),
                IteratorPattern.Where(enumerable, x => x > 0));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Where(enumerable, x => x > 0),
                IteratorPattern.Where(enumerable, x => x > 0));
        }

        [TestMethod]
        public void SequenceTest()
        {
            IteratorPattern.NonGenericSequences();
        }
    }
}
