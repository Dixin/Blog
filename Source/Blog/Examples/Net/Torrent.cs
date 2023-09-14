namespace Examples.Net;

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
    { ;
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
}