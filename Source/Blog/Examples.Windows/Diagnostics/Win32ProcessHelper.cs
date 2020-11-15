#nullable enable
namespace Examples.Diagnostics
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;

    using Examples.Management;

    public static partial class Win32ProcessHelper
    {
        public static IEnumerable<Win32Process> All
            (ManagementScope? managementScope = null) => Wmi
                .Query($"SELECT * FROM {Win32Process.WmiClassName}", managementScope)
                .Select(process => new Win32Process(process));

        public static Win32Process? ById
            (uint processId, ManagementScope? managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ProcessId)} = {processId}",
                    managementScope)
                .Select(process => new Win32Process(process)).FirstOrDefault();

        public static IEnumerable<Win32Process> ByName
            (string name, ManagementScope? managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.Name)} = '{name}'",
                    managementScope)
                .Select(process => new Win32Process(process));
    }

    public static partial class Win32ProcessHelper
    {
        public static Win32Process? ParentProcess(uint childProcessId, ManagementScope? managementScope = null)
        {
            uint? parentProcessId = ById(childProcessId, managementScope)?.ParentProcessId;
            return parentProcessId != null ? ById(parentProcessId.Value, managementScope) : null;
        }

        public static IEnumerable<Win32Process> AllParentProcess(
            uint childProcessId,
            ManagementScope? managementScope = null)
        {
            uint? parentProcessId = ById(childProcessId, managementScope)?.ParentProcessId;
            Win32Process? parentProcess = parentProcessId != null ? ById(parentProcessId.Value, managementScope) : null;
            return parentProcess == null
                ? Enumerable.Empty<Win32Process>()
                : Enumerable.Repeat(parentProcess, 1).Concat(parentProcess.ProcessId.HasValue
                    ? AllParentProcess(parentProcess.ProcessId.Value)
                    : Enumerable.Empty<Win32Process>());
        }
    }

    public static partial class Win32ProcessHelper
    {
        public static IEnumerable<Win32Process> ChildProcesses
            (uint parentProcessId, ManagementScope? managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ParentProcessId)} = {parentProcessId}",
                    managementScope)
                .Select(process => new Win32Process(process));

        public static IEnumerable<Win32Process> AllChildProcesses
            (uint parentProcessId, ManagementScope? managementScope = null)
        {
            IEnumerable<Win32Process> childProcesses = Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ParentProcessId)} = {parentProcessId}",
                    managementScope).Select(process => new Win32Process(process));
            return childProcesses.Concat(childProcesses.SelectMany(process => process.ProcessId.HasValue
                ? AllChildProcesses(process.ProcessId.Value, managementScope)
                : Enumerable.Empty<Win32Process>()));
        }
    }
}
