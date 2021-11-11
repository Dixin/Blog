namespace Examples.Tests.Dynamic;

internal struct StructTest
{
    private int value;

    internal StructTest(int value)
    {
        this.value = value;
    }

    internal int Value
    {
        get => this.value;

        set => this.value = value;
    }
}