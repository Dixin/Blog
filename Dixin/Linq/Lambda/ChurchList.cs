namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    // ListNode<T> is the alias of Tuple<T, ListNode<T>>
    public delegate object ListNode<out T>(Either<T, ListNode<T>> f);

    public static partial class ChurchList<T>
    {
        // Create = value => next => ChurchTuple.Create(value)(next)
        public static readonly Func<T, Func<ListNode<T>, ListNode<T>>>
            Create = value => next => new ListNode<T>(ChurchTuple<T, ListNode<T>>.Create(value)(next));
    }

    public static partial class ChurchList
    {
        // Value = node => node.Item1()
        public static T Value<T>(this ListNode<T> node) => new Tuple<T, ListNode<T>>(node).Item1();

        // Next = node => If(node.IsNull())(_ => Null)(_ => node.Item2())
        public static ListNode<T> Next<T>(this ListNode<T> node) =>
            ChurchBoolean<ListNode<T>>.If(node.IsNull())
                (_ => node)
                (_ => new Tuple<T, ListNode<T>>(node).Item2());
    }

    public static partial class ChurchList<T>
    {
        // Null = f => _ => _;
        public static readonly ListNode<T>
            Null = f => new Func<Boolean, Boolean>(_ => _);
    }

    public static partial class ChurchList
    {
        // IsNull = node => node(value => next => _ => False)(True)
        public static Boolean IsNull<T>(this ListNode<T> node) =>
            (Boolean)((Func<Boolean, object>)node(value => next => new Func<Boolean, object>(_ => False)))(True);

        // Index = start => index => index(Next)(start)
        public static ListNode<T> Index<T>(this ListNode<T> start, Numeral index) =>
            index.Invoke<ListNode<T>>(Next)(start);
    }
}