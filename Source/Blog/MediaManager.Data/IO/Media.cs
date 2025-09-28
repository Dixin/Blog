namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using System.Linq;

internal class Media
{
    private static readonly Regex[] KeywordsToRemove =
    [
        new(@"gc2048\.com[\-]*", RegexOptions.IgnoreCase),
        new(@"www\.98T\.la[@]*", RegexOptions.IgnoreCase),
        new(@"guochan2048\.com[\-]*", RegexOptions.IgnoreCase),
        new(@"【 5d86\.shop】", RegexOptions.IgnoreCase),
        new(@"【7d68\.xyz】", RegexOptions.IgnoreCase),
        new(@"2048\.cc[\-]*", RegexOptions.IgnoreCase),
        new(@"fun2048\.com[@]*", RegexOptions.IgnoreCase),
        new(@"2048社区[\-]*", RegexOptions.IgnoreCase),
        new(@"kcf9\.com[\-]*", RegexOptions.IgnoreCase),
        new(@"[\.]*com[\-]+", RegexOptions.IgnoreCase),
        new(@"\(R18\)", RegexOptions.IgnoreCase),
        new(@"^[\-_]+"),
        new(@"[\-]+$")];

    internal static void SimplifyDirectories(string directory, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        if (!Directory.Exists(directory))
        {
            return;
        }

        string newDirectory = FormatDirectory(directory);

        if (!newDirectory.EqualsIgnoreCase(directory))
        {
            Directory.Move(directory, newDirectory);
            directory = newDirectory;
        }

        string[] subDirectories = Directory.GetDirectories(directory);
        string[] newSubDirectories = subDirectories
            .Select(FormatDirectory)
            .ToArray();
        (string First, string Second)[] subDirectoriesToRename = subDirectories
            .Zip(newSubDirectories)
            .Where(subDirectory => !subDirectory.First.EqualsIgnoreCase(subDirectory.Second))
            .ToArray();
        if (subDirectoriesToRename.Any())
        {
            subDirectoriesToRename.ForEach(subDirectoryToRename => Directory.Move(subDirectoryToRename.First, subDirectoryToRename.Second));
            subDirectories = newSubDirectories;
        }

        string[] files = Directory.GetFiles(directory);
        string[] newFiles = files
            .Select(FormatFile)
            .ToArray();
        (string First, string Second)[] filesToRename = files
            .Zip(newFiles)
            .Where(file => !file.First.EqualsIgnoreCase(file.Second))
            .ToArray();
        if (filesToRename.Any())
        {
            filesToRename.ForEach(fileToRename =>
            {
                if (File.Exists(fileToRename.Second) && new FileInfo(fileToRename.First).Length == new FileInfo(fileToRename.Second).Length)
                {
                    FileHelper.Recycle(fileToRename.Second);
                    log($"Delete {fileToRename.Second}");
                }

                File.Move(fileToRename.First, fileToRename.Second);
            });

            files = newFiles;
        }

        if (subDirectories.IsEmpty() && files.IsEmpty())
        {
            DirectoryHelper.Recycle(directory);
            log($"Delete {directory}");
            return;
        }

        string directoryName = PathHelper.GetFileName(directory);
        while (files.IsEmpty() && subDirectories.Length == 1)
        {
            string subDirectory = subDirectories.Single();
            string subDirectoryName = PathHelper.GetFileName(subDirectory);
            subDirectories = Directory
                .GetDirectories(subDirectory)
                .Do(log)
                .Select(subSubDirectory => DirectoryHelper.MoveToDirectory(subSubDirectory, directory, skipDestinationDirectory: true))
                .Do(log)
                .Do(_ => log(string.Empty))
                .ToArray();
            files = Directory
                .GetFiles(subDirectory)
                .Do(log)
                .Select(file => FileHelper.MoveToDirectory(file, directory, skipDestinationDirectory: true))
                .Do(log)
                .Do(_ => log(string.Empty))
                .ToArray();
            Debug.Assert(Directory.EnumerateFileSystemEntries(subDirectory).IsEmpty());
            DirectoryHelper.Recycle(subDirectory);
            log($"Delete {subDirectory}");

            if (directoryName.ContainsIgnoreCase(subDirectoryName))
            {
            }
            else if (subDirectoryName.ContainsIgnoreCase(directoryName))
            {
                log(directory);
                directory = DirectoryHelper.ReplaceDirectoryName(directory, subDirectoryName);
                log(directory);
                log(string.Empty);
                subDirectories = Directory.GetDirectories(directory);
                files = Directory.GetFiles(directory);
            }
            else
            {
                log(directory);
                directory = DirectoryHelper.AddPostfix(directory, $"^{subDirectoryName}");
                log(directory);
                log(string.Empty);
                subDirectories = Directory.GetDirectories(directory);
                files = Directory.GetFiles(directory);
            }
        }

        if (subDirectories.IsEmpty() && files.Length == 1)
        {
            string file = files.Single();
            string fileName = PathHelper.GetFileName(file);
            string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(file);
            directory = DirectoryHelper.AddPostfix(directory, "^");
            file = Directory.EnumerateFiles(directory).Single();
            string parentDirectory = PathHelper.GetDirectoryName(directory);
            log(file);
            string newFile = Path.Combine(parentDirectory, fileName);
            if (File.Exists(newFile) && new FileInfo(file).Length == new FileInfo(newFile).Length)
            {
                FileHelper.Recycle(newFile);
                log($"Delete {newFile}");
            }

            file = FileHelper.MoveToDirectory(file, parentDirectory, skipDestinationDirectory: true);
            log(file);
            log(string.Empty);

            if (directoryName.ContainsIgnoreCase(fileName) || directoryName.ContainsIgnoreCase(fileNameWithoutExtension))
            {
                string fileExtension = PathHelper.GetExtension(file);
                while (fileExtension.IsNotNullOrWhiteSpace() && directoryName.EndsWithIgnoreCase(fileExtension))
                {
                    directoryName = directoryName[..^fileExtension.Length];
                }

                log(file);
                file = FileHelper.ReplaceFileNameWithoutExtension(file, directoryName);
                log(file);
                log(string.Empty);
            }
            else if (fileName.ContainsIgnoreCase(directoryName) || fileNameWithoutExtension.ContainsIgnoreCase(directoryName))
            {

            }
            else
            {
                string fileExtension = PathHelper.GetExtension(file);
                while (fileExtension.IsNotNullOrWhiteSpace() && directoryName.EndsWithIgnoreCase(fileExtension))
                {
                    directoryName = directoryName[..^fileExtension.Length];
                }

                log(file);
                file = FileHelper.AddPrefix(file, $"{directoryName}^");
                log(file);
                log(string.Empty);
            }

            Debug.Assert(Directory.EnumerateFileSystemEntries(directory).IsEmpty());
            DirectoryHelper.Recycle(directory);
            log($"Delete {directory}");
            log(string.Empty);
            return;
        }

        if (files.Length > 1)
        {
            bool isFileDeleted = false;
            files.ForEach(file =>
            {
                string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(file);
                Match match = Regex.Match(fileNameWithoutExtension, @"( \([0-9]+\)|_[0-9]+| [0-9]+)$");
                if (match.Success)
                {
                    string originalFileNameWithoutExtension = fileNameWithoutExtension[..match.Index];
                    string originalFile = PathHelper.ReplaceFileNameWithoutExtension(file, originalFileNameWithoutExtension);
                    if (File.Exists(originalFile) && new FileInfo(file).Length == new FileInfo(originalFile).Length)
                    {
                        FileHelper.Recycle(file);
                        isFileDeleted = true;
                        log($"Delete {file}");
                        log(string.Empty);
                    }
                }
            });

            //if(isFileDeleted)
            //{
            //    files = Directory.GetFiles(directory);
            //}
        }

        subDirectories.ForEach(subDirectory => SimplifyDirectories(subDirectory, log));

        static string FormatDirectory(string path)
        {
            string newPath = KeywordsToRemove
                .Aggregate(path, (accumulation, keywordToRemove) => keywordToRemove.Replace(accumulation, string.Empty));
            string name = PathHelper.GetFileName(path);
            name = name.Trim();
            char[] nameSeparators = name.Where((character, index) => index % 2 != 0).Distinct().ToArray();
            if (nameSeparators.Count() == 1 && (name.Length > 6 || nameSeparators.Single() == '變'))
            {
                name = new string(name.Where((character, index) => index % 2 == 0).ToArray());
            }

            return PathHelper.ReplaceDirectoryName(newPath, name);
        }

        static string FormatFile(string path)
        {
            string newPath = KeywordsToRemove
                .Aggregate(path, (accumulation, keywordToRemove) => keywordToRemove.Replace(accumulation, string.Empty));
            string name = PathHelper.GetFileNameWithoutExtension(path);
            name = name.Trim();
            char[] nameSeparators = name.Where((character, index) => index % 2 != 0).Distinct().ToArray();
            if (nameSeparators.Count() == 1 && (name.Length > 6 || nameSeparators.Single() == '變'))
            {
                name = new string(name.Where((character, index) => index % 2 == 0).ToArray());
            }

            return PathHelper.ReplaceFileNameWithoutExtension(newPath, name);
        }
    }
}