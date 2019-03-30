namespace Tutorial.CategoryTheory
{
    public static partial class Functions
    {
#if DEMO
        public static Func<TSource, TResult> o<TSource, TMiddle, TResult>(
            this Func<TMiddle, TResult> function2, Func<TSource, TMiddle> function1) =>
                value => function2(function1(value));
#endif

        public static TSource Id<TSource>(TSource value) => value;
    }

    public static partial class Functions
    {
        public static TFalse False<TTrue, TFalse>(TTrue @true, TFalse @false) => @false;
    }
}
