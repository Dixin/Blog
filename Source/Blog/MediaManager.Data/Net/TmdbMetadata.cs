using Examples.Common;
using Examples.IO;
using MediaManager.IO;

namespace MediaManager.Net;

internal static class TmdbMetadata
{
    private const string FileNameSeparator = ImdbMetadata.FileNameSeparator;

    private const string FileNameMetadataSeparator = ImdbMetadata.FileNameMetadataSeparator;

    internal const string Extension = ".xml";

    private const string SearchPattern = $"{PathHelper.AllSearchPattern}{Extension}";

    internal static async Task WriteTmdbMetadataAsync(string movie, bool overwrite = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        string[] existingMetadataFiles = Directory.GetFiles(movie, SearchPattern);
        if (!overwrite && existingMetadataFiles.Any())
        {
            return;
        }

        string[] files = Directory.GetFiles(movie, Video.XmlMetadataSearchPattern);

        if (files.Length < 1)
        {
            throw new InvalidOperationException($"No XML metadata: {movie}.");
        }

        XDocument[] documents = files.Select(XDocument.Load).ToArray();

        string[] tmdbIds = documents.Select(doc => doc.Root?.Element("tmdbid")?.Value ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (tmdbIds.Length != 1)
        {
            throw new InvalidOperationException($"Inconsistent TMDB id: {movie}.");
        }

        string tmdbId = tmdbIds.Single();
        string newMetadataFileName;
        if (tmdbId.IsNullOrWhiteSpace())
        {
            newMetadataFileName = Video.NotExistingFlag;
        }
        else
        {
            string[] years = documents.Select(doc => doc.Root?.Element("year")?.Value ?? string.Empty).Distinct().ToArray();
            if (years.Length != 1)
            {
                throw new InvalidOperationException($"Inconsistent year: {movie}.");
            }

            string[][] countries = documents.Select(doc => doc.Root?.Elements("country").Select(element => element.Value).Order().ToArray() ?? []).ToArray();
            if (countries.Select(country => string.Join("|", country)).Distinct().Count() != 1)
            {
                throw new InvalidOperationException($"Inconsistent countries: {movie}.");
            }

            string[][] genres = documents.Select(doc => doc.Root?.Elements("genre").Select(element => element.Value).Order().ToArray() ?? []).ToArray();
            if (genres.Select(genre => string.Join("|", genre)).Distinct().Count() != 1)
            {
                throw new InvalidOperationException($"Inconsistent genres: {movie}.");
            }

            newMetadataFileName = string.Join(
                FileNameSeparator,
                [
                    tmdbId,
                    years.Single(),
                    string.Join(
                        FileNameMetadataSeparator,
                        countries.First().Take(5).Select(country => country
                            .ReplaceOrdinal(FileNameSeparator, string.Empty)
                            .Replace(FileNameMetadataSeparator, string.Empty)
                            .ReplaceIgnoreCase("United States of America", "USA")
                            .ReplaceIgnoreCase("United Kingdom", "UK"))),
                    string.Join(
                        FileNameMetadataSeparator,
                        genres.First().Take(5).Select(genre => genre.ReplaceOrdinal(FileNameSeparator, string.Empty).Replace(FileNameMetadataSeparator, string.Empty)))
                ]
            );
        }

        string newPath = Path.Combine(movie, $"{newMetadataFileName}{Extension}");

        if (existingMetadataFiles.Any())
        {
            existingMetadataFiles.ForEach(FileHelper.Recycle);
        }

        log(newPath);
        await File.WriteAllTextAsync(newPath, string.Empty, cancellationToken);
    }
}