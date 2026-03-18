namespace Examples.IO;

using System.Buffers;
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

    public static void Move(string source, string destination, Func<string, string, bool> overwrite, bool skipDestinationDirectory = false)
    {
        source.ThrowIfNullOrWhiteSpace();

        string destinationDirectory = PathHelper.GetDirectoryName(destination);
        if (!skipDestinationDirectory && !Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Move(source, destination, overwrite(source, destination));
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

    public static string MoveToDirectory(string source, string destinationParentDirectory, Func<string, string, bool> overwrite, bool skipDestinationDirectory = false)
    {
        string destinationFile = Path.Combine(destinationParentDirectory, Path.GetFileName(source));
        Move(source, destinationFile, overwrite, skipDestinationDirectory);
        return destinationFile;
    }

    public static bool TryMoveToDirectory(string source, string destinationParentDirectory, bool overwrite = false) =>
        TryMove(source, Path.Combine(destinationParentDirectory, Path.GetFileName(source)), overwrite);

    public static string CopyToDirectory(string source, string destinationParentDirectory, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        string destination = Path.Combine(destinationParentDirectory, Path.GetFileName(source));
        Copy(source, destination, overwrite, skipDestinationDirectory);
        return destination;
    }

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

    public static async Task CopyAsync(string source, string destination)
    {
        await using Stream fromStream = File.OpenRead(source);
        await using Stream toStream = File.Create(destination);
        await fromStream.CopyToAsync(toStream);
    }

    public static string AddPrefix(string file, string prefix, bool overwrite = false)
    {
        string destinationFile = PathHelper.AddFilePrefix(file, prefix);
        File.Move(file, destinationFile, overwrite);
        return destinationFile;
    }

    public static string AddPostfix(string file, string postfix, bool overwrite = false)
    {
        string destinationFile = PathHelper.AddFilePostfix(file, postfix);
        File.Move(file, destinationFile, overwrite);
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

    public static async Task WriteTextAsync(string file, string text, bool @finally = false, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";

        if (@finally)
        {
            try { }
            finally
            {
                await File.WriteAllTextAsync(tempFile, text, encoding, cancellationToken);
            }
        }
        else
        {
            await File.WriteAllTextAsync(tempFile, text, encoding, cancellationToken);
        }

        if (File.Exists(file))
        {
            Delete(file);
        }

        File.Move(tempFile, file);
    }

    public static void WriteText(string file, string content, bool @finally = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";

        WriteText(file, tempFile, content, @finally, encoding);
    }

    public static void WriteText(string file, string content, ref readonly Lock @lock, bool @finally = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        string tempFile = $"{file}.tmp";

        lock (@lock)
        {
            WriteText(file, tempFile, content, @finally, encoding);
        }
    }

    private static void WriteText(string file, string tempFile, string content, bool @finally, Encoding encoding)
    {
        if (@finally)
        {
            try { }
            finally
            {
                File.WriteAllText(tempFile, content, encoding);
            }
        }
        else
        {
            File.WriteAllText(tempFile, content, encoding);
        }

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

    public static string ReplaceFileName(string file, string newFileName, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        string destination = PathHelper.ReplaceFileName(file, newFileName);
        Move(file, destination, overwrite, skipDestinationDirectory);
        return destination;
    }

    public static string ReplaceFileName(string file, Func<string, string> replace, bool overwrite = false, bool skipDestinationDirectory = false)
    {
        string destination = PathHelper.ReplaceFileName(file, replace);
        Move(file, destination, overwrite, skipDestinationDirectory);
        return destination;
    }

    public static string ReplaceFileNameWithoutExtension(string file, string newFileNameWithoutExtension, bool overwrite = false)
    {
        string destination = PathHelper.ReplaceFileNameWithoutExtension(file, newFileNameWithoutExtension);
        Move(file, destination, overwrite, true);
        return destination;
    }

    public static string ReplaceFileNameWithoutExtension(string file, Func<string, string> replace, bool overwrite = false)
    {
        string destination = PathHelper.ReplaceFileNameWithoutExtension(file, replace);
        Move(file, destination, overwrite, true);
        return destination;
    }

    private const int DefaultCopyBufferSize = 81920;

    public static async Task CopyAsync(string source, string destination, Action<long, long>? progress = null, int? bufferSize = DefaultCopyBufferSize, TimeSpan? reportingInterval = null, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(source))
        {
            throw new FileNotFoundException($"Source file not found : {source}.");
        }

        if (!overwrite && File.Exists(destination))
        {
            throw new IOException($"Destination file already exists: {destination}.");
        }

        reportingInterval ??= TimeSpan.FromSeconds(1);

        const int DefaultBufferSize = 4096;
        await using FileStream sourceStream = new(source, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, true);
        bufferSize ??= sourceStream.GetCopyBufferSize();
        await using FileStream destinationStream = new(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize.Value, true);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize.Value);
        bool isReported = false;
        long startingTimestamp = Stopwatch.GetTimestamp();
        try
        {
            long sourceSize = sourceStream.Length;
            long copiedSize = 0;
            for (int usedBufferSize = await sourceStream.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
                 usedBufferSize > 0;
                 usedBufferSize = await sourceStream.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await destinationStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, usedBufferSize), cancellationToken).ConfigureAwait(false);
                copiedSize += usedBufferSize;

                if (Stopwatch.GetElapsedTime(startingTimestamp) >= reportingInterval)
                {
                    startingTimestamp = Stopwatch.GetTimestamp();
                    progress?.Invoke(copiedSize, sourceSize);
                    if (copiedSize == sourceSize)
                    {
                        isReported = true;
                    }
                }
            }

            if (!isReported)
            {
                Debug.Assert(copiedSize == sourceSize);
                progress?.Invoke(copiedSize, sourceSize);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static int GetCopyBufferSize(this FileStream sourceStream)
    {
        // This value was originally picked to be the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo{Async} buffer is short-lived and is likely to be collected at Gen0, and it offers a significant improvement in Copy
        // performance.  Since then, the base implementations of CopyTo{Async} have been updated to use ArrayPool, which will end up rounding
        // this size up to the next power of two (131,072), which will by default be on the large object heap.  However, most of the time
        // the buffer should be pooled, the LOH threshold is now configurable and thus may be different than 85K, and there are measurable
        // benefits to using the larger buffer size.  So, for now, this value remains.
        int bufferSize = DefaultCopyBufferSize;

        if (sourceStream.CanSeek)
        {
            long length = sourceStream.Length;
            long position = sourceStream.Position;
            if (length <= position) // Handles negative overflows
            {
                // There are no bytes left in the stream to copy.
                // However, because CopyTo{Async} is virtual, we need to
                // ensure that any override is still invoked to provide its
                // own validation, so we use the smallest legal buffer size here.
                bufferSize = 1;
            }
            else
            {
                long remaining = length - position;
                if (remaining > 0)
                {
                    // In the case of a positive overflow, stick to the default size
                    bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }
        }

        return bufferSize;
    }

    public static bool TextEndsWith(string file, string value, StringComparison comparison, Encoding? filEncoding = null)
    {
        filEncoding ??= Encoding.UTF8;

        byte[] bytes = filEncoding.GetBytes(value);
        Array.Clear(bytes);

        using FileStream stream = new(file, FileMode.Open);
        long position = stream.Seek(0 - bytes.Length, SeekOrigin.End);
        if (position < 0)
        {
            return false;
        }

        stream.ReadExactly(bytes, 0, bytes.Length);
        string fileEnd = filEncoding.GetString(bytes);
        return string.Equals(fileEnd, value, comparison);
    }

    public static bool TextEndsWith(string file, string value, Encoding? filEncoding = null)
    {
        filEncoding ??= Encoding.UTF8;

        byte[] stringBytes = filEncoding.GetBytes(value);
        byte[] fileBytes = new byte[stringBytes.Length];

        using FileStream stream = new(file, FileMode.Open);
        long position = stream.Seek(0 - stringBytes.Length, SeekOrigin.End);
        if (position < 0)
        {
            return false;
        }

        stream.ReadExactly(fileBytes, 0, stringBytes.Length);
        return stringBytes.SequenceCompareTo(fileBytes) == 0;
    }

    public static bool TextEndsWithIgnoreCase(string file, string value, Encoding? filEncoding = null) =>
        TextEndsWith(file, value, StringComparison.OrdinalIgnoreCase, filEncoding);

    public static async Task<bool> TextEndsWithAsync(string file, string value, StringComparison comparison, Encoding? filEncoding = null, CancellationToken cancellation = default)
    {
        filEncoding ??= Encoding.UTF8;

        byte[] bytes = filEncoding.GetBytes(value);
        Array.Clear(bytes);

        using FileStream stream = new(file, FileMode.Open);
        long position = stream.Seek(0 - bytes.Length, SeekOrigin.End);
        if (position < 0)
        {
            return false;
        }

        await stream.ReadExactlyAsync(bytes, 0, bytes.Length, cancellation);
        string fileEnd = filEncoding.GetString(bytes);
        return string.Equals(fileEnd, value, comparison);
    }

    public static async Task<bool> TextEndsWithAsync(string file, string value, Encoding? filEncoding = null, CancellationToken cancellationToken = default)
    {
        filEncoding ??= Encoding.UTF8;

        byte[] stringBytes = filEncoding.GetBytes(value);
        byte[] fileBytes = new byte[stringBytes.Length];

        using FileStream stream = new(file, FileMode.Open);
        long position = stream.Seek(0 - stringBytes.Length, SeekOrigin.End);
        if (position < 0)
        {
            return false;
        }

        await stream.ReadExactlyAsync(fileBytes, 0, stringBytes.Length, cancellationToken);
        return stringBytes.SequenceCompareTo(fileBytes) == 0;
    }

    public static async Task<bool> TextEndsWithIgnoreCaseAsync(string file, string value, Encoding? filEncoding = null, CancellationToken cancellationToken = default) =>
        await TextEndsWithAsync(file, value, StringComparison.OrdinalIgnoreCase, filEncoding, cancellationToken);
}