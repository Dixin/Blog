namespace Dixin.Tests.Linq.Lambda
{
    using System;

    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class UInt32Extensions
    {
        public static uint Pow(this uint mantissa, uint exponent)
        {
            uint result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= mantissa;
            }
            return result;
        }
    }

    [TestClass]
    public class ChurchNumeralTests
    {
        [TestMethod]
        public void IncreaseTest()
        {
            Numeral numeral = 0U.Church();
            Assert.IsTrue(0U + 1U == ++numeral);
            Assert.IsTrue(1U + 1U == ++numeral);
            Assert.IsTrue(2U + 1U == ++numeral);
            Assert.IsTrue(3U + 1U == ++numeral);
            numeral = 123U.Church();
            Assert.IsTrue(123U + 1U == ++numeral);
        }

        [TestMethod]
        public void AddTest()
        {
            Assert.IsTrue(0U + 0U == 0U.Church() + 0U.Church());
            Assert.IsTrue(0U + 1U == 0U.Church() + 1U.Church());
            Assert.IsTrue(10U + 0U == 10U.Church() + 0U.Church());
            Assert.IsTrue(0U + 10U == 0U.Church() + 10U.Church());
            Assert.IsTrue(1U + 1U == 1U.Church() + 1U.Church());
            Assert.IsTrue(10U + 1U == 10U.Church() + 1U.Church());
            Assert.IsTrue(1U + 10U == 1U.Church() + 10U.Church());
            Assert.IsTrue(3U + 5U == 3U.Church() + 5U.Church());
            Assert.IsTrue(123U + 345U == 123U.Church() + 345U.Church());
        }

        [TestMethod]
        public void DecreaseTest()
        {
            Numeral numeral = 3U.Church();
            Assert.IsTrue(3U - 1U == --numeral);
            Assert.IsTrue(2U - 1U == --numeral);
            Assert.IsTrue(1U - 1U == --numeral);
            Assert.IsTrue(0U == --numeral);
            numeral = 123U.Church();
            Assert.IsTrue(123U - 1U == --numeral);
        }

        [TestMethod]
        public void SubtractTest()
        {
            Assert.IsTrue(0U - 0U == 0U.Church() - 0U.Church());
            Assert.IsTrue(0U == 0U.Church() - 1U.Church());
            Assert.IsTrue(10U - 0U == 10U.Church() - 0U.Church());
            Assert.IsTrue(0U == 0U.Church() - 10U.Church());
            Assert.IsTrue(1U - 1U == 1U.Church() - 1U.Church());
            Assert.IsTrue(10U - 1U == 10U.Church() - 1U.Church());
            Assert.IsTrue(0U == 1U.Church() - 10U.Church());
            Assert.IsTrue(0U == 3U.Church() - 5U.Church());
            Assert.IsTrue(0U == 123U.Church() - 345U.Church());
        }

        [TestMethod]
        public void MultiplyTest()
        {
            Assert.IsTrue(0U * 0U == 0U.Church() * 0U.Church());
            Assert.IsTrue(0U * 1U == 0U.Church() * 1U.Church());
            Assert.IsTrue(10U * 0U == 10U.Church() * 0U.Church());
            Assert.IsTrue(0U * 10U == 0U.Church() * 10U.Church());
            Assert.IsTrue(1U * 1U == 1U.Church() * 1U.Church());
            Assert.IsTrue(10U * 1U == 10U.Church() * 1U.Church());
            Assert.IsTrue(1U * 10U == 1U.Church() * 10U.Church());
            Assert.IsTrue(3U * 5U == 3U.Church() * 5U.Church());
            Assert.IsTrue(12U * 23U == 12U.Church() * 23U.Church());
        }

        [TestMethod]
        public void PowTest()
        {
            Assert.IsTrue(0U.Pow(1U) == (0U.Church() ^ 1U.Church()));
            Assert.IsTrue(10U.Pow(0U) == (10U.Church() ^ 0U.Church()));
            Assert.IsTrue(0U.Pow(10U) == (0U.Church() ^ 10U.Church()));
            Assert.IsTrue(1U.Pow(1U) == (1U.Church() ^ 1U.Church()));
            Assert.IsTrue(10U.Pow(1U) == (10U.Church() ^ 1U.Church()));
            Assert.IsTrue(1U.Pow(10U) == (1U.Church() ^ 10U.Church()));
            Assert.IsTrue(3U.Pow(5U) == (3U.Church() ^ 5U.Church()));
            Assert.IsTrue(5U.Pow(3U) == (5U.Church() ^ 3U.Church()));
        }

        [TestMethod]
        public void FactorialTest()
        {
            Func<uint, uint> factorial = null; // Must have. So that factorial can recursively refer itself.
            factorial = x => x == 0U ? 1 : factorial(x - 1);

            Assert.IsTrue(factorial(0U) == 0U.Church().Factorial());
            Assert.IsTrue(factorial(1U) == 1U.Church().Factorial());
            Assert.IsTrue(factorial(2U) == 2U.Church().Factorial());
            Assert.IsTrue(factorial(3U) == 3U.Church().Factorial());
            Assert.IsTrue(factorial(10U) == 10U.Church().Factorial());
        }

        [TestMethod]
        public void FibonacciTest()
        {
            Func<uint, uint> fibonacci = null; // Must have. So that fibonacci can recursively refer itself.
            fibonacci = x => x > 1U ? fibonacci(x - 1) + fibonacci(x - 2) : x;

            Assert.IsTrue(fibonacci(0U) == 0U.Church().Fibonacci());
            Assert.IsTrue(fibonacci(1U) == 1U.Church().Fibonacci());
            Assert.IsTrue(fibonacci(2U) == 2U.Church().Fibonacci());
            Assert.IsTrue(fibonacci(3U) == 3U.Church().Fibonacci());
            Assert.IsTrue(fibonacci(10U) == 10U.Church().Fibonacci());
        }

        [TestMethod]
        public void DivideByTest()
        {
            Assert.IsTrue(1U / 1U == 1U.Church().DivideBy(1U.Church()));
            Assert.IsTrue(1U / 2U == 1U.Church().DivideBy(2U.Church()));
            Assert.IsTrue(2U / 2U == 2U.Church().DivideBy(2U.Church()));
            Assert.IsTrue(2U / 1U == 2U.Church().DivideBy(1U.Church()));
            Assert.IsTrue(10U / 3U == 10U.Church().DivideBy(3U.Church()));
            Assert.IsTrue(3U / 10U == 3U.Church().DivideBy(10U.Church()));
        }
    }
}
