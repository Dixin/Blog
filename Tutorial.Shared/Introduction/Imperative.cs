namespace Tutorial.Introduction
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml.XPath;

    internal static partial class Imperative
    {
        internal static void Sql(string connectionString)
        {
            using (DbConnection connection = new SqlConnection(connectionString))
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT [Product].[Name]
                    FROM [Production].[Product] AS [Product]
                    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Subcategory] 
                        ON [Subcategory].[ProductSubcategoryID] = [Product].[ProductSubcategoryID]
                    LEFT OUTER JOIN [Production].[ProductCategory] AS [Category] 
                        ON [Category].[ProductCategoryID] = [Subcategory].[ProductCategoryID]
                    WHERE [Category].[Name] = @categoryName
                    ORDER BY [Product].[ListPrice] DESC";
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@categoryName";
                parameter.Value = "Bikes";
                command.Parameters.Add(parameter);
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string productName = (string)reader["Name"];
                        Trace.WriteLine(productName);
                    }
                }
            }
        }
    }

    internal static partial class Imperative
    {
        internal static void Xml()
        {
            XPathDocument feed = new XPathDocument("https://weblogs.asp.net/dixin/rss");
            XPathNavigator navigator = feed.CreateNavigator();
            XPathExpression selectExpression = navigator.Compile("//item[guid/@isPermaLink='true']/title/text()");
            XPathExpression sortExpression = navigator.Compile("../../pubDate/text()");
            selectExpression.AddSort(sortExpression, Comparer<DateTime>.Default);
            XPathNodeIterator nodes = navigator.Select(selectExpression);
            foreach (object node in nodes)
            {
                Trace.WriteLine(node);
            }
        }
    }

    internal static partial class Imperative
    {
        internal static void DelegateTypes()
        {
            Assembly coreLibrary = typeof(object).Assembly;
            Dictionary<string, List<Type>> delegateTypes = new Dictionary<string, List<Type>>();
            foreach (Type type in coreLibrary.GetExportedTypes())
            {
                if (type.BaseType == typeof(MulticastDelegate))
                {
                    if (!delegateTypes.TryGetValue(type.Namespace, out List<Type> namespaceTypes))
                    {
                        namespaceTypes = delegateTypes[type.Namespace] = new List<Type>();
                    }
                    namespaceTypes.Add(type);
                }
            }
            List<KeyValuePair<string, List<Type>>> delegateTypesList =
                new List<KeyValuePair<string, List<Type>>>(delegateTypes);
            for (int index = 0; index < delegateTypesList.Count - 1; index++)
            {
                int currentIndex = index;
                KeyValuePair<string, List<Type>> after = delegateTypesList[index + 1];
                while (currentIndex >= 0)
                {
                    KeyValuePair<string, List<Type>> before = delegateTypesList[currentIndex];
                    int compare = before.Value.Count.CompareTo(after.Value.Count);
                    if (compare == 0)
                    {
                        compare = string.Compare(after.Key, before.Key, StringComparison.Ordinal);
                    }
                    if (compare >= 0)
                    {
                        break;
                    }
                    delegateTypesList[currentIndex + 1] = delegateTypesList[currentIndex];
                    currentIndex--;
                }
                delegateTypesList[currentIndex + 1] = after;
            }
            foreach (KeyValuePair<string, List<Type>> namespaceTypes in delegateTypesList) // Output.
            {
                Trace.Write(namespaceTypes.Value.Count + " " + namespaceTypes.Key + ":");
                foreach (Type delegateType in namespaceTypes.Value)
                {
                    Trace.Write(" " + delegateType.Name);
                }
                Trace.WriteLine(null);
            }
            // 30 System: Action`1 Action Action`2 Action`3 Action`4 Func`1 Func`2 Func`3 Func`4 Func`5 Action`5 Action`6 Action`7 Action`8 Func`6 Func`7 Func`8 Func`9 Comparison`1 Converter`2 Predicate`1 ResolveEventHandler AssemblyLoadEventHandler AppDomainInitializer CrossAppDomainDelegate AsyncCallback ConsoleCancelEventHandler EventHandler EventHandler`1 UnhandledExceptionEventHandler
            // 8 System.Threading: SendOrPostCallback ContextCallback ParameterizedThreadStart WaitCallback WaitOrTimerCallback IOCompletionCallback ThreadStart TimerCallback
            // 3 System.Reflection: ModuleResolveEventHandler MemberFilter TypeFilter
            // 3 System.Runtime.CompilerServices: TryCode CleanupCode CreateValueCallback
            // 2 System.Runtime.Remoting.Messaging: MessageSurrogateFilter HeaderHandler
            // 1 System.Runtime.InteropServices: ObjectCreationDelegate
            // 1 System.Runtime.Remoting.Contexts: CrossContextDelegate
        }
    }

    internal class WebClient
    {
        internal FileInfo Download(Uri uri)
        {
            return default;
        }
    }

    internal class DocumentConverter
    {
        internal DocumentConverter(FileInfo template)
        {
            this.Template = template;
        }

        internal FileInfo Template { get; private set; }

        internal FileInfo ToWord(FileInfo htmlDocument)
        {
            return default;
        }
    }

    internal class OneDriveClient
    {
        internal void Upload(FileInfo file) { }
    }

    internal class DocumentBuilder
    {
        private readonly WebClient webClient;

        private readonly DocumentConverter documentConverter;

        private readonly OneDriveClient oneDriveClient;

        internal DocumentBuilder(
            WebClient webClient, DocumentConverter documentConverter, OneDriveClient oneDriveClient)
        {
            this.webClient = webClient;
            this.documentConverter = documentConverter;
            this.oneDriveClient = oneDriveClient;
        }

        internal void Build(Uri uri)
        {
            FileInfo htmlDocument = this.webClient.Download(uri);
            FileInfo wordDocument = this.documentConverter.ToWord(htmlDocument);
            this.oneDriveClient.Upload(wordDocument);
        }
    }

    internal partial class Imperative
    {
        internal static void BuildDocument(Uri uri, FileInfo template)
        {
            DocumentBuilder builder = new DocumentBuilder(
                new WebClient(), new DocumentConverter(template), new OneDriveClient());
            builder.Build(uri);
        }
    }
}
