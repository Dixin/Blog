namespace Tutorial.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using static Tutorial.LinqToObjects.EnumerableX;

    internal static partial class QueryMethods
    {
        internal static void ParentAndAncestors()
        {
            XElement element = new XElement("element");
            new XDocument(new XElement("grandparent", new XElement("parent", element)));

            element.Parent.Name.WriteLine(); // parent
            element
                .Ancestors()
                .Select(ancestor => ancestor.Name)
                .WriteLines(); // parent grandparent
            element
                .AncestorsAndSelf()
                .Select(selfOrAncestor => selfOrAncestor.Name)
                .WriteLines(); // element parent grandparent
            object.ReferenceEquals(element.Ancestors().Last(), element.Document.Root).WriteLine(); // True.
        }
    }

    internal static partial class QueryMethods
    {
        internal static void ChildElements()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<string> categories = rss
                .Root // <rss>.
                .Element("channel") // Single <channel> under <rss>.
                .Elements("item") // All <item>s under single <channel>.
                .Where(item => (bool)item
                    .Element("guid") // Single <guid> under each <item>
                    .Attribute("isPermaLink")) // isPermaLink attribute of <guid>.
                .Elements("category") // All <category>s under all <item>s.
                .GroupBy(
                    keySelector: category => (string)category, // String value of each <category>.
                    elementSelector: category => category,
                    resultSelector: (key, group) => new { Name = key, Count = group.Count() },
                    comparer: StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}");
            string.Join(" ", categories).WriteLine();
            // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4
        }

        internal static void ChildrenAndDescendants()
        {
            XElement root = XElement.Parse(@"
                <root>
                  <![CDATA[cdata]]>0<!--Comment-->
                  <element>1</element>
                  <element>2<element>3</element></element>
                </root>");

            root.Elements()
                .WriteLines(element => element.ToString(SaveOptions.DisableFormatting));
            // <element>1</element>
            // <element>2<element>3</element></element>

            root.Nodes()
                .WriteLines(node => $"{node.NodeType}: {node.ToString(SaveOptions.DisableFormatting)}");
            // CDATA: <![CDATA[cdata]]>
            // Text: 0
            // Comment: <!--Comment-->
            // Element: <element>1</element>
            // Element: <element>2<element>3</element></element>

            root.Descendants()
                .WriteLines(element => element.ToString(SaveOptions.DisableFormatting));
            // <element>1</element>
            // <element>2<element>3</element></element>
            // <element>3</element>

            root.DescendantNodes()
                .WriteLines(node => $"{node.NodeType}: {node.ToString(SaveOptions.DisableFormatting)}");
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

        internal static void ResultReferences()
        {
            XDocument rss1 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XElement[] items1 = rss1.Descendants("item").ToArray();
            XElement[] items2 = rss1.Element("rss").Element("channel").Elements("item").ToArray();
            object.ReferenceEquals(items1.First(), items2.First()).WriteLine(); // True
            items1.SequenceEqual(items2).WriteLine(); // True

            XDocument rss2 = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XElement[] items3 = rss2.Root.Descendants("item").ToArray();
            object.ReferenceEquals(items1.First(), items3.First()).WriteLine(); // False
            items1.SequenceEqual(items3).WriteLine(); // False
        }

        internal static void DocumentOrder()
        {
            XElement element1 = new XElement("element");
            XElement element2 = new XElement("element");
            new XDocument(new XElement("grandparent", new XElement("parent", element1, element2)));

            element1.IsBefore(element2).WriteLine(); // True
            XNode.DocumentOrderComparer.Compare(element1, element2).WriteLine(); // -1

            XElement[] ancestors = element1.Ancestors().ToArray();
            XNode.CompareDocumentOrder(ancestors.First(), ancestors.Last()).WriteLine(); // 1
            ancestors
                .InDocumentOrder()
                .Select(ancestor => ancestor.Name)
                .WriteLines(); // grandparent parent

            element1
                .AncestorsAndSelf()
                .Reverse()
                .SequenceEqual(element1.AncestorsAndSelf().InDocumentOrder())
                .WriteLine(); // True
        }

        internal static void CommonAncestor()
        {
            XElement root = XElement.Parse(@"
                <root>
                  <element value='4' />
                  <element value='2' />
                  <element value='3'><element value='1' /></element>
                </root>");
            XElement[] elements = root
                .Descendants("element")
                .OrderBy(element => (int)element.Attribute("value")).ToArray();
            elements.WriteLines(ancestorOrSelf => ancestorOrSelf.ToString(SaveOptions.DisableFormatting));
            // <element value="1" />
            // <element value="2" />
            // <element value="3"><element value="1" /></element>
            // <element value="4" />

            new XElement[] { elements.First(), elements.Last() }
                .InDocumentOrder()
                .WriteLines(ancestorOrSelf => ancestorOrSelf.ToString(SaveOptions.DisableFormatting));
            // <element value="4" />
            // <element value="1" />

            new XElement[] { elements.First(), elements.Last(), new XElement("element") }
                .InDocumentOrder()
                .ForEach();
            // InvalidOperationException: A common ancestor is missing.
        }

        internal static void XPathNavigator()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XPathNavigator rssNavigator = rss.CreateNavigator();
            rssNavigator.NodeType.WriteLine(); // Root
            rssNavigator.MoveToFirstChild().WriteLine(); // True
            rssNavigator.Name.WriteLine(); // rss

            ((XPathNodeIterator)rssNavigator
                .Evaluate("/rss/channel/item[guid/@isPermaLink='true']/category"))
                .Cast<XPathNavigator>()
                .Select(categoryNavigator => categoryNavigator.UnderlyingObject)
                .Cast<XElement>()
                .GroupBy(
                    category => category.Value, // Current text node's value.
                    category => category,
                    (key, group) => new { Name = key, Count = group.Count() },
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}")
                .WriteLines();
                // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4
        }

        internal static void XPathQuery()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            rss
                .XPathSelectElements("/rss/channel/item[guid/@isPermaLink='true']/category")
                .GroupBy(
                    category => category.Value, // Current text node's value.
                    category => category,
                    (key, group) => new { Name = key, Count = group.Count() },
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}")
                .WriteLines();
                // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4
        }

        internal static void XPathQueryWithNamespace()
        {
            XDocument rss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            XmlNamespaceManager namespaceManager = rss.CreateNamespaceManager();
            IEnumerable<XElement> query1 = rss.XPathSelectElements("/rss/channel/item/media:category", namespaceManager);
            query1.Count().WriteLine(); // 20

            IEnumerable<XElement> query2 = rss.XPathSelectElements("/rss/channel/item/media:category");
            // XPathException: Namespace Manager or XsltContext needed. This query has a prefix, variable, or user-defined function.
        }

        internal static void XPathEvaluateValue()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            double average1 = (double)rss.XPathEvaluate("count(/rss/channel/item/category) div count(/rss/channel/item)");
            average1.WriteLine(); // 4.65

            double average2 = rss
                .Element("rss")
                .Element("channel")
                .Elements("item")
                .Average(item => item.Elements("category").Count());
            average2.WriteLine(); // 4.65
        }

        internal static void XPathEvaluateSequence()
        {
            XDocument rss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            ((IEnumerable<object>)rss
                .XPathEvaluate("/rss/channel/item[guid/@isPermaLink='true']/category/text()"))
                .Cast<XText>()
                .GroupBy(
                    categoryTextNode => categoryTextNode.Value, // Current text node's value.
                    categoryTextNode => categoryTextNode,
                    (key, group) => new { Name = key, Count = group.Count() },
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(category => category.Count)
                .Take(5)
                .Select(category => $"[{category.Name}]:{category.Count}")
                .WriteLines();
                // [C#]:9 [LINQ]:6 [.NET]:5 [Functional Programming]:4 [LINQ via C#]:4
        }

        internal static void XPathEvaluateSequenceWithNamespace()
        {
            XDocument rss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            ((IEnumerable<object>)rss
                .XPathEvaluate(
                    "/rss/channel/item[contains(media:category/text(), 'microsoft')]/media:title/text()",
                    rss.CreateNamespaceManager()))
                .Cast<XText>()
                .WriteLines(mediaTitle => mediaTitle.Value);
                // Chinese President visits Microsoft
                // Satya Nadella, CEO of Microsoft
        }

        internal static void GenerateXPath()
        {
            XDocument aspNetRss = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            XElement element1 = aspNetRss
                .Root
                .Element("channel")
                .Elements("item")
                .Last();
            element1.XPath().WriteLine(); // /rss/channel/item[20]
            XElement element2 = aspNetRss.XPathSelectElement(element1.XPath());
            object.ReferenceEquals(element1, element2).WriteLine(); // True

            XDocument flickrRss = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2");
            XAttribute attribute1 = flickrRss
                .Root
                .Descendants("author") // <author flickr:profile="https://www.flickr.com/people/dixin/">...</author>.
                .First()
                .Attribute(XName.Get("profile", "urn:flickr:user")); // <rss xmlns:flickr="urn:flickr:user">...</rss>.
            attribute1.XPath().WriteLine(); // /rss/channel/item[1]/author/@flickr:profile
            XAttribute attribute2 = ((IEnumerable<object>)flickrRss
                .XPathEvaluate(attribute1.XPath(), flickrRss.CreateNamespaceManager()))
                .Cast<XAttribute>()
                .Single();
            object.ReferenceEquals(attribute1, attribute2).WriteLine(); // True
        }
    }
}

#if DEMO
namespace System.Xml.Linq
{
    using System.Collections.Generic;

    public abstract class XNode : XObject
    {
        public IEnumerable<XElement> Ancestors()
        {
            for (XElement parent = this.Parent; parent != null; parent = parent.Parent)
            {
                yield return parent;
            }
        }

        // Other members.
    }
}

namespace System.Xml.Linq
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
	{
        public static IEnumerable<XElement> Ancestors<T>(this IEnumerable<T> source) where T : XNode =>
            source
                .Where(node => node != null)
                .SelectMany(node => node.Ancestors())
                .Where(ancestor => ancestor != null);
            // Equivalent to:
            // from node in source
            // where node != null
            // from ancestor in node.Ancestors()
            // where ancestor != null
            // select ancestor;

        // Other members.
    }

    public static partial class Extensions
	{
        public static IEnumerable<T> InDocumentOrder<T>(this IEnumerable<T> source) where T : XNode =>
            source.OrderBy(node => node, XNode.DocumentOrderComparer);
    }
}
#endif
