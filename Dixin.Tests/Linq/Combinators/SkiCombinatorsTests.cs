namespace Dixin.Tests.Linq.Combinators
{
    using System;

    using Dixin.Linq.Combinators;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SkiCombinatorsTests
    {
        [TestMethod]
        public void SkiTests()
        {
            Func<int, Func<int, int>> x1 = a => b => a + b;
            Func<int, int> y1 = a => a + 1;
            int z1 = 1;
            Assert.AreEqual(x1(z1)(y1(z1)), (int)SkiCombinators.S(x1)(y1)(z1));
            Assert.AreEqual(x1, (Func<int, Func<int, int>>)SkiCombinators.K(x1)(y1));
            Assert.AreEqual(x1, (Func<int, Func<int, int>>)SkiCombinators.I(x1));
            Assert.AreEqual(y1, (Func<int, int>)SkiCombinators.I(y1));
            Assert.AreEqual(z1, (int)SkiCombinators.I(z1));

            string x2 = "a";
            int y2 = 1;
            Assert.AreEqual(x2, (string)SkiCombinators.K(x2)(y2));
            Assert.AreEqual(x2, (string)SkiCombinators.I(x2));
            Assert.AreEqual(y2, (int)SkiCombinators.I(y2));
        }

        [TestMethod]
        public void BooleanTests()
        {
            Assert.AreEqual(true, (bool)SkiCombinators.True(true)(false));
            Assert.AreEqual(false, (bool)SkiCombinators.False(new Func<dynamic, dynamic>(_ => true))(false));
        }

        [TestMethod]
        public void NumeralTests()
        {
            Assert.AreEqual(0U, SkiCombinators._UnchurchNumeral(SkiCombinators.Zero));
            Assert.AreEqual(1U, SkiCombinators._UnchurchNumeral(SkiCombinators.One));
            Assert.AreEqual(2U, SkiCombinators._UnchurchNumeral(SkiCombinators.Two));
            Assert.AreEqual(3U, SkiCombinators._UnchurchNumeral(SkiCombinators.Three));
            Assert.AreEqual(4U, SkiCombinators._UnchurchNumeral(SkiCombinators.Increase(SkiCombinators.Three)));
        }
    }
}