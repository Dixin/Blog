namespace MediaManager;

public record Settings(
    string TopEnglishKeyword,
    string TopForeignKeyword,
    string PreferredOldKeyword,
    string PreferredNewKeyword,

    string FfmpegDirectory,

    string LibraryDirectory,
    string MovieSubtitleBackupDirectory,

    string Drive115Url,
    string MovieTopX265Url,
    string MovieTopX265EnglishUrl,
    string MovieTopX265ForeignUrl,
    string MovieTopH264Url,
    string MovieTopH264EnglishUrl,
    string MovieTopH264ForeignUrl,
    string MovieTopH264720PUrl,
    string MoviePreferredUrl,
    string TVTopX265Url,

    string Drive115Metadata,
    string MovieLibraryMetadata,
    string MovieIgnoreMetadata,
    string MovieExternalMetadata,
    string MovieTopX265Metadata,
    string MovieTopX265XMetadata,
    string MovieTopH264Metadata,
    string MovieTopH264XMetadata,
    string MovieTopH264720PMetadata,
    string MoviePreferredSummary,
    string MoviePreferredMetadata,
    string MovieRareMetadata,
    string TVTopX265Metadata,
    
    string MovieMetadataDirectory,
    string MovieMetadataCacheDirectory,
    string TVMetadataDirectory,
    string TVMetadataCacheDirectory)
{
    public DirectorySettings Movie3D { get; init; }

    public DirectorySettings MovieHdr { get; init; }

    public DirectorySettings MovieControversial { get; init; }

    public DirectorySettings MovieControversialWithoutSubtitle { get; init; }

    public DirectorySettings MovieControversialTemp3 { get; init; }

    public DirectorySettings MovieMainstream { get; init; }

    public DirectorySettings MovieMainstreamWithoutSubtitle { get; init; }

    public DirectorySettings MovieMusical { get; init; }

    public DirectorySettings MovieTemp1 { get; init; }

    public DirectorySettings MovieTemp2 { get; init; }

    public DirectorySettings MovieTemp3 { get; init; }

    public DirectorySettings MovieTemp { get; init; }

    public DirectorySettings MovieExternalNew { get; init; }

    public DirectorySettings MovieExternalDelete { get; init; }

    public DirectorySettings TVControversial { get; init; }

    public DirectorySettings TVDocumentary { get; init; }

    public DirectorySettings TVMainstream { get; init; }

    public DirectorySettings TVTutorial { get; init; }

    public DirectorySettings TVMainstreamWithoutSubtitle { get; init; }

    public DirectorySettings TVTemp3 { get; init; }

    public DirectorySettings AudioControversial { get; init; }

    public DirectorySettings AudioMainstream { get; init; }

    public DirectorySettings AudioShow { get; init; }

    public DirectorySettings AudioSoundtrack { get; init; }

    public Dictionary<string, string[]> MovieRegions { get; init; } = new();
}

public record struct DirectorySettings(string Directory, int Level)
{
    public void Deconstruct(out string directory, out int level)
    {
        directory = this.Directory;
        level = this.Level;
    }

    public static implicit operator (string Directory, int Level)(DirectorySettings directorySettings) => (directorySettings.Directory, directorySettings.Level);
}