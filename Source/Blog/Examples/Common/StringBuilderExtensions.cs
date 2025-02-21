namespace Examples.Common;

public static class StringBuilderExtensions
{
    public static int IndexOf(this StringBuilder stringBuilder, char character)
    {
        stringBuilder.ThrowIfNull();

        for (int index = 0; index < stringBuilder.Length; index++)
        {
            if (stringBuilder[index] == character)
            {
                return index;
            }
        }

        return -1;
    }

    public static int IndexOf(this StringBuilder stringBuilder, string substring)
    {
        stringBuilder.ThrowIfNull();
        substring.ThrowIfNullOrEmpty();

        if (substring.Length == 1)//can't beat just spinning through for it
        {
            return stringBuilder.IndexOf(substring[0]);
        }

        int m = 0;
        int i = 0;
        int[] table = KmpTable(substring);
        while (m + i < stringBuilder.Length)
        {
            if (substring[i] == stringBuilder[m + i])
            {
                if (i == substring.Length - 1)
                {
                    return m == substring.Length ? -1 : m;//match -1 = failure to find conventional in .NET
                }

                ++i;
            }
            else
            {
                m = m + i - table[i];
                i = table[i] > -1 ? table[i] : 0;
            }
        }
        return -1;
    }

    private static int[] KmpTable(string sought)
    {
        int[] table = new int[sought.Length];
        int pos = 2;
        int cnd = 0;
        table[0] = -1;
        table[1] = 0;
        while (pos < table.Length)
        {
            if (sought[pos - 1] == sought[cnd])
            {
                table[pos++] = ++cnd;
            }
            else if (cnd > 0)
            {
                cnd = table[cnd];
            }
            else
            {
                table[pos++] = 0;
            }
        }

        return table;
    }

    public static StringBuilder TrimEnd(this StringBuilder stringBuilder)
    {
        stringBuilder.ThrowIfNull();

        int length = stringBuilder.Length;
        int whiteSpaceLength = 0;
        for (; whiteSpaceLength < length && char.IsWhiteSpace(stringBuilder[length - 1 - whiteSpaceLength]); whiteSpaceLength++)
        {
        }

        return whiteSpaceLength > 0 ? stringBuilder.Remove(length - whiteSpaceLength, whiteSpaceLength) : stringBuilder;
    }

    public static StringBuilder TrimEnd(this StringBuilder stringBuilder, ReadOnlySpan<char> trim)
    {
        stringBuilder.ThrowIfNull();

        int length = stringBuilder.Length;
        int matchLength = 0;
        for (; matchLength < length && trim.Contains(stringBuilder[length - 1 - matchLength]); matchLength++)
        {
        }

        return matchLength > 0 ? stringBuilder.Remove(length - matchLength, matchLength) : stringBuilder;
    }

    public static StringBuilder TrimStart(this StringBuilder stringBuilder)
    {
        stringBuilder.ThrowIfNull();

        int whiteSpaceLength = 0;
        for (; whiteSpaceLength < stringBuilder.Length && char.IsWhiteSpace(stringBuilder[whiteSpaceLength]); whiteSpaceLength++)
        {
        }

        return whiteSpaceLength > 0 ? stringBuilder.Remove(0, whiteSpaceLength) : stringBuilder;
    }

    public static StringBuilder TrimStart(this StringBuilder stringBuilder, ReadOnlySpan<char> trim)
    {
        stringBuilder.ThrowIfNull();

        int matchLength = 0;
        for (; matchLength < stringBuilder.Length && trim.Contains(stringBuilder[matchLength]); matchLength++)
        {
        }

        return matchLength > 0 ? stringBuilder.Remove(0, matchLength) : stringBuilder;
    }

    public static StringBuilder Trim(this StringBuilder stringBuilder)
    {
        stringBuilder.ThrowIfNull();

        return stringBuilder.TrimStart().TrimEnd();
    }

    public static StringBuilder Trim(this StringBuilder stringBuilder, ReadOnlySpan<char> trim)
    {
        stringBuilder.ThrowIfNull();

        return stringBuilder.TrimStart(trim).TrimEnd(trim);
    }
}