using Examples.Common;
using Examples.IO;
using MediaManager.IO;

namespace MediaManager.Net;

internal static class TmdbMetadata
{
    private const string FileNameSeparator = Video.Delimiter;

    private const string FileNameMetadataSeparator = Video.VersionSeparator;

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
                        countries.First().Where(country => country.IsNotNullOrWhiteSpace()).Take(5).Select(country => country
                            .ReplaceOrdinal(FileNameSeparator, string.Empty)
                            .Replace(FileNameMetadataSeparator, string.Empty)
                            .ReplaceIgnoreCase("United States of America", "USA")
                            .ReplaceIgnoreCase("United Kingdom", "UK"))),
                    string.Join(
                        FileNameMetadataSeparator,
                        genres.First().Where(genre => genre.IsNotNullOrWhiteSpace()).Take(5).Select(genre => genre.ReplaceOrdinal(FileNameSeparator, string.Empty).Replace(FileNameMetadataSeparator, string.Empty)))
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

    internal static bool TryRead(string path, [NotNullWhen(true)] out string? tmdbId, [NotNullWhen(true)] out string? year, [NotNullWhen(true)] out string[]? regions, [NotNullWhen(true)] out string[]? genres)
    {
        if (TryRead(path, out string? file))
        {
            return TryGet(file, out tmdbId, out year, out regions, out genres);
        }

        tmdbId = null;
        year = null;
        regions = null;
        genres = null;
        return false;
    }

    private static bool TryRead(string? path, [NotNullWhen(true)] out string? file)
    {
        if (Directory.Exists(path))
        {
            file = Directory.GetFiles(path, Video.TmdbMetadataSearchPattern, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Debug.Assert(file.IsNullOrWhiteSpace() || PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag) || PathHelper.GetFileNameWithoutExtension(file).Split(FileNameSeparator).Length == 4);
            return file.IsNotNullOrWhiteSpace();
        }

        if (path.IsNotNullOrWhiteSpace() && path.EndsWith(Extension) && File.Exists(path))
        {
            file = path;
            Debug.Assert(file.IsNullOrWhiteSpace() || PathHelper.GetFileNameWithoutExtension(file).EqualsOrdinal(Video.NotExistingFlag) || PathHelper.GetFileNameWithoutExtension(file).Split(FileNameSeparator).Length == 4);
            return true;
        }

        file = null;
        return false;
    }

    internal static bool TryGet(string file, [NotNullWhen(true)] out string? tmdbId, [NotNullWhen(true)] out string? year, [NotNullWhen(true)] out string[]? regions, [NotNullWhen(true)] out string[]? genres)
    {
        tmdbId = null;
        year = null;
        regions = null;
        genres = null;

        if (!file.HasExtension(Extension))
        {
            return false;
        }

        string name = PathHelper.GetFileNameWithoutExtension(file);
        if (name.EqualsOrdinal(Video.NotExistingFlag))
        {
            return false;
        }

        string[] info = name.Split(FileNameSeparator, StringSplitOptions.TrimEntries);
        Debug.Assert(info.Length == 4 && info[0].IsTmdbId());
        tmdbId = info[0];
        year = info[1];
        regions = info[2].Split(FileNameMetadataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        genres = info[3].Split(FileNameMetadataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return true;
    }
}