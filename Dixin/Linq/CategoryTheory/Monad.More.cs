namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    // [Pure]
    public static partial class LazyExtensions
    {
        // Required by LINQ.
        public static Lazy<TResult> SelectMany<TSource, TSelector, TResult>
            (this Lazy<TSource> source,
                Func<TSource, Lazy<TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector) =>
                new Lazy<TResult>(() => resultSelector(source.Value, selector(source.Value).Value));

        // Not required, just for convenience.
        public static Lazy<TResult> SelectMany<TSource, TResult>
            (this Lazy<TSource> source, Func<TSource, Lazy<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class LazyExtensions
    {
        // μ: Lazy<Defer<T> => Defer<T>
        public static Lazy<TResult> Flatten<TResult>
            (this Lazy<Lazy<TResult>> source) => source.SelectMany(Functions.Id);

        // η: T -> Lazy<T> is already implemented previously as LazyExtensions.Defer.

        // φ: Lazy<Defer<T1>, Defer<T2>> => Defer<Defer<T1, T2>>
        public static Lazy<Lazy<T1, T2>> Binary2<T1, T2>
            (this Lazy<Lazy<T1>, Lazy<T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Lazy<TUnit> is already implemented previously with η: T -> Defer<T>.

        // Select: (TSource -> TResult) -> (Lazy<TSource> -> Defer<TResult>)
        public static Lazy<TResult> Select2<TSource, TResult>
            (this Lazy<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Lazy());
    }

    // [Pure]
    public static partial class FuncExtensions
    {
        // Required by LINQ.
        public static Func<TResult> SelectMany<TSource, TSelector, TResult>
            (this Func<TSource> source,
             Func<TSource, Func<TSelector>> selector,
             Func<TSource, TSelector, TResult> resultSelector) =>
                () =>
                    {
                        TSource sourceValue = source();
                        return resultSelector(sourceValue, selector(sourceValue)());
                    };

        // Not required, just for convenience.
        public static Func<TResult> SelectMany<TSource, TResult>
            (this Func<TSource> source, Func<TSource, Func<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class FuncExtensions
    {
        // μ: Func<Func<T> => Func<T>
        public static Func<TResult> Flatten<TResult>
            (this Func<Func<TResult>> source) => source.SelectMany(Functions.Id);

        // η: T -> Func<T> is already implemented previously as FuncExtensions.Func.

        // φ: Lazy<Func<T1>, Func<T2>> => Func<Defer<T1, T2>>
        public static Func<Lazy<T1, T2>> Binary2<T1, T2>
            (this Lazy<Func<T1>, Func<T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Func<TUnit> is already implemented previously with η: T -> Func<T>.

        // Select: (TSource -> TResult) -> (Func<TSource> -> Func<TResult>)
        public static Func<TResult> Select2<TSource, TResult>
            (this Func<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Func());
    }

    // [Pure]
    public static partial class NullableExtensions
    {
        // Required by LINQ.
        public static Nullable<TResult> SelectMany<TSource, TSelector, TResult>
            (this Nullable<TSource> source,
                Func<TSource, Nullable<TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector) =>
                new Nullable<TResult>(() =>
                    {
                        if (source.HasValue)
                        {
                            Nullable<TSelector> selectorResult = selector(source.Value);
                            if (selectorResult.HasValue)
                            {
                                return Tuple.Create(true, resultSelector(source.Value, selectorResult.Value));
                            }
                        }

                        return Tuple.Create(false, default(TResult));
                    });

        // Not required, just for convenience.
        public static Nullable<TResult> SelectMany<TSource, TResult>
            (this Nullable<TSource> source, Func<TSource, Nullable<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class NullableExtensions
    {
        // μ: Nullable<Nullable<T> => Nullable<T>
        public static Nullable<TResult> Flatten<TResult>
            (this Nullable<Nullable<TResult>> source) => source.SelectMany(Functions.Id);

        // η: T -> Nullable<T> is already implemented previously as NullableExtensions.Nullable.

        // φ: Lazy<Nullable<T1>, Nullable<T2>> => Nullable<Defer<T1, T2>>
        public static Nullable<Lazy<T1, T2>> Binary2<T1, T2>
            (this Lazy<Nullable<T1>, Nullable<T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Nullable<TUnit> is already implemented previously with η: T -> Nullable<T>.

        // Select: (TSource -> TResult) -> (Nullable<TSource> -> Nullable<TResult>)
        public static Nullable<TResult> Select2<TSource, TResult>
            (this Nullable<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Nullable());
    }

    public static partial class Func2Extensions
    {
        public static Func<TSourceArg, Func<TSelectorArg, TResult>> SelectMany<TSourceArg, TSource, TSelectorArg, TSelector, TResult>
            (this Func<TSourceArg, TSource> source,
             Func<TSource, Func<TSelectorArg, TSelector>> selector,
             Func<TSource, TSelector, TResult> resultSelector) =>
                sourceArg => selectorArg =>
                    {
                        TSource sourceResult = source(sourceArg);
                        return resultSelector(sourceResult, selector(sourceResult)(selectorArg));
                    };
    }

    // Impure.
    internal static class FuncQuery
    {
        internal static async void Download()
        {
            Func<Task<Unit>> io1 =
                from url in new Func<string>(Console.ReadLine)
                from task in new WebClient().DownloadStringTaskAsync(new Uri(url)).Func()
                from unit in new Func<Task<Unit>>(async () => { Console.WriteLine(await task); return null; })
                select unit;
            await io1();
        }
    }
}
