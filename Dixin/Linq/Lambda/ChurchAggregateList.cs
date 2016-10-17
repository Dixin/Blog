namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    // object AggregateListNode<out T>(object x, Func<object, T, object> f);
    // Curried from: object AggregateListNode<out T>(object x, Func<T, object, object> f).
    // AggregateListNode is the alias of: Func<object, Func<Func<T, Func<object, object>>, object>>.
    public delegate Func<Func<object, Func<T, object>>, object> AggregateListNode<out T>(object x);

    public static partial class ChurchAggregateList<T>
    {
        // Create = value => next => x => f => f(next(x)(f))(value)
        public static readonly Func<T, Func<AggregateListNode<T>, AggregateListNode<T>>>
            Create = value => next => x => f => f(next(x)(f))(value);
    }

    public static partial class ChurchAggregateList<T>
    {
        // Null = x => f => x
        public static readonly AggregateListNode<T>
            Null = x => f => x;

        // IsNull = node => node(True)(x => value => False)
        public static readonly Func<AggregateListNode<T>, Boolean>
            IsNull = node => (Boolean)node(True)(x => value => False);

        // Value = node => node(Id)(x => value => value)
        public static readonly Func<AggregateListNode<T>, T>
            Value = node => (T)node(Functions<T>.Id)(x => value => value);

#if DEMO
        // Next = node => x => f => node(_ => x)(accumulate => value => (g => g(accumulate(f))(value)))(accumulate => value => accumulate);
        public static readonly Func<AggregateListNode<T>, AggregateListNode<T>>
           Next = node => x => f => ((Func<Func<object, Func<T, object>>, object>)node(new Func<Func<object, Func<T, object>>, object>(_ => x))(accumulate => value => new Func<Func<object, Func<T, object>>, object>(g => g(((Func<Func<object, Func<T, object>>, object>)accumulate)(f))(value))))(accumulate => value => accumulate);
#endif

        // https://books.google.com/books?id=1Sm6BQAAQBAJ&pg=PA172&lpg=PA172&dq=church+list+fold+haskell&source=bl&ots=gJccvSeeWw&sig=Zsqfb94JjyF0lWt1veJEfREhHsg&hl=en&sa=X&ei=S07HVMy7EIzjoAT1tID4BQ&ved=0CEYQ6AEwBzgK#v=onepage&q=church%20list%20fold%20haskell&f=false
        // Next = node => node(ChurchTuple.Create(Null)(Null))(tuple => value => tuple.Shift(Create(value))).Item1()
        public static readonly Func<AggregateListNode<T>, AggregateListNode<T>>
            Next = node =>
                ((Tuple<AggregateListNode<T>, AggregateListNode<T>>)node
                    (ChurchTuple<AggregateListNode<T>, AggregateListNode<T>>.Create(Null)(Null))
                    (tuple => value => ((Tuple<AggregateListNode<T>, AggregateListNode<T>>)tuple).Shift(Create(value))))
                .Item1();

        // NodeAt = start => index => index(Next)(start)
        public static readonly Func<AggregateListNode<T>, Func<Numeral, AggregateListNode<T>>>
            NodeAt = start => index => (AggregateListNode<T>)index(node => Next((AggregateListNode<T>)node))(start);
    }

    public static class AggregateListNodeExtensions
    {
        public static Boolean IsNull<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.IsNull(node);

        public static T Value<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.Value(node);

        public static AggregateListNode<T> Next<T>(this AggregateListNode<T> node) => 
            ChurchAggregateList<T>.Next(node);

        public static AggregateListNode<T> NodeAt<T>(this AggregateListNode<T> start, Numeral index) => 
            ChurchAggregateList<T>.NodeAt(start)(index);
    }
}
