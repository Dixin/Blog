namespace Tutorial.LambdaCalculus
{
    using System;
    using System.Linq;

    internal static class Expressions
    {
#if DEMO
        internal static Func<T, T> Variable<T>(Func<T, Func<Func<T, T>, T>> g, Func<T, T> h) => 
            x => g(x)(x => h(x));
#endif

        internal static Func<T, T> Variable<T>(Func<T, Func<Func<T, T>, T>> g, Func<T, T> h) =>
            x => g(x)(y => h(y));

        internal static void LinqQuery()
        {
            Func<int, bool> isEven = value => value % 2 == 0;
            Enumerable.Range(0, 5).Where(value => isEven(value)).ForEach(value => Console.WriteLine(value));
        }

        internal static void EtaConvertion()
        {
            Func<int, bool> isEven = x => x % 2 == 0;
            Enumerable.Range(0, 5).Where(isEven).ForEach(Console.WriteLine);
        }

        internal static void Compose()
        {
            Func<double, double> sqrt = Math.Sqrt;
            Func<double, double> abs = Math.Abs;

            Func<double, double> absSqrt1 = sqrt.o(abs); // Composition: sqrt after abs.
            absSqrt1(-2D).WriteLine(); // 1.4142135623731
        }

        internal static void Associativity()
        {
            Func<double, double> sqrt = Math.Sqrt;
            Func<double, double> abs = Math.Abs;
            Func<double, double> log = Math.Log;

            Func<double, double> absSqrtLog1 = log.o(sqrt).o(abs); // Composition: (log o sqrt) o abs.
            absSqrtLog1(-2D).WriteLine(); // 0.34642256747438094
            Func<double, double> absSqrtLog2 = log.o(sqrt.o(abs)); // Composition: log o (sqrt o abs).
            absSqrtLog2(-2D).WriteLine(); // 0.34642256747438094
        }
    }
}
