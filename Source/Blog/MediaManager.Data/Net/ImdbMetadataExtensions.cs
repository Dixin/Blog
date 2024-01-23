namespace MediaManager.Net;

using Examples.Common;

internal static class ImdbMetadataExtensions
{
    internal static bool ExistsWithImdbId(this string path, string? imdbId) =>
        ImdbMetadata.TryRead(path, out string? fileImdbId, out _, out _, out _, out _) && fileImdbId.EqualsIgnoreCase(imdbId);

    internal static bool HasImdbId(this string file, string? imdbId) =>
        ImdbMetadata.TryGet(file, out string? fileImdbId) && fileImdbId.EqualsIgnoreCase(imdbId);

    internal static string GetImdbId(this string file) => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : throw new ArgumentOutOfRangeException(nameof(file), file, null);

    internal static bool IsImdbId(this string value) => Regex.IsMatch(value, "^tt[0-9]+$");
}
