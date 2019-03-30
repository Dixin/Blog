namespace Tutorial.LinqToXml
{
    using System;
    using System.Text;
    using System.Xml;

    internal static class Dom
    {
        internal static void CreateAndSerialize()
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            const string NamespacePrefix = "dixin";
            namespaceManager.AddNamespace(NamespacePrefix, "https://weblogs.asp.net/dixin");

            XmlDocument document = new XmlDocument(namespaceManager.NameTable);

            XmlElement rss = document.CreateElement("rss");
            rss.SetAttribute("version", "2.0");
            XmlAttribute attribute = document.CreateAttribute(
                "xmlns", NamespacePrefix, namespaceManager.LookupNamespace("xmlns"));
            attribute.Value = namespaceManager.LookupNamespace(NamespacePrefix);
            rss.SetAttributeNode(attribute);
            document.AppendChild(rss);

            XmlElement channel = document.CreateElement("channel");
            rss.AppendChild(channel);

            XmlElement item = document.CreateElement("item");
            channel.AppendChild(item);

            XmlElement title = document.CreateElement("title");
            title.InnerText = "LINQ via C#";
            item.AppendChild(title);

            XmlElement link = document.CreateElement("link");
            link.InnerText = "https://weblogs.asp.net/dixin/linq-via-csharp";
            item.AppendChild(link);

            XmlElement description = document.CreateElement("description");
            description.InnerXml = "<p>This is a tutorial of LINQ and functional programming. Hope it helps.</p>";
            item.AppendChild(description);

            XmlElement pubDate = document.CreateElement("pubDate");
            pubDate.InnerText = new DateTime(2009, 9, 7).ToString("r");
            item.AppendChild(pubDate);

            XmlElement guid = document.CreateElement("guid");
            guid.InnerText = "https://weblogs.asp.net/dixin/linq-via-csharp";
            guid.SetAttribute("isPermaLink", "true");
            item.AppendChild(guid);

            XmlElement category1 = document.CreateElement("category");
            category1.InnerText = "C#";
            item.AppendChild(category1);

            XmlNode category2 = category1.CloneNode(false);
            category2.InnerText = "LINQ";
            item.AppendChild(category2);

            XmlComment comment = document.CreateComment("Comment.");
            item.AppendChild(comment);

            XmlElement source = document.CreateElement(NamespacePrefix, "source", namespaceManager.LookupNamespace(NamespacePrefix));
            source.InnerText = "https://github.com/Dixin/CodeSnippets/tree/master/Dixin/Linq";
            item.AppendChild(source);

            // Serialize XmlDocument to string.
            StringBuilder xmlString = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(xmlString, settings))
            {
                document.Save(writer);
            }

            // rssItem.ToString() returns "System.Xml.XmlElement".
            // rssItem.OuterXml returns a single line of XML text.
            xmlString.WriteLine();
        }
    }
}