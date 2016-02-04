// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Bellevues.com">
//   Copyright (c) Bellevues.com. All rights reserved.
// </copyright>
// <summary>
//   Defines the StringExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dixin.Common
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static string With
                (this string format, params object[] args) => string.Format(CultureInfo.InvariantCulture, format, args);

        public static bool ContainsIgnoreCase(this string value, string substring)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool EqualsIgnoreCase
                (this string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        public static bool EqualsOrdinal
                (this string a, string b) => string.Equals(a, b, StringComparison.Ordinal);

        public static string Left
            (this string value, int count) =>
                string.IsNullOrEmpty(value) || count < 1 ? string.Empty : value.Substring(0, Math.Min(count, value.Length));

        public static void LogWith(this string message, TextWriter logger = null) => logger?.WriteLine(message);

        public static string GetTitleFromHtml(this string html)
        {
            Match match = new Regex(
                @".*<head>.*<title>(.*)</title>.*</head>.*",
                RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(html);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static bool Any(this string value, UnicodeCategory category) =>
            !string.IsNullOrWhiteSpace(value)
            && value.Any(@char => char.GetUnicodeCategory(@char) == category);

        public static bool HasOtherLetter(this string value) => value.Any(UnicodeCategory.OtherLetter);
    }
}