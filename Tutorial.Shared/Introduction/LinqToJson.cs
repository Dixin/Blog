namespace Tutorial.Introduction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    internal static partial class LinqToJson
    {
        internal static async Task QueryExpression(string apiKey)
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
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToJson
    {
        internal static async Task QueryMethods(string apiKey)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string feedUri = $"https://api.tumblr.com/v2/blog/dixinyan.tumblr.com/posts/photo?api_key={apiKey}";
                JObject feed = JObject.Parse((await httpClient.GetStringAsync(feedUri)));
                IEnumerable<JToken> source = feed["response"]["posts"]; // Get source.
                IEnumerable<string> query = source
                    .Where(post => post["tags"].Any(tag => 
                        "Microsoft".Equals((string)tag, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(post => (DateTime)post["date"])
                    .Select(post => (string)post["summary"]); // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
}
