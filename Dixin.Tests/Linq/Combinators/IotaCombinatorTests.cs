namespace Dixin.Tests.Linq.Combinators
{
    using System;

    using Dixin.Linq.Combinators;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IotaCombinatorTests
    {
        [TestMethod]
        public void SkiTests()
        {
            Func<int, Func<int, int>> x1 = a => b => a + b;
            Func<int, int> y1 = a => a + 1;
            int z1 = 1;
            Assert.AreEqual((int)SkiCombinators.S(x1)(y1)(z1), (int)IotaCombinator.S(x1)(y1)(z1));
            Assert.AreEqual((Func<int, Func<int, int>>)SkiCombinators.K(x1)(y1), (Func<int, Func<int, int>>)IotaCombinator.K(x1)(y1));
            Assert.AreEqual((Func<int, Func<int, int>>)SkiCombinators.I(x1), (Func<int, Func<int, int>>)IotaCombinator.I(x1));
            Assert.AreEqual((Func<int, int>)SkiCombinators.I(y1), (Func<int, int>)IotaCombinator.I(y1));
            Assert.AreEqual((int)SkiCombinators.I(z1), (int)IotaCombinator.I(z1));

            string x2 = "a";
            int y2 = 1;
            Assert.AreEqual((string)SkiCombinators.K(x2)(y2), (string)IotaCombinator.K(x2)(y2));
            Assert.AreEqual((string)SkiCombinators.I(x2), (string)IotaCombinator.I(x2));
            Assert.AreEqual((int)SkiCombinators.I(y2), (int)IotaCombinator.I(y2));
        }
    }
}
