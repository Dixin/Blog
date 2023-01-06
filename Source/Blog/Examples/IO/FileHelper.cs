namespace Examples.IO;

using Examples.Common;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

public static class FileHelper
{
    public static void Delete(string file)
    {
        File.SetAttributes(file.NotNullOrWhiteSpace(), FileAttributes.Normal); // In case file is read only.
        File.Delete(file);
    }

    public static bool Contains(string file, string find, Encoding? encoding = null, StringComparison comparison = StringComparison.Ordinal) =>
        File.ReadAllText(file.NotNullOrWhiteSpace(), encoding ?? Encoding.UTF8).Contains(find, comparison);

    public static void Replace(string file, string find, string? replace = null, Encoding? encoding = null)
    {
        file.NotNullOrWhiteSpace();
        replace ??= string.Empty;
        encoding ??= Encoding.UTF8;

        string text = File.ReadAllText(file, encoding).Replace(find, replace);
        File.WriteAllText(file, text, encoding);
    }

    public static void Rename(this FileInfo file, string newName) =>
        file.NotNull().MoveTo(newName.NotNullOrWhiteSpace());

    public static void Move(string source, string destination, bool overwrite = false)
    {
        source.NotNullOrWhiteSpace();

        string destinationDirectory = Path.GetDirectoryName(destination) ?? throw new InvalidOperationException(destination);
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Move(source, destination, overwrite);
    }

    public static void MoveToDirectory(string source, string destinationDirectory, bool overwrite = false)
    {
        string destination = Path.Combine(destinationDirectory, Path.GetFileName(source));
        Move(source, destination, overwrite);
    }

    public static void Copy(string source, string destination, bool overwrite = false)
    {
        source.NotNullOrWhiteSpace();

        string destinationDirectory = Path.GetDirectoryName(destination) ?? throw new InvalidOperationException(destination);
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Copy(source, destination, overwrite);
    }

    public static void Backup(string file, bool overwrite = false)
    {
        string backUp = $"{file}.bak";
        if (File.Exists(backUp))
        {
            FileInfo backupFile = new(backUp);
            if (backupFile.IsReadOnly)
            {
                backupFile.IsReadOnly = false;
            }
        }

        File.Copy(file, backUp, overwrite);
    }

    public static async Task CopyAsync(string fromPath, string toPath)
    {
        await using Stream fromStream = File.OpenRead(fromPath);
        await using Stream toStream = File.Create(toPath);
        await fromStream.CopyToAsync(toStream);
    }

    public static void AddPrefix(string file, string prefix) =>
        File.Move(file, PathHelper.AddFilePrefix(file, prefix));

    public static void AddPostfix(string file, string postfix) =>
        File.Move(file, PathHelper.AddFilePostfix(file, postfix));

    public static void MoveAll(string sourceDirectory, string destinationDirectory, string searchPattern = PathHelper.AllSearchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, bool overwrite = false) =>
        Directory
            .EnumerateFiles(sourceDirectory, searchPattern, searchOption)
            .Where(file => predicate?.Invoke(file) ?? true)
            .ToArray()
            .ForEach(subtitle => Move(subtitle, subtitle.Replace(sourceDirectory, destinationDirectory, StringComparison.InvariantCulture), overwrite));

    public static void CopyAll(string sourceDirectory, string destinationDirectory, string searchPattern = PathHelper.AllSearchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, bool overwrite = false) =>
        Directory
            .EnumerateFiles(sourceDirectory, searchPattern, searchOption)
            .Where(file => predicate?.Invoke(file) ?? true)
            .ToArray()
            .ForEach(subtitle => Copy(subtitle, subtitle.Replace(sourceDirectory, destinationDirectory, StringComparison.InvariantCulture), overwrite));

    public static async Task SaveAndReplaceAsync(string file, string content, Encoding? encoding = null, object? @lock = null)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";
        if (@lock is not null)
        {
            lock (@lock)
            {
                File.WriteAllText(tempFile, content, encoding);
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                File.Move(tempFile, file);
            }
        }
        else
        {
            await File.WriteAllTextAsync(tempFile, content, encoding);
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            File.Move(tempFile, file);
        }
    }

    public static void Recycle(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentOutOfRangeException(nameof(file), file, "Not found.");
        }

        FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }

    public static void ReplaceFileName(string file, string newFileName, bool overwrite = false) =>
        Move(file, PathHelper.ReplaceFileName(file, newFileName), overwrite);

    public static void ReplaceFileNameWithoutExtension(string file, string newFileNameWithoutExtension, bool overwrite = false) =>
        Move(file, PathHelper.ReplaceFileNameWithoutExtension(file, newFileNameWithoutExtension), overwrite);

    public static void ReplaceFileNameWithoutExtension(string file, Func<string, string> replace, bool overwrite = false) =>
        Move(file, PathHelper.ReplaceFileNameWithoutExtension(file, replace), overwrite);
}