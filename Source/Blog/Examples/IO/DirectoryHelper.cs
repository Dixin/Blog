namespace Examples.IO
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Examples.Common;

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

            Directory.Move(directory.FullName, newName);
        }

        public static bool TryRename(this DirectoryInfo directory, string newName)
        {
            directory.NotNull(nameof(directory));
            newName.NotNullOrWhiteSpace(nameof(newName));

            if (directory.Exists && !directory.Name.EqualsOrdinal(newName))
            {
                try
                {
                    Directory.Move(directory.FullName, newName);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                }

                return true;
            }

            return false;
        }

        public static void Move(string source, string destination, bool overwrite)
        {
            source.NotNullOrWhiteSpace(nameof(source));
            destination.NotNullOrWhiteSpace(nameof(destination));

            if (overwrite && Directory.Exists(destination))
            {
                Directory.Delete(destination);
            }
            Directory.Move(source, destination);
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

        public static void AddPrefix(string directory, string prefix)
        {
            Directory.Move(directory, PathHelper.AddDirectoryPrefix(directory, prefix));
        }

        public static void AddPostfix(string directory, string postfix)
        {
            Directory.Move(directory, PathHelper.AddDirectoryPostfix(directory, postfix));
        }

        internal static void RenameFileExtensionToLowerCase(string directory)
        {
            Directory
                .GetFiles(directory, "*", SearchOption.AllDirectories)
                .Where(file => Regex.IsMatch(file.Split(".").Last(), @"[A-Z]+"))
                .ToArray()
                .ForEach(file =>
                {
                    string newFile = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}{Path.GetExtension(file).ToLowerInvariant()}");
                    File.Move(file, newFile);
                });
        }
    }
}