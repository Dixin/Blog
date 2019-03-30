namespace Tutorial.Tests.LinqToXml
{
    using Tutorial.LinqToXml;

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
