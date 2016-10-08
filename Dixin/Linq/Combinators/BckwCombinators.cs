namespace Dixin.Linq.Combinators
{
    using System;

    public static class BckwCombinators<T1, T2, TResult>
    {
        // B = x => => z => x(y(z))
        public static readonly Func<Func<T2, TResult>, Func<Func<T1, T2>, Func<T1, TResult>>> B =
            x => y => z => x(y(z));

        // C = f => x => y => f(y)(z)
        public static readonly Func<Func<T1, Func<T2, TResult>>, Func<T2, Func<T1, TResult>>> C = 
            x => y => z => x(z)(y);
    }

    public static class BckwCombinators<T1, T2>
    {
        // K = x => _ => x
        public static readonly Func<T1, Func<T2, T1>> K = 
            x => _ => x;

        // W = x => y => x(y)(y)
        public static readonly Func<Func<T1, Func<T1, T2>>, Func<T1, T2>> W =
            x => y => x(y)(y);
    }
}