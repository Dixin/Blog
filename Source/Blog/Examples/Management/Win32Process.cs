#nullable enable
namespace Examples.Management;

using System.Management;
using System.Runtime.Versioning;

// https://msdn.microsoft.com/en-us/library/windows/desktop/aa394372.aspx
[SupportedOSPlatform("windows")]
public partial class Win32Process
{
    public const string WmiClassName = "Win32_Process";
}

[DebuggerDisplay("Name = {this.Name}, Id = {this.ProcessId}")]
public partial class Win32Process
{
    public string? Caption { get; }

    public string? CommandLine { get; }

    public string? CreationClassName { get; }

    public DateTime? CreationDate { get; }

    public string? CSCreationClassName { get; }

    public string? CSName { get; }

    public string? Description { get; }

    public string? ExecutablePath { get; }

    public ushort? ExecutionState { get; }

    public string? Handle { get; }

    public uint? HandleCount { get; }

    public DateTime? InstallDate { get; }

    public ulong? KernelModeTime { get; }

    public uint? MaximumWorkingSetSize { get; }

    public uint? MinimumWorkingSetSize { get; }

    public string? Name { get; }

    public string? OSCreationClassName { get; }

    public string? OSName { get; }

    public ulong? OtherOperationCount { get; }

    public ulong? OtherTransferCount { get; }

    public uint? PageFaults { get; }

    public uint? PageFileUsage { get; }

    public uint? ParentProcessId { get; }

    public uint? PeakPageFileUsage { get; }

    public ulong? PeakVirtualSize { get; }

    public uint? PeakWorkingSetSize { get; }

    public uint? Priority { get; }

    public ulong? PrivatePageCount { get; }

    public uint? ProcessId { get; }

    public uint? QuotaNonPagedPoolUsage { get; }

    public uint? QuotaPagedPoolUsage { get; }

    public uint? QuotaPeakNonPagedPoolUsage { get; }

    public uint? QuotaPeakPagedPoolUsage { get; }

    public ulong? ReadOperationCount { get; }

    public ulong? ReadTransferCount { get; }

    public uint? SessionId { get; }

    public string? Status { get; }

    public DateTime? TerminationDate { get; }

    public uint? ThreadCount { get; }

    public ulong? UserModeTime { get; }

    public ulong? VirtualSize { get; }

    public string? WindowsVersion { get; }

    public ulong? WorkingSetSize { get; }

    public ulong? WriteOperationCount { get; }

    public ulong? WriteTransferCount { get; }
}

public partial class Win32Process
{
    public Win32Process(ManagementObject process)
    {
        if (process == null)
        {
            throw new ArgumentNullException(nameof(process));
        }

        this.Caption = process[nameof(this.Caption)] as string;
        this.CommandLine = process[nameof(this.CommandLine)] as string;
        this.CreationClassName = process[nameof(this.CreationClassName)] as string;
        this.CreationDate = process[nameof(this.CreationDate)] is string creationDate && !string.IsNullOrWhiteSpace(creationDate)
            ? ManagementDateTimeConverter.ToDateTime(creationDate) : default;
        this.CSCreationClassName = process[nameof(this.CSCreationClassName)] as string;
        this.CSName = process[nameof(this.CSName)] as string;
        this.Description = process[nameof(this.Description)] as string;
        this.ExecutablePath = process[nameof(this.ExecutablePath)] as string;
        this.ExecutionState = (ushort?)process[nameof(this.ExecutionState)];
        this.Handle = process[nameof(this.Handle)] as string;
        this.HandleCount = (uint?)process[nameof(this.HandleCount)];
        this.InstallDate = process[nameof(this.InstallDate)] is string initialDate && !string.IsNullOrWhiteSpace(initialDate)
            ? ManagementDateTimeConverter.ToDateTime(initialDate) : default;
        this.KernelModeTime = (ulong?)process[nameof(this.KernelModeTime)];
        this.MaximumWorkingSetSize = (uint?)process[nameof(this.MaximumWorkingSetSize)];
        this.MinimumWorkingSetSize = (uint?)process[nameof(this.MinimumWorkingSetSize)];
        this.Name = process[nameof(this.Name)] as string;
        this.OSCreationClassName = process[nameof(this.OSCreationClassName)] as string;
        this.OSName = process[nameof(this.OSName)] as string;
        this.OtherOperationCount = (ulong?)process[nameof(this.OtherOperationCount)];
        this.OtherTransferCount = (ulong?)process[nameof(this.OtherTransferCount)];
        this.PageFaults = (uint?)process[nameof(this.PageFaults)];
        this.PageFileUsage = (uint?)process[nameof(this.PageFileUsage)];
        this.ParentProcessId = (uint?)process[nameof(this.ParentProcessId)];
        this.PeakPageFileUsage = (uint?)process[nameof(this.PeakPageFileUsage)];
        this.PeakVirtualSize = (ulong?)process[nameof(this.PeakVirtualSize)];
        this.PeakWorkingSetSize = (uint?)process[nameof(this.PeakWorkingSetSize)];
        this.Priority = (uint?)process[nameof(this.Priority)];
        this.PrivatePageCount = (ulong?)process[nameof(this.PrivatePageCount)];
        this.ProcessId = (uint?)process[nameof(this.ProcessId)];
        this.QuotaNonPagedPoolUsage = (uint?)process[nameof(this.QuotaNonPagedPoolUsage)];
        this.QuotaPagedPoolUsage = (uint?)process[nameof(this.QuotaPagedPoolUsage)];
        this.QuotaPeakNonPagedPoolUsage = (uint?)process[nameof(this.QuotaPeakNonPagedPoolUsage)];
        this.QuotaPeakPagedPoolUsage = (uint?)process[nameof(this.QuotaPeakPagedPoolUsage)];
        this.ReadOperationCount = (ulong?)process[nameof(this.ReadOperationCount)];
        this.ReadTransferCount = (ulong?)process[nameof(this.ReadTransferCount)];
        this.SessionId = (uint?)process[nameof(this.SessionId)];
        this.Status = process[nameof(this.Status)] as string;
        this.TerminationDate = process[nameof(this.TerminationDate)] is string terminationDate && !string.IsNullOrWhiteSpace(terminationDate)
            ? ManagementDateTimeConverter.ToDateTime(terminationDate) : default;
        this.ThreadCount = (uint?)process[nameof(this.ThreadCount)];
        this.UserModeTime = (ulong?)process[nameof(this.UserModeTime)];
        this.VirtualSize = (ulong?)process[nameof(this.VirtualSize)];
        this.WindowsVersion = process[nameof(this.WindowsVersion)] as string;
        this.WorkingSetSize = (ulong?)process[nameof(this.WorkingSetSize)];
        this.WriteOperationCount = (ulong?)process[nameof(this.WriteOperationCount)];
        this.WriteTransferCount = (ulong?)process[nameof(this.WriteTransferCount)];
    }
}