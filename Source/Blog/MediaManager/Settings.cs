namespace MediaManager;

using System.Buffers;
using Examples.Common;
using Examples.Linq;
using MediaManager.IO;
using MediaManager.Net;

public partial record Settings(
    string KeywordTopEnglish,
    string KeywordTopForeign,
    string KeywordPreferredOld,
    string KeywordPreferredNew,
    string KeywordContrast,

    string DirectoryLibrary,
    string DirectoryMetadataAllMoviesSubtitles,
    string DirectoryMetadataAllTVSubtitles,

    string UrlDrive115,
    string UrlTopMoviesX265,
    string UrlTopMoviesX265English,
    string UrlTopMoviesX265Foreign,
    string UrlTopMoviesH264,
    string UrlTopMoviesH264English,
    string UrlTopMoviesH264Foreign,
    string UrlTopMoviesH264720P,
    string UrlPreferredMovies,
    string UrlSharedMovies,
    string UrlTopTVX265,
    string UrlContrastTV,

    string MetadataDrive115Media,
    string MetadataLibraryMovies,
    string MetadataAllMoviesIgnored,
    string MetadataLibraryMoviesExternal,
    string MetadataTopMoviesX265,
    string MetadataTopMoviesX265X,
    string MetadataTopMoviesH264,
    string MetadataTopMoviesH264X,
    string MetadataTopMoviesH264720P,
    string MetadataPreferredMoviesSummaries,
    string MetadataPreferredMovies,
    string MetadataPreferredMoviesFiles,
    string MetadataRareMovies,
    string MetadataSharedMovies,
    string MetadataAllMoviesSpecial,
    string MetadataAllMovies,
    string MetadataTopTVX265,
    string MetadataContrastTV,

    string DirectoryMetadataAllMovies,
    string DirectoryMetadataAllMoviesBackup,
    string DirectoryMetadataAllMoviesCache,
    string DirectoryMetadataAllMoviesFile,
    string DirectoryMetadataAllMoviesCacheBackup,
    string DirectoryMetadataAllTV,
    string DirectoryMetadataAllTVCache,

    string MetadataTopMediaMagnetUrls,
    string MetadataTopMedia,

    string FileTemp)
{
    public DirectorySettings AudioControversial { get; init; }

    public DirectorySettings AudioMainstream { get; init; }

    public DirectorySettings AudioShow { get; init; }

    public DirectorySettings AudioSoundtrack { get; init; }

    public DirectorySettings Movie3D { get; init; }

    public DirectorySettings Movie4KHdr { get; init; }

    public DirectorySettings Movie4KHdrOverflow { get; init; }

    public DirectorySettings MovieControversial { get; init; }

    public DirectorySettings MovieControversialWithoutSubtitle { get; init; }

    public DirectorySettings MovieDisk { get; init; }

    public DirectorySettings MovieFranchise { get; init; }

    public DirectorySettings MovieMainstream1 { get; init; }

    public DirectorySettings MovieMainstream2 { get; init; }

    public DirectorySettings MovieMainstream3 { get; init; }

    public DirectorySettings MovieMusical { get; init; }

    public DirectorySettings MovieTemp1 { get; init; }

    public DirectorySettings MovieTemp2 { get; init; }

    public DirectorySettings MovieTemp3 { get; init; }

    public DirectorySettings MovieTemp4KHdr1 { get; init; }

    public DirectorySettings MovieTemp4KHdr2 { get; init; }

    public DirectorySettings TV4KHdr { get; init; }

    public DirectorySettings TVControversial { get; init; }

    public DirectorySettings TVDocumentary { get; init; }

    public DirectorySettings TVMainstream { get; init; }

    public DirectorySettings TVMainstreamChinese { get; init; }

    public DirectorySettings TVMainstreamOverflow { get; init; }

    public DirectorySettings TVTutorial { get; init; }

    public DirectorySettings TVTemp1 { get; init; }

    public DirectorySettings TVTemp2 { get; init; }

    public Dictionary<string, string[]> MovieRegions { get; init; } = new();

    public Dictionary<string, string> MovieRegionDirectories { get; init; } = new();

    public string[] ImdbKeywords { get; init; } = [];

    public string[] AllImdbKeywords { get; init; } = [];

    public string[] MetadataTopMoviesDuplication { get; init; } = [];
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

    public async Task<Dictionary<string, VideoMetadata>> LoadMetadataLibraryMoviesExternalAsync(CancellationToken cancellationToken) =>
        this.movieExternalMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, VideoMetadata>>(this.MetadataLibraryMoviesExternal, cancellationToken);

    public async Task WriteMetadataLibraryMoviesExternalAsync(Dictionary<string, VideoMetadata> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieExternalMetadata = value, this.MetadataLibraryMoviesExternal, cancellationToken: cancellationToken);

    private ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>? movieLibraryMetadataAsync;

    public async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>> LoadMetadataLibraryMoviesAsync(CancellationToken cancellationToken) =>
        this.movieLibraryMetadataAsync ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>>>(this.MetadataLibraryMovies, cancellationToken);

    public async Task WriteMetadataLibraryMoviesAsync(ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieLibraryMetadataAsync = value, this.MetadataLibraryMovies, cancellationToken: cancellationToken);

    private HashSet<string>? movieIgnoredMetadata;

    public async Task<HashSet<string>> LoadMetadataAllMoviesIgnoredAsync(CancellationToken cancellationToken) =>
        this.movieIgnoredMetadata ??= (await JsonHelper.DeserializeFromFileAsync<string[]>(this.MetadataAllMoviesIgnored, cancellationToken)).ToHashSetOrdinalIgnoreCase();

    public async Task WriteMetadataAllMoviesIgnoredAsync(HashSet<string> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieIgnoredMetadata = value, this.MetadataAllMoviesIgnored, cancellationToken: cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopX265Metadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesX265Async(CancellationToken cancellationToken) =>
        this.movieTopX265Metadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MetadataTopMoviesX265, cancellationToken);

    public async Task WriteMetadataTopMoviesX265Async(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopX265Metadata = value, this.MetadataTopMoviesX265, cancellationToken: cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopH264Metadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesH264Async(CancellationToken cancellationToken) =>
        this.movieTopH264Metadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MetadataTopMoviesH264, cancellationToken);

    public async Task WriteMetadataTopMoviesH264Async(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopH264Metadata = value, this.MetadataTopMoviesH264, cancellationToken: cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopX265XMetadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesX265XAsync(CancellationToken cancellationToken) =>
        this.movieTopX265XMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MetadataTopMoviesX265X, cancellationToken);

    public async Task WriteMetadataTopMoviesX265XAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopX265XMetadata = value, this.MetadataTopMoviesX265X, cancellationToken: cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopH264XMetadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesH264XAsync(CancellationToken cancellationToken) =>
        this.movieTopH264XMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MetadataTopMoviesH264X, cancellationToken);

    public async Task WriteMetadataTopMoviesH264XAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopH264XMetadata = value, this.MetadataTopMoviesH264X, cancellationToken: cancellationToken);

    private Dictionary<string, TopMetadata[]>? movieTopH264720PMetadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopMoviesH264720PAsync(CancellationToken cancellationToken) =>
        this.movieTopH264720PMetadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MetadataTopMoviesH264720P, cancellationToken);

    public async Task WriteMetadataTopMoviesH264720PAsync(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieTopH264720PMetadata = value, this.MetadataTopMoviesH264720P, cancellationToken: cancellationToken);

    private ConcurrentDictionary<string, PreferredSummary>? preferredSummaries;

    public async Task<ConcurrentDictionary<string, PreferredSummary>> LoadMetadataPreferredMoviesSummariesAsync(CancellationToken cancellationToken) =>
        this.preferredSummaries ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, PreferredSummary>>(this.MetadataPreferredMoviesSummaries, new(), cancellationToken);

    public async Task WriteMetadataPreferredMoviesSummariesAsync(ConcurrentDictionary<string, PreferredSummary> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.preferredSummaries = value, this.MetadataPreferredMoviesSummaries, cancellationToken: cancellationToken);

    private ConcurrentDictionary<string, List<PreferredMetadata>>? preferredMetadata;

    public async Task<ConcurrentDictionary<string, List<PreferredMetadata>>> LoadMetadataPreferredMoviesAsync(CancellationToken cancellationToken) =>
        this.preferredMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, List<PreferredMetadata>>>(this.MetadataPreferredMovies, cancellationToken);

    public async Task WriteMetadataPreferredMoviesAsync(ConcurrentDictionary<string, List<PreferredMetadata>> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.preferredMetadata = value, this.MetadataPreferredMovies, cancellationToken: cancellationToken);

    private ConcurrentDictionary<string, List<PreferredFileMetadata>>? preferredFileMetadata;

    public async Task<ConcurrentDictionary<string, List<PreferredFileMetadata>>> LoadMetadataPreferredMoviesFilesAsync(CancellationToken cancellationToken) =>
     this.preferredFileMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, List<PreferredFileMetadata>>>(this.MetadataPreferredMoviesFiles, new(), cancellationToken);

    public async Task WriteMetadataPreferredMoviesFilesAsync(ConcurrentDictionary<string, List<PreferredFileMetadata>> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.preferredFileMetadata = value, this.MetadataPreferredMoviesFiles, cancellationToken: cancellationToken);

    private ConcurrentDictionary<string, RareMetadata>? movieRareMetadata;

    public async Task<ConcurrentDictionary<string, RareMetadata>> LoadMetadataRareMoviesAsync(CancellationToken cancellationToken) =>
        this.movieRareMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, RareMetadata>>(this.MetadataRareMovies, cancellationToken);

    public async Task WriteMetadataRareMoviesAsync(ConcurrentDictionary<string, RareMetadata> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieRareMetadata = value, this.MetadataRareMovies, cancellationToken: cancellationToken);

    private SharedMetadata[]? movieSharedMetadata;

    public async Task<SharedMetadata[]> LoadMetadataSharedMoviesAsync(CancellationToken cancellationToken) =>
        this.movieSharedMetadata ??= await JsonHelper.DeserializeFromFileAsync<SharedMetadata[]>(this.MetadataSharedMovies, cancellationToken);

    public async Task WriteMetadataSharedMoviesAsync(SharedMetadata[] value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieSharedMetadata = value, this.MetadataSharedMovies, cancellationToken: cancellationToken);

    private string[]? movieImdbSpecialMetadata;

    public async Task<string[]> LoadMetadataAllMoviesSpecialAsync(CancellationToken cancellationToken) =>
        this.movieImdbSpecialMetadata ??= await JsonHelper.DeserializeFromFileAsync<string[]>(this.MetadataAllMoviesSpecial, cancellationToken);

    public async Task WriteMetadataAllMoviesSpecialAsync(string[] value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieImdbSpecialMetadata = value, this.MetadataAllMoviesSpecial, cancellationToken: cancellationToken);

    private ConcurrentDictionary<string, ImdbMetadata>? movieMergedMetadata;

    public async Task<ConcurrentDictionary<string, ImdbMetadata>> LoadMetadataAllMoviesAsync(CancellationToken cancellationToken) =>
        this.movieMergedMetadata ??= await JsonHelper.DeserializeFromFileAsync<ConcurrentDictionary<string, ImdbMetadata>>(this.MetadataAllMovies, cancellationToken);

    public async Task WriteMetadataAllMoviesAsync(ConcurrentDictionary<string, ImdbMetadata> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.movieMergedMetadata = value, this.MetadataAllMovies, cancellationToken: cancellationToken);

    private Dictionary<string, TopMetadata[]>? tvTopX265Metadata;

    public async Task<Dictionary<string, TopMetadata[]>> LoadMetadataTopTVX265Async(CancellationToken cancellationToken) =>
        this.tvTopX265Metadata ??= await JsonHelper.DeserializeFromFileAsync<Dictionary<string, TopMetadata[]>>(this.MetadataTopTVX265, cancellationToken);

    public async Task WriteMetadataTopTVX265Async(Dictionary<string, TopMetadata[]> value, CancellationToken cancellationToken) =>
        await JsonHelper.SerializeToFileAsync(this.tvTopX265Metadata = value, this.MetadataTopTVX265, cancellationToken: cancellationToken);

    private ParallelQuery<(int Order, string Title, string Category, string Id, string MagnetUrl, string ImdbId, DateTime DateTime)>? topMetadata;

    private static readonly SearchValues<string> MovieTVCategories = SearchValues.Create(["|movies_x265|", "|tv|", "|movies|"], StringComparison.OrdinalIgnoreCase);

    public async Task<ParallelQuery<(int Order, string Title, string Category, string Id, string MagnetUrl, string ImdbId, DateTime DateTime)>> LoadMetadataTopMediaAsync(CancellationToken cancellationToken) =>
        this.topMetadata ??= (await File.ReadAllLinesAsync(this.MetadataTopMedia, cancellationToken))
            .AsParallel()
            //.Where(line => line.ContainsAny(MovieTVCategories))
            .Select(line =>
            {
                string[] segments = line.Split('|');

                Debug.Assert(segments.Length >= 6);
                return (
                    Order: int.Parse(segments[0]),
                    Title: string.Join('|', segments[1..^5]),
                    Category: segments[^5],
                    Id: segments[^4],
                    MagnetUrl: segments[^3],
                    ImdbId: segments[^2],
                    DateTime: DateTime.Parse(segments[^1]));
            });

    public async Task<ContrastMetadata[]> LoadMetadataContrastTVAsync(CancellationToken cancellationToken) =>
        (await JsonHelper.DeserializeFromFileAsync<ContrastMetadata[]>(this.MetadataContrastTV, cancellationToken))
        .Where(metadata => metadata.Title.ContainsIgnoreCase(".1080p.") && metadata.Title.ContainsIgnoreCase(this.KeywordContrast) && Regex.IsMatch(metadata.Title, @"\.S[0-9]{2}\."))
        .Do(metadata => Debug.Assert(metadata.ImdbId.IsImdbId()))
        .ToArray();
}
