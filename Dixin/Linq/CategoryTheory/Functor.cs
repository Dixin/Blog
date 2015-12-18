namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    [Pure]
    public static partial class EnumerableExtensions
    {
        // C# specific functor pattern.
        public static IEnumerable<TResult> Select<TSource, TResult>( // Extension
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }

        // General abstract functor definition for IEnumerable<>: DotNet -> DotNet.
        public static IMorphism<IEnumerable<TSource>, IEnumerable<TResult>, DotNet> Select<TSource, TResult>
            (this IMorphism<TSource, TResult, DotNet> selector) =>
                new DotNetMorphism<IEnumerable<TSource>, IEnumerable<TResult>>(source => source.Select(selector.Invoke));
    }

    [Pure]
    public static partial class LazyExtensions
    {
        // C# specific functor pattern.
        public static Lazy<TResult> Select<TSource, TResult>
            (this Lazy<TSource> source, Func<TSource, TResult> selector) =>
                new Lazy<TResult>(() => selector(source.Value));

        // General abstract functor definition for Lazy<>: DotNet -> DotNet.
        public static IMorphism<Lazy<TSource>, Lazy<TResult>, DotNet> Select<TSource, TResult>
            (/* this */ IMorphism<TSource, TResult, DotNet> selector) =>
                new DotNetMorphism<Lazy<TSource>, Lazy<TResult>>(source => source.Select(selector.Invoke));
    }

    [Pure]
    public static partial class FuncExtensions
    {
        public static Func<TResult> Select<TSource, TResult>
            (this Func<TSource> source, Func<TSource, TResult> selector) => () => selector(source());

        // General abstract functor definition for Func<>: DotNet -> DotNet.
        public static IMorphism<Func<TSource>, Func<TResult>, DotNet> Select<TSource, TResult>
            (/* this */ IMorphism<TSource, TResult, DotNet> selector) =>
                new DotNetMorphism<Func<TSource>, Func<TResult>>(source => source.Select(selector.Invoke));
    }

    [Pure]
    public static partial class NullableExtensions
    {
        // C# specific functor pattern.
        public static Nullable<TResult> Select<TSource, TResult>
            (this Nullable<TSource> source, Func<TSource, TResult> selector) =>
                new Nullable<TResult>(() => source.HasValue
                        ? Tuple.Create(true, selector(source.Value))
                        : Tuple.Create(false, default(TResult)));

        // General abstract functor definition.
        public static IMorphism<Nullable<TSource>, Nullable<TResult>, DotNet> Select<TSource, TResult>
            (/* this */ IMorphism<TSource, TResult, DotNet> selector) =>
                new DotNetMorphism<Nullable<TSource>, Nullable<TResult>>(source => source.Select(selector.Invoke));
    }

    [Pure]
    public static partial class QueryableExtensions
    {
        public static IQueryable<TResult> Select<TSource, TResult>
            (this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) =>
                source.Provider.CreateQuery<TResult>(Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(
                        new Type[] { typeof(TSource), typeof(TResult) }),
                    new Expression[] { source.Expression, Expression.Quote(selector) }));
    }

    // [Pure]
    public static partial class FuncExtensions
    {
        public static Func<TSourceArg, TResult> Select<TSourceArg, TSource, TResult>
            (this Func<TSourceArg, TSource> source, Func<TSource, TResult> selector) => arg => selector(source(arg));
    }

    // [Pure]
    public static partial class FuncExtensions
    {
        public static Func<TSource, TResult> Select2<TSource, TMiddle, TResult>
            (this Func<TSource, TMiddle> source, Func<TMiddle, TResult> selector) => selector.o(source);
    }
}
