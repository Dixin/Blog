namespace Dixin.Linq.LinqToXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using Dixin.Common;

    public static partial class XExtensions
    {
        public static IEnumerable<XObject> Objects(this XObject @object)
        {
            @object.NotNull(nameof(@object));

            return Enumerable
                .Repeat(@object, 1)
                .Concat(
                    (@object as XElement)?.Attributes() // T is covariant in IEnumerable<T>.
                    ?? Enumerable.Empty<XObject>())
                .Concat(
                    (@object as XContainer)?
                        .DescendantNodes()
                        .SelectMany(descendant => Enumerable
                            .Repeat<XObject>(descendant, 1)
                            .Concat(
                                (descendant as XElement)?.Attributes() // T is covariant in IEnumerable<T>.
                                ?? Enumerable.Empty<XObject>()))
                    ?? Enumerable.Empty<XObject>());
        }
    }

    public static partial class XExtensions
    {
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

        public static IEnumerable<XAttribute> AllAttributes(this XContainer container)
        {
            container.NotNull(nameof(container));

            return ((container as XElement)?.DescendantsAndSelf() ?? container.Descendants())
                .SelectMany(element => element.Attributes());
        }

        public static IEnumerable<Tuple<string, XNamespace>> Namespaces(this XContainer container)
        {
            container.NotNull(nameof(container));

            // Namespaces are defined as xmlns:prefix="namespace" attributes.
            return container
                .AllAttributes()
                .Where(attribute => attribute.IsNamespaceDeclaration)
                .Select(attribute => Tuple.Create(attribute.Name.LocalName, (XNamespace)attribute.Value));
        }

        public static XmlNamespaceManager CreateNamespaceManager(this XContainer container)
        {
            container.NotNull(nameof(container));

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            container
                .Namespaces()
                .ForEach(@namespace => namespaceManager.AddNamespace(@namespace.Item1, @namespace.Item2.ToString()));
            return namespaceManager;
        }

        public static string XPath(string axis, string test = null, string predicate = null)
        {
            return $"{axis}{(string.Equals(axis, "/", StringComparison.Ordinal) || string.IsNullOrEmpty(test) ? null : "/")}{test}{predicate}";
        }

        public static string XPath(this XElement element, string parentXPath = null)
        {
            element.NotNull(nameof(element));

            if (string.IsNullOrEmpty(parentXPath) && element.Parent == null && element.Document == null)
            {
                return "/";
            }

            string parentAxis = parentXPath ?? element.Parent?.XPath();
            string prefix = element.Name.Namespace == XNamespace.None
                ? null
                : element.GetPrefixOfNamespace(element.Name.Namespace); // Return null if not found.
            string selfAxis = string.IsNullOrEmpty(prefix)
                ? element.Name.ToString()
                : $"{prefix}:{element.Name.LocalName}";
            int index = element.ElementsBeforeSelf(element.Name).Count();
            string predicate = index == 0 && !element.ElementsAfterSelf(element.Name).Any() ? null : $"[{index + 1}]";
            return XPath(parentAxis, selfAxis, predicate);
        }

        public static string XPath(this XAttribute attribute, string parentXPath = null)
        {
            attribute.NotNull(nameof(attribute));

            XElement element = attribute.Parent;
            string axis = parentXPath ?? element?.XPath();
            string prefix = attribute.Name.Namespace == XNamespace.None
                ? null
                : element?.GetPrefixOfNamespace(attribute.Name.Namespace); // Return null if not found.
            string test = string.IsNullOrEmpty(prefix)
                ? attribute.Name.ToString()
                : $"{prefix}:{attribute.Name.LocalName}";
            return XPath(axis, $"@{test}");
        }

        public static string XPath(this XComment comment, string parentXPath = null)
        {
            comment.NotNull(nameof(comment));

            string axis = parentXPath ?? comment.Parent?.XPath();
            string test = "comment()";
            int index = comment
                .NodesBeforeSelf()
                .Count(before => before.NodeType == XmlNodeType.Comment);
            string predicate = index == 0
                && comment
                    .NodesAfterSelf()
                    .All(after => after.NodeType != XmlNodeType.Comment)
                ? null
                : $"[{index + 1}]";
            return XPath(axis, test, predicate);
        }

        public static string XPath(this XText text, string parentXPath = null)
        {
            text.NotNull(nameof(text));

            string axis = parentXPath ?? text.Parent?.XPath();
            string test = "text()";
            int index = text
                .NodesBeforeSelf()
                .Count(before => before.NodeType == XmlNodeType.Text || before.NodeType == XmlNodeType.CDATA);
            string predicate = index == 0
                && text
                    .NodesAfterSelf()
                    .All(after => after.NodeType != XmlNodeType.Comment && after.NodeType != XmlNodeType.CDATA)
                ? null
                : $"[{index + 1}]";
            return XPath(axis, test, predicate);
        }

        public static string XPath(this XProcessingInstruction instruction, string parentXPath = null)
        {
            instruction.NotNull(nameof(instruction));

            string axis = parentXPath ?? instruction.Parent?.XPath();
            string test = $"processing-instruction('{instruction.Target}')";
            int index = instruction
                .NodesBeforeSelf()
                .OfType<XProcessingInstruction>()
                .Count(before => string.Equals(before.Target, instruction.Target, StringComparison.Ordinal));
            string predicate = index == 0
                && !instruction
                    .NodesAfterSelf()
                    .OfType<XProcessingInstruction>()
                    .Any(after => string.Equals(after.Target, instruction.Target, StringComparison.Ordinal))
                ? null
                : $"[{index + 1}]";
            return XPath(axis, test, predicate);
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

        public static IEnumerable<Tuple<XObject, string, XmlSchemaValidity?>> GetValidities(this XElement element, string parentXPath = null)
        {
            element.NotNull(nameof(element));

            string elementXPth = element.XPath(parentXPath);
            return Enumerable.Repeat(Tuple.Create((XObject)element, elementXPth, element.GetSchemaInfo()?.Validity), 1).Concat(element.Attributes().Select(attribute => Tuple.Create((XObject)attribute, attribute.XPath(elementXPth), attribute.GetSchemaInfo()?.Validity))).Concat(element.Elements().SelectMany(child => child.GetValidities(elementXPth)));
        }

        public static IEnumerable<Tuple<XObject, string, XmlSchemaValidity?>> GetValidities(this XDocument document)
        {
            document.NotNull(nameof(document));

            return document.Root.GetValidities();
        }
    }
}
