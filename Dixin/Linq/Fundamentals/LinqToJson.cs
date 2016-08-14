namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;

    using Newtonsoft.Json.Linq;

    internal static partial class LinqToJson
    {
        internal static void QueryExpression()
        {
            using (WebClient webClient = new WebClient())
            {
                const string feedUrl = "https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&lang=en-us&format=json&jsoncallback=?";
                JObject feed = JObject.Parse(webClient.DownloadString(feedUrl).TrimStart('(').TrimEnd(')'));
                IEnumerable<JToken> source = feed["items"]; // Get source.
                IEnumerable<string> query = from item in source
                                            where ((string)item["tags"]).Contains("microsoft")
                                            orderby (DateTime)item["published"]
                                            select (string)item["title"]; // Create query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToJson
    {
        internal static void QueryMethods()
        {
            using (WebClient webClient = new WebClient())
            {
                const string feedUrl = "https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&lang=en-us&format=json&jsoncallback=?";
                JObject feed = JObject.Parse(webClient.DownloadString(feedUrl).TrimStart('(').TrimEnd(')'));
                IEnumerable<JToken> source = feed["items"]; // Get source.
                IEnumerable<string> query = source
                    .Where(item => ((string)item["tags"]).Contains("microsoft"))
                    .OrderBy(item => (DateTime)item["published"])
                    .Select(item => (string)item["title"]); // Create query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
}
