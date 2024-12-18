namespace Examples.Management;

using System.Management;
using System.Runtime.Versioning;
using Examples.Common;

[SupportedOSPlatform("windows")]
public static class Wmi
{
    public static ManagementObject[] Query(ObjectQuery objectQuery, ManagementScope? managementScope = null)
    {
        using ManagementObjectSearcher searcher = new(
            managementScope ?? new(), // Default ManagementPath: \\.\root\cimv2.
            objectQuery.ThrowIfNull()); // Default Query Language: WQL.
        using ManagementObjectCollection processes = searcher.Get();
        return processes.OfType<ManagementObject>().ToArray();
    }

    public static ManagementObject[] Query(string query, ManagementScope? managementScope = null) => 
        Query(new ObjectQuery(query), managementScope);
}