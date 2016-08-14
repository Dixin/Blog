namespace Dixin.Linq
{
    using System;

    using Dixin.Common;

    public static partial class ActionExtensions
    {
        public static void Forward<T>(this T arg, Action<T> action)
        {
            action.NotNull(nameof(action));

            action(arg);
        }

        public static void Forward<T1, T2>(this T1 arg1, Action<T1, T2> action, T2 arg2)
        {
            action.NotNull(nameof(action));

            action(arg1, arg2);
        }

        public static void Forward<T1, T2, T3>(this T1 arg1, Action<T1, T2, T3> action, T2 arg2, T3 arg3)
        {
            action.NotNull(nameof(action));

            action(arg1, arg2, arg3);
        }

        public static void Forward<T1, T2, T3, T4>(this T1 arg1, Action<T1, T2, T3, T4> action, T2 arg2, T3 arg3, T4 arg4)
        {
            action.NotNull(nameof(action));

            action(arg1, arg2, arg3, arg4);
        }
    }
}