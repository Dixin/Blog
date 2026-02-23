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

    public static implicit operator DirectorySettings((string Directory, int Level) directorySettings) => new(directorySettings.Directory, directorySettings.Level);

    public static implicit operator string(DirectorySettings directorySettings) => directorySettings.Directory;

    public static implicit operator DirectorySettings(string directory) => new(directory, Video.DefaultDirectoryLevel);
}

public interface ISettings
{
    DirectorySettings AudioControversial { get; init; }

    DirectorySettings AudioMainstream { get; init; }

    DirectorySettings AudioShow { get; init; }

    DirectorySettings AudioSoundtrack { get; init; }

    DirectorySettings Movie3D { get; init; }

    DirectorySettings Movie4KHdr { get; init; }

    DirectorySettings Movie4KHdrOverflow { get; init; }

    DirectorySettings MovieControversial { get; init; }

    DirectorySettings MovieControversialWithoutSubtitle { get; init; }

    DirectorySettings MovieDisk { get; init; }

    DirectorySettings MovieFranchise { get; init; }

    DirectorySettings MovieMainstream1 { get; init; }

    DirectorySettings MovieMainstream2 { get; init; }

    DirectorySettings MovieMainstream3 { get; init; }

    DirectorySettings MovieMusical { get; init; }

    DirectorySettings MovieTemp1 { get; init; }

    DirectorySettings MovieTemp2 { get; init; }

    DirectorySettings MovieTemp3 { get; init; }

    DirectorySettings MovieTemp4KHdr1 { get; init; }

    DirectorySettings MovieTemp4KHdr2 { get; init; }

    DirectorySettings TV4KHdr { get; init; }

    DirectorySettings TVControversial { get; init; }

    DirectorySettings TVDocumentary { get; init; }

    DirectorySettings TVMainstream { get; init; }

    DirectorySettings TVMainstreamChinese { get; init; }

    DirectorySettings TVMainstreamOverflow { get; init; }

    DirectorySettings TVTutorial { get; init; }

    DirectorySettings TVTemp1 { get; init; }

    DirectorySettings TVTemp2 { get; init; }

    string DirectoryMetadataAllMovies { get; init; }

    string DirectoryMetadataAllMoviesBackup { get; init; }

    string DirectoryMetadataAllMoviesCache { get; init; }

    string DirectoryMetadataAllMoviesFile { get; init; }

    string DirectoryMetadataAllMoviesCacheBackup { get; init; }

    string DirectoryMetadataAllTV { get; init; }

    string DirectoryMetadataAllTVCache { get; init; }

    Dictionary<string, string[]> MovieRegions { get; init; }

    Dictionary<string, string> MovieRegionDirectories { get; init; }

    string[] ImdbKeywords { get; init; }

    string[] AllImdbKeywords { get; init; }

    string[] MetadataTopMoviesDuplication { get; init; }

    string KeywordTopEnglish { get; init; }

    string KeywordTopForeign { get; init; }

    string KeywordPreferredOld { get; init; }

    string KeywordPreferredNew { get; init; }

    string KeywordContrast { get; init; }

    string DirectoryLibrary { get; init; }

    string DirectoryMetadataAllMoviesSubtitles { get; init; }

    string DirectoryMetadataAllTVSubtitles { get; init; }

    string UrlDrive115 { get; init; }

    string UrlTopMoviesX265 { get; init; }

    string UrlTopMoviesX265English { get; init; }

    string UrlTopMoviesX265Foreign { get; init; }

    string UrlTopMoviesH264 { get; init; }

    string UrlTopMoviesH264English { get; init; }

    string UrlTopMoviesH264Foreign { get; init; }

    string UrlTopMoviesH264720P { get; init; }

    string UrlPreferredMovies { get; init; }

    string UrlSharedMovies { get; init; }

    string UrlTopTVX265 { get; init; }

    string UrlContrastTV { get; init; }

    string MetadataDrive115Media { get; init; }

    string MetadataLibraryMovies { get; init; }

    string MetadataAllMoviesIgnored { get; init; }

    string MetadataLibraryMoviesExternal { get; init; }

    string MetadataTopMoviesX265 { get; init; }

    string MetadataTopMoviesX265X { get; init; }

    string MetadataTopMoviesH264 { get; init; }

    string MetadataTopMoviesH264X { get; init; }

    string MetadataTopMoviesH264720P { get; init; }

    string MetadataPreferredMoviesSummaries { get; init; }

    string MetadataPreferredMovies { get; init; }

    string MetadataPreferredMoviesFiles { get; init; }

    string MetadataRareMovies { get; init; }

    string MetadataSharedMovies { get; init; }

    string MetadataAllMoviesSpecial { get; init; }

    string MetadataAllMovies { get; init; }

    string MetadataTopTVX265 { get; init; }

    string MetadataContrastTV { get; init; }

    string MetadataTopMediaMagnetUrls { get; init; }

    string MetadataTopMedia { get; init; }

    string FileTemp { get; init; }

    Task<Dictionary<string, VideoMetadata>> LoadMetadataLibraryMoviesExternalAsync(CancellationToken cancellationToken);

    Task WriteMetadataLibraryMoviesExternalAsync(Dictionary<string, VideoMetadata> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>> LoadMetadataLibraryMoviesAsync(CancellationToken cancellationToken);

    Task WriteMetadataLibraryMoviesAsync(ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> value, CancellationToken cancellationToken);

    Task<HashSet<string>> LoadMetadataAllMoviesIgnoredAsync(CancellationToken cancellationToken);

    Task WriteMetadataAllMoviesIgnoredAsync(HashSet<string> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesX265Async(CancellationToken cancellationToken);

    Task WriteMetadataTopMoviesX265Async(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesH264Async(CancellationToken cancellationToken);

    Task WriteMetadataTopMoviesH264Async(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesX265XAsync(CancellationToken cancellationToken);

    Task WriteMetadataTopMoviesX265XAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesH264XAsync(CancellationToken cancellationToken);

    Task WriteMetadataTopMoviesH264XAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesH264720PAsync(CancellationToken cancellationToken);

    Task WriteMetadataTopMoviesH264720PAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, PreferredSummary>> LoadMetadataPreferredMoviesSummariesAsync(CancellationToken cancellationToken);

    Task WriteMetadataPreferredMoviesSummariesAsync(ConcurrentDictionary<string, PreferredSummary> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, List<PreferredMetadata>>> LoadMetadataPreferredMoviesAsync(CancellationToken cancellationToken);

    Task WriteMetadataPreferredMoviesAsync(ConcurrentDictionary<string, List<PreferredMetadata>> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, List<PreferredFileMetadata>>> LoadMetadataPreferredMoviesFilesAsync(CancellationToken cancellationToken);

    Task WriteMetadataPreferredMoviesFilesAsync(ConcurrentDictionary<string, List<PreferredFileMetadata>> value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, RareMetadata>> LoadMetadataRareMoviesAsync(CancellationToken cancellationToken);

    Task WriteMetadataRareMoviesAsync(ConcurrentDictionary<string, RareMetadata> value, CancellationToken cancellationToken);

    Task<SharedMetadata[]> LoadMetadataSharedMoviesAsync(CancellationToken cancellationToken);

    Task WriteMetadataSharedMoviesAsync(SharedMetadata[] value, CancellationToken cancellationToken);

    Task<string[]> LoadMetadataAllMoviesSpecialAsync(CancellationToken cancellationToken);

    Task WriteMetadataAllMoviesSpecialAsync(string[] value, CancellationToken cancellationToken);

    Task<ConcurrentDictionary<string, ImdbMetadata>> LoadMetadataAllMoviesAsync(CancellationToken cancellationToken);

    Task WriteMetadataAllMoviesAsync(ConcurrentDictionary<string, ImdbMetadata> value, CancellationToken cancellationToken);

    Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopTVX265Async(CancellationToken cancellationToken);

    Task WriteMetadataTopTVX265Async(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken);

    Task<ParallelQuery<(int Order, string Title, string Category, string Id, string MagnetUrl, string ImdbId, DateTime DateTime)>> LoadMetadataTopMediaAsync(CancellationToken cancellationToken);

    Task<ContrastMetadata[]> LoadMetadataContrastTVAsync(CancellationToken cancellationToken);
}
