namespace Tutorial.Tests.LinqToXml
{
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToXml;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public partial class XExtensionsTests
    {
        [TestMethod]
        public void ObjectsTest()
        {
            XAttribute attribute = new XAttribute("attribute", "value");
            EnumerableAssert.IsEmpty(attribute.DescendantObjects());

            XElement element = new XElement("element");
            EnumerableAssert.IsEmpty(element.DescendantObjects());

            XDocument document = new XDocument();
            EnumerableAssert.IsEmpty(document.DescendantObjects());

            element.SetAttributeValue(attribute.Name, attribute.Value);
            element.Add(element);
            Assert.AreEqual(3, element.DescendantObjects().Count());

            document.Add(element);
            document.Root.Add(new XAttribute(XNamespace.Xmlns + "prefix", "namespace"));
            Assert.AreEqual(5, document.DescendantObjects().Count());
        }

        [TestMethod]
        public void SelfAndObjectsTest()
        {
            XAttribute attribute = new XAttribute("attribute", "value");
            Assert.IsTrue(object.ReferenceEquals(attribute.SelfAndDescendantObjects().Single(), attribute));

            XElement element = new XElement("element");
            Assert.IsTrue(object.ReferenceEquals(element.SelfAndDescendantObjects().Single(), element));

            XDocument document = new XDocument();
            Assert.IsTrue(object.ReferenceEquals(document.SelfAndDescendantObjects().Single(), document));

            element.SetAttributeValue(attribute.Name, attribute.Value);
            element.Add(element);
            Assert.IsTrue(object.ReferenceEquals(attribute.SelfAndDescendantObjects().Single(), attribute));
            Assert.AreEqual(4, element.SelfAndDescendantObjects().Count());

            document.Add(element);
            document.Root.Add(new XAttribute(XNamespace.Xmlns + "prefix", "namespace"));
            Assert.AreEqual(6, document.SelfAndDescendantObjects().Count());
        }

        [TestMethod]
        public void NameTest()
        {
            XAttribute attribute = new XAttribute("attribute", "value");
            XElement element = new XElement("element", attribute);
            XDocument document = new XDocument(element);
            Assert.AreEqual(2, element.Names().Count());
            Assert.AreEqual(2, document.Names().Count());

            element.Add(element);
            Assert.AreEqual(2, element.Names().Count());
            Assert.AreEqual(2, document.Names().Count());
        }

        [TestMethod]
        public void AttributesTest()
        {
            XElement element = new XElement("element");
            Assert.IsFalse(element.AllAttributes().Any());

            XAttribute attribute = new XAttribute("attribute", "value");
            element = new XElement("element", attribute);
            Assert.IsTrue(object.ReferenceEquals(element.AllAttributes().Single(), attribute));

            XDocument document = new XDocument(element);
            Assert.IsTrue(object.ReferenceEquals(document.AllAttributes().Single(), attribute));

            element.Add(element);
            Assert.AreEqual(2, element.AllAttributes().Count());
            Assert.AreEqual(2, document.AllAttributes().Count());
        }

        [TestMethod]
        public void NamespacesTest()
        {
            XElement element = new XElement("element");
            Assert.IsFalse(element.Namespaces().Any());

            XNamespace namespace1 = "namespace1";
            element.SetAttributeValue(XNamespace.Xmlns + "prefix1", namespace1);
            (string, XNamespace) prefixAndNamespace = element.Namespaces().Single();
            Assert.IsTrue(object.ReferenceEquals(namespace1, prefixAndNamespace.Item2));
            Assert.IsTrue(object.ReferenceEquals(element.GetNamespaceOfPrefix(prefixAndNamespace.Item1), element.Namespaces().Single().Item2));
            Assert.AreEqual(prefixAndNamespace.Item1, element.GetPrefixOfNamespace(namespace1));

            XNamespace namespace2 = "namespace2";
            XDocument document = new XDocument(element);
            Assert.AreEqual(1, document.Namespaces().Count());
            element.SetAttributeValue(XNamespace.Xmlns + "prefix2", namespace2);
            Assert.AreEqual(2, document.Namespaces().Count());
        }
    }
}
