namespace Examples.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Examples.Common;
    using Examples.Linq;
    using Examples.Text;
    using Ude;

    internal static partial class Video
    {
        private static readonly UTF8Encoding Utf8Encoding = new(true);

        private static readonly byte[] Bom = Utf8Encoding.GetPreamble();

        private static readonly string[] TextSubtitleExtensions = { ".srt", ".ass", ".ssa", ".vtt", ".smi" };

        private static readonly string[] BinarySubtitleExtensions = { ".idx", ".sub", ".sup" };

        private static readonly string[] TextExtensions = TextSubtitleExtensions.Append(".txt").ToArray();

        private static readonly string[] AllSubtitleExtensions = TextSubtitleExtensions.Concat(BinarySubtitleExtensions).ToArray();

        private static readonly string[] CommonChinese = { "的", "是" };

        private static readonly string[] CommonEnglish = { " of ", " is " };

        internal static (string? Charset, float? Confidence, string File)[] GetSubtitles(string directory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Directory
                .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(file => TextExtensions.Any(file.EndsWithIgnoreCase))
                .Select(file =>
                {
                    using FileStream fileStream = File.OpenRead(file);
                    CharsetDetector detector = new();
                    detector.Feed(fileStream);
                    detector.DataEnd();
                    return detector.Charset != null ? (detector.Charset, Confidence: detector.Confidence, File: file) : (Charset: (string?)null, Confidence: (float?)null, File: file);
                })
                .OrderBy(result => result.Charset)
                .ThenByDescending(result => result.Confidence)
                .ToArray();
        }

        internal static void DeleteSubtitle(string directory, bool isDryRun = false, Action<string>? log = null, params string[] encodings)
        {
            log ??= TraceLog;
            GetSubtitles(directory)
                .Where(result => encodings.Any(encoding => encoding.EqualsIgnoreCase(result.Charset)))
                .ForEach(result =>
                {
                    if (!isDryRun)
                    {
                        File.Delete(result.File);
                    }
                    log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
                });
        }

        internal static async Task ConvertToUtf8Async(string directory, bool backup = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            await GetSubtitles(directory)
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
                .ForEachAsync(async result =>
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

                        FileInfo fileInfo = new(result.Item3);
                        if (fileInfo.IsReadOnly)
                        {
                            fileInfo.IsReadOnly = false;
                        }
                        if (backup)
                        {
                            FileHelper.Backup(result.Item3);
                        }
                        await EncodingHelper.Convert(encoding, Utf8Encoding, result.File, null, Bom);
                        log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
                    }
                    catch (Exception exception)
                    {
                        log($"{result.Item3} {exception}");
                    }
                });
        }

        internal static void MoveAllSubtitles(string fromDirectory, string toDirectory, bool overwrite = false, Action<string>? log = null)
        {
            FileHelper.MoveAll(fromDirectory, toDirectory, searchOption: SearchOption.AllDirectories, predicate: file => file.HasAnyExtension(AllSubtitleExtensions), overwrite: overwrite);
        }

        internal static void MoveSubtitlesForEpisodes(string mediaDirectory, string subtitleDirectory, string mediaExtension = VideoExtension, bool overwrite = false, Func<string, string, string>? rename = null)
        {
            Directory
                .EnumerateFiles(mediaDirectory, $"{PathHelper.AllSearchPattern}{mediaExtension}", SearchOption.AllDirectories)
                .ToArray()
                .ForEach(video =>
                {
                    string match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    Directory
                        .EnumerateFiles(subtitleDirectory, $"{PathHelper.AllSearchPattern}{match}{PathHelper.AllSearchPattern}", SearchOption.TopDirectoryOnly)
                        .Where(file => !file.EndsWithIgnoreCase(mediaExtension))
                        .ToArray()
                        .ForEach(subtitle =>
                        {
                            string language = (Path.GetFileNameWithoutExtension(subtitle) ?? throw new InvalidOperationException(subtitle)).Split(".").Last();
                            string newSubtitle = $"{Path.GetFileNameWithoutExtension(video)}.{language}{Path.GetExtension(subtitle)}";
                            string newFile = Path.Combine(Path.GetDirectoryName(video) ?? throw new InvalidOperationException(video), newSubtitle);
                            if (rename != null)
                            {
                                newFile = rename(subtitle, newFile);
                            }

                            if (overwrite)
                            {
                                FileHelper.Move(subtitle, newFile, true);
                            }
                            else if (!File.Exists(newFile))
                            {
                                File.Move(subtitle, newFile);
                            }
                        });
                });
        }

        internal static void MoveSubtitleToParentDirectory(string directory, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory
                .EnumerateFiles(directory, "*.idx", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(directory, "*.sub", SearchOption.AllDirectories))
                .ToArray()
                .Where(subtitle => "Subs".EqualsIgnoreCase(Path.GetFileName(Path.GetDirectoryName(subtitle))))
                .ForEach(subtitle =>
                {
                    string subtitleDirectory = Path.GetDirectoryName(subtitle)!;
                    string parent = Path.GetDirectoryName(subtitleDirectory) ?? throw new InvalidOperationException(subtitle);
                    string[] videos = Directory.GetFiles(parent, VideoSearchPattern, SearchOption.TopDirectoryOnly);
                    string mainVideo = videos.OrderByDescending(video => new FileInfo(video).Length).First();
                    string newSubtitle = Path.Combine(parent, Path.GetFileNameWithoutExtension(mainVideo) + Path.GetExtension(subtitle));
                    log(newSubtitle);
                    if (!isDryRun)
                    {
                        File.Move(subtitle, newSubtitle);
                        if (!Directory.EnumerateFiles(subtitleDirectory).Any())
                        {
                            Directory.Delete(subtitleDirectory);
                        }
                    }
                });

            Directory
                .GetFiles(directory, "*.srt", SearchOption.AllDirectories)
                .Where(subtitle => "Subs".EqualsIgnoreCase(Path.GetFileName(Path.GetDirectoryName(subtitle))))
                .ForEach(subtitle =>
                {
                    string subtitleDirectory = Path.GetDirectoryName(subtitle)!;
                    string parent = Path.GetDirectoryName(subtitleDirectory) ?? throw new InvalidOperationException(subtitle);
                    string[] videos = Directory.GetFiles(parent, VideoSearchPattern, SearchOption.TopDirectoryOnly);
                    string mainVideo = videos.Length == 1
                        ? videos[0]
                        : videos.OrderByDescending(video => new FileInfo(video).Length).First();
                    string language = Path.GetFileNameWithoutExtension(subtitle) ?? throw new InvalidOperationException(subtitle);
                    string suffix = language switch
                    {
                        _ when language.ContainsIgnoreCase("eng") => string.Empty,
                        _ when language.ContainsIgnoreCase("chi") => ".chs",
                        _ => "." + language
                    };
                    string newSubtitle = Path.Combine(parent, $"{Path.GetFileNameWithoutExtension(mainVideo)}{suffix}.srt");
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

                        if (!Directory.EnumerateFiles(subtitleDirectory).Any())
                        {
                            Directory.Delete(subtitleDirectory);
                        }
                    }
                });
        }

        internal static async Task RenameSubtitlesByLanguageAsync(string directory, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            await ConvertToUtf8Async(directory, log: log);
            Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(IsTextSubtitle)
                .ToArray()
                .ForEach(file =>
                {
                    string content = File.ReadAllText(file);
                    List<string> languages = new();
                    if (CommonChinese.All(chinese => content.ContainsOrdinal(chinese)))
                    {
                        languages.Add("chs");
                    }
                    if (CommonEnglish.All(english => content.ContainsIgnoreCase(english)))
                    {
                        languages.Add("eng");
                    }
                    if (languages.Any())
                    {
                        if (Path.GetFileNameWithoutExtension(file).EndsWithIgnoreCase(string.Join('&', languages)) || Path.GetFileNameWithoutExtension(file).EndsWith(string.Join('&', languages.AsEnumerable().Reverse())))
                        {
                            return;
                        }

                        if (languages.Count == 1 && languages.Single().EqualsIgnoreCase("eng"))
                        {
                            return;
                        }

                        string newFile = PathHelper.AddFilePostfix(file, $".{string.Join('&', languages)}");
                        log(file);
                        if (!isDryRun)
                        {
                            File.Move(file, newFile);
                        }
                        log(newFile);
                    }
                    else
                    {
                        log($"!Unknown: {file}");
                    }
                });
        }

        internal static void PrintSubtitlesWithErrors(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(IsTextSubtitle)
                .ForEach(subtitle =>
                {

                    if (".srt".EqualsIgnoreCase(Path.GetExtension(subtitle))
                        && !Regex.IsMatch(File.ReadLines(subtitle).FirstOrDefault(line => line.Trim().IsNotNullOrWhiteSpace())?.Trim() ?? string.Empty, @"^(\-)?[0-9]+$"))
                    {
                        log($"!Format {subtitle}");
                    }

                    if (Path.GetFileNameWithoutExtension(subtitle).EndsWithIgnoreCase(".eng"))
                    {
                        string sss = subtitle.Replace(".eng.", ".");
                        if (File.Exists(sss))
                        {
                            log($"!Duplicate {subtitle}");
                        }
                    }

                    string postfix = Path.GetFileNameWithoutExtension(subtitle).Split(".").Last();
                    if (postfix.ContainsIgnoreCase("chs") || postfix.ContainsIgnoreCase("cht"))
                    {
                        if (!File.ReadAllText(subtitle).ContainsOrdinal("的"))
                        {
                            log($"!Not Chinese {subtitle}");
                        }
                    }
                    else
                    {
                        if (File.ReadAllText(subtitle).ContainsOrdinal("的"))
                        {
                            log($"!Chinese {subtitle}");
                        }
                    }
                });
        }
    }
}