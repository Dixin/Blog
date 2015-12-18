namespace Dixin.Text.GB2312ToUnicode
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Dixin.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                throw new ArgumentException("Path is not provided.", nameof(args));
            }

            string path = args.First();
            if (Directory.Exists(path))
            {
                Directory.EnumerateFiles(path, "*.txt")
                    .ToList()
                    .ForEach(file => EncodingHelper.Convert(
                        Encoding.GetEncoding("gb2312"), Encoding.Unicode, file));
            }
            else if (File.Exists(path))
            {
                EncodingHelper.Convert(Encoding.GetEncoding("gb2312"), Encoding.Unicode, path);
            }

            throw new ArgumentException("Provided path does not exist.", nameof(args));
        }
    }
}
