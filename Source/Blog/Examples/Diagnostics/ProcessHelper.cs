namespace Examples.Diagnostics;

using Examples.Common;

public static partial class ProcessHelper
{
    public static int StartAndWait(
        string fileName, string arguments, Action<string?>? outputReceived = null, Action<string?>? errorReceived = null, Action<ProcessStartInfo>? initialize = null, bool window = false, TimeSpan? timeout = null)
    {
        bool redirectOutput = outputReceived is not null;
        bool redirectError = errorReceived is not null;
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = fileName.NotNullOrWhiteSpace(),
            Arguments = arguments,
            CreateNoWindow = !window,
            UseShellExecute = window,
            RedirectStandardOutput = redirectOutput,
            RedirectStandardError = redirectError,
        };
        initialize?.Invoke(startInfo);

        using Process process = new() { StartInfo = startInfo };

        if (redirectOutput)
        {
            process.OutputDataReceived += (sender, args) => outputReceived!(args.Data);
        }

        if (redirectError)
        {
            process.ErrorDataReceived += (sender, args) => errorReceived!(args.Data);
        }

        process.Start();
        if (redirectOutput)
        {
            process.BeginOutputReadLine();
        }

        if (redirectError)
        {
            process.BeginErrorReadLine();
        }

        if (timeout.HasValue)
        {
            process.WaitForExit(timeout.Value);
        }
        else
        {
            process.WaitForExit();
        }

        return process.ExitCode;
    }

    public static (int ExitCode, List<string?> Output, List<string?> Error) Run(string fileName, string arguments, bool window = false, TimeSpan? timeout = null)
    {
        List<string?> allOutput = [];
        List<string?> allErrors = [];
        int exitCode = StartAndWait(fileName, arguments, output => allOutput.Add(output), error => allErrors.Add(error), null, window, timeout);
        return (exitCode, allOutput, allErrors);
    }

    public static async Task<int> StartAndWaitAsync(
        string fileName, string arguments, Action<string?>? outputReceived = null, Action<string?>? errorReceived = null, Action<ProcessStartInfo>? initialize = null, bool window = false, CancellationToken cancellationToken = default)
    {
        bool redirectOutput = outputReceived is not null;
        bool redirectError = errorReceived is not null;
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = fileName.NotNullOrWhiteSpace(),
            Arguments = arguments,
            CreateNoWindow = !window,
            UseShellExecute = window,
            RedirectStandardOutput = redirectOutput,
            RedirectStandardError = redirectError,
        };
        initialize?.Invoke(startInfo);

        using Process process = new() { StartInfo = startInfo };

        if (redirectOutput)
        {
            process.OutputDataReceived += (sender, args) => outputReceived!(args.Data);
        }

        if (redirectError)
        {
            process.ErrorDataReceived += (sender, args) => errorReceived!(args.Data);
        }

        process.Start();
        if (redirectOutput)
        {
            process.BeginOutputReadLine();
        }

        if (redirectError)
        {
            process.BeginErrorReadLine();
        }

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    public static async Task<(int ExitCode, List<string?> Output, List<string?> Error)> RunAsync(string fileName, string arguments, bool window = false, CancellationToken cancellationToken = default)
    {
        List<string?> allOutput = [];
        List<string?> allErrors = [];
        int exitCode = await StartAndWaitAsync(fileName, arguments, output => allOutput.Add(output), error => allErrors.Add(error), null, window, cancellationToken);
        return (exitCode, allOutput, allErrors);
    }

    public static bool TryKillAll(string name) =>
        Run("taskkill", $"/F /IM {name} /T").ExitCode == 0;
}