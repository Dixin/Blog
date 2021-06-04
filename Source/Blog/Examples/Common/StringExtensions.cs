namespace Examples.Common
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static partial class StringExtensions
    {
        public static string With(this string format, params object[] args) => 
            string.Format(CultureInfo.InvariantCulture, format, args);

        public static bool ContainsIgnoreCase(this string value, string substring)
        {
            Argument.NotNull(value, nameof(value));

            return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWithIgnoreCase(this string value, string substring)
        {
            Argument.NotNull(value, nameof(value));

            return value.StartsWith(substring, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool EqualsIgnoreCase
                (this string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        public static bool EqualsOrdinal
                (this string a, string b) => string.Equals(a, b, StringComparison.Ordinal);

        public static string Left
            (this string value, int count) =>
                string.IsNullOrEmpty(value) || count < 1 ? string.Empty : value.Substring(0, Math.Min(count, value.Length));

        public static void LogWith(this string? message, TextWriter? logger) => logger?.WriteLine(message);

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
    }
}