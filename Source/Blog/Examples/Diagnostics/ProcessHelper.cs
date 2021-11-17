namespace Examples.Diagnostics;

using Examples.Common;

public static partial class ProcessHelper
{
    public static int StartAndWait(
        string fileName, string arguments, Action<string?>? outputReceived = null, Action<string?>? errorReceived = null)
    {
        using Process process = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = fileName.NotNullOrWhiteSpace(),
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        if (outputReceived is not null)
        {
            process.OutputDataReceived += (sender, args) => outputReceived(args.Data);
        }

        if (errorReceived is not null)
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
        int exitCode = StartAndWait(fileName, arguments, output => allOutput.Add(output), error => allErrors.Add(error));
        return (exitCode, allOutput, allErrors);
    }
}