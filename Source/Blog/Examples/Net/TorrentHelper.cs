namespace Examples.Net;

using System;
using System.Linq;
using System.Net.Http;
using Examples.Common;
using Examples.Diagnostics;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonoTorrent;
using MonoTorrent.Client;
using OpenQA.Selenium;

public class TorrentHelper
{
    public static async Task DownloadAsync(string magnetUrl, string torrentDirectory, CancellationToken cancellationToken = default)
    {
        MagnetUri magnetUri = MagnetUri.Parse(magnetUrl).WithDefaultTrackers();
        MagnetLink magnetLink = new(InfoHash.FromHex(magnetUri.ExactTopic), magnetUri.DisplayName, magnetUri.Trackers.ToArray());
        EngineSettingsBuilder engineSettingsBuilder = new()
        {
            CacheDirectory = torrentDirectory,
        };

        ClientEngine clientEngine = new(engineSettingsBuilder.ToSettings());
        //await clientEngine.StartAllAsync();
        byte[] torrent = await clientEngine.DownloadMetadataAsync(magnetLink, cancellationToken);
        string torrentPath = Path.Combine(torrentDirectory, $"{magnetUri.DisplayName}@{magnetUri.ExactTopic}.torrent");
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
            .Select(magnetUrl => MagnetUri.Parse(magnetUrl).WithDefaultTrackers())
            .Select(magnetUri =>
            (
                magnetUri,
                new MagnetLink(InfoHash.FromHex(magnetUri.ExactTopic), magnetUri.DisplayName, magnetUri.Trackers.ToArray())
            ))
            .Select(async task =>
            {
                byte[] torrent = await clientEngine.DownloadMetadataAsync(task.Item2, cancellationToken);
                string torrentPath = Path.Combine(torrentDirectory, $"{task.magnetUri.DisplayName}@{task.magnetUri.ExactTopic}.torrent");
                logger?.Invoke(torrentPath);
                await File.WriteAllBytesAsync(torrentPath, torrent, cancellationToken);
            });
    }

    public static async Task DownloadAllFromCacheAsync(IEnumerable<string> magnetUrls, string torrentDirectory, bool useBrowser = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        const string Referer = "https://magnet2torrent.com/";
        using HttpClient? httpClient = useBrowser ? null : new HttpClient().AddEdgeHeaders(referer: Referer);

        using IWebDriver? webDriver = useBrowser ? WebDriverHelper.Start(downloadDirectory: torrentDirectory) : null;
        webDriver?.Navigate().GoToUrl(Referer);

        await magnetUrls
            .Select(MagnetUri.Parse)
            .ForEachAsync(async magnetUri =>
            {
                string torrentUrl = $"https://itorrents.org/torrent/{magnetUri.ExactTopic}.torrent";
                if (httpClient is not null)
                {
                    try
                    {
                        await Retry.FixedIntervalAsync(
                            async () => await httpClient.GetFileAsync(
                                torrentUrl,
                                Path.Combine(torrentDirectory, $"{magnetUri.DisplayName}@{magnetUri.ExactTopic}.torrent"),
                                cancellationToken),
                            isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.Moved });
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log?.Invoke(magnetUri.ToString());
                        if (exception is not HttpRequestException { StatusCode: HttpStatusCode.Moved })
                        {
                            log?.Invoke(exception.ToString());
                        }

                        log?.Invoke(string.Empty);
                    }
                }
                else if (webDriver is not null)
                {
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
            });
    }

    public static async Task AddDefaultTrackersAsync(string torrentDirectory, Action<string?>? log = null) =>
        await MagnetUri
            .DefaultTrackers
            .SelectMany(tracker => Directory.GetFiles(@torrentDirectory), (tracker, torrent) => $"-a {tracker} {torrent}")
            .AsParallel()
            .WithDegreeOfParallelism(4)
            .ForEachAsync(async command =>
            {
                int result = await ProcessHelper.StartAndWaitAsync("transmission-edit", command, log, log);
                Debug.Assert(result == 0);
            });
}