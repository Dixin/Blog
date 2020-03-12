namespace Dixin.Text.ToUtf8
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Ude;

    internal static class Program
    {
        private static readonly byte[] Utf8Bom = Encoding.UTF8.GetPreamble();

        private static readonly TextWriter Log = Console.Out;

        private static async Task Main(string[] args)
        {
            string path = args[0];
            string searchPattern = args.Length >= 2 ? args[1] : "*.txt";
            int degree = args.Length >= 3 && int.TryParse(args[2], out degree) ? degree : Environment.ProcessorCount;
            Encoding fromEncoding = args.Length >= 4 ? Encoding.GetEncoding(args[3]) : null;
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
                throw new ArgumentException("Provided path does not exist.", nameof(args));
            }
        }

        private static async ValueTask<bool> HasUtf8BomAsync(this string file)
        {
            await using FileStream stream = File.OpenRead(file);
            byte[] head = new byte[Utf8Bom.Length];
            int read = await stream.ReadAsync(head, 0, head.Length);
            return read == Utf8Bom.Length && Utf8Bom.SequenceEqual(head);
        }

        private static async Task ConvertAsync(string file, Encoding fromEncoding)
        {
            if (fromEncoding != null)
            {
                if (await BackUpAndConvertAsync(file, fromEncoding))
                {
                    Log.WriteLine($"Converted {fromEncoding}, {file}");
                }
            }
            else
            {
                await using FileStream fileStream = File.OpenRead(file);
                CharsetDetector detector = new CharsetDetector();
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
                        Log.WriteLine($"!Not supported {detector.Charset}, {detector.Confidence}, {file}. {exception}");
                        return;
                    }

                    if (await BackUpAndConvertAsync(file, fromEncoding))
                    {
                        Log.WriteLine($"Converted {detector.Charset}, {detector.Confidence}, {file}");
                    }
                }
                else
                {
                    Log.WriteLine($"!Not detected {detector.Charset}, {detector.Confidence}, {file}");
                }
            }
        }

        private static void RemoveReadOnly(this string file)
        {
            FileInfo fileInfo = new FileInfo(file);
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

        private static async Task<bool> BackUpAndConvertAsync(string file, Encoding fromEncoding)
        {
            if (Encoding.UTF8.Equals(fromEncoding) && await file.HasUtf8BomAsync())
            {
                return false;
            }

            file.RemoveReadOnly();
            file.BackUp();
            await ConvertAsync(fromEncoding, Encoding.UTF8, file, bom: Utf8Bom);
            return true;
        }

        private static async Task ConvertAsync(Encoding from, Encoding to, string file, string toPath = null, byte[] bom = null)
        {
            byte[] fromBytes = await File.ReadAllBytesAsync(file);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            if (bom != null && !bom.SequenceEqual(toBytes.Take(bom.Length)))
            {
                toBytes = bom.Concat(toBytes).ToArray();
            }
            await File.WriteAllBytesAsync(toPath ?? file, toBytes);
        }
    }
}
