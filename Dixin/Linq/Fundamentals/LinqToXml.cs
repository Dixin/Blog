namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    public static partial class LinqToXml
    {
        public static IEnumerable<string> Titles(string rss, params string[] categories)
        {
            return from item in XDocument.Load(rss).Root.Element("channel").Elements("item")
                   where !categories.Any()
                       || item.Elements("category").Any(category => categories.Contains(category.Value))
                   orderby DateTime.Parse(item.Element("pubDate").Value, CultureInfo.InvariantCulture)
                   select item.Element("title").Value;
        }
    }
}
