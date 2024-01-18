namespace Examples.Net;

using System.Collections.Specialized;
using System.Web;
using Examples.Common;

public static class UriExtensions
{
    public static Uri RemoveQuery(this Uri uri, string query, int port = -1)
    {
        NameValueCollection queries = HttpUtility.ParseQueryString(uri.NotNull().Query);
        queries.Remove(query);

        return new UriBuilder(uri) { Port = port, Query = queries.ToString() }.Uri;
    }
}
