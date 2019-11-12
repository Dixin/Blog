namespace Tutorial.Tests.CategoryTheory
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.CategoryTheory;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class MonoidTests
    {
        [TestMethod]
        public void StringTest()
        {
            IMonoid<string> concatString = new StringConcatMonoid();
            Assert.AreEqual(string.Empty, concatString.Unit());
            Assert.AreEqual("ab", concatString.Multiply("a", "b"));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual("ab", concatString.Multiply(concatString.Unit(), "ab"));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual("ab", concatString.Multiply("ab", concatString.Unit()));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                concatString.Multiply(concatString.Multiply("a", "b"), "c"),
                concatString.Multiply("a", concatString.Multiply("b", "c")));
        }

        [TestMethod]
        public void Int32Test()
        {
            IMonoid<int> addInt32 = new Int32SumMonoid();
            Assert.AreEqual(0, addInt32.Unit());
            Assert.AreEqual(1 + 2, addInt32.Multiply(1, 2));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(1, addInt32.Multiply(addInt32.Unit(), 1));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(1, addInt32.Multiply(1, addInt32.Unit()));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(addInt32.Multiply(addInt32.Multiply(1, 2), 3), addInt32.Multiply(1, addInt32.Multiply(2, 3)));

            IMonoid<int> multiplyInt32 = new Int32ProductMonoid();
            Assert.AreEqual(1, multiplyInt32.Unit());
            Assert.AreEqual(1 * 2, multiplyInt32.Multiply(1, 2));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(2, multiplyInt32.Multiply(multiplyInt32.Unit(), 2));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(2, multiplyInt32.Multiply(2, multiplyInt32.Unit()));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                multiplyInt32.Multiply(multiplyInt32.Multiply(1, 2), 3),
                multiplyInt32.Multiply(1, multiplyInt32.Multiply(2, 3)));
        }

        [TestMethod]
        public void ClockTest()
        {
            // http://channel9.msdn.com/Shows/Going+Deep/Brian-Beckman-Dont-fear-the-Monads
            IMonoid<uint> clock = new ClockMonoid();
            Assert.AreEqual(12U, clock.Unit());
            Assert.AreEqual((7U + 10U) % 12U, clock.Multiply(7U, 10U));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(111U % 12U, clock.Multiply(clock.Unit(), 111U));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(111U % 12U, clock.Multiply(111U, clock.Unit()));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(clock.Multiply(clock.Multiply(11U, 22U), 33U), clock.Multiply(11U, clock.Multiply(22U, 33U)));
        }

        [TestMethod]
        public void BooleanTest()
        {
            IMonoid<bool> orBoolean = new BooleanOrMonoid();
            Assert.IsFalse(orBoolean.Unit());
            Assert.AreEqual(true || false, orBoolean.Multiply(true, false));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(true, orBoolean.Multiply(orBoolean.Unit(), true));
            Assert.AreEqual(false, orBoolean.Multiply(orBoolean.Unit(), false));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(true, orBoolean.Multiply(true, orBoolean.Unit()));
            Assert.AreEqual(false, orBoolean.Multiply(false, orBoolean.Unit()));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                orBoolean.Multiply(orBoolean.Multiply(true, false), true),
                orBoolean.Multiply(true, orBoolean.Multiply(false, true)));

            IMonoid<bool> andBoolean = new BooleanAndMonoid();
            Assert.IsTrue(andBoolean.Unit());
            Assert.AreEqual(true && false, andBoolean.Multiply(true, false));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(true, andBoolean.Multiply(andBoolean.Unit(), true));
            Assert.AreEqual(false, andBoolean.Multiply(andBoolean.Unit(), false));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(true, andBoolean.Multiply(true, andBoolean.Unit()));
            Assert.AreEqual(false, andBoolean.Multiply(false, andBoolean.Unit()));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                andBoolean.Multiply(andBoolean.Multiply(true, false), true),
                andBoolean.Multiply(true, andBoolean.Multiply(false, true)));
        }

        [TestMethod]
        public void EnumerableTest()
        {
            IMonoid<IEnumerable<int>> concatEnumerable = new EnumerableConcatMonoid<int>();
            Assert.IsFalse(concatEnumerable.Unit().Any());
            int[] x = new[] { 0, 1, 2 };
            int[] y = new[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(concatEnumerable.Multiply(x, y), x.Concat(y));

            // Monoid law 1: Unit Binary m == m
            EnumerableAssert.AreSequentialEqual(concatEnumerable.Multiply(concatEnumerable.Unit(), x), x);

            // Monoid law 2: m Binary Unit == m
            EnumerableAssert.AreSequentialEqual(concatEnumerable.Multiply(x, concatEnumerable.Unit()), x);

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            EnumerableAssert.AreSequentialEqual(
                concatEnumerable.Multiply(concatEnumerable.Multiply(x, y), x),
                concatEnumerable.Multiply(x, concatEnumerable.Multiply(y, x)));
        }
    }
}
