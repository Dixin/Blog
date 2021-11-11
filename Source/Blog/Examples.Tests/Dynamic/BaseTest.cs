namespace Examples.Tests.Dynamic;

internal class BaseTest : ITest
{
    #region Constants and Fields

    private readonly int[,] array = new int[10, 10];

    #endregion

    #region Properties

    public int Property2
    {
        get;
        set;
    }

    int ITest.Property
    {
        get;
        set;
    }

    #endregion

    #region Indexers

    public string this[int x, int y]
    {
        get => this.array[x, y].ToString(CultureInfo.InvariantCulture);

        set => this.array[x, y] = Convert.ToInt32(value);
    }

    #endregion

    #region Implemented Interfaces

    #region ITest

    public int Method(int value) => value * 2;

    #endregion

    #endregion

    #region Methods

    private int Method2(int value) => value / 2;

    #endregion
}