namespace Examples.Text;

using Examples.Common;

public static class Korean
{
    public static (char Min, char Max, string Name, string Uri)[] BasicBlocks { get; } =
    [
        // ᄀ
        ('\u1100', '\u11FF', "Hangul Jamo", "https://en.wikipedia.org/wiki/Hangul_Jamo_(Unicode_block)"),

        // ㄱ
        ('\u3130', '\u318F', "Hangul Compatibility Jamo", "https://en.wikipedia.org/wiki/Hangul_Compatibility_Jamo"),
        
        // 가
        ('\uAC00', '\uD7AF', "Hangul Syllables", "https://en.wikipedia.org/wiki/Hangul_Syllables")
    ];

    public static bool IsKoreanCharacter(
        [NotNullWhen(true)] string? value,
        int index,
        out bool isSurrogatePair,
        [NotNullWhen(true)] out string? unicodeBlockName,
        [NotNullWhen(true)] out string? unicodeBlockUri,
        [NotNullWhen(true)] out string? unicodeBlockChart) =>
        IsKoreanCharacter(value.AsSpan(), index, out isSurrogatePair, out unicodeBlockName, out unicodeBlockUri, out unicodeBlockChart);

    public static bool IsKoreanCharacter(
        ReadOnlySpan<char> value,
        int index,
        out bool isSurrogatePair,
        [NotNullWhen(true)] out string? unicodeBlockName,
        [NotNullWhen(true)] out string? unicodeBlockUri,
        [NotNullWhen(true)] out string? unicodeBlockChart)
    {
        unicodeBlockName = null;
        unicodeBlockUri = null;
        unicodeBlockChart = null;
        if (value.Length == 0 || index >= value.Length)
        {
            isSurrogatePair = false;
            return false;
        }

        isSurrogatePair = char.IsHighSurrogate(value[index]);
        if (isSurrogatePair)
        {
            return false;
        }

        char character = value[index];
        (char Min, char Max, string Name, string Uri) block = BasicBlocks.FirstOrDefault(block => character <= block.Max && character >= block.Min);
        if (block.Name.IsNullOrWhiteSpace())
        {
            return false;
        }

        unicodeBlockName = block.Name;
        unicodeBlockUri = block.Uri;
        unicodeBlockChart = Chinese.GetChart(block.Min);
        return true;
    }

    public static bool ContainsKoreanCharacter(
        [NotNullWhen(true)] this string? value,
        out bool isSurrogatePair,
        [NotNullWhen(true)] out string? unicodeBlockName,
        [NotNullWhen(true)] out string? unicodeBlockUri,
        [NotNullWhen(true)] out string? unicodeBlockChart)
    {
        isSurrogatePair = false;
        unicodeBlockName = null;
        unicodeBlockUri = null;
        unicodeBlockChart = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        for (int index = 0; index < value.Length; index++)
        {
            if (IsKoreanCharacter(value, index, out isSurrogatePair, out unicodeBlockName, out unicodeBlockUri, out unicodeBlockChart))
            {
                return true;
            }

            if (isSurrogatePair)
            {
                index++;
            }
        }

        return false;
    }

    public static bool ContainsKoreanCharacter([NotNullWhen(true)] this string? value) =>
        ContainsKoreanCharacter(value, out _, out _, out _, out _);
}