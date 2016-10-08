namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    // Curried from TResult ListNode<out T, TResult>(Func<T, TResult, TResult> f, TResult x)
    // ListNode is the alias of Func<Func<T, Func<TResult, TResult>>, Func<TResult, TResult>>
    public delegate Func<TAccumulate, TAccumulate> AggregateListNode<out T, TAccumulate>(Func<TAccumulate, Func<T, TAccumulate>> f);

    public partial class AggregateListNode<T>
    {
        private readonly T value;

        private readonly AggregateListNode<T> next;

        public AggregateListNode(T value, AggregateListNode<T> next)
        {
            this.value = value;
            this.next = next;
        }

        protected virtual AggregateListNode<T> Next => this.next;

        public virtual Func<TAccumulate, TAccumulate> Invoke<TAccumulate>(
            Func<TAccumulate, Func<T, TAccumulate>> f) =>
                x => f(this.Next.Invoke(f)(x))(this.value);
    }

    public partial class AggregateListNode<T>
    {
        private AggregateListNode()
        {
        }

        private class NullAggregateListNode : AggregateListNode<T>
        {
            protected override AggregateListNode<T> Next => this;

            public override Func<TAccumulate, TAccumulate> Invoke<TAccumulate>(
                Func<TAccumulate, Func<T, TAccumulate>> f) =>
                    x => x;
        }

        public static AggregateListNode<T> Null { get; } = new NullAggregateListNode();
    }

    public static partial class ChurchAggregateList
    {
        // IsNull = node => node(value => _ => False)(True)
        public static Boolean IsNull<T>(this AggregateListNode<T> node) =>
            node.Invoke<Boolean>(value => _ => False)(True);
    }

    public partial class AggregateListNode<T>
    {
        // Create = value => next => f => x => f(value)(next(f)(x))
        public static readonly Func<T, Func<AggregateListNode<T>, AggregateListNode<T>>>
            Create = value => next => new AggregateListNode<T>(value, next);
    }

    public static partial class ChurchAggregateList
    {
        // Value = node => ignore => node(value => _ => value)(ignore)
        public static T Value<T>(this AggregateListNode<T> node, T ignore = default(T)) =>
            node.Invoke<T>(_ => value => value)(ignore);

        // https://books.google.com/books?id=1Sm6BQAAQBAJ&pg=PA172&lpg=PA172&dq=church+list+fold+haskell&source=bl&ots=gJccvSeeWw&sig=Zsqfb94JjyF0lWt1veJEfREhHsg&hl=en&sa=X&ei=S07HVMy7EIzjoAT1tID4BQ&ved=0CEYQ6AEwBzgK#v=onepage&q=church%20list%20fold%20haskell&f=false
        // Next = node => node(value => tuple => tuple.Shift(Create(value)))(ChurchTuple.Create(Null)(Null)).Item1()
        public static AggregateListNode<T> Next<T>(this AggregateListNode<T> node) =>
            node.Invoke<Tuple<AggregateListNode<T>, AggregateListNode<T>>>
                (tuple => value => tuple.Shift(AggregateListNode<T>.Create(value)))
                (ChurchTuple<AggregateListNode<T>, AggregateListNode<T>>.Create(AggregateListNode<T>.Null)(AggregateListNode<T>.Null))
                .Item1();
    }

    public static partial class ChurchAggregateList
    {
        // Index = start => index => index(Next)(start)
        public static AggregateListNode<T> Index<T>(this AggregateListNode<T> start, Numeral index) => 
            index.Invoke<AggregateListNode<T>>(Next)(start);
    }

    public partial class AggregateListNode<T>
    {
        public static AggregateListNode<T> operator ++ (AggregateListNode<T> node) => node.Next();
    }
}
