namespace Tutorial.Tests.LinqToXml
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToXml;

    public partial class XExtensionsTests
    {
        [TestMethod]
        public void ElementTest()
        {
            XDocument document = new XDocument();

            XElement element1 = XElement.Parse(@"<element></element>");
            Assert.AreEqual("/", element1.XPath());
            Assert.AreEqual((element1.XPathEvaluate(element1.XPath()) as IEnumerable<object>).Single(), element1);

            document.Add(element1);

            Assert.AreEqual("/element", element1.XPath());
            Assert.AreEqual((element1.XPathEvaluate(element1.XPath()) as IEnumerable<object>).Single(), element1);

            XElement element2 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace'></prefix:element>");
            Assert.AreEqual("/", element2.XPath());
            Assert.AreEqual((element2.XPathEvaluate(element2.XPath(), element2.CreateNamespaceManager()) as IEnumerable<object>).Single(), element2);

            document.Root.ReplaceWith(element2);

            Assert.AreEqual("/prefix:element", element2.XPath());
            Assert.AreEqual((element2.XPathEvaluate(element2.XPath(), element2.CreateNamespaceManager()) as IEnumerable<object>).Single(), element2);

            XElement element3 = XElement.Parse(@"<element xmlns:prefix='namespace'><prefix:element /></element>");
            Assert.AreEqual("/", element3.XPath());
            Assert.AreEqual((element3.XPathEvaluate(element3.XPath(), element3.CreateNamespaceManager()) as IEnumerable<object>).Single(), element3);

            document.Root.ReplaceWith(element3);

            Assert.AreEqual("/element/prefix:element", element3.Elements().Single().XPath());
            Assert.AreEqual((element3.XPathEvaluate(element3.XPath(), element3.CreateNamespaceManager()) as IEnumerable<object>).Single(), element3);
        }

        [TestMethod]
        public void AttributeTest()
        {
            XDocument document = new XDocument();
            XElement element1 = XElement.Parse(@"<element></element>");
            XElement element2 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace'></prefix:element>");
            XElement element3 = XElement.Parse(@"<element xmlns:prefix='namespace'><prefix:element /></element>");
            XElement element4 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace' attribute='value' prefix:attribute='value'></prefix:element>");

            Assert.AreEqual("/@xmlns:prefix", element4.FirstAttribute.XPath());
            Assert.AreEqual("/@attribute", element4.Attribute("attribute").XPath());
            Assert.AreEqual("/@prefix:attribute", element4.LastAttribute.XPath());
            Assert.AreEqual((element4.XPathEvaluate(element4.Attribute("attribute").XPath(), element4.CreateNamespaceManager()) as IEnumerable<object>).Single(), element4.Attribute("attribute"));
            Assert.AreEqual((element4.XPathEvaluate(element4.LastAttribute.XPath(), element4.CreateNamespaceManager()) as IEnumerable<object>).Single(), element4.LastAttribute);

            document.Add(element4);

            Assert.AreEqual("/prefix:element/@xmlns:prefix", element4.FirstAttribute.XPath());
            Assert.AreEqual("/prefix:element/@attribute", element4.Attribute("attribute").XPath());
            Assert.AreEqual("/prefix:element/@prefix:attribute", element4.LastAttribute.XPath());
            Assert.AreEqual((element4.XPathEvaluate(element4.Attribute("attribute").XPath(), element4.CreateNamespaceManager()) as IEnumerable<object>).Single(), element4.Attribute("attribute"));
            Assert.AreEqual((element4.XPathEvaluate(element4.LastAttribute.XPath(), element4.CreateNamespaceManager()) as IEnumerable<object>).Single(), element4.LastAttribute);

            XElement element5 = new XElement("root", element1, element2, element3, element4);
            Assert.AreEqual("/element[1]", element5.Elements().ElementAt(0).XPath());
            Assert.AreEqual("/prefix:element[1]", element5.Elements().ElementAt(1).XPath());
            Assert.AreEqual("/element[2]", element5.Elements().ElementAt(2).XPath());
            Assert.AreEqual("/prefix:element[2]", element5.Elements().ElementAt(3).XPath());
            Assert.AreEqual("/prefix:element[2]/@xmlns:prefix", element5.Elements().ElementAt(3).FirstAttribute.XPath());
            Assert.AreEqual("/prefix:element[2]/@attribute", element5.Elements().ElementAt(3).Attribute("attribute").XPath());
            Assert.AreEqual("/prefix:element[2]/@prefix:attribute", element5.Elements().ElementAt(3).LastAttribute.XPath());
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(0).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(0));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(1).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(1));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(2).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(2));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(3).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(3).Attribute("attribute").XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3).Attribute("attribute"));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(3).LastAttribute.XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3).LastAttribute);

            document.Root.ReplaceWith(element5);

            Assert.AreEqual("/root/element[1]", element5.Elements().ElementAt(0).XPath());
            Assert.AreEqual("/root/prefix:element[1]", element5.Elements().ElementAt(1).XPath());
            Assert.AreEqual("/root/element[2]", element5.Elements().ElementAt(2).XPath());
            Assert.AreEqual("/root/prefix:element[2]", element5.Elements().ElementAt(3).XPath());
            Assert.AreEqual("/root/prefix:element[2]/@xmlns:prefix", element5.Elements().ElementAt(3).FirstAttribute.XPath());
            Assert.AreEqual("/root/prefix:element[2]/@attribute", element5.Elements().ElementAt(3).Attribute("attribute").XPath());
            Assert.AreEqual("/root/prefix:element[2]/@prefix:attribute", element5.Elements().ElementAt(3).LastAttribute.XPath());
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(0).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(0));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(1).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(1));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(2).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(2));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(3).XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(3).Attribute("attribute").XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3).Attribute("attribute"));
            Assert.AreEqual(
                (element5.XPathEvaluate(element5.Elements().ElementAt(3).LastAttribute.XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3).LastAttribute);
        }

        [TestMethod]
        public void TreeTest()
        {
            XDocument document = new XDocument();
            XElement element1 = XElement.Parse(@"<element></element>");
            XElement element2 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace'></prefix:element>");
            XElement element3 = XElement.Parse(@"<element xmlns:prefix='namespace'><prefix:element /></element>");
            XElement element4 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace' attribute='value' prefix:attribute='value'></prefix:element>");
            XElement element5 = new XElement("root", element1, element2, element3, element4);
            document.Add(element5);
            element4 = element5.Elements().ElementAt(3);
            element4.Add(element5);

            Assert.AreEqual("/root/prefix:element[2]/root/element[1]", element5.Elements().ElementAt(3).Elements().Single().Elements().First().XPath());
            Assert.AreEqual("/root/prefix:element[2]/root/prefix:element[2]/@prefix:attribute", element4.Elements().Single().Elements().Last().LastAttribute.XPath());
            Assert.AreEqual(
                (element4.XPathEvaluate(element5.Elements().ElementAt(3).Elements().Single().Elements().First().XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element5.Elements().ElementAt(3).Elements().Single().Elements().First());
            Assert.AreEqual(
                (element4.XPathEvaluate(element4.Elements().Single().Elements().Last().LastAttribute.XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element4.Elements().Single().Elements().Last().LastAttribute);
        }

        [TestMethod]
        public void CommentInstructionTest()
        {
            XDocument document = new XDocument();
            XElement element1 = XElement.Parse(@"<element></element>");
            XElement element2 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace'></prefix:element>");
            XElement element3 = XElement.Parse(@"<element xmlns:prefix='namespace'><prefix:element /></element>");
            XElement element4 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace' attribute='value' prefix:attribute='value'></prefix:element>");
            XElement element5 = new XElement("root", element1, element2, element3, element4);
            element4 = element5.Elements().ElementAt(3);
            element4.Add(element5);

            XElement element6 = XElement.Parse(@"<root><element><!--Comment1.--><!--Comment2.--><?a b?></element></root>");
            Assert.AreEqual("/element/comment()[2]", element6.Elements().Single().Nodes().OfType<XComment>().Last().XPath());
            Assert.AreEqual("/element/processing-instruction('a')", element6.Elements().Single().Nodes().OfType<XProcessingInstruction>().Single().XPath());
            Assert.AreEqual(
                (element6.XPathEvaluate(element6.Elements().Single().Nodes().OfType<XComment>().Last().XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element6.Elements().Single().Nodes().OfType<XComment>().Last());
            Assert.AreEqual(
                (element6.XPathEvaluate(element6.Elements().Single().Nodes().OfType<XProcessingInstruction>().Single().XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element6.Elements().Single().Nodes().OfType<XProcessingInstruction>().Single());
            document.Add(element6);
            Assert.AreEqual("/root/element/comment()[2]", element6.Elements().Single().Nodes().OfType<XComment>().Last().XPath());
            Assert.AreEqual("/root/element/processing-instruction('a')", element6.Elements().Single().Nodes().OfType<XProcessingInstruction>().Single().XPath());
            Assert.AreEqual(
                (element6.XPathEvaluate(element6.Elements().Single().Nodes().OfType<XComment>().Last().XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element6.Elements().Single().Nodes().OfType<XComment>().Last());
            Assert.AreEqual(
                (element6.XPathEvaluate(element6.Elements().Single().Nodes().OfType<XProcessingInstruction>().Single().XPath(), element5.CreateNamespaceManager()) as IEnumerable<object>).Single(),
                element6.Elements().Single().Nodes().OfType<XProcessingInstruction>().Single());
            document.Root.ReplaceWith(element6);
        }

        [TestMethod]
        public void TextTest()
        {
            XDocument document = new XDocument();
            XElement element1 = XElement.Parse(@"<element></element>");
            XElement element2 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace'></prefix:element>");
            XElement element3 = XElement.Parse(@"<element xmlns:prefix='namespace'><prefix:element /></element>");
            XElement element4 = XElement.Parse(@"<prefix:element xmlns:prefix='namespace' attribute='value' prefix:attribute='value'></prefix:element>");
            XElement element5 = new XElement("root", element1, element2, element3, element4);
            element4 = element5.Elements().ElementAt(3);
            element4.Add(element5);

            XElement element7 = XElement.Parse("<root><element>Text.</element></root>");
            Assert.AreEqual("/element/text()", element7.Elements().Single().Nodes().OfType<XText>().Single().XPath());
            Assert.AreEqual(
                (element7.XPathEvaluate(element7.Elements().Single().Nodes().OfType<XText>().Single().XPath()) as IEnumerable<object>)
#if NETFX
                    .Single(),
#else
                    .First(), // Or: Distinct().Single(),
#endif
                element7.Elements().Single().Nodes().OfType<XText>().Single());
            document.Add(element7);
            Assert.AreEqual("/root/element/text()", element7.Elements().Single().Nodes().OfType<XText>().Single().XPath());

            XElement element8 = XElement.Parse(@"<root>1<element></element>2</root>");
            Assert.AreEqual("/text()[1]", (element8.FirstNode as XText).XPath());
            Assert.AreEqual(
                (element8.XPathEvaluate((element8.FirstNode as XText).XPath()) as IEnumerable<object>)
#if NETFX
                    .Single(),
#else
                    .First(), // Or: Distinct().Single(),
#endif
                element8.FirstNode);
            Assert.AreEqual("/text()[2]", (element8.LastNode as XText).XPath());
            Assert.AreEqual(
                (element8.XPathEvaluate((element8.LastNode as XText).XPath()) as IEnumerable<object>)
#if NETFX
                    .Single(),
#else
                    .First(),
#endif
                element8.LastNode);
        }
    }
}
