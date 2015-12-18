namespace Dixin.Linq.Lambda.Obsolete
{
    using System;

    // Tuple = f => f(item1)(item1)
    public delegate object Tuple<out T1, out T2>(Func<T1, Func<T2, object>> f);
    // Tuple is an alias of Func<Func<T1, Func<T2, object>>, object>

    public static class ChurchTuple
    {
        // CreateTuple = item1 => item2 => f => f(item1)(item2)
        public static Func<T2, Tuple<T1, T2>> Create<T1, T2>
            (T1 item1) => item2 => f => f(item1)(item2);

        // Item1 => tuple => tuple(x => y => x)
        public static T1 Item1<T1, T2>
            (this Tuple<T1, T2> tuple) => (T1)tuple(x => y => x);

        // Item2 => tuple => tuple(x => y => y)
        public static T2 Item2<T1, T2>
            (this Tuple<T1, T2> tuple) => (T2)tuple(x => y => y);
    }
}

namespace Dixin.Linq.Lambda
{
    using System;

    public delegate object Tuple<out T1, out T2>(Boolean<T1, T2> f);

    public static partial class ChurchTuple
    {
        // CreateTuple = item1 => item2 => f => f(item1)(item2)
        public static Func<T2, Tuple<T1, T2>> Create<T1, T2>
            (T1 item1) => item2 => f => f(item1)(item2);

        // Item1 = tuple => tuple(x => y => x)
        public static T1 Item1<T1, T2>
            (this Tuple<T1, T2> tuple) => (T1)tuple(ChurchBoolean.True<T1, T2>);

        // Item2 = tuple => tuple(x => y => y)
        public static T2 Item2<T1, T2>
            (this Tuple<T1, T2> tuple) => (T2)tuple(ChurchBoolean.False<T1, T2>);
    }

    public static partial class ChurchTuple
    {
        public static Boolean Null<T1, T2>
            (Tuple<T1, T2> tuple) => (Boolean)tuple(x => y => new Boolean(ChurchBoolean.False));

        // (x, y) -> (y, f(y))
        // Shift = tuple => f => Create(tuple.Item2())(f(tuple.Item1()))
        public static Tuple<T, T> Shift<T>
            (this Tuple<T, T> tuple, Func<T, T> f) => Create<T, T>(tuple.Item2())(f(tuple.Item2()));

        // Swap = tuple => Create(tuple.Item2())(tuple.Item1())
        public static Tuple<T2, T1> Swap<T1, T2>
            (this Tuple<T1, T2> tuple) => Create<T2, T1>(tuple.Item2())(tuple.Item1());

        public static Tuple<T1, T2> _Create<T1, T2>
            (T1 item1, T2 item2) => Create<T1, T2>(item1)(item2);
    }
}