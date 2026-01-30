namespace Examples.Net;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
    internal const string TorrentExtension = ".torrent";

    internal const string TorrentSearchPattern = $"{PathHelper.AllSearchPattern}{TorrentExtension}";

    private const string HashSeparator = "@";

    private static readonly int IOMaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4);

    public static async Task DownloadAsync(string magnetUrl, string torrentDirectory, CancellationToken cancellationToken = default)
    {
        MagnetUri magnetUri = MagnetUri.Parse(magnetUrl).AddDefaultTrackers();
        MagnetLink magnetLink = new(InfoHash.FromHex(magnetUri.ExactTopic), magnetUri.DisplayName, magnetUri.Trackers.ToArray());
        EngineSettingsBuilder engineSettingsBuilder = new()
        {
            CacheDirectory = torrentDirectory
        };

        using ClientEngine clientEngine = new(engineSettingsBuilder.ToSettings());
        byte[] torrent = (await clientEngine.DownloadMetadataAsync(magnetLink, cancellationToken)).ToArray();
        string torrentPath = Path.Combine(torrentDirectory, $"{magnetUri.DisplayName}{HashSeparator}{magnetUri.ExactTopic}{TorrentExtension}");
        await File.WriteAllBytesAsync(torrentPath, torrent, cancellationToken);
    }

    public static async Task DownloadAllAsync(string magnetUrlPath, string torrentDirectory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        string[] magnetUrls = (await File.ReadAllTextAsync(magnetUrlPath, cancellationToken))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        await DownloadAllAsync(magnetUrls, torrentDirectory, log, cancellationToken);
    }

    public static async Task DownloadAllAsync(IEnumerable<string> magnetUrls, string torrentDirectory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(torrentDirectory))
        {
            Directory.CreateDirectory(torrentDirectory);
        }

        IEnumerable<string> downloadedHashes = Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(torrent => PathHelper.GetFileNameWithoutExtension(torrent).Split(HashSeparator).Last());

        EngineSettingsBuilder engineSettingsBuilder = new()
        {
            CacheDirectory = torrentDirectory
        };

        using ClientEngine clientEngine = new(engineSettingsBuilder.ToSettings());
        await clientEngine.StartAllAsync();
        await Task.WhenAll(magnetUrls
            .Select(magnetUrl => MagnetUri.Parse(magnetUrl).AddDefaultTrackers())
            .ExceptBy(downloadedHashes, uri => uri.ExactTopic, StringComparer.OrdinalIgnoreCase)
            .Select(magnetUri =>
            (
                magnetUri,
                new MagnetLink(InfoHash.FromHex(magnetUri.ExactTopic), magnetUri.DisplayName, magnetUri.Trackers.ToArray())
            ))
            .Select(async link =>
            {
                byte[] torrent = (await clientEngine.DownloadMetadataAsync(link.Item2, cancellationToken)).ToArray();
                string torrentPath = Path.Combine(torrentDirectory, $"{link.magnetUri.DisplayName}{HashSeparator}{link.magnetUri.ExactTopic}{TorrentExtension}");
                log?.Invoke(torrentPath);
                await File.WriteAllBytesAsync(torrentPath, torrent, cancellationToken);
            }));
    }

    public static async Task DownloadAllFromCacheAsync(string magnetUrlPath, string torrentDirectory, bool useBrowser = false, int? degreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= IOMaxDegreeOfParallelism;
        string[] magnetUrls = (await File.ReadAllTextAsync(magnetUrlPath, cancellationToken))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        await DownloadAllFromCacheAsync(magnetUrls, torrentDirectory, null, useBrowser, degreeOfParallelism, log, cancellationToken);
    }

    public static async Task DownloadAllFromCacheAsync(
        IEnumerable<string> magnetUrls, string torrentDirectory, Func<string, string>? getHashFromFile = null, bool useBrowser = false, int? degreeOfParallelism = null,
        Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        getHashFromFile ??= torrent => PathHelper.GetFileNameWithoutExtension(torrent).Split(HashSeparator).Last();

        const string Referer = "https://magnet2torrent.com/";
        degreeOfParallelism ??= IOMaxDegreeOfParallelism;

        if (Directory.Exists(torrentDirectory))
        {
            Directory.CreateDirectory(torrentDirectory);
        }

        IEnumerable<string> downloadedHashes = Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(getHashFromFile);

        ConcurrentQueue<MagnetUri> magnetUris = new(magnetUrls
            .Select(MagnetUri.Parse)
            .ExceptBy(downloadedHashes, uri => uri.ExactTopic, StringComparer.OrdinalIgnoreCase));
        if (useBrowser)
        {
            WebDriverHelper.DisposeAll();
        }

        await Enumerable
            .Range(0, degreeOfParallelism.Value)
            .ParallelForEachAsync
                (async (index, _, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    if (!useBrowser)
                    {
                        using HttpClient httpClient = new HttpClient().AddEdgeHeaders(referer: Referer);
                        while (magnetUris.TryDequeue(out MagnetUri? magnetUri))
                        {
                            string torrentUrl = $"https://itorrents.org/torrent/{magnetUri.ExactTopic}{TorrentExtension}";
                            try
                            {
                                await Retry.FixedIntervalAsync(
                                    async () => await httpClient.GetFileAsync(
                                        torrentUrl,
                                        Path.Combine(torrentDirectory, $"{magnetUri.DisplayName}@{magnetUri.ExactTopic}{TorrentExtension}"),
                                        token),
                                    isTransient: exception => exception is not HttpRequestException { StatusCode: HttpStatusCode.Moved },
                                    cancellationToken: token);
                            }
                            catch (Exception exception) when (exception.IsNotCritical())
                            {
                                log?.Invoke(magnetUri.ToString());
                                log?.Invoke(exception.ToString());
                                log?.Invoke(string.Empty);
                            }
                        }
                    }
                    else
                    {
                        using IWebDriver webDriver = WebDriverHelper.Start(index, downloadDirectory: torrentDirectory, isLoadingAll: true, keepExisting: true);
                        webDriver.Navigate().GoToUrl(Referer);
                        while (magnetUris.TryDequeue(out MagnetUri? magnetUri))
                        {
                            string torrentUrl = $"https://itorrents.org/torrent/{magnetUri.ExactTopic}{TorrentExtension}";
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
                    }
                },
                degreeOfParallelism,
                cancellationToken);
    }

    public static async Task DownloadAllFromCache2Async(string magnetUrlPath, string torrentDirectory, string lastHash = "", Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        string[] magnetUrls = (await File.ReadAllTextAsync(magnetUrlPath, cancellationToken))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        await DownloadAllFromCache2Async(magnetUrls, torrentDirectory, lastHash, log, cancellationToken);
    }

    public static async Task DownloadAllFromCache2Async(string[] magnetUrls, string torrentDirectory, string lastHash = "", Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(torrentDirectory))
        {
            Directory.CreateDirectory(torrentDirectory);
        }

        IEnumerable<string> allDownloadedHashes = Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(torrent => PathHelper.GetFileNameWithoutExtension(torrent).Split(HashSeparator).Last());

        IEnumerable<string> downloadedHashes = Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(PathHelper.GetFileNameWithoutExtension)
            .Where(fileName => !fileName.ContainsOrdinal(HashSeparator));
        if (lastHash.IsNotNullOrWhiteSpace())
        {
            downloadedHashes = downloadedHashes.Append(lastHash);
        }

        Dictionary<string, int> allMagnetUris = magnetUrls
            .Select((magnetUrl, index) => (Uri: MagnetUri.Parse(magnetUrl), Index: index))
            .ToDictionary(magnetUri => magnetUri.Uri.ExactTopic, magnetUri => magnetUri.Index, StringComparer.OrdinalIgnoreCase);
        int lastIndex = downloadedHashes.Select(hash => allMagnetUris[hash]).Max();

        WebDriverHelper.DisposeAll();
        using IWebDriver webDriver = WebDriverHelper.Start(downloadDirectory: torrentDirectory, isLoadingAll: true, keepExisting: true);
        await magnetUrls
            .Skip(lastIndex)
            .Select(MagnetUri.Parse)
            .ExceptBy(allDownloadedHashes, uri => uri.ExactTopic, StringComparer.OrdinalIgnoreCase)
            .ForEachAsync(async magnetUri =>
            {
                string torrentUrl = $"https://torrage.info/torrent.php?h={magnetUri.ExactTopic}";
                try
                {
                    await Retry.FixedIntervalAsync(
                        async () =>
                        {
                            webDriver.Url = torrentUrl;
                            await Task.Delay(WebDriverHelper.DefaultDomWait, cancellationToken);
                            IWebElement link = webDriver.FindElement(By.Id("torrent-operation-link"));
                            string torrentDownloadUrl = link.GetAttribute("href");
                            link.Click();
                            await Task.Delay(WebDriverHelper.DefaultDomWait, cancellationToken);
                            if (!webDriver.Title.ContainsIgnoreCase("Page Not Found"))
                            {
                                await Task.Delay(WebDriverHelper.DefaultNetworkWait, cancellationToken);
                            }

                            log?.Invoke(magnetUri.ExactTopic);
                            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                        },
                        cancellationToken: cancellationToken);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                    log?.Invoke(magnetUri.ToString());
                    log?.Invoke(exception.ToString());
                    log?.Invoke(string.Empty);
                }
            }, cancellationToken);
    }

    public static async Task DownloadAllFromCache3Async(string magnetUrlPath, string torrentDirectory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        string[] magnetUrls = (await File.ReadAllTextAsync(magnetUrlPath, cancellationToken))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        await DownloadAllFromCache3Async(magnetUrls, torrentDirectory, log, cancellationToken);
    }
    public static async Task DownloadAllFromCache3Async(string[] magnetUrls, string torrentDirectory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(torrentDirectory))
        {
            Directory.CreateDirectory(torrentDirectory);
        }

        IEnumerable<string> allDownloadedHashes = Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(torrent => PathHelper.GetFileNameWithoutExtension(torrent).Split(HashSeparator).Last());

        IEnumerable<string> downloadedHashes = Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(PathHelper.GetFileNameWithoutExtension)
            .Where(fileName => !fileName.ContainsOrdinal(HashSeparator));

        WebDriverHelper.DisposeAll();
        using IWebDriver webDriver = WebDriverHelper.Start(downloadDirectory: torrentDirectory, isLoadingAll: true, keepExisting: true);
        await magnetUrls
            .Select(MagnetUri.Parse)
            .ExceptBy(allDownloadedHashes, uri => uri.ExactTopic, StringComparer.OrdinalIgnoreCase)
            .ForEachAsync(async magnetUri =>
            {
                string torrentUrl = $"https://btcache.me/torrent/{magnetUri.ExactTopic}";
                try
                {
                    await Retry.FixedIntervalAsync(
                        async () =>
                        {
                            webDriver.Url = torrentUrl;
                            await Task.Delay(WebDriverHelper.DefaultDomWait, cancellationToken);
                            IWebElement bodyElement = webDriver.FindElement(By.TagName("body"));
                            if (!bodyElement.Text.ContainsIgnoreCase("Invalid INFO_HASH") && Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }

                            log?.Invoke(magnetUri.ExactTopic);
                        },
                        cancellationToken: cancellationToken);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                    log?.Invoke(magnetUri.ToString());
                    log?.Invoke(exception.ToString());
                    log?.Invoke(string.Empty);
                }
            }, cancellationToken);
    }

    public static async Task AddDefaultTrackersAsync(string torrentDirectory, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        await Directory
            .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
            .Select(file => $"""
                 "{PathHelper.GetFileName(file)}"
                 """)
            .Chunk(250)
            .SelectMany(chunk => MagnetUri.DefaultTrackers, (chunk, uri) => (chunk, uri))
            .ForEachAsync(async chunkUri =>
            {
                int result = await ProcessHelper.StartAndWaitAsync(
                    """
                    "C:\Program Files\Transmission\transmission-edit.exe"
                    """,
                    $"-a {chunkUri.uri} {string.Join(" ", chunkUri.chunk)}",
                    log,
                    log,
                    startInfo => startInfo.WorkingDirectory = torrentDirectory,
                    cancellationToken: cancellationToken);
                Debug.Assert(result == 0);
            }, cancellationToken);
    }

    public static async Task PrintNotDownloadedAsync(string magnetUrlPath, string torrentDirectory, bool addTrackers = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        HashSet<string> downloadedHashes = new(
            Directory
                .EnumerateFiles(torrentDirectory, TorrentSearchPattern, SearchOption.AllDirectories)
                .Select(torrent => PathHelper.GetFileNameWithoutExtension(torrent).Split(HashSeparator).Last()),
            StringComparer.OrdinalIgnoreCase);

        (await File.ReadAllTextAsync(magnetUrlPath, cancellationToken))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(MagnetUri.Parse)
            .GroupBy(magnetUri => magnetUri.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Where(group => !group.Select(magnetUri => magnetUri.ExactTopic).Any(downloadedHashes.Contains))
            .SelectMany(group => group)
            .ForEach(magnetUri => log?.Invoke((addTrackers ? magnetUri.AddDefaultTrackers() : magnetUri).ToString()));
    }

    public static async Task<string> GetExactTopicAsync(string file) => 
        (await Torrent.LoadAsync(file)).InfoHashes.V1OrV2.ToHex().ToUpperInvariant() ?? throw new InvalidOperationException(file);
}