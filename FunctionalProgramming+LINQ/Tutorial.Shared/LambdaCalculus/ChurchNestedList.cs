namespace Tutorial.LambdaCalculus
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

    public static partial class ChurchNestedList<T>
    {
        // IsNull = node => node.Item1()
        public static readonly Func<NestedListNode<T>, Boolean>
            IsNull = node => new Tuple<Boolean, Tuple<T, NestedListNode<T>>>(node).Item1();
    }

    public static partial class ChurchNestedList<T>
    {
        // Create = value => next => ChurchTuple.Create(False)(ChurchTuple.Create(value)(next))
        public static readonly Func<T, Func<NestedListNode<T>, NestedListNode<T>>>
            Create = value => next => new NestedListNode<T>(ChurchTuple<Boolean, Tuple<T, NestedListNode<T>>>.Create
                (False)
                (ChurchTuple<T, NestedListNode<T>>.Create(value)(next)));
    }

    public static partial class ChurchNestedList<T>
    {
        // Value = node => node.Item2().Item1()
        public static readonly Func<NestedListNode<T>, T>
            Value = node => new Tuple<Boolean, Tuple<T, NestedListNode<T>>>(node).Item2().Item1();

        // Next = node => node.Item2().Item2()
        public static readonly Func<NestedListNode<T>, NestedListNode<T>> 
            Next = node => new Tuple<Boolean, Tuple<T, NestedListNode<T>>>(node).Item2().Item2();
    }

    public static partial class ChurchNestedList<T>
    {
        // NodeAt = start => index = index(Next)(start)
        public static readonly Func<NestedListNode<T>, Func<Numeral, NestedListNode<T>>> 
            NodeAt = start => index => (NestedListNode<T>)index(node => Next((NestedListNode<T>)node))(start);
    }

    public static partial class NestedListNodeExtensions
    {
        public static Boolean IsNull<T>(this NestedListNode<T> node) => ChurchNestedList<T>.IsNull(node);

        public static T Value<T>(this NestedListNode<T> node) => ChurchNestedList<T>.Value(node);

        public static NestedListNode<T> Next<T>(this NestedListNode<T> node) => ChurchNestedList<T>.Next(node);

        public static NestedListNode<T> NodeAt<T>(this NestedListNode<T> start, Numeral index) => 
            ChurchNestedList<T>.NodeAt(start)(index);
    }
}