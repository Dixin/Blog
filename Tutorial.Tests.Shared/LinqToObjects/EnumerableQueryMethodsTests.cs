namespace Tutorial.Tests.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Tutorial.LinqToObjects;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class EnumerableQueryMethodsTests
    {
        [TestMethod]
        public void ConcatTest()
        {
            int[] first = new int[] { 0, 1, 2 };
            int[] second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Concat(first, second),
                EnumerableExtensions.Concat(first, second));

            first = new int[] { };
            second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Concat(first, second),
                EnumerableExtensions.Concat(first, second));

            first = new int[] { 0, 1, 2 };
            second = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Concat(first, second),
                EnumerableExtensions.Concat(first, second));
        }

        [TestMethod]
        public void DistinctTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Distinct(enumerable),
                EnumerableExtensions.Distinct(enumerable, EqualityComparer<int>.Default));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Distinct(enumerable),
                EnumerableExtensions.Distinct(enumerable, EqualityComparer<int>.Default));
        }

        [TestMethod]
        public void ExceptTest()
        {
            int[] first = new int[] { 0, 1, 2 };
            int[] second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Except(first, second),
                EnumerableExtensions.Except(first, second, EqualityComparer<int>.Default));

            first = new int[] { 0, 1, 2 };
            second = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Except(first, second),
                EnumerableExtensions.Except(first, second, EqualityComparer<int>.Default));

            first = new int[] { };
            second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Except(first, second),
                EnumerableExtensions.Except(first, second, EqualityComparer<int>.Default));

            first = new int[] { 0, 1, 2 };
            second = new int[] { 2, 3, 4 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Except(first, second),
                EnumerableExtensions.Except(first, second, EqualityComparer<int>.Default));
        }

        [TestMethod]
        public void GroupByTest()
        {
            int[] enumerable = new int[] { 0, 1, 2, 4, 5, 6 };
            IGrouping<int, int>[] expected = Enumerable.ToArray(Enumerable.GroupBy(enumerable, value => value % 3, value => value));
            IGrouping<int, int>[] actual = Enumerable.ToArray(EnumerableExtensions.GroupBy(enumerable, value => value % 3, value => value, EqualityComparer<int>.Default));
            Assert.AreEqual(Enumerable.Count(expected), Enumerable.Count(actual));
            expected.ForEach((group, index) =>
            {
                Assert.AreEqual(group.Key, actual[index].Key);
                EnumerableAssert.AreSequentialEqual(group, actual[index]);
            });

            enumerable = new int[] { };
            expected = Enumerable.ToArray(Enumerable.GroupBy(enumerable, value => value % 3, value => value));
            actual = Enumerable.ToArray(EnumerableExtensions.GroupBy(enumerable, value => value % 3, value => value, EqualityComparer<int>.Default));
            Assert.AreEqual(Enumerable.Count(expected), Enumerable.Count(actual));
            expected.ForEach((group, index) =>
            {
                Assert.AreEqual(group.Key, actual[index].Key);
                EnumerableAssert.AreSequentialEqual(group, actual[index]);
            });
        }

        [TestMethod]
        public void GroupJoinTest()
        {
            Tuple<int, string>[] categories = new Tuple<int, string>[]
                                                {
                                                new Tuple<int, string>(1, "A"),
                                                new Tuple<int, string>(2, "B"),
                                                new Tuple<int, string>(3, "C"),
                                                };
            Tuple<int, string, int>[] products = new Tuple<int, string, int>[]
                                                    {
                                                    new Tuple<int, string, int>(1, "aa", 1),
                                                    new Tuple<int, string, int>(2, "bb", 1),
                                                    new Tuple<int, string, int>(3, "cc", 2),
                                                    new Tuple<int, string, int>(4, "dd", 2),
                                                    new Tuple<int, string, int>(5, "ee", 2),
                                                    };
            Tuple<string, int>[] expected = Enumerable.ToArray(Enumerable.GroupJoin(
                    categories,
                    products,
                    category => category.Item1,
                    product => product.Item3,
                    (category, categoryProducts) => new Tuple<string, int>(category.Item2, EnumerableExtensions.Count(categoryProducts))));
            Tuple<string, int>[] actual = EnumerableExtensions.ToArray(EnumerableExtensions.GroupJoin(
                    categories,
                    products,
                    category => category.Item1,
                    product => product.Item3,
                    (category, categoryProducts) => new Tuple<string, int>(category.Item2, EnumerableExtensions.Count(categoryProducts)),
                    EqualityComparer<int>.Default));
            Assert.AreEqual(Enumerable.Count(expected), Enumerable.Count(actual));
            expected.ForEach((product, index) =>
            {
                Assert.AreEqual(product.Item1, actual[index].Item1);
                Assert.AreEqual(product.Item2, actual[index].Item2);
            });
        }

        [TestMethod]
        public void IntersectTest()
        {
            int[] first = new int[] { 0, 1, 2 };
            int[] second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Intersect(first, second),
                EnumerableExtensions.Intersect(first, second, EqualityComparer<int>.Default));

            first = new int[] { 0, 1, 2 };
            second = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Intersect(first, second),
                EnumerableExtensions.Intersect(first, second, EqualityComparer<int>.Default));

            first = new int[] { };
            second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Intersect(first, second),
                EnumerableExtensions.Intersect(first, second, EqualityComparer<int>.Default));

            first = new int[] { 0, 1, 2 };
            second = new int[] { 2, 3, 4 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Intersect(first, second),
                EnumerableExtensions.Intersect(first, second, EqualityComparer<int>.Default));
        }

        [TestMethod]
        public void JoinTest()
        {
            Tuple<int, string, int>[] products = new Tuple<int, string, int>[]
                                                    {
                                                    new Tuple<int, string, int>(1, "aa", 1),
                                                    new Tuple<int, string, int>(2, "bb", 1),
                                                    new Tuple<int, string, int>(3, "cc", 2),
                                                    new Tuple<int, string, int>(4, "dd", 2),
                                                    new Tuple<int, string, int>(5, "ee", 2),
                                                    };
            Tuple<int, string>[] categories = new Tuple<int, string>[]
                                                {
                                                new Tuple<int, string>(1, "A"),
                                                new Tuple<int, string>(2, "B"),
                                                new Tuple<int, string>(3, "C"),
                                                };
            Tuple<string, string>[] expected = Enumerable.ToArray(Enumerable.Join(
                    products,
                    categories,
                    product => product.Item3,
                    category => category.Item1,
                    (product, category) => new Tuple<string, string>(category.Item2, product.Item2)));
            Tuple<string, string>[] actual = Enumerable.ToArray(EnumerableExtensions.Join(
                    products,
                    categories,
                    product => product.Item3,
                    category => category.Item1,
                    (product, category) => new Tuple<string, string>(category.Item2, product.Item2),
                    EqualityComparer<int>.Default));
            Assert.AreEqual(Enumerable.Count(expected), Enumerable.Count(actual));
            expected.ForEach((product, index) =>
            {
                Assert.AreEqual(product.Item1, actual[index].Item1);
                Assert.AreEqual(product.Item2, actual[index].Item2);
            });

            products = new Tuple<int, string, int>[]
                                                    {
                                                    new Tuple<int, string, int>(1, "aa", 1),
                                                    new Tuple<int, string, int>(2, "bb", 1),
                                                    new Tuple<int, string, int>(3, "cc", 2),
                                                    new Tuple<int, string, int>(4, "dd", 2),
                                                    new Tuple<int, string, int>(5, "ee", 2),
                                                    };
            categories = new Tuple<int, string>[] { };
            expected = Enumerable.ToArray(Enumerable.Join(
                    products,
                    categories,
                    product => product.Item3,
                    category => category.Item1,
                    (product, category) => new Tuple<string, string>(category.Item2, product.Item2)));
            actual = Enumerable.ToArray(EnumerableExtensions.Join(
                    products,
                    categories,
                    product => product.Item3,
                    category => category.Item1,
                    (product, category) => new Tuple<string, string>(category.Item2, product.Item2),
                    EqualityComparer<int>.Default));
            Assert.AreEqual(Enumerable.Count(expected), Enumerable.Count(actual));
            expected.ForEach((product, index) =>
            {
                Assert.AreEqual(product.Item1, actual[index].Item1);
                Assert.AreEqual(product.Item2, actual[index].Item2);
            });

            products = new Tuple<int, string, int>[] { };
            categories = new Tuple<int, string>[]
                                                {
                                                new Tuple<int, string>(1, "A"),
                                                new Tuple<int, string>(1, "B"),
                                                new Tuple<int, string>(1, "C"),
                                                };
            expected = Enumerable.ToArray(Enumerable.Join(
                    products,
                    categories,
                    product => product.Item3,
                    category => category.Item1,
                    (product, category) => new Tuple<string, string>(category.Item2, product.Item2)));
            actual = Enumerable.ToArray(EnumerableExtensions.Join(
                    products,
                    categories,
                    product => product.Item3,
                    category => category.Item1,
                    (product, category) => new Tuple<string, string>(category.Item2, product.Item2),
                    EqualityComparer<int>.Default));
            Assert.AreEqual(Enumerable.Count(expected), Enumerable.Count(actual));
            expected.ForEach((product, index) =>
            {
                Assert.AreEqual(product.Item1, actual[index].Item1);
                Assert.AreEqual(product.Item2, actual[index].Item2);
            });
        }

        [TestMethod]
        public void SelectTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Select(enumerable, x => x.ToString()),
                EnumerableExtensions.Select(enumerable, x => x.ToString()));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Select(enumerable, x => x.ToString()),
                EnumerableExtensions.Select(enumerable, x => x.ToString()));
        }

        [TestMethod]
        public void SkipTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Skip(enumerable, 2),
                EnumerableExtensions.Skip(enumerable, 2));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Skip(enumerable, 0),
                EnumerableExtensions.Skip(enumerable, 0));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Skip(enumerable, -1),
                EnumerableExtensions.Skip(enumerable, -1));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Skip(enumerable, 100),
                EnumerableExtensions.Skip(enumerable, 100));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Skip(enumerable, 100),
                EnumerableExtensions.Skip(enumerable, 100));
        }

        [TestMethod]
        public void SkipWhileTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.SkipWhile(enumerable, x => x > 0),
                EnumerableExtensions.SkipWhile(enumerable, x => x > 0));

            enumerable = new int[] { 2, 1, 0, -1 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.SkipWhile(enumerable, x => x > 0),
                EnumerableExtensions.SkipWhile(enumerable, x => x > 0));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.SkipWhile(enumerable, x => x > 0),
                EnumerableExtensions.SkipWhile(enumerable, x => x > 0));
        }

        [TestMethod]
        public void TakeTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Take(enumerable, 2),
                EnumerableExtensions.Take(enumerable, 2));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Take(enumerable, 0),
                EnumerableExtensions.Take(enumerable, 0));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Take(enumerable, -1),
                EnumerableExtensions.Take(enumerable, -1));

            enumerable = new int[] { 0, 1, 1, 1, 2, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Take(enumerable, 100),
                EnumerableExtensions.Take(enumerable, 100));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Take(enumerable, 100),
                EnumerableExtensions.Take(enumerable, 100));
        }

        [TestMethod]
        public void TakeWhileTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.TakeWhile(enumerable, x => x > 0),
                EnumerableExtensions.TakeWhile(enumerable, x => x > 0));

            enumerable = new int[] { 2, 1, 0, -1 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.TakeWhile(enumerable, x => x > 0),
                EnumerableExtensions.TakeWhile(enumerable, x => x > 0));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.TakeWhile(enumerable, x => x > 0),
                EnumerableExtensions.TakeWhile(enumerable, x => x > 0));
        }

        [TestMethod]
        public void UnionTest()
        {
            int[] first = new int[] { 0, 1, 2 };
            int[] second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Union(first, second),
                EnumerableExtensions.Union(first, second, EqualityComparer<int>.Default));

            first = new int[] { 0, 1, 2 };
            second = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Union(first, second),
                EnumerableExtensions.Union(first, second, EqualityComparer<int>.Default));

            first = new int[] { };
            second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Union(first, second),
                EnumerableExtensions.Union(first, second, EqualityComparer<int>.Default));

            first = new int[] { 0, 1, 2 };
            second = new int[] { 2, 3, 4 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Union(first, second),
                EnumerableExtensions.Union(first, second, EqualityComparer<int>.Default));
        }

        [TestMethod]
        public void WhereTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Where(enumerable, x => x > 0),
                EnumerableExtensions.Where(enumerable, x => x > 0));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Where(enumerable, x => x > 0),
                EnumerableExtensions.Where(enumerable, x => x > 0));
        }

        [TestMethod]
        public void ZipTest()
        {
            int[] first = new int[] { 0, 1, 2 };
            int[] second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Zip(first, second, (x, y) => x * y),
                EnumerableExtensions.Zip(first, second, (x, y) => x * y));

            first = new int[] { 0, 1, 2 };
            second = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Zip(first, second, (x, y) => x * y),
                EnumerableExtensions.Zip(first, second, (x, y) => x * y));

            first = new int[] { };
            second = new int[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Zip(first, second, (x, y) => x * y),
                EnumerableExtensions.Zip(first, second, (x, y) => x * y));

            first = new int[] { 0, 1, 2 };
            second = new int[] { 2, 3, 4 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Zip(first, second, (x, y) => x * y),
                EnumerableExtensions.Zip(first, second, (x, y) => x * y));

            first = new int[] { 0, 1 };
            second = new int[] { 2, 3, 4 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Zip(first, second, (x, y) => x * y),
                EnumerableExtensions.Zip(first, second, (x, y) => x * y));

            first = new int[] { 0, 1, 2 };
            second = new int[] { 2, 3 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Zip(first, second, (x, y) => x * y),
                EnumerableExtensions.Zip(first, second, (x, y) => x * y));
        }
    }
}
