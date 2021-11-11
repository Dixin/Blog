namespace Examples.Tests.Dynamic;

internal class StaticTest
{
#pragma warning disable 649
    private static int value;
#pragma warning restore 649

    internal static int Value => value;

    internal static int Method() => 2;
}