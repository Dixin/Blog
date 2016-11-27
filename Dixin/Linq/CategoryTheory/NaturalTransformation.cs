namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;

    public static partial class NaturalTransformations
    {
        // ToFunc: Lazy<> -> Func<>
        public static Func<T> ToFunc<T>(this Lazy<T> lazy) => () => lazy.Value;

        // ToEnumerable: Func<> -> IEnumerable<>
        public static IEnumerable<T> ToEnumerable<T>(this Func<T> function)
        {
            yield return function();
        }

        // ToEnumerable: Lazy<> -> IEnumerable<>
        public static IEnumerable<T> ToEnumerable<T>(this Lazy<T> lazy)
        {
            yield return lazy.Value;
        }

        // ToLazy: Func<> -> Lazy<>
        public static Lazy<T> ToLazy<T>(this Func<T> function) => new Lazy<T>(function);
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

        // ToOptional: Func<> -> Optional<>
        public static Optional<T> ToOptional<T>(this Func<T> function) => 
            new Optional<T>(() => true.Tuple(function()));

        // ToOptional: Lazy<> -> Optional<>
        public static Optional<T> ToOptional<T>(this Lazy<T> lazy) =>
                // new Func<Func<T>, Optional<T>>(ToOptional).o(new Func<Lazy<T>, Func<T>>(ToFunc))(lazy);
                lazy.ToFunc().ToOptional();
    }
}
