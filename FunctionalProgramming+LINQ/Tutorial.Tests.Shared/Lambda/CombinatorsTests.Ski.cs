namespace Tutorial.Tests.LambdaCalculus
{
    using System;

    using Tutorial.LambdaCalculus;

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

            string x2 = "a";
            int y2 = 1;
            Assert.AreEqual(x2, (string)SkiCombinators.K(x2)(y2));
            Assert.AreEqual(x2, (string)SkiCombinators.I(x2));
            Assert.AreEqual(y2, (int)SkiCombinators.I(y2));
        }

        [TestMethod]
        public void BooleanTests()
        {
            Func<int, Func<int, int>> t1 = a => b => a + b;
            int f1 = 1;
            Assert.AreEqual(ChurchBoolean.True(t1)(f1), SkiCalculus.True(t1)(f1));
            Assert.AreEqual(ChurchBoolean.False(t1)(f1), SkiCalculus.False(t1)(f1));

            Func<int, int> t2 = a => a + 1;
            int f2 = 2;
            Assert.AreEqual(ChurchBoolean.True(t2)(f2), SkiCalculus.True(t2)(f2));
            Assert.AreEqual(ChurchBoolean.False(t2)(f2), SkiCalculus.False(t2)(f2));
        }

        [TestMethod]
        public void NumeralTests()
        {
            Assert.AreEqual(0U, SkiCalculus.UnchurchNumeral(SkiCalculus.Zero));
            Assert.AreEqual(1U, SkiCalculus.UnchurchNumeral(SkiCalculus.One));
            Assert.AreEqual(2U, SkiCalculus.UnchurchNumeral(SkiCalculus.Two));
            Assert.AreEqual(3U, SkiCalculus.UnchurchNumeral(SkiCalculus.Three));
            Assert.AreEqual(4U, SkiCalculus.UnchurchNumeral(SkiCalculus.Increase(SkiCalculus.Three)));
        }
    }
}