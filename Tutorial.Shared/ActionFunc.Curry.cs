namespace Tutorial
{
    using System;

    public static partial class FuncExtensions
    {
        // Transform (value1, value2) => result
        // to value1 => value2 => result.
        public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(
            this Func<T1, T2, TResult> function) => 
                value1 => value2 => function(value1, value2);
        
        // Transform (value1, value2, value3) => result
        // to value1 => value2 => value3 => result.
        public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> function) => 
                value1 => value2 => value3 => function(value1, value2, value3);

        // Transform (value1, value2, value3, value4) => result
        // to value1 => value2 => value3 => value4 => result.
        public static Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>> Curry<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> function) => 
                value1 => value2 => value3 => value4 => function(value1, value2, value3, value4);

        // ...
    }

    public static partial class ActionExtensions
    {
        // Transform (value1, value2) => { }
        // to value1 => value2 => { }.
        public static Func<T1, Action<T2>> Curry<T1, T2>(
            this Action<T1, T2> function) =>
                value1 => value2 => function(value1, value2);

        // Transform (value1, value2, value3) => { }
        // to value1 => value2 => value3 => { }.
        public static Func<T1, Func<T2, Action<T3>>> Curry<T1, T2, T3>(
            this Action<T1, T2, T3> function) => value1 => value2 => value3 => function(value1, value2, value3);

        // Transform (value1, value2, value3, value4) => { }
        // to value1 => value2 => value3 => value4 => { }.
        public static Func<T1, Func<T2, Func<T3, Action<T4>>>> Curry<T1, T2, T3, T4>(
            this Action<T1, T2, T3, T4> function) =>
                value1 => value2 => value3 => value4 => function(value1, value2, value3, value4);

        // ...
    }

    public static partial class FuncExtensions
    {
        // Transform value1 => value2 => result
        // to (value1, value2) => result.
        public static Func<T1, T2, TResult> Uncurry<T1, T2, TResult>(
            this Func<T1, Func<T2, TResult>> function) => 
                (value1, value2) => function(value1)(value2);

        // Transform value1 => value2 => value3 => result
        // to (value1, value2, value3) => result.
        public static Func<T1, T2, T3, TResult> Uncurry<T1, T2, T3, TResult>(
            this Func<T1, Func<T2, Func<T3, TResult>>> function) => 
                (value1, value2, value3) => function(value1)(value2)(value3);

        // Transform value1 => value2 => value3 => value4 => result
        // to (value1, value2, value3, value4) => result.
        public static Func<T1, T2, T3, T4, TResult> Uncurry<T1, T2, T3, T4, TResult>(
            this Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>> function) => 
                (value1, value2, value3, value4) => function(value1)(value2)(value3)(value4);

        // ...
    }

    public static partial class ActionExtensions
    {
        // Transform value1 => value2 => { }
        // to (value1, value2) => { }.
        public static Action<T1, T2> Uncurry<T1, T2>(
            this Func<T1, Action<T2>> function) => (value1, value2) =>
                function(value1)(value2);

        // Transform value1 => value2 => value3 => { }
        // to (value1, value2, value3) => { }.
        public static Action<T1, T2, T3> Uncurry<T1, T2, T3>(
            this Func<T1, Func<T2, Action<T3>>> function) =>
                (value1, value2, value3) => function(value1)(value2)(value3);

        // Transform value1 => value2 => value3 => value4 => { }
        // to (value1, value2, value3, value4) => { }.
        public static Action<T1, T2, T3, T4> Uncurry<T1, T2, T3, T4>(
            this Func<T1, Func<T2, Func<T3, Action<T4>>>> function) =>
                (value1, value2, value3, value4) => function(value1)(value2)(value3)(value4);

        // ...
    }

    public static partial class FuncExtensions
    {
        public static Func<T2, TResult> Partial<T1, T2, TResult>(
            this Func<T1, T2, TResult> function, T1 value1) => 
                value2 => function(value1, value2);

        public static Func<T2, Func<T3, TResult>> Partial<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> function, T1 value1) => 
                value2 => value3 => function(value1, value2, value3);

        public static Func<T2, Func<T3, Func<T4, TResult>>> Partial<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> function, T1 value1) => 
                value2 => value3 => value4 => function(value1, value2, value3, value4);

        // ...
    }

    public static partial class ActionExtensions
    {
        public static Action<T2> Partial<T1, T2>(
            this Action<T1, T2> function, T1 value1) =>
                value2 => function(value1, value2);

        public static Func<T2, Action<T3>> Partial<T1, T2, T3>(
            this Action<T1, T2, T3> function, T1 value1) =>
                value2 => value3 => function(value1, value2, value3);

        public static Func<T2, Func<T3, Action<T4>>> Partial<T1, T2, T3, T4>(
            this Action<T1, T2, T3, T4> function, T1 value1) =>
                value2 => value3 => value4 => function(value1, value2, value3, value4);

        // ...
    }

    public static partial class FuncExtensions
    {
        public static Func<TResult> Partial<T1, TResult>(
            this Func<T1, TResult> function, T1 value1) =>
                () => function(value1);
    }
}
