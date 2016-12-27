namespace Dixin.Linq.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static partial class Functions
    {
        private static IDictionary<string, byte[]> cache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        internal static async ValueTask<byte[]> Download(string uri)
        {
            if (cache.TryGetValue(uri, out byte[] result))
            {
                return result;
            }
            using (HttpClient httpClient = new HttpClient())
            {
                result = await httpClient.GetByteArrayAsync(uri);
                cache.Add(uri, result);
                return result;
            }
        }
    }
}
