namespace Tutorial.Tests.LambdaCalculus
{
    using System;

    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FixedPointCombinatorTests
    {
        [TestMethod]
        public void FactorialTest()
        {
            Func<uint, uint> factorial = null; // Must have to be compiled.
            factorial = x => x == 0 ? 1U : x * factorial(x - 1U);

            Assert.AreEqual(factorial(0U), 0U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(1U), 1U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(2U), 2U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(3U), 3U.Church().Factorial().Unchurch());
            Assert.AreEqual(factorial(7U), 7U.Church().Factorial().Unchurch());
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
            Assert.AreEqual(fibonacci(8U), 8U.Church().Fibonacci().Unchurch());
        }

        [TestMethod]
        public void DivideByTest()
        {
            Assert.AreEqual(1U / 1U, 1U.Church().DivideBy(1U.Church()).Unchurch());
            Assert.AreEqual(1U / 2U, 1U.Church().DivideBy(2U.Church()).Unchurch());
            Assert.AreEqual(2U / 2U, 2U.Church().DivideBy(2U.Church()).Unchurch());
            Assert.AreEqual(2U / 1U, 2U.Church().DivideBy(1U.Church()).Unchurch());
            Assert.AreEqual(8U / 3U, 8U.Church().DivideBy(3U.Church()).Unchurch());
            Assert.AreEqual(3U / 8U, 3U.Church().DivideBy(8U.Church()).Unchurch());
        }
    }
}