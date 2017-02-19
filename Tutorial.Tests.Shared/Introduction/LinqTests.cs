namespace Tutorial.Tests.Introduction
{
    using Tutorial.Introduction;

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
