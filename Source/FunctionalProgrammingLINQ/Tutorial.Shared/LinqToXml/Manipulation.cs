namespace Tutorial.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Schema;

    internal static class Manipulation
    {
        internal static void ExplicitClone()
        {
            XElement sourceElement = XElement.Parse("<element />");
            XElement clonedElement = new XElement(sourceElement);

            XText sourceText = new XText("text");
            XText clonedText = new XText(sourceText);

            XDocument sourceDocument = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XDocument clonedDocument = new XDocument(sourceDocument);
            object.ReferenceEquals(sourceDocument, clonedDocument).WriteLine(); // False
            object.Equals(sourceDocument, clonedDocument).WriteLine(); // False
            EqualityComparer<XDocument>.Default.Equals(sourceDocument, clonedDocument).WriteLine(); // False
            sourceDocument.Equals(clonedDocument).WriteLine(); // False
            (sourceDocument == clonedDocument).WriteLine(); // False
            XNode.DeepEquals(sourceDocument, clonedDocument).WriteLine(); // True
            XNode.EqualityComparer.Equals(sourceDocument, clonedDocument).WriteLine(); // True
        }

        internal static void ImplicitClone()
        {
            XElement child = XElement.Parse("<child />");
            XName parentName = "parent";
            XElement parent1 = new XElement(parentName, child); // Attach.
            object.ReferenceEquals(child, parent1.Elements().Single()).WriteLine(); // True
            object.ReferenceEquals(parentName, parent1.Name).WriteLine(); // True

            XElement parent2 = new XElement(parentName, child); // Clone and attach.
            object.ReferenceEquals(child, parent2.Elements().Single()).WriteLine(); // False
            object.ReferenceEquals(parentName, parent2.Name).WriteLine(); // True

            XElement element = new XElement("element");
            element.Add(element); // Clone and attach.
            object.ReferenceEquals(element, element.Elements().Single()).WriteLine(); // False
        }

        internal static void Manipulate()
        {
            XElement child = new XElement("child");
            child.Changing += (sender, e) => 
                $"Before {e.ObjectChange}: ({sender.GetType().Name} {sender}) => {child}".WriteLine();
            child.Changed += (sender, e) => 
                $"After {e.ObjectChange}: ({sender.GetType().Name} {sender}) => {child}".WriteLine();
            XElement parent = new XElement("parent");
            parent.Changing += (sender, e) => 
                $"Before {e.ObjectChange}: ({sender.GetType().Name} {sender}) => {parent.ToString(SaveOptions.DisableFormatting)}".WriteLine();
            parent.Changed += (sender, e) => 
                $"After {e.ObjectChange}: ({sender.GetType().Name} {sender}) => {parent.ToString(SaveOptions.DisableFormatting)}".WriteLine();

            child.Value = "value1";
            // Before Add: (XText value1) => <child />
            // After Add: (XText value1) => <child>value1</child>

            child.Value = "value2";
            // Before Remove: (XText value1) => <child>value1</child>
            // After Remove: (XText value1) => <child />
            // Before Add: (XText value2) => <child />
            // After Add: (XText value2) => <child>value2</child>

            child.Value = string.Empty;
            // Before Remove: (XText value2) => <child>value2</child>
            // After Remove: (XText value2) => <child />
            // Before Value: (XElement <child />) => <child />
            // After Value: (XElement <child></child>) => <child></child>

            parent.Add(child);
            // Before Add: (XElement <child></child>) => <parent />
            // After Add: (XElement <child></child>) => <parent><child></child></parent>

            child.Add(new XAttribute("attribute", "value"));
            // Before Add: (XAttribute attribute="value") => <child></child>
            // Before Add: (XAttribute attribute="value") => <parent><child></child></parent>
            // After Add: (XAttribute attribute="value") => <child attribute="value"></child>
            // After Add: (XAttribute attribute="value") => <parent><child attribute="value"></child></parent>

            child.AddBeforeSelf(0);
            // Before Add: (XText 0) => <parent><child attribute="value"></child></parent>
            // After Add: (XText 0) => <parent>0<child attribute="value"></child></parent>

            parent.ReplaceAll(new XText("Text."));
            // Before Remove: (XText 0) => <parent>0<child attribute="value"></child></parent>
            // After Remove: (XText 0) => <parent><child attribute="value"></child></parent>
            // Before Remove: (XElement <child attribute="value"></child>) => <parent><child attribute="value"></child></parent>
            // After Remove: (XElement <child attribute="value"></child>) => <parent />
            // Before Add: (XText Text.) => <parent />
            // After Add: (XText Text.) => <parent>Text.</parent>

            parent.Name = "name";
            // Before Name: (XElement <parent>Text.</parent>) => <parent>Text.</parent>
            // After Name: (XElement <name>Text.</name>) => <name>Text.</name>

            XElement clonedChild = new XElement(child);
            clonedChild.SetValue(DateTime.Now); // No tracing.
        }

        internal static void SetAttributeValue()
        {
            XElement element = new XElement("element");
            element.Changing += (sender, e) => 
                $"Before {e.ObjectChange}: ({sender.GetType().Name} {sender}) => {element}".WriteLine();
            element.Changed += (sender, e) => 
                $"After {e.ObjectChange}: ({sender.GetType().Name} {sender}) => {element}".WriteLine();

            element.SetAttributeValue("attribute", "value1"); // Equivalent to: child1.Add(new XAttribute("attribute", "value1"));
            // Before Add: (XAttribute attribute="value1") => <element />
            // After Add: (XAttribute attribute="value1") => <element attribute="value1" />

            element.SetAttributeValue("attribute", "value2"); // Equivalent to: child1.Attribute("attribute").Value = "value2";
            // Before Value: (XAttribute attribute="value1") => <element attribute="value1" />
            // After Value: (XAttribute attribute="value2") => <element attribute="value2" />

            element.SetAttributeValue("attribute", null);
            // Before Remove: (XAttribute attribute="value2") => <element attribute="value2" />
            // After Remove: (XAttribute attribute="value2") => <element />
        }

        internal static void SetElementValue()
        {
            XElement parent = new XElement("parent");
            parent.Changing += (sender, e) => 
                $"Before {e.ObjectChange}: {sender} => {parent.ToString(SaveOptions.DisableFormatting)}".WriteLine();
            parent.Changed += (sender, e) => 
                $"After {e.ObjectChange}: {sender} => {parent.ToString(SaveOptions.DisableFormatting)}".WriteLine();

            parent.SetElementValue("child", string.Empty); // Add child element.
            // Before Add: <child></child> => <parent />
            // After Add: <child></child> => <parent><child></child></parent>

            parent.SetElementValue("child", "value"); // Update child element.
            // Before Value: <child></child> => <parent><child></child></parent>
            // After Value: <child /> => <parent><child /></parent>
            // Before Add: value => <parent><child /></parent>
            // After Add: value => <parent><child>value</child></parent>

            parent.SetElementValue("child", null); // Remove child element.
            // Before Remove: <child>value</child> => <parent><child>value</child></parent>
            // After Remove: <child>value</child> => <parent />
        }

        internal static void Annotation()
        {
            XElement element = new XElement("element");
            element.AddAnnotation(new Uri("https://microsoft.com"));

            Uri annotation = element.Annotation<Uri>();
            annotation.WriteLine(); // https://microsoft.com
            element.WriteLine(); // <element />

            XElement clone = new XElement(element); // element is cloned.
            clone.Annotations<Uri>().Any().WriteLine(); // False

            element.RemoveAnnotations<Uri>();
            (element.Annotation<Uri>() == null).WriteLine(); // True
        }

        internal static void InferSchemas()
        {
            XDocument aspNetRss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            XmlSchemaSet schemaSet = aspNetRss.InferSchema();
            schemaSet.Schemas().Cast<XmlSchema>().WriteLines(schema => schema.ToXDocument().ToString());
        }

        internal static void Validate()
        {
            XDocument aspNetRss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XmlSchemaSet schemaSet = aspNetRss.InferSchema();

            XDocument flickrRss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            flickrRss.Validate(
                schemaSet,
                (sender, args) =>
                {
                    $"{args.Severity}: ({sender.GetType().Name}) => {args.Message}".WriteLine();
                    // Error: (XElement) => The element 'channel' has invalid child element 'pubDate'. List of possible elements expected: 'item'.
                    args.Exception?.WriteLine();
                    // XmlSchemaValidationException: The element 'channel' has invalid child element 'pubDate'. List of possible elements expected: 'item'.
                });
        }

        internal static void GetSchemaInfo()
        {
            XDocument aspNetRss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XmlSchemaSet schemaSet = aspNetRss.InferSchema();

            XDocument flickrRss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            flickrRss.Validate(schemaSet, (sender, args) => { }, addSchemaInfo: true);
            flickrRss
                .Root
                .DescendantsAndSelf()
                .ForEach(element =>
                {
                    $"{element.XPath()} - {element.GetSchemaInfo()?.Validity}".WriteLine();
                    element.Attributes().WriteLines(attribute => 
                        $"{attribute.XPath()} - {attribute.GetSchemaInfo()?.Validity.ToString() ?? "null"}");
                });
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
            // ...
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
#if !WINDOWS_UWP
            XDocument html = rss.XslTransform(xsl);
            html.WriteLine();
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
#endif
        }

        internal static void Transform()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XDocument html = rss
                .Element("rss")
                .Element("channel")
                .Elements("item")
                .Take(5)
                .Select(item =>
                {
                    string link = (string)item.Element("link");
                    string title = (string)item.Element("title");
                    return new XElement("li", new XElement("a", new XAttribute("href", link), title));
                    // Equivalent to: return XElement.Parse($"<li><a href='{link}'>{title}</a></li>");
                })
                .Aggregate(new XElement("ul"), (ul, li) => { ul.Add(li); return ul; }, ul => new XDocument(ul));
            html.WriteLine();
        }
    }
}
