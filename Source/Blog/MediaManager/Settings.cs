﻿namespace MediaManager;

public record Settings(
    string FfmpegDirectory,

    string LibraryDirectory,
    string MovieSubtitleBackupDirectory,

    string Drive115Url,
    string MovieRarbgX265Url,
    string MovieRarbgH264Url,
    string MovieRarbgH264720PUrl,
    string MovieYtsUrl,
    string TVRarbgX265Url,

    string Drive115Metadata,
    string MovieLibraryMetadata,
    string MovieIgnoreMetadata,
    string MovieExternalMetadata,
    string MovieRarbgX265Metadata,
    string MovieRarbgH264Metadata,
    string MovieRarbgH264720PMetadata,
    string MovieYtsSummary,
    string MovieYtsMetadata,
    string TVRarbgX265Metadata)
{
    public DirectorySettings Movie3D { get; init; }

    public DirectorySettings MovieHdr { get; init; }

    public DirectorySettings MovieControversial { get; init; }

    public DirectorySettings MovieMainstream { get; init; }

    public DirectorySettings MovieMainstreamWithoutSubtitle { get; init; }

    public DirectorySettings MovieMusical { get; init; }

    public DirectorySettings MovieTemp { get; init; }

    public DirectorySettings MovieExternalNew { get; init; }

    public DirectorySettings MovieExternalDelete { get; init; }

    public DirectorySettings TVControversial { get; init; }

    public DirectorySettings TVDocumentary { get; init; }

    public DirectorySettings TVMainstream { get; init; }

    public DirectorySettings TVTutorial { get; init; }

    public DirectorySettings TVTemp { get; init; }

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