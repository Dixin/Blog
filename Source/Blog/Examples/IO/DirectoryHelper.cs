namespace Examples.IO;

using Examples.Common;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

public static class DirectoryHelper
{
    public static void Delete(string directory)
    {
        Directory.GetFiles(directory.ThrowIfNull()).ForEach(FileHelper.Delete);
        Directory.GetDirectories(directory).ForEach(Delete);
        SetAttributes(directory, FileAttributes.Normal);
        Directory.Delete(directory, false);
    }

    public static void Rename(this DirectoryInfo directory, string newName) =>
        Directory.Move(directory.ThrowIfNull().FullName, newName.ThrowIfNullOrWhiteSpace());

    public static bool TryRename(this DirectoryInfo directory, string newName)
    {
        if (directory.ThrowIfNull().Exists && !directory.Name.EqualsOrdinal(newName.ThrowIfNullOrWhiteSpace()))
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

    public static void Move(string source, string destination, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        source.ThrowIfNullOrWhiteSpace();
        destination.ThrowIfNullOrWhiteSpace();

        if (overwrite && (skipDestinationDirectory || Directory.Exists(destination)))
        {
            Directory.Delete(destination);
        }

        string? parent = Path.GetDirectoryName(destination);
        if (!skipDestinationDirectory && !string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }

        if (!source.EqualsOrdinal(destination))
        {
            bool isHidden = IsHidden(source);
            try
            {
                FileSystem.MoveDirectory(source, destination);
            }
            catch (IOException ioException) when (ioException.Message.EqualsIgnoreCase("Could not complete operation since source directory and target directory are the same."))
            {
                return;
            }

            if (isHidden != IsHidden(destination))
            {
                SetHidden(destination, isHidden);
            }
        }
    }

    public static string MoveToDirectory(string source, string destinationParentDirectory, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        string destination = Path.Combine(destinationParentDirectory, PathHelper.GetFileName(source));
        Move(source, destination, overwrite);
        return destination;
    }

    public static void Empty(DirectoryInfo directory)
    {
        if (directory.ThrowIfNull().Exists)
        {
            directory.EnumerateFileSystemInfos().ForEach(fileSystemInfo => fileSystemInfo.Delete());
        }
    }

    public static void SetAttributes(string directory, FileAttributes fileAttributes) =>
        new DirectoryInfo(directory.ThrowIfNull()).Attributes = fileAttributes;

    public static string AddPrefix(string directory, string prefix)
    {
        string destinationDirectory = PathHelper.AddDirectoryPrefix(directory, prefix);
        Directory.Move(directory, destinationDirectory);
        return destinationDirectory;
    }

    public static string AddPostfix(string directory, string postfix)
    {
        string destinationDirectory = PathHelper.AddDirectoryPostfix(directory, postfix);
        Directory.Move(directory, destinationDirectory);
        return destinationDirectory;
    }

    public static void RenameFileExtensionToLowerCase(params string[][] drives) =>
        drives
            .AsParallel()
            .ForAll(drive => drive
                .ForEach(directory => Directory
                    .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                    .Where(file => PathHelper.GetExtension(file).Any(@char => @char is >= 'A' and <= 'Z'))
                    .ToArray()
                    .ForEach(file => FileHelper.ReplaceFileName(file, $"{PathHelper.GetFileNameWithoutExtension(file)}{PathHelper.GetExtension(file).ToLowerInvariant()}"))));

    public static void MoveFiles(string sourceDirectory, string destinationDirectory, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories, bool overwrite = false, bool skipDestinationDirectory = false) =>
        Directory
            .GetFiles(sourceDirectory, searchPattern, searchOption)
            .ForEach(sourceFile =>
            {
                string destinationFile = Path.Combine(destinationDirectory, Path.GetRelativePath(sourceDirectory, sourceFile));
                string newDirectory = PathHelper.GetDirectoryName(destinationFile);
                if (!skipDestinationDirectory && !Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }

                File.Move(sourceFile, destinationFile, overwrite);
            });

    public static void Recycle(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new ArgumentOutOfRangeException(nameof(directory), directory, "Not found.");
        }

        FileSystem.DeleteDirectory(directory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }

    public static string ReplaceDirectoryName(string directory, string newName, bool overwrite = false)
    {
        string destination = PathHelper.ReplaceDirectoryName(directory, newName);
        Move(directory, destination, overwrite);
        return destination;
    }

    public static string ReplaceDirectoryName(string directory, Func<string, string> replace, bool overwrite = false)
    {
        string destination = PathHelper.ReplaceDirectoryName(directory, replace);
        Move(directory, destination, overwrite);
        return destination;
    }

    public static void Copy(string sourceDirectory, string destinationDirectory, bool overwrite = false) =>
        FileSystem.CopyDirectory(sourceDirectory, destinationDirectory, overwrite);

    public static void CopyToDirectory(string sourceDirectory, string destinationParentDirectory, bool overwrite = false) =>
        FileSystem.CopyDirectory(sourceDirectory, Path.Combine(destinationParentDirectory, Path.GetFileName(sourceDirectory)), overwrite);

    public static bool IsHidden(string directory) =>
        new DirectoryInfo(directory.ThrowIfNullOrWhiteSpace()).Attributes.HasFlag(FileAttributes.Hidden);

    public static void SetHidden(string directory, bool isHidden)
    {
        if (isHidden)
        {
            new DirectoryInfo(directory.ThrowIfNullOrWhiteSpace()).Attributes |= FileAttributes.Hidden;
        }
        else
        {
            new DirectoryInfo(directory.ThrowIfNullOrWhiteSpace()).Attributes &= ~FileAttributes.Hidden;
        }
    }

    public static void DeleteEmptySubDirectory(string directory, bool isPermanent = false)
    {
        Directory.GetDirectories(directory).ForEach(subDirectory =>
        {
            DeleteEmptySubDirectory(subDirectory);

            if (Directory.EnumerateFileSystemEntries(subDirectory).Any())
            {
                return;
            }

            if (isPermanent)
            {
                Directory.Delete(subDirectory);
            }
            else
            {
                Recycle(subDirectory);
            }
        });
    }
}