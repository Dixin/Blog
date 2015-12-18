namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    [Pure]
    public static partial class TupleExtensions
    {
        // C# specific functor pattern.
        public static Tuple<TResult> Select<TSource, TResult>
            (this Tuple<TSource> source, Func<TSource, TResult> selector) =>
                new Tuple<TResult>(selector(source.Item1));

        // General abstract functor definition of Tuple<>: DotNet -> DotNet.
        public static IMorphism<Tuple<TSource>, Tuple<TResult>, DotNet> Select<TSource, TResult>
            (/* this */ IMorphism<TSource, TResult, DotNet> selector) =>
                new DotNetMorphism<Tuple<TSource>, Tuple<TResult>>(source => source.Select(selector.Invoke));
    }

    // [Pure]
    public static partial class TupleExtensions
    {
        // C# specific functor pattern.
        public static Tuple<TResult, T2> Select<TSource, TResult, T2>
            (this Tuple<TSource, T2> source, Func<TSource, TResult> selector) =>
                new Tuple<TResult, T2>(selector(source.Item1), source.Item2);

        // General abstract functor definition of Tuple< , >: DotNet -> DotNet.
        public static IMorphism<Tuple<TSource, T2>, Tuple<TResult, T2>, DotNet> Select<TSource, TResult, T2>
            (this IMorphism<TSource, TResult, DotNet> selector) =>
                new DotNetMorphism<Tuple<TSource, T2>, Tuple<TResult, T2>>(source => source.Select(selector.Invoke));
    }

    // Impure.
    public static partial class TaskExtensions
    {
        public static Task<T> Task<T>
            (this T value, bool isNotStarted) => isNotStarted
                ? new Task<T>(() => value)
                : System.Threading.Tasks.Task.Run(() => value);
    }

    // Impure.
    public static partial class TaskExtensions
    {
        public static async Task<TResult> Select<TSource, TResult>
            (this Task<TSource> source, Func<TSource, TResult> selector) => selector(await source);
    }
}
