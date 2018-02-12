namespace Tutorial.Tests.Functional
{
    using System;
    using System.Linq.Expressions;

    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BinaryArithmeticCompilerTests
    {
        [TestMethod]
        public void CompileTest()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            Func<double, double, double, double, double, double> expected = expression.Compile();
#if !__IOS__
            Func<double, double, double, double, double, double> actual = BinaryArithmeticCompiler.Compile(expression);
            Assert.AreEqual(expected(1, 2, 3, 4, 5), actual(1, 2, 3, 4, 5));
#endif
        }
    }
}
