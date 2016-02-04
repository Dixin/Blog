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
        private const string Separater = ".";

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
                    string[] names = Path.GetFileName(album).Split(Separater.ToCharArray());
                    if ((otherLettersFirst && names.Last().HasOtherLetter()) || (!otherLettersFirst && names.First().HasOtherLetter()))
                    {
                        new DirectoryInfo(album).Rename(string.Join(Separater, names.Last(), names[0], names.First()));
                    }
                });
        }

        private static bool IsNotFormated(string album)
        {
            string[] names = Path.GetFileName(album).Split(Separater.ToCharArray());
            int result;
            return names.Length != 3
                || names[1].Length != 4
                || !int.TryParse(names[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                || string.IsNullOrWhiteSpace(names.First()) || string.IsNullOrWhiteSpace(names.Last())
                || (names.First().HasOtherLetter() && names.Last().HasOtherLetter());
        }
    }
}
