namespace Examples.Text;

public static class Cjk
{
    public static bool ContainsCjkCharacter([NotNullWhen(true)] this string? value) =>
        value.ContainsChineseCharacter() || value.ContainsKoreanCharacter();
}