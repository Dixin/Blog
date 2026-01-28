namespace MediaManager.IO;

using Examples.Common;
using Examples.IO;
using MediaManager.Net;

internal static partial class Video
{
    private static string GetPrefix(string name, string id)
    {
        return name switch
        {
            "パコパコママ" => "PACOPACOMAMA",
            "一本道" => "1PONDO",
            "加勒比" => id switch
            {
                _ when id.ContainsOrdinal("-") => "CARIBBEAN",
                _ when id.ContainsOrdinal("_") => "CARIBBEANPR",
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, "The id is invalid")
            },
            "天然むすめ" => "10MUSUME",
            "muramura" => "MURAMURA",
            "东京热" => "TOKYOHOT",
            "Hey動画" => "Heydouga".ToUpperInvariant(),
            _ => string.Empty
        };
    }

    internal static void AddPrefix(string directory)
    {
        if (!Regex.IsMatch(PathHelper.GetFileName(directory), @"^[0-9\-_]+(-C)?$", RegexOptions.IgnoreCase))
        {
            return;
        }

        string metadata = Directory.GetFiles(directory, TmdbMetadata.NfoSearchPattern).FirstOrDefault(string.Empty);
        if (metadata.IsNullOrWhiteSpace())
        {
            return;
        }

        XElement? root = XDocument.Load(metadata).Root;
        string name = root?.Element("studio")?.Value ?? root?.Element("maker")?.Value ?? root?.Element("publisher")?.Value ?? string.Empty;
        string id = root?.Element("num")?.Value ?? string.Empty;
        string prefix = GetPrefix(name, id);
        if (prefix.IsNotNullOrWhiteSpace())
        {
            prefix = $"{prefix}-";
        }

        DirectoryHelper.AddPrefix(directory, prefix);
    }
}