namespace Examples.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Examples.Linq;
    using Examples.Net;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Xabe.FFmpeg;
    using Xabe.FFmpeg.Streams;

    using JsonReaderException = Newtonsoft.Json.JsonReaderException;

    internal static class Video
    {
        private static readonly string[] UncommonVideoSearchPatterns = { "*.avi", "*.wmv", "*.webm", "*.mpg", "*.mpeg", "*.rmvb", "*.rm", "*.3gp", "*.divx", "*.m1v", "*.mov", "*.ts", "*.vob", "*.flv", "*.m4v", "*.mkv" };

        private static readonly string[] CommonVideoSearchPatterns = { "*.avi", "*.mp4" };

        private static readonly string[] CommonVideoExtensions = { ".avi", ".mp4", ".mkv", ".iso" };

        private static readonly string[] AllVideoSearchPattern = UncommonVideoSearchPatterns.Union(CommonVideoSearchPatterns).ToArray();

        private static readonly string[] TextExtensions = { ".srt", ".ass", ".ssa", ".txt" };

        private static readonly string[] TextSubtitleSearchPatterns = { "*.srt", "*.ass", "*.ssa", "*.txt" };

        private static readonly string[] SubtitleExtensions = { ".srt", ".ass", ".ssa", ".idx", ".sub", ".sup" };

        private static void TraceLog(string message) => Trace.WriteLine(message);

        private static readonly Encoding Utf8Encoding = new UTF8Encoding(true);

        private static readonly byte[] Bom = Utf8Encoding.GetPreamble();

        internal static void RenameFiles(string path, Func<string, int, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetFiles(path, pattern ?? "*", searchOption ?? SearchOption.AllDirectories)
                .Where(file => predicate?.Invoke(file) ?? true)
                .ForEach((file, index) =>
                {
                    string newFile = rename(file, index);
                    if (!string.Equals(file, newFile))
                    {
                        log(file);
                        if (!isDryRun)
                        {
                            FileHelper.Move(file, newFile, overwrite);
                        }
                        log(newFile);
                    }
                });
        }

        internal static void RenameDirectories(string path, Func<string, string> rename, string? pattern = null, SearchOption? searchOption = null, Func<string, bool>? predicate = null, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetDirectories(path, pattern ?? "*", searchOption ?? SearchOption.AllDirectories)
                .Where(directory => predicate?.Invoke(directory) ?? true)
                .ForEach(directory =>
                {
                    string newDirectory = rename(directory);
                    if (!string.Equals(directory, newDirectory))
                    {
                        log(directory);
                        Directory.Move(directory, newDirectory);
                        log(newDirectory);
                    }
                });
        }

        private static void Convert(Encoding from, Encoding to, string fromPath, string? toPath = null, byte[]? bom = null)
        {
            byte[] fromBytes = File.ReadAllBytes(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            if (bom != null && !bom.SequenceEqual(toBytes.Take(bom.Length)))
            {
                toBytes = bom.Concat(toBytes).ToArray();
            }
            File.WriteAllBytes(toPath ?? fromPath, toBytes);
        }

        internal static (string? Charset, float? Confidence, string File)[] GetSubtitles(string directory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Directory
                .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(file => TextExtensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
                .Select(file =>
                {
                    using FileStream fileStream = File.OpenRead(file);
                    Ude.CharsetDetector detector = new Ude.CharsetDetector();
                    detector.Feed(fileStream);
                    detector.DataEnd();
                    return detector.Charset != null ? (detector.Charset, Confidence: (float?)detector.Confidence, File: file) : (Charset: (string?)null, Confidence: (float?)null, File: file);
                })
                .OrderBy(result => result.Charset)
                .ThenByDescending(result => result.Confidence)
                .ToArray();
        }

        internal static void DeleteSubtitle(string directory, bool isDryRun = false, Action<string>? log = null, params string[] encodings)
        {
            log ??= TraceLog;
            GetSubtitles(directory)
                .Where(result => encodings.Any(encoding => string.Equals(encoding, result.Charset, StringComparison.OrdinalIgnoreCase)))
                .ForEach(result =>
                {
                    if (!isDryRun)
                    {
                        File.Delete(result.File);
                    }
                    log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
                });
        }

        internal static void ConvertToUtf8(string directory, bool backup = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetSubtitles(directory)
                .Where(result =>
                {
                    if (result.Charset?.ToUpperInvariant() != "UTF-8")
                    {
                        return true;
                    }
                    using FileStream stream = File.OpenRead(result.File);
                    byte[] head = new byte[Bom.Length];
                    int read = stream.Read(head, 0, head.Length);
                    return !(read == Bom.Length && Bom.SequenceEqual(head));
                })
                .ForEach(result =>
                {
                    try
                    {
                        Encoding encoding;
                        switch (result.Charset?.ToUpperInvariant())
                        {
                            case "UTF-16LE":
                                encoding = Encoding.Unicode;
                                break;
                            case "GB18030":
                                encoding = Encoding.GetEncoding("gb18030");
                                break;
                            case "WINDOWS-1251":
                                encoding = Encoding.GetEncoding(1251);
                                break;
                            case "WINDOWS-1252":
                                encoding = Encoding.GetEncoding(1252);
                                break;
                            case "BIG5":
                                encoding = Encoding.GetEncoding("big5");
                                break;
                            case "ASCII":
                                encoding = Encoding.ASCII;
                                break;
                            case "UTF-8":
                                encoding = Encoding.UTF8;
                                break;
                            default:
                                log($"Not supported {result.Item1}, file {result.Item3}");
                                return;
                        }

                        FileInfo fileInfo = new FileInfo(result.Item3);
                        if (fileInfo.IsReadOnly)
                        {
                            fileInfo.IsReadOnly = false;
                        }
                        if (backup)
                        {
                            BackupFile(result.Item3);
                        }
                        Convert(encoding, Utf8Encoding, result.File, null, Bom);
                        log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
                    }
                    catch (Exception exception)
                    {
                        log($"{result.Item3} {exception}");
                    }
                });

            static void BackupFile(string file)
            {
                string backUp = file + ".bak";
                if (File.Exists(backUp))
                {
                    FileInfo backupFile = new FileInfo(backUp);
                    if (backupFile.IsReadOnly)
                    {
                        backupFile.IsReadOnly = false;
                    }
                }

                File.Copy(file, backUp, true);
            }
        }

        private static IEnumerable<string> GetVideos(string directory, Func<string, bool>? predicate = null)
        {
            return AllVideoSearchPattern
                .SelectMany(pattern => Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories))
                .Where(file => predicate?.Invoke(file) ?? true);
        }

        private static (string FullPath, string FileName, int Width, int Height, int Audio, int Subtitle, int[] AudioBitrates, TimeSpan Duration) GetVideoMetadata(string file, int retryCount = 10, Action<string>? log = null)
        {
            log ??= TraceLog;
            Task<(string FullPath, string FileName, int Width, int Height, int Audio, int Subtitle, int[] AudioBitrates, TimeSpan Duration)> task = Task.Run(() =>
            {

                try
                {
                    return Retry.Incremental(
                        () =>
                        {
                            IMediaInfo mediaInfo = MediaInfo.Get(file).Result;
                            IVideoStream videoStream = mediaInfo.VideoStreams.First();
                            return (file, Path.GetFileName(file), videoStream.Width, videoStream.Height, mediaInfo.AudioStreams?.Count() ?? 0, mediaInfo.SubtitleStreams?.Count() ?? 0, mediaInfo.AudioStreams?.Select(audio => (int)audio.Bitrate).ToArray() ?? Array.Empty<int>(), mediaInfo.Duration);
                        },
                        retryCount,
                        exception => true);
                }
                catch (Exception exception)
                {
                    log($"Fail {file} {exception}");
                    return (file, Path.GetFileName(file), -1, -1, -1, -1, Array.Empty<int>(), TimeSpan.MinValue);
                }
            });
            if (task.Wait(TimeSpan.FromSeconds(20)))
            {
                log($"{task.Result.Width}x{task.Result.Height}, {task.Result.Audio} audio, {file}");
                return task.Result;
            }

            log($"Timeout {file}");
            return (file, Path.GetFileName(file), -1, -1, -1, -1, Array.Empty<int>(), TimeSpan.MinValue);
        }

        private static int GetAudioMetadata(string file, Action<string>? log = null)
        {
            log ??= TraceLog;
            Task<int> task = Task.Run(() =>
            {
                try
                {
                    IMediaInfo mediaInfo = MediaInfo.Get(file).Result;
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

        private static string? GetVideoError((string FullPath, string FileName, int Width, int Height, int Audio, int Subtitle, int[] AudioBitrates, TimeSpan Duration) video, bool isNoAudioAllowed)
        {
            if (video.Width <= 0 || video.Height <= 0 || (isNoAudioAllowed ? video.Audio < 0 : video.Audio <= 0))
            {
                return $"Failed {video.Width}x{video.Height} {video.Audio}Audio {video.FullPath}";
            }

            string fileName = Path.GetFileNameWithoutExtension(video.FileName) ?? string.Empty;
            if (fileName.Contains("1080p"))
            {
                if (video.Height < 1070 && video.Width < 1900)
                {
                    return $"!Not 1080p: {video.Width}x{video.Height} {video.FullPath}";
                }
            }
            else
            {
                if (video.Height >= 1070 || video.Width >= 1900)
                {
                    return $"!1080p: {video.Width}x{video.Height} {video.FullPath}";
                }
                else
                {
                    if (fileName.Contains("720p"))
                    {
                        if (video.Height < 720 && video.Width < 1280)
                        {
                            return $"!Not 720p: {video.Width}x{video.Height} {video.FullPath}";
                        }
                    }
                    else
                    {
                        if (video.Height >= 720 || video.Width >= 1280)
                        {
                            return $"!720p: {video.Width}x{video.Height} {video.FullPath}";
                        }
                    }
                }
            }

            if (Regex.IsMatch(fileName, "[1-9]Audio"))
            {
                if (video.Audio < 2)
                {
                    return $"!Not multiple audio: {video.Audio} {video.FullPath}";
                }
            }
            else
            {
                if (video.Audio >= 2)
                {
                    return $"!Multiple audio: {video.Audio} {video.FullPath}";
                }
            }

            return video.AudioBitrates.Any(bitRate => bitRate < 192000)
                ? $"!Bad audio: Bit rate is only {string.Join(',', video.AudioBitrates)} {video.FullPath}"
                : null;
        }

        internal static void PrintVideoError(string directory, bool isNoAudioAllowed = false, string? pattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, Action<string>? log = null)
        {
            PrintVideosWithError(string.IsNullOrWhiteSpace(pattern) ? GetVideos(directory, predicate) : Directory.EnumerateFiles(directory, pattern, searchOption).Where(file => predicate?.Invoke(file) ?? true), isNoAudioAllowed, log);
        }

        internal static void PrintVideosWithError(IEnumerable<string> files, bool isNoAudioAllowed = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            files
                .ToArray()
                .AsParallel()
                .Select((video, index) => GetVideoMetadata(video, log: message => log($"{index} {message}")))
                .Select(video => (Video: video, Error: GetVideoError(video, isNoAudioAllowed)))
                .Where(result => !string.IsNullOrWhiteSpace(result.Error))
                .AsSequential()
                .OrderBy(result => result.Video.FullPath)
                .ForEach(result => log(result.Error ?? string.Empty));
        }

        internal static void PrintVideoErrorsFast(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetVideos(directory)
                .OrderBy(video => video)
                .ForEach(video =>
                {
                    if (!Regex.IsMatch(Path.GetFileNameWithoutExtension(video) ?? string.Empty, "[1-9]Audio", RegexOptions.IgnoreCase))
                    {
                        int audio = GetAudioMetadata(video, _ => { });
                        if (audio != 1)
                        {
                            log($"!! {audio} audio: {video}");
                        }
                    }
                });
        }

        private static readonly string MovieDirectory = @"^[^\.]+\.([0-9]{4})\..+\[([0-9]\.[0-9]|\-)\](\[[0-9]{3,4}p\](\[3D\])?)?$";

        internal static void PrintDirectoriesWithErrors(string directory, int level = 2, bool isTV = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(d =>
                {
                    string movie = Path.GetFileName(d) ?? throw new InvalidOperationException(d);
                    if (movie.StartsWith("0."))
                    {
                        movie = movie.Substring("0.".Length);
                    }

                    if (movie.Contains("{"))
                    {
                        movie = movie.Substring(0, movie.IndexOf("{", StringComparison.Ordinal));
                    }
                    return !Regex.IsMatch(movie, MovieDirectory)
                        || movie.Contains("1080p", StringComparison.InvariantCultureIgnoreCase) != Directory.GetFiles(d, "*", isTV ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Any(file => file.Contains("1080p", StringComparison.InvariantCultureIgnoreCase))
                        || movie.Contains("720p", StringComparison.InvariantCultureIgnoreCase) != Directory.GetFiles(d, "*", isTV ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Any(file => file.Contains("720p", StringComparison.InvariantCultureIgnoreCase));
                })
                .ForEach(log);
        }

        internal static IEnumerable<string> GetDirectories(string directory, int level = 2)
        {
            IEnumerable<string> directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            while (--level > 0)
            {
                directories = directories.SelectMany(d => Directory.EnumerateDirectories(d, "*", SearchOption.TopDirectoryOnly));
                level--;
            }

            return directories;
        }

        internal static void RenameVideosWithMultipleAudio(IEnumerable<string> files, Action<string> log)
        {
            files.ForEach(file =>
            {
                if (!File.Exists(file))
                {
                    log($"Not exist {file}");
                    return;
                }

                int audio = GetAudioMetadata(file, log);
                if (audio <= 1)
                {
                    log($"Audio {audio} {file}");
                    return;
                }

                List<string> info = (Path.GetFileName(file) ?? throw new InvalidOperationException(file)).Split(".").ToList();
                if (info.Count <= 3)
                {
                    // Space.
                    info = Path.GetFileNameWithoutExtension(file).Split(" ").Append(info.Last()).ToList();
                }

                string last = info[^2];
                if (last.IndexOf("handbrake", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    info.Insert(info.Count - 3, $"{audio}Audio");
                }
                else
                {
                    info.Insert(info.Count - 2, $"{audio}Audio");

                }
                string newFile = string.Join(".", info);
                string directory = Path.GetDirectoryName(file);
                log(file);
                log(Path.Combine(directory, newFile));
                File.Move(file, Path.Combine(directory, newFile));
                Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly)
                    .Where(attachment => attachment != file)
                    .Where(attachment => (Path.GetFileName(attachment) ?? throw new InvalidOperationException(file)).StartsWith(Path.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .ForEach(attachment =>
                    {
                        string newAttachment = Path.Combine(directory, (Path.GetFileName(attachment) ?? throw new InvalidOperationException(file)).Replace(Path.GetFileNameWithoutExtension(file), string.Join(".", info.SkipLast(1))));
                        log(newAttachment);
                        File.Move(attachment, newAttachment);
                    });
            });
        }

        internal static void PrintDirectoriesWithMultipleVideos(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(movie => CommonVideoSearchPatterns.SelectMany(pattern => Directory.EnumerateFiles(movie, pattern, SearchOption.TopDirectoryOnly)).Count() > 1)
                .ForEach(log);
        }

        internal static async Task ConvertAsync(Encoding from, Encoding to, string fromPath, string? toPath = null)
        {
            byte[] fromBytes = await File.ReadAllBytesAsync(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            await File.WriteAllBytesAsync(toPath ?? fromPath, toBytes);
        }

        internal static async Task CopyAsync(string fromPath, string toPath)
        {
            await using Stream fromStream = File.OpenRead(fromPath);
            await using Stream toStream = File.Create(toPath);
            await fromStream.CopyToAsync(toStream);
        }

        internal static void MoveSubtitles(string mediaDirectory, string mediaExtension, string subtitleDirectory)
        {
            Directory
                .EnumerateFiles(mediaDirectory, $"*{mediaExtension}", SearchOption.AllDirectories)
                .ForEach(video =>
                {
                    string match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    Directory
                        .EnumerateFiles(subtitleDirectory, $"*{match}*", SearchOption.TopDirectoryOnly)
                        .Where(file => !file.EndsWith(mediaExtension, StringComparison.OrdinalIgnoreCase))
                        .ForEach(subtitle =>
                        {
                            string language = (Path.GetFileNameWithoutExtension(subtitle) ?? throw new InvalidOperationException(subtitle)).Split(".").Last();
                            string newSubtitle = $"{Path.GetFileNameWithoutExtension(video)}.{language}{Path.GetExtension(subtitle)}";
                            FileHelper.Move(subtitle, Path.Combine(Path.GetDirectoryName(video), newSubtitle), true);
                        });
                });
        }

        private static string FilterTitleForFileSystem(this string value) => value.Replace("?", "").Replace(": ", "-").Replace("*", "_").Replace("/", "_");

        internal static void RenameEpisodesWithTitle(string nfoDirectory, string mediaDirectory, string searchPattern, Func<string, string, string> rename, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.EnumerateFiles(nfoDirectory, $"{searchPattern}.nfo", SearchOption.AllDirectories)
                .ToList()
                .ForEach(nfo =>
                {

                    string match = Regex.Match(Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException($"{nfo} is invalid."), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    string title = XDocument.Load(nfo).Root?.Element("title")?.Value.FilterTitleForFileSystem() ?? throw new InvalidOperationException($"{nfo} has no title.");
                    Directory
                        .EnumerateFiles(mediaDirectory, $"*{match}*", SearchOption.AllDirectories)
                        .ForEach(file =>
                        {
                            log(file);
                            string newFile = rename(file, title).Trim();
                            if (!isDryRun)
                            {
                                File.Move(file, newFile);
                            }
                            log(newFile);
                        });
                });
        }

        internal static void RenameDirectoriesWithDefinition(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ToList()
                .ForEach(movie =>
                {
                    string[] files = Directory.EnumerateFiles(movie, "*", SearchOption.TopDirectoryOnly).ToArray();
                    if (files.Any(file => file.IndexOf("1080", StringComparison.Ordinal) >= 0))
                    {
                        Directory.Move(movie, movie + "[1080p]");
                        return;
                    }

                    if (files.Any(file => file.IndexOf("720", StringComparison.Ordinal) >= 0))
                    {
                        Directory.Move(movie, movie + "[720p]");
                        return;
                    }
                    log(movie);
                });
        }

        internal static void PrintDirectoriesWithLowDefinition(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ForEach(movie =>
                {
                    List<string> files = Directory
                        .GetFiles(movie)
                        .Where(file => CommonVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                    if (files.All(file => !file.Contains("1080p")))
                    {
                        log(movie);
                    }
                });
        }

        internal static void PrintDirectoriesMultipleMedia(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;

            GetDirectories(directory, level)
                .ForEach(movie =>
                {
                    if (Directory.GetFiles(movie).Count(file => CommonVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase))) > 1)
                    {
                        log.Invoke(movie);
                    }
                });
        }

        internal static void PrintDirectoriesWithMissingVideo(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
            .Where(m => !Directory.EnumerateFiles(m, "*.mp4", SearchOption.TopDirectoryOnly).Any() && !Directory.EnumerateFiles(m, "*.avi", SearchOption.TopDirectoryOnly).Any())
            .ForEach(log);
        }

        internal static void RenameVideosWithDefinition(string[] files, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            files.ForEach(file => Debug.Assert(Regex.IsMatch(file, @"^\!(720p|1080p)\: ([0-9]{2,4}x[0-9]{2,4} )?(.*)$")));
            files
                .Select(file =>
                {
                    Match match = Regex.Match(file, @"^\!(720p|1080p)\: ([0-9]{2,4}x[0-9]{2,4} )?(.*)$");
                    string path = match.Groups.Last().Value;
                    return (Definition: match.Groups[1].Value, File: path, Extension: Path.GetExtension(Path.GetFileNameWithoutExtension(path)));
                })
                .ForEach(result =>
                {
                    string extension = Path.GetExtension(result.File);
                    string newFile;
                    if (!string.IsNullOrWhiteSpace(result.Extension) && AllVideoSearchPattern.Any(pattern => string.Equals(pattern.Substring(2), result.Extension.Substring(1), StringComparison.OrdinalIgnoreCase)))
                    {
                        string file = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(result.File));
                        newFile = Path.Combine(Path.GetDirectoryName(result.File), $"{file}.{result.Definition}{result.Extension}{extension}");

                    }
                    else
                    {
                        string file = Path.GetFileNameWithoutExtension(result.File);
                        newFile = Path.Combine(Path.GetDirectoryName(result.File), $"{file}.{result.Definition}{extension}");
                    }
                    if (isDryRun)
                    {
                        log(newFile);
                    }
                    else
                    {
                        if (File.Exists(result.File))
                        {
                            FileHelper.Move(result.File, newFile, true);
                        }
                    }
                });
        }

        internal static void MoveSubtitleToParentDirectory(string directory, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory
                .EnumerateFiles(directory, "*.idx", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(directory, "*.sub", SearchOption.AllDirectories))
                .ToArray()
                .Where(subtitle => "Subs".Equals(Path.GetFileName(Path.GetDirectoryName(subtitle)), StringComparison.OrdinalIgnoreCase))
                .ForEach(subtitle =>
                {
                    string parent = Path.GetDirectoryName(Path.GetDirectoryName(subtitle));
                    string[] videos = Directory.GetFiles(parent, "*.mp4", SearchOption.TopDirectoryOnly);
                    string mainVideo = videos.OrderByDescending(video => new FileInfo(video).Length).First();
                    string newSubtitle = Path.Combine(parent, Path.GetFileNameWithoutExtension(mainVideo) + Path.GetExtension(subtitle));
                    File.Move(subtitle, newSubtitle);
                });

            Directory
                .GetFiles(directory, "*.srt", SearchOption.AllDirectories)
                .Where(subtitle => "Subs".Equals(Path.GetFileName(Path.GetDirectoryName(subtitle)), StringComparison.OrdinalIgnoreCase))
                .ForEach(subtitle =>
                {
                    string parent = Path.GetDirectoryName(Path.GetDirectoryName(subtitle));
                    string[] videos = Directory.GetFiles(parent, "*.mp4", SearchOption.TopDirectoryOnly);
                    string mainVideo = videos.Length == 1
                        ? videos[0]
                        : videos.OrderByDescending(video => new FileInfo(video).Length).First();
                    string language =
                        (Path.GetFileNameWithoutExtension(subtitle) ?? throw new InvalidOperationException(subtitle))
                        .ToUpperInvariant();
                    string suffix = language switch
                    {
                        _ when language.Contains("ENG") => string.Empty,
                        _ when language.Contains("CHI") => ".chs",
                        _ => "." + language
                    };
                    string newSubtitle = Path.Combine(parent, Path.GetFileNameWithoutExtension(mainVideo) + suffix + ".srt");
                    log(subtitle);
                    if (!isDryRun)
                    {
                        if (File.Exists(newSubtitle))
                        {
                            if (string.IsNullOrEmpty(suffix))
                            {
                                long subtitleSize = new FileInfo(subtitle).Length;
                                long newSubtitleSize = new FileInfo(newSubtitle).Length;
                                if (subtitleSize >= newSubtitleSize)
                                {
                                    new FileInfo(newSubtitle).IsReadOnly = false;
                                    File.Delete(newSubtitle);
                                    File.Move(subtitle, newSubtitle);
                                }
                                else
                                {
                                    new FileInfo(subtitle).IsReadOnly = false;
                                    File.Delete(subtitle);
                                }
                            }
                            else
                            {
                                log($"!{subtitle}");
                            }
                        }
                        else
                        {
                            File.Move(subtitle, newSubtitle);
                        }
                    }

                    log(newSubtitle);
                });
        }

        private static readonly Regex[] PreferredVersions = new string[] { @"[\. ]YIFY(\+HI)?$", @"[\. ]YIFY(\.[1-9]Audio)?$", @"\[YTS\.[A-Z]{2}\](\.[1-9]Audio)?$", @"\-RARBG(\.[1-9]Audio)?$", @"\-VXT(\.[1-9]Audio)?$", @"\.GAZ$" }.Select(version => new Regex(version)).ToArray();

        internal static void PrintVideosNonPreferred(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ForEach(movie => Directory
                    .GetFiles(movie, "*.mp4", SearchOption.TopDirectoryOnly)
                    .Where(file => !PreferredVersions.Any(version => version.IsMatch(Path.GetFileNameWithoutExtension(file))))
                    .ForEach(log));
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(movie => Directory.GetFiles(movie).All(video => SubtitleExtensions.All(extension => !video.EndsWith(extension, StringComparison.OrdinalIgnoreCase))))
                .ForEach(log);
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string>? log = null, params string[] languages)
        {
            log ??= TraceLog;
            string[] searchPatterns = languages.SelectMany(language => SubtitleExtensions.Select(extension => $"*{language}*{extension}")).ToArray();
            GetDirectories(directory, level)
                .Where(movie => searchPatterns.All(searchPattern => !Directory.EnumerateFiles(movie, searchPattern, SearchOption.TopDirectoryOnly).Any()))
                .ForEach(log);
        }

        internal static void RenameDirectoriesWithMetadata(string directory, int level = 2, bool additionalInfo = false, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    string[] nfos = Directory.GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).OrderBy(nfo => nfo).ToArray();
                    XDocument english;
                    XDocument? chinese;
                    if (nfos.Any(nfo => nfo.EndsWith(".eng.nfo")))
                    {
                        english = XDocument.Load(nfos.First(nfo => nfo.EndsWith(".eng.nfo")));
                        chinese = XDocument.Load(nfos.First(nfo => !nfo.EndsWith(".eng.nfo")));
                    }
                    else
                    {
                        english = XDocument.Load(nfos.First());
                        chinese = null;
                    }

                    string json = Directory.GetFiles(movie, "*.json", SearchOption.TopDirectoryOnly).Single();
                    ImdbMetadata? imdbMetadata = null;
                    if (Path.GetFileNameWithoutExtension(json).Length > 1)
                    {
                        imdbMetadata = JsonSerializer.Deserialize<ImdbMetadata>(
                            File.ReadAllText(json),
                            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreReadOnlyProperties = true });
                    }

                    string englishTitle = english.Root?.Element("title")?.Value ?? throw new InvalidOperationException($"{movie} has no English title.");
                    string chineseTitle = chinese?.Root?.Element("title")?.Value ?? string.Empty;
                    string? originalTitle = english.Root?.Element("originaltitle")?.Value ?? imdbMetadata?.Name;
                    string year = imdbMetadata?.Year ?? english.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{movie} has no year.");
                    string? imdb = english.Root?.Element("imdbid")?.Value;
                    if (string.IsNullOrWhiteSpace(imdb))
                    {
                        Debug.Assert(string.Equals("-", Path.GetFileNameWithoutExtension(json), StringComparison.InvariantCulture));
                    }
                    else
                    {
                        Debug.Assert(string.Equals(imdb, Path.GetFileNameWithoutExtension(json), StringComparison.InvariantCultureIgnoreCase));
                    }
                    string rating = string.IsNullOrWhiteSpace(imdb)
                        ? "-"
                        : float.TryParse(imdbMetadata?.AggregateRating?.RatingValue, out float ratingFloat) ? ratingFloat.ToString("0.0") : "0.0";
                    string[] videos = Directory.GetFiles(movie, "*.mp4", SearchOption.TopDirectoryOnly).Concat(Directory.GetFiles(movie, "*.avi", SearchOption.TopDirectoryOnly)).ToArray();
                    string definition = videos switch
                    {
                        _ when videos.Any(video => Path.GetFileNameWithoutExtension(video)?.Contains("1080p") ?? false) => "[1080p]",
                        _ when videos.Any(video => Path.GetFileNameWithoutExtension(video)?.Contains("720p") ?? false) => "[720p]",
                        _ => string.Empty
                    };
                    originalTitle = string.Equals(originalTitle, englishTitle, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(originalTitle)
                        ? string.Empty
                        : $"={originalTitle}";
                    string additional = additionalInfo
                        ? $"{{{Path.GetFileNameWithoutExtension(Directory.GetFiles(movie, "*.region", SearchOption.TopDirectoryOnly).SingleOrDefault())?.Split('.')[1]};{string.Join(",", imdbMetadata?.Genre.Take(3))};{imdbMetadata?.ContentRating}}}"
                        : string.Empty;
                    string newMovie = $"{englishTitle.FilterTitleForFileSystem()}{originalTitle.FilterTitleForFileSystem()}.{year}.{chineseTitle.FilterTitleForFileSystem()}[{rating}]{definition}{additional}";
                    string newDirectory = Path.Combine(Path.GetDirectoryName(movie), newMovie);
                    if (isDryRun)
                    {
                        log(newDirectory);
                    }
                    else
                    {
                        if (!string.Equals(movie, newDirectory))
                        {
                            log(movie);
                            Directory.Move(movie, newDirectory);
                            log(newDirectory);
                        }
                    }
                });
        }

        internal static void CopyMetadata(string directory)
        {
            Directory
                .GetFiles(directory, "*.nfo", SearchOption.AllDirectories)
                .ForEach(nfo => File.Copy(nfo, Path.Combine(Path.GetDirectoryName(nfo), Path.GetFileNameWithoutExtension(nfo) + ".eng" + Path.GetExtension(nfo))));
        }

        internal static void RestoreMetadata(string directory)
        {
            Directory
                .GetFiles(directory, "*.nfo", SearchOption.AllDirectories)
                .Where(nfo => nfo.EndsWith(".eng.nfo"))
                .Where(nfo => File.Exists(nfo.Replace(".eng.nfo", ".nfo")))
                .ForEach(nfo => FileHelper.Move(nfo, Path.Combine(Path.GetDirectoryName(nfo), (Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException(nfo)).Replace(".eng", string.Empty) + Path.GetExtension(nfo)), true));
        }

        private static readonly string[] IndependentNfos = { "tvshow.nfo", "season.nfo" };

        internal static void PrintMetadataWithoutMedia(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.EnumerateFiles(directory, "*.nfo", SearchOption.AllDirectories)
                .Where(nfo => IndependentNfos.All(independent => !string.Equals(independent, Path.GetFileName(nfo), StringComparison.OrdinalIgnoreCase)))
                .Where(nfo => CommonVideoExtensions.Select(extension => Path.Combine(Path.GetDirectoryName(nfo), Path.GetFileNameWithoutExtension(nfo) + extension)).All(video => !File.Exists(video)))
                .ForEach(log);
        }

        internal static void PrintMetadataByGroup(string directory, int level = 2, string field = "genre", Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Select(movie => (movie, nfo: XDocument.Load(Directory.GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).First())))
                .Select(movie => (movie.movie, field: movie.nfo.Root?.Element(field)?.Value))
                .OrderBy(movie => movie.field)
                .ForEach(movie => log($"{movie.field}: {Path.GetFileName(movie.movie)}"));
        }

        internal static void PrintMetadataByDuplication(string directory, string field = "imdbid", Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetFiles(directory, "*.nfo", SearchOption.AllDirectories)
                .Select(nfo => (nfo, field: XDocument.Load(nfo).Root?.Element(field)?.Value))
                .GroupBy(movie => movie.field)
                .Where(group => group.Count() > 1)
                .ForEach(group => group.ForEach(movie => log($"{movie.field} - {movie.nfo}")));
        }

        internal static void MoveMovies(string destination, string directory, int level = 2, string field = "genre", string? value = null, bool isDryRun = false)
        {
            MoveMovies(
                (movie, metadata) => Path.Combine(destination, Path.GetFileName(movie)),
                directory,
                level,
                (movie, metadata) => string.Equals(value, metadata.Root?.Element(field)?.Value, StringComparison.OrdinalIgnoreCase),
                isDryRun);
        }

        internal static void MoveMovies(Func<string, XDocument, string> rename, string directory, int level = 2, Func<string, XDocument, bool>? predicate = null, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ToArray()
                .Select(movie => (Directory: movie, Metadata: XDocument.Load(Directory.GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).First())))
                .Where(movie => predicate?.Invoke(movie.Directory, movie.Metadata) ?? true)
                .ForEach(movie =>
                {
                    string newMovie = rename(movie.Directory, movie.Metadata);
                    log(movie.Directory);
                    if (!isDryRun)
                    {
                        Directory.Move(movie.Directory, newMovie);
                    }
                    log(newMovie);
                });
        }

        internal static void DeleteDuplication(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ForEach(movie => Directory
                    .GetFiles(movie, "*", SearchOption.TopDirectoryOnly)
                    .Select(file => (file, new FileInfo(file).Length))
                    .GroupBy(file => file.Length)
                    .Where(group => group.Count() > 1)
                    .ForEach(group => group
                        .OrderByDescending(file => file.file)
                        .Skip(1)
                        .ForEach(file =>
                        {
                            if (!isDryRun)
                            {
                                File.Delete(file.file);
                            }
                            log(file.file);
                        })));
        }

        private static readonly string[] CommonChinese = { "的", "是" };

        private static readonly string[] CommonEnglish = { " of ", " is " };

        internal static void RenameSubtitlesByLanguage(string directory, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            TextSubtitleSearchPatterns
                .SelectMany(pattern => Directory.GetFiles(directory, pattern, SearchOption.AllDirectories))
                .ToArray()
                .ForEach(file =>
                {
                    string content = File.ReadAllText(file);
                    List<string> languages = new List<string>();
                    if (CommonChinese.All(chinese => content.Contains(chinese)))
                    {
                        languages.Add("chs");
                    }
                    if (CommonEnglish.All(english => content.IndexOf(english, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        languages.Add("eng");
                    }
                    if (languages.Any())
                    {
                        if (Path.GetFileNameWithoutExtension(file).EndsWith(string.Join('&', languages)) || Path.GetFileNameWithoutExtension(file).EndsWith(string.Join('&', languages.AsEnumerable().Reverse())))
                        {
                            return;
                        }
                        string newFile = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.{string.Join('&', languages)}{Path.GetExtension(file)}");
                        log(file);
                        if (!isDryRun)
                        {
                            File.Move(file, newFile);
                        }
                        log(newFile);
                    }
                    else
                    {
                        log($"!Unkown: {file}");
                    }
                });
        }

        internal static void PrintYears(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    XDocument metadata = XDocument.Load(Directory.GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).First());
                    string movieDirectory = Path.GetFileName(movie);
                    if (movieDirectory.StartsWith("0."))
                    {
                        movieDirectory = movieDirectory.Substring("0.".Length);
                    }
                    Match match = Regex.Match(movieDirectory, MovieDirectory);
                    Debug.Assert(match.Success);
                    string directoryYear = match.Groups[1].Value;
                    string metadataYear = metadata.Root?.Element("year")?.Value ?? throw new InvalidOperationException($"{metadata} has no year.");
                    string videoName = string.Empty;
                    if (!(directoryYear == metadataYear
                        && CommonVideoSearchPatterns
                            .SelectMany(pattern => Directory.GetFiles(movie, pattern, SearchOption.TopDirectoryOnly))
                            .All(video => (videoName = Path.GetFileName(video) ?? throw new InvalidOperationException($"{video} is invalid.")).Contains(directoryYear))))
                    {
                        log($"Directory: {directoryYear}, Metadata {metadataYear}, Video: {videoName}, {movie}");
                    }
                });
        }

        internal static void PrintDirectoriesWithNonLatinOriginalTitle(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(movie => movie.Contains("="))
                .Where(movie => !Regex.IsMatch(movie.Split("=")[1], "^[a-z]{1}.", RegexOptions.IgnoreCase))
                .ForEach(log);
        }

        internal static void PrintDirectoriesWithDuplicatePictures(string directory, int level = 2, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(movie =>
                {
                    string[] files = Directory.GetFiles(movie, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
                    return files.Contains("poster.jpg", StringComparer.OrdinalIgnoreCase)
                        && files.Any(file => file.EndsWith("-poster.jpg", StringComparison.OrdinalIgnoreCase));
                })
                .ForEach(log);
        }

        internal static async Task DownloadImdbMetadataAsync(string directory, int level = 2, bool overwrite = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            await GetDirectories(directory, level)
                .ForEachAsync(async movie =>
                {
                    if (!overwrite && Directory.EnumerateFiles(movie, "*.json", SearchOption.TopDirectoryOnly).Any())
                    {
                        log($"Skip {movie}.");
                        return;
                    }

                    string nfo = Directory.EnumerateFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).First();
                    string? imdbId = XDocument.Load(nfo).Root?.Element("imdbid")?.Value;
                    if (string.IsNullOrWhiteSpace(imdbId))
                    {
                        await File.WriteAllTextAsync(Path.Combine(movie, "-.json"), "{}");
                        return;
                    }

                    string json = Path.Combine(movie, $"{imdbId}.json");
                    (string imdbJson, string[] regions) = await Retry.FixedIntervalAsync(async () => await Imdb.DownloadJsonAsync($"https://www.imdb.com/title/{imdbId}"), retryCount: 10);
                    Debug.Assert(!string.IsNullOrWhiteSpace(imdbJson));
                    log($"Downloaded https://www.imdb.com/title/{imdbId}.");
                    await File.WriteAllTextAsync(json, imdbJson);
                    await File.WriteAllTextAsync(Path.Combine(movie, $"{imdbId}.{string.Join(",", regions)}.region"), JsonSerializer.Serialize(regions));
                    log($"Saved to {json}.");
                });
        }

        private static readonly string[] VersionKeywords = { "RARBG", "VXT" };

        internal static bool HasVersionKeywords(this string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            return VersionKeywords.Any(keyword => name.Contains(keyword, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static async Task SaveAllVideoMetadata(string jsonPath, Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
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
                        .Where(file => CommonVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)) && file.Contains("1080"))
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

                    (string fullPath, _, int width, int height, int audio, int subtitle, _, TimeSpan duration) = GetVideoMetadata(video);
                    if (!(width > 0 && height > 0 && audio > 0 && subtitle >= 0 && duration > TimeSpan.Zero))
                    {
                        return (json.ImdbId, null);
                    }

                    return (json.ImdbId, Value: new VideoMetadata()
                    {
                        File = fullPath,
                        Width = width,
                        Height = height,
                        TotalSeconds = duration.TotalSeconds,
                        Audio = audio,
                        Subtitle = subtitle
                    });
                })
                .Where(metadata => metadata.Value != null)
                .Distinct(metadata => metadata.ImdbId)
                .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value!);

            allVideoMetadata.ForEach(metadata => existingMetadata[metadata.Key] = metadata.Value);

            string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
        }

        internal static async Task SaveExternalVideoMetadataAsync(string jsonPath, Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
            Dictionary<string, VideoMetadata> existingMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(jsonPath));

            Dictionary<string, VideoMetadata> allVideoMetadata = directories
                .SelectMany(directory => Directory.GetFiles(directory, "*.mp4", SearchOption.AllDirectories))
                .Select(video =>
                {
                    string metadata = video.Replace(".mp4", ".nfo", StringComparison.InvariantCultureIgnoreCase);
                    string imdbId = XDocument.Load(metadata).Root.Element("imdbid").Value;
                    if (existingMetadata.ContainsKey(imdbId) && string.Equals(existingMetadata[imdbId].File, video, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (ImdbId: imdbId, Value: (VideoMetadata?)null);
                    }


                    (string fullPath, _, int width, int height, int audio, int subtitle, _, TimeSpan duration) = GetVideoMetadata(video);
                    if (!(width > 0 && height > 0 && audio > 0 && subtitle >= 0 && duration > TimeSpan.Zero))
                    {
                        return (imdbId, Value: null);
                    }

                    return (ImdbId: imdbId, Value: new VideoMetadata()
                    {
                        File = fullPath,
                        Width = width,
                        Height = height,
                        TotalSeconds = duration.TotalSeconds,
                        Audio = audio,
                        Subtitle = subtitle
                    });
                })
                .Where(metadata => metadata.Value != null)
                .Distinct(metadata => metadata.ImdbId)
                .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value!);

            allVideoMetadata.ForEach(metadata => existingMetadata[metadata.Key] = metadata.Value);

            string mergedVideoMetadataJson = JsonSerializer.Serialize(existingMetadata, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, mergedVideoMetadataJson);
        }

        internal static async Task CompareAndMoveAsync(string externalJsonPath, string moviesJsonPath, string newDirectory, string deletedDirectory, Action<string>? log = null, bool isDryRun = false, bool moveAllAttachment = true)
        {
            log ??= TraceLog;
            Dictionary<string, VideoMetadata> externalMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(externalJsonPath));
            Dictionary<string, VideoMetadata> moviesMetadata = JsonSerializer.Deserialize<Dictionary<string, VideoMetadata>>(await File.ReadAllTextAsync(moviesJsonPath));

            externalMetadata
                .Where(externalVideo => File.Exists(externalVideo.Value.File))
                .ForEach(externalVideo =>
            {
                string externalMovie = Path.GetDirectoryName(externalVideo.Value.File);
                log($"Starting {externalMovie}");

                if (!moviesMetadata.TryGetValue(externalVideo.Key, out VideoMetadata videoMetadata))
                {
                    string newExternalMovie = Path.Combine(newDirectory, Path.GetFileName(Path.GetDirectoryName(externalVideo.Value.File)));
                    if (!isDryRun)
                    {
                        Directory.Move(externalMovie, newExternalMovie);
                    }

                    log($"Move external movie {externalMovie} to {newExternalMovie}");
                    return;
                }

                if (videoMetadata.File.Contains("265", StringComparison.InvariantCultureIgnoreCase) && (videoMetadata.File.Contains("rarbg", StringComparison.InvariantCultureIgnoreCase) || videoMetadata.File.Contains("vxt", StringComparison.InvariantCultureIgnoreCase)))
                {
                    log($"Video {videoMetadata.File} is H265.");
                    return;
                }

                if (!File.Exists(videoMetadata.File))
                {
                    log($"Video {videoMetadata.File} does not exist.");
                    return;
                }

                if ((videoMetadata.File.Contains("rarbg", StringComparison.InvariantCultureIgnoreCase) ||
                     videoMetadata.File.Contains("vxt", StringComparison.InvariantCultureIgnoreCase)) &&
                    !(externalVideo.Value.File.Contains("rarbg", StringComparison.InvariantCultureIgnoreCase) ||
                      externalVideo.Value.File.Contains("vxt", StringComparison.InvariantCultureIgnoreCase)))
                {
                    log($"Video {videoMetadata.File} is better version.");
                    return;
                }

                if (videoMetadata.Width - externalVideo.Value.Width > 5)
                {
                    log($"Width {externalVideo.Value.File} {externalVideo.Value.Width} {videoMetadata.Width}");
                    return;
                }

                if (videoMetadata.Height - externalVideo.Value.Height > 20)
                {
                    log($"Height {externalVideo.Value.File} {externalVideo.Value.Height} {videoMetadata.Height}");
                    return;
                }

                if (Math.Abs(videoMetadata.TotalSeconds - externalVideo.Value.TotalSeconds) > 1.1)
                {
                    log($"Duration {externalVideo.Value.File} {externalVideo.Value.TotalSeconds} {videoMetadata.TotalSeconds}");
                    return;
                }

                if (videoMetadata.Audio - externalVideo.Value.Audio > 0)
                {
                    log($"Audio {externalVideo.Value.File} {externalVideo.Value.Audio} {videoMetadata.Audio}");
                    return;
                }

                if (videoMetadata.Subtitle - externalVideo.Value.Subtitle > 0)
                {
                    log($"Subtitle {externalVideo.Value.File} {externalVideo.Value.Subtitle} {videoMetadata.Subtitle}");
                    return;
                }

                string movieDirectory = Path.GetDirectoryName(videoMetadata.File);
                string newExternalVideo = Path.Combine(movieDirectory, Path.GetFileName(externalVideo.Value.File));
                if (!isDryRun)
                {
                    File.Move(externalVideo.Value.File, newExternalVideo);
                }

                log($"Move external video {externalVideo.Value.File} to {newExternalVideo}");

                Directory.GetFiles(Path.GetDirectoryName(externalVideo.Value.File), "*", SearchOption.TopDirectoryOnly)
                    .Where(file => moveAllAttachment || Path.GetFileNameWithoutExtension(file).Contains(Path.GetFileNameWithoutExtension(externalVideo.Value.File), StringComparison.InvariantCultureIgnoreCase))
                    .ToArray()
                    .ForEach(externalAttachment =>
                    {
                        string newExternalAttachment = Path.Combine(movieDirectory, Path.GetFileName(externalAttachment));
                        if (File.Exists(newExternalAttachment))
                        {
                            if (!isDryRun)
                            {
                                new FileInfo(newExternalAttachment).IsReadOnly = false;
                                File.Delete(newExternalAttachment);
                            }

                            log($"Delete attachment {newExternalAttachment}");
                        }
                        if (!isDryRun)
                        {
                            File.Move(externalAttachment, newExternalAttachment);
                        }

                        log($"Move external attachment {externalAttachment} to {newExternalAttachment}");
                    });

                if (!isDryRun)
                {
                    new FileInfo(videoMetadata.File).IsReadOnly = false;
                    File.Delete(videoMetadata.File);
                }

                log($"Delete video {videoMetadata.File}");

                Directory.GetFiles(movieDirectory, "*", SearchOption.TopDirectoryOnly)
                    .Where(file => Path.GetFileNameWithoutExtension(file).Contains(Path.GetFileNameWithoutExtension(videoMetadata.File), StringComparison.InvariantCultureIgnoreCase))
                    .ToArray()
                    .ForEach(attachment =>
                    {
                        string newAttachment = Path.Combine(movieDirectory, Path.GetFileName(attachment).Replace(Path.GetFileNameWithoutExtension(videoMetadata.File), Path.GetFileNameWithoutExtension(externalVideo.Value.File)));
                        if (File.Exists(newAttachment))
                        {
                            if (!isDryRun)
                            {
                                new FileInfo(attachment).IsReadOnly = false;
                                File.Delete(attachment);
                            }

                            log($"Delete attachment {attachment}");
                        }
                        else
                        {
                            if (!isDryRun)
                            {
                                File.Move(attachment, newAttachment);
                            }

                            log($"Move attachment {attachment} to {newAttachment}");
                        }
                    });

                string deletedExternalMovie = Path.Combine(deletedDirectory, Path.GetFileName(externalMovie));
                if (!isDryRun)
                {
                    Directory.Move(externalMovie, deletedExternalMovie);
                }

                log($"Move external movie {externalMovie} to {deletedExternalMovie}");
            });
        }

        internal static void PrintSubtitlesWithError(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetFiles(directory, "*.srt", SearchOption.AllDirectories)
                .Where(subtitle => !Regex.IsMatch(File.ReadLines(subtitle).FirstOrDefault(line => !string.IsNullOrWhiteSpace(line.Trim()))?.Trim() ?? string.Empty, @"^(\-)?[0-9]+$"))
                .ForEach(log);
        }

        internal static void PrintDuplicateImdbId(Action<string>? log = null, params string[] directories)
        {
            log ??= TraceLog;
            directories.SelectMany(directory => Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories))
                .GroupBy(Path.GetFileNameWithoutExtension)
                .Where(group => group.Count() > 1)
                .ForEach(group => group.ForEach(log));
        }

        internal static void RenameDirectoriesWithoutMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .ToArray()
                .ForEach(d =>
                {
                    string movie = Path.GetFileName(d) ?? throw new InvalidOperationException(d);
                    bool isRenamed = false;
                    if (movie.StartsWith("0."))
                    {
                        movie = movie.Substring("0.".Length);
                        isRenamed = true;
                    }

                    if (movie.Contains("{"))
                    {
                        movie = movie.Substring(0, movie.IndexOf("{", StringComparison.Ordinal));
                        isRenamed = true;
                    }

                    if (isRenamed)
                    {
                        string newMovie = Path.Combine(Path.GetDirectoryName(d), movie);
                        log(d);
                        if (!isDryRun)
                        {
                            Directory.Move(d, newMovie);
                        }
                        log(newMovie);
                    }
                });
        }
    }

    public class VideoMetadata
    {
        public string ImdbId { get; set; } = string.Empty;

        public string File { get; set; } = string.Empty;

        public int Width { get; set; }

        public int Height { get; set; }

        public double TotalSeconds { get; set; }

        public int Audio { get; set; }

        public int Subtitle { get; set; }
    }
}
