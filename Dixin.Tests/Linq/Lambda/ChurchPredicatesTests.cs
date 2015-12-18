namespace Dixin.Linq.Lambda.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchPredicatesTests
    {
        private static readonly _Numeral Zero = _Numeral.Zero;

        [TestMethod]
        public void IsZeroTest()
        {
            Assert.AreEqual(0U == 0U, Zero.IsZero()._Unchurch());
            Assert.AreEqual(1U == 0U, 1U._Church().IsZero()._Unchurch());
            Assert.AreEqual(2U == 0U, 2U._Church().IsZero()._Unchurch());
            Assert.AreEqual(123U == 0U, 123U._Church().IsZero()._Unchurch());
        }

        [TestMethod]
        public void IsLessOrEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U <= 0U, (Zero <= Zero)._Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U <= 0U, (1U._Church() <= Zero)._Unchurch());
            Assert.AreEqual(2U <= 0U, (2U._Church() <= Zero)._Unchurch());
            Assert.AreEqual(123U <= 0U, (123U._Church() <= Zero)._Unchurch());
            Assert.AreEqual(0U <= 2U, (Zero <= 2U._Church())._Unchurch());
            Assert.AreEqual(1U <= 2U, (1U._Church() <= 2U._Church())._Unchurch());
            Assert.AreEqual(2U <= 2U, (2U._Church() <= 2U._Church())._Unchurch());
            Assert.AreEqual(123U <= 2U, (123U._Church() <= 2U._Church())._Unchurch());
            Assert.AreEqual(0U <= 124U, (Zero <= 124U._Church())._Unchurch());
            Assert.AreEqual(1U <= 124U, (1U._Church() <= 124U._Church())._Unchurch());
            Assert.AreEqual(2U <= 124U, (2U._Church() <= 124U._Church())._Unchurch());
            Assert.AreEqual(123U <= 124U, (123U._Church() <= 124U._Church())._Unchurch());
        }

        [TestMethod]
        public void IsGreaterOrEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U >= 0U, (Zero >= Zero)._Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U >= 0U, (1U._Church() >= Zero)._Unchurch());
            Assert.AreEqual(2U >= 0U, (2U._Church() >= Zero)._Unchurch());
            Assert.AreEqual(123U >= 0U, (123U._Church() >= Zero)._Unchurch());
            Assert.AreEqual(0U >= 2U, (Zero >= 2U._Church())._Unchurch());
            Assert.AreEqual(1U >= 2U, (1U._Church() >= 2U._Church())._Unchurch());
            Assert.AreEqual(2U >= 2U, (2U._Church() >= 2U._Church())._Unchurch());
            Assert.AreEqual(123U >= 2U, (123U._Church() >= 2U._Church())._Unchurch());
            Assert.AreEqual(0U >= 124U, (Zero >= 124U._Church())._Unchurch());
            Assert.AreEqual(1U >= 124U, (1U._Church() >= 124U._Church())._Unchurch());
            Assert.AreEqual(2U >= 124U, (2U._Church() >= 124U._Church())._Unchurch());
            Assert.AreEqual(123U >= 124U, (123U._Church() >= 124U._Church())._Unchurch());
        }

        [TestMethod]
        public void IsLessTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U < 0U, (Zero < Zero)._Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U < 0U, (1U._Church() < Zero)._Unchurch());
            Assert.AreEqual(2U < 0U, (2U._Church() < Zero)._Unchurch());
            Assert.AreEqual(123U < 0U, (123U._Church() < Zero)._Unchurch());
            Assert.AreEqual(0U < 2U, (Zero < 2U._Church())._Unchurch());
            Assert.AreEqual(1U < 2U, (1U._Church() < 2U._Church())._Unchurch());
            Assert.AreEqual(2U < 2U, (2U._Church() < 2U._Church())._Unchurch());
            Assert.AreEqual(123U < 2U, (123U._Church() < 2U._Church())._Unchurch());
            Assert.AreEqual(0U < 124U, (Zero < 124U._Church())._Unchurch());
            Assert.AreEqual(1U < 124U, (1U._Church() < 124U._Church())._Unchurch());
            Assert.AreEqual(2U < 124U, (2U._Church() < 124U._Church())._Unchurch());
            Assert.AreEqual(123U < 124U, (123U._Church() < 124U._Church())._Unchurch());
        }

        [TestMethod]
        public void IsGreaterTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U > 0U, (Zero > Zero)._Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U > 0U, (1U._Church() > Zero)._Unchurch());
            Assert.AreEqual(2U > 0U, (2U._Church() > Zero)._Unchurch());
            Assert.AreEqual(123U > 0U, (123U._Church() > Zero)._Unchurch());
            Assert.AreEqual(0U > 2U, (Zero > 2U._Church())._Unchurch());
            Assert.AreEqual(1U > 2U, (1U._Church() > 2U._Church())._Unchurch());
            Assert.AreEqual(2U > 2U, (2U._Church() > 2U._Church())._Unchurch());
            Assert.AreEqual(123U > 2U, (123U._Church() > 2U._Church())._Unchurch());
            Assert.AreEqual(0U > 124U, (Zero > 124U._Church())._Unchurch());
            Assert.AreEqual(1U > 124U, (1U._Church() > 124U._Church())._Unchurch());
            Assert.AreEqual(2U > 124U, (2U._Church() > 124U._Church())._Unchurch());
            Assert.AreEqual(123U > 124U, (123U._Church() > 124U._Church())._Unchurch());
        }

        [TestMethod]
        public void IsEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U == 0U, (Zero == Zero)._Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U == 0U, (1U._Church() == Zero)._Unchurch());
            Assert.AreEqual(2U == 0U, (2U._Church() == Zero)._Unchurch());
            Assert.AreEqual(123U == 0U, (123U._Church() == Zero)._Unchurch());
            Assert.AreEqual(0U == 2U, (Zero == 2U._Church())._Unchurch());
            Assert.AreEqual(1U == 2U, (1U._Church() == 2U._Church())._Unchurch());
            Assert.AreEqual(2U == 2U, (2U._Church() == 2U._Church())._Unchurch());
            Assert.AreEqual(123U == 2U, (123U._Church() == 2U._Church())._Unchurch());
            Assert.AreEqual(0U == 124U, (Zero == 124U._Church())._Unchurch());
            Assert.AreEqual(1U == 124U, (1U._Church() == 124U._Church())._Unchurch());
            Assert.AreEqual(2U == 124U, (2U._Church() == 124U._Church())._Unchurch());
            Assert.AreEqual(123U == 124U, (123U._Church() == 124U._Church())._Unchurch());
        }

        [TestMethod]
        public void IsNotEqualTest()
        {
#pragma warning disable 1718
            Assert.AreEqual(0U != 0U, (Zero != Zero)._Unchurch());
#pragma warning restore 1718
            Assert.AreEqual(1U != 0U, (1U._Church() != Zero)._Unchurch());
            Assert.AreEqual(2U != 0U, (2U._Church() != Zero)._Unchurch());
            Assert.AreEqual(123U != 0U, (123U._Church() != Zero)._Unchurch());
            Assert.AreEqual(0U != 2U, (Zero != 2U._Church())._Unchurch());
            Assert.AreEqual(1U != 2U, (1U._Church() != 2U._Church())._Unchurch());
            Assert.AreEqual(2U != 2U, (2U._Church() != 2U._Church())._Unchurch());
            Assert.AreEqual(123U != 2U, (123U._Church() != 2U._Church())._Unchurch());
            Assert.AreEqual(0U != 124U, (Zero != 124U._Church())._Unchurch());
            Assert.AreEqual(1U != 124U, (1U._Church() != 124U._Church())._Unchurch());
            Assert.AreEqual(2U != 124U, (2U._Church() != 124U._Church())._Unchurch());
            Assert.AreEqual(123U != 124U, (123U._Church() != 124U._Church())._Unchurch());
        }
    }
}
