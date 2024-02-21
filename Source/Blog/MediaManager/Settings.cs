namespace MediaManager;

using Examples.Common;
using MediaManager.IO;

public partial record Settings(
    string TopEnglishKeyword,
    string TopForeignKeyword,
    string PreferredOldKeyword,
    string PreferredNewKeyword,

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
    string MovieSharedUrl,
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
    string MoviePreferredFileMetadata,
    string MovieRareMetadata,
    string MovieSharedMetadata,
    string MovieImdbSpecialMetadata,
    string MovieMergedMetadata,
    string TVTopX265Metadata,

    string MovieMetadataDirectory,
    string MovieMetadataBackupDirectory,
    string MovieMetadataCacheDirectory,
    string MovieMetadataCacheBackupDirectory,
    string TVMetadataDirectory,
    string TVMetadataCacheDirectory,

    string TopMagnetUrls,
    string TopDatabase,
    
    string TempFile)
{
    public DirectorySettings Movie3D { get; init; }

    public DirectorySettings MovieHdr { get; init; }

    public DirectorySettings MovieControversial { get; init; }

    public DirectorySettings MovieControversialWithoutSubtitle { get; init; }

    public DirectorySettings MovieControversialTemp4 { get; init; }

    public DirectorySettings MovieMainstream { get; init; }

    public DirectorySettings MovieMainstreamWithoutSubtitle { get; init; }

    public DirectorySettings MovieMusical { get; init; }

    public DirectorySettings MovieTemp1 { get; init; }

    public DirectorySettings MovieTemp2 { get; init; }

    public DirectorySettings MovieTemp3 { get; init; }

    public DirectorySettings MovieTemp31 { get; init; }

    public DirectorySettings MovieTemp32 { get; init; }

    public DirectorySettings MovieTemp4 { get; init; }

    public DirectorySettings MovieTemp41 { get; init; }

    public DirectorySettings MovieTemp42 { get; init; }

    public DirectorySettings MovieTemp4Encode { get; init; }

    public DirectorySettings MovieExternalNew { get; init; }

    public DirectorySettings MovieExternalDelete { get; init; }

    public DirectorySettings TVControversial { get; init; }

    public DirectorySettings TVDocumentary { get; init; }

    public DirectorySettings TVMainstream { get; init; }

    public DirectorySettings TVTutorial { get; init; }

    public DirectorySettings TVMainstreamWithoutSubtitle { get; init; }

    public DirectorySettings TVTemp4 { get; init; }

    public DirectorySettings AudioControversial { get; init; }

    public DirectorySettings AudioMainstream { get; init; }

    public DirectorySettings AudioShow { get; init; }

    public DirectorySettings AudioSoundtrack { get; init; }

    public Dictionary<string, string[]> MovieRegions { get; init; } = new();

    public string[] ImdbKeywords { get; init; } = [];

    public string[] AllImdbKeywords { get; init; } = [];

    public string[] MovieTopDuplications { get; init; } = [];
}

public partial record Settings : ISettings
{
    internal static void FormatImdbKeywords(string keywords) =>
        keywords
            .Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(keyword => keyword.Trim().Trim(',').Trim('"').ToLowerInvariant().Replace("-", " "))
            .Select(keyword => keyword.StartsWithOrdinal(@"//""") ? (IsComment: true, Value: keyword[@"//""".Length..]) : (IsComment: false, Value: keyword))
            .OrderBy(keyword => keyword.Value)
            .DistinctBy(keyword => keyword.Value)
            .Select(keyword => keyword.IsComment ? $"""//"{keyword.Value}",""" : $"""
                "{keyword.Value}",
                """)
            .ForEach(Logger.WriteLine);
}