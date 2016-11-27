namespace Dixin.Linq.CategoryTheory
{
    using System;

    using Microsoft.FSharp.Core;

    // Cps<T, TContinuation> is alias of Func<Func<T, TContinuation>, TContinuation>
    public delegate TContinuation Cps<TContinuation, out T>(Func<T, TContinuation> continuation);

    public static partial class Cps
    {
        // Sum = (x, y) => x + y
        internal static int Sum(int x, int y) => x + y;
    }

    public static partial class Cps
    {
        // SumWithCallback = (x, y, callback) => callback(x + y)
        internal static TCallback SumWithCallback<TCallback>(int x, int y, Func<int, TCallback> callback) =>
            callback(x + y);

        // SumWithCallback = (x, y) => callback => callback(x + y)
        internal static Func<Func<int, TCallback>, TCallback> SumWithCallback<TCallback>(int x, int y) =>
            callback => callback(x + y);

        // SumCps = (x, y) => continuation => continuation(x + y)
        internal static Cps<TContinuation, int> SumCps<TContinuation>(int x, int y) =>
            continuation => continuation(x + y);

        // SquareCps = x => continuation => continuation(x * x)
        internal static Cps<TContinuation, int> SquareCps<TContinuation>(int x) =>
            continuation => continuation(x * x);

        // SumOfSquaresCps = (x, y) => continuation => SquareCps(x)(xx => SquareCps(y)(yy => SumCps(xx)(yy)(continuation)));
        internal static Cps<TContinuation, int> SumOfSquaresCps<TContinuation>(int x, int y) =>
            continuation =>
                SquareCps<TContinuation>(x)(xx =>
                    SquareCps<TContinuation>(y)(yy =>
                        SumCps<TContinuation>(xx, yy)(continuation)));
    }

    public static partial class CpsExtensions
    {
        public static Cps<TContinuation, TResult> SelectMany<TContinuation, TSource, TSelector, TResult>(
            this Cps<TContinuation, TSource> source,
            Func<TSource, Cps<TContinuation, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                continuation => source(value =>
                    selector(value)(result =>
                        continuation(resultSelector(value, result))));

        // Wrap: T -> Cps<T, TContinuation>
        public static Cps<TContinuation, T> Cps<TContinuation, T>(
            this T arg) => continuation => continuation(arg);
    }

    public static partial class CpsExtensions
    {
        // φ: Lazy<Cps<TContinuation, T1>, Cps<TContinuation, T2>> => Cps<TContinuation, Lazy<T1, T2>>
        public static Cps<TContinuation, Lazy<T1, T2>> Binary<TContinuation, T1, T2>(
            this Lazy<Cps<TContinuation, T1>, Cps<TContinuation, T2>> bifunctor) =>
                bifunctor.Value1.SelectMany(
                    value1 => bifunctor.Value2,
                    (value1, value2) => value1.Lazy(value2));

        // ι: TUnit -> Cps<TContinuation, TUnit>
        public static Cps<TContinuation, Unit> Unit<TContinuation>(Unit unit) => unit.Cps<TContinuation, Unit>();
    }

    public static partial class Cps
    {
        internal static void SumOfSquare(int x, int y)
        {
            Cps<string, int> query = from xx in SquareCps<string>(x)
                                     from yy in SquareCps<string>(y)
                                     from sum in SumCps<string>(xx, yy)
                                     select sum;
            string result = query(continuation: int32 => int32.ToString());
        }

        internal static Cps<TContinuation, int> FibonacciCps<TContinuation>(int int32) =>
            int32 > 1
                ? (from a in FibonacciCps<TContinuation>(int32 - 1)
                   from b in FibonacciCps<TContinuation>(int32 - 2)
                   select a + b)
                : 0.Cps<TContinuation, int>();
        // continuation => int32 > 1
        //    ? continuation(FibonacciCps<int>(int32 - 1)(Id) + FibonacciCps<int>(int32 - 2)(Id))
        //    : continuation(0);
    }

    public static partial class CpsExtensions
    {
        // Select: (TSource -> TResult) -> (Cps<TContinuation, TSource> -> Cps<TContinuation, TResult>)
        public static Cps<TContinuation, TResult> Select<TContinuation, TSource, TResult>(
            this Cps<TContinuation, TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Cps<TContinuation, TResult>(), Functions.False);
        // Equivalent to:
        // continuation => source(value => continuation(selector(value)));
        // Or:
        // continuation => source(continuation.o(selector));

        public static Func<T, TContinuation> NoCps<T, TContinuation>(
            this Func<T, Cps<TContinuation, TContinuation>> cps) =>
                value => cps(value)(Functions.Id);

        public static T Invoke<T>(this Cps<T, T> cps) => cps(Functions.Id);
    }
}
