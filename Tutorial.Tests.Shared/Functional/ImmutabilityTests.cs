namespace Tutorial.Tests.Shared.Functional
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Functional;

    [TestClass]
    public class ImmutabilityTests
    {
        [TestMethod]
        public void FibonacciTest()
        {
            Assert.AreEqual(0, Immutability.Fibonacci(0));
            Assert.AreEqual(1, Immutability.Fibonacci(1));
            Assert.AreEqual(1, Immutability.Fibonacci(2));
            Assert.AreEqual(2, Immutability.Fibonacci(3));
            Assert.AreEqual(3, Immutability.Fibonacci(4));
            Assert.AreEqual(5, Immutability.Fibonacci(5));
            Assert.AreEqual(55, Immutability.Fibonacci(10));
            Assert.AreEqual(6765, Immutability.Fibonacci(20));
        }
    }
}
