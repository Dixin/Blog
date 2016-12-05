namespace Dixin.Linq.CategoryTheory
{
    using System;

    using Microsoft.FSharp.Core;

    public interface IMonoidalCategory<TObject, TMorphism> : ICategory<TObject, TMorphism>, IMonoid<TObject>
    {
    }

    public partial class DotNetCategory : IMonoidalCategory<Type, Delegate>
    {
        public Type Multiply(Type value1, Type value2) => typeof(Lazy<,>).MakeGenericType(value1, value2);

        public Type Unit() => typeof(Unit);
    }

    public static partial class LazyExtensions
    {
        // LeftUnitor: Unit x T -> T
        public static T LeftUnitor<T>(this Lazy<Unit, T> product) => product.Value2;

        // RightUnitor: T x Unit -> T
        public static T RightUnitor<T>(this Lazy<T, Unit> product) => product.Value1;

        // Associator: (T1 x T2) x T3 -> T1 x (T2 x T3)
        public static Lazy<T1, Lazy<T2, T3>> Associator<T1, T2, T3>(this Lazy<Lazy<T1, T2>, T3> product) =>
            product.Value1.Value1.Lazy(product.Value1.Value2.Lazy(product.Value2));
    }
}
