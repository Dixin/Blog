namespace Examples.IO
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Examples.Common;

    public static class FileHelper
    {
        public static void Delete(string file)
        {
            file.NotNullOrWhiteSpace(nameof(file));

            File.SetAttributes(file, FileAttributes.Normal); // In case file is read only.
            File.Delete(file);
        }

        public static bool Contains(string file, string find, Encoding? encoding = null)
        {
            file.NotNullOrWhiteSpace(nameof(file));

            encoding ??= Encoding.UTF8;
            return File.ReadAllText(file, encoding).Contains(find);
        }

        public static void Replace(string file, string find, string? replace = null, Encoding? encoding = null)
        {
            file.NotNullOrWhiteSpace(nameof(file));

            replace ??= string.Empty;
            encoding ??= Encoding.UTF8;
            string text = File.ReadAllText(file, encoding).Replace(find, replace);
            File.WriteAllText(file, text, encoding);
        }

        public static void Rename(this FileInfo file, string newName)
        {
            file.NotNull(nameof(file));
            newName.NotNullOrWhiteSpace(nameof(newName));

            file.MoveTo(newName);
        }

        public static void Move(string source, string destination, bool overwrite = false)
        {
            source.NotNullOrWhiteSpace(nameof(source));
            destination.NotNullOrWhiteSpace(nameof(destination));

            if (overwrite && File.Exists(destination))
            {
                File.Delete(destination);
            }

            string destinationDirectory = Path.GetDirectoryName(destination) ?? throw new InvalidOperationException(destination);
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Move(source, destination);
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

        public static void AddPrefix(string file, string prefix)
        {
            File.Move(file, PathHelper.AddFilePrefix(file, prefix));
        }

        public static void AddPostfix(string file, string postfix)
        {
            File.Move(file, PathHelper.AddFilePostfix(file, postfix));
        }

        public static void MoveAll(string sourceDirectory, string destinationDirectory, string searchPattern = PathHelper.AllSearchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly, Func<string, bool>? predicate = null, bool overwrite = false)
        {
            Directory
                .EnumerateFiles(sourceDirectory, searchPattern, searchOption)
                .Where(file => predicate?.Invoke(file) ?? true)
                .ToArray()
                .ForEach(subtitle => Move(subtitle, subtitle.Replace(sourceDirectory, destinationDirectory, StringComparison.InvariantCulture), overwrite));
        }
    }
}