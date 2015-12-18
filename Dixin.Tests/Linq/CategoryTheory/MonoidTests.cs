namespace Dixin.Linq.CategoryTheory.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MonoidTests
    {
        [TestMethod]
        public void StringTest()
        {
            IMonoid<string> concatString = string.Empty.Monoid(string.Concat);
            Assert.AreEqual(string.Empty, concatString.Unit);
            Assert.AreEqual("ab", concatString.Binary("a", "b"));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual("ab", concatString.Binary(concatString.Unit, "ab"));
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual("ab", concatString.Binary("ab", concatString.Unit));
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(concatString.Binary(concatString.Binary("a", "b"), "c"), concatString.Binary("a", concatString.Binary("b", "c")));
        }

        [TestMethod]
        public void Int32Test()
        {
            IMonoid<int> addInt32 = 0.Monoid((a, b) => a + b);
            Assert.AreEqual(0, addInt32.Unit);
            Assert.AreEqual(1 + 2, addInt32.Binary(1, 2));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(1, addInt32.Binary(addInt32.Unit, 1));
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(1, addInt32.Binary(1, addInt32.Unit));
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(addInt32.Binary(addInt32.Binary(1, 2), 3), addInt32.Binary(1, addInt32.Binary(2, 3)));

            IMonoid<int> multiplyInt32 = 1.Monoid((a, b) => a * b);
            Assert.AreEqual(1, multiplyInt32.Unit);
            Assert.AreEqual(1 * 2, multiplyInt32.Binary(1, 2));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(2, multiplyInt32.Binary(multiplyInt32.Unit, 2));
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(2, multiplyInt32.Binary(2, multiplyInt32.Unit));
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(multiplyInt32.Binary(multiplyInt32.Binary(1, 2), 3), multiplyInt32.Binary(1, multiplyInt32.Binary(2, 3)));
        }

        [TestMethod]
        public void ClockTest()
        {
            // Stolen from: http://channel9.msdn.com/Shows/Going+Deep/Brian-Beckman-Dont-fear-the-Monads
            IMonoid<int> clock = 12.Monoid((a, b) => (a + b) % 12);
            Assert.AreEqual(12, clock.Unit);
            Assert.AreEqual((7 + 10) % 12, clock.Binary(7, 10));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(111 % 12, clock.Binary(clock.Unit, 111));
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(111 % 12, clock.Binary(111, clock.Unit));
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(clock.Binary(clock.Binary(11, 22), 33), clock.Binary(11, clock.Binary(22, 33)));
        }

        [TestMethod]
        public void BooleanTest()
        {
            IMonoid<bool> orBoolean = false.Monoid((a, b) => a || b);
            Assert.IsFalse(orBoolean.Unit);
            Assert.AreEqual(true || false, orBoolean.Binary(true, false));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(true, orBoolean.Binary(orBoolean.Unit, true));
            Assert.AreEqual(false, orBoolean.Binary(orBoolean.Unit, false));
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(true, orBoolean.Binary(true, orBoolean.Unit));
            Assert.AreEqual(false, orBoolean.Binary(false, orBoolean.Unit));
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(orBoolean.Binary(orBoolean.Binary(true, false), true), orBoolean.Binary(true, orBoolean.Binary(false, true)));

            IMonoid<bool> andBoolean = true.Monoid((a, b) => a && b);
            Assert.IsTrue(andBoolean.Unit);
            Assert.AreEqual(true && false, andBoolean.Binary(true, false));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(true, andBoolean.Binary(andBoolean.Unit, true));
            Assert.AreEqual(false, andBoolean.Binary(andBoolean.Unit, false));
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(true, andBoolean.Binary(true, andBoolean.Unit));
            Assert.AreEqual(false, andBoolean.Binary(false, andBoolean.Unit));
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(andBoolean.Binary(andBoolean.Binary(true, false), true), andBoolean.Binary(true, andBoolean.Binary(false, true)));
        }

        [TestMethod]
        public void EnumerableTest()
        {
            IMonoid<IEnumerable<int>> concatEnumerable = Enumerable.Empty<int>().Monoid((a, b) => a.Concat(b));
            Assert.IsFalse(concatEnumerable.Unit.Any());
            int[] x = new int[] { 0, 1, 2 };
            int[] y = new int[] { 3, 4, 5 };
            EnumerableAssert.AreEqual(concatEnumerable.Binary(x, y), x.Concat(y));

            // Monoid law 1: Unit Binary m == m
            EnumerableAssert.AreEqual(concatEnumerable.Binary(concatEnumerable.Unit, x), x);
            // Monoid law 2: m Binary Unit == m
            EnumerableAssert.AreEqual(concatEnumerable.Binary(x, concatEnumerable.Unit), x);
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            EnumerableAssert.AreEqual(
                concatEnumerable.Binary(concatEnumerable.Binary(x, y), x),
                concatEnumerable.Binary(x, concatEnumerable.Binary(y, x)));
        }

        [TestMethod]
        public void NullableTest()
        {
            IMonoid<int> addInt32 = 0.Monoid((a, b) => a + b);
            IMonoid<CategoryTheory.Nullable<int>> addNullable = addInt32.MonoidOfNullable();
            Assert.IsFalse(addNullable.Unit.HasValue);
            Assert.AreEqual(addInt32.Binary(1, 2), addNullable.Binary(1.Nullable(), 2.Nullable()).Value);
            Assert.AreEqual(1, addNullable.Binary(1.Nullable(), new CategoryTheory.Nullable<int>()).Value);
            Assert.AreEqual(2, addNullable.Binary(new CategoryTheory.Nullable<int>(), 2.Nullable()).Value);
            Assert.IsFalse(addNullable.Binary(new CategoryTheory.Nullable<int>(), new CategoryTheory.Nullable<int>()).HasValue);

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(1, addNullable.Binary(addNullable.Unit, 1.Nullable()).Value);
            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(1, addNullable.Binary(1.Nullable(), addNullable.Unit).Value);
            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            CategoryTheory.Nullable<int> left = addNullable.Binary(addNullable.Binary(1.Nullable(), 2.Nullable()), 3.Nullable());
            CategoryTheory.Nullable<int> right = addNullable.Binary(1.Nullable(), addNullable.Binary(2.Nullable(), 3.Nullable()));
            Assert.AreEqual(left.Value, right.Value);
        }
    }
}
