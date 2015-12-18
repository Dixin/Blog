namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [Pure]
    public static partial class NaturalTransformations
    {
        // Lazy<> => IEnumerable<>
        public static IEnumerable<T> ToEnumerable<T>(this Lazy<T> lazy)
        {
            yield return lazy.Value;
        }

        // Func<> => IEnumerable<>
        public static IEnumerable<T> ToEnumerable<T>(this Func<T> function)
        {
            yield return function();
        }

        // Nullable<> => IEnumerable<>
        public static IEnumerable<T> ToEnumerable<T>(this Nullable<T> nullable)
        {
            if (nullable.HasValue)
            {
                yield return nullable.Value;
            }
        }
    }

    // [Pure]
    public static partial class NaturalTransformations
    {
        // Lazy<> => Func<>
        public static Func<T> ToFunc<T>
            (this Lazy<T> lazy) => () => lazy.Value;

        // Func<> => Nullable<>
        public static Nullable<T> ToNullable<T>
            (this Func<T> function) => new Nullable<T>(() => Tuple.Create(true, function()));
    }

    // [Pure]
    public static partial class NaturalTransformations
    {
        // Lazy<> => Nullable<>
        public static Nullable<T> ToNullable<T>
            (this Lazy<T> lazy) =>
                // new Func<Func<T>, Nullable<T>>(ToNullable).o(new Func<Lazy<T>, Func<T>>(ToFunc))(lazy);
                lazy.ToFunc().ToNullable();
    }

    // [Pure]
    public static partial class NaturalTransformations
    {
        // Func<> => Lazy<>
        public static Lazy<T> ToLazy<T>
            (this Func<T> function) => new Lazy<T>(function);
    }
}
