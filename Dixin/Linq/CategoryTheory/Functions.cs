namespace Dixin.Linq.CategoryTheory
{
    using System;

    public static partial class Functions
    {
        public static TSource Id<TSource>(TSource value) => value;
    }

    public static partial class Functions<TSource, TMiddle, TResult>
    {
        public static readonly Func<Func<TMiddle, TResult>, Func<Func<TSource, TMiddle>, Func<TSource, TResult>>>
            o = function2 => function1 => value => function2(function1(value));
    }

    public static partial class Functions
    {
        public static TFalse False<TTrue, TFalse>(TTrue @true, TFalse @false) => @false;
    }
}
