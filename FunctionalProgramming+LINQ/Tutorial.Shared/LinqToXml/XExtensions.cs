namespace Tutorial.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

    public static partial class XExtensions
    {
        public static IEnumerable<XObject> DescendantObjects(this XObject source) =>
            Enumerable
                .Empty<XObject>()
                .Concat(
                    source is XElement element
                        ? element.Attributes() // T is covariant in IEnumerable<T>.
                        : Enumerable.Empty<XObject>())
                .Concat(
                    source is XContainer container
                        ? container
                            .DescendantNodes()
                            .SelectMany(descendant => EnumerableEx
                                .Return(descendant)
                                .Concat(
                                    descendant is XElement descendantElement
                                        ? descendantElement.Attributes() // T is covariant in IEnumerable<T>.
                                        : Enumerable.Empty<XObject>()))
                        : Enumerable.Empty<XObject>());
    }

    public static partial class XExtensions
    {
        public static IEnumerable<XObject> SelfAndDescendantObjects(this XObject source) =>
            EnumerableEx
                .Return(source)
                .Concat(source.DescendantObjects());

        public static IEnumerable<XName> Names(this XContainer source) =>
            (source is XElement element
                ? element.DescendantsAndSelf()
                : source.Descendants())
                    .SelectMany(descendantElement => EnumerableEx
                        .Return(descendantElement.Name)
                        .Concat(descendantElement
                            .Attributes()
                            .Select(attribute => attribute.Name)))
                .Distinct();

        public static IEnumerable<XAttribute> AllAttributes(this XContainer source) =>
            (source is XElement element 
                ? element.DescendantsAndSelf() 
                : source.Descendants())
                .SelectMany(elementOrDescendant => elementOrDescendant.Attributes());

        public static IEnumerable<(string, XNamespace)> Namespaces(this XContainer source) =>
            source // Namespaces are defined as xmlns:prefix="namespace" attributes.
                .AllAttributes()
                .Where(attribute => attribute.IsNamespaceDeclaration)
                .Select(attribute => (attribute.Name.LocalName, (XNamespace)attribute.Value));

        public static XmlNamespaceManager CreateNamespaceManager(this XContainer source)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            source
                .Namespaces()
                .ForEach(@namespace => namespaceManager.AddNamespace(@namespace.Item1, @namespace.Item2.ToString()));
            return namespaceManager;
        }

        public static string XPath(this XName source, XElement container)
        {
            string prefix = source.Namespace == XNamespace.None
                ? null
                : container.GetPrefixOfNamespace(source.Namespace); // GetPrefixOfNamespace returns null if not found.
            return string.IsNullOrEmpty(prefix) ? source.ToString() : $"{prefix}:{source.LocalName}";
        }

        private static string CombineXPath(string xPath1, string xPath2, string predicate = null) =>
            string.Equals(xPath1, "/", StringComparison.Ordinal) || string.IsNullOrEmpty(xPath2)
                ? $"{xPath1}{xPath2}{predicate}"
                : $"{xPath1}/{xPath2}{predicate}";

        private static string XPath<TSource>(
            this TSource source,
            string parentXPath,
            string selfXPath = null,
            Func<TSource, bool> siblingPredicate = null) where TSource : XNode
        {
            int index = source
                .NodesBeforeSelf()
                .OfType<TSource>()
                .Where(siblingPredicate ?? (_ => true))
                .Count();
            string predicate = index == 0
                && !source
                    .NodesAfterSelf()
                    .OfType<TSource>()
                    .Where(siblingPredicate ?? (_ => true))
                    .Any()
                ? null
                : $"[{index + 1}]";

            return CombineXPath(parentXPath, selfXPath, predicate);
        }

        public static string XPath(this XElement source, string parentXPath = null) =>
            string.IsNullOrEmpty(parentXPath) && source.Parent == null && source.Document == null
                ? "/" // source is an element on the fly, not attached to any parent node.
                : source.XPath(
                    parentXPath ?? source.Parent?.XPath(),
                    source.Name.XPath(source),
                    sibling => sibling.Name == source.Name);

        public static string XPath(this XComment source, string parentXPath = null) =>
            source.XPath(parentXPath ?? source.Parent?.XPath(), "comment()");

        public static string XPath(this XText source, string parentXPath = null) =>
            source.XPath(parentXPath ?? source.Parent?.XPath(), "text()");

        public static string XPath(this XProcessingInstruction source, string parentXPath = null) =>
            source.XPath(
                parentXPath ?? source.Parent?.XPath(),
                $"processing-instruction('{source.Target}')",
                sibling => string.Equals(sibling.Target, source.Target, StringComparison.Ordinal));

        public static string XPath(this XAttribute source, string parentXPath = null) =>
            CombineXPath(parentXPath ?? source.Parent?.XPath(), $"@{source.Name.XPath(source.Parent)}");

        public static XmlSchemaSet InferSchema(this XNode source)
        {
            XmlSchemaInference schemaInference = new XmlSchemaInference();
            using (XmlReader reader = source.CreateReader())
            {
                return schemaInference.InferSchema(reader);
            }
        }

        public static XDocument ToXDocument(this XmlSchema source)
        {
            XDocument document = new XDocument();
            using (XmlWriter writer = document.CreateWriter())
            {
                source.Write(writer);
            }
            return document;
        }

        public static IEnumerable<(XObject, string, IXmlSchemaInfo)> GetValidities(this XElement source, string parentXPath = null)
        {
            string xPath = source.XPath(parentXPath);
            return EnumerableEx
                .Return(((XObject)source, xPath, source.GetSchemaInfo()))
                .Concat(source
                    .Attributes()
                    .Select(attribute => ((XObject)attribute, attribute.XPath(xPath), attribute.GetSchemaInfo())))
                .Concat(source
                    .Elements()
                    .SelectMany(child => child.GetValidities(xPath)));
        }

        public static XDocument XslTransform(this XNode source, XNode xsl)
        {
            XDocument result = new XDocument();
            using (XmlReader sourceReader = source.CreateReader())
            using (XmlReader xslReader = xsl.CreateReader())
            using (XmlWriter resultWriter = result.CreateWriter())
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xslReader);
                transform.Transform(sourceReader, resultWriter);
                return result;
            }
        }
    }
}
