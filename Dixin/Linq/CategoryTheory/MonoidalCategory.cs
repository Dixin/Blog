namespace Dixin.Linq.CategoryTheory
{
    using System;

    using Microsoft.FSharp.Core;

    public interface IMonoidalCategory<TObject, TMorphism> : ICategory<TObject, TMorphism>, IMonoid<TObject>
    {
    }

    public partial class DotNetCategory : IMonoidalCategory<Type, Delegate>
    {
        public Type Multiply(Type value1, Type value2) => typeof(Tuple<,>).MakeGenericType(value1, value2);

        public Type Unit() => typeof(Unit);
    }

    public partial class DotNetCategory
    {
        // Associator: (T1 x T2) x T3 -> T1 x (T2 x T3)
        // Associator: Tuple<Tuple<T1, T2>, T3> -> Tuple<T1, Tuple<T2, T3>>
        public static Tuple<T1, Tuple<T2, T3>> Associator<T1, T2, T3>(Tuple<Tuple<T1, T2>, T3> product) =>
            product.Item1.Item1.Tuple(product.Item1.Item2.Tuple(product.Item2));

        // LeftUnitor: Unit x T -> T
        // LeftUnitor: Tuple<Unit, T> -> T
        public static T LeftUnitor<T>(Tuple<Unit, T> product) => product.Item2;

        // RightUnitor: T x Unit -> T
        // RightUnitor: Tuple<T, Unit> -> T
        public static T RightUnitor<T>(Tuple<T, Unit> product) => product.Item1;
    }
}
