namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;

    // Tuple is the alias of (dynamic -> dynamic -> dynamic) -> dynamic.
    public delegate dynamic Tuple<out T1, out T2>(Boolean f);

    public static partial class ChurchTuple<T1, T2>
    {
        public static readonly Func<T1, Func<T2, Tuple<T1, T2>>> 
            Create = item1 => item2 => f => f(item1)(item2);

        // Item1 = tuple => tuple(True)
        public static readonly Func<Tuple<T1, T2>, T1> 
            Item1 = tuple => (T1)(object)tuple(True); // Bug: http://stackoverflow.com/questions/37392566/

        // Item2 = tuple => tuple(False)
        public static readonly Func<Tuple<T1, T2>, T2> 
            Item2 = tuple => (T2)(object)tuple(False); // Bug: http://stackoverflow.com/questions/37392566/
    }

    public static partial class ChurchTuple<T1, T2>
    {
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

    public static partial class ChurchTuple<T1, T2, T3>
    {
        // Shift = f => tuple => Create(tuple.Item2())(f(tuple.Item1()))
        public static readonly Func<Func<T2, T3>, Func<Tuple<T1, T2>, Tuple<T2, T3>>>
            Shift = f => tuple => ChurchTuple<T2, T3>.Create(tuple.Item2())(f(tuple.Item2()));
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
            ChurchTuple<T1, T2, TResult>.Shift(f)(tuple);
    }

    public delegate dynamic Tuple<out T1, out T2, out T3>(Boolean f);

    public static partial class ChurchTuple<T1, T2, T3>
    {
        // Create = item1 => item2 => item3 => Create(item1)(CreateTuple(item2)(item3))
        public static readonly Func<T1, Func<T2, Func<T3, Tuple<T1, T2, T3>>>>
            Create = item1 => item2 => item3 => new Tuple<T1, T2, T3>(ChurchTuple<T1, Tuple<T2, T3>>.Create(item1)(ChurchTuple<T2, T3>.Create(item2)(item3)));

        // Item1 = tuple.Item1()
        public static readonly Func<Tuple<T1, T2, T3>, T1>
            Item1 = tuple => new Tuple<T1, Tuple<T2, T3>>(tuple).Item1();

        // Item2 = tuple.Item2().Item1()
        public static readonly Func<Tuple<T1, T2, T3>, T2>
            Item2 = tuple => new Tuple<T1, Tuple<T2, T3>>(tuple).Item2().Item1();

        // Item3 = tuple.Item2().Item2()
        public static readonly Func<Tuple<T1, T2, T3>, T3>
            Item3 = tuple => new Tuple<T1, Tuple<T2, T3>>(tuple).Item2().Item2();
    }

    public static partial class TupleExtensions
    {
        public static T1 Item1<T1, T2, T3>(this Tuple<T1, T2, T3> tuple) => ChurchTuple<T1, T2, T3>.Item1(tuple);

        public static T2 Item2<T1, T2, T3>(this Tuple<T1, T2, T3> tuple) => ChurchTuple<T1, T2, T3>.Item2(tuple);

        public static T3 Item3<T1, T2, T3>(this Tuple<T1, T2, T3> tuple) => ChurchTuple<T1, T2, T3>.Item3(tuple);
    }
}