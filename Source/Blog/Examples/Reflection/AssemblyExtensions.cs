namespace Examples.Reflection
{
    using System;
    using System.IO;
    using System.Reflection;

    using Examples.Common;

    public static class AssemblyExtensions
    {
        public static string GetResourceString(this Assembly assembly, string resourceName)
        {
            assembly.NotNull(nameof(assembly));
            resourceName.NotNullOrWhiteSpace(nameof(resourceName));

            using StreamReader reader = new(assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentOutOfRangeException(nameof(resourceName)));
            return reader.ReadToEnd();
        }

        public static void GetResourceFile(this Assembly assembly, string resourceName, string filePath)
        {
            assembly.NotNull(nameof(assembly));
            resourceName.NotNullOrWhiteSpace(nameof(resourceName));
            filePath.NotNullOrWhiteSpace(nameof(filePath));

            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentOutOfRangeException(nameof(resourceName));
            using FileStream fileStream = new(filePath, FileMode.CreateNew);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }
    }
}
