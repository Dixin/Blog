namespace Dixin.Tests.Linq.LinqToSql
{
    using System;
    using System.Linq.Expressions;

    using Dixin.Linq.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BinaryArithmeticTanslatorTests
    {
        [TestMethod]
        public void TranslateToSql()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            Func<double, double, double, double, double, double> remote = BinaryArithmeticTanslator
                .Translate(expression);
            Func<double, double, double, double, double, double> local = expression.Compile();
            Assert.AreEqual(local(1, 2, 3, 4, 5), remote(1, 2, 3, 4, 5)); // 12
        }
    }
}
