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
        public static T2 LeftUnitor<T2>(this Lazy<Unit, T2> bifunctor) => bifunctor.Value2;

        public static T1 RightUnitor<T1>(this Lazy<T1, Unit> bifunctor) => bifunctor.Value1;

        public static Lazy<T1, Lazy<T2, T3>> Associator<T1, T2, T3>(this Lazy<Lazy<T1, T2>, T3> product) =>
            product.Value1.Value1.Lazy(product.Value1.Value2.Lazy(product.Value2));
    }
}
