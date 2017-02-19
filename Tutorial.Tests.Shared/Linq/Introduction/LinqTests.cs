namespace Dixin.Tests.Linq.Introduction
{
    using Dixin.Linq.Introduction;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LinqTests
    {
        [TestMethod]
        public void LinqToJsonTest()
        {
            LinqToJson.QueryMethods();
        }
    }
}
