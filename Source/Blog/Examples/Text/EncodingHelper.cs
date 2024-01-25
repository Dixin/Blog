namespace Examples.Text;

using Examples.Common;
using Ude;

public static class EncodingHelper
{
    public static byte[] Utf8Bom { get; } = Encoding.UTF8.GetPreamble();

    public static string Convert(string value, Encoding from, Encoding to)
    {
        to.ThrowIfNull();

        byte[] fromBytes = from.ThrowIfNull().GetBytes(value);
        byte[] toBytes = Encoding.Convert(from, to, fromBytes);

        return to.GetString(toBytes);
    }

    public static string GB2312ToUtf8(this string gb2312Value) => Convert(gb2312Value, Encoding.GetEncoding("gb2312"), Encoding.UTF8);

    public static string Utf8ToGB2312(this string utf8Value) => Convert(utf8Value, Encoding.UTF8, Encoding.GetEncoding("gb2312"));

    public static async Task ConvertAsync(Encoding from, Encoding to, string fromPath, string? toPath = null, byte[]? bom = null)
    {
        from.ThrowIfNull();
        to.ThrowIfNull();

        byte[] fromBytes = await File.ReadAllBytesAsync(fromPath.ThrowIfNullOrWhiteSpace());
        byte[] toBytes = Encoding.Convert(from, to, fromBytes);
        await File.WriteAllBytesAsync(toPath ?? fromPath, toBytes);
        await using FileStream fileStream = new(toPath ?? fromPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        if (bom is not null && !bom.SequenceEqual(toBytes.Take(bom.Length)))
        {
            await fileStream.WriteAsync(bom);
        }

        await fileStream.WriteAsync(toBytes);
    }

    public static bool TryDetect(string file, [NotNullWhen(true)] out Encoding? encoding)
    {
        using FileStream fileStream = File.OpenRead(file);
        byte[] head = new byte[Utf8Bom.Length];
        int readCount = fileStream.Read(head, 0, Utf8Bom.Length);
        if (readCount == Utf8Bom.Length && Utf8Bom.SequenceEqual(head))
        {
            encoding = Encoding.UTF8;
            return true;
        }

        long position = fileStream.Seek(0, SeekOrigin.Begin);
        Debug.Assert(position == 0L);
        CharsetDetector detector = new();
        detector.Feed(fileStream);
        detector.DataEnd();
        encoding = detector.Charset?.ToUpperInvariant() switch
        {
            "UTF-16LE" => Encoding.Unicode,
            "GB18030" => Encoding.GetEncoding("gb18030"),
            "WINDOWS-1251" => Encoding.GetEncoding(1251),
            "WINDOWS-1252" => Encoding.GetEncoding(1252),
            "BIG5" => Encoding.GetEncoding("big5"),
            "ASCII" => Encoding.ASCII,
            "UTF-8" => Encoding.UTF8,
            _ => null
        };

        return encoding is not null;
    }

    public static bool TryRead(string file, [NotNullWhen(true)] out string? content, [NotNullWhen(true)] out Encoding? encoding)
    {
        if (TryDetect(file, out encoding))
        {
            content = File.ReadAllText(file, encoding);
            return true;
        }

        content = null;
        return false;
    }
}