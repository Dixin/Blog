namespace Tutorial.Tests.Functional
{
    using System;
    using System.Linq.Expressions;

    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PrefixVisitorTests
    {
        [TestMethod]
        public void VisitBodyTest()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            PrefixVisitor prefixVisitor = new PrefixVisitor();
            Assert.AreEqual("add(sub(add(a, b), div(mul(c, d), 2)), mul(e, 3))", prefixVisitor.VisitBody(expression));
        }
    }
}
