namespace Examples.IO;

using Examples.Common;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

public static class FileHelper
{
    public static void Delete(string file)
    {
        // new FileInfo(toAttachment).IsReadOnly = false;
        File.SetAttributes(file.ThrowIfNullOrWhiteSpace(), FileAttributes.Normal); // In case file is read only.
        File.Delete(file);
    }

    public static bool Contains(string file, string find, Encoding? encoding = null, StringComparison comparison = StringComparison.Ordinal) =>
        File.ReadAllText(file.ThrowIfNullOrWhiteSpace(), encoding ?? Encoding.UTF8).Contains(find, comparison);

    public static void Replace(string file, string find, string? replace = null, Encoding? encoding = null)
    {
        file.ThrowIfNullOrWhiteSpace();
        replace ??= string.Empty;
        encoding ??= Encoding.UTF8;

        string text = File.ReadAllText(file, encoding).Replace(find, replace);
        File.WriteAllText(file, text, encoding);
    }

    public static void Rename(this FileInfo file, string newName) =>
        file.ThrowIfNull().MoveTo(newName.ThrowIfNullOrWhiteSpace());

    public static void Move(string source, string destination, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        source.ThrowIfNullOrWhiteSpace();

        string destinationDirectory = PathHelper.GetDirectoryName(destination);
        if (!skipDestinationDirectory && !Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Move(source, destination, overwrite);
    }

    public static bool TryMove(string source, string destination, bool overwrite = false)
    {
        source.ThrowIfNullOrWhiteSpace();

        if (!overwrite && File.Exists(destination))
        {
            return false;
        }

        string destinationDirectory = PathHelper.GetDirectoryName(destination);
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Move(source, destination, overwrite);
        return true;
    }

    public static string MoveToDirectory(string source, string destinationParentDirectory, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        string destinationFile = Path.Combine(destinationParentDirectory, Path.GetFileName(source));
        Move(source, destinationFile, overwrite, skipDestinationDirectory);
        return destinationFile;
    }

    public static bool TryMoveToDirectory(string source, string destinationParentDirectory, bool overwrite = false) =>
        TryMove(source, Path.Combine(destinationParentDirectory, Path.GetFileName(source)), overwrite);

    public static void CopyToDirectory(string source, string destinationParentDirectory, bool overwrite = false, bool skipDestinationDirectory = false) =>
        Copy(source, Path.Combine(destinationParentDirectory, Path.GetFileName(source)), overwrite, skipDestinationDirectory);

    public static void Copy(string source, string destination, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        source.ThrowIfNullOrWhiteSpace();

        string destinationDirectory = PathHelper.GetDirectoryName(destination);
        if (!skipDestinationDirectory && !Directory.Exists(destinationDirectory))
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

    public static string AddPrefix(string file, string prefix)
    {
        string destinationFile = PathHelper.AddFilePrefix(file, prefix);
        File.Move(file, destinationFile);
        return destinationFile;
    }

    public static string AddPostfix(string file, string postfix)
    {
        string destinationFile = PathHelper.AddFilePostfix(file, postfix);
        File.Move(file, destinationFile);
        return destinationFile;
    }

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

    public static async Task WriteTextAsync(string file, string text, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";

        await File.WriteAllTextAsync(tempFile, text, encoding, cancellationToken);
        if (File.Exists(file))
        {
            Delete(file);
        }

        File.Move(tempFile, file);
    }

    public static void WriteText(string file, string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";

        WriteTextImplementation(file, tempFile, text, encoding);
    }

    public static void WriteText(string file, string text, ref readonly object @lock, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";

        lock (@lock)
        {
            WriteTextImplementation(file, tempFile, text, encoding);
        }
    }

    private static void WriteTextImplementation(string file, string tempFile, string text, Encoding encoding)
    {
        File.WriteAllText(tempFile, text, encoding);
        if (File.Exists(file))
        {
            Recycle(file);
        }

        File.Move(tempFile, file);
    }

    public static void Recycle(string file)
    {
        if (!File.Exists(file.ThrowIfNullOrWhiteSpace()))
        {
            throw new ArgumentOutOfRangeException(nameof(file), file, "Not found.");
        }

        // new FileInfo(toAttachment).IsReadOnly = false;
        File.SetAttributes(file, FileAttributes.Normal); // In case file is read only.
        FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }

    public static void ReplaceFileName(string file, string newFileName, bool overwrite = false) =>
        Move(file, PathHelper.ReplaceFileName(file, newFileName), overwrite);

    public static void ReplaceFileNameWithoutExtension(string file, string newFileNameWithoutExtension, bool overwrite = false, bool skipDestinationDirectory = false) =>
        Move(file, PathHelper.ReplaceFileNameWithoutExtension(file, newFileNameWithoutExtension), overwrite, skipDestinationDirectory);

    public static void ReplaceFileNameWithoutExtension(string file, Func<string, string> replace, bool overwrite = false, bool skipDestinationDirectory = false) =>
        Move(file, PathHelper.ReplaceFileNameWithoutExtension(file, replace), overwrite, skipDestinationDirectory);
}