namespace Dixin.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;

    public static class FileHelper
    {
        public static void Delete(string file)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(file));

            File.SetAttributes(file, FileAttributes.Normal); // In case file is read only.
            File.Delete(file);
        }

        public static bool Contains(string file, string find, Encoding encoding = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(file));

            encoding = encoding ?? Encoding.UTF8;
            return File.ReadAllText(file, encoding).Contains(find);
        }

        public static void Replace(string file, string find, string replace = null, Encoding encoding = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(file));

            replace = replace ?? string.Empty;
            encoding = encoding ?? Encoding.UTF8;
            string text = File.ReadAllText(file, encoding).Replace(find, replace);
            File.WriteAllText(file, text, encoding);
        }

        public static void Rename(this FileInfo file, string newName)
        {
            Contract.Requires<ArgumentNullException>(file != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(newName));

            file.MoveTo(newName);
        }
    }
}