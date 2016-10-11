namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    // Curried from object AggregateListNode<out T>(Func<T, object, object> f, object x)
    // AggregateListNode is the alias of Func<Func<T, Func<object, object>>, Func<object, object>>
    public delegate Func<object, object> AggregateListNode<out T>(Func<object, Func<T, object>> f);

    public static partial class ChurchAggregateList<T>
    {
        public static readonly Func<T, Func<AggregateListNode<T>, AggregateListNode<T>>>
            Create = value => next => f => x => f(next(f)(x))(value);

        public static readonly AggregateListNode<T>
            Null = f => x => x;
    }

    public static partial class ChurchAggregateList<T>
    {
        // IsNull = node => node(value => _ => False)(True)
        public static readonly Func<AggregateListNode<T>, Boolean>
            IsNull = node => (Boolean)node(value => _ => False)(True);

        public static readonly Func<AggregateListNode<T>, T>
            Value = node => (T)node(_ => value => value)(Functions<T>.Id);

        // https://books.google.com/books?id=1Sm6BQAAQBAJ&pg=PA172&lpg=PA172&dq=church+list+fold+haskell&source=bl&ots=gJccvSeeWw&sig=Zsqfb94JjyF0lWt1veJEfREhHsg&hl=en&sa=X&ei=S07HVMy7EIzjoAT1tID4BQ&ved=0CEYQ6AEwBzgK#v=onepage&q=church%20list%20fold%20haskell&f=false
        // Next = node => node(tuple => value => tuple.Shift(Create(value)))(ChurchTuple.Create(Null)(Null)).Item1()
        public static readonly Func<AggregateListNode<T>, AggregateListNode<T>>
            Next = node =>
                ((Tuple<AggregateListNode<T>, AggregateListNode<T>>)node
                    (tuple => value => ((Tuple<AggregateListNode<T>, AggregateListNode<T>>)tuple).Shift(Create(value)))
                    (ChurchTuple<AggregateListNode<T>, AggregateListNode<T>>.Create(Null)(Null)))
                .Item1();

        // Index = start => index => index(Next)(start)
        public static readonly Func<AggregateListNode<T>, Func<Numeral, AggregateListNode<T>>>
            NodeAt = start => index => (AggregateListNode<T>)index(node => ((AggregateListNode<T>)node).Next())(start);
    }

    public static class AggregateListNodeExtensions
    {
        public static Boolean IsNull<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.IsNull(node);

        public static T Value<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.Value(node);

        public static AggregateListNode<T> Next<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.Next(node);

        public static AggregateListNode<T> NodeAt<T>(this AggregateListNode<T> start, Numeral index) => ChurchAggregateList<T>.NodeAt(start)(index);
    }
}
