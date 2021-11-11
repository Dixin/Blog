#nullable enable
namespace Examples.Management;

using System.Management;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
public static class Wmi
{
    public static ManagementObject[] Query(ObjectQuery objectQuery, ManagementScope? managementScope = null)
    {
        if (objectQuery == null)
        {
            throw new ArgumentNullException(nameof(objectQuery));
        }

        using ManagementObjectSearcher searcher = new(
            managementScope ?? new(), // Default ManagementPath: \\.\root\cimv2.
            objectQuery); // Default QueryLangauge: WQL.
        using ManagementObjectCollection processes = searcher.Get();
        return processes.OfType<ManagementObject>().ToArray();
    }

    public static ManagementObject[] Query
        (string query, ManagementScope? managementScope = null) => Query(new ObjectQuery(query), managementScope);
}