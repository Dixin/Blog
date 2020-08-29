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
                .Where(file => predicate?.Invoke(file) ?? true && AllVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)));
        }

        private static VideoMetadata GetVideoMetadata(string file, int retryCount = 10, Action<string>? log = null)
        {
            log ??= TraceLog;
            Task<VideoMetadata> task = Task.Run(() =>
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
                                File = file,
                                Width = videoStream.Width,
                                Height = videoStream.Height,
                                Audio = mediaInfo.AudioStreams?.Count() ?? 0,
                                Subtitle = mediaInfo.SubtitleStreams?.Count() ?? 0,
                                AudioBitRates = mediaInfo.AudioStreams?.Select(audio => (int)audio.Bitrate).ToArray() ?? Array.Empty<int>(),
                                Duration = mediaInfo.Duration
                            };
                        },
                        retryCount,
                        exception => true);
                }
                catch (Exception exception)
                {
                    log($"Fail {file} {exception}");
                    return new VideoMetadata()
                    {
                        File = file,
                        Width = -1,
                        Height = -1,
                        Audio = -1,
                        Subtitle = -1
                    };
                }
            });
            if (task.Wait(TimeSpan.FromSeconds(20)))
            {
                log($"{task.Result.Width}x{task.Result.Height}, {task.Result.Audio} audio, {file}");
                return task.Result;
            }

            log($"Timeout {file}");
            return new VideoMetadata()
            {
                File = file,
                Width = -1,
                Height = -1,
                Audio = -1,
                Subtitle = -1
            };
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

            return directories;
        }

        private static readonly string[] VersionKeywords = { "RARBG", "VXT" };

        internal static bool HasVersionKeywords(this string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            return VersionKeywords.Any(keyword => name.Contains(keyword, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static async Task DownloadImdbMetadataAsync(string directory, int level = 2, bool overwrite = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            await EnumerateDirectories(directory, level)
                .ForEachAsync(async movie =>
                {
                    if (!overwrite && Directory.EnumerateFiles(movie, "*.json", SearchOption.TopDirectoryOnly).Any())
                    {
                        log($"Skip {movie}.");
                        return;
                    }

                    string nfo = Directory.EnumerateFiles(movie, MetadataSearchPattern, SearchOption.TopDirectoryOnly).First();
                    string? imdbId = XDocument.Load(nfo).Root?.Element("imdbid")?.Value;
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
                            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true });
                        year = imdbMetadata.YearOfCurrentRegion;
                    }
                    
                    Debug.Assert(!string.IsNullOrWhiteSpace(year));
                    Debug.Assert(regions.Any());
                    string json = Path.Combine(movie, $"{imdbId}.{year}.{string.Join(",", regions)}.json");
                    log($"Downloaded https://www.imdb.com/title/{imdbId} to {json}.");
                    await File.WriteAllTextAsync(json, imdbJson);
                    log($"Saved to {json}.");
                });
        }

        internal static async Task SaveAllVideoMetadata(string jsonPath, params string[] directories)
        {
            Dictionary<string, VideoMetadata> existingMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(jsonPath));
            existingMetadata.Keys.ToArray()
                .Where(imdbId => !File.Exists(existingMetadata[imdbId].File))
                .ForEach(imdbId => existingMetadata.Remove(imdbId));

            Dictionary<string, VideoMetadata> allVideoMetadata = directories
                .SelectMany(directory => Directory.GetFiles(directory, "tt*.json", SearchOption.AllDirectories))
                .Select(json => (ImdbId: Path.GetFileNameWithoutExtension(json), Json: json))
                .Distinct(json => json.ImdbId)
                .Select(json =>
                {
                    string movie = Path.GetDirectoryName(json.Json);
                    if (!movie.Contains("1080", StringComparison.InvariantCulture))
                    {
                        return (json.ImdbId, Value: (VideoMetadata?)null);
                    }

                    string[] videos = Directory
                        .GetFiles(movie, "*", SearchOption.TopDirectoryOnly)
                        .Where(file => file.IsCommonVideo() && file.Contains("1080"))
                        .ToArray();
                    if (!videos.Any())
                    {
                        return (json.ImdbId, null);
                    }

                    string video = videos.FirstOrDefault(file => file.HasVersionKeywords()) ?? videos.First();

                    if (existingMetadata.ContainsKey(json.ImdbId) && string.Equals(video, existingMetadata[json.ImdbId].File, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (json.ImdbId, null);
                    }

                    VideoMetadata videoMetadata = GetVideoMetadata(video);
                    if (!(videoMetadata.Width > 0 && videoMetadata.Height > 0 && videoMetadata.Audio > 0 && videoMetadata.Subtitle >= 0 && videoMetadata.Duration > TimeSpan.Zero))
                    {
                        return (json.ImdbId, null);
                    }

                    return (json.ImdbId, Value: videoMetadata);
                })
                .Where(metadata => metadata.Value != null)
                .Distinct(metadata => metadata.ImdbId)
                .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value!);

            allVideoMetadata.ForEach(metadata => existingMetadata[metadata.Key] = metadata.Value);

            string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
        }

        internal static async Task SaveExternalVideoMetadataAsync(string jsonPath, params string[] directories)
        {
            Dictionary<string, VideoMetadata> existingMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(jsonPath));

            Dictionary<string, VideoMetadata> allVideoMetadata = directories
                .SelectMany(directory => Directory.GetFiles(directory, VideoSearchPattern, SearchOption.AllDirectories))
                .Select(video =>
                {
                    string metadata = video.Replace(VideoSearchPattern, MetadataExtension, StringComparison.InvariantCultureIgnoreCase);
                    string imdbId = XDocument.Load(metadata).Root.Element("imdbid").Value;
                    if (existingMetadata.ContainsKey(imdbId) && string.Equals(existingMetadata[imdbId].File, video, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (ImdbId: imdbId, Value: (VideoMetadata?)null);
                    }


                    VideoMetadata videoMetadata = GetVideoMetadata(video);
                    if (!(videoMetadata.Width > 0 && videoMetadata.Height > 0 && videoMetadata.Audio > 0 && videoMetadata.Subtitle >= 0 && videoMetadata.Duration > TimeSpan.Zero))
                    {
                        return (imdbId, Value: null);
                    }

                    return (ImdbId: imdbId, Value: videoMetadata);
                })
                .Where(metadata => metadata.Value != null)
                .Distinct(metadata => metadata.ImdbId)
                .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value!);

            allVideoMetadata.ForEach(metadata => existingMetadata[metadata.Key] = metadata.Value);

            string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
        }

        private static bool HasExtension(this string file, string extension)
        {
            return file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static bool AnyExtension(this string file, IEnumerable<string> extensions)
        {
            return extensions.Any(file.HasExtension);
        }

        internal static bool IsCommonVideo(this string file)
        {
            return file.AnyExtension(CommonVideoExtensions);
        }

        internal static bool IsTextSubtitle(this string file)
        {
            return file.AnyExtension(TextSubtitleExtensions);
        }

        internal static bool IsSubtitle(this string file)
        {
            return file.AnyExtension(AllSubtitleExtensions);
        }
    }
}