namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Text;
using Ude;

internal static partial class Video
{
    static Video() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private static readonly UTF8Encoding Utf8Encoding = new(true);

    private static readonly byte[] Utf8Bom = Utf8Encoding.GetPreamble();

    private static readonly string[] TextSubtitleExtensions = [".srt", ".ass", ".ssa", ".vtt", ".smi"];

    private static readonly string[] BinarySubtitleExtensions = [".idx", ".sub", ".sup"];

    private static readonly string[] TextExtensions = TextSubtitleExtensions.Append(".txt").ToArray();

    private static readonly string[] AllSubtitleExtensions = TextSubtitleExtensions.Concat(BinarySubtitleExtensions).ToArray();

    private static readonly string[] CommonChinese = ["的", "是"];

    private static readonly string[] CommonEnglish = [" of ", " is "];

    internal static (string? Charset, float? Confidence, string File)[] GetSubtitles(string directory) =>
        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => file.HasAnyExtension(TextExtensions))
            .Select(file =>
            {
                using FileStream fileStream = File.OpenRead(file);
                CharsetDetector detector = new();
                detector.Feed(fileStream);
                detector.DataEnd();
                return detector.Charset is not null ? (detector.Charset, detector.Confidence, File: file) : (Charset: (string?)null, Confidence: (float?)null, File: file);
            })
            .OrderBy(result => result.Charset)
            .ThenByDescending(result => result.Confidence)
            .ToArray();

    internal static void DeleteSubtitle(string directory, bool isDryRun = false, Action<string>? log = null, params string[] encodings)
    {
        log ??= Logger.WriteLine;
        GetSubtitles(directory)
            .Where(result => encodings.Any(encoding => encoding.EqualsIgnoreCase(result.Charset)))
            .ForEach(result =>
            {
                if (!isDryRun)
                {
                    FileHelper.Recycle(result.File);
                }
                log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
            });
    }

    internal static async Task ConvertToUtf8Async(string directory, bool backup = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        await GetSubtitles(directory)
            .Where(result =>
            {
                if (result.Charset?.ToUpperInvariant() != "UTF-8")
                {
                    return true;
                }

                using FileStream stream = File.OpenRead(result.File);
                byte[] head = new byte[Utf8Bom.Length];
                int read = stream.Read(head, 0, head.Length);
                return !(read == Utf8Bom.Length && Utf8Bom.SequenceEqual(head));
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
                    await EncodingHelper.ConvertAsync(encoding, Utf8Encoding, result.File, null, Utf8Bom);
                    log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
                }
                catch (Exception exception)
                {
                    log($"{result.Item3} {exception}");
                }
            });
    }

    internal static void MoveAllSubtitles(string fromDirectory, string toDirectory, bool overwrite = false, Action<string>? log = null) =>
        FileHelper.MoveAll(fromDirectory, toDirectory, searchOption: SearchOption.AllDirectories, predicate: file => file.HasAnyExtension(AllSubtitleExtensions), overwrite: overwrite);

    internal static void CopyAllSubtitles(string fromDirectory, string toDirectory, bool overwrite = false, Action<string>? log = null) =>
        FileHelper.CopyAll(fromDirectory, toDirectory, searchOption: SearchOption.AllDirectories, predicate: file => file.HasAnyExtension(AllSubtitleExtensions), overwrite: overwrite);

    internal static void MoveSubtitlesForEpisodes(string mediaDirectory, string subtitleDirectory, string mediaExtension = VideoExtension, bool overwrite = false, Func<string, string, string>? rename = null, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .EnumerateFiles(mediaDirectory, $"{PathHelper.AllSearchPattern}{mediaExtension}", SearchOption.AllDirectories)
            .ToArray()
            .ForEach(video =>
            {
                string match = Regex.Match(PathHelper.GetFileNameWithoutExtension(video), @"S[\d]+E[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                string[] files = Directory
                    .GetFiles(subtitleDirectory, $"{PathHelper.AllSearchPattern}{match}{PathHelper.AllSearchPattern}", SearchOption.AllDirectories);
                string subtitleBase = string.Empty;
                string subtitleVideo = files.SingleOrDefault(IsVideo, string.Empty);
                if (subtitleBase.IsNotNullOrWhiteSpace())
                {
                    subtitleBase = PathHelper.GetFileNameWithoutExtension(subtitleVideo);
                }
                else
                {
                    string subtitleMetadata = files.SingleOrDefault(IsXmlMetadata, string.Empty);
                    if (subtitleMetadata.IsNotNullOrWhiteSpace())
                    {
                        subtitleBase = PathHelper.GetFileNameWithoutExtension(subtitleMetadata);
                    }
                }

                files
                    .Where(IsSubtitle)
                    .ToArray()
                    .ForEach(subtitle =>
                    {
                        string subtitleName = PathHelper.GetFileNameWithoutExtension(subtitle);
                        string language = subtitleBase.IsNotNullOrWhiteSpace() && subtitleName.StartsWithIgnoreCase(subtitleBase)
                            ? subtitleName.Substring(subtitleBase.Length).TrimStart(Delimiter.ToCharArray())
                            : subtitleName.Split(Delimiter).Last();
                        if (!Regex.IsMatch(language, @"^([a-z]{3}(\&[a-z]{3})?(\-[a-z0-9]+)?)?$"))
                        {
                            language = string.Empty;
                        }

                        string newSubtitleName = $"{PathHelper.GetFileNameWithoutExtension(video)}{(language.IsNullOrWhiteSpace() ? string.Empty : Delimiter)}{language}{PathHelper.GetExtension(subtitle)}";
                        string newSubtitle = Path.Combine(PathHelper.GetDirectoryName(video), newSubtitleName);
                        if (rename is not null)
                        {
                            newSubtitle = rename(subtitle, newSubtitle);
                        }

                        if (overwrite)
                        {
                            log($"Move {subtitle}");
                            if (!isDryRun)
                            {
                                FileHelper.Move(subtitle, newSubtitle, true);
                            }

                            log(newSubtitle);
                            log(string.Empty);
                        }
                        else if (!File.Exists(newSubtitle))
                        {
                            log($"Move {subtitle}");
                            if (!isDryRun)
                            {
                                File.Move(subtitle, newSubtitle, false);
                            }

                            log(newSubtitle);
                            log(string.Empty);
                        }
                        else
                        {
                            log($"Ignore {subtitle}");
                            log(string.Empty);
                        }
                    });
            });
    }

    internal static void MoveSubtitleToParentDirectory(string directory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Directory
            .EnumerateFiles(directory, "*.idx", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(directory, "*.sub", SearchOption.AllDirectories))
            .Where(subtitle => "Subs".EqualsIgnoreCase(PathHelper.GetFileName(PathHelper.GetDirectoryName(subtitle))))
            .ToArray()
            .ForEach(subtitle =>
            {
                string subtitleDirectory = PathHelper.GetDirectoryName(subtitle);
                string parent = PathHelper.GetDirectoryName(subtitleDirectory);
                string[] videos = Directory.GetFiles(parent, VideoSearchPattern, SearchOption.TopDirectoryOnly);
                string mainVideo = videos.OrderByDescending(video => new FileInfo(video).Length).First();
                string newSubtitle = Path.Combine(parent, PathHelper.GetFileNameWithoutExtension(mainVideo) + PathHelper.GetExtension(subtitle));
                log(newSubtitle);
                if (!isDryRun)
                {
                    File.Move(subtitle, newSubtitle);
                    if (Directory.EnumerateFiles(subtitleDirectory).IsEmpty())
                    {
                        DirectoryHelper.Recycle(subtitleDirectory);
                    }
                }
            });

        Directory
            .EnumerateFiles(directory, "*.srt", SearchOption.AllDirectories)
            .Where(subtitle => "Subs".EqualsIgnoreCase(PathHelper.GetFileName(PathHelper.GetDirectoryName(subtitle))))
            .ToArray()
            .ForEach(subtitle =>
            {
                string subtitleDirectory = PathHelper.GetDirectoryName(subtitle);
                string parent = PathHelper.GetDirectoryName(subtitleDirectory);
                string[] videos = Directory.GetFiles(parent, VideoSearchPattern, SearchOption.TopDirectoryOnly);
                string mainVideo = videos.Length == 1
                    ? videos[0]
                    : videos.OrderByDescending(video => new FileInfo(video).Length).First();
                string language = PathHelper.GetFileNameWithoutExtension(subtitle);
                string postfix = language switch
                {
                    _ when language.ContainsIgnoreCase("eng") => string.Empty,
                    _ when language.ContainsIgnoreCase("chi") => ".chs",
                    _ => $"{Delimiter}{language}"
                };
                string newSubtitle = Path.Combine(parent, $"{PathHelper.GetFileNameWithoutExtension(mainVideo)}{postfix}.srt");
                log(subtitle);
                if (!isDryRun)
                {
                    if (File.Exists(newSubtitle))
                    {
                        if (string.IsNullOrEmpty(postfix)) // English.
                        {
                            long subtitleSize = new FileInfo(subtitle).Length;
                            long newSubtitleSize = new FileInfo(newSubtitle).Length;
                            if (subtitleSize >= newSubtitleSize)
                            {
                                string newSecondarySubtitle = PathHelper.AddFilePostfix(newSubtitle, ".eng");
                                if (File.Exists(newSecondarySubtitle))
                                {
                                    Debug.Assert(newSubtitleSize >= new FileInfo(newSecondarySubtitle).Length);
                                    FileHelper.Recycle(newSecondarySubtitle);
                                }

                                FileHelper.Move(newSubtitle, newSecondarySubtitle); // smaller.srt => smaller.eng.srt
                                FileHelper.Move(subtitle, newSubtitle); // larger.srt
                            }
                            else
                            {
                                newSubtitle = PathHelper.AddFilePostfix(newSubtitle, ".eng");
                                if (File.Exists(newSubtitle))
                                {
                                    newSubtitleSize = new FileInfo(newSubtitle).Length;
                                    if (subtitleSize >= newSubtitleSize)
                                    {
                                        FileHelper.Recycle(newSubtitle);
                                        FileHelper.Move(subtitle, newSubtitle);
                                    }
                                    else
                                    {
                                        FileHelper.Recycle(subtitle);
                                    }
                                }
                                else
                                {
                                    FileHelper.Move(subtitle, newSubtitle);
                                }
                            }
                        }
                        else
                        {
                            log($"!{subtitle}");
                        }
                    }
                    else
                    {
                        FileHelper.Move(subtitle, newSubtitle);
                    }

                    if (Directory.EnumerateFiles(subtitleDirectory).IsEmpty())
                    {
                        DirectoryHelper.Recycle(subtitleDirectory);
                    }
                }
            });
    }

    internal static async Task RenameSubtitlesByLanguageAsync(string directory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        await ConvertToUtf8Async(directory, log: log);
        Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(IsTextSubtitle)
            .ToArray()
            .ForEach(file =>
            {
                string content = File.ReadAllText(file);
                List<string> languages = [];
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
                    if (PathHelper.GetFileNameWithoutExtension(file).EndsWithIgnoreCase(string.Join('&', languages)) || PathHelper.GetFileNameWithoutExtension(file).EndsWith(string.Join('&', languages.AsEnumerable().Reverse())))
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
        log ??= Logger.WriteLine;
        Directory.GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(IsTextSubtitle)
            .ForEach(subtitle =>
            {

                if (".srt".EqualsIgnoreCase(PathHelper.GetExtension(subtitle))
                    && !Regex.IsMatch(File.ReadLines(subtitle).FirstOrDefault(line => line.Trim().IsNotNullOrWhiteSpace())?.Trim() ?? string.Empty, @"^(\-)?[0-9]+$"))
                {
                    log($"!Format {subtitle}");
                }

                if (PathHelper.GetFileNameWithoutExtension(subtitle).EndsWithIgnoreCase($"{Delimiter}eng"))
                {
                    string sss = subtitle.Replace($"{Delimiter}eng.", Delimiter);
                    if (File.Exists(sss))
                    {
                        log($"!Duplicate {subtitle}");
                    }
                }

                string postfix = PathHelper.GetFileNameWithoutExtension(subtitle).Split(Delimiter).Last();
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

    internal static void RemoveSubtitleSuffix(string directory, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => file.IsSubtitle() && Regex.IsMatch(PathHelper.GetFileNameWithoutExtension(file).Split(Delimiter).Last(), @"^[a-z]{3}\-[0-9]{1,2}$"))
            .ToArray()
            .ForEach(subtitle =>
            {
                string newSubtitle = Regex.Replace(subtitle, @"(\.[a-z]{3})\-[0-9]{1,2}(\.[a-zA-Z]{3})$", "$1$2");
                if (File.Exists(newSubtitle))
                {
                    Logger.WriteLine(subtitle);
                }
                else
                {
                    FileHelper.Move(subtitle, newSubtitle);
                }
            });
    }
}