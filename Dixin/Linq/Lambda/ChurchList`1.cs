namespace Dixin.Linq.Lambda
{
    using System;

    // Curried from TResult ListNode<out T, TResult>(Func<T, TResult, TResult> f, TResult x)
    public delegate Func<TAccumulate, TAccumulate> ListNode<out T, TAccumulate>(Func<TAccumulate, Func<T, TAccumulate>> f);
    // ListNode is the alias of: Func<Func<T, Func<TResult, TResult>>, Func<TResult, TResult>>

    public partial class _ListNode<T>
    {
        private readonly T value;

        protected virtual _ListNode<T> Next { get; set; }

        public _ListNode(T value, _ListNode<T> next)
        {
            this.value = value;
            this.Next = next;
        }

        public virtual ListNode<T, TAccumulate> Node<TAccumulate>
            () =>
                f => x => f(this.Next.Node<TAccumulate>()(f)(x))(this.value);
    }

    public partial class _ListNode<T>
    {
        private _ListNode()
        {
        }

        private class _NullListNode : _ListNode<T>
        {
            protected override _ListNode<T> Next { get { return this; } set { } }

            public override ListNode<T, TAccumulate> Node<TAccumulate>
                () =>
                    f => x => x;
        }

        public static _ListNode<T> Null { get; } = new _NullListNode();
    }

    public static partial class _ListNodeExtensions
    {
        // IsNull = node => node(value => _ => ChurchBoolean.False)(ChurchBoolean.True)
        public static Boolean IsNull<T>
            (this _ListNode<T> node) =>
                node.Node<Boolean>()(value => _ => ChurchBoolean.False)(ChurchBoolean.True);
    }

    public static partial class _ListNodeExtensions
    {
        // Create = value => next => f => x => f(value)(next(f)(x))
        public static Func<_ListNode<T>, _ListNode<T>> Create<T>
            (T value) => next => new _ListNode<T>(value, next);


        // Value = node => anyValueToIgnore => node(value => _ => value)(anyValueToIgnore)
        public static T Value<T>
            (this _ListNode<T> node, T anyValueToIgnore = default(T)) =>
                node.Node<T>()(_ => value => value)(anyValueToIgnore);

        // https://books.google.com/books?id=1Sm6BQAAQBAJ&pg=PA172&lpg=PA172&dq=church+list+fold+haskell&source=bl&ots=gJccvSeeWw&sig=Zsqfb94JjyF0lWt1veJEfREhHsg&hl=en&sa=X&ei=S07HVMy7EIzjoAT1tID4BQ&ved=0CEYQ6AEwBzgK#v=onepage&q=church%20list%20fold%20haskell&f=false
        // Next = node => node(value => tuple => tuple.Shift(Create(value)))(ChurchTuple.Create(Null)(Null)).Item1()
        public static _ListNode<T> Next<T>
            (this _ListNode<T> node) =>
                node.Node<Tuple<_ListNode<T>, _ListNode<T>>>()
                    (tuple => value => tuple.Shift(Create(value)))
                    (ChurchTuple.Create<_ListNode<T>, _ListNode<T>>(_ListNode<T>.Null)(_ListNode<T>.Null))
                    .Item1();
    }

    public static partial class _ListNodeExtensions
    {
        // Index = start => index => index(Next)(start)
        public static _ListNode<T> Index<T>
            (this _ListNode<T> start, _Numeral index) => index.Numeral<_ListNode<T>>()(Next)(start);
    }

    public partial class _ListNode<T>
    {
        public static _ListNode<T> operator ++
            (_ListNode<T> node) => node.Next();
    }
}
