namespace Examples.Net;

using System;
using System.Linq;
using System.Net.Http;
using Examples.Common;
using Examples.Diagnostics;
using Examples.IO;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonoTorrent;
using MonoTorrent.Client;
using OpenQA.Selenium;

public class TorrentHelper
{
    private const string TorrentExtension = ".torrent";

    public static async Task DownloadAsync(string magnetUrl, string torrentDirectory, CancellationToken cancellationToken = default)
    {
        MagnetUri magnetUri = MagnetUri.Parse(magnetUrl).AddDefaultTrackers();
        MagnetLink magnetLink = new(InfoHash.FromHex(magnetUri.ExactTopic), magnetUri.DisplayName, magnetUri.Trackers.ToArray());
        EngineSettingsBuilder engineSettingsBuilder = new()
        {
            CacheDirectory = torrentDirectory,
        };

        ClientEngine clientEngine = new(engineSettingsBuilder.ToSettings());
        //await clientEngine.StartAllAsync();
        byte[] torrent = await clientEngine.DownloadMetadataAsync(magnetLink, cancellationToken);
        string torrentPath = Path.Combine(torrentDirectory, $"{magnetUri.DisplayName}@{magnetUri.ExactTopic}{TorrentExtension}");
        await File.WriteAllBytesAsync(torrentPath, torrent, cancellationToken);
    }

    public static async Task<IEnumerable<Task>> DownloadAllAsync(IEnumerable<string> magnetUrls, string torrentDirectory, Action<string>? logger = null, CancellationToken cancellationToken = default)
    {
        EngineSettingsBuilder engineSettingsBuilder = new()
        {
            CacheDirectory = torrentDirectory
        };

        ClientEngine clientEngine = new(engineSettingsBuilder.ToSettings());
        await clientEngine.StartAllAsync();
        return magnetUrls
            .Select(magnetUrl => MagnetUri.Parse(magnetUrl).AddDefaultTrackers())
            .Select(magnetUri =>
            (
                magnetUri,
                new MagnetLink(InfoHash.FromHex(magnetUri.ExactTopic), magnetUri.DisplayName, magnetUri.Trackers.ToArray())
            ))
            .Select(async task =>
            {
                byte[] torrent = await clientEngine.DownloadMetadataAsync(task.Item2, cancellationToken);
                string torrentPath = Path.Combine(torrentDirectory, $"{task.magnetUri.DisplayName}@{task.magnetUri.ExactTopic}{TorrentExtension}");
                logger?.Invoke(torrentPath);
                await File.WriteAllBytesAsync(torrentPath, torrent, cancellationToken);
            });
    }

    public static async Task DownloadAllFromCacheAsync(IEnumerable<string> magnetUrls, string torrentDirectory, bool useBrowser = false, Action<string>? log = null, CancellationToken cancellationToken = default, int degreeOfParallelism = 4)
    {
        const string Referer = "https://magnet2torrent.com/";

        await magnetUrls
            .Select(MagnetUri.Parse)
            .ParallelForEachAsync(async (magnetUri, index) =>
            {
                string torrentUrl = $"https://itorrents.org/torrent/{magnetUri.ExactTopic}{TorrentExtension}";
                if (!useBrowser)
                {
                    using HttpClient? httpClient = useBrowser ? null : new HttpClient().AddEdgeHeaders(referer: Referer);
                    try
                    {
                        await Retry.FixedIntervalAsync(
                            async () => await httpClient.GetFileAsync(
                                torrentUrl,
                                Path.Combine(torrentDirectory, $"{magnetUri.DisplayName}@{magnetUri.ExactTopic}{TorrentExtension}"),
                                cancellationToken),
                            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.Moved });
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log?.Invoke(magnetUri.ToString());
                        log?.Invoke(exception.ToString());
                        log?.Invoke(string.Empty);
                    }
                }
                else
                {
                    using IWebDriver? webDriver = useBrowser ? WebDriverHelper.Start(downloadDirectory: torrentDirectory) : null;
                    webDriver?.Navigate().GoToUrl(Referer);
                    try
                    {
                        Retry.FixedInterval(() => webDriver.Url = torrentUrl);
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log?.Invoke(magnetUri.ToString());
                        log?.Invoke(exception.ToString());
                        log?.Invoke(string.Empty);
                    }
                }
            }, degreeOfParallelism);
    }

    public static async Task AddDefaultTrackersAsync(string torrentDirectory, Action<string?>? log = null)
    {
        await Directory
            .GetFiles(torrentDirectory, $"{PathHelper.AllSearchPattern}{TorrentExtension}", SearchOption.AllDirectories)
            .Select(file => @$"""{Path.GetFileName(file)}""")
            .Chunk(250)
            .SelectMany(chunk => MagnetUri.DefaultTrackers, (chunk, uri) => (chunk, uri))
            .ForEachAsync(async chunkUri =>
            {
                int result = await ProcessHelper.StartAndWaitAsync(
                    @"""C:\Program Files\Transmission\transmission-edit.exe""", 
                    $"-a {chunkUri.uri} {string.Join(" ", chunkUri.chunk)}", 
                    log, 
                    log, 
                    startInfo => startInfo.WorkingDirectory = torrentDirectory);
                Debug.Assert(result == 0);
            });
    }
}