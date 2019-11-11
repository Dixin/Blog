namespace Dixin.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Dixin.Common;
    using Xabe.FFmpeg;
    using Xabe.FFmpeg.Streams;

    internal static class Video
    {
        private const string Separator = ".";

        internal static void MoveCaption(string mediaDirectory, string mediaExtension, string subtitleDirectory)
        {
            Directory
                .EnumerateFiles(mediaDirectory, $"*{mediaExtension}", SearchOption.AllDirectories)
                .ForEach(video =>
                {
                    string match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"s[\d]+e[\d]+", RegexOptions.IgnoreCase).Value.ToLowerInvariant();
                    Directory
                        .EnumerateFiles(subtitleDirectory, $"*{match}*", SearchOption.AllDirectories)
                        .Where(file => !file.EndsWith(mediaExtension))
                        .ForEach(subtitle =>
                        {
                            string language = Path.GetFileNameWithoutExtension(subtitle).Split(Separator.ToCharArray()).Last();
                            string newSubtitle = $"{Path.GetFileNameWithoutExtension(video)}.{language}{Path.GetExtension(subtitle)}";
                            File.Move(subtitle, Path.Combine(Path.GetDirectoryName(video), newSubtitle));
                        });
                });

        }

        internal static void RenameSeasons(string mediaDirectory)
        {
            Directory
                .EnumerateDirectories(mediaDirectory)
                .OrderBy(seasion => seasion)
                .ForEach((season, index) =>
                {
                    try
                    {
                        Directory.Move(season, Path.Combine(Path.GetDirectoryName(season), $"Season {(index + 1).ToString("D2")}"));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                });
        }

        internal static void Convert(string[] files)
        {
            files
                .ForEach(file =>
                {
                    if (!File.Exists(file))
                    {
                        return;
                    }

                    using (Process process = new Process())
                    {
                        string target = file + ".mp4";
                        process.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "ffmpeg",
                            Arguments = $@"-i ""{file}"" ""{target}"" -y",
                            UseShellExecute = true
                        };

                        if (process.Start())
                        {
                            process.WaitForExit();
                            if (File.Exists(target) && new FileInfo(target).Length > 1024)
                            {
                                File.Move(file, file + ".temp");
                            }
                            else
                            {
                                Console.WriteLine($"Fail: {file}");
                            }
                        }
                    }
                });
        }

        internal static string[] FindVideos()
        {
            List<string> messages = new List<string>();
            List<string> files = new List<string>();
            foreach (string file in Directory.EnumerateFiles(@"G:\Files\Temp\Backup\System Volume Information\Overflow\Temp\Move\", "*.mp4", SearchOption.AllDirectories))
            {
                try
                {
                    if (!Task.Run(async () =>
                    {
                         IMediaInfo mediaInfo = await MediaInfo.Get(file);
                         foreach (IVideoStream videoStream in mediaInfo.VideoStreams)
                         {
                             if (!string.Equals("h264", videoStream.Format, StringComparison.OrdinalIgnoreCase))
                             {
                                 string message = $"{videoStream.Format}: {file}";
                                 Console.WriteLine(message);
                                 messages.Add(message);
                                 files.Add(file);
                             }
                         }
                     }).Wait(TimeSpan.FromSeconds(10)))
                    {
                        Console.WriteLine($"TIMEOUT: {file}");
                    }
                }
                catch (Exception exception)
                {
                    string message = $"FAIL: {file} {exception}";
                    Console.WriteLine(message);
                    messages.Add(message);
                    files.Add(file);
                }
            }

            messages.ForEach(Console.WriteLine);
            files.ForEach(Console.WriteLine);

            return files.ToArray();
        }

        internal static void RenameAllAlbums(string root, bool otherLettersFirst = false)
        {
            bool hasError = false;
            Directory.EnumerateDirectories(root).Where(IsNotFormated).ForEach(album =>
                {
                    Trace.WriteLine(album);
                    hasError = true;
                });
            if (hasError)
            {
                throw new OperationCanceledException();
            }

            Directory.EnumerateDirectories(root).ForEach(album =>
                {
                    string[] names = Path.GetFileName(album).Split(Separator.ToCharArray());
                    if ((otherLettersFirst && names.Last().HasOtherLetter()) || (!otherLettersFirst && names.First().HasOtherLetter()))
                    {
                        new DirectoryInfo(album).Rename(string.Join(Separator, names.Last(), names[1], names.First()));
                    }
                });
        }

        internal static void FixAlbums(string source, string target)
        {
            Directory.EnumerateDirectories(target).ForEach(targetAlbum =>
                {
                    string name = Path.GetFileName(targetAlbum).Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
                    string[] sources = Directory.EnumerateDirectories(source)
                        .Where(sourceAlbum =>
                            name.EqualsOrdinal(Path.GetFileName(sourceAlbum).Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First()))
                        .ToArray();
                    if (sources.Length == 1)
                    {
                        try
                        {
                            new DirectoryInfo(targetAlbum).Rename(Path.GetFileName(sources.Single()));
                        }
                        catch (Exception exception) when (exception.IsNotCritical())
                        {
                        }

                    }
                });
        }

        private static bool IsNotFormated(string album)
        {
            string[] names = Path.GetFileName(album).Split(Separator.ToCharArray());
            int result;
            return names.Length != 3
                || names[1].Length != 4
                || !int.TryParse(names[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                || string.IsNullOrWhiteSpace(names.First()) || string.IsNullOrWhiteSpace(names.Last())
                || (names.First().HasOtherLetter() && names.Last().HasOtherLetter());
        }
    }
}
