namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;

    // Curried from: (dynamic, dynamic -> T -> dynamic) -> dynamic.
    // AggregateListNode is the alias of: dynamic -> (dynamic -> T -> dynamic) -> dynamic.
    public delegate Func<Func<dynamic, Func<T, dynamic>>, dynamic> AggregateListNode<out T>(dynamic x);

    public static partial class ChurchAggregateList<T>
    {
        public static readonly Func<T, Func<AggregateListNode<T>, AggregateListNode<T>>>
            Create = value => next => x => f => f(next(x)(f))(value);
    }

    public static partial class ChurchAggregateList<T>
    {
        public static readonly AggregateListNode<T>
            Null = x => f => x;

        public static readonly Func<AggregateListNode<T>, Boolean>
            IsNull = node => node(True)(x => value => False);

        // Value = node => node(Id)(x => value => value)
        public static readonly Func<AggregateListNode<T>, T>
            Value = node => node(Functions<T>.Id)(x => value => value);

        // Next = node => x => f => node(_ => x)(accumulate => value => (g => g(accumulate(f))(value)))(accumulate => value => accumulate);
        public static readonly Func<AggregateListNode<T>, AggregateListNode<T>>
           Next = node => x => f => node(new Func<Func<dynamic, Func<T, dynamic>>, dynamic>(_ => x))(accumulate => value => new Func<Func<dynamic, Func<T, dynamic>>, dynamic>(g => g(accumulate(f))(value)))(new Func<dynamic, Func<T, dynamic>>(accumulate => value => accumulate));

        // https://books.google.com/books?id=1Sm6BQAAQBAJ&pg=PA172&lpg=PA172&dq=church+list+fold+haskell&source=bl&ots=gJccvSeeWw&sig=Zsqfb94JjyF0lWt1veJEfREhHsg&hl=en&sa=X&ei=S07HVMy7EIzjoAT1tID4BQ&ved=0CEYQ6AEwBzgK#v=onepage&q=church%20list%20fold%20haskell&f=false
        // Next = node => node((Null, Null))(tuple => value => tuple.Shift(ChurchTuple.Create(value))).Item1()
        public static readonly Func<AggregateListNode<T>, AggregateListNode<T>>
            NextWithSwap = node =>
                ((Tuple<AggregateListNode<T>, AggregateListNode<T>>)node
                    (ChurchTuple<AggregateListNode<T>, AggregateListNode<T>>.Create(Null)(Null))
                    (tuple => value => ((Tuple<AggregateListNode<T>, AggregateListNode<T>>)tuple).Shift(Create(value))))
                .Item1();

        // NodeAt = start => index => index(Next)(start)
        public static readonly Func<AggregateListNode<T>, Func<Numeral, AggregateListNode<T>>>
            ListNodeAt = start => index => index(node => Next(node))(start);
    }

    public static class AggregateListNodeExtensions
    {
        public static Boolean IsNull<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.IsNull(node);

        public static T Value<T>(this AggregateListNode<T> node) => ChurchAggregateList<T>.Value(node);

        public static AggregateListNode<T> Next<T>(this AggregateListNode<T> node) =>
            ChurchAggregateList<T>.Next(node);

        public static AggregateListNode<T> ListNodeAt<T>(this AggregateListNode<T> start, Numeral index) =>
            ChurchAggregateList<T>.ListNodeAt(start)(index);
    }
}
