namespace Tutorial.CategoryTheory
{
    using System;

    using Microsoft.FSharp.Core;

    public interface IMonoidalCategory<TObject, TMorphism> : ICategory<TObject, TMorphism>, IMonoid<TObject> { }

    public partial class DotNetCategory : IMonoidalCategory<Type, Delegate>
    {
        public Type Multiply(Type value1, Type value2) => typeof(ValueTuple<,>).MakeGenericType(value1, value2);

        public Type Unit() => typeof(Unit);
    }

    public partial class DotNetCategory
    {
        // Associator: (T1 x T2) x T3 -> T1 x (T2 x T3)
        // Associator: ValueTuple<ValueTuple<T1, T2>, T3> -> ValueTuple<T1, ValueTuple<T2, T3>>
        public static (T1, (T2, T3)) Associator<T1, T2, T3>(((T1, T2), T3) product) =>
            (product.Item1.Item1, (product.Item1.Item2, product.Item2));

        // LeftUnitor: Unit x T -> T
        // LeftUnitor: ValueTuple<Unit, T> -> T
        public static T LeftUnitor<T>((Unit, T) product) => product.Item2;

        // RightUnitor: T x Unit -> T
        // RightUnitor: ValueTuple<T, Unit> -> T
        public static T RightUnitor<T>((T, Unit) product) => product.Item1;
    }
}
