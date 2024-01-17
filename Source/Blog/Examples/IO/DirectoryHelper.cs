namespace Examples.IO;

using Examples.Common;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

public static class DirectoryHelper
{
    public static void Delete(string directory)
    {
        Directory.GetFiles(directory.NotNull()).ForEach(FileHelper.Delete);
        Directory.GetDirectories(directory).ForEach(Delete);
        SetAttributes(directory, FileAttributes.Normal);
        Directory.Delete(directory, false);
    }

    public static void Rename(this DirectoryInfo directory, string newName) => 
        Directory.Move(directory.NotNull().FullName, newName.NotNullOrWhiteSpace());

    public static bool TryRename(this DirectoryInfo directory, string newName)
    {
        if (directory.NotNull().Exists && !directory.Name.EqualsOrdinal(newName.NotNullOrWhiteSpace()))
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

    public static void Move(string source, string destination, bool overwrite = false)
    {
        source.NotNullOrWhiteSpace();

        if (overwrite && Directory.Exists(destination.NotNullOrWhiteSpace()))
        {
            Directory.Delete(destination);
        }

        string? parent = Path.GetDirectoryName(destination);
        if (!string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }

        if (!source.EqualsOrdinal(destination))
        {
            Directory.Move(source, destination);
        }
    }

    public static void MoveToDirectory(string source, string destinationParentDirectory, bool overwrite = false)
    {
        string destination = Path.Combine(destinationParentDirectory, PathHelper.GetFileName(source));
        Move(source, destination, overwrite);
    }

    public static void Empty(DirectoryInfo directory)
    {
        if (directory.NotNull().Exists)
        {
            directory.EnumerateFileSystemInfos().ForEach(fileSystemInfo => fileSystemInfo.Delete());
        }
    }

    public static void SetAttributes(string directory, FileAttributes fileAttributes) => 
        new DirectoryInfo(directory.NotNull()).Attributes = fileAttributes;

    public static void AddPrefix(string directory, string prefix) => 
        Directory.Move(directory, PathHelper.AddDirectoryPrefix(directory, prefix));

    public static void AddPostfix(string directory, string postfix) => 
        Directory.Move(directory, PathHelper.AddDirectoryPostfix(directory, postfix));

    public static void RenameFileExtensionToLowerCase(string directory) =>
        Directory
            .GetFiles(directory, "*", SearchOption.AllDirectories)
            .Where(file => Regex.IsMatch(Path.GetExtension(file), @"[A-Z]+"))
            .ToArray()
            .ForEach(file => File.Move(file, PathHelper.ReplaceFileName(file, PathHelper.GetExtension(file).ToLowerInvariant())));

    public static void MoveFiles(string sourceDirectory, string destinationDirectory, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories) =>
        Directory
            .GetFiles(sourceDirectory, searchPattern, searchOption)
            .ForEach(sourceFile =>
            {
                string destinationFile = Path.Combine(destinationDirectory, Path.GetRelativePath(sourceDirectory, sourceFile));
                string newDirectory = PathHelper.GetDirectoryName(destinationFile);
                if (!Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }

                File.Move(sourceFile, destinationFile);
            });

    public static void Recycle(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new ArgumentOutOfRangeException(nameof(directory), directory, "Not found.");
        }

        FileSystem.DeleteDirectory(directory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }

    public static void ReplaceDirectoryName(string directory, string newName, bool overwrite = false) => 
        Move(directory, PathHelper.ReplaceDirectoryName(directory, newName), overwrite);

    public static void ReplaceDirectoryName(string directory, Func<string, string> replace, bool overwrite = false) =>
        Move(directory, PathHelper.ReplaceDirectoryName(directory, replace), overwrite);

}