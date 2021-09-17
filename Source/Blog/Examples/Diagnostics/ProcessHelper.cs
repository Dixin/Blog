namespace Examples.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Examples.Common;

    public static partial class ProcessHelper
    {
        public static int StartAndWait(
            string fileName, string arguments, Action<string?>? outputReceived = null, Action<string?>? errorReceived = null)
        {
            fileName.NotNullOrWhiteSpace(nameof(fileName));

            using Process process = new()
            {
                StartInfo = new()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
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

        public static (int ExitCode, List<string?> Output, List<string?> Error) StartAndWait(string fileName, string arguments)
        {
            List<string?> allOutput = new();
            List<string?> allErrors = new();
            int exitCode = ProcessHelper.StartAndWait(fileName, arguments, output => allOutput.Add(output), error => allErrors.Add(error));
            return (exitCode, allOutput, allErrors);
        }
    }
}