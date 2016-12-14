namespace Dixin.Linq.CategoryTheory
{
    using System;

    // Cps: (T -> TContinuation>) -> TContinuation
    public delegate TContinuation Cps<TContinuation, out T>(Func<T, TContinuation> continuation);

    public static partial class Cps
    {
        // Sum: (int, int) -> int
        // Sum = (x, y) => x + y
        internal static int Sum(int x, int y) => x + y;
    }

    public static partial class Cps
    {
        // SumWithCallback: (int, int, int -> TContinuation) -> TContinuation
        // SumWithCallback = (x, y, callback) => callback(x + y)
        internal static TContinuation SumWithCallback<TContinuation>(int x, int y, Func<int, TContinuation> callback) =>
            callback(x + y);

        // SumWithCallback: (int, int) -> (int -> TContinuation) -> TContinuation
        // SumWithCallback = (x, y) => callback => callback(x + y)
        internal static Func<Func<int, TContinuation>, TContinuation> SumWithCallback<TContinuation>(int x, int y) =>
            callback => callback(x + y);

        // SumCps: (int, int) -> Cps<TContinuation, int>
        // SumCps = (x, y) => continuation => continuation(x + y)
        internal static Cps<TContinuation, int> SumCps<TContinuation>(int x, int y) =>
            continuation => continuation(x + y);

        // SquareCps: int -> Cps<TContinuation, int>
        // SquareCps = x => continuation => continuation(x * x)
        internal static Cps<TContinuation, int> SquareCps<TContinuation>(int x) =>
            continuation => continuation(x * x);

        // SumOfSquaresCps: (int, int) -> Cps<TContinuation, int>
        // SumOfSquaresCps = (x, y) => continuation => SquareCps(x)(xx => SquareCps(y)(yy => SumCps(xx)(yy)(continuation)));
        internal static Cps<TContinuation, int> SumOfSquaresCps<TContinuation>(int x, int y) =>
            continuation =>
                SquareCps<TContinuation>(x)(xx =>
                    SquareCps<TContinuation>(y)(yy =>
                        SumCps<TContinuation>(xx, yy)(continuation)));
    }

    public static partial class CpsExtensions
    {
        // SelectMany: (Cps<TContinuation, TSource>, TSource -> Cps<TContinuation, TSelector>, (TSource, TSelector) -> TResult) -> Cps<TContinuation, TResult>
        public static Cps<TContinuation, TResult> SelectMany<TContinuation, TSource, TSelector, TResult>(
            this Cps<TContinuation, TSource> source,
            Func<TSource, Cps<TContinuation, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                continuation => source(value =>
                    selector(value)(result =>
                        continuation(resultSelector(value, result))));

        // Wrap: TSource -> Cps<TContinuation, TSource>
        public static Cps<TContinuation, TSource> Cps<TContinuation, TSource>(this TSource value) => 
            continuation => continuation(value);

        // Select: (Cps<TContinuation, TSource>, TSource -> TResult) -> Cps<TContinuation, TResult>
        public static Cps<TContinuation, TResult> Select<TContinuation, TSource, TResult>(
            this Cps<TContinuation, TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Cps<TContinuation, TResult>(), (value, result) => result);
                // Equivalent to:
                // continuation => source(value => continuation(selector(value)));
                // Or:
                // continuation => source(continuation.o(selector));
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
        public static Func<T, TContinuation> NoCps<T, TContinuation>(
            this Func<T, Cps<TContinuation, TContinuation>> cps) =>
                value => cps(value)(Functions.Id);

        public static T Invoke<T>(this Cps<T, T> cps) => cps(Functions.Id);
    }
}
