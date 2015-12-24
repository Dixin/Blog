namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchSignedNumeralTests
    {
        [TestMethod]
        public void SignNegatePositiveNegativeTest()
        {
            SignedNumeral signed = 0U._Church().Sign();
            Assert.IsTrue(0U == signed.Positive());
            Assert.IsTrue(0U == signed.Negative());
            signed = signed.Negate();
            Assert.IsTrue(0U == signed.Positive());
            Assert.IsTrue(0U == signed.Negative());

            signed = 1U._Church().Sign();
            Assert.IsTrue(1U == signed.Positive());
            Assert.IsTrue(0U == signed.Negative());
            signed = signed.Negate();
            Assert.IsTrue(0U == signed.Positive());
            Assert.IsTrue(1U == signed.Negative());

            signed = 2U._Church().Sign();
            Assert.IsTrue(2U == signed.Positive());
            Assert.IsTrue(0U == signed.Negative());
            signed = signed.Negate();
            Assert.IsTrue(0U == signed.Positive());
            Assert.IsTrue(2U == signed.Negative());

            signed = 123U._Church().Sign();
            Assert.IsTrue(123U == signed.Positive());
            Assert.IsTrue(0U == signed.Negative());
            signed = signed.Negate();
            Assert.IsTrue(0U == signed.Positive());
            Assert.IsTrue(123U == signed.Negative());

            signed = new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>(12U._Church())(23U._Church()));
            Assert.IsTrue(12U == signed.Positive());
            Assert.IsTrue(23U == signed.Negative());
            signed = signed.Negate();
            Assert.IsTrue(23U == signed.Positive());
            Assert.IsTrue(12U == signed.Negative());
        }

        [TestMethod]
        public void FormatWithZeroTest()
        {
            SignedNumeral signed = new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>(12U._Church())(23U._Church()));
            signed = signed.FormatWithZero();
            Assert.IsTrue(0U == signed.Positive());
            Assert.IsTrue(11U == signed.Negative());

            signed = new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>(23U._Church())(12U._Church()));
            signed = signed.FormatWithZero();
            Assert.IsTrue(11U == signed.Positive());
            Assert.IsTrue(0U == signed.Negative());
        }

        [TestMethod]
        public void AddTest()
        {
            SignedNumeral a = 0U._Church().Sign();
            SignedNumeral b = 0U._Church().Sign();
            SignedNumeral result = a.Add(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(0U == result.Negative());

            a = 1U._Church().Sign();
            b = 1U._Church().Sign().Negate();
            result = a.Add(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(0U == result.Negative());

            a = 3U._Church().Sign();
            b = 5U._Church().Sign().Negate();
            result = a.Add(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(2U == result.Negative());
        }

        [TestMethod]
        public void SubtractTest()
        {
            SignedNumeral a = 0U._Church().Sign();
            SignedNumeral b = 0U._Church().Sign();
            SignedNumeral result = a.Subtract(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(0U == result.Negative());

            a = 1U._Church().Sign();
            b = 1U._Church().Sign().Negate();
            result = a.Subtract(b);
            Assert.IsTrue(2U == result.Positive());
            Assert.IsTrue(0U == result.Negative());

            a = 3U._Church().Sign();
            b = 5U._Church().Sign().Negate();
            result = a.Subtract(b);
            Assert.IsTrue(8U == result.Positive());
            Assert.IsTrue(0U == result.Negative());
        }

        [TestMethod]
        public void MultiplyTest()
        {
            SignedNumeral a = 0U._Church().Sign();
            SignedNumeral b = 0U._Church().Sign();
            SignedNumeral result = a.Multiply(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(0U == result.Negative());

            a = 1U._Church().Sign();
            b = 1U._Church().Sign().Negate();
            result = a.Multiply(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(1U == result.Negative());

            a = 3U._Church().Sign();
            b = 5U._Church().Sign().Negate();
            result = a.Multiply(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(15U == result.Negative());
        }

        [TestMethod]
        public void DivideByTest()
        {
            SignedNumeral a = 0U._Church().Sign();
            SignedNumeral b = 0U._Church().Sign();
            SignedNumeral result = a.DivideBy(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(0U == result.Negative());

            a = 1U._Church().Sign();
            b = 1U._Church().Sign().Negate();
            result = a.DivideBy(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(1U == result.Negative());

            a = 11U._Church().Sign();
            b = 5U._Church().Sign().Negate();
            result = a.DivideBy(b);
            Assert.IsTrue(0U == result.Positive());
            Assert.IsTrue(2U == result.Negative());
        }
    }
}
