namespace MediaManager.Net;

using Examples.Common;

internal static class TmdbMetadataExtensions
{
    internal static bool IsTmdbId([NotNullWhen(true)] this string? value) => value.IsNotNullOrWhiteSpace() && Regex.IsMatch(value, "^[0-9]+$");
}