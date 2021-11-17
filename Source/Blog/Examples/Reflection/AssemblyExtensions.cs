namespace Examples.Reflection;

using Examples.Common;

public static class AssemblyExtensions
{
    public static string GetResourceString(this Assembly assembly, string resourceName)
    {
        using StreamReader reader = new(assembly.NotNull().GetManifestResourceStream(resourceName.NotNullOrWhiteSpace())
            ?? throw new ArgumentOutOfRangeException(nameof(resourceName)));
        return reader.ReadToEnd();
    }

    public static void GetResourceFile(this Assembly assembly, string resourceName, string filePath)
    {
        filePath.NotNullOrWhiteSpace();

        using Stream stream = assembly.NotNull().GetManifestResourceStream(resourceName.NotNullOrWhiteSpace())
            ?? throw new ArgumentOutOfRangeException(nameof(resourceName));
        using FileStream fileStream = new(filePath, FileMode.CreateNew);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
    }
}