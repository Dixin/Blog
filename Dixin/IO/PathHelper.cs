namespace Dixin.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dixin.Common;

    using Microsoft.Win32;

    public static class PathHelper
    {
        public static string FromUrl(string url)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(url));

            return new Uri(url).AbsoluteUri;
        }

        public static string AbsolutePath(string url)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(url));

            return new Uri(url).AbsolutePath;
        }

        public static string ExecutingAssembly
            // Better than AbsolutePath(Assembly.GetExecutingAssembly().GetName().CodeBase);
            () => Assembly.GetExecutingAssembly().Location;

        public static string ExecutingDirectory() => Path.GetDirectoryName(ExecutingAssembly());

        public static bool TryGetOneDriveRoot(out string oneDrive)
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
    }
}