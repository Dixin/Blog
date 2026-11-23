namespace Examples.Common;

public readonly record struct Unit
{
    public static readonly Unit Value = default;

    public override string ToString() => "()";
}