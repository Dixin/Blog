namespace Dixin.Linq.Lambda
{
    using System;

    // ListNode2 is the alias of Tuple<Boolean, Tuple<T, ListNode2<T>>>
    public delegate object ListNode2<out T>(Boolean<Boolean, Tuple<T, ListNode2<T>>> f);

    public static partial class ChurchList2
    {
        // Null = f => ChurchBoolean.True
        public static object Null<T>
            (Boolean<Boolean, Tuple<T, ListNode2<T>>> f) => new Boolean(ChurchBoolean.True);

        // IsNull = node => node.Item1()
        public static Boolean IsNull<T>
            (this ListNode2<T> node) => new Tuple<Boolean, Tuple<T, ListNode2<T>>>(node).Item1();
    }

    public static partial class ChurchList2
    {
        // Create = value => next => ChurchTuple.Create(ChurchBoolean.False)(ChurchTuple.Create(value)(next))
        public static Func<ListNode2<T>, ListNode2<T>> Create<T>
            (T value) => next =>
                new ListNode2<T>(ChurchTuple.Create<Boolean, Tuple<T, ListNode2<T>>>
                    (ChurchBoolean.False)
                    (ChurchTuple.Create<T, ListNode2<T>>(value)(next)));

        // Value = node => node.Item2().Item1()
        public static T Value<T>
            (this ListNode2<T> node) => new Tuple<Boolean, Tuple<T, ListNode2<T>>>(node).Item2().Item1();

        // Next = node => ChurchBoolean.If(node.IsNull())(_ => node)(_ => node.Item2().Item2())
        public static ListNode2<T> Next<T>
            (this ListNode2<T> node) =>
                ChurchBoolean.If<ListNode2<T>>(node.IsNull())
                    (_ => node)
                    (_ => new Tuple<Boolean, Tuple<T, ListNode2<T>>>(node).Item2().Item2());
    }

    public static partial class ChurchList2
    {
        // Index = start => index = index(Next)(start)
        public static ListNode2<T> Index<T>
            (this ListNode2<T> start, _Numeral index) => index.Numeral<ListNode2<T>>()(Next)(start);
    }
}