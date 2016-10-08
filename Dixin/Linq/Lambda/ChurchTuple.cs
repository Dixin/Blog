namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

#if DEMO
    // Tuple = f => f(item1)(item1)
    // Tuple is the alias of Func<Func<T1, Func<T2, object>>, object>.
    public delegate object Tuple<out T1, out T2>(Func<T1, Func<T2, object>> f);
#endif

    // Either is the alias of Func<T1, Func<T2, object>>.
    public delegate Func<T2, object> Either<in T1, in T2>(T1 value1);

    public delegate object Tuple<out T1, out T2>(Either<T1, T2> f);

    public static partial class ChurchTuple<T1, T2>
    {
        // CreateTuple = item1 => item2 => f => f(item1)(item2)
        public static readonly Func<T1, Func<T2, Tuple<T1, T2>>> 
            Create = item1 => item2 => f => f(item1)(item2);
    }

    public static partial class ChurchTuple
    {
        // Item1 = tuple => tuple(x => y => x)
        public static T1 Item1<T1, T2>(this Tuple<T1, T2> tuple) => (T1)tuple(x => y => x); // True = x => y => x

        // Item2 = tuple => tuple(x => y => y)
        public static T2 Item2<T1, T2>(this Tuple<T1, T2> tuple) => (T2)tuple(x => y => y); // False = x => y => y
    }

    public static partial class ChurchTuple<T1, T2>
    {
        public static readonly Func<Tuple<T1, T2>, Boolean> 
            Null = tuple => (Boolean)tuple(x => y => False);
    }

    public static partial class ChurchTuple
    {
        // (x, y) -> (y, f(y))
        // Shift = tuple => f => Create(tuple.Item2())(f(tuple.Item1()))
        public static Tuple<T, T> Shift<T>(this Tuple<T, T> tuple, Func<T, T> f) => 
            ChurchTuple<T, T>.Create(tuple.Item2())(f(tuple.Item2()));

        // Swap = tuple => Create(tuple.Item2())(tuple.Item1())
        public static Tuple<T2, T1> Swap<T1, T2>(this Tuple<T1, T2> tuple) => 
            ChurchTuple<T2, T1>.Create(tuple.Item2())(tuple.Item1());
    }
}