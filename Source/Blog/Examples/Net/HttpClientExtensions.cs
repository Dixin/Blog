namespace Examples.Net;

using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using Examples.Common;

public static class HttpClientExtensions
{
    public static async Task<string> GetCompressedStringAsync(this HttpClient httpClient, string url, CancellationToken cancellation = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url)
        {
            Version = httpClient.DefaultRequestVersion,
            VersionPolicy = httpClient.DefaultVersionPolicy
        };
        httpClient.DefaultRequestHeaders.ForEach(header => request.Headers.Add(header.Key, header.Value));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip, deflate, br"));

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation);
        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellation);
        await using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using StreamReader reader = new(gZipStream);
        return await reader.ReadToEndAsync(cancellation);
    }

    public static async Task GetFileAsync(this HttpClient httpClient, string url, string file, CancellationToken cancellation = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url)
        {
            Version = httpClient.DefaultRequestVersion,
            VersionPolicy = httpClient.DefaultVersionPolicy
        };
        httpClient.DefaultRequestHeaders.ForEach(header => request.Headers.Add(header.Key, header.Value));

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation);
        response.EnsureSuccessStatusCode();
        await using FileStream fileStream = File.OpenWrite(file);
        await response.Content.CopyToAsync(fileStream, cancellation);
    }

    public static async Task<string> GetStringAsync(this HttpClient httpClient, string url, Encoding? encoding = null, CancellationToken cancellation = default)
    {
        if (encoding is null)
        {
            return await httpClient.GetStringAsync(url, cancellation);
        }

        byte[] bytes = await httpClient.GetByteArrayAsync(url, cancellation);
        return encoding.GetString(bytes, 0, bytes.Length - 1);
    }

    public static HttpClient AddEdgeHeaders(this HttpClient httpClient, string cookie = "", string referer = "")
    {
        HttpRequestHeaders defaultHeaders = httpClient.DefaultRequestHeaders;
        defaultHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
        defaultHeaders.Add("accept-language", "en-US,en;q=0.9");
        if (cookie.IsNotNullOrWhiteSpace())
        {
            defaultHeaders.Add("cookie", cookie);
        }

        defaultHeaders.Add("dnt", "1");
        if (referer.IsNotNullOrWhiteSpace())
        {
            defaultHeaders.Add("referer", referer);
        }

        defaultHeaders.Add("sec-ch-ua", """"Not?A_Brand";v="8", "Chromium";v="108", "Microsoft Edge";v="108"""");
        defaultHeaders.Add("sec-ch-ua-mobile", "?0");
        defaultHeaders.Add("sec-fetch-dest", "document");
        defaultHeaders.Add("sec-fetch-mode", "navigate");
        defaultHeaders.Add("sec-fetch-site", "same-origin");
        defaultHeaders.Add("sec-fetch-user", "?1");
        defaultHeaders.Add("upgrade-insecure-requests", "1");
        defaultHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 Edg/108.0.1462.46");
        return httpClient;
    }
}