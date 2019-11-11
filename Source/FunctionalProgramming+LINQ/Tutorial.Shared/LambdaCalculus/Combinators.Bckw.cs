namespace Tutorial.LambdaCalculus
{
    using System;

    public static class BckwCombinators<T1, T2, TResult>
    {
        public static readonly Func<Func<T2, TResult>, Func<Func<T1, T2>, Func<T1, TResult>>>
            B = x => y => z => x(y(z));

        public static readonly Func<Func<T1, Func<T2, TResult>>, Func<T2, Func<T1, TResult>>> 
            C = x => y => z => x(z)(y);
    }

    public static class BckwCombinators<T1, T2>
    {
        public static readonly Func<T1, Func<T2, T1>> 
            K = x => y => x;

        public static readonly Func<Func<T1, Func<T1, T2>>, Func<T1, T2>> 
            W = x => y => x(y)(y);
    }
}
