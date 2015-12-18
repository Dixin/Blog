namespace Dixin.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualBasic.FileIO;

    public static class DirectoryHelper
    {
        public static void Delete(string directory)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(directory));

            Directory.EnumerateFiles(directory).ForEach(FileHelper.Delete);
            Directory.EnumerateDirectories(directory).ForEach(Delete);
            SetAttributes(directory, FileAttributes.Normal);
            Directory.Delete(directory, false);
        }

        public static void Rename(this DirectoryInfo directory, string newName)
        {
            Contract.Requires<ArgumentNullException>(directory != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(newName));

            FileSystem.RenameDirectory(directory.FullName, newName);
        }

        public static void Rename(string directory, string newName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(directory));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(newName));

            new DirectoryInfo(directory).Rename(newName);
        }

        public static void Empty(DirectoryInfo directory)
        {
            Contract.Requires<ArgumentNullException>(directory != null);

            if (directory.Exists)
            {
                directory.EnumerateFileSystemInfos().ForEach(fileSystemInfo => fileSystemInfo.Delete());
            }
        }

        public static void SetAttributes(string directory, FileAttributes fileAttributes)
        {
            Contract.Requires<ArgumentNullException>(directory != null);

            new DirectoryInfo(directory).Attributes = FileAttributes.Normal;
        }
    }
}