namespace Dixin.Linq.Combinators
{
    using System;

    public static class BckwCombinators
    {
        // B = x => => z => x(y(z))
        public static Func<Func<T1, T2>, Func<T1, TResult>> B<T1, T2, TResult>
            (Func<T2, TResult> x) => y => z => x(y(z));

        // C = f => x => y => f(y)(z)
        public static Func<T2, Func<T1, TResult>> C<T1, T2, TResult>
            (Func<T1, Func<T2, TResult>> x) => y => z => x(z)(y);

        // K = x => _ => x
        public static Func<T2, T1> K<T1, T2>
            (T1 x) => _ => x;

        // W = x => y => x(y)(y)
        public static Func<T, TResult> W<T, TResult>
            (Func<T, Func<T, TResult>> x) => y => x(y)(y);
    }
}