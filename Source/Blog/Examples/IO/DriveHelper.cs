namespace Examples.IO;

public static class DriveHelper
{
    public static bool TryGetAvailableFreeSpace(string path, [NotNullWhen(true)] out long? availableFreeSpace)
    {
        string root = PathHelper.GetPathRoot(path);
        if (string.IsNullOrEmpty(root) || root.StartsWith(@"\\", StringComparison.Ordinal))
        {
            availableFreeSpace = null;
            return false;
        }

        availableFreeSpace = new DriveInfo(root).AvailableFreeSpace;
        return true;
    }
}