namespace Dixin.Linq.CategoryTheory.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BinaryFunctorTests
    {
        [TestMethod]
        public void LazyTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Lazy<int, string> lazyBinaryFunctor = new Lazy<int, string>(1, "abc");
            Func<int, bool> selector1 = x => { isExecuted1 = true; return x > 0; };
            Func<string, int> selector2 = x => { isExecuted2 = true; return x.Length; };

            Lazy<bool, int> query = lazyBinaryFunctor.Select(selector1, selector2);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.

            Assert.AreEqual(true, query.Value1); // Execution.
            Assert.AreEqual("abc".Length, query.Value2); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
        }
    }
}