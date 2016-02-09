namespace Dixin.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Dixin.Common;

    internal static class Music
    {
        private const char Separater = '.';

        private static readonly HashSet<string> Extensions = new HashSet<string>(
            new[] { ".mp3", ".m4a", ".wma" }, StringComparer.OrdinalIgnoreCase);

        internal static void RenameAlbum(string from, string to)
        {
            DirectoryInfo fromDirectory = new DirectoryInfo(from);
            DirectoryInfo toDirectory = new DirectoryInfo(to);

            bool hasError = false;

            fromDirectory
                .EnumerateFiles()
                .Where(song => IsMusicFile(song.Extension) && IsNotFormated(song.Name))
                .ForEach(song =>
                    {
                        Trace.WriteLine(song.Name);
                        hasError = true;
                    });

            if (hasError)
            {
                throw new OperationCanceledException();
            }

            fromDirectory
                .EnumerateFiles()
                .ForEach(song =>
                    {
                        if (!IsMusicFile(song.Extension))
                        {
                            return;
                        }

                        string[] names = song.Name.Split(Separater);
                        string year = names[0];
                        string albumName = names[1];
                        string artistName = names[3];
                        string genre = names[5];

                        string newAlbumName = $"{genre}{Separater}{artistName}{Separater}{year}{Separater}{albumName}";
                        DirectoryInfo newAlbum = new DirectoryInfo(Path.Combine(toDirectory.FullName, newAlbumName));
                        if (!newAlbum.Exists)
                        {
                            newAlbum.Create();
                        }

                        song.MoveTo(Path.Combine(newAlbum.FullName, song.Name));
                    });
        }

        internal static void RenameAllAlbums(string from, string to)
        {
            bool hasError = false;
            DirectoryInfo music = new DirectoryInfo(from);

            music
                .EnumerateDirectories()
                .SelectMany(artist => artist.EnumerateDirectories())
                .SelectMany(album => album.EnumerateFiles())
                .Where(song => IsMusicFile(song.Extension) && IsNotFormated(song.Name))
                .ForEach(song =>
                    {
                        Trace.WriteLine(song.Name);
                        hasError = true;
                    });

            if (hasError)
            {
                return;
            }

            music
                .EnumerateDirectories()
                .SelectMany(artist => artist.EnumerateDirectories())
                .ForEach(album =>
                    {
                        IEnumerable<FileInfo> songs = album.EnumerateFiles()
                                .Where(song => IsMusicFile(song.Extension));
                        if (songs.IsEmpty())
                        {
                            Trace.WriteLine(album.Name);
                        }
                        else
                        {
                            string[] names = songs.First().Name.Split(Separater);
                            string artistName = names[3];
                            string year = names[0];
                            string albumName = names[1];
                            string genre = names[5];
                            if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(year)
                                || string.IsNullOrWhiteSpace(artistName))
                            {
                                Trace.WriteLine(album.Name);
                            }
                            else
                            {
                                string newAlbumName = $"{genre}{Separater}{artistName}{Separater}{year}{Separater}{albumName}";
                                if (!album.Name.EqualsIgnoreCase(newAlbumName))
                                {
                                    album.Rename(newAlbumName);
                                }

                                DirectoryInfo newAlbum = new DirectoryInfo(Path.Combine(album.Parent.FullName, newAlbumName));
                                newAlbum.MoveTo(Path.Combine(to, newAlbumName));
                            }
                        }
                    });
        }

        private static bool IsMusicFile(string extension) => Extensions.Contains(extension);

        private static bool IsNotFormated(string fileName)
        {
            string[] names = fileName.Split(Separater);
            return names.Length != 7
                || names[0].Length != 4
                || (names[2].Length != 2 && names[2].Length != 3 && names.Any(string.IsNullOrWhiteSpace));
        }
    }
}