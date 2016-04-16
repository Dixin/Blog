namespace Dixin.Linq.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using Dixin.Common;

    public static class XExtensions
    {
        public static IEnumerable<XObject> Objects(this XObject @object)
        {
            @object.NotNull(nameof(@object));

            return Enumerable
                .Repeat(@object, 1)
                .Concat((@object as XElement)?.Attributes() // T is covariant in IEnumerable<T>.
                    ?? Enumerable.Empty<XObject>())
                .Concat((@object as XContainer)?
                    .DescendantNodes()
                    .SelectMany(descendant => Enumerable
                        .Repeat<XObject>(descendant, 1)
                        .Concat((descendant as XElement)?.Attributes() // T is covariant in IEnumerable<T>.
                            ?? Enumerable.Empty<XObject>()))
                     ?? Enumerable.Empty<XObject>());
        }

        public static IEnumerable<XName> Names(this XContainer container)
        {
            container.NotNull(nameof(container));

            return ((container as XElement)?.DescendantsAndSelf() ?? container.Descendants())
                .SelectMany(element => Enumerable
                    .Repeat(element.Name, 1)
                    .Concat(element
                        .Attributes()
                        .Select(attribute => attribute.Name)))
                .Distinct();
        }

        public static IEnumerable<XAttribute> Attributes(this XContainer container)
        {
            container.NotNull(nameof(container));

            return ((container as XElement)?.DescendantsAndSelf() ?? container.Descendants())
                .SelectMany(element => element.Attributes());
        }

        public static IEnumerable<Tuple<string, XNamespace>> Namespaces(this XContainer container)
        {
            container.NotNull(nameof(container));

            // Namespaces are defined as xmlns:prefix="namepsace" attributes.
            return container.Attributes()
                .Where(attribute => attribute.IsNamespaceDeclaration)
                .Select(attribute => Tuple.Create(attribute.Name.LocalName, (XNamespace)attribute.Value));
        }

        public static XmlNamespaceManager GetNamespaceManager(this XContainer container)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            container.Namespaces().ForEach(@namespace => namespaceManager.AddNamespace(@namespace.Item1, @namespace.Item2.ToString()));
            return namespaceManager;
        }

        public static string GetXPath(
            this XElement element, string parentXPath = null, XmlNamespaceManager namespaceManager = null)
        {
            element.NotNull(nameof(element));

            int index = element.ElementsBeforeSelf(element.Name).Count();
            string position = index == 0 && !element.ElementsAfterSelf(element.Name).Any() ? null : $"[{index + 1}]";
            string elementName = namespaceManager != null && element.Name.Namespace != XNamespace.None
                ? $"{namespaceManager.LookupPrefix(element.Name.Namespace.ToString())}:{element.Name.LocalName}"
                : element.Name.ToString();
            return $"{parentXPath ?? element.Parent?.GetXPath()}/{elementName}{position}";
        }

        public static string GetXPath(
            this XAttribute attribute, 
            XElement element, 
            string parentXPath = null, 
            XmlNamespaceManager namespaceManager = null)
        {
            attribute.NotNull(nameof(attribute));
            element.NotNull(nameof(element));

            string attributeName = namespaceManager != null && attribute.Name.Namespace != XNamespace.None
                ? $"{namespaceManager.LookupPrefix(attribute.Name.Namespace.ToString())}:{attribute.Name.LocalName}"
                : attribute.Name.ToString();
            return $"{element.GetXPath(parentXPath, namespaceManager)}/@{attributeName}";
        }

        public static XmlSchemaSet InferSchema(this XNode node)
        {
            node.NotNull(nameof(node));

            XmlSchemaInference schemaInference = new XmlSchemaInference();
            using (XmlReader reader = node.CreateReader())
            {
                return schemaInference.InferSchema(reader);
            }
        }

        public static XDocument ToXDocument(this XmlSchema schema)
        {
            schema.NotNull(nameof(schema));

            XDocument document = new XDocument();
            using (XmlWriter writer = document.CreateWriter())
            {
                schema.Write(writer);
            }
            return document;
        }

        public static IEnumerable<Tuple<XObject, string, XmlSchemaValidity?>> GetValidities(
            this XElement element, string parentXPath = null, XmlNamespaceManager namespaceManager = null)
        {
            element.NotNull(nameof(element));

            string elementXPth = element.GetXPath(parentXPath, namespaceManager);
            return Enumerable
                .Repeat(Tuple.Create((XObject)element, elementXPth, element.GetSchemaInfo()?.Validity), 1)
                .Concat(element
                    .Attributes()
                    .Select(attribute => Tuple.Create(
                        (XObject)attribute, attribute.GetXPath(element, parentXPath, namespaceManager), attribute.GetSchemaInfo()?.Validity)))
                .Concat(element
                    .Elements()
                    .SelectMany(child => child.GetValidities(elementXPth, namespaceManager)));
        }

        public static IEnumerable<Tuple<XObject, string, XmlSchemaValidity?>> GetValidities(
            this XDocument document, XmlNamespaceManager namespaceManager = null)
        {
            document.NotNull(nameof(document));

            return document.Root.GetValidities(null, namespaceManager ?? document.GetNamespaceManager());
        }
    }
}
