﻿namespace MediaManager;

using MediaManager.IO;

public record struct DirectorySettings(string Directory, int Level)
{
    public readonly void Deconstruct(out string directory, out int level)
    {
        directory = this.Directory;
        level = this.Level;
    }

    public static implicit operator (string Directory, int Level)(DirectorySettings directorySettings) => (directorySettings.Directory, directorySettings.Level);

    public static implicit operator string(DirectorySettings directorySettings) => directorySettings.Directory;

    public static implicit operator DirectorySettings(string directory) => new(directory, Video.DefaultDirectoryLevel);
}

public interface ISettings
{
    DirectorySettings Movie3D { get; init; }

    DirectorySettings MovieHdr { get; init; }

    DirectorySettings MovieControversial { get; init; }

    DirectorySettings MovieControversialWithoutSubtitle { get; init; }

    DirectorySettings MovieControversialTemp3 { get; init; }

    DirectorySettings MovieMainstream { get; init; }

    DirectorySettings MovieMainstreamWithoutSubtitle { get; init; }

    DirectorySettings MovieMusical { get; init; }

    DirectorySettings MovieTemp1 { get; init; }

    DirectorySettings MovieTemp2 { get; init; }

    DirectorySettings MovieTemp3 { get; init; }

    DirectorySettings MovieTemp31 { get; init; }

    DirectorySettings MovieTemp32 { get; init; }

    DirectorySettings MovieTemp3Encode { get; init; }

    DirectorySettings MovieExternalNew { get; init; }

    DirectorySettings MovieExternalDelete { get; init; }

    DirectorySettings TVControversial { get; init; }

    DirectorySettings TVDocumentary { get; init; }

    DirectorySettings TVMainstream { get; init; }

    DirectorySettings TVTutorial { get; init; }

    DirectorySettings TVMainstreamWithoutSubtitle { get; init; }

    DirectorySettings TVTemp3 { get; init; }

    DirectorySettings AudioControversial { get; init; }

    DirectorySettings AudioMainstream { get; init; }

    DirectorySettings AudioShow { get; init; }

    DirectorySettings AudioSoundtrack { get; init; }

    Dictionary<string, string[]> MovieRegions { get; init; }

    string[] ImdbKeywords { get; init; }

    string[] AllImdbKeywords { get; init; }

    string[] MovieTopDuplications { get; init; }

    string TopEnglishKeyword { get; init; }

    string TopForeignKeyword { get; init; }

    string PreferredOldKeyword { get; init; }

    string PreferredNewKeyword { get; init; }

    string LibraryDirectory { get; init; }

    string MovieSubtitleBackupDirectory { get; init; }

    string Drive115Url { get; init; }

    string MovieTopX265Url { get; init; }

    string MovieTopX265EnglishUrl { get; init; }

    string MovieTopX265ForeignUrl { get; init; }

    string MovieTopH264Url { get; init; }

    string MovieTopH264EnglishUrl { get; init; }

    string MovieTopH264ForeignUrl { get; init; }

    string MovieTopH264720PUrl { get; init; }

    string MoviePreferredUrl { get; init; }

    string TVTopX265Url { get; init; }

    string Drive115Metadata { get; init; }

    string MovieLibraryMetadata { get; init; }

    string MovieIgnoreMetadata { get; init; }

    string MovieExternalMetadata { get; init; }

    string MovieTopX265Metadata { get; init; }

    string MovieTopX265XMetadata { get; init; }

    string MovieTopH264Metadata { get; init; }

    string MovieTopH264XMetadata { get; init; }

    string MovieTopH264720PMetadata { get; init; }

    string MoviePreferredSummary { get; init; }

    string MoviePreferredMetadata { get; init; }

    string MovieRareMetadata { get; init; }

    string MovieImdbSpecialMetadata { get; init; }

    string MovieMergedMetadata { get; init; }

    string TVTopX265Metadata { get; init; }

    string MovieMetadataDirectory { get; init; }

    string MovieMetadataCacheDirectory { get; init; }

    string TVMetadataDirectory { get; init; }

    string TVMetadataCacheDirectory { get; init; }

    string TopMagnetUrls { get; init; }

    string TopDatabase { get; init; }
}