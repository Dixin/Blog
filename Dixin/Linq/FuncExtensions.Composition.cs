namespace Dixin.Linq
{
    using System;

    public static partial class FuncExtensions
    {
        public static Func<T1, T3> o<T1, T2, T3>
            (this Func<T2, T3> function2, Func<T1, T2> function1) =>
                arg => function2(function1(arg));
    }

    public static partial class FuncExtensions
    {
        public static T Id<T>
            (T value) => value;
    }
}
