namespace MediaManager.Net;

using Examples.Common;
using Examples.IO;
using MediaManager.IO;

internal static class ImdbMetadataExtensions
{
    internal static bool ExistsWithImdbId(this string path, string? imdbId) =>
        ImdbMetadata.TryRead(path, out string? fileImdbId, out _, out _, out _, out _) && fileImdbId.EqualsIgnoreCase(imdbId);

    internal static bool HasImdbId(this string file, string? imdbId) =>
        ImdbMetadata.TryGet(file, out string? fileImdbId) && fileImdbId.EqualsIgnoreCase(imdbId);

    internal static string GetImdbId(this string path) =>
        ImdbMetadata.TryGet(
            path.HasExtension(ImdbMetadata.Extension)
                ? path
                : Directory.EnumerateFiles(path, Video.ImdbMetadataSearchPattern).Single(),
            out string? imdbId)
            ? imdbId
            : throw new ArgumentOutOfRangeException(nameof(path), path, string.Empty);

    internal static bool IsImdbId([NotNullWhen(true)] this string? value) => value.IsNotNullOrWhiteSpace() && ImdbMetadata.ImdbIdOnlyRegex().IsMatch(value);

    internal static bool TryGetImdbId(this XDocument xml, [NotNullWhen(true)] out string? imdbId)
    {
        XElement? root = xml.Root;
        imdbId = root?.Element("imdbid")?.Value ?? root?.Element("imdb_id")?.Value;
        return imdbId.IsImdbId();
    }

    internal static bool TryLoadXmlImdbId(this string xml, [NotNullWhen(true)] out string? imdbId)
    {
        XDocument metadata;
        try
        {
            metadata = XDocument.Load(xml);
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
            imdbId = null;
            return false;
        }

        return metadata.TryGetImdbId(out imdbId);
    }

    internal static bool TryGetTitle(this XDocument xml, [NotNullWhen(true)] out string? title)
    {
        title = xml.Root?.Element("title")?.Value;
        return title.IsNotNullOrWhiteSpace();
    }
}
