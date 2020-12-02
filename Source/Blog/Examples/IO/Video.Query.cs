namespace Examples.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Examples.Linq;
    using Examples.Net;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Xabe.FFmpeg;

    using JsonReaderException = Newtonsoft.Json.JsonReaderException;

    internal static partial class Video
    {
        private static IEnumerable<string> EnumerateVideos(string directory, Func<string, bool>? predicate = null)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(file => (predicate?.Invoke(file) ?? true) && file.HasAnyExtension(AllVideoExtensions));
        }

        internal static bool TryGetVideoMetadata(string file, out VideoMetadata? videoMetadata, ImdbMetadata? imdbMetadata = null, string? relativePath = null, int retryCount = 10, Action<string>? log = null)
        {
            log ??= TraceLog;
            Task<VideoMetadata?> task = Task.Run(() =>
            {
                try
                {
                    return Retry.Incremental(
                        () =>
                        {
                            IMediaInfo mediaInfo = FFmpeg.GetMediaInfo(file).Result;
                            IVideoStream videoStream = mediaInfo.VideoStreams.First();
                            return new VideoMetadata()
                            {
                                File = string.IsNullOrWhiteSpace(relativePath) ? file : Path.GetRelativePath(relativePath, file),
                                Width = videoStream.Width,
                                Height = videoStream.Height,
                                Audio = mediaInfo.AudioStreams?.Count() ?? 0,
                                Subtitle = mediaInfo.SubtitleStreams?.Count() ?? 0,
                                AudioBitRates = mediaInfo.AudioStreams?.Select(audio => (int)audio.Bitrate).ToArray() ?? Array.Empty<int>(),
                                TotalMilliseconds = mediaInfo.Duration.TotalMilliseconds,
                                Imdb = imdbMetadata
                            };
                        },
                        retryCount,
                        exception => true);
                }
                catch (Exception exception)
                {
                    log($"!Fail {file} {exception}");
                    return null;
                }
            });
            if (task.Wait(TimeSpan.FromSeconds(30)))
            {
                if ((videoMetadata = task.Result) is null)
                {
                    return false;
                }

                log($"{videoMetadata.Width}x{videoMetadata.Height}, {videoMetadata.Audio} audio, {file}");
                return true;
            }

            log($"!Timeout {file}");
            videoMetadata = null;
            return false;
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
                    catch (AggregateException exception) when (typeof(JsonReaderException) == exception.InnerException?.GetType())
                    {
                        log($"Fail {file} {exception}");
                        return -1;
                    }
                    catch (AggregateException exception) when (typeof(ArgumentException) == exception.InnerException?.GetType())
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

        private static string? GetVideoError(VideoMetadata videoMetadata, bool isNoAudioAllowed)
        {
            if (videoMetadata.Width <= 0 || videoMetadata.Height <= 0 || (isNoAudioAllowed ? videoMetadata.Audio < 0 : videoMetadata.Audio <= 0))
            {
                return $"Failed {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.Audio}Audio {videoMetadata.File}";
            }

            string fileName = Path.GetFileNameWithoutExtension(videoMetadata.File) ?? string.Empty;
            if (fileName.Contains("1080p"))
            {
                if (videoMetadata.Height < 1070 && videoMetadata.Width < 1900)
                {
                    return $"!Not 1080p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}";
                }
            }
            else
            {
                if (videoMetadata.Height >= 1070 || videoMetadata.Width >= 1900)
                {
                    return $"!1080p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}";
                }

                if (fileName.Contains("720p"))
                {
                    if (videoMetadata.Height < 720 && videoMetadata.Width < 1280)
                    {
                        return $"!Not 720p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}";
                    }
                }
                else
                {
                    if (videoMetadata.Height >= 720 || videoMetadata.Width >= 1280)
                    {
                        return $"!720p: {videoMetadata.Width}x{videoMetadata.Height} {videoMetadata.File}";
                    }
                }
            }

            if (Regex.IsMatch(fileName, "[1-9]Audio"))
            {
                if (videoMetadata.Audio < 2)
                {
                    return $"!Not multiple audio: {videoMetadata.Audio} {videoMetadata.File}";
                }
            }
            else
            {
                if (videoMetadata.Audio >= 2)
                {
                    return $"!Multiple audio: {videoMetadata.Audio} {videoMetadata.File}";
                }
            }

            return videoMetadata.AudioBitRates.Any(bitRate => bitRate < 192000)
                ? $"!Bad audio: Bit rate is only {string.Join(',', videoMetadata.AudioBitRates)} {videoMetadata.File}"
                : null;
        }



        internal static IEnumerable<string> EnumerateDirectories(string directory, int level = 2)
        {
            IEnumerable<string> directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            while (--level > 0)
            {
                directories = directories.SelectMany(d => Directory.EnumerateDirectories(d, "*", SearchOption.TopDirectoryOnly));
                level--;
            }

            return directories.OrderBy(movie => movie);
        }

        internal static async Task DownloadImdbMetadataAsync(string directory, int level = 2, bool overwrite = false, bool isTV = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            await EnumerateDirectories(directory, level)
                .ParallelForEachAsync(async movie =>
                {
                    if (!overwrite && Directory.EnumerateFiles(movie, "*.json", SearchOption.TopDirectoryOnly).Any())
                    {
                        log($"Skip {movie}.");
                        return;
                    }

                    string? nfo = Directory.EnumerateFiles(movie, MetadataSearchPattern, SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(nfo))
                    {
                        log($"!Missing metadata {movie}.");
                        return;
                    }

                    string? imdbId = XDocument.Load(nfo).Root?.Element((isTV ? "imdb_id" : "imdbid")!)?.Value;
                    if (string.IsNullOrWhiteSpace(imdbId))
                    {
                        await File.WriteAllTextAsync(Path.Combine(movie, "-.json"), "{}");
                        return;
                    }

                    (string imdbJson, string year, string[] regions) = await Retry.FixedIntervalAsync(async () => await Imdb.DownloadJsonAsync($"https://www.imdb.com/title/{imdbId}"), retryCount: 10);
                    Debug.Assert(!string.IsNullOrWhiteSpace(imdbJson));
                    if (string.IsNullOrWhiteSpace(year))
                    {
                        ImdbMetadata imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                            imdbJson,
                            new() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true }) ?? throw new InvalidOperationException(imdbJson);
                        year = imdbMetadata.YearOfCurrentRegion;
                    }

                    if (string.IsNullOrWhiteSpace(year))
                    {
                        log($"!Year is missing for {imdbId}: {movie}");
                    }
                    if (!regions.Any())
                    {
                        log($"!Location is missing for {imdbId}: {movie}");
                    }
                    string json = Path.Combine(movie, $"{imdbId}.{year}.{string.Join(",", regions.Take(5))}.json");
                    log($"Downloaded https://www.imdb.com/title/{imdbId} to {json}.");
                    await File.WriteAllTextAsync(json, imdbJson);
                    log($"Saved to {json}.");
                }, MaxDegreeOfParallelism);
        }

        internal static async Task SaveAllVideoMetadata(string jsonPath, Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
            Dictionary<string, Dictionary<string, VideoMetadata>> existingMetadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, VideoMetadata>>>(await File.ReadAllTextAsync(jsonPath))
                ?? throw new InvalidOperationException(jsonPath);

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
                .ToDictionary(video => video, video => string.Empty);

            Dictionary<string, Dictionary<string, VideoMetadata>> allVideoMetadata = directories
                .SelectMany(directory => Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories))
                .AsParallel()
                .WithDegreeOfParallelism(MaxDegreeOfParallelism)
                .SelectMany(movieJson =>
                {
                    string relativePath = Path.GetDirectoryName(jsonPath) ?? string.Empty;
                    Imdb.TryLoad(movieJson, out ImdbMetadata? imdbMetadata);
                    return Directory
                        .GetFiles(Path.GetDirectoryName(movieJson) ?? string.Empty, AllSearchPattern, SearchOption.TopDirectoryOnly)
                        .Where(video => video.IsCommonVideo() && !existingVideos.ContainsKey(Path.GetRelativePath(relativePath, video)))
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

            string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
        }

        internal static async Task SaveExternalVideoMetadataAsync(string jsonPath, params string[] directories)
        {
            Dictionary<string, VideoMetadata> allVideoMetadata = directories
                .SelectMany(directory => Directory.GetFiles(directory, VideoSearchPattern, SearchOption.AllDirectories))
                .Select(video =>
                {
                    string metadata = PathHelper.ReplaceExtension(video, MetadataExtension);
                    string imdbId = XDocument.Load(metadata).Root?.Element("imdbid")?.Value ?? throw new InvalidOperationException(video);
                    if (TryGetVideoMetadata(video, out VideoMetadata? videoMetadata))
                    {
                        return (ImdbId: imdbId, Value: videoMetadata);
                    }

                    throw new InvalidOperationException(video);
                })
                .Where(metadata => metadata.Value != null)
                .Distinct(metadata => metadata.ImdbId)
                .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value!);

            string mergedVideoMetadataJson = JsonSerializer.Serialize(allVideoMetadata, new() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
        }

        internal static bool IsCommonVideo(this string file)
        {
            return file.HasAnyExtension(CommonVideoExtensions);
        }

        internal static bool IsTextSubtitle(this string file)
        {
            return file.HasAnyExtension(TextSubtitleExtensions);
        }

        internal static bool IsSubtitle(this string file)
        {
            return file.HasAnyExtension(AllSubtitleExtensions);
        }
    }
}