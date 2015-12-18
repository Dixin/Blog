namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::LinqToTwitter;

    public static partial class LinqToTwitter
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

        public static async Task<IEnumerable<Tuple<string, string>>> SearchAsync(string keyword, int count)
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
                       select Tuple.Create(status.User.ScreenNameResponse, status.Text);
            }
        }
    }
}
