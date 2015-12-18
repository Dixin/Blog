namespace Dixin.Linq
{
    using System;

    public static partial class FuncExtensions
    {
        // from arg => result
        // to () => arg => result
        public static Func<Func<T, TResult>> Curry<T, TResult>
            (this Func<T, TResult> function) => 
                () => arg => function(arg);

        // from (arg1, arg2) => result
        // to arg1 => arg2 => result
        public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>
            (this Func<T1, T2, TResult> function) => 
                arg1 => arg2 => function(arg1, arg2);

        // from (arg1, arg2, arg3) => result
        // to arg1 => arg2 => arg3 => result
        public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>
            (this Func<T1, T2, T3, TResult> function) => 
                arg1 => arg2 => arg3 => function(arg1, arg2, arg3);

        // from (arg1, arg2, arg3, arg4) => result
        // to arg1 => arg2 => arg3 => arg4 => result
        public static Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>> Curry<T1, T2, T3, T4, TResult>
            (this Func<T1, T2, T3, T4, TResult> function) => 
                arg1 => arg2 => arg3 => arg4 => function(arg1, arg2, arg3, arg4);

        // ...
    }

    public static partial class FuncExtensions
    {
        // from () => arg => result
        // to arg => result
        public static Func<T, TResult> Uncurry<T, TResult>
            (this Func<Func<T, TResult>> function) => 
                arg => function()(arg);

        // from arg1 => arg2 => result
        // to (arg1, arg2) => result
        public static Func<T1, T2, TResult> Uncurry<T1, T2, TResult>
            (this Func<T1, Func<T2, TResult>> function) => 
                (arg1, arg2) => function(arg1)(arg2);

        // from arg1 => arg2 => arg3 => result
        // to (arg1, arg2, arg3) => result
        public static Func<T1, T2, T3, TResult> Uncurry<T1, T2, T3, TResult>
            (this Func<T1, Func<T2, Func<T3, TResult>>> function) => 
                (arg1, arg2, arg3) => function(arg1)(arg2)(arg3);

        // from arg1 => arg2 => arg3 => arg4 => result
        // to (arg1, arg2, arg3, arg4) => result
        public static Func<T1, T2, T3, T4, TResult> Uncurry<T1, T2, T3, T4, TResult>
            (this Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>> function) => 
                (arg1, arg2, arg3, arg4) => function(arg1)(arg2)(arg3)(arg4);

        // ...
    }

    public static partial class FuncExtensions
    {
        public static Func<TResult> Partial<T, TResult>
            (this Func<T, TResult> function, T arg) => 
                () => function(arg);

        public static Func<T2, TResult> Partial<T1, T2, TResult>
            (this Func<T1, T2, TResult> function, T1 arg1) => 
                arg2 => function(arg1, arg2);

        public static Func<T2, Func<T3, TResult>> Partial<T1, T2, T3, TResult>
            (this Func<T1, T2, T3, TResult> function, T1 arg1) => 
                arg2 => arg3 => function(arg1, arg2, arg3);

        public static Func<T2, Func<T3, Func<T4, TResult>>> Partial<T1, T2, T3, T4, TResult>
            (this Func<T1, T2, T3, T4, TResult> function, T1 arg1) => 
            arg2 => arg3 => arg4 => function(arg1, arg2, arg3, arg4);

        // ...
    }
}
