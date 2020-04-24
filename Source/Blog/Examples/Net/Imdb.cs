namespace Examples.Net
{
    using System.Net;
    using System.Threading.Tasks;
    using CsQuery;

    internal static class Imdb
    {
        internal static async Task<string> DownloadJsonAsync(string url)
        {
            using WebClient webClient = new WebClient();
            string imdbHtml = await webClient.DownloadStringTaskAsync(url);
            CQ cqImdb = new CQ(imdbHtml);
            return cqImdb.Find(@"script[type=""application/ld+json""]").Text();
        }
    }
}