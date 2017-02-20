namespace Tutorial.Tests.Functional
{
    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

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
