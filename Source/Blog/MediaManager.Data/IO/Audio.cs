namespace Examples.IO;

using System.Drawing;
using Examples.Common;
using Examples.Text;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using TagLib;
using TagFile = TagLib.File;

internal static class Audio
{
    private static readonly string[] TraditionChineseException = { "黄霑" };

    private static readonly string[] Attachments = { "cover", "clearart", "cdart", "back", "Introduction", "booklet", "box" };

    private const string AudioExtension = ".mp3";

    private static void TraceLog(string? message) => Trace.WriteLine(message);

    internal static void ReplaceTraditionalChinese(string directory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= TraceLog;
        GetTags(directory, (audio, tagFile, tag) =>
        {
            if (tag.Performers.Any(name => name.ContainsChineseCharacter() && !TraditionChineseException.Any(name.ContainsOrdinal)))
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

            if (tag.AlbumArtists.Any(name => name.ContainsChineseCharacter() && !TraditionChineseException.Any(name.ContainsOrdinal)))
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

            if (tag.Title.ContainsChineseCharacter())
            {
                string title = ChineseConverter.Convert(tag.Title, ChineseConversionDirection.TraditionalToSimplified);
                if (!tag.Title.EqualsOrdinal(title))
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

            if (tag.Album.ContainsChineseCharacter())
            {
                string album = ChineseConverter.Convert(tag.Album, ChineseConversionDirection.TraditionalToSimplified);
                if (!tag.Album.EqualsOrdinal(album))
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
            if (tag.Performers.Any(name => name.ContainsIgnoreCase(from)))
            {
                tag.Performers = tag.Performers
                    .Select(name => name.ReplaceIgnoreCase(from, to))
                    .ToArray();
                tag.Performers.ForEach(log);
                log(string.Empty);
                if (!isDryRun)
                {
                    tagFile.Save();
                }
            }

            if (tag.AlbumArtists.Any(name => name.ContainsIgnoreCase(from)))
            {
                tag.AlbumArtists = tag.AlbumArtists
                    .Select(name => name.ReplaceIgnoreCase(from, to))
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
            if (tag.Performers.Any(name => name.ContainsChineseCharacter())
                || tag.AlbumArtists.Any(name => name.ContainsChineseCharacter()))
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
                string[] audios = files.Where(f => f.EndsWithIgnoreCase(AudioExtension)).ToArray();
                string[] attachments = files.Where(f => Attachments.ContainsIgnoreCase(Path.GetFileNameWithoutExtension(f))).ToArray();
                string[] metadata = files.Where(f => Path.GetFileName(f).EqualsIgnoreCase("album.nfo")).ToArray();

                files.Except(audios).Except(attachments).Except(metadata).ForEach(log);
            });
    }

    public static void SavePicture(string tagFile, string pictureFile)
    {
        using TagFile audioFile = TagFile.Create(tagFile);
        IPicture? picture = audioFile.Tag.Pictures.FirstOrDefault();
        if (picture is null)
        {
            throw new ArgumentOutOfRangeException(nameof(tagFile), tagFile, "The file contains no pictures.");
        }

        using MemoryStream ms = new(audioFile.Tag.Pictures[0].Data.Data);
        using Image image = Image.FromStream(ms);
        image.Save(pictureFile);
    }
}