namespace Dixin.Text.GB2312ToUnicode
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class Program
    {
        private static void Main(string[] args)
        {
            string path = args.First();
            if (Directory.Exists(path))
            {
                Directory.EnumerateFiles(path, args.Length >= 2 ? args[1] : "*.txt", SearchOption.AllDirectories)
                    .ToList()
                    .ForEach(file => Convert(Encoding.GetEncoding("gb2312"), Encoding.UTF8, file));
            }
            else if (File.Exists(path))
            {
                Convert(Encoding.GetEncoding("gb2312"), Encoding.UTF8, path);
            }
            else
            {
                throw new ArgumentException("Provided path does not exist.", nameof(args));
            }
        }

        public static void Convert(Encoding from, Encoding to, string fromPath, string toPath = null)
        {
            byte[] fromBytes = File.ReadAllBytes(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            File.WriteAllBytes(toPath ?? fromPath, toBytes);
        }
    }
}
