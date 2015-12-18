namespace Dixin.Linq.CategoryTheory
{
    using System.Diagnostics.Contracts;

    [Pure]
    public static partial class Functions
    {
        // Id is alias of DotNet.Category.Id().Invoke
        public static T Id<T>
            (T value) => DotNet.Category.Id<T>().Invoke(value);
    }

    // [Pure]
    public static partial class Functions
    {
        public static TFalse False<TTrue, TFalse>
            (TTrue @true, TFalse @false) => @false;
    }
}
