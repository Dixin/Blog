using Ude;

const int MaxDegreeOfParallelism = 4;

string path = args[0];
string searchPattern = args.Length >= 2 ? args[1] : "*.txt";
int degree = args.Length >= 3 && int.TryParse(args[2], out degree) ? degree : Math.Max(Environment.ProcessorCount, MaxDegreeOfParallelism);
Encoding? fromEncoding = args.Length >= 4 ? Encoding.GetEncoding(args[3]) : null;

if (Directory.Exists(path))
{
    Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
        .AsParallel()
        .WithDegreeOfParallelism(degree)
        .ForAll(file => ConvertAsync(file, fromEncoding).Wait());
}
else if (File.Exists(path))
{
    await ConvertAsync(path, fromEncoding);
}
else
{
    throw new ArgumentException($"The provided path {path} does not exist.", nameof(args));
}

static void Log(string message)
{
    Console.WriteLine(message);
    Trace.WriteLine(message);
}

static async Task ConvertAsync(string file, Encoding? fromEncoding)
{
    if (fromEncoding is not null)
    {
        if (await file.BackUpAndConvertAsync(fromEncoding))
        {
            Log($"Converted {fromEncoding}, {file}");
        }
    }
    else
    {
        await using FileStream fileStream = File.OpenRead(file);
        CharsetDetector detector = new();
        detector.Feed(fileStream);
        detector.DataEnd();
        if (detector.Confidence > 0.5 && !string.IsNullOrWhiteSpace(detector.Charset))
        {
            try
            {
                fromEncoding = Encoding.GetEncoding(detector.Charset);

            }
            catch (ArgumentException exception)
            {
                Log($"!Not supported {detector.Charset}, {detector.Confidence}, {file}. {exception}");
                return;
            }

            if (await file.BackUpAndConvertAsync(fromEncoding))
            {
                Log($"Converted {detector.Charset}, {detector.Confidence}, {file}");
            }
        }
        else
        {
            Log($"!Not detected {detector.Charset}, {detector.Confidence}, {file}");
        }
    }
}

internal static class Extensions
{
    private static readonly byte[] Utf8Bom = Encoding.UTF8.GetPreamble();

    private static async ValueTask<bool> HasUtf8BomAsync(this string file)
    {
        await using FileStream stream = File.OpenRead(file);
        byte[] head = new byte[Utf8Bom.Length];
        int read = await stream.ReadAsync(head, 0, head.Length);
        return read == Utf8Bom.Length && Utf8Bom.SequenceEqual(head);
    }

    private static void RemoveReadOnly(this string file)
    {
        FileInfo fileInfo = new(file);
        if (fileInfo.IsReadOnly)
        {
            fileInfo.IsReadOnly = false;
        }
    }

    private static void BackUp(this string file)
    {
        string backUpFile = file + ".bak";
        if (File.Exists(backUpFile))
        {
            backUpFile.RemoveReadOnly();
        }

        File.Copy(file, backUpFile, true);
    }

    internal static async Task<bool> BackUpAndConvertAsync(this string file, Encoding fromEncoding)
    {
        if (Encoding.UTF8.Equals(fromEncoding) && await file.HasUtf8BomAsync())
        {
            return false;
        }

        file.RemoveReadOnly();
        file.BackUp();
        await file.ConvertAsync(fromEncoding, Encoding.UTF8, bom: Utf8Bom);
        return true;
    }

    private static async Task ConvertAsync(this string file, Encoding from, Encoding to, string? toPath = null, byte[]? bom = null)
    {
        byte[] fromBytes = await File.ReadAllBytesAsync(file);
        byte[] toBytes = Encoding.Convert(from, to, fromBytes);
        if (bom is not null && !bom.SequenceEqual(toBytes.Take(bom.Length)))
        {
            toBytes = bom.Concat(toBytes).ToArray();
        }
        await File.WriteAllBytesAsync(string.IsNullOrWhiteSpace(toPath) ? file : toPath, toBytes);
    }
}
