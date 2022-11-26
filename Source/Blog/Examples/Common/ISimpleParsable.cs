namespace Examples.Common;

public interface ISimpleParsable<TSelf>
{
    static abstract TSelf Parse(string value);

    static abstract bool TryParse([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out TSelf result);
}