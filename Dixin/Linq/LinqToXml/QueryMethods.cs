namespace Dixin.Linq.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal static class QueryMethods
    {
        internal static void ParentAndAncestors()
        {
            XElement element = new XElement("element");
            new XDocument(new XElement("grandparent", new XElement("parent", element)));
            Trace.WriteLine(element.Parent.ToString(SaveOptions.DisableFormatting));
            // <parent><element /></parent>.

            element
                .Ancestors()
                .ForEach(ancestor => Trace.WriteLine(ancestor.ToString(SaveOptions.DisableFormatting)));
            // <parent><element /></parent>
            // <grandparent><parent><element /></parent></grandparent>

            element
                .AncestorsAndSelf()
                .ForEach(ancestorOrSelf => Trace.WriteLine(ancestorOrSelf.ToString(SaveOptions.DisableFormatting)));
            // <element />.
            // <parent><element /></parent>.
            // <grandparent><parent><element /></parent></grandparent>.

            Trace.WriteLine(object.ReferenceEquals(element.Ancestors().Last(), element.Document.Root)); // True.
        }

        internal static void InDocumentOrder()
        {
            XElement element = new XElement("element");
            new XDocument(new XElement("grandparent", new XElement("parent", element)));
            element
                .Ancestors()
                .InDocumentOrder()
                .ForEach(ancestor => Trace.WriteLine(ancestor.ToString(SaveOptions.DisableFormatting)));
            // <grandparent><parent><element /></parent></grandparent>
            // <parent><element /></parent>

            bool areSequentialEqual = element
                .AncestorsAndSelf()
                .Reverse()
                .SequenceEqual(element.AncestorsAndSelf().InDocumentOrder());
            Trace.WriteLine(areSequentialEqual); // True.
        }

        internal static void CommonAncestor()
        {
            // https://www.w3.org/TR/xml/#NT-AttValue
            XElement root = XElement.Parse(@"
                <root>
                  <element value='4' />
                  <element value='2' />
                  <element value='3'><element value='1' /></element>
                </root>");
            XElement[] elements = root
                .Descendants("element")
                .OrderBy(element => (int)element.Attribute("value")).ToArray();
            elements.ForEach(ancestorOrSelf => Trace.WriteLine(ancestorOrSelf.ToString(SaveOptions.DisableFormatting)));
            // <element value="1" />
            // <element value="2" />
            // <element value="3"><element value="1" /></element>
            // <element value="4" />

            new XElement[] { elements.First(), elements.Last() }
                .InDocumentOrder()
                .ForEach(ancestorOrSelf => Trace.WriteLine(ancestorOrSelf.ToString(SaveOptions.DisableFormatting)));
            // <element value="4" />
            // <element value="1" />

            new XElement[] { elements.First(), elements.Last(), new XElement("element") }
                .InDocumentOrder()
                .ForEach();
            // InvalidOperationException: A common ancestor is missing.
        }

        internal static void Elements()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<string> categories = rss
                .Root // <rss>.
                .Element("channel") // <channel> under <rss>.
                .Elements("item") // All <item>s under <channel>.
                .Where(item => (bool)item
                    .Element("guid") // <category> under each <item>
                    .Attribute("isPermaLink")) // <category>'s isPermaLink attribute.
                .Elements("category") // All <category>s under all <item>s.
                .GroupBy(
                    category => (string)category, // String value of <category>.
                    category => category,
                    (key, group) => new { Name = key, Count = group.Count() },
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}");
            Trace.WriteLine(string.Join(" ", categories));
            // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4.
        }

        internal static void ChildrenAndDecendants()
        {
            XElement root = XElement.Parse(@"
                <root>
                  <![CDATA[cdata]]>0<!--Comment-->
                  <element>1</element>
                  <element>2<element>3</element></element>
                </root>");

            root.Elements()
                .ForEach(element => Trace.WriteLine(element.ToString(SaveOptions.DisableFormatting)));
            // <element>1</element>
            // <element>2<element>3</element></element>

            root.Nodes()
                .ForEach(node => Trace.WriteLine($"{node.NodeType}: {node.ToString(SaveOptions.DisableFormatting)}"));
            // CDATA: <![CDATA[cdata]]>
            // Text: 0
            // Comment: <!--Comment-->
            // Element: <element>1</element>
            // Element: <element>2<element>3</element></element>

            root.Descendants()
                .ForEach(element => Trace.WriteLine(element.ToString(SaveOptions.DisableFormatting)));
            // <element>1</element>
            // <element>2<element>3</element></element>
            // <element>3</element>

            root.DescendantNodes()
                .ForEach(node => Trace.WriteLine($"{node.NodeType}: {node.ToString(SaveOptions.DisableFormatting)}"));
            // CDATA: <![CDATA[cdata]]>
            // Text: 0
            // Comment: <!--Comment-->
            // Element: <element>1</element>
            // Text: 1
            // Element: <element>2<element>3</element></element>
            // Text: 2
            // Element: <element>3</element>
            // Text: 3
        }

        internal static void ResultXObjects()
        {
            XDocument rss1 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XElement[] query1 = rss1.Descendants("item").ToArray();
            XElement[] query2 = rss1.Element("rss").Element("channel").Elements("item").ToArray();
            Trace.WriteLine(object.ReferenceEquals(query1.First(), query2.First())); // True.
            Trace.WriteLine(query1.SequenceEqual(query2)); // True.

            XDocument rss2 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XElement[] query3 = rss2.Root.Descendants("item").ToArray();
            Trace.WriteLine(object.ReferenceEquals(query1.First(), query3.First())); // False.
            Trace.WriteLine(query1.SequenceEqual(query3)); // False.
        }

        internal static void XPathNavigator()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XPathNavigator rssNavigator = rss.CreateNavigator();
            Trace.WriteLine(rssNavigator.NodeType); // Root.
            Trace.WriteLine(rssNavigator.MoveToFirstChild()); // True.
            Trace.WriteLine(rssNavigator.Name); // rss.
            IEnumerable<string> categories = rssNavigator
                .Select(XPathExpression.Compile("/rss/channel/item[guid/@isPermaLink='true']/category/text()"))
                .OfType<XPathNavigator>()
                .GroupBy(
                    categoryNavigator => categoryNavigator.Value, // Text node's value.
                    categoryNavigator => categoryNavigator,
                    (key, group) => new { Name = key, Count = group.Count() },
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}");
            Trace.WriteLine(string.Join(" ", categories));
            // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4.
        }

        internal static void XPathQuery()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<XElement> query1 = rss.XPathSelectElements("/rss/channel/item");
            IEnumerable<XElement> query2 = rss.Element("rss").Element("channel").Elements("item");
            Trace.WriteLine(query1.SequenceEqual(query2)); // True.

            XElement latestItem1 = rss.XPathSelectElement("/rss/channel/item[1]");
            XElement latestItem2 = rss.Element("rss").Element("channel").Elements("item").First();
            Trace.WriteLine(object.ReferenceEquals(latestItem1, latestItem2)); // True.
        }

        internal static void XPathQueryWithNamespace()
        {
            XDocument rss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            XmlNamespaceManager namespaceManager = rss.GetNamespaceManager();
            IEnumerable<XElement> query1 = rss.XPathSelectElements("/rss/channel/item/media:category", namespaceManager);
            Trace.WriteLine(query1.Count());

            IEnumerable<XElement> query2 = rss.XPathSelectElements("/rss/channel/item/media:category");
            // System.Xml.XPath.XPathException: Namespace Manager or XsltContext needed. This query has a prefix, variable, or user-defined function.
        }

        internal static void XPathEvaluateObject()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            double average1 = (double)rss.XPathEvaluate("count(/rss/channel/item/category) div count(/rss/channel/item)");
            Trace.WriteLine(average1); // 4.65.
            double average2 = rss
                .Element("rss")
                .Element("channel")
                .Elements("item")
                .Average(item => item.Elements("category").Count());
            Trace.WriteLine(average2); // 4.65.
        }

        internal static void XPathEvaluateSequence()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<string> categories = (rss
                .XPathEvaluate("/rss/channel/item[guid/@isPermaLink='true']/category/text()") as IEnumerable<object>)
                .OfType<XText>()
                .GroupBy(
                    categoryTextNode => categoryTextNode.Value, // Text node's value.
                    categoryTextNode => categoryTextNode,
                    (key, group) => new { Name = key, Count = group.Count() },
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}");
            Trace.WriteLine(string.Join(" ", categories));
            // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4.
        }

        internal static void XPathEvaluateSequenceWithNamespace()
        {
            XDocument rss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            IEnumerable<XText> mediaTitles = (rss
                .XPathEvaluate(
                    "/rss/channel/item[contains(media:category/text(), 'microsoft')]/media:title/text()",
                    rss.GetNamespaceManager()) as IEnumerable<object>)
                .OfType<XText>();
            mediaTitles.ForEach(mediaTitle => Trace.WriteLine(mediaTitle.Value));
            // Chinese President visits Microsoft
            // Chinese President visits Microsoft
            // Satya Nadella, CEO of Microsoft
        }
    }
}
