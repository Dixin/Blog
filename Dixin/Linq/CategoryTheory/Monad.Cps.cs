namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;

    using Microsoft.FSharp.Core;

    // Cps<T, TContinuation> is alias of Func<Func<T, TContinuation>, TContinuation>
    public delegate TContinuation Cps<out T, TContinuation>(Func<T, TContinuation> continuation);

    // [Pure]
    public static partial class Cps
    {
        // Add = (x, y) => x + y
        public static int Add
            (int x, int y) => x + y;
    }

    // [Pure]
    public static partial class Cps
    {
        // AddWithCallback = (x, y, callback) => callback(x + y)
        public static TCallback AddWithCallback<TCallback>
            (int x, int y, Func<int, TCallback> callback) => callback(x + y);

        // AddWithCallback = (x, y) => callback => callback(x + y)
        public static Func<Func<int, TCallback>, TCallback> AddWithCallback<TCallback>
            (int x, int y) => callback => callback(x + y);

        // AddCps = (x, y) => continuation => continuation(x + y)
        public static Cps<int, TContinuation> AddCps<TContinuation>
            (int x, int y) => continuation => continuation(x + y);

        // SquareCps = x => continuation => continuation(x * x)
        public static Cps<int, TContinuation> SquareCps<TContinuation>
            (int x) => continuation => continuation(x * x);

        // SumOfSquaresCps = (x, y) => continuation => SquareCps(x)(xx => SquareCps(y)(yy => AddCps(xx)(yy)(continuation)));
        public static Cps<int, TContinuation> SumOfSquaresCps<TContinuation>
            (int x, int y) => continuation =>
                SquareCps<TContinuation>(x)(xx =>
                    SquareCps<TContinuation>(y)(yy =>
                        AddCps<TContinuation>(xx, yy)(continuation)));
    }

    [Pure]
    public static partial class CpsExtensions
    {
        // Required by LINQ.
        public static Cps<TResult, TContinuation> SelectMany<TSource, TSelector, TResult, TContinuation>
            (this Cps<TSource, TContinuation> source,
                Func<TSource, Cps<TSelector, TContinuation>> selector,
                Func<TSource, TSelector, TResult> resultSelector) =>
                continuation => source(sourceArg =>
                    selector(sourceArg)(selectorArg =>
                        continuation(resultSelector(sourceArg, selectorArg))));

        // Not required, just for convenience.
        public static Cps<TResult, TContinuation> SelectMany<TSource, TResult, TContinuation>
            (this Cps<TSource, TContinuation> source, Func<TSource, Cps<TResult, TContinuation>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class CpsExtensions
    {
        // η: T -> Cps<T, TContinuation>
        public static Cps<T, TContinuation> Cps<T, TContinuation>
            (this T arg) => continuation => continuation(arg);

        // φ: Lazy<Cps<T1, TContinuation>, Cps<T2, TContinuation>> => Cps<Defer<T1, T2>, TContinuation>
        public static Cps<Lazy<T1, T2>, TContinuation> Binary<T1, T2, TContinuation>
            (this Lazy<Cps<T1, TContinuation>, Cps<T2, TContinuation>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Cps<TUnit, TContinuation>
        public static Cps<Unit, TContinuation> Unit<TContinuation>
            (Unit unit) => unit.Cps<Unit, TContinuation>();

        // Select: (TSource -> TResult) -> (Cps<TSource, TContinuation> -> Cps<TResult, TContinuation>)
        public static Cps<TResult, TContinuation> Select<TSource, TResult, TContinuation>
            (this Cps<TSource, TContinuation> source, Func<TSource, TResult> selector) =>
                // continuation => source(sourceArg => continuation(selector(sourceArg)));
                // continuation => source(continuation.o(selector));
                source.SelectMany(value => selector(value).Cps<TResult, TContinuation>());
    }

    // [Pure]
    public static partial class CpsExtensions
    {
        public static Func<T, TContinuation> NoCps<T, TContinuation>
            (this Func<T, Cps<TContinuation, TContinuation>> cps) => arg => cps(arg)(Functions.Id);

        public static T Invoke<T>
            (this Cps<T, T> cps) => cps(Functions.Id);
    }

    [Pure]
    public static partial class CpsQuery
    {
        public static void SumOfSqaure()
        {
            Func<int, Func<int, int>> add = x => y => x + y;
            Func<int, int> sqaure = x => x * x;
            Func<int, Func<int, int>> sumOfSquares = x => y => add(sqaure(x))(sqaure(y));
        }
    }
}
