namespace Tutorial.Introduction
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using global::LinqToTwitter;

    internal static partial class LinqToTwitter
    {
        internal static void QueryExpression()
        {
            SingleUserAuthorizer credentials = new SingleUserAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = "ConsumerKey",
                    ConsumerSecret = "ConsumerSecret",
                    OAuthToken = "OAuthToken",
                    OAuthTokenSecret = "OAuthTokenSecret"
                }
            };
            using (TwitterContext twitter = new TwitterContext(credentials))
            {
                IQueryable<Search> source = twitter.Search; // Get source.
                IQueryable<List<Status>> query = from search in source
                                                 where search.Type == SearchType.Search && search.Query == "LINQ"
                                                 orderby search.SearchMetaData.Count
                                                 select search.Statuses; // Define query.
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

    internal static partial class LinqToTwitter
    {
        internal static void QueryMethods()
        {
            SingleUserAuthorizer credentials = new SingleUserAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = "ConsumerKey",
                    ConsumerSecret = "ConsumerSecret",
                    OAuthToken = "OAuthToken",
                    OAuthTokenSecret = "OAuthTokenSecret"
                }
            };
            using (TwitterContext twitter = new TwitterContext(credentials))
            {
                IQueryable<Search> source = twitter.Search; // Get source.
                IQueryable<List<Status>> query = source
                    .Where(search => search.Type == SearchType.Search && search.Query == "LINQ")
                    .OrderBy(search => search.SearchMetaData.Count)
                    .Select(search => search.Statuses); // Define query.
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
