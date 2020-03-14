namespace Examples.IO
{
    using System.IO;
    using System.Text;

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

        public static void Move(string source, string destination, bool overwrite)
        {
            source.NotNullOrWhiteSpace(nameof(source));
            destination.NotNullOrWhiteSpace(nameof(destination));

            if (overwrite && File.Exists(destination))
            {
                File.Delete(destination);
            }
            File.Move(source, destination);
        }
    }
}