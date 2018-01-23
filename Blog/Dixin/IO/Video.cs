namespace Dixin.IO
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Dixin.Common;

    internal static class Video
    {
        private const string Separator = ".";

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
                        catch (Exception exception) when(exception.IsNotCritical())
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
