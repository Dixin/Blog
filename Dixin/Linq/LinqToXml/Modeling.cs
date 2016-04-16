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
            Trace.WriteLine(rss); // Call rssItem.ToString.
        }
    }

    internal static partial class Modeling
    {
        internal static void Construction()
        {
            XDeclaration declaration = new XDeclaration("1.0", null, "no");
            Trace.WriteLine(declaration); // <?xml version="1.0" standalone="no"?>.

            XDocumentType documentType = new XDocumentType("html", null, null, null);
            Trace.WriteLine(documentType); // <!DOCTYPE html >.

            XComment comment = new XComment("Comment.");
            Trace.WriteLine(comment); // <!--Comment.-->.

            XText text = new XText("<p>text</p>");
            Trace.WriteLine(text); // &lt;p&gt;text&lt;/p&gt;.

            XCData cData = new XCData("cdata");
            Trace.WriteLine(cData); // <![CDATA[cdata]]>.

            XDocument document = new XDocument();
            Trace.WriteLine(document); // Empty.
        }

        internal static void Name()
        {
            XName attributeName1 = "isPermaLink"; // Implicitly convert string to XName.
            XName attributeName2 = XName.Get("isPermaLink");
            XName attributeName3 = "IsPermaLink";
            Trace.WriteLine(object.ReferenceEquals(attributeName1, attributeName2)); // True.
            Trace.WriteLine(attributeName1 == attributeName2); // True. == operators.
            Trace.WriteLine(attributeName1 != attributeName3); // True. != operators.
        }

        internal static void Namespace()
        {
            XNamespace @namespace1 = "http://www.w3.org/XML/1998/namespace"; // Implicitly convert string to XNamespace.
            XNamespace @namespace2 = XNamespace.Xml;
            XNamespace @namespace3 = XNamespace.Get("http://www.w3.org/2000/xmlns/");
            Trace.WriteLine(@namespace1 == @namespace2); // True. == operator.
            Trace.WriteLine(@namespace1 != @namespace3); // True. != operator.

            XNamespace @namespace = "https://weblogs.asp.net/dixin";
            XName name = @namespace + "localName"; // + operator.
            Trace.WriteLine(name); // {https://weblogs.asp.net/dixin}localName.
            XElement element = new XElement(name, new XAttribute(XNamespace.Xmlns + "dixin", @namespace)); // + operator.
            Trace.WriteLine(element); // <dixin:localName xmlns:dixin="https://weblogs.asp.net/dixin" />.
        }

        internal static void Element()
        {
            XElement pubDateElement = XElement.Parse("<pubDate>Mon, 07 Sep 2009 00:00:00 GMT</pubDate>");
            DateTime pubDate = (DateTime)pubDateElement;
            Trace.WriteLine(pubDate); // 9/7/2009 12:00:00 AM.
        }

        internal static void Attribute()
        {
            XName name = "isPermaLink";
            XAttribute isPermaLinkAttribute = new XAttribute(name, "true");
            bool isPermaLink = (bool)isPermaLinkAttribute;
            Trace.WriteLine(isPermaLink); // True.
        }

        internal static void Node()
        {
            XDocument document = XDocument.Parse("<outer><inner></inner></outer>");
            XElement element1 = new XElement("outer", new XElement("inner", null)); // <inner/>.
            XElement element2 = new XElement("outer", new XElement("inner", string.Empty)); // <inner></inner>.
            Trace.WriteLine(XNode.DeepEquals(document.Root, element1)); // False.
            Trace.WriteLine(XNode.DeepEquals(document.Root, element2)); // True.
        }

        internal static void Read()
        {
            XmlReader reader = XmlReader.Create("https://weblogs.asp.net/dixin/rss");
            reader.MoveToContent();
            XNode node = XNode.ReadFrom(reader);

            XDocument document1 = XDocument.Parse(@"<html><head></head><body></body></html>");
            XDocument document2 = XDocument.Load("https://weblogs.asp.net/dixin/rss");

            XElement element1 = XElement.Parse(@"<html><head></head><body></body></html>");
            XElement element2 = XElement.Load("https://weblogs.asp.net/dixin");
            // System.Xml.XmlException: '>' is an unexpected token. The expected token is '='. Line 270, position 48.
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
                        yield return XNode.ReadFrom(reader) as XElement;
                    }
                }
            }
        }

        internal static void Write()
        {
            XDocument document1 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            document1.Save(Path.GetTempFileName());
            
            XElement element1 = new XElement("element1", string.Empty);
            using (XmlTextWriter writer = new XmlTextWriter(Console.Out))
            {
                element1.WriteTo(writer); // <element1></element1>.
            }
            
            XDocument document2 = new XDocument();
            using (XmlWriter writer = document2.CreateWriter())
            {
                element1.WriteTo(writer);
            }
            Trace.WriteLine(document2); // <element1></element1>.

            XElement element2 = new XElement("element2", string.Empty);
            using (XmlWriter writer = element2.CreateWriter())
            {
                writer.WriteStartElement("child");
                writer.WriteAttributeString("attribute", "value");
                writer.WriteString("text");
                writer.WriteEndElement();
            }
            Trace.WriteLine(element2.ToString(SaveOptions.DisableFormatting)); // <element2><child attribute="value">text</child></element2>.
        }

        internal static void StreamingElement()
        {
            Func<IEnumerable<XElement>> getChildElements = () => Enumerable
                .Range(0, 5)
                .Do(value => Trace.WriteLine(value))
                .Select(value => new XElement("child", value));

            XElement immediate1 = new XElement("parent", getChildElements()); // 0 1 2 3 4.
            Trace.WriteLine(immediate1); // <parent>...

            XStreamingElement deferred1 = new XStreamingElement("parent", getChildElements());
            Trace.WriteLine(deferred1); // 0 1 2 3 4 <parent>...

            XElement immediate2 = new XElement("parent", immediate1.Elements());
            immediate1.Remove();
            XStreamingElement deferred2 = new XStreamingElement("parent", immediate1.Elements());
            immediate1.RemoveAll();
            Trace.WriteLine(deferred1); // 0 1 2 3 4 <parent>...
        }
    }
}
