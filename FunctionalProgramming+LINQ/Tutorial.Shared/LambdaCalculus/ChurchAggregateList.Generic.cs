namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;

    // Curried from TResult AggregateListNode<out T, TAccumulate>(Func<T, TAccumulate, TAccumulate> f, TResult x)
    // AggregateListNode is the alias of Func<Func<T, Func<TAccumulate, TAccumulate>>, Func<TAccumulate, TAccumulate>>
    public delegate Func<TAccumulate, TAccumulate> AggregateListNode<out T, TAccumulate>(Func<TAccumulate, Func<T, TAccumulate>> f);

    public partial class AggregateListNodeWrapper<T>
    {
        private readonly T value;

        private readonly AggregateListNodeWrapper<T> next;

        public AggregateListNodeWrapper(T value, AggregateListNodeWrapper<T> next)
        {
            this.value = value;
            this.next = next;
        }

        protected virtual AggregateListNodeWrapper<T> Next => this.next;

        public virtual Func<TAccumulate, TAccumulate> Invoke<TAccumulate>(
            Func<TAccumulate, Func<T, TAccumulate>> f) =>
                x => f(this.Next.Invoke(f)(x))(this.value);
    }

    public partial class AggregateListNodeWrapper<T>
    {
        private AggregateListNodeWrapper() { }

        private class NullAggregateListNodeWrapper : AggregateListNodeWrapper<T>
        {
            protected override AggregateListNodeWrapper<T> Next => this;

            public override Func<TAccumulate, TAccumulate> Invoke<TAccumulate>(
                Func<TAccumulate, Func<T, TAccumulate>> f) =>
                    x => x;
        }

        public static AggregateListNodeWrapper<T> Null { get; } = new NullAggregateListNodeWrapper();
    }

    public static partial class ChurchAggregateListNodeWrapper
    {
        // IsNull = node => node(value => _ => False)(True)
        public static Boolean IsNull<T>(this AggregateListNodeWrapper<T> node) =>
            node.Invoke<Boolean>(value => _ => False)(True);
    }

    public partial class AggregateListNodeWrapper<T>
    {
        // Create = value => next => f => x => f(value)(next(f)(x))
        public static readonly Func<T, Func<AggregateListNodeWrapper<T>, AggregateListNodeWrapper<T>>>
            Create = value => next => new AggregateListNodeWrapper<T>(value, next);
    }

    public static partial class ChurchAggregateListNodeWrapper
    {
        // Value = node => ignore => node(value => _ => value)(ignore)
        public static T Value<T>(this AggregateListNodeWrapper<T> node, T ignore = default) =>
            node.Invoke<T>(_ => value => value)(ignore);

        // https://books.google.com/books?id=1Sm6BQAAQBAJ&pg=PA172&lpg=PA172&dq=church+list+fold+haskell&source=bl&ots=gJccvSeeWw&sig=Zsqfb94JjyF0lWt1veJEfREhHsg&hl=en&sa=X&ei=S07HVMy7EIzjoAT1tID4BQ&ved=0CEYQ6AEwBzgK#v=onepage&q=church%20list%20fold%20haskell&f=false
        // Next = node => node(tuple => value => tuple.Shift(Create(value)))(ChurchTuple.Create(Null)(Null)).Item1()
        public static AggregateListNodeWrapper<T> Next<T>(this AggregateListNodeWrapper<T> node) =>
            node.Invoke<Tuple<AggregateListNodeWrapper<T>, AggregateListNodeWrapper<T>>>
                (tuple => value => tuple.Shift(AggregateListNodeWrapper<T>.Create(value)))
                (ChurchTuple<AggregateListNodeWrapper<T>, AggregateListNodeWrapper<T>>.Create(AggregateListNodeWrapper<T>.Null)(AggregateListNodeWrapper<T>.Null))
                .Item1();

        // Index = start => index => index(Next)(start)
        public static AggregateListNodeWrapper<T> Index<T>(this AggregateListNodeWrapper<T> start, Numeral index) =>
            (AggregateListNodeWrapper<T>)index(node => Next((AggregateListNodeWrapper<T>)node))(start);
    }
}
