namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;

    public class Lazy<T1, T2>
    {
        private readonly Lazy<Tuple<T1, T2>> lazy;

        public Lazy(Func<T1> factory1, Func<T2> factory2)
            : this(() => Tuple.Create(factory1(), factory2()))
        {
        }

        public Lazy(T1 value1, T2 value2)
            : this(() => Tuple.Create(value1, value2))
        {
        }

        public Lazy(Func<Tuple<T1, T2>> factory)
        {
            this.lazy = new Lazy<Tuple<T1, T2>>(factory);
        }

        public T1 Value1
        {
            [Pure]get { return this.lazy.Value.Item1; }
        }

        public T2 Value2
        {
            [Pure]get { return this.lazy.Value.Item2; }
        }
    }

    // [Pure]
    public static partial class LazyExtensions
    {
        public static Lazy<TResult1, TResult2> Select<TSource1, TSource2, TResult1, TResult2>
            (this Lazy<TSource1, TSource2> source,
                Func<TSource1, TResult1> selector1,
                Func<TSource2, TResult2> selector2) =>
                    new Lazy<TResult1, TResult2>(() => selector1(source.Value1), () => selector2(source.Value2));

        public static IMorphism<Lazy<TSource1, TSource2>, Lazy<TResult1, TResult2>, DotNet> Select<TSource1, TSource2, TResult1, TResult2>
            (IMorphism<TSource1, TResult1, DotNet> selector1, IMorphism<TSource2, TResult2, DotNet> selector2) =>
                new DotNetMorphism<Lazy<TSource1, TSource2>, Lazy<TResult1, TResult2>>(
                    source => source.Select(selector1.Invoke, selector2.Invoke));
    }
}
