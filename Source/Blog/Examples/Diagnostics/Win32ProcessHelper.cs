﻿namespace Examples.Diagnostics;

using System.Management;
using System.Runtime.Versioning;
using Examples.Common;
using Examples.Management;

[SupportedOSPlatform("windows")]
public static partial class Win32ProcessHelper
{
    public static IEnumerable<Win32Process> All(ManagementScope? managementScope = null) =>
        Wmi
            .Query($"SELECT * FROM {Win32Process.WmiClassName}", managementScope)
            .Select(process => new Win32Process(process));

    public static Win32Process? ById(uint processId, ManagementScope? managementScope = null) =>
        Wmi
            .Query(
                $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ProcessId)} = {processId}",
                managementScope)
            .Select(process => new Win32Process(process)).FirstOrDefault();

    public static IEnumerable<Win32Process> ByName(string name, ManagementScope? managementScope = null) =>
        Wmi
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
        return parentProcessId is null ? null : ById(parentProcessId.Value, managementScope);
    }

    public static IEnumerable<Win32Process> AllParentProcess(uint childProcessId, ManagementScope? managementScope = null)
    {
        uint? parentProcessId = ById(childProcessId, managementScope)?.ParentProcessId;
        Win32Process? parentProcess = parentProcessId is null ? null : ById(parentProcessId.Value, managementScope);
        return parentProcess is null
            ? []
            : Enumerable.Repeat(parentProcess, 1).Concat(parentProcess.ProcessId.HasValue
                ? AllParentProcess(parentProcess.ProcessId.Value)
                : []);
    }
}

public static partial class Win32ProcessHelper
{
    public static IEnumerable<Win32Process> ChildProcesses(uint parentProcessId, ManagementScope? managementScope = null) =>
        Wmi
            .Query(
                $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ParentProcessId)} = {parentProcessId}",
                managementScope)
            .Select(process => new Win32Process(process));

    public static IEnumerable<Win32Process> AllChildProcesses(
        uint parentProcessId, ManagementScope? managementScope = null)
    {
        Win32Process[] childProcesses = Wmi
            .Query($"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ParentProcessId)} = {parentProcessId}", managementScope)
            .Select(process => new Win32Process(process))
            .ToArray();
        return childProcesses
            .Concat(childProcesses
                .SelectMany(process => process.ProcessId.HasValue ? AllChildProcesses(process.ProcessId.Value, managementScope) : []));
    }

    public static bool TryKillAll(string name, params string[] arguments)
    {
        string query = $"SELECT {nameof(Win32Process.ProcessId)} FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.Name)} = '{name}'{string.Join(string.Empty, arguments.Select(argument => $" AND {nameof(Win32Process.CommandLine)} LIKE '%{argument}%'"))}";
        uint?[] processesIds = Wmi
            .Query(query)
            .Select(process => (uint?)process[nameof(Win32Process.ProcessId)])
            .ToArray();
        processesIds.ForEach(processId =>
        {
            if (!processId.HasValue)
            {
                return;
            }

            try
            {
                Process.GetProcessById((int)processId.Value).Kill(true);
            }
            catch (Exception exception) when (exception.IsNotCritical())
            {
            }
        });
        return Wmi
            .Query(query)
            .IsEmpty();
    }
}