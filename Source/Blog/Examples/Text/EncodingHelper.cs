namespace Examples.Text
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Examples.Common;

    public static class EncodingHelper
    {
        public static string Convert(string value, Encoding from, Encoding to)
        {
            from.NotNull(nameof(from));
            to.NotNull(nameof(to));

            byte[] fromBytes = from.GetBytes(value);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);

            return to.GetString(toBytes);
        }

        public static string GB2312ToUtf8
            (this string gb2312Value) => Convert(gb2312Value, Encoding.GetEncoding("gb2312"), Encoding.UTF8);

        public static string Utf8ToGB2312
            (this string utf8Value) => Convert(utf8Value, Encoding.UTF8, Encoding.GetEncoding("gb2312"));

        public static async Task Convert(Encoding from, Encoding to, string fromPath, string? toPath = null, byte[]? bom = null)
        {
            byte[] fromBytes = await File.ReadAllBytesAsync(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            await File.WriteAllBytesAsync(toPath ?? fromPath, toBytes);
            await using FileStream fileStream = new(toPath ?? fromPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            if (bom != null && !bom.SequenceEqual(toBytes.Take(bom.Length)))
            {
                await fileStream.WriteAsync(bom, 0, bom.Length);
            }
            await fileStream.WriteAsync(toBytes, 0, toBytes.Length);
        }
    }
}
