namespace MediaManager;

using MediaManager.IO;
using MediaManager.Net;

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

    DirectorySettings MovieControversialTemp4 { get; init; }

    DirectorySettings MovieMainstream { get; init; }

    DirectorySettings MovieMainstreamWithoutSubtitle { get; init; }

    DirectorySettings MovieMusical { get; init; }

    DirectorySettings MovieTemp1 { get; init; }

    DirectorySettings MovieTemp2 { get; init; }

    DirectorySettings MovieTemp3 { get; init; }

    DirectorySettings MovieTemp31 { get; init; }

    DirectorySettings MovieTemp32 { get; init; }

    DirectorySettings MovieTemp4 { get; init; }

    DirectorySettings MovieTemp41 { get; init; }

    DirectorySettings MovieTemp42 { get; init; }

    DirectorySettings MovieTemp4Encode { get; init; }

    DirectorySettings MovieExternalNew { get; init; }

    DirectorySettings MovieExternalDelete { get; init; }

    DirectorySettings TVControversial { get; init; }

    DirectorySettings TVDocumentary { get; init; }

    DirectorySettings TVMainstream { get; init; }

    DirectorySettings TVTutorial { get; init; }

    DirectorySettings TVMainstreamWithoutSubtitle { get; init; }

    DirectorySettings TVTemp4 { get; init; }

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

    string MovieSharedUrl { get; init; }

    string TVTopX265Url { get; init; }

    string Drive115Metadata { get; init; }

    string MovieLibraryMetadata { get; init; }

    string MovieIgnoredMetadata { get; init; }

    string MovieExternalMetadata { get; init; }

    string MovieTopX265Metadata { get; init; }

    string MovieTopX265XMetadata { get; init; }

    string MovieTopH264Metadata { get; init; }

    string MovieTopH264XMetadata { get; init; }

    string MovieTopH264720PMetadata { get; init; }

    string MoviePreferredSummary { get; init; }

    string MoviePreferredMetadata { get; init; }

    string MoviePreferredFileMetadata { get; init; }

    string MovieRareMetadata { get; init; }

    string MovieSharedMetadata { get; init; }

    string MovieImdbSpecialMetadata { get; init; }

    string MovieMergedMetadata { get; init; }

    string TVTopX265Metadata { get; init; }

    string MovieMetadataDirectory { get; init; }

    string MovieMetadataBackupDirectory { get; init; }

    string MovieMetadataCacheDirectory { get; init; }

    string MovieMetadataCacheBackupDirectory { get; init; }

    string TVMetadataDirectory { get; init; }

    string TVMetadataCacheDirectory { get; init; }

    string TopMagnetUrls { get; init; }

    string TopDatabase { get; init; }

    string TempFile { get; init; }

    Task<Dictionary<string, VideoMetadata>> LoadMovieExternalMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieExternalMetadataAsync(Dictionary<string, VideoMetadata> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>> LoadMovieLibraryMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieLibraryMetadataAsync(ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> value, CancellationToken cancellationToken);

    Task<HashSet<string>> LoadIgnoredAsync(CancellationToken cancellationToken);

    Task WriteMovieIgnoredMetadataAsync(HashSet<string> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMovieTopX265MetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieTopX265MetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMovieTopH264MetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieTopH264MetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMovieTopX265XMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieTopX265XMetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMovieTopH264XMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieTopH264XMetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMovieTopH264720PMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieTopH264720PMetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, PreferredSummary>> LoadMoviePreferredSummaryAsync(CancellationToken cancellationToken);

    Task WriteMoviePreferredSummaryAsync(ConcurrentDictionary<string, PreferredSummary> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, List<PreferredMetadata>>> LoadMoviePreferredMetadataAsync(CancellationToken cancellationToken);

    Task WriteMoviePreferredMetadataAsync(ConcurrentDictionary<string, List<PreferredMetadata>> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, List<PreferredFileMetadata>>> LoadMoviePreferredFileMetadataAsync(CancellationToken cancellationToken);

    Task WriteMoviePreferredFileMetadataAsync(ConcurrentDictionary<string, List<PreferredFileMetadata>> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, RareMetadata>> LoadMovieRareMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieRareMetadataAsync(ConcurrentDictionary<string, RareMetadata> value, CancellationToken cancellationToken);

    Task<SharedMetadata[]> LoadMovieSharedMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieSharedMetadataAsync(SharedMetadata[] value, CancellationToken cancellationToken);

    Task<string[]> LoadMovieImdbSpecialMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieImdbSpecialMetadataAsync(string[] value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, ImdbMetadata>> LoadMovieMergedMetadataAsync(CancellationToken cancellationToken);

    Task WriteMovieMergedMetadataAsync(ConcurrentDictionary<string, ImdbMetadata> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadTVTopX265MetadataAsync(CancellationToken cancellationToken);

    Task WriteTVTopX265MetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);
}
