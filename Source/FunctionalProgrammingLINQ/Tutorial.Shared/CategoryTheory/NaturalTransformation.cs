namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;

    public static partial class NaturalTransformations
    {
        // ToLazy: Func<> -> Lazy<>
        public static Lazy<T> ToLazy<T>(this Func<T> function) => new Lazy<T>(function);
    }

    public static partial class NaturalTransformations
    {
        internal static void Naturality()
        {
            Func<int, string> selector = int32 => Math.Sqrt(int32).ToString("0.00");

            // Naturality square:
            // ToFunc<string>.o(LazyExtensions.Select(selector)) == FuncExtensions.Select(selector).o(ToFunc<int>)
            Func<Func<string>, Lazy<string>> funcStringToLazyString = ToLazy<string>;
            Func<Func<int>, Func<string>> funcInt32ToFuncString = FuncExtensions.Select(selector);
            Func<Func<int>, Lazy<string>> leftComposition = funcStringToLazyString.o(funcInt32ToFuncString);
            Func<Lazy<int>, Lazy<string>> lazyInt32ToLazyString = LazyExtensions.Select(selector);
            Func<Func<int>, Lazy<int>> funcInt32ToLazyInt32 = ToLazy<int>;
            Func<Func<int>, Lazy<string>> rightComposition = lazyInt32ToLazyString.o(funcInt32ToLazyInt32);

            Func<int> funcInt32 = () => 2;
            Lazy<string> lazyString = leftComposition(funcInt32);
            lazyString.Value.WriteLine(); // 1.41
            lazyString = rightComposition(funcInt32);
            lazyString.Value.WriteLine(); // 1.41
        }
    }

    public static partial class NaturalTransformations
    {
        // ToFunc: Lazy<T> -> Func<T>
        public static Func<T> ToFunc<T>(this Lazy<T> lazy) => () => lazy.Value;

        // ToEnumerable: Func<T> -> IEnumerable<T>
        public static IEnumerable<T> ToEnumerable<T>(this Func<T> function)
        {
            yield return function();
        }

        // ToEnumerable: Lazy<T> -> IEnumerable<T>
        public static IEnumerable<T> ToEnumerable<T>(this Lazy<T> lazy)
        {
            yield return lazy.Value;
        }
    }

    public static partial class NaturalTransformations
    {
        // ToEnumerable: Optional<> -> IEnumerable<>
        public static IEnumerable<T> ToEnumerable<T>(this Optional<T> optional)
        {
            if (optional.HasValue)
            {
                yield return optional.Value;
            }
        }

#if DEMO
        // ToFunc: Lazy<T> -> Func<T>
        public static Func<T> ToFunc<T>(this Lazy<T> lazy) => () => lazy.Value;
#endif

        // ToOptional: Func<T> -> Optional<T>
        public static Optional<T> ToOptional<T>(this Func<T> function) =>
            new Optional<T>(() => (true, function()));

        // ToOptional: Lazy<T> -> Optional<T>
        public static Optional<T> ToOptional<T>(this Lazy<T> lazy) =>
            // new Func<Func<T>, Optional<T>>(ToOptional).o(new Func<Lazy<T>, Func<T>>(ToFunc))(lazy);
            lazy.ToFunc().ToOptional();
    }
}
