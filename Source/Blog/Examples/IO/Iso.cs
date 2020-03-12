namespace Examples.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    internal static class Iso
    {
        internal static void DirectoriesToIsos(string source, string destination)
        {
            Directory.EnumerateDirectories(source, "*", SearchOption.TopDirectoryOnly)
                .Where(directory => !File.Exists(Path.Combine(destination, Path.GetFileName(directory) + ".iso")))
                .Do(directory => Directory
                    .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                    .ForEach(file =>
                    {
                        string truncated = file;
                        if (Path.GetFileName(file).Length > 110)
                        {
                            truncated = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Substring(0, 110 - Path.GetExtension(file).Length) + Path.GetExtension(file));
                            File.Move(file, truncated);
                        }
                        if (Path.GetFileNameWithoutExtension(truncated).Contains(";"))
                        {
                            File.Move(truncated, Path.Combine(Path.GetDirectoryName(truncated), Path.GetFileNameWithoutExtension(truncated).Replace(";", "_") + Path.GetExtension(truncated)));
                        }
                    }))
                .Do(directory => Directory
                    .EnumerateDirectories(directory, "*", SearchOption.AllDirectories)
                    .Where(subderectory => Path.GetFileName(subderectory).Contains(";"))
                    .ForEach(subderectory => Directory.Move(subderectory, Path.Combine(Path.GetDirectoryName(subderectory), subderectory.Replace(";", "_")))))
                .Select(directory => $@" create -o ""{destination}\{Path.GetFileName(directory)}.iso"" -add ""{directory}"" ""/{Path.GetFileName(directory)}"" -label {Path.GetFileName(directory)} -disable-optimization")
                .Do(directory => Console.WriteLine(directory))
                .ForEach(argument =>
                {
                    using (Process piso = new Process())
                    {
                        piso.StartInfo.UseShellExecute = true;
                        piso.StartInfo.CreateNoWindow = false;
                        piso.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                        piso.StartInfo.FileName = @"""C:\Program Files\PowerISO\piso.exe""";
                        piso.StartInfo.Arguments = argument;
                        piso.Start();
                        piso.WaitForExit();
                    }
                });
        }
    }
}
