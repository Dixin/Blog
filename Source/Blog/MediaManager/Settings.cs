namespace MediaManager;

using Examples.Common;
using MediaManager.IO;
using MediaManager.Net;

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
    string MovieIgnoredMetadata,
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

    public DirectorySettings MovieDisk { get; init; }

    public DirectorySettings MovieExternalNew { get; init; }

    public DirectorySettings MovieExternalDelete { get; init; }

    public DirectorySettings TVControversial { get; init; }

    public DirectorySettings TVDocumentary { get; init; }

    public DirectorySettings TVMainstream { get; init; }

    public DirectorySettings TVTutorial { get; init; }

    public DirectorySettings TVMainstreamWithoutSubtitle { get; init; }

    public DirectorySettings TVTemp4 { get; init; }

    public DirectorySettings TVHdr { get; init; }

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

public partial record Settings
{
    private Dictionary<string, VideoMetadata>? movieExternalMetadata;

    public async Task<Dictionary<string, VideoMetadata>> LoadMovieExternalMetadataAsync(CancellationToken cancellationToken) =>
        this.movieExternalMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, VideoMetadata>>(this.MovieExternalMetadata, cancellationToken);

    public async Task WriteMovieExternalMetadataAsync(Dictionary<string, VideoMetadata> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieExternalMetadata = value, this.MovieExternalMetadata, cancellationToken);

    private ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>? movieLibraryMetadataAsync;

    public async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>> LoadMovieLibraryMetadataAsync(CancellationToken cancellationToken) =>
        this.movieLibraryMetadataAsync ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>>(this.MovieLibraryMetadata, cancellationToken);

    public async Task WriteMovieLibraryMetadataAsync(ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieLibraryMetadataAsync = value, this.MovieLibraryMetadata, cancellationToken);

    private HashSet<string>? movieIgnoredMetadata;

    public async Task<HashSet<string>> LoadIgnoredAsync(CancellationToken cancellationToken) =>
        this.movieIgnoredMetadata ??= new(await JsonHelper.DeserializeFromFileAsync<string[]>(this.MovieIgnoredMetadata, cancellationToken), StringComparer.OrdinalIgnoreCase);

    public async Task WriteMovieIgnoredMetadataAsync(HashSet<string> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieIgnoredMetadata = value, this.MovieIgnoredMetadata, cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopX265Metadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMovieTopX265MetadataAsync(CancellationToken cancellationToken) =>
        this.movieTopX265Metadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MovieTopX265Metadata, cancellationToken);

    public async Task WriteMovieTopX265MetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopX265Metadata = value, this.MovieTopX265Metadata, cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopH264Metadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMovieTopH264MetadataAsync(CancellationToken cancellationToken) =>
        this.movieTopH264Metadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MovieTopH264Metadata, cancellationToken);

    public async Task WriteMovieTopH264MetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopH264Metadata = value, this.MovieTopH264Metadata, cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopX265XMetadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMovieTopX265XMetadataAsync(CancellationToken cancellationToken) =>
        this.movieTopX265XMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MovieTopX265XMetadata, cancellationToken);

    public async Task WriteMovieTopX265XMetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopX265XMetadata = value, this.MovieTopX265XMetadata, cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopH264XMetadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMovieTopH264XMetadataAsync(CancellationToken cancellationToken) =>
        this.movieTopH264XMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MovieTopH264XMetadata, cancellationToken);

    public async Task WriteMovieTopH264XMetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopH264XMetadata = value, this.MovieTopH264XMetadata, cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopH264720PMetadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMovieTopH264720PMetadataAsync(CancellationToken cancellationToken) =>
        this.movieTopH264720PMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MovieTopH264720PMetadata, cancellationToken);

    public async Task WriteMovieTopH264720PMetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopH264720PMetadata = value, this.MovieTopH264720PMetadata, cancellationToken);

    private ConcurrentDictionary<string, PreferredSummary>? preferredSummaries;

    public async Task<ConcurrentDictionary<string, PreferredSummary>> LoadMoviePreferredSummaryAsync(CancellationToken cancellationToken) =>
        this.preferredSummaries ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, PreferredSummary>>(this.MoviePreferredSummary, new(), cancellationToken);

    public async Task WriteMoviePreferredSummaryAsync(ConcurrentDictionary<string, PreferredSummary> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.preferredSummaries = value, this.MoviePreferredSummary, cancellationToken);

    private ConcurrentDictionary<string, List<PreferredMetadata>>? preferredMetadata;

    public async Task<ConcurrentDictionary<string, List<PreferredMetadata>>> LoadMoviePreferredMetadataAsync(CancellationToken cancellationToken) =>
        this.preferredMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, List<PreferredMetadata>>>(this.MoviePreferredMetadata, cancellationToken);

    public async Task WriteMoviePreferredMetadataAsync(ConcurrentDictionary<string, List<PreferredMetadata>> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.preferredMetadata = value, this.MoviePreferredMetadata, cancellationToken);

    private ConcurrentDictionary<string, List<PreferredFileMetadata>>? preferredFileMetadata;

    public async Task<ConcurrentDictionary<string, List<PreferredFileMetadata>>> LoadMoviePreferredFileMetadataAsync(CancellationToken cancellationToken) =>
     this.preferredFileMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, List<PreferredFileMetadata>>>(this.MoviePreferredFileMetadata, new(), cancellationToken);

    public async Task WriteMoviePreferredFileMetadataAsync(ConcurrentDictionary<string, List<PreferredFileMetadata>> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.preferredFileMetadata = value, this.MoviePreferredFileMetadata, cancellationToken);

    private ConcurrentDictionary<string, RareMetadata>? movieRareMetadata;

    public async Task<ConcurrentDictionary<string, RareMetadata>> LoadMovieRareMetadataAsync(CancellationToken cancellationToken) =>
        this.movieRareMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, RareMetadata>>(this.MovieRareMetadata, cancellationToken);

    public async Task WriteMovieRareMetadataAsync(ConcurrentDictionary<string, RareMetadata> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieRareMetadata = value, this.MovieRareMetadata, cancellationToken);

    private SharedMetadata[]? movieSharedMetadata;

    public async Task<SharedMetadata[]> LoadMovieSharedMetadataAsync(CancellationToken cancellationToken) =>
        this.movieSharedMetadata ??= await JsonHelper.DeserializeFromFileAsync<SharedMetadata[]>(this.MovieSharedMetadata, cancellationToken);

    public async Task WriteMovieSharedMetadataAsync(SharedMetadata[] value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieSharedMetadata = value, this.MovieSharedMetadata, cancellationToken);

    private string[]? movieImdbSpecialMetadata;

    public async Task<string[]> LoadMovieImdbSpecialMetadataAsync(CancellationToken cancellationToken) =>
        this.movieImdbSpecialMetadata ??= await JsonHelper.DeserializeFromFileAsync<string[]>(this.MovieImdbSpecialMetadata, cancellationToken);

    public async Task WriteMovieImdbSpecialMetadataAsync(string[] value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieImdbSpecialMetadata = value, this.MovieImdbSpecialMetadata, cancellationToken);

    private ConcurrentDictionary<string, ImdbMetadata>? movieMergedMetadata;

    public async Task<ConcurrentDictionary<string, ImdbMetadata>> LoadMovieMergedMetadataAsync(CancellationToken cancellationToken) =>
        this.movieMergedMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, ImdbMetadata>>(this.MovieMergedMetadata, cancellationToken);

    public async Task WriteMovieMergedMetadataAsync(ConcurrentDictionary<string, ImdbMetadata> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieMergedMetadata = value, this.MovieMergedMetadata, cancellationToken);

    private Dictionary<string, TopMetadata[]>? tvTopX265Metadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadTVTopX265MetadataAsync(CancellationToken cancellationToken) =>
        this.tvTopX265Metadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.TVTopX265Metadata, cancellationToken);

    public async Task WriteTVTopX265MetadataAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.tvTopX265Metadata = value, this.TVTopX265Metadata, cancellationToken);
}
