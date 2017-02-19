namespace Tutorial
{
    using System;

    public static partial class FuncExtensions
    {
        public static TResult Forward<T, TResult>(
            this T value, Func<T, TResult> function) => 
                function(value);

        public static TResult Forward<T1, T2, TResult>(
            this T1 value1, Func<T1, T2, TResult> function, T2 value2) => 
                function(value1, value2);

        public static TResult Forward<T1, T2, T3, TResult>(
            this T1 value1, Func<T1, T2, T3, TResult> function, T2 value2, T3 value3) => 
                function(value1, value2, value3);

        public static TResult Forward<T1, T2, T3, T4, TResult>(
            this T1 value1, Func<T1, T2, T3, T4, TResult> function, T2 value2, T3 value3, T4 value4) => 
                function(value1, value2, value3, value4);

        // ...
    }

    public static partial class ActionExtensions
    {
        public static void Forward<T>(
            this T value, Action<T> function) => 
                function(value);

        public static void Forward<T1, T2>(
            this T1 value1, Action<T1, T2> function, T2 value2) => 
                function(value1, value2);

        public static void Forward<T1, T2, T3>(
            this T1 value1, Action<T1, T2, T3> function, T2 value2, T3 value3) => 
                function(value1, value2, value3);

        public static void Forward<T1, T2, T3, T4>(
            this T1 value1, Action<T1, T2, T3, T4> function, T2 value2, T3 value3, T4 value4) => 
                function(value1, value2, value3, value4);

        // ...
    }
}