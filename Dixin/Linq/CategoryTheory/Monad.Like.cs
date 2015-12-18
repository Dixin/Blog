namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    // [Pure]
    public static partial class TupleExtensions
    {
        // Required by LINQ.
        public static Tuple<TResult> SelectMany<TSource, TSelector, TResult>
            (this Tuple<TSource> source,
             Func<TSource, Tuple<TSelector>> selector,
             Func<TSource, TSelector, TResult> resultSelector) =>
                new Tuple<TResult>(resultSelector(source.Item1, selector(source.Item1).Item1));

        // Not required, just for convenience.
        public static Tuple<TResult> SelectMany<TSource, TResult>
            (this Tuple<TSource> source, Func<TSource, Tuple<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class TupleExtensions
    {
        // μ: Tuple<Tuple<T> => Tuple<T>
        public static Tuple<TResult> Flatten<TResult>
            (this Tuple<Tuple<TResult>> source) => source.SelectMany(Functions.Id);

        // η: T -> Tuple<T> is already implemented previously as TupleExtensions.Tuple.

        // φ: Lazy<Tuple<T1>, Tuple<T2>> => Tuple<Defer<T1, T2>>
        public static Tuple<Lazy<T1, T2>> Binary2<T1, T2>
            (this Lazy<Tuple<T1>, Tuple<T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Tuple<TUnit> is already implemented previously with η: T -> Tuple<T>.

        // Select: (TSource -> TResult) -> (Tuple<TSource> -> Tuple<TResult>)
        public static Tuple<TResult> Select2<TSource, TResult>
            (this Tuple<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Tuple());
    }

    // Impure.
    public static partial class TaskExtensions
    {
        // Required by LINQ.
        public static async Task<TResult> SelectMany<TSource, TSelector, TResult>
            (this Task<TSource> source,
             Func<TSource, Task<TSelector>> selector,
             Func<TSource, TSelector, TResult> resultSelector) =>
                resultSelector(await source, await selector(await source));

        // Not required, just for convenience.
        public static Task<TResult> SelectMany<TSource, TResult>
            (this Task<TSource> source, Func<TSource, Task<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // Impure.
    public static partial class TaskExtensions
    {
        // μ: Task<Task<T> => Task<T>
        public static Task<TResult> Flatten<TResult>
            (this Task<Task<TResult>> source) => source.SelectMany(Functions.Id);

        // η: T -> Task<T> is already implemented previously as TaskExtensions.Task.

        // φ: Lazy<Task<T1>, Task<T2>> => Task<Defer<T1, T2>>
        public static Task<Lazy<T1, T2>> Binary2<T1, T2>
            (this Lazy<Task<T1>, Task<T2>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> Task<TUnit> is already implemented previously with η: T -> Task<T>.

        // Select: (TSource -> TResult) -> (Task<TSource> -> Task<TResult>)
        public static Task<TResult> Select2<TSource, TResult>
            (this Task<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Task());
    }

    // Impure.
    public static partial class TaskExtensions
    {
        // Required by LINQ.
        public static async Task<TResult> SelectMany<TSelector, TResult>
            (this Task source,
             Func<Unit, Task<TSelector>> selector,
             Func<Unit, TSelector, TResult> resultSelector)
        {
            await source;
            return resultSelector(null, await selector(null));
        }

        // Not required, just for convenience.
        public static Task<TResult> SelectMany<TResult>
            (this Task source, Func<Unit, Task<TResult>> selector) => source.SelectMany(selector, Functions.False);
    }

    // Impure.
    public static partial class TaskExtensions
    {
        // η: Unit -> Task.
        public static Task Task(Unit unit) => System.Threading.Tasks.Task.Run(() => { });

        // ι: TUnit -> Task is already implemented previously with η: Unit -> Task.

        // Select: (Unit -> TResult) -> (Task -> Task<TResult>)
        public static Task<TResult> Select<TResult>
            (this Task source, Func<Unit, TResult> selector) => source.SelectMany(value => selector(value).Task());
    }

    // [Pure]
    public static partial class QueryableExtensions
    {
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>
            (this IQueryable<TSource> source,
             Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector,
             Expression<Func<TSource, TCollection, TResult>> resultSelector) =>
                source.Provider.CreateQuery<TResult>(Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(
                        new Type[] { typeof(TSource), typeof(TCollection), typeof(TResult) }),
                    new Expression[]
                        {
                            source.Expression,
                            Expression.Quote(collectionSelector),
                            Expression.Quote(resultSelector)
                        }));

        public static IQueryable<TResult> SelectMany<TSource, TResult>
            (this IQueryable<TSource> source,
             Expression<Func<TSource, IEnumerable<TResult>>> selector) =>
                source.Provider.CreateQuery<TResult>(Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(
                        new Type[] { typeof(TSource), typeof(TResult) }),
                    new Expression[] { source.Expression, Expression.Quote(selector) }));
    }


    // Impure.
    public static class TaskQuery
    {
        public static async void Download()
        {
            Func<string, Task<string>> query = url =>
                from response in new HttpClient().GetAsync(url) // Returns Task<HttpResponseMessage>.
                from html in response.Content.ReadAsStringAsync() // Returns Task<string>.
                select html;
            string result = await query("http://weblogs.asp.net/dixin");
        }
    }
}
