namespace Examples.IO;

public static class DriveHelper
{
    public static long GetAvailableFreeSpace(string path) =>
        new DriveInfo(PathHelper.GetPathRoot(path)).AvailableFreeSpace;
}