namespace Examples.IO;

using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
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

    internal static Task<VideoMetadata> GetVideoMetadataAsync(string file, ImdbMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = 10) =>
        Retry.IncrementalAsync(
            async () =>
            {
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(file);
                IVideoStream videoStream = mediaInfo.VideoStreams.Single();
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
            exception => true);

    internal static bool TryGetVideoMetadata(string file, [NotNullWhen(true)] out VideoMetadata? videoMetadata, ImdbMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = 10, Action<string>? log = null)
    {
        log ??= TraceLog;
        Task<VideoMetadata> task = GetVideoMetadataAsync(file, imdbMetadata, relativePath, retryCount);
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

    internal static int GetAudioMetadata(string file, Action<string>? log = null)
    {
        log ??= TraceLog;
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

        string fileName = Path.GetFileNameWithoutExtension(videoMetadata.File) ?? string.Empty;
        if (fileName.ContainsIgnoreCase("1080p"))
        {
            if (!videoMetadata.Is1080P)
            {
                return ($"!Not 1080p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", null);
            }
        }
        else
        {
            if (videoMetadata.Is1080P)
            {
                return ($"!1080p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", is1080);
            }

            if (fileName.ContainsIgnoreCase("720p"))
            {
                if (!videoMetadata.Is720P)
                {
                    return ($"!Not 720p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", null);
                }
            }
            else
            {
                if (videoMetadata.Is720P)
                {
                    return ($"!720p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}", is720);
                }
            }
        }

        if (Math.Abs(videoMetadata.FrameRate - 23.976) > 0.001)
        {
            return ($"!Not 23.976fps: {videoMetadata.FrameRate} {videoMetadata.File}", null);
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
            return ($"!Bad audio: Bit rate is only {string.Join(',', videoMetadata.AudioBitRates)} {videoMetadata.File}", null);
        }

        return (null, null);
    }

    internal static IEnumerable<string> EnumerateDirectories(string directory, int level = 2)
    {
        IEnumerable<string> directories = Directory.EnumerateDirectories(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
        while (--level > 0)
        {
            directories = directories.SelectMany(d => Directory.EnumerateDirectories(d, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly));
            level--;
        }

        return directories.OrderBy(movie => movie);
    }

    internal static async Task DownloadImdbMetadataAsync(string directory, int level = 2, bool overwrite = false, bool useCache = false, bool useBrowser = false, int? degreeOfParallelism = null, Action<string>? log = null)
    {
        log ??= TraceLog;
        string[] movies = EnumerateDirectories(directory, level).ToArray();
        Task[] tasks = Partitioner
            .Create(movies, true)
            .GetOrderablePartitions(degreeOfParallelism ?? IOMaxDegreeOfParallelism)
            .Select((partition, partitionIndex) => Task.Run(async () =>
            {
                using IWebDriver? webDriver = useBrowser ? WebDriverHelper.StartEdge(partitionIndex) : null;
                if (webDriver is not null)
                {
                    webDriver.Url = "https://www.imdb.com/";
                }

                await partition.ForEachAsync(async movieWithIndex =>
                {
                    (long index, string movie) = movieWithIndex;
                    await DownloadImdbMetadataAsync(movie, webDriver, overwrite, useCache, useBrowser, log);
                });
            }))
            .ToArray();
        await Task.WhenAll(tasks);
    }

    private static async Task DownloadImdbMetadataAsync(string directory, IWebDriver? webDriver, bool overwrite = false, bool useCache = false, bool useBrowser = false, Action<string>? log = null)
    {
        log ??= TraceLog;
        string[] files = Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
        string[] jsonFiles = files.Where(file => file.EndsWithIgnoreCase(ImdbMetadataExtension)).ToArray();
        if (jsonFiles.Any())
        {
            if (overwrite)
            {
                jsonFiles.ForEach(jsonFile =>
                {
                    log($"Delete imdb metadata {jsonFile}.");
                    File.Delete(jsonFile);
                });
                files = files.Except(jsonFiles).ToArray();
                jsonFiles = Array.Empty<string>();
            }
            else
            {
                log($"Skip {directory}.");
                return;
            }
        }

        string? nfo = files.FirstOrDefault(file => file.EndsWithIgnoreCase(XmlMetadataExtension));
        if (nfo.IsNullOrWhiteSpace())
        {
            log($"!Missing metadata {directory}.");
            return;
        }

        XElement? root = XDocument.Load(nfo).Root;
        string imdbId = (root?.Element("imdbid") ?? root?.Element("imdb_id"))?.Value ?? NotExistingFlag;
        string imdbFile = Path.Combine(directory, $"{imdbId}{ImdbCacheExtension}");
        string parentFile = Path.Combine(directory, $"{imdbId}.Parent{ImdbCacheExtension}");
        string releaseFile = Path.Combine(directory, $"{imdbId}.Release{ImdbCacheExtension}");
        if (imdbId.EqualsOrdinal(NotExistingFlag))
        {
            if (!files.Any(file => file.EqualsIgnoreCase(imdbFile)))
            {
                await File.WriteAllTextAsync(Path.Combine(directory, imdbFile), string.Empty);
            }

            if (!files.Any(file => file.EqualsIgnoreCase(releaseFile)))
            {
                await File.WriteAllTextAsync(Path.Combine(directory, releaseFile), string.Empty);
            }

            string emptyMetadataFile = Path.Combine(directory, $"{NotExistingFlag}{ImdbMetadataExtension}");
            if (!files.Any(file => file.EqualsIgnoreCase(emptyMetadataFile)))
            {
                await File.WriteAllTextAsync(emptyMetadataFile, string.Empty);
            }

            return;
        }

        log($"Start {directory}");
        (string imdbUrl, string imdbHtml, string parentUrl, string parentHtml, string releaseUrl, string releaseHtml, ImdbMetadata imdbMetadata) = await Imdb.DownloadAsync(
            imdbId,
            useCache,
            useCache ? imdbFile : string.Empty,
            useCache ? parentFile : string.Empty,
            useCache ? releaseFile : string.Empty,
            webDriver);
        Debug.Assert(imdbHtml.IsNotNullOrWhiteSpace());
        if (!imdbMetadata.Regions.Any())
        {
            log($"!Location is missing for {imdbId}: {directory}");
        }

        if (!useCache || !files.Any(file => file.EqualsIgnoreCase(imdbFile)))
        {
            log($"Downloaded {imdbUrl} to {imdbFile}.");
            await File.WriteAllTextAsync(imdbFile, imdbHtml);
            log($"Saved to {imdbFile}.");
        }

        if (!useCache || !files.Any(file => file.EqualsIgnoreCase(releaseFile)))
        {
            log($"Downloaded {releaseUrl} to {releaseFile}.");
            await File.WriteAllTextAsync(releaseFile, releaseHtml);
            log($"Saved to {releaseFile}.");
        }

        if (parentUrl.IsNotNullOrWhiteSpace() && (!useCache || !files.Any(file => file.EqualsIgnoreCase(parentFile))))
        {
            log($"Downloaded {parentUrl} to {parentFile}.");
            await File.WriteAllTextAsync(parentFile, parentHtml);
            log($"Saved to {parentFile}.");
        }

        string jsonFile = Path.Combine(directory, $"{imdbId}{SubtitleSeparator}{imdbMetadata.Year}{SubtitleSeparator}{string.Join(ImdbMetadataSeparator, imdbMetadata.Regions?.Select(value => value.Replace(SubtitleSeparator, string.Empty)).Take(5) ?? Array.Empty<string>())}{SubtitleSeparator}{string.Join(ImdbMetadataSeparator, imdbMetadata.Languages?.Take(3).Select(value => value.Replace(SubtitleSeparator, string.Empty)) ?? Array.Empty<string>())}{SubtitleSeparator}{string.Join(ImdbMetadataSeparator, imdbMetadata.Genres?.Take(5).Select(value => value.Replace(SubtitleSeparator, string.Empty)) ?? Array.Empty<string>())}{ImdbMetadataExtension}");
        log($"Merged {imdbUrl} and {releaseUrl} to {jsonFile}.");
        string jsonContent = JsonSerializer.Serialize(
            imdbMetadata,
            new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
        await File.WriteAllTextAsync(jsonFile, jsonContent);
        log($"Saved to {jsonFile}.");
    }

    internal const string ImdbMetadataSeparator = ",";

    internal static async Task DownloadImdbMetadataAsync(
        (string directory, int level)[] directories, Func<VideoDirectoryInfo, bool> predicate,
        bool overwrite = false, bool useCache = false, bool useBrowser = false, int? degreeOfParallelism = null, Action<string>? log = null)
    {
        string[] movies = directories
            .SelectMany(directory => EnumerateDirectories(directory.directory, directory.level))
            .Where(directory => predicate(VideoDirectoryInfo.Parse(directory)))
            .ToArray();
        if (movies.Any())
        {
            using IWebDriver? webDriver = useBrowser ? WebDriverHelper.StartEdge() : null;
            if (webDriver is not null)
            {
                webDriver.Url = "https://www.imdb.com/";
            }

            await movies.ForEachAsync(async movie => await DownloadImdbMetadataAsync(movie, webDriver, overwrite, useCache, useBrowser, log));
        }
    }

    internal static async Task SaveAllVideoMetadata(string jsonPath, Action<string>? log = null, params string[] directories)
    {
        log ??= TraceLog;
        Dictionary<string, Dictionary<string, VideoMetadata>> existingMetadata = File.Exists(jsonPath)
            ? JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(jsonPath))
            ?? throw new InvalidOperationException(jsonPath)
            : new();

        existingMetadata
            .Values
            .ForEach(group => group
                .Keys
                .ToArray()
                .Where(video => !File.Exists(Path.IsPathRooted(video) ? video : Path.Combine(Path.GetDirectoryName(jsonPath) ?? string.Empty, video)))
                .ForEach(video => group.Remove(video)));

        Dictionary<string, string> existingVideos = existingMetadata
            .Values
            .SelectMany(group => group.Keys)
            .ToDictionary(video => video, _ => string.Empty);

        Dictionary<string, Dictionary<string, VideoMetadata>> allVideoMetadata = directories
            .SelectMany(directory => Directory.GetFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .SelectMany(movieJson =>
            {
                string relativePath = Path.GetDirectoryName(jsonPath) ?? string.Empty;
                Imdb.TryLoad(movieJson, out ImdbMetadata? imdbMetadata);
                return Directory
                    .GetFiles(Path.GetDirectoryName(movieJson) ?? string.Empty, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                    .Where(video => video.IsCommonVideo() && !video.IsDiskImage() && !existingVideos.ContainsKey(Path.GetRelativePath(relativePath, video)))
                    .Select(video =>
                    {
                        if (!TryGetVideoMetadata(video, out VideoMetadata? videoMetadata, imdbMetadata, relativePath))
                        {
                            log($"!Fail: {video}");
                        }

                        return videoMetadata;
                    })
                    .NotNull();
            })
            .ToLookup(videoMetadata => videoMetadata.Imdb?.ImdbId ?? string.Empty, metadata => metadata)
            .ToDictionary(group => group.Key, group => group.ToDictionary(videoMetadata => videoMetadata.File, videoMetadata => videoMetadata));

        allVideoMetadata.ForEach(
            group =>
            {
                if (!existingMetadata.ContainsKey(group.Key))
                {
                    existingMetadata[group.Key] = new();
                }

                group.Value.ForEach(pair => existingMetadata[group.Key][pair.Key] = pair.Value);
            });

        existingMetadata
            .Keys
            .ToArray()
            .Where(imdbId => !existingMetadata[imdbId].Any())
            .ForEach(imdbId => existingMetadata.Remove(imdbId));

        string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
    }

    internal static async Task SaveExternalVideoMetadataAsync(string jsonPath, params string[] directories)
    {
        Dictionary<string, VideoMetadata> allVideoMetadata = directories
            .SelectMany(directory => Directory.GetFiles(directory, VideoSearchPattern, SearchOption.AllDirectories))
            .Select(video =>
            {
                string metadata = PathHelper.ReplaceExtension(video, XmlMetadataExtension);
                string imdbId = XDocument.Load(metadata).Root?.Element("imdbid")?.Value ?? throw new InvalidOperationException(video);
                if (TryGetVideoMetadata(video, out VideoMetadata? videoMetadata))
                {
                    return (ImdbId: imdbId, Value: videoMetadata);
                }

                throw new InvalidOperationException(video);
            })
            .Where(metadata => metadata.Value is not null)
            .Distinct(metadata => metadata.ImdbId)
            .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value!);

        string mergedVideoMetadataJson = JsonSerializer.Serialize(allVideoMetadata, new JsonSerializerOptions() { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
    }

    internal static bool IsCommonVideo(this string file) => file.HasAnyExtension(CommonVideoExtensions);

    internal static bool IsVideo(this string file) => file.HasAnyExtension(AllVideoExtensions);

    internal static bool IsTextSubtitle(this string file) => file.HasAnyExtension(TextSubtitleExtensions);

    internal static bool IsSubtitle(this string file) => file.HasAnyExtension(AllSubtitleExtensions);

    internal static bool IsDiskImage(this string file) => file.HasExtension(DiskImageExtension);
}