namespace Examples.Net;

using System.IO.Compression;
using Examples.Common;

public static class WebClientExtensions
{
    public static async Task<string> DownloadCompressedStringAsync(this WebClient webClient, string url)
    {
        string? encoding = webClient.Headers[HttpRequestHeader.AcceptEncoding];
        if (string.IsNullOrWhiteSpace(encoding))
        {
            webClient.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
        }

        await using GZipStream gZipStream = new(await webClient.OpenReadTaskAsync(url), CompressionMode.Decompress);
        using StreamReader reader = new(gZipStream);
        return await reader.ReadToEndAsync();
    }

    public static WebClient AddChromeHeaders(this WebClient webClient, string cookie = "")
    {
        WebHeaderCollection headers = webClient.Headers;
        headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
        headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
        headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
        headers.Add("dnt", "1");
        headers.Add("sec-ch-ua", """
             " Not A;Brand";v="99", "Chromium";v="90", "Google Chrome";v="90"
             """);
        headers.Add("sec-ch-ua-mobile", "?0");
        headers.Add("sec-fetch-dest", "document");
        headers.Add("sec-fetch-mode", "navigate");
        headers.Add("sec-fetch-site", "none");
        headers.Add("sec-fetch-user", "1");
        headers.Add("upgrade-insecure-requests", "1");
        headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
        if (cookie.IsNotNullOrWhiteSpace())
        {
            headers.Add(HttpRequestHeader.Cookie, cookie);
        }

        return webClient;
    }
}