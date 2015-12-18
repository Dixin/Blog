namespace Dixin.Management
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Management;

    public static class Wmi
    {
        public static ManagementObject[] Query(ObjectQuery objectQuery, ManagementScope managementScope = null)
        {
            Contract.Requires<ArgumentNullException>(objectQuery != null);

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                managementScope ?? new ManagementScope(), // Default ManagementPath: \\.\root\cimv2.
                objectQuery)) // Default QueryLangauge: WQL.
            using (ManagementObjectCollection processes = searcher.Get())
            {
                return processes.OfType<ManagementObject>().ToArray();
            }
        }

        public static ManagementObject[] Query
            (string query, ManagementScope managementScope = null) => Query(new ObjectQuery(query), managementScope);
    }
}
