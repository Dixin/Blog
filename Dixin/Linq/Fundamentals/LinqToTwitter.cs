namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::LinqToTwitter;

    using Settings = Dixin.Properties.Settings;

    internal static class LinqToTwitter
    {
        private static readonly SingleUserAuthorizer Authorizer = new SingleUserAuthorizer()
        {
            CredentialStore = new InMemoryCredentialStore()
            {
                ConsumerKey = Settings.Default.TwitterConsumerKey,
                ConsumerSecret = Settings.Default.TwitterConsumerSecret,
                OAuthToken = Settings.Default.TwitterOAuthToken,
                OAuthTokenSecret = Settings.Default.TwitterOAuthTokenSecret
            }
        };

        internal static async Task<IEnumerable<Tuple<string, string>>> SearchAsync(string keyword, int count)
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
