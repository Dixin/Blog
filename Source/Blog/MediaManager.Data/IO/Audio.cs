namespace Examples.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Examples.Text;
    using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
    using TagLib;
    using TagFile = TagLib.File;

    internal static class Audio
    {
        private static readonly string[] TraditionChineseException = { "黄霑" };

        private static readonly string[] Attachments = { "cover", "clearart", "cdart", "back", "Introduction", "booklet", "box" };

        private static void TraceLog(string? message) => Trace.WriteLine(message);

        internal static void ReplaceTraditionalChinese(string directory, bool isDryRun = false, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetTags(directory, (audio, tagFile, tag) =>
            {
                if (tag.Performers.Any(name => name.HasChineseCharacter() && !TraditionChineseException.Any(exception => name.Contains(exception, StringComparison.InvariantCulture))))
                {
                    string[] performers = tag.Performers
                        .Select(name => ChineseConverter.Convert(name, ChineseConversionDirection.TraditionalToSimplified))
                        .ToArray();
                    if (!tag.Performers.SequenceEqual(performers, StringComparer.InvariantCulture))
                    {
                        log(audio);
                        tag.Performers.ForEach(log);
                        performers.ForEach(log);
                        log(string.Empty);
                        if (!isDryRun)
                        {
                            tag.Performers = performers;
                            tagFile.Save();
                        }
                    }

                }

                if (tag.AlbumArtists.Any(name => name.HasChineseCharacter() && !TraditionChineseException.Any(name.Contains)))
                {
                    string[] albumArtists = tag.AlbumArtists
                        .Select(name => ChineseConverter.Convert(name, ChineseConversionDirection.TraditionalToSimplified))
                        .ToArray();
                    if (!tag.AlbumArtists.SequenceEqual(albumArtists, StringComparer.InvariantCulture))
                    {
                        log(audio);
                        tag.AlbumArtists.ForEach(log);
                        albumArtists.ForEach(log);
                        log(string.Empty);
                        if (!isDryRun)
                        {
                            tag.AlbumArtists = albumArtists;
                            tagFile.Save();
                        }
                    }
                }

                if (tag.Title.HasChineseCharacter())
                {
                    string title = ChineseConverter.Convert(tag.Title, ChineseConversionDirection.TraditionalToSimplified);
                    if (!string.Equals(tag.Title, title, StringComparison.InvariantCulture))
                    {
                        log(audio);
                        log(tag.Title);
                        log(title);
                        log(string.Empty);
                        if (!isDryRun)
                        {
                            tag.Title = title;
                            tagFile.Save();
                        }
                    }
                }

                if (tag.Album.HasChineseCharacter())
                {
                    string album = ChineseConverter.Convert(tag.Album, ChineseConversionDirection.TraditionalToSimplified);
                    if (!string.Equals(tag.Album, album, StringComparison.InvariantCulture))
                    {
                        log(audio);
                        log(tag.Album);
                        log(album);
                        log(string.Empty);
                        if (!isDryRun)
                        {
                            tag.Album = album;
                            tagFile.Save();
                        }
                    }
                }
            });
        }

        internal static void ReplaceName(string directory, bool isDryRun = false, Action<string>? log = null, params (string from, string to)[] names)
        {
            log ??= TraceLog;
            GetTags(directory, (audio, tagFile, tag) => names.ForEach(name =>
            {
                (string from, string to) = name;
                if (tag.Performers.Any(name => name.Contains(from, StringComparison.InvariantCultureIgnoreCase)))
                {
                    tag.Performers = tag.Performers
                        .Select(name => name.Replace(from, to, StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();
                    tag.Performers.ForEach(log);
                    log(string.Empty);
                    if (!isDryRun)
                    {
                        tagFile.Save();
                    }
                }

                if (tag.AlbumArtists.Any(name => name.Contains(from, StringComparison.InvariantCultureIgnoreCase)))
                {
                    tag.AlbumArtists = tag.AlbumArtists
                        .Select(name => name.Replace(from, to, StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();
                    tag.AlbumArtists.ForEach(log);
                    log(string.Empty);
                    if (!isDryRun)
                    {
                        tagFile.Save();
                    }
                }
            }));
        }

        internal static void PrintTraditionalChinese(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetTags(directory, (audio, tagFile, tag) =>
            {
                if (tag.Performers.Any(name => name.HasChineseCharacter())
                    || tag.AlbumArtists.Any(name => name.HasChineseCharacter()))
                {
                    log(audio);
                    tag.AlbumArtists.ForEach(name => log(name));
                    tag.Performers.ForEach(name => log(name));
                    log(string.Empty);
                }
            });
        }

        internal static void PrintInvalidCharacters(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            GetTags(directory, (audio, tagFile, tag) =>
            {
                if (tag.Performers.Any(name => name.HasInvalidFileNameCharacter()))
                {
                    log(audio);
                    tag.Performers.ForEach(name => log(name));
                    log(string.Empty);
                }

                if (tag.AlbumArtists.Any(name => name.HasInvalidFileNameCharacter()))
                {
                    log(audio);
                    tag.AlbumArtists.ForEach(name => log(name));
                    log(string.Empty);
                }

                if (tag.Title.HasInvalidFileNameCharacter())
                {
                    log(audio);
                    log(tag.Title);
                    log(string.Empty);
                }

                if (tag.Album.HasInvalidFileNameCharacter())
                {
                    log(audio);
                    log(tag.Album);
                    log(string.Empty);
                }
            });
        }

        internal static void GetTags(string directory, Action<string, TagFile, Tag> action)
        {
            Directory.EnumerateDirectories(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                .OrderBy(album => album)
                .ForEach(album => Directory
                    .EnumerateFiles(album, "*.mp3", SearchOption.TopDirectoryOnly)
                    .OrderBy(audio => audio)
                    .ToArray()
                    .ForEach(audio =>
                    {
                        using TagFile tagFile = TagFile.Create(audio);
                        Tag tag = tagFile.Tag;
                        action(audio, tagFile, tag);
                    }));
        }

        internal static void GetTagsByAlbum(string directory, Action<(string Audio, TagFile TagFile, Tag Tag)[]> action)
        {
            Directory.EnumerateDirectories(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                .OrderBy(album => album)
                .ForEach(album =>
                {
                    (string audio, TagFile tagFile, Tag tag)[] allMetadata = Directory
                        .EnumerateFiles(album, "*.mp3", SearchOption.TopDirectoryOnly)
                        .OrderBy(audio => audio)
                        .Select(audio =>
                        {
                            TagFile tagFile = TagFile.Create(audio);
                            Tag tag = tagFile.Tag;
                            return (audio, tagFile, tag);
                        })
                        .ToArray();
                    action(allMetadata);
                    allMetadata.ForEach(metadata => metadata.tagFile?.Dispose());
                });
        }

        internal static void PrintDirectoriesWithErrors(string directory, Action<string>? log = null)
        {
            log ??= TraceLog;
            Directory
                .GetDirectories(directory, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly)
                .ForEach(album =>
                {
                    string[] files = Directory.GetFiles(album, PathHelper.AllSearchPattern, SearchOption.TopDirectoryOnly);
                    string[] audios = files.Where(f => f.EndsWith(".mp3")).ToArray();
                    string[] attachments = files.Where(f => Attachments.Contains(Path.GetFileNameWithoutExtension(f))).ToArray();
                    string[] metadata = files.Where(f => Path.GetFileName(f) == "album.nfo").ToArray();

                    files.Except(audios).Except(attachments).Except(metadata).ForEach(log);
                });
        }
    }
}