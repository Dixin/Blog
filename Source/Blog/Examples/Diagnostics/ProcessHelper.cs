namespace Examples.Diagnostics;

using Examples.Common;

public static partial class ProcessHelper
{
    public static int StartAndWait(
        string fileName, string arguments, Action<string?>? outputReceived = null, Action<string?>? errorReceived = null, Action<ProcessStartInfo>? initialize = null, int milliseconds = -1)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = fileName.NotNullOrWhiteSpace(),
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        initialize?.Invoke(startInfo);

        using Process process = new()
        {
            StartInfo = startInfo
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
        process.WaitForExit(milliseconds);
        return process.ExitCode;
    }

    public static (int ExitCode, List<string?> Output, List<string?> Error) Run(string fileName, string arguments, int milliseconds = -1)
    {
        List<string?> allOutput = new();
        List<string?> allErrors = new();
        int exitCode = StartAndWait(fileName, arguments, output => allOutput.Add(output), error => allErrors.Add(error), null, milliseconds);
        return (exitCode, allOutput, allErrors);
    }

    public static async Task<int> StartAndWaitAsync(
        string fileName, string arguments, Action<string?>? outputReceived = null, Action<string?>? errorReceived = null, Action<ProcessStartInfo>? initialize = null, CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = fileName.NotNullOrWhiteSpace(),
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        initialize?.Invoke(startInfo);

        using Process process = new()
        {
            StartInfo = startInfo
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
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    public static async Task<(int ExitCode, List<string?> Output, List<string?> Error)> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
    {
        List<string?> allOutput = new();
        List<string?> allErrors = new();
        int exitCode = await StartAndWaitAsync(fileName, arguments, output => allOutput.Add(output), error => allErrors.Add(error), null, cancellationToken);
        return (exitCode, allOutput, allErrors);
    }
}