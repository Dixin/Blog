namespace MediaManager.Net;

using System.Web;
using Examples.Common;
using Examples.IO;
using MediaManager;
using MediaManager.IO;

internal static class Download
{
    internal static async Task PrintDownloadingTitleAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, List<PreferredFileMetadata>> preferredFileMetadata = await settings.LoadMetadataPreferredMoviesFilesAsync(cancellationToken);
        Dictionary<string, string> preferredTitleToFileName = preferredFileMetadata
            .Values
            .SelectMany(group => group)
            .ToLookup(file => file.DisplayName, file => file.File, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Distinct(StringComparer.OrdinalIgnoreCase).Single());
        (string Title, string FileName)[] downloadingItems = File.ReadAllLines(Path.Combine(settings.DirectoryLibrary, "Movie.Downloading.txt"))
            .AsParallel()
            .Where(line => line.IsNotNullOrWhiteSpace())
            .Select(line => HttpUtility.UrlDecode(line.Trim()))
            .Select(title => (
                title,
                (title.ContainsIgnoreCase(settings.KeywordPreferredOld) || title.ContainsIgnoreCase(settings.KeywordPreferredNew)) && preferredTitleToFileName.TryGetValue(title, out string? fileName) ? fileName : title
            ))
            .ToArray();
        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMetadataLibraryMoviesAsync(cancellationToken);
        string[] existingNames = existingMetadata
            .Values
            .AsParallel()
            .SelectMany(videos => videos.Select(video => PathHelper.GetFileNameWithoutExtension(video.Key)))
            .ToArray();
        downloadingItems
            .AsParallel()
            .Where(item => existingNames.Any(existingName => existingName.StartsWithIgnoreCase(item.FileName)))
            .ForAll(item => log(item.Title));
    }
}