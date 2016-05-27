namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::LinqToTwitter;

    internal static class LinqToTwitter
    {
        private static readonly SingleUserAuthorizer Authorizer = new SingleUserAuthorizer()
        {
            CredentialStore = new InMemoryCredentialStore()
            {
                ConsumerKey = "ConsumerKey",
                ConsumerSecret = "ConsumerSecret",
                OAuthToken = "OAuthToken",
                OAuthTokenSecret = "OAuthTokenSecret"
            }
        };

        internal static async Task<IEnumerable<string>> QueryAsync(string keyword, int count)
        {
            using (TwitterContext twitter = new TwitterContext(Authorizer))
            {
                IQueryable<Search> searchQuery = from search in twitter.Search
                                                 where search.Type == SearchType.Search
                                                     && search.Query == keyword
                                                     && search.Count == count
                                                 select search;
                Search searchResult = await searchQuery.SingleAsync();
                return from status in searchResult.Statuses
                       select $"{status.User.ScreenNameResponse}: {status.Text}";
            }
        }
    }
}
