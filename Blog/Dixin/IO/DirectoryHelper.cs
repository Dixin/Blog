namespace Dixin.IO
{
    using System;
    using System.IO;
    using System.Linq;

    using Dixin.Common;

    using Microsoft.VisualBasic.FileIO;

    public static class DirectoryHelper
    {
        public static void Delete(string directory)
        {
            directory.NotNullOrWhiteSpace(nameof(directory));

            Directory.EnumerateFiles(directory).ForEach(FileHelper.Delete);
            Directory.EnumerateDirectories(directory).ForEach(Delete);
            SetAttributes(directory, FileAttributes.Normal);
            Directory.Delete(directory, false);
        }

        public static void Rename(this DirectoryInfo directory, string newName)
        {
            directory.NotNull(nameof(directory));
            newName.NotNullOrWhiteSpace(nameof(newName));

            FileSystem.RenameDirectory(directory.FullName, newName);
        }

        public static bool TryRename(this DirectoryInfo directory, string newName)
        {
            directory.NotNull(nameof(directory));
            newName.NotNullOrWhiteSpace(nameof(newName));

            if (directory.Exists && !directory.Name.EqualsOrdinal(newName))
            {
                try
                {
                    FileSystem.RenameDirectory(directory.FullName, newName);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                }

                return true;
            }

            return false;
        }

        public static void Rename(string directory, string newName)
        {
            directory.NotNullOrWhiteSpace(nameof(directory));
            newName.NotNullOrWhiteSpace(nameof(newName));

            new DirectoryInfo(directory).Rename(newName);
        }

        public static void Empty(DirectoryInfo directory)
        {
            directory.NotNull(nameof(directory));

            if (directory.Exists)
            {
                directory.EnumerateFileSystemInfos().ForEach(fileSystemInfo => fileSystemInfo.Delete());
            }
        }

        public static void SetAttributes(string directory, FileAttributes fileAttributes)
        {
            directory.NotNull(nameof(directory));

            new DirectoryInfo(directory).Attributes = fileAttributes;
        }
    }
}