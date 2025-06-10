namespace Examples.Common;

public static class StringHelper
{
    private static readonly Dictionary<string, int> SmallNumbers = new(StringComparer.OrdinalIgnoreCase)
    {
        { "zero", 0 }, { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 },
        { "five", 5 }, { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 },
        { "ten", 10 }, { "eleven", 11 }, { "twelve", 12 }, { "thirteen", 13 },
        { "fourteen", 14 }, { "fifteen", 15 }, { "sixteen", 16 },
        { "seventeen", 17 }, { "eighteen", 18 }, { "nineteen", 19 }
    };

    private static readonly Dictionary<string, int> Tens = new(StringComparer.OrdinalIgnoreCase)
    {
        { "twenty", 20 }, { "thirty", 30 }, { "forty", 40 },
        { "fifty", 50 }, { "sixty", 60 }, { "seventy", 70 },
        { "eighty", 80 }, { "ninety", 90 }
    };

    private static readonly Dictionary<string, int> Scales = new(StringComparer.OrdinalIgnoreCase)
    {
        { "hundred", 100 }, { "thousand", 1000 }, { "million", 1000000 }
    };

    public static bool TryReplaceNumbers(string input, out string output)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            output = input;
            return false;
        }

        // Only allow replacements that start with a number word
        const string StarterWords = "zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety";

        const string AllNumberWords = "(?:zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|million|and)";

        // Match phrases that start with a number word and contain only number words
        const string Pattern = $@"(?<=^|\W)((?:{StarterWords})(?:[\s-]+{AllNumberWords})*)(?=\W|$)";

        bool replaced = false;

        output = Regex.Replace(input, Pattern, match =>
        {
            string phrase = match.Value.ToLower()
                .Replace("-", " ")
                .Replace(" and", " ");
            string[] words = phrase.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            long total = 0;
            long current = 0;
            bool valid = false;

            foreach (string word in words)
            {
                if (SmallNumbers.TryGetValue(word, out int small))
                {
                    current += small;
                    valid = true;
                }
                else if (Tens.TryGetValue(word, out int ten))
                {
                    current += ten;
                    valid = true;
                }
                else if (Scales.TryGetValue(word, out int scale))
                {
                    if (scale == 100)
                    {
                        if (current == 0)
                        {
                            return match.Value; // Reject "hundred" without leading number
                        }

                        current *= scale;
                    }
                    else
                    {
                        if (current == 0)
                        {
                            return match.Value; // Reject "thousand" or "million" without leading number
                        }

                        total += current * scale;
                        current = 0;
                    }
                    valid = true;
                }
                else if (word != "and")
                {
                    return match.Value; // Invalid word in phrase
                }
            }

            if (!valid)
            {
                return match.Value;
            }

            total += current;
            replaced = true;
            return total.ToString();
        }, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        return replaced;
    }
}