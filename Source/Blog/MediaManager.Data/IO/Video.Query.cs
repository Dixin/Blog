namespace Examples.IO;

using System;
using Examples.Common;
using Examples.Linq;
using Examples.Net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;
using Xabe.FFmpeg;

using JsonReaderException = Newtonsoft.Json.JsonReaderException;

internal static partial class Video
{
    private static IEnumerable<string> EnumerateVideos(string directory, Func<string, bool>? predicate = null) =>
        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => (predicate?.Invoke(file) ?? true) && file.HasAnyExtension(AllVideoExtensions));

    internal static Task<VideoMetadata> ReadVideoMetadataAsync(string file, ImdbMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = 10) =>
        Retry.IncrementalAsync(
            async () =>
            {
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(file);
                IVideoStream videoStream = mediaInfo.VideoStreams.Single(videoStream => !videoStream.Codec.EqualsIgnoreCase("mjpeg") && !videoStream.Codec.EqualsIgnoreCase("png"));
                return new VideoMetadata()
                {
                    File = relativePath.IsNullOrWhiteSpace() ? file : Path.GetRelativePath(relativePath, file),
                    Width = videoStream.Width,
                    Height = videoStream.Height,
                    Audio = mediaInfo.AudioStreams?.Count() ?? 0,
                    Subtitle = mediaInfo.SubtitleStreams?.Count() ?? 0,
                    AudioBitRates = mediaInfo.AudioStreams?.Select(audio => (int)audio.Bitrate).ToArray() ?? Array.Empty<int>(),
                    TotalMilliseconds = mediaInfo.Duration.TotalMilliseconds,
                    Imdb = imdbMetadata,
                    FrameRate = videoStream.Framerate
                };
            },
            retryCount,
            _ => true);

    internal static bool TryReadVideoMetadata(string file, [NotNullWhen(true)] out VideoMetadata? videoMetadata, ImdbMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = 10, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Task<VideoMetadata> task = ReadVideoMetadataAsync(file, imdbMetadata, relativePath, retryCount);
        if (!task.Wait(TimeSpan.FromSeconds(30)))
        {
            videoMetadata = null;
            log($"!Timeout {file}");
            return false;
        }

        if (task.Exception is not null)
        {
            videoMetadata = null;
            log($"!Fail {file} {task.Exception}");
            return false;
        }

        videoMetadata = task.Result;
        log($"{videoMetadata.Width}x{videoMetadata.Height}, {videoMetadata.Audio} audio, {file}");
        return true;
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
        if (videoMetadata.Width <= 0 || videoMetadata.Height <= 0 || (isNoAudioAllowed ? videoMetadata.Audio < 0 : videoMetadata.Audio <= 0))
        {
            return ($"Failed {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.Audio}Audio {videoMetadata.File}", null);
        }

        string fileName = Path.GetFileNameWithoutExtension(videoMetadata.File);
        if (fileName.ContainsIgnoreCase("1080p"))
        {
            if (videoMetadata.DefinitionType is not DefinitionType.P1080)
            {
                return ($"!Not 1080p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", null);
            }
        }
        else
        {
            if (videoMetadata.DefinitionType is DefinitionType.P1080)
            {
                return ($"!1080p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", is1080);
            }

            if (fileName.ContainsIgnoreCase("720p"))
            {
                if (videoMetadata.DefinitionType is not DefinitionType.P720)
                {
                    return ($"!Not 720p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", null);
                }
            }
            else
            {
                if (videoMetadata.DefinitionType is DefinitionType.P720)
                {
                    return ($"!720p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", is720);
                }
            }
        }

        if (Math.Abs(videoMetadata.FrameRate - 23.976) > 0.001)
        {
            //return ($"!Not 23.976fps: {videoMetadata.FrameRate} {videoMetadata.File}", null);
        }

        if (Regex.IsMatch(fileName, "[1-9]Audio"))
        {
            if (videoMetadata.Audio < 2)
            {
                return ($"!Not multiple audio: {videoMetadata.Audio} {videoMetadata.File}", null);
            }
        }
        else
        {
            if (videoMetadata.Audio >= 2)
            {
                return ($"!Multiple audio: {videoMetadata.Audio} {videoMetadata.File}", null);
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
        IEnumerable<string> directories = Directory.EnumerateDirectories(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
        while (--level > 0)
        {
            directories = directories.SelectMany(d => Directory.EnumerateDirectories(d, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly));
            level--;
        }

        return directories.Order();
    }

    internal static async Task DownloadImdbMetadataAsync(string directory, int level = DefaultDirectoryLevel, bool overwrite = false, bool useCache = false, bool useBrowser = false, int? degreeOfParallelism = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        string[] movies = EnumerateDirectories(directory, level).ToArray();
        Task[] tasks = Partitioner
            .Create(movies, true)
            .GetOrderablePartitions(degreeOfParallelism ?? IOMaxDegreeOfParallelism)
            .Select((partition, partitionIndex) => Task.Run(async () =>
            {
                using IWebDriver? webDriver = useBrowser ? WebDriverHelper.Start(partitionIndex) : null;
                if (webDriver is not null)
                {
                    webDriver.Url = "https://www.imdb.com/";
                }

                await partition.ForEachAsync(async movieWithIndex =>
                {
                    (long _, string movie) = movieWithIndex;
                    await DownloadImdbMetadataAsync(movie, webDriver, overwrite, useCache, log);
                });
            }))
            .ToArray();
        await Task.WhenAll(tasks);
    }

    internal static bool IsCommonVideo(this string file) => file.HasAnyExtension(CommonVideoExtensions);

    internal static bool IsVideo(this string file) => file.HasAnyExtension(AllVideoExtensions);

    internal static bool IsTextSubtitle(this string file) => file.HasAnyExtension(TextSubtitleExtensions);

    internal static bool IsSubtitle(this string file) => file.HasAnyExtension(AllSubtitleExtensions);

    internal static bool IsDiskImage(this string file) => file.HasExtension(DiskImageExtension);
}