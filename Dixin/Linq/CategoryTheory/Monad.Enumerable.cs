namespace Dixin.Linq.CategoryTheory.Obsolete2
{
    using System.Collections.Generic;

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<IEnumerable<TSource>> source)
        {
            foreach (IEnumerable<TSource> enumerable in source)
            {
                foreach (TSource value in enumerable)
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<T> Enumerable<T>(this T value)
        {
            yield return value;
        }
    }
}

namespace Dixin.Linq.CategoryTheory.Obsolete3
{
    using System;
    using System.Collections.Generic;
    using Microsoft.FSharp.Core;

    // [Pure]
    public static partial class EnumerableExtensions
    {
        // η: Lazy<T> => IEnumerable<T>
        // or
        // η: T -> IEnumerable<T>
        public static IEnumerable<T> Enumerable<T>(this T value)
        {
            yield return value;
        }

        // μ: IEnumerable<> ◎ IEnumerable<> => IEnumerable<>
        // or 
        // μ: IEnumerable<IEnumerable<T>> => IEnumerable<T>
        public static IEnumerable<TSource> Flatten<TSource>
            (this IEnumerable<IEnumerable<TSource>> source) => source.SelectMany(Functions.Id);
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        // φ: Lazy<IEnumerable<T1>, IEnumerable<T2>> => IEnumerable<Defer<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<IEnumerable<T1>, IEnumerable<T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit
            (Unit unit) => unit.Enumerable();
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        // Select: (TSource -> TResult) -> (TDotNetMonad<TSource> -> TDotNetMonad<TResult>)
        public static IEnumerable<TResult> Select<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(sourceValue => selector(sourceValue).Enumerable(), Functions.False);
    }
}

namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<IEnumerable<TSource>> source)
        {
            foreach (IEnumerable<TSource> enumerable in source)
            {
                foreach (TSource value in enumerable)
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TSelector, TResult>
            (this IEnumerable<TSource> source,
                Func<TSource, IEnumerable<TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector) =>
                    // (from sourceValue in source
                    //     select (from selectorValue in selector(sourceValue)
                    //         select resultSelector(sourceValue, selectorValue))).Flatten();
                    source.Select(sourceValue => selector(sourceValue)
                            .Select(selectorValue => resultSelector(sourceValue, selectorValue)))
                        .Flatten();
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectMany2<TSource, TSelector, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector)
        {
            foreach (TSource sourceValue in source)
            {
                foreach (TSelector selectorValue in selector(sourceValue))
                {
                    yield return resultSelector(sourceValue, selectorValue);
                }
            }
        }

        public static IEnumerable<TSource> Flatten2<TSource>
            (this IEnumerable<IEnumerable<TSource>> source) =>
                // source.SelectMany(enumerable => enumerable);
                source.SelectMany2(Functions.Id);
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectMany<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
                source.SelectMany(selector, (sourceValue, selectorValue) => selectorValue);

        public static IEnumerable<TResult> SelectMany2<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> Select3<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                from sourceValue in source
                from selectorValue in selector(sourceValue).Enumerable()
                select selectorValue;

        public static IEnumerable<TSource> Flatten3<TSource>
            (this IEnumerable<IEnumerable<TSource>> source) =>
                from enumerable in source
                from value in enumerable
                select value;
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> Select4<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(
                    sourceValue => selector(sourceValue).Enumerable(),
                    (sourceValue, selectorValue) => selectorValue);

        public static IEnumerable<TSource> Flatten4<TSource>
            (this IEnumerable<IEnumerable<TSource>> source) =>
                source.SelectMany(enumerable => enumerable);
    }

    public class Enumerable<T> : IEnumerable<T>
    {
        private readonly T value;

        public Enumerable(T value)
        {
            this.value = value;
        }

        [Pure]
        public IEnumerator<T> GetEnumerator()
        {
            yield return this.value;
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator
            () => this.GetEnumerator();
    }

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> Select2<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector) => source.SelectMany(
                sourceValue => selector(sourceValue).Enumerable(),
                Functions.False);
    }
}
