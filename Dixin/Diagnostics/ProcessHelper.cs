namespace Dixin.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Management;

    using Dixin.Linq;
    using Dixin.Management;

    public static partial class ProcessHelper
    {
        public static int StartAndWait(
            string fileName,
            string arguments,
            Action<string> outputReceived = null,
            Action<string> errorReceived = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName));

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                if (outputReceived != null)
                {
                    process.OutputDataReceived += (sender, args) => outputReceived(args.Data);
                }

                if (errorReceived != null)
                {
                    process.ErrorDataReceived += (sender, args) => errorReceived(args.Data);
                }

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }

    public static partial class ProcessHelper
    {
        public static IEnumerable<Win32Process> All
            (ManagementScope managementScope = null) => Wmi
                .Query($"SELECT * FROM {Win32Process.WmiClassName}", managementScope)
                .Select(process => new Win32Process(process));

        public static Win32Process ById
            (uint processId, ManagementScope managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ProcessId)} = {processId}",
                    managementScope)
                .Select(process => new Win32Process(process)).FirstOrDefault();

        public static IEnumerable<Win32Process> ByName
            (string name, ManagementScope managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.Name)} = '{name}'",
                    managementScope)
                .Select(process => new Win32Process(process));
    }

    public static partial class ProcessHelper
    {
        public static Win32Process ParentProcess(uint childProcessId, ManagementScope managementScope = null)
            => ById(childProcessId)?.ParentProcessId?.Forward(parentProcessId => ById(parentProcessId));

        public static IEnumerable<Win32Process> AllParentProcess(
            uint childProcessId,
            ManagementScope managementScope = null)
        {
            Win32Process parentProcess =
                ById(childProcessId)?.ParentProcessId?.Forward(parentProcessId => ById(parentProcessId));
            return parentProcess == null
                ? Enumerable.Empty<Win32Process>()
                : Enumerable.Repeat(parentProcess, 1).Concat(parentProcess.ProcessId.HasValue
                    ? AllParentProcess(parentProcess.ProcessId.Value)
                    : Enumerable.Empty<Win32Process>());
        }
    }

    public static partial class ProcessHelper
    {
        public static IEnumerable<Win32Process> ChildProcesses
            (uint parentProcessId, ManagementScope managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {Win32Process.WmiClassName} WHERE {nameof(Win32Process.ParentProcessId)} = {parentProcessId}",
                    managementScope)
                .Select(process => new Win32Process(process));

        public static IEnumerable<Win32Process> AllChildProcesses
            (uint parentProcessId, ManagementScope managementScope = null)
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