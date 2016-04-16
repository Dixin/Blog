namespace Dixin.Linq.LinqToXml
{
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

    internal static class Manipulation
    {
        internal static void AddChildNode()
        {
            XElement child = new XElement("child");
            XElement parent1 = new XElement("parent", child); // Child node is attached.
            Trace.WriteLine(object.ReferenceEquals(child, parent1.Elements().Single())); // True.

            XElement parent2 = new XElement("parent", child); // Child node is already attached, so it is cloned.
            Trace.WriteLine(object.ReferenceEquals(child, parent2.Elements().Single())); // False.
        }

        internal static void Delete()
        {
            XDocument rss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2&tags=Microsoft");
            EnumerableEx.ForEach(rss.Root.Elements("item"), element => element.Remove());
            Trace.WriteLine(rss.Root);
        }

        internal static void Clone()
        {
            XElement element = XElement.Parse("<element />");
            XElement clonedElement = new XElement(element);

            XText text = new XText("text");
            XText clonedText = new XText(text);

            XDocument document = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XDocument clonedDocument = new XDocument(document);
            Trace.WriteLine(object.ReferenceEquals(document, clonedDocument)); // False.
            Trace.WriteLine(object.Equals(document, clonedDocument)); // False.
            Trace.WriteLine(document.Equals(clonedDocument)); // False.
            Trace.WriteLine(document == clonedDocument); // False.
            Trace.WriteLine(XNode.DeepEquals(document, clonedDocument)); // True.
            Trace.WriteLine(XNode.EqualityComparer.Equals(document, clonedDocument)); // True.
        }

        internal static void XslTransform()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XDocument xsl = XDocument.Parse(@"
                <xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                  <xsl:template match='/rss/channel'>
                    <ul>
                      <xsl:for-each select='item[position() &lt;= 5]'><!--Position is less than or equal to 5.-->
                        <li>
                          <a>
                            <xsl:attribute name='href'><xsl:value-of select='link' /></xsl:attribute>
                            <xsl:value-of select='title' />
                          </a>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </xsl:template>
                </xsl:stylesheet>");
            XDocument html = new XDocument();
            using (XmlReader rssReader = rss.CreateReader())
            using (XmlReader xslReader = xsl.CreateReader())
            using (XmlWriter htmlWriter = html.CreateWriter())
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xslReader);
                transform.Transform(rssReader, htmlWriter);
            }
            Trace.WriteLine(html);
            // <ul>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/c-6-0-exception-filter-and-when-keyword">C# 6.0 Exception Filter and when Keyword</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/use-fiddler-with-node-js">Use Fiddler with Node.js</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/diskpart-problem-cannot-select-partition">DiskPart Problem: Cannot Select Partition</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/configure-git-for-visual-studio-2015">Configure Git for Visual Studio 2015</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/query-operating-system-processes-in-c">Query Operating System Processes in C#</a>
            //  </li>
            // </ul>
        }

        internal static void Transform()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XDocument html = rss
                .Element("rss")
                .Element("channel")
                .Elements("item")
                .Take(5)
                .Select(item => new XElement(
                    "li",
                    new XElement(
                        "a", new XAttribute("href", (string)item.Element("link")), (string)item.Element("title"))))
                .Aggregate(new XElement("ul"), (ul, li) => { ul.Add(li); return ul; }, ul => new XDocument(ul));
            Trace.WriteLine(html);
            // <ul>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/c-6-0-exception-filter-and-when-keyword">C# 6.0 Exception Filter and when Keyword</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/use-fiddler-with-node-js">Use Fiddler with Node.js</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/diskpart-problem-cannot-select-partition">DiskPart Problem: Cannot Select Partition</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/configure-git-for-visual-studio-2015">Configure Git for Visual Studio 2015</a>
            //  </li>
            //  <li>
            //    <a href="https://weblogs.asp.net:443/dixin/query-operating-system-processes-in-c">Query Operating System Processes in C#</a>
            //  </li>
            // </ul>
        }

        internal static void Validate()
        {
            XDocument rss1 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XmlSchemaSet schemaSet = rss1.InferSchema();
            EnumerableEx.ForEach(schemaSet.Schemas().OfType<XmlSchema>(), schema => Trace.WriteLine(schema.ToXDocument()));
            // <xs:schema attributeFormDefault="unqualified" elementFormDefault="qualified" xmlns:xs="http://www.w3.org/2001/XMLSchema">
            //  <xs:element name="rss">
            //    <xs:complexType>
            //      <xs:sequence>
            //        <xs:element name="channel">
            //          <xs:complexType>
            //            <xs:sequence>
            //              <xs:element name="title" type="xs:string" />
            //              <xs:element name="link" type="xs:string" />
            //              <xs:element name="description" type="xs:string" />
            //              <xs:element maxOccurs="unbounded" name="item">
            //                <xs:complexType>
            //                  <xs:sequence>
            //                    <xs:element name="title" type="xs:string" />
            //                    <xs:element name="link" type="xs:string" />
            //                    <xs:element name="description" type="xs:string" />
            //                    <xs:element name="pubDate" type="xs:string" />
            //                    <xs:element name="guid">
            //                      <xs:complexType>
            //                        <xs:simpleContent>
            //                          <xs:extension base="xs:string">
            //                            <xs:attribute name="isPermaLink" type="xs:boolean" use="required" />
            //                          </xs:extension>
            //                        </xs:simpleContent>
            //                      </xs:complexType>
            //                    </xs:element>
            //                    <xs:element maxOccurs="unbounded" name="category" type="xs:string" />
            //                  </xs:sequence>
            //                </xs:complexType>
            //              </xs:element>
            //            </xs:sequence>
            //          </xs:complexType>
            //        </xs:element>
            //      </xs:sequence>
            //      <xs:attribute name="version" type="xs:decimal" use="required" />
            //    </xs:complexType>
            //  </xs:element>
            // </xs:schema>

            XDocument rss2 = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            rss2.Validate(
                schemaSet,
                (sender, args) =>
                    {
                        Trace.WriteLine(args.Message);
                        // The element 'channel' has invalid child element 'pubDate'. List of possible elements expected: 'item'.
                        if (args.Exception != null)
                        {
                            Trace.WriteLine(args.Exception);
                            // System.Xml.Schema.XmlSchemaValidationException: The element 'channel' has invalid child element 'pubDate'. List of possible elements expected: 'item'.
                        }
                    },
                addSchemaInfo: true);
            rss2
                .GetValidities()
                .ForEach(validity => Trace.WriteLine($"{validity.Item2} - {validity.Item3?.ToString() ?? "null"}"));
            // /rss - Invalid
            // /rss/@version - Valid
            // /rss/@xmlns:media - null
            // /rss/@xmlns:dc - null
            // /rss/@xmlns:creativeCommons - null
            // /rss/@xmlns:flickr - null
            // /rss/channel - Invalid
            // /rss/channel/title - Valid
            // /rss/channel/link - Valid
            // /rss/channel/description - Valid
            // /rss/channel/pubDate - Invalid
            // /rss/channel/lastBuildDate - NotKnown
            // /rss/channel/generator - NotKnown
            // ...
        }
    }
}