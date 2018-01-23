namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;

    // ListNode<T> is the alias of Tuple<T, ListNode<T>>.
    public delegate dynamic ListNode<out T>(Boolean f);

    public static partial class ChurchList<T>
    {
        // Create = value => next => (value, next)
        public static readonly Func<T, Func<ListNode<T>, ListNode<T>>>
            Create = value => next => new ListNode<T>(ChurchTuple<T, ListNode<T>>.Create(value)(next));

        // Value = node => node.Item1()
        public static readonly Func<ListNode<T>, T> 
            Value = node => new Tuple<T, ListNode<T>>(node).Item1();

        // Next = node => node.Item2()
        public static readonly Func<ListNode<T>, ListNode<T>> 
            Next = node => new Tuple<T, ListNode<T>>(node).Item2();
    }

    public static partial class ChurchList<T>
    {
        // Null = False;
        public static readonly ListNode<T>
            Null = new ListNode<T>(False);

        // IsNull = node => node(value => next => _ => False)(True)
        public static readonly Func<ListNode<T>, Boolean> 
            IsNull = node => node(value => next => new Func<Boolean, Boolean>(_ => False))(True);

        public static readonly Func<ListNode<T>, Func<Numeral, ListNode<T>>>
            ListNodeAt = start => index => index(node => Next(node))(start);
    }

    public static class ListNodeExtensions
    {
        public static T Value<T>(this ListNode<T> node) => ChurchList<T>.Value(node);

        public static ListNode<T> Next<T>(this ListNode<T> node) => ChurchList<T>.Next(node);

        public static Boolean IsNull<T>(this ListNode<T> node) => ChurchList<T>.IsNull(node);

        public static ListNode<T> ListNodeAt<T>(this ListNode<T> start, Numeral index) => ChurchList<T>.ListNodeAt(start)(index);
    }
}
