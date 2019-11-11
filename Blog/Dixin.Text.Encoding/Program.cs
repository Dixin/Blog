namespace Dixin.Text.Encode
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length != 5)
            {
                throw new ArgumentException(nameof(args));
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Encoding fromEncoding = Encoding.GetEncoding(args[0]);
            Encoding toEncoding = Encoding.GetEncoding(args[1]);
            string path = args[2];
            string searchPattern = args[3];

            if (Directory.Exists(path))
            {
                foreach (string file in Directory.EnumerateFiles(path, searchPattern, "all".Equals(args[4], StringComparison.OrdinalIgnoreCase) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    await CopyAsync(file, file + ".bak");
                    await ConvertAsync(fromEncoding, toEncoding, file);
                }
            }
            else if (File.Exists(path))
            {
                await CopyAsync(path, path + ".bak");
                await ConvertAsync(fromEncoding, toEncoding, path);
            }
            else
            {
                throw new ArgumentException("Provided path does not exist.", nameof(args));
            }
        }

        private static async Task ConvertAsync(Encoding from, Encoding to, string fromPath, string toPath = null)
        {
            byte[] fromBytes = await File.ReadAllBytesAsync(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            await File.WriteAllBytesAsync(toPath ?? fromPath, toBytes);
        }

        private static async Task CopyAsync(string fromPath, string toPath)
        {
            using Stream fromStream = File.OpenRead(fromPath);
            using Stream toStream = File.Create(toPath);
            await fromStream.CopyToAsync(toStream);
        }
    }
}
