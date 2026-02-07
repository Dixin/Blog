namespace Examples.Common;

using System.Web;

public static class StringExtensions
{
    public static string With(this string format, params object[] args) =>
        string.Format(CultureInfo.InvariantCulture, format, args);


    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) =>
        string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string? value) =>
        !string.IsNullOrWhiteSpace(value);

    public static string IfNullOrWhiteSpace(this string? value, string alternative) =>
        string.IsNullOrWhiteSpace(value) ? alternative : value;

    public static string Left(this string value, int count) =>
        string.IsNullOrEmpty(value) || count < 1 ? string.Empty : value[..Math.Min(count, value.Length)];

    public static void LogWith(this string? message, TextWriter? logger) =>
        logger?.WriteLine(message);

    public static string GetTitleFromHtml(this string html)
    {
        Match match = new Regex(
            @".*<head>.*<title>(.*)</title>.*</head>.*",
            RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(html);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    public static bool Any(this string value, UnicodeCategory category) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Any(@char => char.GetUnicodeCategory(@char) == category);

    public static bool HasOtherLetter(this string value) => value.Any(UnicodeCategory.OtherLetter);

    public static string AppendIfMissing(this string value, string append) => value.EndsWith(append) ? value : string.Concat(value, append);

    public static IEnumerable<string> DistinctOrdinal(this IEnumerable<string> source) => source.Distinct(StringComparer.Ordinal);

    public static IEnumerable<string> DistinctIgnoreCase(this IEnumerable<string> source) => source.Distinct(StringComparer.OrdinalIgnoreCase);

    public static bool EqualsOrdinal(this string? value1, string? value2) => string.Equals(value1, value2, StringComparison.Ordinal);

    public static bool EqualsIgnoreCase(this string? value1, string? value2) => string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsOrdinal(this string value, char character) => value.Contains(character, StringComparison.Ordinal);

    public static bool ContainsIgnoreCase(this string value, char character) => value.Contains(character, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsOrdinal(this string value, string substring) => value.Contains(substring, StringComparison.Ordinal);

    public static bool ContainsIgnoreCase(this string value, string substring) => value.ThrowIfNull().Contains(substring, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsOrdinal(this IEnumerable<string> source, string value) => source.Contains(value, StringComparer.Ordinal);

    public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value) => source.Contains(value, StringComparer.OrdinalIgnoreCase);

    public static int IndexOfOrdinal(this string value, string substring) => value.IndexOf(substring, StringComparison.Ordinal);

    public static int IndexOfIgnoreCase(this string value, string substring) => value.IndexOf(substring, StringComparison.OrdinalIgnoreCase);

    public static int LastIndexOfOrdinal(this string value, string substring) => value.LastIndexOf(substring, StringComparison.Ordinal);

    public static int LastIndexOfIgnoreCase(this string value, string substring) => value.LastIndexOf(substring, StringComparison.OrdinalIgnoreCase);

    public static int CompareOrdinal(this string? value1, string? value2) => string.Compare(value1, value2, StringComparison.Ordinal);

    public static int CompareIgnoreCase(this string? value1, string? value2) => string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);

    public static bool StartsWithOrdinal(this string value, string substring) => value.ThrowIfNull().StartsWith(substring, StringComparison.Ordinal);

    public static bool StartsWithIgnoreCase(this string value, string substring) => value.ThrowIfNull().StartsWith(substring, StringComparison.OrdinalIgnoreCase);

    public static bool EndsWithIgnoreCase(this string value, string substring) => value.EndsWith(substring, StringComparison.OrdinalIgnoreCase);

    public static bool EndsWithOrdinal(this string value, string substring) => value.EndsWith(substring, StringComparison.Ordinal);

    public static string ReplaceOrdinal(this string value, string oldValue, string newValue) => value.Replace(oldValue, newValue, StringComparison.Ordinal);

    public static string ReplaceIgnoreCase(this string value, string oldValue, string newValue) => value.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<int> AllIndexesOfOrdinal(this string value, string substring) => value.AllIndexesOf(substring, StringComparison.Ordinal);

    public static IEnumerable<int> AllIndexesOfIgnoreCase(this string value, string substring) => value.AllIndexesOf(substring, StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<int> AllIndexesOf(this string value, string substring, StringComparison stringComparison)
    {
        value.ThrowIfNullOrEmpty();
        substring.ThrowIfNullOrEmpty();

        for (int index = 0; ; index += substring.Length)
        {
            index = value.IndexOf(substring, index, stringComparison);
            if (index < 0)
            {
                break;
            }

            yield return index;
        }
    }

    public static string GetUrlPath(this string url) => new Uri(url, UriKind.Absolute).GetLeftPart(UriPartial.Path);

    public static string TrimDecode(this string value) => HttpUtility.HtmlDecode(value.Trim());
}