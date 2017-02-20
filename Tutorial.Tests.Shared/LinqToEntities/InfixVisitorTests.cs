namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Linq.Expressions;

    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InfixVisitorTests
    {
        [TestMethod]
        public void VisitBodyTest()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            InfixVisitor prefixVisitor = new InfixVisitor();
            Assert.AreEqual("(((@a + @b) - ((@c * @d) / 2)) + (@e * 3))", prefixVisitor.VisitBody(expression));
        }
    }
}
