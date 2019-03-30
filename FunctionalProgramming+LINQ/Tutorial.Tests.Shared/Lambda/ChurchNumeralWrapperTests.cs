namespace Tutorial.Tests.LambdaCalculus
{
    using System;

    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchNumeralWrapperTests
    {
        [TestMethod]
        public void IncreaseTest()
        {
            NumeralWrapper numeral = 0U.ChurchWarpper();
            Assert.IsTrue(0U + 1U == ++numeral);
            Assert.IsTrue(1U + 1U == ++numeral);
            Assert.IsTrue(2U + 1U == ++numeral);
            Assert.IsTrue(3U + 1U == ++numeral);
            numeral = 123U.ChurchWarpper();
            Assert.IsTrue(123U + 1U == ++numeral);
        }

        [TestMethod]
        public void AddTest()
        {
            Assert.IsTrue(0U + 0U == 0U.ChurchWarpper() + 0U.ChurchWarpper());
            Assert.IsTrue(0U + 1U == 0U.ChurchWarpper() + 1U.ChurchWarpper());
            Assert.IsTrue(10U + 0U == 10U.ChurchWarpper() + 0U.ChurchWarpper());
            Assert.IsTrue(0U + 10U == 0U.ChurchWarpper() + 10U.ChurchWarpper());
            Assert.IsTrue(1U + 1U == 1U.ChurchWarpper() + 1U.ChurchWarpper());
            Assert.IsTrue(10U + 1U == 10U.ChurchWarpper() + 1U.ChurchWarpper());
            Assert.IsTrue(1U + 10U == 1U.ChurchWarpper() + 10U.ChurchWarpper());
            Assert.IsTrue(3U + 5U == 3U.ChurchWarpper() + 5U.ChurchWarpper());
            Assert.IsTrue(123U + 345U == 123U.ChurchWarpper() + 345U.ChurchWarpper());
        }

        [TestMethod]
        public void DecreaseTest()
        {
            NumeralWrapper numeral = 3U.ChurchWarpper();
            Assert.IsTrue(3U - 1U == --numeral);
            Assert.IsTrue(2U - 1U == --numeral);
            Assert.IsTrue(1U - 1U == --numeral);
            Assert.IsTrue(0U == --numeral);
            numeral = 123U.ChurchWarpper();
            Assert.IsTrue(123U - 1U == --numeral);
        }

        [TestMethod]
        public void SubtractTest()
        {
            Assert.IsTrue(0U - 0U == 0U.ChurchWarpper() - 0U.ChurchWarpper());
            Assert.IsTrue(0U == 0U.ChurchWarpper() - 1U.ChurchWarpper());
            Assert.IsTrue(10U - 0U == 10U.ChurchWarpper() - 0U.ChurchWarpper());
            Assert.IsTrue(0U == 0U.ChurchWarpper() - 10U.ChurchWarpper());
            Assert.IsTrue(1U - 1U == 1U.ChurchWarpper() - 1U.ChurchWarpper());
            Assert.IsTrue(10U - 1U == 10U.ChurchWarpper() - 1U.ChurchWarpper());
            Assert.IsTrue(0U == 1U.ChurchWarpper() - 10U.ChurchWarpper());
            Assert.IsTrue(0U == 3U.ChurchWarpper() - 5U.ChurchWarpper());
            Assert.IsTrue(0U == 123U.ChurchWarpper() - 345U.ChurchWarpper());
        }

        [TestMethod]
        public void MultiplyTest()
        {
            Assert.IsTrue(0U * 0U == 0U.ChurchWarpper() * 0U.ChurchWarpper());
            Assert.IsTrue(0U * 1U == 0U.ChurchWarpper() * 1U.ChurchWarpper());
            Assert.IsTrue(10U * 0U == 10U.ChurchWarpper() * 0U.ChurchWarpper());
            Assert.IsTrue(0U * 10U == 0U.ChurchWarpper() * 10U.ChurchWarpper());
            Assert.IsTrue(1U * 1U == 1U.ChurchWarpper() * 1U.ChurchWarpper());
            Assert.IsTrue(10U * 1U == 10U.ChurchWarpper() * 1U.ChurchWarpper());
            Assert.IsTrue(1U * 10U == 1U.ChurchWarpper() * 10U.ChurchWarpper());
            Assert.IsTrue(3U * 5U == 3U.ChurchWarpper() * 5U.ChurchWarpper());
            Assert.IsTrue(12U * 23U == 12U.ChurchWarpper() * 23U.ChurchWarpper());
        }

        [TestMethod]
        public void PowTest()
        {
            Assert.IsTrue((uint)Math.Pow(0U, 1U) == (0U.ChurchWarpper() ^ 1U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(10U, 0U) == (10U.ChurchWarpper() ^ 0U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(0U, 10U) == (0U.ChurchWarpper() ^ 10U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(1U, 1U) == (1U.ChurchWarpper() ^ 1U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(10U, 1U) == (10U.ChurchWarpper() ^ 1U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(1U, 10U) == (1U.ChurchWarpper() ^ 10U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(3U, 5U) == (3U.ChurchWarpper() ^ 5U.ChurchWarpper()));
            Assert.IsTrue((uint)Math.Pow(5U, 3U) == (5U.ChurchWarpper() ^ 3U.ChurchWarpper()));
        }

        [TestMethod]
        public void FactorialTest()
        {
            Func<uint, uint> factorial = null; // Must have to be compiled.
            factorial = x => x == 0 ? 1U : x * factorial(x - 1U);

            Assert.IsTrue(factorial(0U) == 0U.ChurchWarpper().Factorial());
            Assert.IsTrue(factorial(1U) == 1U.ChurchWarpper().Factorial());
            Assert.IsTrue(factorial(2U) == 2U.ChurchWarpper().Factorial());
            Assert.IsTrue(factorial(3U) == 3U.ChurchWarpper().Factorial());
            Assert.IsTrue(factorial(7U) == 7U.ChurchWarpper().Factorial());
        }

        [TestMethod]
        public void FibonacciTest()
        {
            Func<uint, uint> fibonacci = null; // Must have. So that fibonacci can recursively refer itself.
            fibonacci = x => x > 1U ? fibonacci(x - 1) + fibonacci(x - 2) : x;

            Assert.IsTrue(fibonacci(0U) == 0U.ChurchWarpper().Fibonacci());
            Assert.IsTrue(fibonacci(1U) == 1U.ChurchWarpper().Fibonacci());
            Assert.IsTrue(fibonacci(2U) == 2U.ChurchWarpper().Fibonacci());
            Assert.IsTrue(fibonacci(3U) == 3U.ChurchWarpper().Fibonacci());
            Assert.IsTrue(fibonacci(10U) == 10U.ChurchWarpper().Fibonacci());
        }

        [TestMethod]
        public void DivideByTest()
        {
            Assert.IsTrue(1U / 1U == 1U.ChurchWarpper().DivideBy(1U.ChurchWarpper()));
            Assert.IsTrue(1U / 2U == 1U.ChurchWarpper().DivideBy(2U.ChurchWarpper()));
            Assert.IsTrue(2U / 2U == 2U.ChurchWarpper().DivideBy(2U.ChurchWarpper()));
            Assert.IsTrue(2U / 1U == 2U.ChurchWarpper().DivideBy(1U.ChurchWarpper()));
            Assert.IsTrue(10U / 3U == 10U.ChurchWarpper().DivideBy(3U.ChurchWarpper()));
            Assert.IsTrue(3U / 10U == 3U.ChurchWarpper().DivideBy(10U.ChurchWarpper()));
        }
    }
}
