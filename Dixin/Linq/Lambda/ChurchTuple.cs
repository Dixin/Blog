namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    // Tuple is the alias of Func<Func<object, Func<object, object>>, object>.
    public delegate object Tuple<out T1, out T2>(Boolean f);

    public static partial class ChurchTuple<T1, T2>
    {
        // CreateTuple = item1 => item2 => f => f(item1)(item2)
        public static readonly Func<T1, Func<T2, Tuple<T1, T2>>>
            Create = item1 => item2 => f => f(item1)(item2);

        // Item1 = tuple => tuple(x => y => x)
        public static readonly Func<Tuple<T1, T2>, T1>
            Item1 = tuple => (T1)tuple(True);

        // Item2 = tuple => tuple(x => y => y)
        public static readonly Func<Tuple<T1, T2>, T2>
            Item2 = tuple => (T2)tuple(False);

        internal static void Point(Numeral x, Numeral y)
        {
            Tuple<Numeral, Numeral> point1 = ChurchTuple<Numeral, Numeral>.Create(x)(y);
            Numeral x1 = point1.Item1();
            Numeral y1 = point1.Item1();

            // Move up.
            Numeral y2 = y1.Increase();
            Tuple<Numeral, Numeral> point2 = ChurchTuple<Numeral, Numeral>.Create(x1)(y2);
        }
    }

    public static partial class ChurchTuple<T1, T2>
    {
        // Swap = tuple => Create(tuple.Item2())(tuple.Item1())
        public static readonly Func<Tuple<T1, T2>, Tuple<T2, T1>>
            Swap = tuple => ChurchTuple<T2, T1>.Create(tuple.Item2())(tuple.Item1());
    }

    public static partial class ChurchTuple<T1, T2, TResult>
    {
        // Shift = tuple => f => Create(tuple.Item2())(f(tuple.Item1()))
        public static readonly Func<Tuple<T1, T2>, Func<Func<T2, TResult>, Tuple<T2, TResult>>>
            Shift = tuple => f => ChurchTuple<T2, TResult>.Create(tuple.Item2())(f(tuple.Item2()));
    }

    public static partial class TupleExtensions
    {
        public static T1 Item1<T1, T2>(this Tuple<T1, T2> tuple) => ChurchTuple<T1, T2>.Item1(tuple);

        public static T2 Item2<T1, T2>(this Tuple<T1, T2> tuple) => ChurchTuple<T1, T2>.Item2(tuple);
    }

    public static partial class TupleExtensions
    {
        public static Tuple<T2, T1> Swap<T1, T2>(this Tuple<T1, T2> tuple) => ChurchTuple<T1, T2>.Swap(tuple);

        public static Tuple<T2, TResult> Shift<T1, T2, TResult>(this Tuple<T1, T2> tuple, Func<T2, TResult> f) => 
            ChurchTuple<T1, T2, TResult>.Shift(tuple)(f);
    }
}