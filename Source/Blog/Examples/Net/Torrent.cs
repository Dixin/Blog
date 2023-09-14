namespace Examples.Net;

using System;
using System.Net.Http;
using Examples.Common;
using Examples.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonoTorrent;
using MonoTorrent.Client;

public class Torrent
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

    public static async Task DownloadAllFromCacheAsync(IEnumerable<string> magnetUrls, string torrentDirectory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders(referer: "https://magnet2torrent.com/");

        await magnetUrls
            .Select(MagnetUri.Parse)
            .ParallelForEachAsync(async magnetUri =>
            {
                try
                {
                    await Retry.FixedIntervalAsync(
                        async () => await httpClient.GetFileAsync(
                            $"https://itorrents.org/torrent/{magnetUri.ExactTopic}.torrent",
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
            }, 2);
    }
}