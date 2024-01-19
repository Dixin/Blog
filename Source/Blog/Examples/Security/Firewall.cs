namespace Examples.Security;

using Examples.Common;
using Examples.Diagnostics;
using Examples.Linq;

public static class Firewall
{
    private const string PrefixSeparator = "@";

    public static async Task BlockProgramInboundAsync(string ruleName, string path, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        int exitCode = await ProcessHelper.StartAndWaitAsync(
            "netsh",
            $"""advfirewall firewall add rule name="{ruleName}" dir=in action=block program="{path}" enable=yes profile=any""",
            output,
            error,
            null,
            false,
            cancellationToken);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(exitCode.ToString());
        }
    }

    public static async Task BlockProgramOutboundAsync(string ruleName, string path, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        int exitCode = await ProcessHelper.StartAndWaitAsync(
            "netsh",
            $"""advfirewall firewall add rule name="{ruleName}" dir=out action=block program="{path}" enable=yes profile=any""",
            output,
            error,
            null,
            false,
            cancellationToken);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(exitCode.ToString());
        }
    }

    public static async Task BlockProgramAsync(string ruleName, string path, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        await BlockProgramInboundAsync(ruleName, path, output, error, cancellationToken);
        await BlockProgramOutboundAsync(ruleName, path, output, error, cancellationToken);
    }

    public static async Task BlockProgramsAsync(string? prefix = null, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default, params string[] paths)
    {
        paths.ForEach(path =>
        {
            if (!File.Exists(path))
            {
                throw new ArgumentOutOfRangeException(nameof(paths), path, "Not found.");
            }
        });

        if (prefix.IsNullOrWhiteSpace())
        {
            prefix = string.Empty;
        }
        else
        {
            prefix = prefix.Trim();
            if (!prefix.EndsWithOrdinal(PrefixSeparator))
            {
                prefix = $"{prefix}{PrefixSeparator}";
            }
        }

        await paths.ForEachAsync(async path =>
            await BlockProgramAsync($"{prefix}{path}", path, output, error, cancellationToken), cancellationToken);
    }

    public static async Task BlockAllProgramsAsync(string? prefix = null, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default, params string[] directories) =>
        await BlockProgramsAsync(
            prefix,
            output,
            error,
            cancellationToken,
            directories
                .SelectMany(directory => Directory.EnumerateFiles(directory, "*.exe", SearchOption.AllDirectories))
                .ToArray());

    public static async Task<string[]> GetRuleNamesAsync(string? prefix = null)
    {
        (int exitCode, List<string?> output, List<string?> error) = await ProcessHelper.RunAsync(
            "netsh",
            "advfirewall firewall show rule name=all");
        if (exitCode != 0)
        {
            throw new InvalidOperationException(exitCode.ToString());
        }

        const string Prefix = "Rule Name:";
        string[] ruleNames = output
            .Where(line => line.IsNotNullOrWhiteSpace() && line.StartsWith(Prefix))
            .Select(line => line![Prefix.Length..].Trim())
            .OrderBy(line => line)
            .ToArray();
        if (prefix.IsNullOrWhiteSpace())
        {
            return ruleNames;
        }

        if (!prefix.EndsWithOrdinal(PrefixSeparator))
        {
            prefix = $"{prefix}{PrefixSeparator}";
        }

        return ruleNames.Where(line => line.StartsWithOrdinal(prefix)).ToArray();

    }

    public static async Task DeleteInboundRuleAsync(string ruleName, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        int exitCode = await ProcessHelper.StartAndWaitAsync(
            "netsh",
            $"""advfirewall firewall delete rule name="{ruleName}" dir=in profile=any""",
            output,
            error,
            null,
            false,
            cancellationToken);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(exitCode.ToString());
        }
    }

    public static async Task DeleteOutboundRuleAsync(string ruleName, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        int exitCode = await ProcessHelper.StartAndWaitAsync(
            "netsh",
            $"""advfirewall firewall delete rule name="{ruleName}" dir=out profile=any""",
            output,
            error,
            null,
            false,
            cancellationToken);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(exitCode.ToString());
        }
    }

    public static async Task DeleteRuleAsync(string ruleName, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        int exitCode = await ProcessHelper.StartAndWaitAsync(
            "netsh",
            $"""advfirewall firewall delete rule name="{ruleName}" profile=any""",
            output,
            error,
            null,
            false,
            cancellationToken);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(exitCode.ToString());
        }
    }

    public static async Task DeleteRulesAsync(string prefix, Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        string[] ruleNames = await GetRuleNamesAsync(prefix);
        await ruleNames
            .Distinct(StringComparer.Ordinal)
            .ForEachAsync(ruleName => DeleteRuleAsync(ruleName, output, error, cancellationToken), cancellationToken);
    }

    public static async Task BlockAdobeProgramsAsync(Action<string?>? output = null, Action<string?>? error = null, CancellationToken cancellationToken = default)
    {
        output ??= message => Trace.TraceInformation(message);
        error ??= message => Trace.TraceError(message);

        const string Prefix = "Adobe";
        await DeleteRulesAsync(Prefix, output, error, cancellationToken);

        await BlockAllProgramsAsync(Prefix, output, error, cancellationToken,

            // @"C:\Program Files\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Prefix),

            // @"C:\Program Files\Common Files\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), Prefix),

            // @"C:\Program Files (x86)\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Prefix),

            // @"C:\Program Files (x86)\Common Files\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), Prefix),

            // @"C:\ProgramData\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Prefix),

            // @"C:\Users\dixin\AppData\Local\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Prefix),

            // @"C:\Users\dixin\AppData\Roaming\Adobe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Prefix));
    }
}
