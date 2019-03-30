namespace Tutorial.CategoryTheory
{
    using System;
    using System.IO;
    using System.Text;

    using Microsoft.FSharp.Core;

    // Cps: (T -> TContinuation>) -> TContinuation
    public delegate TContinuation Cps<TContinuation, out T>(Func<T, TContinuation> continuation);

    public static partial class CpsExtensions
    {
        // Sqrt: int -> double
        internal static double Sqrt(int int32) => Math.Sqrt(int32);

        // SqrtWithCallback: (int, double -> TContinuation) -> TContinuation
        internal static TContinuation SqrtWithCallback<TContinuation>(
            int int32, Func<double, TContinuation> continuation) =>
                continuation(Math.Sqrt(int32));
    }

    public static partial class CpsExtensions
    {
        // SqrtWithCallback: int -> (double -> TContinuation) -> TContinuation
        internal static Func<Func<double, TContinuation>, TContinuation> SqrtWithCallback<TContinuation>(int int32) =>
            continuation => continuation(Math.Sqrt(int32));

        // SqrtCps: int -> Cps<TContinuation, double>
        internal static Cps<TContinuation, double> SqrtCps<TContinuation>(int int32) =>
            continuation => continuation(Math.Sqrt(int32));
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

    public static partial class CpsExtensions
    {
        // SquareCps: int -> Cps<TContinuation, int>
        internal static Cps<TContinuation, int> SquareCps<TContinuation>(int x) =>
            continuation => continuation(x * x);

        // SumCps: (int, int) -> Cps<TContinuation, int>
        internal static Cps<TContinuation, int> SumCps<TContinuation>(int a, int b) =>
            continuation => continuation(a + b);

#if DEMO
        // SumOfSquaresCps: (int, int) -> Cps<TContinuation, int>
        internal static Cps<TContinuation, int> SumOfSquaresCps<TContinuation>(int a, int b) =>
            continuation =>
                SquareCps<TContinuation>(a)(squareOfA =>
                SquareCps<TContinuation>(b)(squareOfB =>
                SumCps<TContinuation>(squareOfA, squareOfB)(continuation)));
#endif

        internal static Cps<TContinuation, int> SumOfSquaresCps<TContinuation>(int a, int b) =>
            from squareOfA in SquareCps<TContinuation>(a) // Cps<TContinuation, int>.
            from squareOfB in SquareCps<TContinuation>(b) // Cps<TContinuation, int>.
            from sum in SumCps<TContinuation>(squareOfA, squareOfB) // Cps<TContinuation, int>.
            select sum;

        internal static Cps<TContinuation, uint> FactorialCps<TContinuation>(uint uInt32) =>
            uInt32 > 0
                ? (from nextProduct in FactorialCps<TContinuation>(uInt32 - 1U)
                   select uInt32 * nextProduct)
                : 1U.Cps<TContinuation, uint>();

        internal static Cps<TContinuation, uint> FibonacciCps<TContinuation>(uint uInt32) =>
            uInt32 > 1
                ? (from a in FibonacciCps<TContinuation>(uInt32 - 1U)
                   from b in FibonacciCps<TContinuation>(uInt32 - 2U)
                   select a + b)
                : uInt32.Cps<TContinuation, uint>();
            // Equivalent to:
            // continuation => uInt32 > 1U
            //    ? continuation(FibonacciCps<int>(uInt32 - 1U)(Id) + FibonacciCps<int>(uInt32 - 2U)(Id))
            //    : continuation(uInt32);

        public static Cps<TContinuation, T> Cps<TContinuation, T>(Func<T> function) =>
            continuation => continuation(function());

        public static Cps<TContinuation, Unit> Cps<TContinuation>(Action action) =>
            continuation =>
            {
                action();
                return continuation(default);
            };

        internal static void Workflow<TContinuation>(Func<string, TContinuation> continuation)
        {
            Cps<TContinuation, string> query =
                from filePath in Cps<TContinuation, string>(Console.ReadLine) // Cps<TContinuation, string>.
                from encodingName in Cps<TContinuation, string>(Console.ReadLine) // Cps<TContinuation, string>.
                from encoding in Cps<TContinuation, Encoding>(() => Encoding.GetEncoding(encodingName)) // Cps<TContinuation, Encoding>.
                from fileContent in Cps<TContinuation, string>(() => File.ReadAllText(filePath, encoding)) // Cps<TContinuation, string>.
                select fileContent; // Define query.
            TContinuation result = query(continuation); // Execute query.
        }
    }

    public static partial class CpsExtensions
    {
        public static Func<T, TContinuation> NoCps<T, TContinuation>(
            this Func<T, Cps<TContinuation, TContinuation>> cps) =>
                value => cps(value)(Functions.Id);

        public static T Invoke<T>(this Cps<T, T> cps) => cps(Functions.Id);
    }
}
