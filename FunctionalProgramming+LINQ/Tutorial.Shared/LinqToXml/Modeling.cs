namespace Tutorial.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    internal static partial class Modeling
    {
        internal static void CreateAndSerialize()
        {
            XNamespace @namespace = "https://weblogs.asp.net/dixin";
            XElement rss = new XElement(
                "rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "dixin", @namespace),
                new XElement(
                    "channel",
                    new XElement(
                        "item", // Implicitly converted to XName.
                        new XElement("title", "LINQ via C#"),
                        new XElement("link", "https://weblogs.asp.net/dixin/linq-via-csharp"),
                        new XElement(
                            "description",
                            XElement.Parse("<p>This is a tutorial of LINQ and functional programming. Hope it helps.</p>")),
                        new XElement("pubDate", new DateTime(2009, 9, 7).ToString("r")),
                        new XElement(
                            "guid",
                            new XAttribute("isPermaLink", "true"), // "isPermaLink" is implicitly converted to XName.
                            "https://weblogs.asp.net/dixin/linq-via-csharp"),
                        new XElement("category", "C#"),
                        new XElement("category", "LINQ"),
                        new XComment("Comment."),
                        new XElement(
                            @namespace + "source",
                            "https://github.com/Dixin/CodeSnippets/tree/master/Dixin/Linq"))));
            rss.ToString().WriteLine(); // Serialize XDocument to string.
        }
    }

    internal static partial class Modeling
    {
        internal static void Construction()
        {
            XDeclaration declaration = new XDeclaration("1.0", null, "no");
            declaration.WriteLine(); // <?xml version="1.0" standalone="no"?>

            XDocumentType documentType = new XDocumentType("html", null, null, null);
            documentType.WriteLine(); // <!DOCTYPE html >

            XText text = new XText("<p>text</p>");
            text.WriteLine(); // &lt;p&gt;text&lt;/p&gt;

            XCData cData = new XCData("cdata");
            cData.WriteLine(); // <![CDATA[cdata]]>

            XProcessingInstruction processingInstruction = new XProcessingInstruction(
                "xml-stylesheet", @"type=""text/xsl"" href=""Style.xsl""");
            processingInstruction.WriteLine(); // <?xml-stylesheet type="text/xsl" href="Style.xsl"?>
        }

        internal static void Name()
        {
            XName attributeName1 = "isPermaLink"; // Implicitly convert string to XName.
            XName attributeName2 = XName.Get("isPermaLink");
            XName attributeName3 = "IsPermaLink";
            object.ReferenceEquals(attributeName1, attributeName2).WriteLine(); // True
            (attributeName1 == attributeName2).WriteLine(); // True
            (attributeName1 != attributeName3).WriteLine(); // True
        }

        internal static void Namespace()
        {
            XNamespace namespace1 = "http://www.w3.org/XML/1998/namespace"; // Implicitly convert string to XNamespace.
            XNamespace namespace2 = XNamespace.Xml;
            XNamespace namespace3 = XNamespace.Get("http://www.w3.org/2000/xmlns/");
            (namespace1 == namespace2).WriteLine(); // True
            (namespace1 != namespace3).WriteLine(); // True

            XNamespace @namespace = "https://weblogs.asp.net/dixin";
            XName name = @namespace + "localName"; // + operator.
            name.WriteLine(); // {https://weblogs.asp.net/dixin}localName
            XElement element = new XElement(name, new XAttribute(XNamespace.Xmlns + "dixin", @namespace)); // + operator.
            element.WriteLine(); // <dixin:localName xmlns:dixin="https://weblogs.asp.net/dixin" />
        }

        internal static void Element()
        {
            XElement pubDateElement = XElement.Parse("<pubDate>Mon, 07 Sep 2009 00:00:00 GMT</pubDate>");
            DateTime pubDate = (DateTime)pubDateElement;
            pubDate.WriteLine(); // 9/7/2009 12:00:00 AM
        }

        internal static void Attribute()
        {
            XName name = "isPermaLink";
            XAttribute isPermaLinkAttribute = new XAttribute(name, "true");
            bool isPermaLink = (bool)isPermaLinkAttribute;
            isPermaLink.WriteLine(); // True
        }

        internal static void DeepEquals()
        {
            XElement element1 = XElement.Parse("<parent><child></child></parent>");
            XElement element2 = new XElement("parent", new XElement("child")); // <parent><child /></parent>
            object.ReferenceEquals(element1, element2).WriteLine(); // False
            XNode.DeepEquals(element1, element2).WriteLine(); // True

            XElement element3 = new XElement("parent", new XElement("child", string.Empty)); // <parent><child></child></parent>
            object.ReferenceEquals(element1, element2).WriteLine(); // False
            XNode.DeepEquals(element1, element3).WriteLine(); // False
        }

        internal static void Read()
        {
            using (XmlReader reader = XmlReader.Create("https://weblogs.asp.net/dixin/rss"))
            {
                reader.MoveToContent();
                XNode node = XNode.ReadFrom(reader);
            }

            XElement element1 = XElement.Parse("<html><head></head><body></body></html>");
            XElement element2 = XElement.Load("https://weblogs.asp.net/dixin/rss");

            XDocument document1 = XDocument.Parse("<html><head></head><body></body></html>");
            XDocument document2 = XDocument.Load("https://microsoft.com"); // Succeed.
            XDocument document3 = XDocument.Load("https://asp.net"); // Fail.
            // System.Xml.XmlException: The 'ul' start tag on line 68 position 116 does not match the end tag of 'div'. Line 154, position 109.
        }

        internal static IEnumerable<XElement> RssItems(string rssUri)
        {
            using (XmlReader reader = XmlReader.Create(rssUri))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("item", StringComparison.Ordinal))
                    {
                        yield return (XElement)XNode.ReadFrom(reader);
                    }
                }
            }
        }

        internal static void Write()
        {
            XDocument document1 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            using (FileStream stream = File.OpenWrite(Path.GetTempFileName()))
            {
                document1.Save(stream);
            }

            XElement element1 = new XElement("element", string.Empty);
            XDocument document2 = new XDocument();
            using (XmlWriter writer = document2.CreateWriter())
            {
                element1.WriteTo(writer);
            }
            document2.WriteLine(); // <element></element>

            XElement element2 = new XElement("element", string.Empty);
            using (XmlWriter writer = element2.CreateWriter())
            {
                writer.WriteStartElement("child");
                writer.WriteAttributeString("attribute", "value");
                writer.WriteString("text");
                writer.WriteEndElement();
            }
            element2.ToString(SaveOptions.DisableFormatting).WriteLine();
            // <element><child attribute="value">text</child></element>
        }

        internal static void XNodeToString()
        {
            XDocument document = XDocument.Parse(
                "<root xmlns:prefix='namespace'><element xmlns:prefix='namespace' /></root>");
            document.ToString(SaveOptions.None).WriteLine(); // Equivalent to document.ToString().
            // <root xmlns:prefix="namespace">
            //  <element xmlns:prefix="namespace" />
            // </root>
            document.ToString(SaveOptions.DisableFormatting).WriteLine();
            // <root xmlns:prefix="namespace"><element xmlns:prefix="namespace" /></root>
            document.ToString(SaveOptions.OmitDuplicateNamespaces).WriteLine();
            // <root xmlns:prefix="namespace">
            //  <element />
            // </root>
        }

        internal static void StreamingElementWithChildElements()
        {
            IEnumerable<XElement> ChildElementsFactory() =>
                Enumerable
                    .Range(0, 5).Do(value => value.WriteLine())
                    .Select(value => new XElement("child", value));

            XElement immediateParent = new XElement("parent", ChildElementsFactory()); // 0 1 2 3 4.
            immediateParent.ToString(SaveOptions.DisableFormatting).WriteLine();
            // <parent><child>0</child><child>1</child><child>2</child><child>3</child><child>4</child></parent>

            XStreamingElement deferredParent = new XStreamingElement("parent", ChildElementsFactory()); // Deferred.
            deferredParent.ToString(SaveOptions.DisableFormatting).WriteLine();
            // 0 1 2 3 4 
            // <parent><child>0</child><child>1</child><child>2</child><child>3</child><child>4</child></parent>
        }

        internal static void StreamingElementWithChildElementModification()
        {
            XElement source = new XElement("source", new XElement("child", "a"));
            XElement child = source.Elements().Single();

            XElement immediateParent = new XElement("parent", child);
            XStreamingElement deferredParent = new XStreamingElement("parent", child); // Deferred.

            child.Value = "b";
            immediateParent.ToString(SaveOptions.DisableFormatting).WriteLine(); // <parent><child>a</child></parent>
            deferredParent.ToString(SaveOptions.DisableFormatting).WriteLine(); // <parent><child>b</child></parent>
        }
    }
}
