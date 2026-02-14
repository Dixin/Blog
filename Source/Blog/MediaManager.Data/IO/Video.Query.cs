namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using Examples.Linq;
using MediaManager.Net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Spectre.Console;
using Xabe.FFmpeg;
using JsonReaderException = Newtonsoft.Json.JsonReaderException;

internal static partial class Video
{
    internal static IEnumerable<string> EnumerateVideos(string directory, Func<string, bool>? predicate = null) =>
        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => (predicate?.Invoke(file) ?? true) && file.IsVideo());

    internal static Task<VideoMetadata> ReadVideoMetadataAsync(string file, ImdbMinMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = IODefaultRetryCount, CancellationToken cancellationToken = default) =>
        Retry.IncrementalAsync(
            async () =>
            {
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(file, cancellationToken);
                IVideoStream videoStream = mediaInfo
                    .VideoStreams
                    .Single(videoStream => !videoStream.Codec.EqualsIgnoreCase("mjpeg") && !videoStream.Codec.EqualsIgnoreCase("png"));
                return new VideoMetadata()
                {
                    File = relativePath.IsNullOrWhiteSpace() ? file : Path.GetRelativePath(relativePath, file),
                    VideoWidth = videoStream.Width,
                    VideoHeight = videoStream.Height,
                    AudioBitRates = mediaInfo.AudioStreams?.OrderBy(audio => audio.Index).Select(audio => audio.Bitrate).ToArray() ?? [],
                    AudioLanguages = mediaInfo.AudioStreams?.OrderBy(audio => audio.Index).Select(audio => audio.Language).ToArray() ?? [],
                    AudioTitles = mediaInfo.AudioStreams?.OrderBy(audio => audio.Index).Select(audio => audio.Title).ToArray() ?? [],
                    SubtitleStreams = mediaInfo.SubtitleStreams?.OrderBy(subtitle => subtitle.Index).Select(subtitle => (subtitle.Language, subtitle.Title, subtitle.Path)).ToArray() ?? [],
                    Duration = mediaInfo.Duration,
                    Imdb = imdbMetadata,
                    VideoFrameRate = videoStream.Framerate
                };
            },
            retryCount,
            cancellationToken: cancellationToken);

    internal static bool TryReadVideoMetadata(string file, [NotNullWhen(true)] out VideoMetadata? videoMetadata, ImdbMinMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = IODefaultRetryCount, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        videoMetadata = null;
        try
        {
            Task<VideoMetadata> task = ReadVideoMetadataAsync(file, imdbMetadata, relativePath, retryCount);
            if (!task.Wait(TimeSpan.FromSeconds(30)))
            {
                log($"!Timeout with {file}");
                return false;
            }

            if (task.Exception is not null)
            {
                log($"!Fail with {file} for {task.Exception}");
                return false;
            }

            videoMetadata = task.Result;
            log($"{videoMetadata.VideoWidth}x{videoMetadata.VideoHeight}, {videoMetadata.AudioBitRates.Length} audio, {file}");
            return true;
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
            log($"!Fail with {file} for {exception}");
            return false;
        }
    }

    internal static int ReadAudioMetadata(string file, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Task<int> task = Task.Run(() =>
        {
            try
            {
                IMediaInfo mediaInfo = FFmpeg.GetMediaInfo(file).Result;
                return mediaInfo.AudioStreams.Count();
            }
            catch (AggregateException exception) when (exception.InnerException is JsonReaderException and not null)
            {
                log($"Fail {file} {exception}");
                return -1;
            }
            catch (AggregateException exception) when (exception.InnerException is ArgumentException and not null)
            {
                log($"Cannot read {file} {exception}");
                return -1;
            }
        });
        if (task.Wait(TimeSpan.FromSeconds(3)))
        {
            log($"{task.Result} audio, {file}");
            return task.Result;
        }

        log($"Timeout {file}");
        return -1;
    }

    private static (string? Message, Action<string>? Action) GetVideoError(VideoMetadata videoMetadata, bool isNoAudioAllowed, Action<string>? is720 = null, Action<string>? is1080 = null)
    {
        if (videoMetadata.VideoWidth <= 0 || videoMetadata.VideoHeight <= 0 || isNoAudioAllowed || videoMetadata.AudioBitRates.IsEmpty())
        {
            return ($"Failed {videoMetadata.VideoWidth}x{videoMetadata.VideoHeight} {videoMetadata.AudioBitRates.Length}Audio {videoMetadata.File}", null);
        }

        string fileName = PathHelper.GetFileNameWithoutExtension(videoMetadata.File);
        if (fileName.ContainsIgnoreCase("1080p"))
        {
            if (videoMetadata.PhysicalDefinitionType is not DefinitionType.P1080)
            {
                return ($"!Not 1080p: {videoMetadata.VideoWidth}x{videoMetadata.VideoHeight} {videoMetadata.File}", null);
            }
        }
        else
        {
            if (videoMetadata.PhysicalDefinitionType is DefinitionType.P1080)
            {
                return ($"!1080p: {videoMetadata.VideoWidth}x{videoMetadata.VideoHeight} {videoMetadata.File}", is1080);
            }

            if (fileName.ContainsIgnoreCase("720p"))
            {
                if (videoMetadata.PhysicalDefinitionType is not DefinitionType.P720)
                {
                    return ($"!Not 720p: {videoMetadata.VideoWidth}x{videoMetadata.VideoHeight} {videoMetadata.File}", null);
                }
            }
            else
            {
                if (videoMetadata.PhysicalDefinitionType is DefinitionType.P720)
                {
                    return ($"!720p: {videoMetadata.VideoWidth}x{videoMetadata.VideoHeight} {videoMetadata.File}", is720);
                }
            }
        }

        if (Math.Abs(videoMetadata.VideoFrameRate - 23.976) > 0.001)
        {
            //return ($"!Not 23.976fps: {videoMetadata.FrameRate} {videoMetadata.File}", null);
        }

        if (Regex.IsMatch(fileName, "[1-9]Audio"))
        {
            if (videoMetadata.AudioBitRates.Length < 2)
            {
                return ($"!Not multiple audio: {videoMetadata.AudioBitRates.Length} {videoMetadata.File}", null);
            }
        }
        else
        {
            if (videoMetadata.AudioBitRates.Length >= 2)
            {
                return ($"!Multiple audio: {videoMetadata.AudioBitRates.Length} {videoMetadata.File}", null);
            }
        }

        if (videoMetadata.AudioBitRates.Any(bitRate => bitRate < 192000))
        {
            //return ($"!Bad audio: Bit rate is only {string.Join(',', videoMetadata.AudioBitRates)} {videoMetadata.File}", null);
        }

        return (null, null);
    }

    internal static IEnumerable<string> EnumerateDirectories(string directory, int level = DefaultDirectoryLevel)
    {
        level.ThrowIfNegative();

        IEnumerable<string> directories = [directory];
        for (int pass = 1; pass <= level; pass++)
        {
            directories = directories.SelectMany(Directory.EnumerateDirectories);
        }

        return directories.Order();
    }

    internal static async Task DownloadImdbMetadataAsync(
        string directory, int level = DefaultDirectoryLevel, 
        bool overwrite = false, bool useCache = false, bool useBrowser = false, 
        int? degreeOfParallelism = null, Func<int, Range>? getRange = null, Func<string, HashSet<string>, string>? resolveImdbIdConflict = null, 
        Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        degreeOfParallelism ??= Imdb.MaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ConcurrentQueue<string> movies = new(EnumerateDirectories(directory, level));
        if (getRange is not null)
        {
            movies = new(movies.Take(getRange(movies.Count)));
        }

        int totalCountToDownload = movies.Count;
        Lock @lock = new();
        await Retry.FixedIntervalAsync(
            async () => await Enumerable
                .Range(0, degreeOfParallelism.Value)
                .ParallelForEachAsync(
                    async (webDriverIndex, index, token) =>
                    {
                        await using PlayWrightWrapper playWrightWrapper = new();
                        while (movies.TryDequeue(out string? movie))
                        {
                            int movieIndex = totalCountToDownload - movies.Count;
                            log($"[yellow]{movieIndex * 100 / totalCountToDownload}% - {movieIndex}/{totalCountToDownload}[/] - [green]{movie.EscapeMarkup()}[/]");

                            if (!await Retry.FixedIntervalAsync(
                                async () => await DownloadImdbMetadataAsync(movie, playWrightWrapper, @lock, overwrite, useCache, resolveImdbIdConflict, log, token),
                                cancellationToken: token))
                            {
                                Interlocked.Decrement(ref totalCountToDownload);
                            }
                        }
                    },
                    degreeOfParallelism,
                    cancellationToken),
            retryingHandler: (sender, args) =>
            {
                log(args.LastException.ToString().EscapeMarkup());
                //cancellationToken.ThrowIfCancellationRequested();
                //cancellationTokenSource.Cancel();
                //cancellationTokenSource.Dispose();
                //cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                //WebDriverHelper.DisposeAll();
                //Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
                //Thread.Sleep(WebDriverHelper.DefaultNetworkWait);
            },
            cancellationToken: cancellationToken);
        try
        {
            cancellationTokenSource.Dispose();
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
            log(exception.ToString());
        }
    }

    internal static bool IsCommonVideo(this string file) => file.HasAnyExtension(CommonVideoExtensions);

    internal static bool IsVideo(this string file) => file.HasAnyExtension(AllVideoExtensions);

    internal static bool IsTextSubtitle(this string file) => file.HasAnyExtension(TextSubtitleExtensions);

    internal static bool IsSubtitle(this string file) => file.HasAnyExtension(AllSubtitleExtensions);

    internal static bool IsDiskImage(this string file) => file.HasExtension(DiskImageExtension);

    internal static bool IsImdbMetadata(this string file) => file.HasExtension(ImdbMetadata.Extension);

    internal static bool IsTmdbXmlMetadata(this string file) => file.HasExtension(TmdbMetadata.XmlExtension);

    internal static bool IsTmdbNfoMetadata(this string file) => file.HasExtension(TmdbMetadata.NfoExtension);

    internal static bool IsImdbCache(this string file) => file.HasExtension(ImdbCacheExtension);
}
