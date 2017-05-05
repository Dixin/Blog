namespace Tutorial.Tests.Functional
{
    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FundamentalsTests
    {
        [TestMethod]
        public void TypeTest()
        {
            Fundamentals.ValueTypeReferenceType();
            Fundamentals.Default();
        }
    }
}
