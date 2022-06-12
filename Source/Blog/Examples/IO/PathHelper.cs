#nullable enable
namespace Examples.IO;

using System.Runtime.Versioning;

using Examples.Common;
using Examples.Diagnostics;
using Microsoft.Win32;

public static class PathHelper
{
    public const string AllSearchPattern = "*";

    public static readonly char[] InvalidFileNameCharacters = Path.GetInvalidFileNameChars();

    [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
    [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
    public static string FromUrl(string url) =>
        new Uri(url.NotNullOrWhiteSpace()).AbsoluteUri;

    [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
    public static string AbsolutePath(string url) =>
        new Uri(url.NotNullOrWhiteSpace()).AbsolutePath;

    public static string ExecutingAssembly() =>
         // Better than AbsolutePath(Assembly.GetExecutingAssembly().GetName().CodeBase);
         Assembly.GetExecutingAssembly().Location;

    public static string ExecutingDirectory() =>
        Path.GetDirectoryName(ExecutingAssembly()) ?? string.Empty;

    [SupportedOSPlatform("windows")]
    public static bool TryGetOneDriveRoot([NotNullWhen(true)] out string? oneDrive)
    {
        oneDrive = Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\OneDrive", "UserFolder", null) as string;
        if (!string.IsNullOrWhiteSpace(oneDrive))
        {
            return true;
        }

        string settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Microsoft\OneDrive\Settings\Personal");
        if (!Directory.Exists(settingsDirectory))
        {
            return false;
        }

        string[] datFiles = Directory.GetFiles(settingsDirectory, "*.dat");
        if (!datFiles.Any())
        {
            return false;
        }

        string iniFile = Path.ChangeExtension(datFiles.First(), "ini");
        try
        {
            oneDrive = File.ReadLines(iniFile)
                .Last(line => !string.IsNullOrWhiteSpace(line))
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Trim('"');
            return !string.IsNullOrWhiteSpace(oneDrive);
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
            return false;
        }
    }

    public static string AddFilePrefix(string file, string prefix) =>
        ReplaceFileNameWithoutExtension(file, name => $"{prefix}{name}");

    public static string AddFilePostfix(string file, string postfix) =>
        ReplaceFileNameWithoutExtension(file, name => $"{name}{postfix}");

    public static string AddDirectoryPrefix(string directory, string prefix) =>
        ReplaceDirectoryName(directory, name => $"{prefix}{name}");

    public static string AddDirectoryPostfix(string directory, string postfix) => $"{directory}{postfix}";

    public static bool HasInvalidFileNameCharacter(this string value) =>
        InvalidFileNameCharacters.Any(invalid => value.Contains(invalid, StringComparison.InvariantCulture));

    public static string ReplaceFileName(string file, string newFileName)
    {
        string? parent = Path.GetDirectoryName(file);
        return parent.IsNullOrWhiteSpace() ? newFileName : Path.Combine(parent, newFileName);
    }

    public static string ReplaceFileNameWithoutExtension(string file, string newFileNameWithoutExtension) =>
        ReplaceFileName(file, $"{newFileNameWithoutExtension}{Path.GetExtension(file)}");

    public static string ReplaceFileNameWithoutExtension(string file, Func<string, string> replace) =>
        ReplaceFileNameWithoutExtension(file, replace(Path.GetFileNameWithoutExtension(file)));

    public static string ReplaceDirectoryName(string directory, string newName)
    {
        string? parent = Path.GetDirectoryName(directory);
        return parent.IsNullOrWhiteSpace() ? newName : Path.Combine(parent, newName);
    }

    public static string ReplaceDirectoryName(string directory, Func<string, string> replace) =>
        ReplaceDirectoryName(directory, replace(Path.GetFileName(directory)));

    public static string ReplaceExtension(string file, string newExtension)
    {
        string? directory = Path.GetDirectoryName(file);
        string newFile = $"{Path.GetFileNameWithoutExtension(file)}{newExtension}";
        return string.IsNullOrEmpty(directory) ? newFile : Path.Combine(directory, newFile);
    }

    public static bool HasExtension(this string file, string extension) =>
        string.Equals(Path.GetExtension(file), extension, StringComparison.InvariantCultureIgnoreCase);

    public static bool HasAnyExtension(this string file, IEnumerable<string> extensions) =>
        extensions.Any(file.HasExtension);

    public static bool HasAnyExtension(this string file, params string[] extensions) =>
        extensions.Any(file.HasExtension);

    public static string ToWsl(string path, bool forceAbsolute = false)
    {
        (int exitCode, List<string?> output, List<string?> error) = OperatingSystem.IsLinux()
            ? ProcessHelper.Run("wslpath", $@"{(forceAbsolute ? "-a " : string.Empty)}""{path.ReplaceOrdinal("'", @"\'")}""")
            : OperatingSystem.IsWindows()
                ? ProcessHelper.Run("wsl", $@"wslpath {(forceAbsolute ? "-a " : string.Empty)}""{path.ReplaceOrdinal("'", @"\'")}""")
                : throw new NotSupportedException(Environment.OSVersion.ToString());
        if (exitCode is 0)
        {
            string[] nonNullOutput = output.Where(line => line is not null).Select(line => line!).ToArray();
            if (nonNullOutput.Length is 1)
            {
                return nonNullOutput.Single();
            }
        }

        throw new InvalidOperationException(string.Join(Environment.NewLine, error.Concat(output)));
    }

    public static string FromWsl(string path, bool forwardSlash = false)
    {
        (int exitCode, List<string?> output, List<string?> error) = OperatingSystem.IsLinux()
            ? ProcessHelper.Run("wslpath", $@"-{(forwardSlash ? "w" : "m")} ""{path.ReplaceOrdinal("'", @"\'")}""")
            : OperatingSystem.IsWindows()
                ? ProcessHelper.Run("wsl", $@"wslpath -{(forwardSlash ? "w" : "m")} ""{path.ReplaceOrdinal("'", @"\'")}""")
                : throw new NotSupportedException(Environment.OSVersion.ToString());
        if (exitCode is 0)
        {
            string[] nonNullOutput = output.Where(line => line is not null).Select(line => line!).ToArray();
            if (nonNullOutput.Length is 1)
            {
                return nonNullOutput.Single();
            }
        }

        throw new InvalidOperationException(string.Join(Environment.NewLine, error.Concat(output)));
    }
}