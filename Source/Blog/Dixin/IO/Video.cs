namespace Dixin.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Newtonsoft.Json;
    using Xabe.FFmpeg;
    using Xabe.FFmpeg.Streams;

    internal static class Video
    {
        private static readonly string[] UncommonVideoSearchPatterns = { "*.avi", "*.wmv", "*.webm", "*.mpg", "*.mpeg", "*.rmvb", "*.rm", "*.3gp", "*.divx", "*.m1v", "*.mov", "*.ts", "*.vob", "*.flv", "*.m4v", "*.mkv" };

        private static readonly string[] CommonVideoSearchPatterns = { "*.avi", "*.mp4" };
        private static readonly string[] CommonVideoExtensions = { ".avi", ".mp4", ".mkv", ".iso" };

        private static readonly string[] AllVideoSearchPattern = UncommonVideoSearchPatterns.Union(CommonVideoSearchPatterns).ToArray();

        private static readonly string[] TextSearchPatterns = { "*.srt", "*.ass", "*.ssa", "*.txt" };

        private static readonly string[] TextSubtitleSearchPatterns = { "*.srt", "*.ass", "*.ssa", "*.txt" };

        private static readonly string[] SubtitleExtensions = { ".srt", ".ass", ".ssa", ".idx", ".sub", ".sup" };

        private static void TraceLog(string message) => Trace.WriteLine(message);

        private static readonly Encoding Utf8Encoding = new UTF8Encoding(true);

        private static readonly byte[] Bom = Utf8Encoding.GetPreamble();

        internal static void RenameFiles(string path, Func<string, int, string> rename, string pattern = null, SearchOption? searchOption = null, Func<string, bool> predicate = null, bool overwrite = false, bool isDryRun = false, Action<string> log = null)
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
                            File.Move(file, newFile, overwrite);
                        }
                        log(newFile);
                    }
                });
        }

        internal static void RenameDirectories(string path, Func<string, string> rename, string pattern = null, SearchOption? searchOption = null, Func<string, bool> predicate = null, Action<string> log = null)
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

        private static void Convert(Encoding from, Encoding to, string fromPath, string toPath = null, byte[] bom = null)
        {
            byte[] fromBytes = File.ReadAllBytes(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            if (bom != null && !bom.SequenceEqual(toBytes.Take(bom.Length)))
            {
                toBytes = bom.Concat(toBytes).ToArray();
            }
            File.WriteAllBytes(toPath ?? fromPath, toBytes);
        }

        internal static (string Charset, float? Confidence, string File)[] GetSubtitles(string directory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return TextSearchPatterns
                .SelectMany(searchPattern => Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories))
                .Select(file =>
                {
                    using FileStream fileStream = File.OpenRead(file);
                    Ude.CharsetDetector detector = new Ude.CharsetDetector();
                    detector.Feed(fileStream);
                    detector.DataEnd();
                    return detector.Charset != null ? (detector.Charset, Confidence: (float?)detector.Confidence, File: file) : (Charset: (string)null, Confidence: (float?)null, File: file);
                })
                .OrderBy(result => result.Charset)
                .ThenByDescending(result => result.Confidence)
                .ToArray();
        }

        internal static void DeleteSubtitle(string directory, bool isDryRun = false, Action<string> log = null, params string[] encodings)
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

        internal static void ConvertToUtf8(string directory, bool backup = false, Action<string> log = null)
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

        private static IEnumerable<string> GetVideos(string directory)
        {
            return AllVideoSearchPattern
                .SelectMany(pattern => Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories));
        }

        private static (string FullPath, string FileName, int Width, int Height, int Audio, int Subtitle) GetVideoMetadata(string file, int retryCount = 10, Action<string> log = null)
        {
            log ??= TraceLog;
            Task<(string FullPath, string FileName, int Width, int Height, int Audio, int Subtitle)> task = Task.Run(() =>
            {

                try
                {
                    return Retry.Incremental(
                        () =>
                        {
                            IMediaInfo mediaInfo = MediaInfo.Get(file).Result;
                            IVideoStream videoStream = mediaInfo.VideoStreams.First();
                            return (file, Path.GetFileName(file), videoStream.Width, videoStream.Height, mediaInfo.AudioStreams?.Count() ?? 0, mediaInfo.SubtitleStreams?.Count() ?? 0);
                        },
                        retryCount,
                        exception => true);
                }
                catch (Exception exception)
                {
                    log($"Fail {file} {exception}");
                    return (file, Path.GetFileName(file), -1, -1, -1, -1);
                }
            });
            if (task.Wait(TimeSpan.FromSeconds(20)))
            {
                log($"{task.Result.Width}x{task.Result.Height}, {task.Result.Audio} audio, {file}");
                return task.Result;
            }

            log($"Timeout {file}");
            return (file, Path.GetFileName(file), -1, -1, -1, -1);
        }

        private static int GetAudioMetadata(string file, Action<string> log = null)
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

        private static string GetVideoError((string FullPath, string FileName, int Width, int Height, int Audio, int Subtitle) video, bool isNoAudioAllowed)
        {
            if (video.Width <= 0 || video.Height <= 0 || (isNoAudioAllowed ? video.Audio < 0 : video.Audio <= 0))
            {
                return $"Failed {video.Width}x{video.Height} {video.Audio}Audio {video.FullPath}";
            }

            string fileName = Path.GetFileNameWithoutExtension(video.FileName);
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

            return null;
        }

        internal static void PrintVideoError(string directory, bool isNoAudioAllowed = false, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, Action<string> log = null)
        {
            PrintVideosWithError(string.IsNullOrWhiteSpace(searchPattern) ? GetVideos(directory) : Directory.EnumerateFiles(directory, searchPattern, searchOption), isNoAudioAllowed, log);
        }

        internal static void PrintVideosWithError(IEnumerable<string> files, bool isNoAudioAllowed = false, Action<string> log = null)
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
                .ForEach(result => log(result.Error));
        }

        internal static void PrintVideoErrorsFast(string directory, Action<string> log = null)
        {
            log ??= TraceLog;
            GetVideos(directory)
                .OrderBy(video => video)
                .ForEach(video =>
                {
                    if (!Regex.IsMatch(Path.GetFileNameWithoutExtension(video), "[1-9]Audio", RegexOptions.IgnoreCase))
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

        internal static void PrintDirectoryError(string directory, int level = 2, Action<string> log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(d => !Regex.IsMatch(Path.GetFileName(d), MovieDirectory))
                .ForEach(d => log(d));
        }

        private static IEnumerable<string> GetDirectories(string directory, int level = 2)
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

                List<string> info = Path.GetFileName(file).Split(".").ToList();
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
                    .Where(attachment => Path.GetFileName(attachment).StartsWith(Path.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .ForEach(attachment =>
                    {
                        string newAttachment = Path.Combine(directory, Path.GetFileName(attachment).Replace(Path.GetFileNameWithoutExtension(file), string.Join(".", info.SkipLast(1))));
                        log(newAttachment);
                        File.Move(attachment, newAttachment);
                    });
            });
        }

        internal static void PrintDirectoriesWithMultipleVideos(string directory, int level = 2, Action<string> log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(movie => CommonVideoSearchPatterns.SelectMany(pattern => Directory.EnumerateFiles(movie, pattern, SearchOption.TopDirectoryOnly)).Count() > 1)
                .ForEach(log);
        }

        internal static async Task ConvertAsync(Encoding from, Encoding to, string fromPath, string toPath = null)
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
                            string language = Path.GetFileNameWithoutExtension(subtitle).Split(".").Last();
                            string newSubtitle = $"{Path.GetFileNameWithoutExtension(video)}.{language}{Path.GetExtension(subtitle)}";
                            File.Move(subtitle, Path.Combine(Path.GetDirectoryName(video), newSubtitle), true);
                        });
                });
        }

        private static string FilterTitleForFileSystem(this string value) => value.Replace("?", "").Replace(": ", "-").Replace("*", "_").Replace("/", "_");

        internal static void RenameEpisodesWithTitle(string nfoDirectory, string mediaDirectory, string searchPattern, Func<string, string, string> rename, bool isDryRun = false, Action<string> log = null)
        {
            Directory.EnumerateFiles(nfoDirectory, $"{searchPattern}.nfo", SearchOption.AllDirectories)
                .ToList()
                .ForEach(nfo =>
                {
                    string match = Regex.Match(Path.GetFileNameWithoutExtension(nfo), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    string title = XDocument.Load(nfo).Root?.Element("title")?.Value.FilterTitleForFileSystem();
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

        internal static void RenameDirectoriesWithDefinition(string directory, int level = 2, Action<string> log = null)
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

        internal static void PrintDirectoriesWithLowDefinition(string directory, int level = 2, Action<string> log = null)
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

        internal static void PrintDirectoriesMultipleMedia(string directory, int level = 2, Action<string> log = null)
        {
            GetDirectories(directory, level)
                .ForEach(movie =>
                {
                    if (Directory.GetFiles(movie).Count(file => CommonVideoExtensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase))) > 1)
                    {
                        log(movie);
                    }
                });
        }

        internal static void PrintDirectoriesWithMissingVideo(string directory, int level = 2, Action<string> log = null)
        {
            GetDirectories(directory, level)
            .Where(m => !Directory.EnumerateFiles(m, "*.mp4", SearchOption.TopDirectoryOnly).Any() && !Directory.EnumerateFiles(m, "*.avi", SearchOption.TopDirectoryOnly).Any())
            .ForEach(log);
        }

        internal static void RenameVideosWithDefinition(string[] files, bool isDryRun = false, Action<string> log = null)
        {
            log ??= TraceLog;
            files.ForEach(file => Debug.Assert(Regex.IsMatch(file, @"^\!(720p|1080p)\: ([0-9]{2,4}x[0-9]{2,4} )?(.*)$")));
            files
                .Select(file =>
                {
                    Match match = Regex.Match(file, @"^\!(720p|1080p)\: ([0-9]{2,4}x[0-9]{2,4} )?(.*)$");
                    string path = match.Groups.Cast<Group>().Last().Value;
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
                            File.Move(result.File, newFile, true);
                        }
                    }
                });
        }

        internal static void MoveSubtitleToParentDirectory(string directory, bool isDryRun = false, Action<string> log = null)
        {
            Directory.GetFiles(directory, "*.srt", SearchOption.AllDirectories)
                .ForEach(subtitle =>
                {
                    if ("Subs".Equals(Path.GetFileName(Path.GetDirectoryName(subtitle)), StringComparison.OrdinalIgnoreCase))
                    {
                        string parent = Path.GetDirectoryName(Path.GetDirectoryName(subtitle));
                        string[] videos = Directory.GetFiles(parent, "*.mp4", SearchOption.TopDirectoryOnly);
                        string mainVideo = videos.Length == 1
                            ? videos[0]
                            : videos.OrderByDescending(video => new FileInfo(video).Length).First();
                        string language = Path.GetFileNameWithoutExtension(subtitle).ToUpperInvariant();
                        string suffix = language switch
                        {
                            _ when language.Contains("ENG") => "",
                            _ when language.Contains("CHI") => ".chs",
                            _ => "." + language
                        };
                        string newSubtitle = Path.Combine(parent, Path.GetFileNameWithoutExtension(mainVideo) + suffix + ".srt");
                        log(subtitle);
                        if (!isDryRun)
                        {
                            File.Move(subtitle, newSubtitle);
                        }
                        log(newSubtitle);
                    }
                });
        }

        private static readonly Regex[] PreferredVersions = new string[] { @"[\. ]YIFY(\+HI)?$", @"[\. ]YIFY[\. ]", @"\[YTS\.", @"\-RARBG(\.[1-9]Audio)?$", @"\-VXT(\.[1-9]Audio)?$", @"\.GAZ$" }.Select(version => new Regex(version)).ToArray();

        internal static void PrintVideosNonPreferred(string directory, int level = 2, Action<string> log = null)
        {
            GetDirectories(directory, level)
                .ForEach(movie => Directory
                    .GetFiles(movie, "*.mp4", SearchOption.TopDirectoryOnly)
                    .Where(file => !PreferredVersions.Any(version => version.IsMatch(Path.GetFileNameWithoutExtension(file))))
                    .ForEach(log));
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string> log = null)
        {
            GetDirectories(directory, level)
                .Where(movie => Directory.GetFiles(movie).All(video => SubtitleExtensions.All(extension => !video.EndsWith(extension, StringComparison.OrdinalIgnoreCase))))
                .ForEach(log);
        }

        internal static void PrintMoviesWithNoSubtitle(string directory, int level = 2, Action<string> log = null, params string[] languages)
        {
            string[] searchPatterns = languages.SelectMany(language => SubtitleExtensions.Select(extension => $"*{language}*{extension}")).ToArray();
            GetDirectories(directory, level)
                .Where(movie => searchPatterns.All(searchPattern => !Directory.EnumerateFiles(movie, searchPattern, SearchOption.TopDirectoryOnly).Any()))
                .ForEach(log);
        }

        internal static void RenameDirectoriesWithMetadata(string directory, int level = 2, bool isDryRun = false, Action<string> log = null)
        {
            GetDirectories(directory, level)
                .ToArray()
                .ForEach(movie =>
                {
                    string[] nfos = Directory.GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).OrderBy(nfo => nfo).ToArray();
                    XDocument english = XDocument.Load(nfos.First(nfo => nfo.EndsWith(".eng.nfo")));
                    XDocument chinese = XDocument.Load(nfos.First(nfo => !nfo.EndsWith(".eng.nfo")));
                    string englishTitle = english.Root?.Element("title")?.Value;
                    string chineseTitle = chinese.Root?.Element("title")?.Value;
                    string originalTitle = chinese.Root?.Element("originaltitle")?.Value;
                    string year = chinese.Root?.Element("year")?.Value;
                    string imdb = chinese.Root?.Element("imdbid")?.Value;
                    string rating = string.IsNullOrWhiteSpace(imdb)
                        ? "[-]"
                        : float.TryParse(chinese.Root.Element("rating")?.Value, out float ratingFloat) ? ratingFloat.ToString("0.0") : "0.0";
                    string[] videos = Directory.GetFiles(movie, "*.mp4", SearchOption.TopDirectoryOnly).Concat(Directory.GetFiles(movie, "*.avi", SearchOption.TopDirectoryOnly)).ToArray();
                    string definition = videos switch
                    {
                        _ when videos.Any(video => Path.GetFileNameWithoutExtension(video).Contains("1080p")) => "[1080p]",
                        _ when videos.Any(video => Path.GetFileNameWithoutExtension(video).Contains("720p")) => "[720p]",
                        _ => string.Empty
                    };
                    originalTitle = string.Equals(originalTitle, englishTitle, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(originalTitle)
                        ? string.Empty
                        : $"={originalTitle}";
                    string newMovie = $"{englishTitle.FilterTitleForFileSystem()}{originalTitle.FilterTitleForFileSystem()}.{year}.{chineseTitle.FilterTitleForFileSystem()}[{rating}]{definition}";
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
                .Do(nfo => Debug.Assert(File.Exists(nfo.Replace(".eng.nfo", ".nfo"))))
                .ForEach(nfo => File.Move(nfo, Path.Combine(Path.GetDirectoryName(nfo), Path.GetFileNameWithoutExtension(nfo).Replace(".eng", string.Empty) + Path.GetExtension(nfo)), true));
        }

        private static readonly string[] IndependentNfos = { "tvshow.nfo", "season.nfo" };

        internal static void PrintMetadataWithoutMedia(string directory, Action<string> log = null)
        {
            Directory.EnumerateFiles(directory, "*.nfo", SearchOption.AllDirectories)
                .Where(nfo => IndependentNfos.All(independent => !string.Equals(independent, Path.GetFileName(nfo), StringComparison.OrdinalIgnoreCase)))
                .Where(nfo => CommonVideoExtensions.Select(extension => Path.Combine(Path.GetDirectoryName(nfo), Path.GetFileNameWithoutExtension(nfo) + extension)).All(video => !File.Exists(video)))
                .ForEach(log);
        }

        internal static void PrintMetadataByGroup(string directory, int level = 2, string field = "genre", Action<string> log = null)
        {
            GetDirectories(directory, level)
                .Select(movie => (movie, nfo: XDocument.Load(Directory.GetFiles(movie, "*.nfo", SearchOption.TopDirectoryOnly).First())))
                .Select(movie => (movie.movie, field: movie.nfo.Root?.Element(field)?.Value))
                .OrderBy(movie => movie.field)
                .ForEach(movie => log($"{movie.field}: {Path.GetFileName(movie.movie)}"));
        }

        internal static void PrintMetadataByDuplication(string directory, int level = 2, string field = "imdbid", Action<string> log = null)
        {
            Directory.GetFiles(directory, "*.nfo", SearchOption.AllDirectories)
                .Select(nfo => (nfo, field: XDocument.Load(nfo).Root?.Element(field)?.Value))
                .GroupBy(movie => movie.field)
                .Where(group => group.Count() > 1)
                .ForEach(group => group.ForEach(movie => log($"{movie.field} - {movie.nfo}")));
        }

        internal static void MoveMovies(string destination, string directory, int level = 2, string field = "genre", string value = null, bool isDryRun = false)
        {
            MoveMovies(
                (movie, metadata) => Path.Combine(destination, Path.GetFileName(movie)),
                directory,
                level,
                (movie, metadata) => string.Equals(value, metadata.Root?.Element(field)?.Value, StringComparison.OrdinalIgnoreCase),
                isDryRun);
        }

        internal static void MoveMovies(Func<string, XDocument, string> rename, string directory, int level = 2, Func<string, XDocument, bool> predicate = null, bool isDryRun = false, Action<string> log = null)
        {
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

        internal static void DeleteDuplication(string directory, int level = 2, bool isDryRun = false, Action<string> log = null)
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

        internal static void RenameSubtitlesByLanguage(string directory, bool isDryRun = false, Action<string> log = null)
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

        internal static void PrintYears(string directory, int level = 2, Action<string> log = null)
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
                    string metadataYear = metadata.Root?.Element("year")?.Value;
                    string videoName = null;
                    if (!(directoryYear == metadataYear
                        && CommonVideoSearchPatterns
                            .SelectMany(pattern => Directory.GetFiles(movie, pattern, SearchOption.TopDirectoryOnly))
                            .All(video => (videoName = Path.GetFileName(video)).Contains(directoryYear))))
                    {
                        log($"Direcoty: {directoryYear}, Metadata {metadataYear}, Video: {videoName}, {movie}");
                    }
                });
        }

        internal static void PrintDirectoriesWithNonLatinOriginalTitle(string directory, int level = 2, Action<string> log = null)
        {
            log ??= TraceLog;
            GetDirectories(directory, level)
                .Where(movie => movie.Contains("="))
                .Where(movie => !Regex.IsMatch(movie.Split("=")[1], "^[a-z]{1}.", RegexOptions.IgnoreCase))
                .ForEach(log);
        }

        internal static void PrintDirectoriesWithDuplicatePictures(string directory, int level = 2, Action<string> log = null)
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
    }
}
