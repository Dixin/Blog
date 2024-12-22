namespace Examples.Reflection;

using Examples.Common;

public static class AssemblyExtensions
{
    public static string GetResourceString(this Assembly assembly, string resourceName)
    {
        using StreamReader reader = new(assembly.ThrowIfNull().GetManifestResourceStream(resourceName.ThrowIfNullOrWhiteSpace())
            ?? throw new ArgumentOutOfRangeException(nameof(resourceName), resourceName, string.Empty));
        return reader.ReadToEnd();
    }

    public static void GetResourceFile(this Assembly assembly, string resourceName, string filePath)
    {
        filePath.ThrowIfNullOrWhiteSpace();

        using Stream stream = assembly.ThrowIfNull().GetManifestResourceStream(resourceName.ThrowIfNullOrWhiteSpace())
            ?? throw new ArgumentOutOfRangeException(nameof(resourceName), resourceName, string.Empty);
        using FileStream fileStream = new(filePath, FileMode.CreateNew);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
    }
}