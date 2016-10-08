namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchPredicateTests
    {
        private static readonly Numeral Zero = Numeral.Zero;

        [TestMethod]
        public void IsZeroTest()
        {
            Assert.AreEqual(0U == 0U, Zero.IsZero().Unchurch());
            Assert.AreEqual(1U == 0U, 1U.Church().IsZero().Unchurch());
            Assert.AreEqual(2U == 0U, 2U.Church().IsZero().Unchurch());
            Assert.AreEqual(123U == 0U, 123U.Church().IsZero().Unchurch());
        }

        [TestMethod]
        public void IsLessOrEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U <= 0U, (Zero <= Zero).Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U <= 0U, (1U.Church() <= Zero).Unchurch());
            Assert.AreEqual(2U <= 0U, (2U.Church() <= Zero).Unchurch());
            Assert.AreEqual(123U <= 0U, (123U.Church() <= Zero).Unchurch());
            Assert.AreEqual(0U <= 2U, (Zero <= 2U.Church()).Unchurch());
            Assert.AreEqual(1U <= 2U, (1U.Church() <= 2U.Church()).Unchurch());
            Assert.AreEqual(2U <= 2U, (2U.Church() <= 2U.Church()).Unchurch());
            Assert.AreEqual(123U <= 2U, (123U.Church() <= 2U.Church()).Unchurch());
            Assert.AreEqual(0U <= 124U, (Zero <= 124U.Church()).Unchurch());
            Assert.AreEqual(1U <= 124U, (1U.Church() <= 124U.Church()).Unchurch());
            Assert.AreEqual(2U <= 124U, (2U.Church() <= 124U.Church()).Unchurch());
            Assert.AreEqual(123U <= 124U, (123U.Church() <= 124U.Church()).Unchurch());
        }

        [TestMethod]
        public void IsGreaterOrEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U >= 0U, (Zero >= Zero).Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U >= 0U, (1U.Church() >= Zero).Unchurch());
            Assert.AreEqual(2U >= 0U, (2U.Church() >= Zero).Unchurch());
            Assert.AreEqual(123U >= 0U, (123U.Church() >= Zero).Unchurch());
            Assert.AreEqual(0U >= 2U, (Zero >= 2U.Church()).Unchurch());
            Assert.AreEqual(1U >= 2U, (1U.Church() >= 2U.Church()).Unchurch());
            Assert.AreEqual(2U >= 2U, (2U.Church() >= 2U.Church()).Unchurch());
            Assert.AreEqual(123U >= 2U, (123U.Church() >= 2U.Church()).Unchurch());
            Assert.AreEqual(0U >= 124U, (Zero >= 124U.Church()).Unchurch());
            Assert.AreEqual(1U >= 124U, (1U.Church() >= 124U.Church()).Unchurch());
            Assert.AreEqual(2U >= 124U, (2U.Church() >= 124U.Church()).Unchurch());
            Assert.AreEqual(123U >= 124U, (123U.Church() >= 124U.Church()).Unchurch());
        }

        [TestMethod]
        public void IsLessTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U < 0U, (Zero < Zero).Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U < 0U, (1U.Church() < Zero).Unchurch());
            Assert.AreEqual(2U < 0U, (2U.Church() < Zero).Unchurch());
            Assert.AreEqual(123U < 0U, (123U.Church() < Zero).Unchurch());
            Assert.AreEqual(0U < 2U, (Zero < 2U.Church()).Unchurch());
            Assert.AreEqual(1U < 2U, (1U.Church() < 2U.Church()).Unchurch());
            Assert.AreEqual(2U < 2U, (2U.Church() < 2U.Church()).Unchurch());
            Assert.AreEqual(123U < 2U, (123U.Church() < 2U.Church()).Unchurch());
            Assert.AreEqual(0U < 124U, (Zero < 124U.Church()).Unchurch());
            Assert.AreEqual(1U < 124U, (1U.Church() < 124U.Church()).Unchurch());
            Assert.AreEqual(2U < 124U, (2U.Church() < 124U.Church()).Unchurch());
            Assert.AreEqual(123U < 124U, (123U.Church() < 124U.Church()).Unchurch());
        }

        [TestMethod]
        public void IsGreaterTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U > 0U, (Zero > Zero).Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U > 0U, (1U.Church() > Zero).Unchurch());
            Assert.AreEqual(2U > 0U, (2U.Church() > Zero).Unchurch());
            Assert.AreEqual(123U > 0U, (123U.Church() > Zero).Unchurch());
            Assert.AreEqual(0U > 2U, (Zero > 2U.Church()).Unchurch());
            Assert.AreEqual(1U > 2U, (1U.Church() > 2U.Church()).Unchurch());
            Assert.AreEqual(2U > 2U, (2U.Church() > 2U.Church()).Unchurch());
            Assert.AreEqual(123U > 2U, (123U.Church() > 2U.Church()).Unchurch());
            Assert.AreEqual(0U > 124U, (Zero > 124U.Church()).Unchurch());
            Assert.AreEqual(1U > 124U, (1U.Church() > 124U.Church()).Unchurch());
            Assert.AreEqual(2U > 124U, (2U.Church() > 124U.Church()).Unchurch());
            Assert.AreEqual(123U > 124U, (123U.Church() > 124U.Church()).Unchurch());
        }

        [TestMethod]
        public void IsEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U == 0U, (Zero == Zero).Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U == 0U, (1U.Church() == Zero).Unchurch());
            Assert.AreEqual(2U == 0U, (2U.Church() == Zero).Unchurch());
            Assert.AreEqual(123U == 0U, (123U.Church() == Zero).Unchurch());
            Assert.AreEqual(0U == 2U, (Zero == 2U.Church()).Unchurch());
            Assert.AreEqual(1U == 2U, (1U.Church() == 2U.Church()).Unchurch());
            Assert.AreEqual(2U == 2U, (2U.Church() == 2U.Church()).Unchurch());
            Assert.AreEqual(123U == 2U, (123U.Church() == 2U.Church()).Unchurch());
            Assert.AreEqual(0U == 124U, (Zero == 124U.Church()).Unchurch());
            Assert.AreEqual(1U == 124U, (1U.Church() == 124U.Church()).Unchurch());
            Assert.AreEqual(2U == 124U, (2U.Church() == 124U.Church()).Unchurch());
            Assert.AreEqual(123U == 124U, (123U.Church() == 124U.Church()).Unchurch());
        }

        [TestMethod]
        public void IsNotEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U != 0U, (Zero != Zero).Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U != 0U, (1U.Church() != Zero).Unchurch());
            Assert.AreEqual(2U != 0U, (2U.Church() != Zero).Unchurch());
            Assert.AreEqual(123U != 0U, (123U.Church() != Zero).Unchurch());
            Assert.AreEqual(0U != 2U, (Zero != 2U.Church()).Unchurch());
            Assert.AreEqual(1U != 2U, (1U.Church() != 2U.Church()).Unchurch());
            Assert.AreEqual(2U != 2U, (2U.Church() != 2U.Church()).Unchurch());
            Assert.AreEqual(123U != 2U, (123U.Church() != 2U.Church()).Unchurch());
            Assert.AreEqual(0U != 124U, (Zero != 124U.Church()).Unchurch());
            Assert.AreEqual(1U != 124U, (1U.Church() != 124U.Church()).Unchurch());
            Assert.AreEqual(2U != 124U, (2U.Church() != 124U.Church()).Unchurch());
            Assert.AreEqual(123U != 124U, (123U.Church() != 124U.Church()).Unchurch());
        }
    }
}
