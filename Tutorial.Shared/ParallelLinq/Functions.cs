namespace Tutorial.ParallelLinq
{
    using System.IO;
    using System.Linq;
    using System.Net;

    using static Tutorial.LinqToObjects.EnumerableX;

    internal static partial class Functions
    {
        internal static int Compute(int value = 0, int iteration = 10_000_000)
        {
            Enumerable.Range(0, iteration * (value + 1)).ForEach();
            return value;
        }
    }

    internal static partial class Functions
    {
        internal static string Download(string uri)
        {
            WebRequest request = WebRequest.Create(uri);
            using (WebResponse response = request.EndGetResponse(request.BeginGetResponse(null, null)))
            using (Stream downloadStream = response.GetResponseStream())
            using (StreamReader streamReader = new StreamReader(downloadStream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
