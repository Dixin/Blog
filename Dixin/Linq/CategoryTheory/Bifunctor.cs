namespace Dixin.Linq.CategoryTheory
{
    using System;

#if DEMO
    // Cannot be compiled.
    public interface IBifunctor<TBifunctor<,>> where TBifunctor<,> : IFunctor<TBifunctor<,>>
    {
        Func<TBifunctor<TSource1, TSource2>, TBifunctor<TResult1, TResult2>> Select<TSource1, TSource2, TResult1, TResult2>(
            Func<TSource1, TResult1> selector1, Func<TSource2, TResult2> selector2);
    }
#endif

    public static partial class TupleExtensions // Tuple<T1, T2> : IBifunctor<Tuple<,>>
    {
        // Bifunctor Select: (TSource1 -> TResult1, TSource2 -> TResult2) -> (Tuple<TSource1, TSource2> -> Tuple<TResult1, TResult2>).
        public static Func<Tuple<TSource1, TSource2>, Tuple<TResult1, TResult2>> Select<TSource1, TSource2, TResult1, TResult2>(
            Func<TSource1, TResult1> selector1, Func<TSource2, TResult2> selector2) => source =>
                Select(source, selector1, selector2);

        // LINQ-like Select: (Tuple<TSource1, TSource2>, TSource1 -> TResult1, TSource2 -> TResult2) -> Tuple<TResult1, TResult2>).
        public static Tuple<TResult1, TResult2> Select<TSource1, TSource2, TResult1, TResult2>(
            this Tuple<TSource1, TSource2> source,
            Func<TSource1, TResult1> selector1,
            Func<TSource2, TResult2> selector2) =>
                selector1(source.Item1).Tuple(selector2(source.Item2));
    }

    public class Lazy<T1, T2>
    {
        private readonly Lazy<Tuple<T1, T2>> lazy;

        public Lazy(Func<Tuple<T1, T2>> factory)
        {
            this.lazy = new Lazy<Tuple<T1, T2>>(factory);
        }

        public T1 Value1 => this.lazy.Value.Item1;

        public T2 Value2 => this.lazy.Value.Item2;

        public override string ToString() => this.lazy.Value.ToString();
    }

    public static partial class LazyExtensions
    {
        public static Lazy<T1, T2> Lazy<T1, T2>(this T1 value1, T2 value2) =>
            new Lazy<T1, T2>(() => value1.Tuple(value2));
    }

    public static partial class LazyExtensions // Lazy<T1, T2> : IBifunctor<Lazy<,>>
    {
        // Bifunctor Select: (TSource1 -> TResult1, TSource2 -> TResult2) -> (Lazy<TSource1, TSource2> -> Lazy<TResult1, TResult2>).
        public static Func<Lazy<TSource1, TSource2>, Lazy<TResult1, TResult2>> Select<TSource1, TSource2, TResult1, TResult2>(
            Func<TSource1, TResult1> selector1, Func<TSource2, TResult2> selector2) => source =>
                Select(source, selector1, selector2);

        // LINQ-like Select: (Lazy<TSource1, TSource2>, TSource1 -> TResult1, TSource2 -> TResult2) -> Lazy<TResult1, TResult2>).
        public static Lazy<TResult1, TResult2> Select<TSource1, TSource2, TResult1, TResult2>(
            this Lazy<TSource1, TSource2> source,
            Func<TSource1, TResult1> selector1,
            Func<TSource2, TResult2> selector2) =>
                new Lazy<TResult1, TResult2>(() => selector1(source.Value1).Tuple(selector2(source.Value2)));
    }
}
