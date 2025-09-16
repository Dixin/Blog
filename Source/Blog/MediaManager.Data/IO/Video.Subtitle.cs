namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Text;
using System;
using System.Linq;
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

    private static readonly string[] CommonEnglish = [" the ", " is ", " to ", " of ", " and "];

    private static bool ContainsCommonEnglish([NotNullWhen(true)] this string? value) => value.IsNotNullOrWhiteSpace() && CommonEnglish.All(value.ContainsIgnoreCase);

    internal static IEnumerable<(string? Charset, float? Confidence, string File)> EnumerateSubtitles(string directory) =>
        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => file.IsTextSubtitle())
            .Select(file =>
            {
                using FileStream fileStream = File.OpenRead(file);
                CharsetDetector detector = new();
                detector.Feed(fileStream);
                detector.DataEnd();
                return detector.Charset is not null ? (detector.Charset, detector.Confidence, File: file) : (Charset: (string?)null, Confidence: (float?)null, File: file);
            });
    //.OrderBy(result => result.Charset)
    //.ThenByDescending(result => result.Confidence);

    internal static void DeleteSubtitle(string directory, bool isDryRun = false, Action<string>? log = null, params string[] encodings)
    {
        log ??= Logger.WriteLine;
        EnumerateSubtitles(directory)
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
        await EnumerateSubtitles(directory)
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
                        case "UTF-16BE":
                            encoding = Encoding.BigEndianUnicode;
                            break;
                        case "GB18030":
                            encoding = Encoding.GetEncoding("gb18030");
                            break;
                        case "EUC-KR":
                            encoding = Encoding.GetEncoding("EUC-KR");
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
                            log($"!Not supported {result.Charset}, file {result.File}");
                            return;
                    }

                    FileInfo fileInfo = new(result.File);
                    if (fileInfo.IsReadOnly)
                    {
                        fileInfo.IsReadOnly = false;
                    }

                    if (backup)
                    {
                        FileHelper.Backup(result.File);
                    }

                    await EncodingHelper.ConvertAsync(encoding, Utf8Encoding, result.File, null, Utf8Bom);
                    log($"Charset: {result.Charset}, confidence: {result.Confidence}, file {result.File}");
                }
                catch (Exception exception)
                {
                    log($"{result.File} {exception}");
                }
            });
    }

    internal static void MoveAllSubtitles(string fromDirectory, string toDirectory, bool overwrite = false, Action<string>? log = null) =>
        FileHelper.MoveAll(fromDirectory, toDirectory, searchOption: SearchOption.AllDirectories, predicate: file => file.HasAnyExtension(AllSubtitleExtensions), overwrite: overwrite);

    internal static void CopyAllSubtitles(string fromDirectory, string toDirectory, bool overwrite = false, Action<string>? log = null) =>
        FileHelper.CopyAll(fromDirectory, toDirectory, searchOption: SearchOption.AllDirectories, predicate: file => file.HasAnyExtension(AllSubtitleExtensions), overwrite: overwrite);

    internal static void MoveSubtitlesForEpisodes(ISettings settings, string mediaDirectory, string subtitleDirectory = "", string subtitleBackupDirectory = "", bool overwrite = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        if (subtitleDirectory.IsNullOrEmpty())
        {
            subtitleDirectory = mediaDirectory;
        }

        if (subtitleBackupDirectory.IsNullOrWhiteSpace())
        {
            subtitleBackupDirectory = settings.TVSubtitleBackupDirectory;
        }

        string[] mediaDirectoryFiles = Directory.GetFiles(mediaDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
        Dictionary<string, string> episodeToVideos = mediaDirectoryFiles
            .Where(IsVideo)
            .Select(video => (video, SeasonEpisodeRegex.Match(PathHelper.GetFileNameWithoutExtension(video))))
            .Where(videoMatch => videoMatch.Item2.Success)
            .ToDictionary(videoMatch => videoMatch.Item2.Value, videoMatch => videoMatch.video);

        ILookup<string, string> episodeToSubtitles = (subtitleDirectory.EqualsIgnoreCase(mediaDirectory)
                ? mediaDirectoryFiles.AsEnumerable()
                : Directory.EnumerateFiles(subtitleDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories))
            .Where(IsSubtitle)
            .ToLookup(subtitle =>
            {
                Match match = SeasonEpisodeRegex.Match(PathHelper.GetFileNameWithoutExtension(subtitle));
                if (match.Success)
                {
                    return match.Value;
                }

                string subtitleDirectory = PathHelper.GetDirectoryName(subtitle);
                match = SeasonEpisodeRegex.Match(PathHelper.GetFileName(subtitleDirectory));
                return match.Success ? match.Value : string.Empty;
            });

        episodeToVideos
            .Where(pair => episodeToSubtitles.Contains(pair.Key))
            .ForEach(pair =>
            {
                string videoDirectory = PathHelper.GetDirectoryName(pair.Value);
                string videoName = PathHelper.GetFileNameWithoutExtension(pair.Value);
                episodeToSubtitles[pair.Key]
                    .Select(subtitle =>
                    {
                        string name = PathHelper.GetFileNameWithoutExtension(subtitle);
                        string title = name.StartsWithIgnoreCase(videoName)
                            ? name[videoName.Length..].Trim(Delimiter.Single())
                            : SeasonEpisodeRegex.IsMatch(name) ? name.Split(Delimiter).Last() : name;
                        Match match = Regex.Match(title, "[A-Za-z]{3}");
                        string language = match.Success ? match.Value.ToLowerInvariant() : "eng";
                        language = language switch
                        {
                            "jap" => "jpn",
                            "rom" => "rum",
                            "und" => "eng",
                            "sdh" => "eng",
                            "tra" => "cht",
                            "sim" => "chs",
                            "chi" when title.ContainsIgnoreCase("Simplified") => "chs",
                            "chi" when title.ContainsIgnoreCase("Traditional") => "cht",
                            "chi" => EncodingHelper.TryRead(subtitle, out string? content, out _) && !content.ContainsCommonTraditionalChineseCharacter() ? "chs" : "cht",
                            _ => language
                        };
                        log($"{language} | {title} | {name}");
                        string extension = PathHelper.GetExtension(subtitle);
                        string newSubtitleName = $"{videoName}{Delimiter}{language}{extension}";
                        string newBackupName = $"{videoName}{Delimiter}{title}{extension}";
                        log(newSubtitleName);
                        log(newBackupName);
                        log(string.Empty);
                        return (subtitle, language, newSubtitleName, newBackupName, extension);
                    })
                    .GroupBy(subtitle => subtitle.language)
                    .ForEach(group =>
                    {
                        Debug.Assert(group.Key is not "chi");

                        if (group.Count() == 1)
                        {
                            (string subtitle, string language, string newSubtitleName, string newBackupName, string extension) = group.Single();
                            string newSubtitle = group.Key is "eng"
                                ? PathHelper.ReplaceExtension(pair.Value, extension)
                                : Path.Combine(videoDirectory, newSubtitleName);
                            log(subtitle);
                            if (!isDryRun)
                            {
                                if (overwrite || !File.Exists(newSubtitle))
                                {
                                    FileHelper.Move(subtitle, newSubtitle, overwrite, true);
                                }
                                else
                                {
                                    FileHelper.Move(subtitle, Path.Combine(subtitleBackupDirectory, newBackupName), overwrite, true);
                                }
                            }

                            log(newSubtitle);
                        }
                        else
                        {
                            (string subtitle, string language, string newSubtitleName, string newBackupName, string extension)[] subtitles = group
                                .OrderByDescending(item => new FileInfo(item.subtitle).Length)
                                .ToArray();
                            if (group.Key is "eng")
                            {
                                (string subtitle, string language, string newSubtitleName, string newBackupName, string extension) = subtitles[0];
                                string newSubtitle = PathHelper.ReplaceExtension(pair.Value, extension);
                                log(subtitle);
                                if (!isDryRun)
                                {
                                    if (overwrite || !File.Exists(newSubtitle))
                                    {
                                        FileHelper.Move(subtitle, newSubtitle, overwrite, true);
                                    }
                                    else
                                    {
                                        FileHelper.Move(subtitle, Path.Combine(subtitleBackupDirectory, newBackupName), overwrite, true);
                                    }
                                }

                                log(newSubtitle);

                                (subtitle, language, newSubtitleName, newBackupName, extension) = subtitles[1];
                                newSubtitle = Path.Combine(videoDirectory, newSubtitleName);
                                log(subtitle);
                                if (!isDryRun)
                                {
                                    if (overwrite || !File.Exists(newSubtitle))
                                    {
                                        FileHelper.Move(subtitle, newSubtitle, overwrite, true);
                                    }
                                    else
                                    {
                                        FileHelper.Move(subtitle, Path.Combine(subtitleBackupDirectory, newBackupName), overwrite, true);
                                    }
                                }

                                log(newSubtitle);

                                subtitles = subtitles.Skip(2).ToArray();
                            }
                            else
                            {
                                (string subtitle, string language, string newSubtitleName, string newBackupName, string extension) = group
                                    .OrderByDescending(item => new FileInfo(item.subtitle).Length)
                                    .First();
                                string newSubtitle = Path.Combine(videoDirectory, newSubtitleName);
                                log(subtitle);
                                if (!isDryRun)
                                {
                                    if (overwrite || !File.Exists(newSubtitle))
                                    {
                                        FileHelper.Move(subtitle, newSubtitle, overwrite, true);
                                    }
                                    else
                                    {
                                        FileHelper.Move(subtitle, Path.Combine(subtitleBackupDirectory, newBackupName), overwrite, true);
                                    }
                                }

                                log(newSubtitle);

                                subtitles = subtitles.Skip(1).ToArray();
                            }

                            if (!isDryRun)
                            {
                                subtitles.ForEach(subtitle => FileHelper.Move(subtitle.subtitle, Path.Combine(subtitleBackupDirectory, subtitle.newBackupName), overwrite, true));
                            }
                        }
                    });
            });

        if (episodeToSubtitles.Contains(string.Empty))
        {
            episodeToSubtitles[string.Empty].ForEach(subtitle => log($"!No match {subtitle}"));
        }

        if (!isDryRun)
        {
            Directory
                .EnumerateDirectories(subtitleDirectory)
                .Where(directory => Directory.EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories).IsEmpty())
                .ToArray()
                .ForEach(DirectoryHelper.Recycle);
        }

        //Directory
        //    .EnumerateFiles(mediaDirectory, $"{PathHelper.AllSearchPattern}{mediaExtension}", SearchOption.AllDirectories)
        //    .ToArray()
        //    .ForEach(video =>
        //    {
        //        string match = Regex.Match(PathHelper.GetFileNameWithoutExtension(video), @"S[\d]+E[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
        //        string[] files = Directory
        //            .GetFiles(subtitleDirectory, $"{PathHelper.AllSearchPattern}{match}{PathHelper.AllSearchPattern}", SearchOption.AllDirectories);
        //        string subtitleBase = string.Empty;
        //        string subtitleVideo = files.SingleOrDefault(IsVideo, string.Empty);
        //        if (subtitleBase.IsNotNullOrWhiteSpace())
        //        {
        //            subtitleBase = PathHelper.GetFileNameWithoutExtension(subtitleVideo);
        //        }
        //        else
        //        {
        //            string subtitleMetadata = files.SingleOrDefault(IsXmlMetadata, string.Empty);
        //            if (subtitleMetadata.IsNotNullOrWhiteSpace())
        //            {
        //                subtitleBase = PathHelper.GetFileNameWithoutExtension(subtitleMetadata);
        //            }
        //        }

        //        files
        //            .Where(IsSubtitle)
        //            .ToArray()
        //            .ForEach(subtitle =>
        //            {
        //                string subtitleName = PathHelper.GetFileNameWithoutExtension(subtitle);
        //                string language = subtitleBase.IsNotNullOrWhiteSpace() && subtitleName.StartsWithIgnoreCase(subtitleBase)
        //                    ? subtitleName[subtitleBase.Length..].TrimStart(Delimiter.Single())
        //                    : subtitleName.Split(Delimiter).Last();
        //                if (!Regex.IsMatch(language, @"^([a-z]{3}(\&[a-z]{3})?(\-[a-z0-9]+)?)?$"))
        //                {
        //                    language = string.Empty;
        //                }

        //                string newSubtitleName = $"{PathHelper.GetFileNameWithoutExtension(video)}{(language.IsNullOrWhiteSpace() ? string.Empty : Delimiter)}{language}{PathHelper.GetExtension(subtitle)}";
        //                string newSubtitle = Path.Combine(PathHelper.GetDirectoryName(video), newSubtitleName);
        //                if (rename is not null)
        //                {
        //                    newSubtitle = rename(subtitle, newSubtitle);
        //                }

        //                if (overwrite)
        //                {
        //                    log($"Move {subtitle}");
        //                    if (!isDryRun)
        //                    {
        //                        FileHelper.Move(subtitle, newSubtitle, true);
        //                    }

        //                    log(newSubtitle);
        //                    log(string.Empty);
        //                }
        //                else if (!File.Exists(newSubtitle))
        //                {
        //                    log($"Move {subtitle}");
        //                    if (!isDryRun)
        //                    {
        //                        File.Move(subtitle, newSubtitle, false);
        //                    }

        //                    log(newSubtitle);
        //                    log(string.Empty);
        //                }
        //                else
        //                {
        //                    log($"Ignore {subtitle}");
        //                    log(string.Empty);
        //                }
        //            });
        //    });
    }

    internal static void MoveMovieSubtitleToParent(string directory, string subtitleBackupDirectory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        if (!Directory.Exists(subtitleBackupDirectory))
        {
            Directory.CreateDirectory(subtitleBackupDirectory);
        }

        Directory
            .EnumerateFiles(directory, "*.idx", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(directory, "*.sub", SearchOption.AllDirectories))
            .Where(subtitle => SubtitleDirectory.EqualsIgnoreCase(PathHelper.GetFileName(PathHelper.GetDirectoryName(subtitle))))
            .GroupBy(subtitle => PathHelper.GetDirectoryName(PathHelper.GetDirectoryName(subtitle)))
            .ToArray()
            .ForEach(movieGroup =>
            {
                string movieSubtitleDirectory = PathHelper.GetDirectoryName(movieGroup.First());
                string[] videos = Directory.GetFiles(movieGroup.Key, VideoSearchPattern, SearchOption.TopDirectoryOnly);
                if (videos.Length != 1)
                {
                    log($"!Video count is {videos.Length} in {movieGroup.Key}.");
                    return;
                }

                string video = videos.Single();
                string[] subtitles = movieGroup.ToArray();
                subtitles.ForEach(subtitle =>
                {
                    string name = PathHelper.GetFileNameWithoutExtension(subtitle);
                    Match match = Regex.Match(name, @"^(.+) \([0-9]+\)$");
                    if (match.Success)
                    {
                        string duplicate = PathHelper.ReplaceFileNameWithoutExtension(subtitle, match.Groups[1].Value);
                        if (!isDryRun && subtitles.ContainsIgnoreCase(duplicate))
                        {
                            FileHelper.Recycle(subtitle);
                            return;
                        }
                    }

                    string destinationSubtitle = Path.Combine(movieGroup.Key, $"{PathHelper.GetFileNameWithoutExtension(video)}{PathHelper.GetExtension(subtitle)}");
                    log(subtitle);
                    if (!isDryRun)
                    {
                        FileHelper.Move(subtitle, destinationSubtitle, false, true);
                    }

                    log(destinationSubtitle);
                    log(string.Empty);
                });

                if (!isDryRun)
                {
                    string[] files = Directory.GetFiles(movieSubtitleDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
                    if (files.IsEmpty() || files.All(file => file.HasExtension(".bak")))
                    {
                        DirectoryHelper.Recycle(movieSubtitleDirectory);
                    }
                }
            });

        Directory
            .EnumerateFiles(directory, "*.srt", SearchOption.AllDirectories)
            .Where(subtitle => SubtitleDirectory.EqualsIgnoreCase(PathHelper.GetFileName(PathHelper.GetDirectoryName(subtitle))))
            .GroupBy(subtitle => PathHelper.GetDirectoryName(PathHelper.GetDirectoryName(subtitle)))
            .Select(movieGroup => (
                Movie: movieGroup.Key,
                Subtitles: movieGroup.ToArray(),
                Videos: Directory.GetFiles(movieGroup.Key, VideoSearchPattern, SearchOption.TopDirectoryOnly)))
            .ToArray()
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .ForAll(movieGroup =>
            {
                if (movieGroup.Videos.Length != 1)
                {
                    log($"!Video count is {movieGroup.Videos.Length} in {movieGroup.Movie}.");
                    return;
                }

                HashSet<string> movieSubtitles = new(movieGroup.Subtitles, StringComparer.OrdinalIgnoreCase);
                movieGroup
                    .Subtitles
                    .ForEach(subtitle =>
                    {
                        string name = PathHelper.GetFileNameWithoutExtension(subtitle);
                        Match match = Regex.Match(name, @"^(.+) \([0-9]+\)$");
                        if (match.Success)
                        {
                            string duplicate = PathHelper.ReplaceFileNameWithoutExtension(subtitle, match.Groups[1].Value);
                            if (!isDryRun && movieGroup.Subtitles.ContainsIgnoreCase(duplicate))
                            {
                                FileHelper.Recycle(subtitle);
                                Debug.Assert(movieSubtitles.Remove(subtitle));
                            }
                        }
                    });

                string video = movieGroup.Videos.Single();
                string videoName = PathHelper.GetFileNameWithoutExtension(video);
                string[] originalNames = movieSubtitles
                    .Select(PathHelper.GetFileNameWithoutExtension)
                    .ToArray();
                Debug.Assert(originalNames.Length == movieSubtitles.Count);

                string[] postfixes = movieSubtitles
                    .Select((subtitle, index) =>
                    {
                        string originalName = originalNames[index].ToLowerInvariant();
                        string postfix = originalName;
                        Match match = Regex.Match(postfix, "^[0-9]+_(.+)$");
                        if (match.Success)
                        {
                            postfix = match.Groups[1].Value;
                        }
                        else
                        {
                            match = Regex.Match(postfix, @"^.+\.([a-z]{3})(\.hi)?$");
                            if (match.Success)
                            {
                                postfix = match.Groups[1].Value;
                            }
                        }

                        string content = string.Empty;
                        if (postfix is "subs" or "Sdh" or "sdh-sdh")
                        {
                            content = File.ReadAllText(subtitle);
                            if (content.ContainsCommonEnglish())
                            {
                                return "eng";
                            }
                        }

                        if (!postfix.ContainsOrdinal(Delimiter))
                        {
                            if (postfix.Length >= 3)
                            {
                                postfix = postfix[..3];
                            }
                            else
                            {
                                log($"Language is too short as {postfix} for {subtitle}");
                                return postfix;
                            }
                        }
                        else
                        {
                            string[] languages = postfix
                                .Split(Delimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                .Where(segment => Regex.IsMatch(segment, "^[a-z]{3}$"))
                                .ToArray();
                            if (languages.Length == 1)
                            {
                                postfix = languages.Single();
                            }
                            else if (languages.Any(language => language.StartsWithIgnoreCase("chi")))
                            {
                                postfix = "chi";
                            }
                            else
                            {
                                if (content.IsNullOrWhiteSpace())
                                {
                                    content = File.ReadAllText(subtitle);
                                }

                                if (content.ContainsCommonEnglish())
                                {
                                    return "eng";
                                }

                                log($"!Language cannot be detected from {postfix} for {subtitle}.");
                            }
                        }

                        if (postfix is "deu")
                        {
                            return "ger";
                        }

                        if (postfix is "fra")
                        {
                            return "fre";
                        }

                        if (postfix is "nld")
                        {
                            return "dut";
                        }

                        if (postfix is "und")
                        {
                            if (content.IsNullOrWhiteSpace())
                            {
                                content = File.ReadAllText(subtitle);
                            }

                            if (content.ContainsCommonEnglish())
                            {
                                return "eng";
                            }

                            if (content.ContainsChineseCharacter())
                            {
                                return content.ContainsCommonTraditionalChineseCharacter() ? "cht" : "chs";
                            }
                        }

                        if (postfix is "chi")
                        {
                            if (originalName.ContainsOrdinal("simplified"))
                            {
                                return "chs";
                            }

                            if (originalName.ContainsOrdinal("traditional"))
                            {
                                return "cht";
                            }

                            if (content.IsNullOrWhiteSpace())
                            {
                                content = File.ReadAllText(subtitle);
                            }

                            return content.ContainsCommonTraditionalChineseCharacter() ? "cht" : "chs";
                        }

                        return postfix;
                    })
                    .ToArray();
                Debug.Assert(postfixes.Length == movieSubtitles.Count);

                (string Language, (string Subtitle, string OriginalName, long Length)[] Subtitles)[] languageSubtitles = movieSubtitles
                    .Zip(postfixes, originalNames)
                    .GroupBy(subtitle => subtitle.Second, subtitle => (Subtitle: subtitle.First, OriginalName: subtitle.Third))
                    .Select(subtitleGroup =>
                    {
                        (string Subtitle, string OriginalName)[] subtitles = subtitleGroup.ToArray();
                        return (
                            Language: subtitleGroup.Key,
                            Subtitles: subtitles.Length == 1
                                ? subtitles
                                    .Select(subtitle => (subtitle.Subtitle, subtitle.OriginalName, 0L))
                                    .ToArray()
                                : subtitles
                                    .Select(subtitle => (subtitle.Subtitle, subtitle.OriginalName, new FileInfo(subtitle.Subtitle).Length))
                                    .OrderByDescending(subtitle => subtitle.Length)
                                    .ToArray());
                    })
                    .ToArray();

                languageSubtitles.ForEach(subtitleGroup =>
                {
                    if (subtitleGroup.Language is "ara" or "per")
                    {
                        subtitleGroup.Subtitles.ForEach(subtitle => FileHelper.Recycle(subtitle.Subtitle));
                        return;
                    }

                    if (subtitleGroup.Language is "eng")
                    {
                        (string Subtitle, string OriginalName, long Length) firstSubtitle = subtitleGroup.Subtitles.First();
                        string destinationFirstSubtitle = Path.Combine(movieGroup.Movie, $"{videoName}.srt");
                        string destinationSecondSubtitle = Path.Combine(movieGroup.Movie, $"{videoName}.eng.srt");
                        log(firstSubtitle.Subtitle);
                        if (!isDryRun)
                        {
                            if (File.Exists(destinationFirstSubtitle))
                            {
                                if (firstSubtitle.Length == 0)
                                {
                                    firstSubtitle.Length = new FileInfo(firstSubtitle.Subtitle).Length;
                                }

                                long destinationFirstSubtitleLength = new FileInfo(destinationFirstSubtitle).Length;
                                if (firstSubtitle.Length > destinationFirstSubtitleLength)
                                {
                                    FileHelper.Move(destinationFirstSubtitle, destinationSecondSubtitle, false, true);
                                    FileHelper.Move(firstSubtitle.Subtitle, destinationFirstSubtitle, false, true);
                                }
                                else if (firstSubtitle.Length == destinationFirstSubtitleLength)
                                {
                                    FileHelper.Move(firstSubtitle.Subtitle, Path.Combine(subtitleBackupDirectory, $"{videoName}{Delimiter}{firstSubtitle.OriginalName}.srt"), true, true);
                                }
                                else
                                {
                                    if (File.Exists(destinationSecondSubtitle))
                                    {
                                        long destinationSecondSubtitleLength = new FileInfo(destinationSecondSubtitle).Length;
                                        if (firstSubtitle.Length > destinationSecondSubtitleLength)
                                        {
                                            FileHelper.MoveToDirectory(destinationSecondSubtitle, subtitleBackupDirectory);
                                            FileHelper.Move(firstSubtitle.Subtitle, destinationSecondSubtitle, false, true);
                                        }
                                        else
                                        {
                                            FileHelper.Move(firstSubtitle.Subtitle, Path.Combine(subtitleBackupDirectory, $"{videoName}{Delimiter}{firstSubtitle.OriginalName}.srt"), false, true);
                                        }
                                    }
                                    else
                                    {
                                        FileHelper.Move(firstSubtitle.Subtitle, destinationSecondSubtitle, false, true);
                                    }
                                }
                            }
                            else
                            {
                                FileHelper.Move(firstSubtitle.Subtitle, destinationFirstSubtitle, false, true);
                            }
                        }

                        log(destinationFirstSubtitle);
                        log(string.Empty);

                        if (subtitleGroup.Subtitles.Length > 1)
                        {
                            (string Subtitle, string OriginalName, long Length) secondSubtitle = subtitleGroup.Subtitles[1];
                            log(secondSubtitle.Subtitle);
                            if (!isDryRun)
                            {
                                if (File.Exists(destinationSecondSubtitle))
                                {
                                    if (secondSubtitle.Length == 0)
                                    {
                                        secondSubtitle.Length = new FileInfo(secondSubtitle.Subtitle).Length;
                                    }

                                    long destinationSecondSubtitleLength = new FileInfo(destinationSecondSubtitle).Length;
                                    if (secondSubtitle.Length > destinationSecondSubtitleLength)
                                    {
                                        FileHelper.MoveToDirectory(destinationSecondSubtitle, subtitleBackupDirectory, false, true);
                                        FileHelper.Move(secondSubtitle.Subtitle, destinationSecondSubtitle, false, true);
                                    }
                                    else
                                    {
                                        FileHelper.Move(secondSubtitle.Subtitle, Path.Combine(subtitleBackupDirectory, $"{videoName}{Delimiter}{secondSubtitle.OriginalName}.srt"), false, true);
                                    }
                                }
                                else
                                {
                                    FileHelper.Move(secondSubtitle.Subtitle, destinationSecondSubtitle, false, true);
                                }
                            }

                            log(destinationSecondSubtitle);
                            log(string.Empty);
                        }

                        subtitleGroup
                            .Subtitles
                            .Skip(2)
                            .ForEach(subtitle =>
                            {
                                log(subtitle.Subtitle);
                                string destinationSubtitle = Path.Combine(subtitleBackupDirectory, $"{videoName}{Delimiter}{subtitle.OriginalName}.srt");
                                if (!isDryRun)
                                {
                                    FileHelper.Move(subtitle.Subtitle, destinationSubtitle, false, true);
                                }

                                log(destinationSubtitle);
                                log(string.Empty);
                            });
                    }
                    else
                    {
                        (string Subtitle, string OriginalName, long Length) firstSubtitle = subtitleGroup.Subtitles.First();
                        string destinationFirstSubtitle = Path.Combine(movieGroup.Movie, $"{videoName}{Delimiter}{subtitleGroup.Language}.srt");
                        log(firstSubtitle.Subtitle);
                        if (!isDryRun)
                        {
                            if (File.Exists(destinationFirstSubtitle))
                            {
                                if (firstSubtitle.Length == 0)
                                {
                                    firstSubtitle.Length = new FileInfo(firstSubtitle.Subtitle).Length;
                                }

                                long destinationFirstSubtitleLength = new FileInfo(destinationFirstSubtitle).Length;
                                if (firstSubtitle.Length > destinationFirstSubtitleLength)
                                {
                                    FileHelper.MoveToDirectory(destinationFirstSubtitle, subtitleBackupDirectory, false, true);
                                    FileHelper.Move(firstSubtitle.Subtitle, destinationFirstSubtitle, false, true);
                                }
                                else
                                {
                                    FileHelper.Move(firstSubtitle.Subtitle, Path.Combine(subtitleBackupDirectory, $"{videoName}{Delimiter}{subtitleGroup.Subtitles.First().OriginalName}.srt"), false, true);
                                }
                            }
                            else
                            {
                                FileHelper.Move(firstSubtitle.Subtitle, destinationFirstSubtitle, false, true);
                            }
                        }

                        log(destinationFirstSubtitle);
                        log(string.Empty);

                        subtitleGroup
                            .Subtitles
                            .Skip(1)
                            .ForEach(subtitle =>
                            {
                                log(subtitle.Subtitle);
                                string destinationSubtitle = Path.Combine(subtitleBackupDirectory, $"{videoName}{Delimiter}{subtitle.OriginalName}.srt");
                                if (!isDryRun)
                                {
                                    FileHelper.Move(subtitle.Subtitle, destinationSubtitle, false, true);
                                }

                                log(destinationSubtitle);
                                log(string.Empty);
                            });
                    }
                });

                string movieSubtitleDirectory = Path.Combine(movieGroup.Movie, SubtitleDirectory);

                if (!isDryRun)
                {
                    string[] files = Directory.GetFiles(movieSubtitleDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories);
                    Debug.Assert(files.IsEmpty() || files.All(file => file.HasExtension(".bak")));
                    DirectoryHelper.Recycle(movieSubtitleDirectory);
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
                if (content.ContainsChineseCharacter())
                {
                    languages.Add(content.ContainsCommonTraditionalChineseCharacter() ? "cht" : "chs");
                }

                if (content.ContainsCommonEnglish())
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
                    if (!File.ReadAllText(subtitle).ContainsChineseCharacter())
                    {
                        log($"!Not Chinese {subtitle}");
                    }
                }
                else
                {
                    if (File.ReadAllText(subtitle).ContainsChineseCharacter())
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
                    log(subtitle);
                }
                else
                {
                    FileHelper.Move(subtitle, newSubtitle);
                }
            });
    }

    private static void MoveSeasonSubtitlesToParent(string directory, string subtitleBackupDirectory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(IsSubtitle)
            .ToArray()
            .ForEach(subtitle =>
            {
                string name = PathHelper.GetFileNameWithoutExtension(subtitle);
                if (SeasonEpisodeRegex.IsMatch(name))
                {
                    return;
                }

                string subtitleDirectory = PathHelper.GetDirectoryName(subtitle);
                string subtitleDirectoryName = PathHelper.GetFileName(subtitleDirectory);
                if (SeasonEpisodeRegex.IsMatch(subtitleDirectoryName))
                {
                    string parentDirectory = PathHelper.GetDirectoryName(subtitleDirectory);
                    string newSubtitle = Path.Combine(parentDirectory, $"{subtitleDirectoryName}{Delimiter}{PathHelper.GetFileName(subtitle)}");
                    log(subtitle);
                    if (!isDryRun)
                    {
                        FileHelper.Move(subtitle, newSubtitle);
                    }

                    log(newSubtitle);
                    log(string.Empty);
                }
            });
    }

    internal static void FormatSubtitleSuffix(string directory, int seasonLevel = DefaultDirectoryLevel, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        RenameFiles(
            directory, 
            (file, index) => file
                .ReplaceIgnoreCase(".chi-中文_(繁體)", ".cht")
                .ReplaceIgnoreCase(".chi-中文_(简體)", ".chs")
                .ReplaceIgnoreCase(".chi-中文 (繁体)", ".cht")
                .ReplaceIgnoreCase(".chi-中文 (简体)", ".chs")
                .ReplaceIgnoreCase(".chi-中文(繁体)", ".cht")
                .ReplaceIgnoreCase(".chi-中文(简体)", ".chs")
                .ReplaceIgnoreCase(".chi-中文（繁體）", ".cht")
                .ReplaceIgnoreCase(".chi-中文（简體）", ".chs")
                .ReplaceIgnoreCase(".chi-Chinese_Traditional", ".cht")
                .ReplaceIgnoreCase(".chi-Chinese_Simplified", ".chs")
                .ReplaceIgnoreCase(".chi-Chinese_(Traditional)", ".cht")
                .ReplaceIgnoreCase(".chi-Chinese_(Simplified)", ".chs")
                .ReplaceIgnoreCase(".chi-Chinese_(Hong_Kong_SAR_China)", ".cht-cantonese")
                .ReplaceIgnoreCase(".chi-Chinese_(Hong_Kong)", ".cht-cantonese")
                .ReplaceIgnoreCase(".chi-Hong_Kong", ".cht-cantonese")
                .ReplaceIgnoreCase(".chi-Traditional_Chinese", ".cht")
                .ReplaceIgnoreCase(".chi-Simplified_Chinese", ".chs")
                .ReplaceIgnoreCase(".chi-Traditional", ".cht")
                .ReplaceIgnoreCase(".chi-Simplified", ".chs")
                .ReplaceIgnoreCase(".chi-Cht_SUP__原盘繁体", ".cht")
                .ReplaceIgnoreCase(".chi-Chs_SUP__原盘简体", ".chs")
                .ReplaceIgnoreCase(".chi-CHT&ENG", ".cht&eng")
                .ReplaceIgnoreCase(".chi-CHS&ENG", ".chs&eng")
                .ReplaceIgnoreCase(".chi-CHT", ".cht")
                .ReplaceIgnoreCase(".chi-CHS", ".chs")
                .ReplaceIgnoreCase(".eng-sdh", "")
                .ReplaceIgnoreCase(".eng-English", ".eng")
                .ReplaceIgnoreCase(".eng-Eng_SUP", ".eng")
                .ReplaceIgnoreCase(".eng-English_Regular___OCR", ".eng-ocr")
                .ReplaceIgnoreCase(".eng_Regular___OCR", ".eng-ocr")
        );

        EnumerateDirectories(directory, seasonLevel)
            .Where(season => !season.ContainsIgnoreCase(Featurettes))
            .ForEach(season =>
            {
                string[] files = Directory.GetFiles(season);
                string[] videoFiles = files.Where(f => f.IsVideo()).ToArray();
                (string nonVideo, string Name)[] nonVideoFiles = files.Except(videoFiles)
                    .Select(nonVideo => (nonVideo, Name: PathHelper.GetFileNameWithoutExtension(nonVideo)))
                    .ToArray();
                videoFiles.ForEach(video =>
                {
                    string videoName = PathHelper.GetFileNameWithoutExtension(video);
                    nonVideoFiles
                        .Where(nonVideo => nonVideo.Name.Length > videoName.Length && nonVideo.Name.StartsWith(videoName))
                        .ToArray()
                        .ForEach(nonVideo =>
                        {
                            string suffix = nonVideo.Name[videoName.Length..];
                            if (suffix.StartsWithIgnoreCase(".eng") || suffix.StartsWithIgnoreCase(".cht") || suffix.StartsWithIgnoreCase(".chs")
                                || Regex.IsMatch(suffix, @"^\.[a-z]{3}$"))
                            {
                                return;
                            }

                            if (Regex.IsMatch(suffix, @"^\.[a-z]{3}\-.+"))
                            {
                                string newSuffix = suffix[..4];
                                log(nonVideo.nonVideo);
                                log(FileHelper.ReplaceFileNameWithoutExtension(nonVideo.nonVideo, videoName + newSuffix));
                                log(string.Empty);
                            }
                            else
                            {
                                log("!" + nonVideo.nonVideo);
                                log(string.Empty);
                            }
                        });
                });
            });
    }
}