namespace Dixin.Reflection
{
    using System.IO;
    using System.Reflection;

    using Dixin.Common;

    public static class AssemblyExtensions
    {
        public static string GetResourceString(this Assembly assembly, string resourceName)
        {
            assembly.NotNull(nameof(assembly));
            resourceName.NotNullOrWhiteSpace(nameof(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void GetResourceFile(this Assembly assembly, string resourceName, string filePath)
        {
            assembly.NotNull(nameof(assembly));
            resourceName.NotNullOrWhiteSpace(nameof(resourceName));
            filePath.NotNullOrWhiteSpace(nameof(filePath));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }
    }
}
