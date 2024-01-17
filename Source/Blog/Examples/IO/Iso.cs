namespace Examples.IO;

using Examples.Common;

internal static class Iso
{
    internal static void DirectoriesToIsos(string source, string destination)
    {
        Directory.EnumerateDirectories(source, "*", SearchOption.TopDirectoryOnly)
            .OrderBy(directory => directory)
            .Where(directory => !File.Exists(Path.Combine(destination, PathHelper.GetFileName(directory) + ".iso")))
            .Do(directory => Directory
                .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .ForEach(file =>
                {
                    string truncated = file;
                    if (PathHelper.GetFileName(file).Length > 110)
                    {
                        truncated = Path.Combine(PathHelper.GetDirectoryName(file), PathHelper.GetFileNameWithoutExtension(file).Substring(0, 110 - PathHelper.GetExtension(file).Length) + PathHelper.GetExtension(file));
                        File.Move(file, truncated);
                    }
                    if (PathHelper.GetFileNameWithoutExtension(truncated).ContainsOrdinal(";"))
                    {
                        File.Move(truncated, Path.Combine(PathHelper.GetDirectoryName(truncated), PathHelper.GetFileNameWithoutExtension(truncated).Replace(";", "_") + PathHelper.GetExtension(truncated)));
                    }
                }))
            .Do(directory => Directory
                .EnumerateDirectories(directory, "*", SearchOption.AllDirectories)
                .Where(subDirectory => PathHelper.GetFileName(subDirectory).ContainsOrdinal(";"))
                .ForEach(subDirectory => DirectoryHelper.Move(subDirectory, Path.Combine(PathHelper.GetDirectoryName(subDirectory), subDirectory.Replace(";", "_")))))
            .Select(directory => $""" create -o "{destination}\{PathHelper.GetFileName(directory)}.iso" -add "{directory}" "/{PathHelper.GetFileName(directory)}" -label {PathHelper.GetFileName(directory)} -disable-optimization""")
            .Do(Console.WriteLine)
            .ForEach(argument =>
            {
                using Process powerIso = new()
                {
                    StartInfo = new()
                    {
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        FileName = """
                            "C:\Program Files\PowerISO\piso.exe"
                            """,
                        Arguments = argument
                    }
                };
                powerIso.Start();
                powerIso.WaitForExit();
            });
    }
}