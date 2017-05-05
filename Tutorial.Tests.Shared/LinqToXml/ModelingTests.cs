namespace Tutorial.Tests.LinqToXml
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    using Tutorial.LinqToXml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ModelingTests
    {
        [TestMethod]
        public void CreateAndSerializeTest()
        {
            Modeling.CreateAndSerialize();
            Modeling.Construction();
        }

        [TestMethod]
        public void ApiTest()
        {
            Modeling.Name();
            Modeling.Namespace();
            Modeling.Element();
            Modeling.Attribute();
            Modeling.DeepEquals();
        }

        [TestMethod]
        public void ReadWriteTest()
        {
            try
            {
                Modeling.Read();
                Assert.Fail();
            }
            catch (XmlException exception)
            {
                Trace.WriteLine(exception);
            }
            IEnumerable<XElement> items = Modeling.RssItems("https://weblogs.asp.net/dixin/rss");
            Assert.IsTrue(items.Any());
            Modeling.XNodeToString();
            Modeling.Write();
        }

        [TestMethod]
        public void StreamingTest()
        {
            Modeling.StreamingElementWithChildElements();
            Modeling.StreamingElementWithChildElementModification();
        }
    }
}
