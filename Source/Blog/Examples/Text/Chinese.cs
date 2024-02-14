namespace Examples.Text;

using Examples.Common;
using System.Buffers;

public static partial class Chinese
{
    // https://en.wikipedia.org/wiki/List_of_Unicode_characters#East_Asian_writing_systems
    public static (char Min, char Max, string Name, string Uri)[] BasicBlocks { get; } =
    [
        // ⺀
        ('\u2E80', '\u2EFF', "CJK Radicals Supplement", "https://en.wikipedia.org/wiki/CJK_Radicals_Supplement"),

        // ⼀
        ('\u2F00', '\u2FDF', "Kangxi Radicals", "https://en.wikipedia.org/wiki/Kangxi_radical#Unicode"),

        // ⿰
        ('\u2FF0', '\u2FFF', "Ideographic Description Characters", "https://en.wikipedia.org/wiki/Ideographic_Description_Characters_(Unicode_block)"),

        // 〥
        ('\u3000', '\u303F', "CJK Symbols and Punctuation", "https://en.wikipedia.org/wiki/CJK_Symbols_and_Punctuation"),

        // い
        ('\u3040', '\u309F', "Hiragana", "https://en.wikipedia.org/wiki/Hiragana_(Unicode_block)"),

        // ア
        ('\u30A0', '\u30FF', "Katakana", "https://en.wikipedia.org/wiki/Katakana_(Unicode_block)"),

        // ㄆ
        ('\u3100', '\u312F', "Bopomofo", "https://en.wikipedia.org/wiki/Bopomofo_(Unicode_block)"),

        // ㆝
        ('\u3190', '\u319F', "Kanbun", "https://en.wikipedia.org/wiki/Kanbun_(Unicode_block)"),

        // ㆡ
        ('\u31A0', '\u31BF', "Bopomofo Extended", "https://en.wikipedia.org/wiki/Bopomofo_Extended"),

        // ㇏
        // Strokes in (31C0, 31E3) are not handled by SQL Server collation: ㇀㇁㇂㇃㇄㇅㇆㇇㇈㇉㇊㇋㇌㇍㇎㇏㇐㇑㇒㇓㇔㇕㇖㇗㇘㇙㇚㇛㇜㇝㇞㇟㇠㇡㇢㇣.
        ('\u31C0', '\u31EF', "CJK Strokes", "https://en.wikipedia.org/wiki/CJK_Strokes_(Unicode_block)"),

        // ㇰ
        ('\u31F0', '\u31FF', "Katakana Phonetic Extensions", "https://en.wikipedia.org/wiki/Katakana_Phonetic_Extensions"),

        // ㊥
        ('\u3200', '\u32FF', "Enclosed CJK Letters and Months", "https://en.wikipedia.org/wiki/Enclosed_CJK_Letters_and_Months"),

        // ㍞
        ('\u3300', '\u33FF', "CJK Compatibility", "https://en.wikipedia.org/wiki/CJK_Compatibility"),

        // 㐦
        ('\u3400', '\u4DBF', "CJK Ideographs Extension A", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_A"),

        // ䷿
        ('\u4DC0', '\u4DFF', "Yijing Hexagram Symbols", "https://en.wikipedia.org/wiki/Yijing_Hexagram_Symbols_(Unicode_block)"),

        // 一
        ('\u4E00', '\u9FFF', "CJK Unified Ideographs", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_(Unicode_block)"),

        // ꀎ
        ('\uA000', '\uA48F', "Yi Syllables", "https://en.wikipedia.org/wiki/Yi_Syllables"),

        // ꒘
        ('\uA490', '\uA4CF', "Yi Radicals", "https://en.wikipedia.org/wiki/Yi_Radicals"),

        // ꜔ 
        ('\uA700', '\uA71F', "Modifier Tone Letters", "https://en.wikipedia.org/wiki/Modifier_Tone_Letters"),

        // 豈
        ('\uF900', '\uFAFF', "CJK Compatibility Ideographs", "https://en.wikipedia.org/wiki/CJK_Compatibility_Ideographs"),

        // ︗
        ('\uFE10', '\uFE1F', "Vertical Forms", "https://en.wikipedia.org/wiki/Vertical_Forms"),

        // ︽
        ('\uFE30', '\uFE4F', "CJK Compatibility Forms", "https://en.wikipedia.org/wiki/CJK_Compatibility_Forms"),

        // ﹨
        ('\uFE50', '\uFE6F', "Small Form Variants", "https://en.wikipedia.org/wiki/Small_Form_Variants"),

        // ｬ
        ('\uFF00', '\uFFEF', "Half width and Full width Forms", "https://en.wikipedia.org/wiki/Halfwidth_and_Fullwidth_Forms_(Unicode_block)")
    ];

    public static (int Min, int Max, string Name, string Uri)[] SurrogatePairBlocks { get; } =
    [
        // 𖿡
        (0x16FE0, 0x16FFF, "Ideographic Symbols and Punctuation", "https://en.wikipedia.org/wiki/Ideographic_Symbols_and_Punctuation"),

        // 𗀀
        (0x17000, 0x187FF, "Tangut", "https://en.wikipedia.org/wiki/Tangut_(Unicode_block)"),

        // 𘠀
        (0x18800, 0x18AFF, "Tangut Components", "https://en.wikipedia.org/wiki/Tangut_Components"),

        // 𘮐
        (0x18B00, 0x18CFF, "Khitan Small Script", "https://en.wikipedia.org/wiki/Khitan_Small_Script_(Unicode_block)"),

        // 𘴀
        (0x18D00, 0x18D7F, "Tangut Supplement", "https://en.wikipedia.org/wiki/Tangut_Supplement"),

        // 𚿻
        (0x1AFF0, 0x1AFFF, "Kana Extended-B", "https://en.wikipedia.org/wiki/Kana_Extended-B"),

        // 𛁺
        (0x1B000, 0x1B0FF, "Kana Supplement", "https://en.wikipedia.org/wiki/Kana_Supplement"),

        // 𛄏
        (0x1B100, 0x1B12F, "Kana Extended-A", "https://en.wikipedia.org/wiki/Kana_Extended-A"),

        // 𛄯
        (0x1B130, 0x1B16F, "Small Kana Extension", "https://en.wikipedia.org/wiki/Small_Kana_Extension"),

        // 𛅰
        (0x1B170, 0x1B2FF, "Nushu", "https://en.wikipedia.org/wiki/Nushu_(Unicode_block)"),

        // 𝌿
        (0x1D300, 0x1D35F, "Tai Xuan Jing Symbols", "https://en.wikipedia.org/wiki/Taixuanjing"),

        // 𝍵
        (0x1D360, 0x1D37F, "Counting Rod Numerals", "https://en.wikipedia.org/wiki/Counting_Rod_Numerals_(Unicode_block)"),

        // 🈗
        (0x1F200, 0x1F2FF, "Enclosed Ideographic Supplement", "https://en.wikipedia.org/wiki/Enclosed_Ideographic_Supplement"),

        // 𠀀
        (0x20000, 0x2A6DF, "CJK Ideographs Extension B", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_B"),

        // 𪜀
        (0x2A700, 0x2B73F, "CJK Ideographs Extension C", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_C"),

        // 𫝀
        (0x2B740, 0x2B81F, "CJK Ideographs Extension D", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_D"),

        // 𫢸
        (0x2B820, 0x2CEAF, "CJK Ideographs Extension E", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_E"),

        // 𬺰
        (0x2CEB0, 0x2EBEF, "CJK Ideographs Extension F", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_F"),

        // 丽
        (0x2F800, 0x2FA1F, "CJK Comparability Ideographs Supplement", "https://en.wikipedia.org/wiki/CJK_Compatibility_Ideographs_Supplement"),

        // 𰀃
        (0x30000, 0x3134F, "CJK Ideographs Extension G", "https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_Extension_G")
    ];

    public static readonly SearchValues<char> CommonTraditionalChinese = SearchValues.Create("這個們來時為說國著後會過對於學麼發當還樣種開總從無現動頭經長兒愛給間親進話與問爾點幾將實聲車機氣書體卻電門聽員許寫馬難結樂東記處讓應場報關張認軍歲覺萬邊風媽變師戰遠輕條達業羅錢紶嗎語離飛歡題該論終請醫製決窢傳講讀運則產視連類隊觀盡紅識亞術熱興談極講辦強華諣計雙轉訴稱麗領節統斷歷驚臉選緊維絕樹傷願誰準聯婦紀買靜詩獨復義確單蘭舉鍾遊號費價圖剛腦響禮細專塊腳靈據眾筆習務須試懷調廣蘇顯議夢錯設線雖養際陽紙納驗夠嚴證飯導頓獲藝創區謝組館質續標實倫護貝劇險煙依鬥幫漢慢聞資擊顧淚團聖園勞陳魚異寶權魯簡態級尋殺勝範樓貴責較職屬漸錄絲黨繼趕葉賣堅遺臨擔戲衛藥詞雲規舊適鄉彈鐵壓負雜畢亂頂農練徵壞餘蒆燈環憶歐層陣瑪島項惡戀擁營諾銀勢獎優課鳥劉敗揮鮮財槍夥傑跡藸遍蓋順薩劃歸聽預編濟釋燒誤");

    public static IEnumerable<(int Index, int Length)> GetTextElements(this string value)
    {
        int index = 0;
        ReadOnlyMemory<char> remaining = value.AsMemory(); // ReadOnlySpan<> cannot be used with yield.
        while (!remaining.IsEmpty)
        {
            int length = StringInfo.GetNextTextElementLength(remaining.Span);
            yield return (index, length);
            index += length;
            remaining = remaining[length..];
        }
    }

    private static string GetChart(int min) => $"https://www.unicode.org/charts/PDF/U{min:X4}.pdf";

    public static bool IsChineseCharacter(
        [NotNullWhen(true)] string? value,
        int index,
        out bool isSurrogatePair,
        [NotNullWhen(true)] out string? unicodeBlockName,
        [NotNullWhen(true)] out string? unicodeBlockUri,
        [NotNullWhen(true)] out string? unicodeBlockChart)
    {
        return IsChineseCharacter(value.AsSpan(), index, out isSurrogatePair, out unicodeBlockName, out unicodeBlockUri, out unicodeBlockChart);
    }

    public static bool IsChineseCharacter(
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
            int lowIndex = index + 1;
            if (lowIndex == value.Length)
            {
                isSurrogatePair = false;
                return false;
            }

            int codePoint = char.ConvertToUtf32(value[index], value[lowIndex]);
            foreach ((int Min, int Max, string Name, string Uri) block in SurrogatePairBlocks)
            {
                if (codePoint <= block.Max && codePoint >= block.Min)
                {
                    unicodeBlockName = block.Name;
                    unicodeBlockUri = block.Uri;
                    unicodeBlockChart = GetChart(block.Min);
                    return true;
                }
            }

            return false;
        }

        char character = value[index];
        foreach ((char Min, char Max, string Name, string Uri) block in BasicBlocks)
        {
            if (character <= block.Max && character >= block.Min)
            {
                unicodeBlockName = block.Name;
                unicodeBlockUri = block.Uri;
                unicodeBlockChart = GetChart(block.Min);
                return true;
            }
        }

        return false;
    }

    public static bool IsChineseCharacter([NotNullWhen(true)] string? value, int index) =>
        IsChineseCharacter(value, index, out _, out _, out _, out _);

    public static bool ContainsChineseCharacter(
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
            if (IsChineseCharacter(value, index, out isSurrogatePair, out unicodeBlockName, out unicodeBlockUri, out unicodeBlockChart))
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

    public static bool ContainsChineseCharacter([NotNullWhen(true)] this string? value) =>
        ContainsChineseCharacter(value, out _, out _, out _, out _);

    public static bool IsSingleChineseCharacter(
        [NotNullWhen(true)] this string? value,
        out bool isSurrogatePair,
        [NotNullWhen(true)] out string? unicodeBlockName,
        [NotNullWhen(true)] out string? unicodeBlockUri,
        [NotNullWhen(true)] out string? unicodeBlockChart) =>
        IsChineseCharacter(value, 0, out isSurrogatePair, out unicodeBlockName, out unicodeBlockUri, out unicodeBlockChart) && (isSurrogatePair ? value.Length == 2 : value.Length == 1);

    public static bool IsSingleChineseCharacter([NotNullWhen(true)] this string? value) =>
        IsSingleChineseCharacter(value, out _, out _, out _, out _);

    public static bool IsSingleCharacter([NotNullWhen(true)] this string? value, out bool isSingleSurrogatePair, [NotNullWhen(false)] out Exception? error, [CallerArgumentExpression(nameof(value))] string argument = "")
    {
        // Equivalent to: !string.IsNullOrEmpty(text) && new StringInfo(text).LengthInTextElements == 1.
        if (string.IsNullOrEmpty(value))
        {
            error = new ArgumentNullException(argument, "Input is null or empty.");
            isSingleSurrogatePair = false;
            return false;
        }

        int length = value.Length;
        if (length > 2)
        {
            error = new ArgumentOutOfRangeException(argument, "Input is more than a single character.");
            isSingleSurrogatePair = false;
            return false;
        }

        // length is either 1 or 2.
        isSingleSurrogatePair = char.IsHighSurrogate(value, 0);
        if (isSingleSurrogatePair)
        {
            // length must be 2.
            if (length != 2)
            {
                error = new ArgumentException("Input is a single surrogate character and missing another character in the pair.", argument);
                isSingleSurrogatePair = false;
                return false;
            }
        }
        else
        {
            // length must be 1.
            if (length != 1)
            {
                error = new ArgumentOutOfRangeException(argument, "Input is more than a single character.");
                // isSingleSurrogatePair = false;
                return false;
            }
        }

        error = null;
        return true;
    }

    public static bool IsSingleChineseCharacter(
        [NotNullWhen(true)] this string? value,
        out bool isSingleSurrogatePair,
        [NotNullWhen(true)] out string? unicodeBlockName,
        [NotNullWhen(true)] out string? unicodeBlockUri,
        [NotNullWhen(true)] out string? unicodeBlockChart,
        [NotNullWhen(false)] out Exception? error,
        [CallerArgumentExpression(nameof(value))] string argument = "")
    {
        unicodeBlockName = null;
        unicodeBlockUri = null;
        unicodeBlockChart = null;
        if (!IsSingleCharacter(value, out isSingleSurrogatePair, out error, argument))
        {
            return false;
        }

        if (IsChineseCharacter(value, 0, out isSingleSurrogatePair, out unicodeBlockName, out unicodeBlockUri, out unicodeBlockChart))
        {
            return true;
        }

        error = new ArgumentOutOfRangeException(argument, "Input is a single character but not Chinese.");
        return false;
    }

    private const char HalfWidthSpace = ' '; // 32

    private const char FullWidthSpace = '　'; // 12288

    private const char HalfWidthExclamation = '!'; // 33

    private const char FullWidthExclamation = '！'; // 65281

    private const char HalfWidthTilde = '~'; // 126

    private const char FullWidthTilde = '～'; // 65374

    private const char FullHalfWidthDelta = (char)65248;

    public static char ToHalfWidth(this char value) => value switch
    {
        FullWidthSpace => HalfWidthSpace,
        >= FullWidthExclamation and <= FullWidthTilde => (char)(value - FullHalfWidthDelta),
        _ => value
    };

    public static string ToHalfWidth(this string value)
    {
        char[] characters = value.ToCharArray();
        for (int index = 0; index < characters.Length; index++)
        {
            characters[index] = characters[index].ToHalfWidth();
        }

        return new string(characters);
    }

    public static string ToFullWidth(this string value)
    {
        char[] characters = value.ToCharArray();
        for (int index = 0; index < characters.Length; index++)
        {
            characters[index] = characters[index] switch
            {
                HalfWidthSpace => FullWidthSpace,
                >= HalfWidthExclamation and <= HalfWidthTilde => (char)(characters[index] + FullHalfWidthDelta),
                _ => characters[index]
            };
        }

        return new string(characters);
    }

    private const char FullWidthZero = '０';

    private const char FullWidthNine = '９';

    private const char FullWidthUpperA = 'Ａ';

    private const char FullWidthLowerZ = 'ｚ';

    public static string ToHalfWidthLettersAndNumbers(this string value)
    {
        char[] characters = value.ToCharArray();
        for (int index = 0; index < characters.Length; index++)
        {
            characters[index] = characters[index] switch
            {
                >= FullWidthZero and <= FullWidthNine => (char)(characters[index] - FullHalfWidthDelta),
                >= FullWidthUpperA and <= FullWidthLowerZ => (char)(characters[index] - FullHalfWidthDelta),
                _ => characters[index]
            };
        }

        return new string(characters);
    }

    public static bool ContainsCommonTraditionalChineseCharacter(this string? value) => 
        value.IsNotNullOrWhiteSpace() && value.AsSpan().ContainsAny(CommonTraditionalChinese);
}
