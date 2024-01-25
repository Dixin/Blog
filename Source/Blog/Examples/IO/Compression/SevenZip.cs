namespace Examples.IO.Compression;

using Examples.Common;
using Examples.Diagnostics;

public class SevenZip
{
    #region Constants

    // http://sevenzip.sourceforge.jp/chm/cmdline/switches/method.htm#Zip
    private const int DefaultCompressionLevel = 9;

    #endregion

    #region Static Fields

    private static readonly int ProcessorCount = Environment.ProcessorCount;

    #endregion

    #region Fields

    private readonly string sevenZ;

    #endregion

    #region Constructors and Destructors

    public SevenZip(string sevenZ) => this.sevenZ = sevenZ.ThrowIfNullOrWhiteSpace();

    #endregion

    #region Public Methods and Operators

    public static void DeleteDirectory(string directory, TextWriter? logger)
    {
        $"Start deleting directory {directory.ThrowIfNullOrWhiteSpace()}".LogWith(logger);
        DirectoryHelper.Delete(directory);
        $"End deleting directory {directory}".LogWith(logger);
    }

    public static void DeleteFile(string file, TextWriter? logger)
    {
        $"Start deleting file {file.ThrowIfNullOrWhiteSpace()}".LogWith(logger);
        FileHelper.Delete(file);
        $"End deleting file {file}".LogWith(logger);
    }

    public void AllToZips(
        string directory,
        string[] archiveExtensions,
        Func<string, string>? zipFile = null,
        bool deleteArchive = false,
        bool isRecursive = false,
        TextWriter? logger = null,
        int level = DefaultCompressionLevel)
    {
        Directory.EnumerateFiles(directory.ThrowIfNullOrWhiteSpace())
            .Where(file => archiveExtensions.Contains(PathHelper.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .ForEach(archive => this.ToZip(archive, zipFile?.Invoke(archive), deleteArchive, logger, level));

        if (isRecursive)
        {
            Directory.EnumerateDirectories(directory)
                .ForEach(subDirectory => this.AllToZips(subDirectory, archiveExtensions, zipFile, deleteArchive, true, logger, level));
        }
    }

    public void DoubleZip(
        string source,
        string password,
        Func<string, string>? intermediateFile = null,
        TextWriter? logger = null,
        int level = DefaultCompressionLevel)
    {
        source.ThrowIfNullOrWhiteSpace();
        intermediateFile ??= name => $"{source}..zip";

        string firstPassZip = intermediateFile(source);
        this.Zip(source, firstPassZip, logger, null, level);

        string secondPassZip = $"{source}.zip";
        this.Zip(firstPassZip, secondPassZip, logger, password, level);

        DeleteFile(firstPassZip, logger);
    }

    public void Extract(string archive, string? destination = null, bool deleteArchive = false, TextWriter? logger = null)
    {
        archive.ThrowIfNullOrWhiteSpace();

        destination = !string.IsNullOrWhiteSpace(destination)
            ? destination
            : Path.Combine(PathHelper.GetDirectoryName(archive), PathHelper.GetFileNameWithoutExtension(archive));
        $"Start extracting {archive} to {destination}".LogWith(logger);
        ProcessHelper.StartAndWait(
            this.sevenZ,
            $"""
             x "{archive}" -y -r -o"{destination}"
             """,
            message => message.LogWith(logger),
            message => message.LogWith(logger));
        $"End extracting {archive} to {destination}".LogWith(logger);

        if (deleteArchive)
        {
            DeleteFile(archive, logger);
        }
    }

    public void ExtractAll(
        string directory,
        string[] archiveExtensions,
        Func<string, string>? destinationDirectory = null,
        bool deleteArchive = false,
        bool isRecursive = false,
        TextWriter? logger = null)
    {
        archiveExtensions.ThrowIfNull();

        Directory
            .EnumerateFiles(directory.ThrowIfNullOrWhiteSpace())
            .Where(file => archiveExtensions.Contains(PathHelper.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .ForEach(archive =>
                this.Extract(archive, destinationDirectory?.Invoke(archive), deleteArchive, logger));

        if (isRecursive)
        {
            Directory
                .EnumerateDirectories(directory)
                .ForEach(subDirectory => 
                    this.ExtractAll(subDirectory, archiveExtensions, destinationDirectory, deleteArchive, true, logger));
        }
    }

    public void ToZip(
        string archive,
        string? zip = null,
        bool deleteArchive = false,
        TextWriter? logger = null,
        int level = DefaultCompressionLevel)
    {
        archive.ThrowIfNullOrWhiteSpace();

        // Create temp directory.
        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            // Extract archive entries to temp directory.
            this.Extract(archive, tempDirectory, false, logger);

            // Compress temp directory entries (tempDirectory\*) to zip.
            string zipFullName = string.IsNullOrWhiteSpace(zip) ? Path.ChangeExtension(archive, "zip") : zip;
            this.Zip(Path.Combine(tempDirectory, "*"), zipFullName, logger, null, level);

            if (deleteArchive)
            {
                // Delete archive.
                DeleteFile(archive, logger);
            }
        }
        finally
        {
            // Delete temp directory.
            DeleteDirectory(tempDirectory, logger);
        }
    }

    public void Zip(
        string source,
        string? zip = null,
        TextWriter? logger = null,
        string? password = null,
        int level = DefaultCompressionLevel)
    {
        source.ThrowIfNullOrWhiteSpace();
        level = FormatCompressionLevel(level);
        zip = !string.IsNullOrWhiteSpace(zip) ? zip : $"{source}.zip";
        string? passwordArgument = string.IsNullOrEmpty(password) ? null : $"-p{password}";

        $"Start creating {zip} from {source}".LogWith(logger);
        ProcessHelper.StartAndWait(
            this.sevenZ,
            $"""a "{zip}" "{source}"  -tzip -r -mx={level} -mmt={ProcessorCount} {passwordArgument}""",
            message => message.LogWith(logger),
            message => message.LogWith(logger));
        $"End creating {zip} from {source}".LogWith(logger);
    }

    #endregion

    #region Methods

    private static int FormatCompressionLevel(int level) =>
        level switch
        {
            // http://sevenzip.sourceforge.jp/chm/cmdline/switches/method.htm#Zip
            < 0 => 0,
            > 9 => 9,
            _ => level
        };

    #endregion
}