namespace Dixin.Linq
{
    using System;

    using Dixin.Common;
    using System.Linq;
    using System.Diagnostics;
    using System.Collections.Generic;

    public static partial class FuncExtensions
    {
        public static TResult Forward<T, TResult>(this T arg, Func<T, TResult> func)
        {
            func.NotNull(nameof(func));

            return func(arg);
        }

        public static TResult Forward<T1, T2, TResult>(this T1 arg1, Func<T1, T2, TResult> func, T2 arg2)
        {
            func.NotNull(nameof(func));

            return func(arg1, arg2);
        }

        public static TResult Forward<T1, T2, T3, TResult>(this T1 arg1, Func<T1, T2, T3, TResult> func, T2 arg2, T3 arg3)
        {
            func.NotNull(nameof(func));

            return func(arg1, arg2, arg3);
        }

        public static TResult Forward<T1, T2, T3, T4, TResult>(this T1 arg1, Func<T1, T2, T3, T4, TResult> func, T2 arg2, T3 arg3, T4 arg4)
        {
            func.NotNull(nameof(func));

            return func(arg1, arg2, arg3, arg4);
        }

        internal static void Forward()
        {
            Enumerable.Range(0, 5)
                .Forward(Enumerable.Where, new Func<int, bool>(value => value > 0))
                .Forward(Enumerable.OrderBy, new Func<int, int>(value => value))
                .Forward(Enumerable.Select, new Func<int, double>(value => Math.Sqrt(value)))
                .Forward(EnumerableEx.ForEach, new Action<double>(value => Trace.WriteLine(value)));
        }
    }
}