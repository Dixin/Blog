namespace Tutorial.Introduction
{
#if NETFX
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Newtonsoft.Json.Linq;

    using Tutorial.LinqToEntities;

    using global::LinqToTwitter;
#else
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Newtonsoft.Json.Linq;

    using Tutorial.LinqToEntities;

    using global::LinqToTwitter;
#endif

    internal static partial class Linq
    {
        internal static void LinqToObjectsQueryExpression()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 }; // Get source.
            IEnumerable<double> query =
                from int32 in source
                where int32 > 0
                orderby int32
                select Math.Sqrt(int32); // Define query.
            foreach (double result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }
    }

    internal static partial class Linq
    {
        internal static void LinqToObjectsQueryMethods()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 }; // Get source.
            IEnumerable<double> query = source
                .Where(int32 => int32 > 0)
                .OrderBy(int32 => int32)
                .Select(int32 => Math.Sqrt(int32)); // Define query.
            foreach (double result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }

        internal static void ParallelLinq()
        {
            int[] values = { 4, 3, 2, 1, 0, -1 };
            ParallelQuery<int> source = values.AsParallel(); // Get source.
            ParallelQuery<double> query =
                from int32 in source
                where int32 > 0
                orderby int32
                select Math.Sqrt(int32); // Define query.
            // Equivalent to:
            // ParallelQuery<double> query = source
            //    .Where(int32 => int32 > 0)
            //    .OrderBy(int32 => int32)
            //    .Select(int32 => Math.Sqrt(int32));
            query.ForAll(result => Trace.WriteLine(result)); // Execute query.
        }

        internal static void LinqToXml()
        {
            XDocument feed = XDocument.Load("https://weblogs.asp.net/dixin/rss");
            IEnumerable<XElement> source = feed.Descendants("item"); // Get source.
            IEnumerable<string> query =
                from item in source
                where (bool)item.Element("guid").Attribute("isPermaLink")
                orderby (DateTime)item.Element("pubDate")
                select (string)item.Element("title"); // Define query.
            // Equivalent to:
            // IEnumerable<string> query = source
            //    .Where(item => (bool)item.Element("guid").Attribute("isPermaLink"))
            //    .OrderBy(item => (DateTime)item.Element("pubDate"))
            //    .Select(item => (string)item.Element("title"));
            foreach (string result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }

#if NETFX
        internal static void LinqToDataSets(string connectionString)
        {
            using (DataSet dataSet = new DataSet())
            using (DataAdapter dataAdapter = new SqlDataAdapter(
                @"SELECT [Name], [ListPrice], [ProductSubcategoryID] FROM [Production].[Product]", connectionString))
            {
                dataAdapter.Fill(dataSet);
                EnumerableRowCollection<DataRow> source = dataSet.Tables[0].AsEnumerable(); // Get source.
                EnumerableRowCollection<string> query =
                    from product in source
                    where product.Field<int>("ProductSubcategoryID") == 1
                    orderby product.Field<decimal>("ListPrice")
                    select product.Field<string>("Name"); // Define query.
                // Equivalent to:
                // EnumerableRowCollection<string> query = source
                //    .Where(product => product.Field<int>("ProductSubcategoryID") == 1)
                //    .OrderBy(product => product.Field<decimal>("ListPrice"))
                //    .Select(product => product.Field<string>("Name"));
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
#endif

        internal static void LinqToEntities()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> source = adventureWorks.Products; // Get source.
                IQueryable<string> query =
                    from product in source
                    where product.ProductSubcategory.ProductCategory.Name == "Bikes"
                    orderby product.ListPrice
                    select product.Name; // Define query.
                // Equivalent to:
                // IQueryable<string> query = source
                //    .Where(product => product.ProductSubcategory.ProductCategory.Name == "Bikes")
                //    .OrderBy(product => product.ListPrice)
                //    .Select(product => product.Name);
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }

        internal static async Task LinqToJson(string apiKey)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string feedUri = $"https://api.tumblr.com/v2/blog/dixinyan.tumblr.com/posts/photo?api_key={apiKey}";
                JObject feed = JObject.Parse((await httpClient.GetStringAsync(feedUri)));
                IEnumerable<JToken> source = feed["response"]["posts"]; // Get source.
                IEnumerable<string> query =
                    from post in source
                    where post["tags"].Any(tag => "Microsoft".Equals((string)tag, StringComparison.OrdinalIgnoreCase))
                    orderby (DateTime)post["date"]
                    select (string)post["summary"]; // Define query.
                // Equivalent to:
                // IEnumerable<string> query = source
                //    .Where(post => post["tags"].Any(tag =>
                //        "Microsoft".Equals((string)tag, StringComparison.OrdinalIgnoreCase)))
                //    .OrderBy(post => (DateTime)post["date"])
                //    .Select(post => (string)post["summary"]);
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }

        internal static void LinqToTwitter(
            string consumerKey, string consumerSecret, string oAuthToken, string oAuthTokenSecret)
        {
            SingleUserAuthorizer credentials = new SingleUserAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    OAuthToken = oAuthToken,
                    OAuthTokenSecret = oAuthTokenSecret
                }
            };
            using (TwitterContext twitter = new TwitterContext(credentials))
            {
                IQueryable<Search> source = twitter.Search; // Get source.
                IQueryable<List<Status>> query =
                    from search in source
                    where search.Type == SearchType.Search && search.Query == "LINQ"
                    orderby search.SearchMetaData.Count
                    select search.Statuses; // Define query.
                // Equivalent to:
                // IQueryable<List<Status>> query = source
                //    .Where(search => search.Type == SearchType.Search && search.Query == "LINQ")
                //    .OrderBy(search => search.SearchMetaData.Count)
                //    .Select(search => search.Statuses);
                foreach (List<Status> search in query) // Execute query.
                {
                    foreach (Status status in search)
                    {
                        Trace.WriteLine(status.Text);
                    }
                }
            }
        }
    }
}
