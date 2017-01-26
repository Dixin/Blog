namespace Dixin.Tests.Linq.LinqToXml
{
    using Dixin.Linq.LinqToXml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DomTests
    {
        [TestMethod]
        public void CreateAndSerializeTest()
        {
            Dom.CreateAndSerialize();
        }
    }
}
