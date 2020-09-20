#nullable enable
namespace Examples.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Examples.Common;

    using Microsoft.Win32;

    public static class PathHelper
    {
        public const string AllSearchPattern = "*";

        public static readonly char[] InvalidFileNameCharacters = Path.GetInvalidFileNameChars();

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public static string FromUrl(string url)
        {
            url.NotNullOrWhiteSpace(nameof(url));

            return new Uri(url).AbsoluteUri;
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public static string AbsolutePath(string url)
        {
            url.NotNullOrWhiteSpace(nameof(url));

            return new Uri(url).AbsolutePath;
        }

        public static string ExecutingAssembly
            // Better than AbsolutePath(Assembly.GetExecutingAssembly().GetName().CodeBase);
            () => Assembly.GetExecutingAssembly().Location;

        public static string ExecutingDirectory() => Path.GetDirectoryName(ExecutingAssembly());

        public static bool TryGetOneDriveRoot(out string? oneDrive)
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

            try
            {
                string datFile = Directory.EnumerateFiles(settingsDirectory, "*.dat").FirstOrDefault();
                string iniFile = Path.ChangeExtension(datFile, "ini");
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

        public static string AddFilePrefix(string file, string prefix)
        {
            return Path.Combine(Path.GetDirectoryName(file), $"{prefix}{Path.GetFileName(file)}");
        }

        public static string AddFilePostfix(string file, string postfix)
        {
            return Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}{postfix}{Path.GetExtension(file)}");
        }

        public static string AddDirectoryPrefix(string directory, string prefix)
        {
            return Path.Combine(Path.GetDirectoryName(directory), $"{prefix}{Path.GetFileName(directory)}");
        }

        public static string AddDirectoryPostfix(string directory, string postfix)
        {
            return $"{directory}{postfix}";
        }

        public static bool HasInvalidFileNameCharacter(this string value)
        {
            return InvalidFileNameCharacters.Any(invalid => value.Contains(invalid, StringComparison.InvariantCulture));
        }

        public static string ReplaceFileName(string file, string newFileName)
        {
            string? directory = Path.GetDirectoryName(file);
            return string.IsNullOrEmpty(directory) ? newFileName : Path.Combine(directory, newFileName);
        }

        public static string ReplaceFileNameWithoutExtension(string file, string newFileNameWithoutExtension)
        {
            return ReplaceFileName(file, $"{newFileNameWithoutExtension}{Path.GetExtension(file)}");
        }

        public static string ReplaceExtension(string file, string newExtension)
        {
            string? directory = Path.GetDirectoryName(file);
            string newFile = $"{Path.GetFileNameWithoutExtension(file)}{newExtension}";
            return string.IsNullOrEmpty(directory) ? newFile : Path.Combine(directory, newFile);
        }

        public static bool HasExtension(this string file, string extension)
        {
            return string.Equals(Path.GetExtension(file), extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool HasAnyExtension(this string file, IEnumerable<string> extensions)
        {
            return extensions.Any(file.HasExtension);
        }

        public static bool HasAnyExtension(this string file, params string[] extensions)
        {
            return extensions.Any(file.HasExtension);
        }
    }
}