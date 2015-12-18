namespace Dixin.Linq.Lambda.Obsolete2
{
    using System;

    public delegate Func<object, object> Boolean(object @true);

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

        // IsNull = node(value => next => _ => ChurchBoolean.False)(ChurchBoolean.True)
        public static Boolean IsNull<T>
            (this ListNode<T> node) =>
                ((Func<Boolean, Boolean>)node(value => next =>
                    new Func<Boolean, Boolean>(_ => ChurchBoolean.False)))(ChurchBoolean.True);
    }

    // ListNode<T> is alias of Tuple<T, ListNode<T>>
    public delegate object ListNode<out T>(Boolean<T, ListNode<T>> f);

    public static class ChurchList
    {
        // Create = value => next => ChurchTuple.Create(value)(next)
        public static Func<ListNode<T>, ListNode<T>> Create<T>
            (T value) => next => new ListNode<T>(ChurchTuple.Create<T, ListNode<T>>(value)(next));

        // Value = node => node.Item1()
        public static T Value<T>
            (this ListNode<T> node) => new Tuple<T, ListNode<T>>(node).Item1();

        // Next = node => node.Item2()
        public static ListNode<T> Next<T>
            (this ListNode<T> node) => new Tuple<T, ListNode<T>>(node).Item2();
    }
}

namespace Dixin.Linq.Lambda
{
    using System;

    // ListNode<T> is alias of Tuple<T, ListNode<T>>
    public delegate object ListNode<out T>(Boolean<T, ListNode<T>> f);

    public static partial class ChurchList
    {
        // Create = value => next => ChurchTuple.Create(value)(next)
        public static Func<ListNode<T>, ListNode<T>> Create<T>
            (T value) => next => new ListNode<T>(ChurchTuple.Create<T, ListNode<T>>(value)(next));

        // Value = node => node.Item1()
        public static T Value<T>
            (this ListNode<T> node) => new Tuple<T, ListNode<T>>(node).Item1();

        // Next = node => If(node.IsNull())(_ => Null)(_ => node.Item2())
        public static ListNode<T> Next<T>
            (this ListNode<T> node) =>
                ChurchBoolean.If<ListNode<T>>(node.IsNull())
                    (_ => node)
                    (_ => new Tuple<T, ListNode<T>>(node).Item2());
    }

    public static partial class ChurchList
    {
        // Null = f => _ => _;
        public static object Null<T>
            (Boolean<T, ListNode<T>> f) => new Func<Boolean, Boolean>(_ => _);


        // Null = ChurchBoolean.False;
        public static ListNode<T> GetNull<T>
            () => ChurchBoolean.False<Boolean<T, ListNode<T>>, Boolean>;

        // IsNull = node => node(value => next => _ => ChurchBoolean.False)(ChurchBoolean.True)
        public static Boolean IsNull<T>
            (this ListNode<T> node) =>
                (Boolean)((Func<Boolean, object>)node(value => next =>
                    new Func<Boolean, object>(_ =>
                        new Boolean(ChurchBoolean.False))))(ChurchBoolean.True);

        // Index = start => index => index(Next)(start)
        public static ListNode<T> Index<T>
            (this ListNode<T> start, _Numeral index) => index.Numeral<ListNode<T>>()(Next)(start);
    }
}