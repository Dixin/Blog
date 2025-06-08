namespace Examples.Text.RegularExpressions;

public static class RegexHelper
{
    public static Regex Create(RegexOptions options = RegexOptions.None, [StringSyntax(StringSyntaxAttribute.Regex)] params string[] patterns) =>
        new(string.Join(string.Empty, patterns), options);

    public static Regex Create([StringSyntax(StringSyntaxAttribute.Regex)] params string[] patterns) =>
        new(string.Join(string.Empty, patterns));
}