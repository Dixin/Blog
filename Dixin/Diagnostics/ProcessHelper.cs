namespace Dixin.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Management;

    using Dixin.Management;

    public static class ProcessHelper
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

        public static IEnumerable<Win32_Process> QueryAll
            (ManagementScope managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {nameof(Win32_Process)}", 
                    managementScope)
                .Select(process => new Win32_Process(process));

        public static IEnumerable<Win32_Process> QueryById
            (uint processId, ManagementScope managementScope = null) => Wmi
                .Query(
                    $"SELECT * FROM {nameof(Win32_Process)} WHERE {nameof(Win32_Process.ProcessId)} = {processId}",
                    managementScope)
                .Select(process => new Win32_Process(process));

        public static IEnumerable<Win32_Process> QueryByName
            (string name, ManagementScope managementScope = null) => Wmi.Query(
                    $"SELECT * FROM {nameof(Win32_Process)} WHERE {nameof(Win32_Process.Name)} = '{name}'",
                    managementScope)
                .Select(process => new Win32_Process(process));
    }
}