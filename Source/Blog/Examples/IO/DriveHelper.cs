namespace Examples.IO;

using Examples.Common;

public static class DriveHelper
{
    public static long GetAvailableFreeSpace(string driveName) =>
        DriveInfo.GetDrives().Single(drive => drive.Name.EqualsIgnoreCase(driveName)).AvailableFreeSpace;
}