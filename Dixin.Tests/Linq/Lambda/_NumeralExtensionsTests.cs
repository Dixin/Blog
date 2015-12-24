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
    public class _NumeralExtensionsTests
    {
        [TestMethod]
        public void IncreaseTest()
        {
            _Numeral numeral = 0U._Church();
            Assert.IsTrue(0U + 1U == ++numeral);
            Assert.IsTrue(1U + 1U == ++numeral);
            Assert.IsTrue(2U + 1U == ++numeral);
            Assert.IsTrue(3U + 1U == ++numeral);
            numeral = 123U._Church();
            Assert.IsTrue(123U + 1U == ++numeral);
        }

        [TestMethod]
        public void AddTest()
        {
            Assert.IsTrue(0U + 0U == 0U._Church() + 0U._Church());
            Assert.IsTrue(0U + 1U == 0U._Church() + 1U._Church());
            Assert.IsTrue(10U + 0U == 10U._Church() + 0U._Church());
            Assert.IsTrue(0U + 10U == 0U._Church() + 10U._Church());
            Assert.IsTrue(1U + 1U == 1U._Church() + 1U._Church());
            Assert.IsTrue(10U + 1U == 10U._Church() + 1U._Church());
            Assert.IsTrue(1U + 10U == 1U._Church() + 10U._Church());
            Assert.IsTrue(3U + 5U == 3U._Church() + 5U._Church());
            Assert.IsTrue(123U + 345U == 123U._Church() + 345U._Church());
        }

        [TestMethod]
        public void DecreaseTest()
        {
            _Numeral numeral = 3U._Church();
            Assert.IsTrue(3U - 1U == --numeral);
            Assert.IsTrue(2U - 1U == --numeral);
            Assert.IsTrue(1U - 1U == --numeral);
            Assert.IsTrue(0U == --numeral);
            numeral = 123U._Church();
            Assert.IsTrue(123U - 1U == --numeral);
        }

        [TestMethod]
        public void SubtractTest()
        {
            Assert.IsTrue(0U - 0U == 0U._Church() - 0U._Church());
            Assert.IsTrue(0U == 0U._Church() - 1U._Church());
            Assert.IsTrue(10U - 0U == 10U._Church() - 0U._Church());
            Assert.IsTrue(0U == 0U._Church() - 10U._Church());
            Assert.IsTrue(1U - 1U == 1U._Church() - 1U._Church());
            Assert.IsTrue(10U - 1U == 10U._Church() - 1U._Church());
            Assert.IsTrue(0U == 1U._Church() - 10U._Church());
            Assert.IsTrue(0U == 3U._Church() - 5U._Church());
            Assert.IsTrue(0U == 123U._Church() - 345U._Church());
        }

        [TestMethod]
        public void MultiplyTest()
        {
            Assert.IsTrue(0U * 0U == 0U._Church() * 0U._Church());
            Assert.IsTrue(0U * 1U == 0U._Church() * 1U._Church());
            Assert.IsTrue(10U * 0U == 10U._Church() * 0U._Church());
            Assert.IsTrue(0U * 10U == 0U._Church() * 10U._Church());
            Assert.IsTrue(1U * 1U == 1U._Church() * 1U._Church());
            Assert.IsTrue(10U * 1U == 10U._Church() * 1U._Church());
            Assert.IsTrue(1U * 10U == 1U._Church() * 10U._Church());
            Assert.IsTrue(3U * 5U == 3U._Church() * 5U._Church());
            Assert.IsTrue(12U * 23U == 12U._Church() * 23U._Church());
        }

        [TestMethod]
        public void PowTest()
        {
            Assert.IsTrue(0U.Pow(1U) == (0U._Church() ^ 1U._Church()));
            Assert.IsTrue(10U.Pow(0U) == (10U._Church() ^ 0U._Church()));
            Assert.IsTrue(0U.Pow(10U) == (0U._Church() ^ 10U._Church()));
            Assert.IsTrue(1U.Pow(1U) == (1U._Church() ^ 1U._Church()));
            Assert.IsTrue(10U.Pow(1U) == (10U._Church() ^ 1U._Church()));
            Assert.IsTrue(1U.Pow(10U) == (1U._Church() ^ 10U._Church()));
            Assert.IsTrue(3U.Pow(5U) == (3U._Church() ^ 5U._Church()));
            Assert.IsTrue(5U.Pow(3U) == (5U._Church() ^ 3U._Church()));
        }

        [TestMethod]
        public void FactorialTest()
        {
            Func<uint, uint> factorial = null; // Must have. So that factorial can recursively refer itself.
            factorial = x => x == 0U ? 1 : factorial(x - 1);

            Assert.IsTrue(factorial(0U) == 0U._Church().Factorial());
            Assert.IsTrue(factorial(1U) == 1U._Church().Factorial());
            Assert.IsTrue(factorial(2U) == 2U._Church().Factorial());
            Assert.IsTrue(factorial(3U) == 3U._Church().Factorial());
            Assert.IsTrue(factorial(10U) == 10U._Church().Factorial());
        }

        [TestMethod]
        public void FibonacciTest()
        {
            Func<uint, uint> fibonacci = null; // Must have. So that fibonacci can recursively refer itself.
            fibonacci = x => x > 1U ? fibonacci(x - 1) + fibonacci(x - 2) : x;

            Assert.IsTrue(fibonacci(0U) == 0U._Church().Fibonacci());
            Assert.IsTrue(fibonacci(1U) == 1U._Church().Fibonacci());
            Assert.IsTrue(fibonacci(2U) == 2U._Church().Fibonacci());
            Assert.IsTrue(fibonacci(3U) == 3U._Church().Fibonacci());
            Assert.IsTrue(fibonacci(10U) == 10U._Church().Fibonacci());
        }

        [TestMethod]
        public void DivideByTest()
        {
            Assert.IsTrue(1U / 1U == 1U._Church().DivideBy(1U._Church()));
            Assert.IsTrue(1U / 2U == 1U._Church().DivideBy(2U._Church()));
            Assert.IsTrue(2U / 2U == 2U._Church().DivideBy(2U._Church()));
            Assert.IsTrue(2U / 1U == 2U._Church().DivideBy(1U._Church()));
            Assert.IsTrue(10U / 3U == 10U._Church().DivideBy(3U._Church()));
            Assert.IsTrue(3U / 10U == 3U._Church().DivideBy(10U._Church()));
        }
    }
}
