namespace Dixin.Diagnostics
{
    using System;
    using System.Diagnostics;

    using Dixin.Common;

    public static partial class ProcessHelper
    {
        public static int StartAndWait(
            string fileName,
            string arguments,
            Action<string> outputReceived = null,
            Action<string> errorReceived = null)
        {
            fileName.NotNullOrWhiteSpace(nameof(fileName));

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
}