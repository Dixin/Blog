namespace Dixin.Reflection
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;

    public static class AssemblyExtensions
    {
        public static string GetResourceString(this Assembly assembly, string resourceName)
        {
            Contract.Requires<ArgumentNullException>(assembly != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void GetResourceFile(this Assembly assembly, string resourceName, string filePath)
        {
            Contract.Requires<ArgumentNullException>(assembly != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(resourceName));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filePath));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }
    }
}
