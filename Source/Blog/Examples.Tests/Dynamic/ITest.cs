namespace Examples.Tests.Dynamic;

public interface ITest
{
    #region Properties

    int Property
    {
        get;
        set;
    }

    #endregion

    #region Indexers

    string this[int x, int y]
    {
        get;
        set;
    }

    #endregion

    #region Public Methods

    int Method(int value);

    #endregion
}