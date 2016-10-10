namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    // ListNode2 is the alias of Tuple<Boolean, Tuple<T, ListNode2<T>>>.
    public delegate object NestedListNode<out T>(Boolean f);

    public static partial class ChurchNestedList<T>
    {
        // Null = f => True
        public static readonly NestedListNode<T> 
            Null = f => True;
    }

    public static partial class ChurchNestedList
    {
        // IsNull = node => node.Item1()
        public static Boolean IsNull<T>(this NestedListNode<T> node) => 
            new Tuple<Boolean, Tuple<T, NestedListNode<T>>>(node).Item1();
    }

    public static partial class ChurchNestedList<T>
    {
        // Create = value => next => ChurchTuple.Create(False)(ChurchTuple.Create(value)(next))
        public static readonly Func<T, Func<NestedListNode<T>, NestedListNode<T>>> 
            Create = value => next =>
                new NestedListNode<T>(ChurchTuple<Boolean, Tuple<T, NestedListNode<T>>>.Create
                    (False)
                    (ChurchTuple<T, NestedListNode<T>>.Create(value)(next)));
    }

    public static partial class ChurchNestedList
    {
        // Value = node => node.Item2().Item1()
        public static T Value<T>(this NestedListNode<T> node) =>
            new Tuple<Boolean, Tuple<T, NestedListNode<T>>>(node).Item2().Item1();

        // Next = node => If(node.IsNull())(_ => node)(_ => node.Item2().Item2())
        public static NestedListNode<T> Next<T>(this NestedListNode<T> node) =>
            ChurchBoolean<NestedListNode<T>>.If(node.IsNull())
                (_ => node)
                (_ => new Tuple<Boolean, Tuple<T, NestedListNode<T>>>(node).Item2().Item2());
    }

    public static partial class ChurchNestedList
    {
        // Index = start => index = index(Next)(start)
        public static NestedListNode<T> Index<T>(this NestedListNode<T> start, Numeral index) =>
            (NestedListNode<T>)index(node => Next((NestedListNode<T>)node))(start);
    }
}