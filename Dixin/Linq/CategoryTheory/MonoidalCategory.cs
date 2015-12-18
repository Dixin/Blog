namespace Dixin.Linq.CategoryTheory
{
    using System.Diagnostics.Contracts;

    using Microsoft.FSharp.Core;

    // [Pure]
    public static partial class LazyExtensions
    {
        public static T2 LeftUnit<T2>
            (this Lazy<Unit, T2> product) => product.Value2;

        public static T1 RightUnit<T1>
            (this Lazy<T1, Unit> product) => product.Value1;

        public static Lazy<T1, Lazy<T2, T3>> Associate<T1, T2, T3>
            (Lazy<Lazy<T1, T2>, T3> product) =>
                new Lazy<T1, Lazy<T2, T3>>(
                    () => product.Value1.Value1,
                    () => new Lazy<T2, T3>(() => product.Value1.Value2, () => product.Value2));
    }

    [Pure]
    public static class DotNetExtensions
    {
        public static Lazy<T1, T2> x<T1, T2>
            (this DotNet category, T1 value1, T2 value2) => new Lazy<T1, T2>(() => value1, () => value2);
    }

    // [Pure]
    public static partial class LazyExtensions
    {
        public static Lazy<T1, T2> x<T1, T2>
            (this T1 value1, T2 value2) => new Lazy<T1, T2>(value1, value2);
    }
}
