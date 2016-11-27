namespace Dixin.Linq.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    using Dixin.Common;

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
            // Serialize XDocument to string.
            Trace.WriteLine(rss.ToString());
        }
    }

    internal static partial class Modeling
    {
        internal static void Construction()
        {
            XDeclaration declaration = new XDeclaration("1.0", null, "no");
            Trace.WriteLine(declaration); // <?xml version="1.0" standalone="no"?>

            XDocumentType documentType = new XDocumentType("html", null, null, null);
            Trace.WriteLine(documentType); // <!DOCTYPE html >

            XText text = new XText("<p>text</p>");
            Trace.WriteLine(text); // &lt;p&gt;text&lt;/p&gt;

            XCData cData = new XCData("cdata");
            Trace.WriteLine(cData); // <![CDATA[cdata]]>

            XProcessingInstruction processingInstruction = new XProcessingInstruction(
                "xml-stylesheet", @"type=""text/xsl"" href=""Style.xsl""");
            Trace.WriteLine(processingInstruction); // <?xml-stylesheet type="text/xsl" href="Style.xsl"?>
        }

        internal static void Name()
        {
            XName attributeName1 = "isPermaLink"; // Implicitly convert string to XName.
            XName attributeName2 = XName.Get("isPermaLink");
            XName attributeName3 = "IsPermaLink";
            Trace.WriteLine(object.ReferenceEquals(attributeName1, attributeName2)); // True
            Trace.WriteLine(attributeName1 == attributeName2); // True
            Trace.WriteLine(attributeName1 != attributeName3); // True
        }

        internal static void Namespace()
        {
            XNamespace @namespace1 = "http://www.w3.org/XML/1998/namespace"; // Implicitly convert string to XNamespace.
            XNamespace @namespace2 = XNamespace.Xml;
            XNamespace @namespace3 = XNamespace.Get("http://www.w3.org/2000/xmlns/");
            Trace.WriteLine(@namespace1 == @namespace2); // True
            Trace.WriteLine(@namespace1 != @namespace3); // True

            XNamespace @namespace = "https://weblogs.asp.net/dixin";
            XName name = @namespace + "localName"; // + operator.
            Trace.WriteLine(name); // {https://weblogs.asp.net/dixin}localName
            XElement element = new XElement(name, new XAttribute(XNamespace.Xmlns + "dixin", @namespace)); // + operator.
            Trace.WriteLine(element); // <dixin:localName xmlns:dixin="https://weblogs.asp.net/dixin" />
        }

        internal static void Element()
        {
            XElement pubDateElement = XElement.Parse("<pubDate>Mon, 07 Sep 2009 00:00:00 GMT</pubDate>");
            DateTime pubDate = (DateTime)pubDateElement;
            Trace.WriteLine(pubDate); // 9/7/2009 12:00:00 AM
        }

        internal static void Attribute()
        {
            XName name = "isPermaLink";
            XAttribute isPermaLinkAttribute = new XAttribute(name, "true");
            bool isPermaLink = (bool)isPermaLinkAttribute;
            Trace.WriteLine(isPermaLink); // True
        }

        internal static void Node()
        {
            XDocument document = XDocument.Parse("<element></element>");
            XElement element1 = new XElement("element", null); // <element />
            XElement element2 = new XElement("element", string.Empty); // <element></element>
            Trace.WriteLine(XNode.DeepEquals(document.Root, element1)); // False
            Trace.WriteLine(XNode.DeepEquals(document.Root, element2)); // True
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
            XDocument document2 = XDocument.Load("https://microsoft.com"); // Success.
            XDocument document3 = XDocument.Load("https://asp.net"); // Fail.
            // System.Xml.XmlException: The 'ul' start tag on line 68 position 116 does not match the end tag of 'div'. Line 154, position 109.
        }

        internal static IEnumerable<XElement> RssItems(string rssUrl)
        {
            using (XmlReader reader = XmlReader.Create(rssUrl))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.EqualsOrdinal("item"))
                    {
                        yield return (XElement)XNode.ReadFrom(reader);
                    }
                }
            }
        }

        internal static void Write()
        {
            XDocument document1 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            document1.Save(Path.GetTempFileName());

            XElement element1 = new XElement("element", string.Empty);
            using (XmlTextWriter writer = new XmlTextWriter(Console.Out))
            {
                element1.WriteTo(writer); // <element></element>
            }

            XDocument document2 = new XDocument();
            using (XmlWriter writer = document2.CreateWriter())
            {
                element1.WriteTo(writer);
            }
            Trace.WriteLine(document2); // <element></element>

            XElement element2 = new XElement("element", string.Empty);
            using (XmlWriter writer = element2.CreateWriter())
            {
                writer.WriteStartElement("child");
                writer.WriteAttributeString("attribute", "value");
                writer.WriteString("text");
                writer.WriteEndElement();
            }
            Trace.WriteLine(element2.ToString(SaveOptions.DisableFormatting));
            // <element><child attribute="value">text</child></element>
        }

        internal static void XNodeToString()
        {
            XDocument document = XDocument.Parse(
                "<root xmlns:prefix='namespace'><element xmlns:prefix='namespace' /></root>");
            Trace.WriteLine(document.ToString(SaveOptions.None)); // Equivalent to document.ToString().
            // <root xmlns:prefix="namespace">
            //  <element xmlns:prefix="namespace" />
            // </root>
            Trace.WriteLine(document.ToString(SaveOptions.DisableFormatting));
            // <root xmlns:prefix="namespace"><element xmlns:prefix="namespace" /></root>
            Trace.WriteLine(document.ToString(SaveOptions.OmitDuplicateNamespaces));
            // <root xmlns:prefix="namespace">
            //  <element />
            // </root>
        }

        internal static void StreamingElement()
        {
            Func<IEnumerable<XElement>> getChildElements = () => Enumerable
                .Range(0, 5)
                .Do(value => Trace.WriteLine(value))
                .Select(value => new XElement("child", value));

            XElement immediate1 = new XElement("parent", getChildElements()); // 0 1 2 3 4.

            XStreamingElement deferred1 = new XStreamingElement("parent", getChildElements());
            Trace.WriteLine(deferred1.ToString(SaveOptions.DisableFormatting));
            // 0 1 2 3 4 <parent><child>0</child><child>1</child><child>2</child><child>3</child><child>4</child></parent>

            XElement immediate2 = new XElement("parent", immediate1.Elements());
            XStreamingElement deferred2 = new XStreamingElement("parent", immediate1.Elements());
            immediate1.RemoveAll();
            Trace.WriteLine(immediate2.ToString(SaveOptions.DisableFormatting));
            // <parent><child>0</child><child>1</child><child>2</child><child>3</child><child>4</child></parent>
            Trace.WriteLine(deferred2); // <parent />
        }
    }
}
