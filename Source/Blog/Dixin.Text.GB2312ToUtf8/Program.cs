namespace Dixin.Text.GB2312ToUnicode
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string path = args.First();
            if (Directory.Exists(path))
            {
                await Task.WhenAll(Directory.EnumerateFiles(path, args.Length >= 2 ? args[1] : "*.txt", SearchOption.AllDirectories)
                    .ToList()
                    .Select(async file => await ConvertAsync(Encoding.GetEncoding("gb2312"), Encoding.UTF8, file)));
            }
            else if (File.Exists(path))
            {
                await ConvertAsync(Encoding.GetEncoding("gb2312"), Encoding.UTF8, path);
            }
            else
            {
                throw new ArgumentException("Provided path does not exist.", nameof(args));
            }
        }

        public static async Task ConvertAsync(Encoding from, Encoding to, string fromPath, string toPath = null)
        {
            byte[] fromBytes = await File.ReadAllBytesAsync(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            await File.WriteAllBytesAsync(toPath ?? fromPath, toBytes);
        }
    }
}
