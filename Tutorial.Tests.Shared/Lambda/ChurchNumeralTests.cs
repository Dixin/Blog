namespace Tutorial.Tests.LambdaCalculus
{
    using System;

    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class ChurchNumeralTests
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
            Assert.AreEqual(Math.Pow(0U, 1U), 0U.Church().Pow(1U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(10U, 0U), 10U.Church().Pow(0U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(0U, 10U), 0U.Church().Pow(10U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(1U, 1U), 1U.Church().Pow(1U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(10U, 1U), 10U.Church().Pow(1U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(1U, 10U), 1U.Church().Pow(10U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(3U, 5U), 3U.Church().Pow(5U.Church()).Unchurch());
            Assert.AreEqual(Math.Pow(5U, 3U), 5U.Church().Pow(3U.Church()).Unchurch());
        }

        [TestMethod]
        public void DivideByRecursionTest()
        {
            Assert.AreEqual(1U / 1U, 1U.Church().DivideBySelfReference(1U.Church()).Unchurch());
            Assert.AreEqual(1U / 2U, 1U.Church().DivideBySelfReference(2U.Church()).Unchurch());
            Assert.AreEqual(2U / 2U, 2U.Church().DivideBySelfReference(2U.Church()).Unchurch());
            Assert.AreEqual(2U / 1U, 2U.Church().DivideBySelfReference(1U.Church()).Unchurch());
            Assert.AreEqual(10U / 3U, 10U.Church().DivideBySelfReference(3U.Church()).Unchurch());
            Assert.AreEqual(3U / 10U, 3U.Church().DivideBySelfReference(10U.Church()).Unchurch());
        }
    }
}
