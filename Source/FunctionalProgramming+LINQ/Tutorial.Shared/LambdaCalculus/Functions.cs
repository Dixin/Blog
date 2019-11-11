namespace Tutorial.LambdaCalculus
{
    // Unit<T> is the alias of Func<T, T>.
    public delegate T Unit<T>(T value);

    public static partial class Functions<T>
    {
        public static readonly Unit<T>
            Id = x => x;
    }
}
