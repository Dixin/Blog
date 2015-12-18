namespace Dixin.Linq.CategoryTheory.Obsolete
{
    using System;
    using System.Collections.Generic;

    using Microsoft.FSharp.Core;

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> Apply<TSource, TResult>(
            this IEnumerable<Func<TSource, TResult>> selectorFunctor, IEnumerable<TSource> source)
        {
            foreach (Func<TSource, TResult> selector in selectorFunctor)
            {
                foreach (TSource value in source)
                {
                    yield return selector(value);
                }
            }
        }

        public static IEnumerable<T> Enumerable<T>(this T value)
        {
            yield return value;
        }

        // φ: Lazy<IEnumerable<T1>, IEnumerable<T2>> => IEnumerable<Defer<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<IEnumerable<T1>, IEnumerable<T2>> binaryFunctor) =>
                new Func<T1, Func<T2, Lazy<T1, T2>>>(x => y => new Lazy<T1, T2>(x, y))
                    .Enumerable()
                    .Apply(binaryFunctor.Value1)
                    .Apply(binaryFunctor.Value2);

        // ι: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit
            (Unit unit) => unit.Enumerable();
    }
}

namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.FSharp.Core;

    // [Pure]
    public static partial class EnumerableExtensions
    {
        // φ: Lazy<IEnumerable<T1>, IEnumerable<T2>> => IEnumerable<Defer<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Binary<T1, T2>(
            this Lazy<IEnumerable<T1>, IEnumerable<T2>> binaryFunctor)
        {
            foreach (T1 value1 in binaryFunctor.Value1)
            {
                foreach (T2 value2 in binaryFunctor.Value2)
                {
                    yield return new Lazy<T1, T2>(value1, value2);
                }
            }
        }

        // ι: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit)
        {
            yield return unit;
        }
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> Apply<TSource, TResult>
            (this IEnumerable<Func<TSource, TResult>> selectorFunctor, IEnumerable<TSource> source) =>
                new Lazy<IEnumerable<Func<TSource, TResult>>, IEnumerable<TSource>>(selectorFunctor, source)
                    .Binary().Select(pair => pair.Value1(pair.Value2));

        public static IEnumerable<T> Enumerable<T>
            (this T value) => Unit(null).Select(unit => value);
    }

    [Pure]
    public static partial class EnumerableExtensions2
    {
        public static IEnumerable<TResult> ApplyWithZip<TSource, TResult>
            (this IEnumerable<Func<TSource, TResult>> selectorFunctor, IEnumerable<TSource> source) =>
                selectorFunctor
                    .Aggregate(
                        Enumerable.Empty<Func<TSource, TResult>>(),
                        (current, selector) => current.Concat(source.Select(sourceValue => selector)))
                    .Zip(
                        selectorFunctor.Aggregate(
                            Enumerable.Empty<TSource>(),
                            (current, selector) => current.Concat(source)),
                        (selector, value) => selector(value));

        public static IEnumerable<TResult> ApplyWithJoin<TSource, TResult>
            (this IEnumerable<Func<TSource, TResult>> selectorFunctor, IEnumerable<TSource> source) =>
                selectorFunctor.Join(
                    source,
                    selector => true,
                    value => true,
                    (selector, value) => selector(value),
                    EqualityComparer<bool>.Default);
    }

    // [Pure]
    public static partial class EnumerableExtensions2
    {
        public static IEnumerable<TResult> ApplyWithLinqJoin<TSource, TResult>
            (this IEnumerable<Func<TSource, TResult>> selectorFunctor, IEnumerable<TSource> source) =>
                from selector in selectorFunctor
                join value in source on true equals true // Cross join.
                select selector(value);
    }

    // [Pure]
    public static partial class LazyExtensions
    {
        public static Lazy<TResult> Apply<TSource, TResult>
            (this Lazy<Func<TSource, TResult>> selectorFunctor, Lazy<TSource> source) =>
                new Lazy<TResult>(() => selectorFunctor.Value(source.Value));

        public static Lazy<T> Lazy<T>
            (this T value) => new Lazy<T>(() => value);

        // φ: Lazy<Defer<T1>, Defer<T2>> => Defer<Defer<T1, T2>>
        public static Lazy<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<Lazy<T1>, Lazy<T2>> binaryFunctor) =>
                new Func<T1, Func<T2, Lazy<T1, T2>>>(x => y => new Lazy<T1, T2>(x, y))
                    .Lazy()
                    .Apply(binaryFunctor.Value1)
                    .Apply(binaryFunctor.Value2);

        // ι: Unit -> Lazy<Unit>
        public static Lazy<Unit> Unit
            (Unit unit) => unit.Lazy();
    }

    // [Pure]
    public static partial class FuncExtensions
    {
        public static Func<TResult> Apply<TSource, TResult>
            (this Func<Func<TSource, TResult>> selectorFunctor, Func<TSource> source) =>
                () => selectorFunctor()(source());

        public static Func<T> Func<T>
            (this T value) => () => value;

        // φ: Lazy<Func<T1>, Func<T2>> => Func<Defer<T1, T2>>
        public static Func<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<Func<T1>, Func<T2>> binaryFunctor) =>
                Func(new Func<T1, Func<T2, Lazy<T1, T2>>>(x => y => new Lazy<T1, T2>(x, y)))
                    .Apply(binaryFunctor.Value1)
                    .Apply(binaryFunctor.Value2);

        // ι: Unit -> Func<Unit>
        public static Func<Unit> Unit
            (Unit unit) => unit.Func();
    }

    // [Pure]
    public static partial class NullableExtensions
    {
        public static Nullable<TResult> Apply<TSource, TResult>
            (this Nullable<Func<TSource, TResult>> selectorFunctor, Nullable<TSource> source) =>
                new Nullable<TResult>(() => selectorFunctor.HasValue && source.HasValue ?
                    new Tuple<bool, TResult>(true, selectorFunctor.Value(source.Value)) :
                    new Tuple<bool, TResult>(false, default(TResult)));

        public static Nullable<T> Nullable<T>
            (this T value) => new Nullable<T>(() => Tuple.Create(true, value));

        // φ: Lazy<Nullable<T1>, Nullable<T2>> => Nullable<Defer<T1, T2>>
        public static Nullable<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<Nullable<T1>, Nullable<T2>> binaryFunctor) =>
                new Func<T1, Func<T2, Lazy<T1, T2>>>(x => y => new Lazy<T1, T2>(x, y))
                    .Nullable()
                    .Apply(binaryFunctor.Value1)
                    .Apply(binaryFunctor.Value2);

        // ι: Unit -> Nullable<Unit>
        public static Nullable<Unit> Unit
            (Unit unit) => unit.Nullable();
    }

    // [Pure]
    public static partial class TupleExtensions
    {
        public static Tuple<TResult> Apply<TSource, TResult>
            (this Tuple<Func<TSource, TResult>> selectorFunctor, Tuple<TSource> source) =>
                new Tuple<TResult>(selectorFunctor.Item1(source.Item1));

        public static Tuple<T> Tuple<T>
            (this T value) => new Tuple<T>(value);

        // φ: Lazy<Tuple<T1>, Tuple<T2>> => Tuple<Defer<T1, T2>>
        public static Tuple<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<Tuple<T1>, Tuple<T2>> binaryFunctor) =>
                new Func<T1, Func<T2, Lazy<T1, T2>>>(x => y => new Lazy<T1, T2>(x, y))
                    .Tuple()
                    .Apply(binaryFunctor.Value1)
                    .Apply(binaryFunctor.Value2);

        // ι: Unit -> Tuple<Unit>
        public static Tuple<Unit> Unit
            (Unit unit) => unit.Tuple();
    }

    // Impure.
    public static partial class TaskExtensions
    {
        public static async Task<TResult> Apply<TSource, TResult>
            (this Task<Func<TSource, TResult>> selectorFunctor, Task<TSource> source) =>
                (await selectorFunctor)(await source);

        public static Task<T> Task<T>
            (this T value) => System.Threading.Tasks.Task.FromResult(value);

        // φ: Lazy<Task<T1>, Task<T2>> => Task<Defer<T1, T2>>
        public static Task<Lazy<T1, T2>> Binary<T1, T2>
            (this Lazy<Task<T1>, Task<T2>> binaryFunctor) =>
                new Func<T1, Func<T2, Lazy<T1, T2>>>(x => y => new Lazy<T1, T2>(x, y))
                    .Task()
                    .Apply(binaryFunctor.Value1)
                    .Apply(binaryFunctor.Value2);

        // ι: Unit -> Func<Unit>
        public static Task<Unit> Unit
            (Unit unit) => unit.Task();
    }
}
