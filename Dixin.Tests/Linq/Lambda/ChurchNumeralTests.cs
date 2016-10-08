namespace Dixin.Tests.Linq.Lambda
{
    using System;

    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class UInt32Extensions
    {
        internal static uint Pow(this uint mantissa, uint exponent)
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
            Assert.AreEqual(0U + 1U, (numeral = numeral.Increase()).Unchurch());
            Assert.AreEqual(1U + 1U, (numeral = numeral.Increase()).Unchurch());
            Assert.AreEqual(2U + 1U, (numeral = numeral.Increase()).Unchurch());
            Assert.AreEqual(3U + 1U, (numeral = numeral.Increase()).Unchurch());
            numeral = 123U.Church();
            Assert.AreEqual(123U + 1U, numeral.Increase().Unchurch());
        }

        [TestMethod]
        public void AddTest()
        {
            Assert.AreEqual(0U + 0U, 0U.Church().Add(0U.Church()).Unchurch());
            Assert.AreEqual(0U + 1U, 0U.Church().Add(1U.Church()).Unchurch());
            Assert.AreEqual(10U + 0U, 10U.Church().Add(0U.Church()).Unchurch());
            Assert.AreEqual(0U + 10U, 0U.Church().Add(10U.Church()).Unchurch());
            Assert.AreEqual(1U + 1U, 1U.Church().Add(1U.Church()).Unchurch());
            Assert.AreEqual(10U + 1U, 10U.Church().Add(1U.Church()).Unchurch());
            Assert.AreEqual(1U + 10U, 1U.Church().Add(10U.Church()).Unchurch());
            Assert.AreEqual(3U + 5U, 3U.Church().Add(5U.Church()).Unchurch());
            Assert.AreEqual(123U + 345U, 123U.Church().Add(345U.Church()).Unchurch());
        }

        [TestMethod]
        public void DecreaseTest()
        {
            Numeral numeral = 3U.Church();
            Assert.AreEqual(3U - 1U, (numeral = numeral.Decrease()).Unchurch());
            Assert.AreEqual(2U - 1U, (numeral = numeral.Decrease()).Unchurch());
            Assert.AreEqual(1U - 1U, (numeral = numeral.Decrease()).Unchurch());
            Assert.AreEqual(0U, (numeral = numeral.Decrease()).Unchurch());
            numeral = 123U.Church();
            Assert.AreEqual(123U - 1U, numeral.Decrease().Unchurch());
        }

        [TestMethod]
        public void SubtractTest()
        {
            Assert.AreEqual(0U - 0U, 0U.Church().Subtract(0U.Church()).Unchurch());
            Assert.AreEqual(0U, 0U.Church().Subtract(1U.Church()).Unchurch());
            Assert.AreEqual(10U - 0U, 10U.Church().Subtract(0U.Church()).Unchurch());
            Assert.AreEqual(0U, 0U.Church().Subtract(10U.Church()).Unchurch());
            Assert.AreEqual(1U - 1U, 1U.Church().Subtract(1U.Church()).Unchurch());
            Assert.AreEqual(10U - 1U, 10U.Church().Subtract(1U.Church()).Unchurch());
            Assert.AreEqual(0U, 1U.Church().Subtract(10U.Church()).Unchurch());
            Assert.AreEqual(0U, 3U.Church().Subtract(5U.Church()).Unchurch());
            Assert.AreEqual(0U, 123U.Church().Subtract(345U.Church()).Unchurch());
        }

        [TestMethod]
        public void MultiplyTest()
        {
            Assert.AreEqual(0U * 0U, 0U.Church().Multiply(0U.Church()).Unchurch());
            Assert.AreEqual(0U * 1U, 0U.Church().Multiply(1U.Church()).Unchurch());
            Assert.AreEqual(10U * 0U, 10U.Church().Multiply(0U.Church()).Unchurch());
            Assert.AreEqual(0U * 10U, 0U.Church().Multiply(10U.Church()).Unchurch());
            Assert.AreEqual(1U * 1U, 1U.Church().Multiply(1U.Church()).Unchurch());
            Assert.AreEqual(10U * 1U, 10U.Church().Multiply(1U.Church()).Unchurch());
            Assert.AreEqual(1U * 10U, 1U.Church().Multiply(10U.Church()).Unchurch());
            Assert.AreEqual(3U * 5U, 3U.Church().Multiply(5U.Church()).Unchurch());
            Assert.AreEqual(12U * 23U, 12U.Church().Multiply(23U.Church()).Unchurch());
        }

        [TestMethod]
        public void PowTest()
        {
            Assert.AreEqual(0U.Pow(1U), 0U.Church().Pow(1U.Church()).Unchurch());
            Assert.AreEqual(10U.Pow(0U), 10U.Church().Pow(0U.Church()).Unchurch());
            Assert.AreEqual(0U.Pow(10U), 0U.Church().Pow(10U.Church()).Unchurch());
            Assert.AreEqual(1U.Pow(1U), 1U.Church().Pow(1U.Church()).Unchurch());
            Assert.AreEqual(10U.Pow(1U), 10U.Church().Pow(1U.Church()).Unchurch());
            Assert.AreEqual(1U.Pow(10U), 1U.Church().Pow(10U.Church()).Unchurch());
            Assert.AreEqual(3U.Pow(5U), 3U.Church().Pow(5U.Church()).Unchurch());
            Assert.AreEqual(5U.Pow(3U), 5U.Church().Pow(3U.Church()).Unchurch());
        }

        [TestMethod]
        public void FactorialTest()
        {
            Func<uint, uint> factorial = null; // Must have. So that factorial can recursively refer itself.
            factorial = x => x == 0U ? 1 : factorial(x - 1);

            Assert.AreEqual(factorial(0U), 0U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(1U), 1U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(2U), 2U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(3U), 3U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(10U), 10U.Church().Factorial().Unchurch());
        }

        [TestMethod]
        public void FibonacciTest()
        {
            Func<uint, uint> fibonacci = null; // Must have. So that fibonacci can recursively refer itself.
            fibonacci = x => x > 1U ? fibonacci(x - 1) + fibonacci(x - 2) : x;

            Assert.AreEqual(fibonacci(0U), 0U.Church().Fibonacci().Unchurch());
            Assert.AreEqual(fibonacci(1U), 1U.Church().Fibonacci().Unchurch());
            Assert.AreEqual(fibonacci(2U), 2U.Church().Fibonacci().Unchurch());
            Assert.AreEqual(fibonacci(3U), 3U.Church().Fibonacci().Unchurch());
            Assert.AreEqual(fibonacci(10U), 10U.Church().Fibonacci().Unchurch());
        }

        [TestMethod]
        public void DivideByTest()
        {
            Assert.AreEqual(1U / 1U, 1U.Church().DivideBy(1U.Church()).Unchurch());
            Assert.AreEqual(1U / 2U, 1U.Church().DivideBy(2U.Church()).Unchurch());
            Assert.AreEqual(2U / 2U, 2U.Church().DivideBy(2U.Church()).Unchurch());
            Assert.AreEqual(2U / 1U, 2U.Church().DivideBy(1U.Church()).Unchurch());
            Assert.AreEqual(10U / 3U, 10U.Church().DivideBy(3U.Church()).Unchurch());
            Assert.AreEqual(3U / 10U, 3U.Church().DivideBy(10U.Church()).Unchurch());
        }
    }
}
