namespace Tutorial
{
    using System;

    public static partial class FuncExtensions
    {
        public static TResult Forward<T, TResult>(this T value, Func<T, TResult> function) =>
            function(value);
    }

    public static partial class ActionExtensions
    {
        public static void Forward<T>(this T value, Action<T> function) =>
            function(value);
    }
}