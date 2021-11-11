namespace Examples.Dynamic;

public static class DynamicWrapperExtensions
{
    #region Public Methods

    public static dynamic ToDynamic<T>(this T value) // where T : class
        => new DynamicWrapper<T>(ref value);

    #endregion
}