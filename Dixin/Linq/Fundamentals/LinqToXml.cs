namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    internal static partial class LinqToXml
    {
        internal static void QueryExpression()
        {
            XDocument feed = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<XElement> source = feed.Descendants("item"); // Get source.
            IEnumerable<string> query = from item in source
                where (bool)item.Element("guid").Attribute("isPermaLink")
                orderby (DateTime)item.Element("pubDate")
                select (string)item.Element("title"); // Define query.
            foreach (string result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }
    }

    internal static partial class LinqToXml
    {
        internal static void QueryMethods()
        {
            XDocument feed = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<XElement> source = feed.Descendants("item"); // Get source.
            IEnumerable<string> query = source
                .Where(item => (bool)item.Element("guid").Attribute("isPermaLink"))
                .OrderBy(item => (DateTime)item.Element("pubDate"))
                .Select(item => (string)item.Element("title")); // Define query.
            foreach (string result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }
    }

    internal static partial class LinqToXml
    {
        internal static IEnumerable<string> Titles(string rss, params string[] categories)
        {
            return from item in XDocument.Load(rss).Root.Element("channel").Elements("item")
                   where !categories.Any()
                       || item.Elements("category").Any(category => categories.Contains(category.Value))
                   orderby DateTime.Parse(item.Element("pubDate").Value, CultureInfo.InvariantCulture)
                   select item.Element("title").Value;
        }
    }
}
