namespace Dixin.Tests.Linq.LinqToObjects
{
    using System.Linq;

    using Dixin.Linq.LinqToObjects;
    using Dixin.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IteratorPatternTests
    {
        [TestMethod]
        public void EnumerableTest()
        {
            EnumerableAssert.AreEqual(
                EnumerableEx.Return(1),
                IteratorPattern.Enumerable(1));

            object value = new object();
            EnumerableAssert.AreEqual(
                EnumerableEx.Return(value),
                IteratorPattern.Enumerable(value));
        }

        [TestMethod]
        public void RepeatTest()
        {
            EnumerableAssert.AreEqual(
                Enumerable.Repeat(1, 5),
                IteratorPattern.Repeat(1, 5));
            EnumerableAssert.AreEqual(
                Enumerable.Repeat(1, 0),
                IteratorPattern.Repeat(1, 0));
            EnumerableAssert.AreEqual(
                Enumerable.Repeat(1, 1),
                IteratorPattern.Repeat(1, 1));

            object value = new object();
            EnumerableAssert.AreEqual(
                Enumerable.Repeat(value, 10),
                IteratorPattern.Repeat(value, 10));
        }

        [TestMethod]
        public void SelectTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreEqual(
                Enumerable.Select(enumerable, x => x.ToString()),
                IteratorPattern.Select(enumerable, x => x.ToString()));

            enumerable = new int[] { };
            EnumerableAssert.AreEqual(
                Enumerable.Select(enumerable, x => x.ToString()),
                IteratorPattern.Select(enumerable, x => x.ToString()));
        }

        [TestMethod]
        public void WhereTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreEqual(
                Enumerable.Where(enumerable, x => x > 0),
                IteratorPattern.Where(enumerable, x => x > 0));

            enumerable = new int[] { };
            EnumerableAssert.AreEqual(
                Enumerable.Where(enumerable, x => x > 0),
                IteratorPattern.Where(enumerable, x => x > 0));
        }
    }
}
