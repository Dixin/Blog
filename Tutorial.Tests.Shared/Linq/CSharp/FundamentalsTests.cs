namespace Dixin.Tests.Linq.CSharp
{
    using Dixin.Linq.CSharp;

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
