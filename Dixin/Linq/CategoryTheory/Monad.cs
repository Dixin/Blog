namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    using static Dixin.Linq.CategoryTheory.Functions;

#if DEMO
    public interface IMonad<TMonad<>> : IFunctor<TMonad<>>
    {
        // Select: (TSource -> TResult) -> (TMonad<TSource> -> TMonad<TResult>)
        // Func<TMonad<TSource>, TMonad<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Multiply: TMonad<TMonad<TSource>> -> TMonad<TSource>
        TMonad<TSource> Multiply<TSource>(TMonad<TMonad<TSource>> sourceWrapper);
        
        // Unit: TSource -> TMonad<TSource>
        // TMonad<TSource> Unit(TSource value);
    }

    public interface IMonad<TMonad<>> : IMonoidalFunctor<TMonad<>>
    {
        // Select: (TSource -> TResult) -> (TMonad<TSource> -> TMonad<TResult>)
        // Func<TMonad<TSource>, TMonad<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Multiply: Lazy<TMonad<T1>, TMonad<T2>> -> TMonad<Lazy<T1, T2>>
        // TMonad<Lazy<T1, T2>> Multiply<T1, T2>(Lazy<TMonad<T1>, TMonad<T2>> bifunctor);

        // Unit: Unit -> TMonad<Unit>
        // TMonad<Unit> Unit(Unit unit);

        // Multiply: TMonad<TMonad<TSource>> -> TMonad<TSource>
        TMonad<TSource> Multiply<TSource>(TMonad<TMonad<TSource>> sourceWrapper);
        
        // Unit: TSource -> TMonad<TSource>
        // TMonad<TSource> Unit(TSource value);
    }

    public static partial class Functions
    {
        public static Func<TSource, TMonad<TResult>> o<TMonad<>, TSource, TMiddle, TResult>( // After.
            this Func<TMiddle, TMonad<TResult>> selector2, Func<TSource, TMonad<TMiddle>> selector1) 
            where TMonad<> : IMonad<TMonad<>> =>
                value => selector1(value).Select(selector2).Multiply();
    }
#endif

    #region IEnumerable<>

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonad<IEnumerable<>>
    {
        // Multiply: IEnumerable<IEnumerable<TSource>> -> IEnumerable<TSource>
        public static IEnumerable<TSource> Multiply<TSource>(this IEnumerable<IEnumerable<TSource>> sourceWrapper)
        {
            foreach (IEnumerable<TSource> source in sourceWrapper)
            {
                foreach (TSource value in source)
                {
                    yield return value;
                }
            }
        }

        // Unit: TSource -> TMonad<TSource>
        public static IEnumerable<TSource> Unit<TSource>(TSource value)
        {
            yield return value;
        }
    }

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectMany<TSource, TSelector, TResult>(
                this IEnumerable<TSource> source,
                Func<TSource, IEnumerable<TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector)
            =>
            (from value in source select (from result in selector(value) select resultSelector(value, result))).Multiply();

        // Compiled to:
        // source
        //    .Select(value => selector(value).Select(result => resultSelector(value, result)))
        //    .Multiply();
    }

#if DEMO
    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: Lazy<IEnumerable<T1>, IEnumerable<T2>> => IEnumerable<Lazy<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Multiply<T1, T2>(
            this Lazy<IEnumerable<T1>, IEnumerable<T2>> bifunctor) =>
                (from value1 in bifunctor.Value1
                 select (from value2 in bifunctor.Value2
                         select value1.Lazy(value2))).Multiply();
                // bifunctor.Value1
                //    .Select(value1 => bifunctor.Value2.Select(value2 => value1.Lazy(value2)))
                //    .Multiply();

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit)) => Unit<Unit>(unit);
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IApplicativeFunctor<IEnumerable<>>
    {
        // Apply: (IEnumerable<TSource -> TResult>, IEnumerable<TSource>) -> IEnumerable<TResult>
        public static IEnumerable<TResult> Apply<TSource, TResult>(
            this IEnumerable<Func<TSource, TResult>> selectorWrapper, IEnumerable<TSource> source) =>
                (from selector in selectorWrapper
                 select (from value in source
                         select selector(value))).Multiply();
                // selectorWrapper
                //    .Select(selector => source.Select(value => selector(value)))
                //    .Multiply();

        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value) => Unit(value);
    }

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectMany<TSource, TSelector, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector)
        {
            foreach (TSource value in source)
            {
                foreach (TSelector result in selector(value))
                {
                    yield return resultSelector(value, result);
                }
            }
        }

        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value)
        {
            yield return value;
        }
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonad<IEnumerable<>>
    {
        // Multiply: IEnumerable<IEnumerable<TSource>> -> IEnumerable<TSource>
        public static IEnumerable<TSource> Multiply<TSource>(this IEnumerable<IEnumerable<TSource>> sourceWrapper) =>
            from source in sourceWrapper
            from value in source
            select value;        
            // Compiled to: 
            // sourceWrapper.SelectMany(value => value, (value, result) => result);
            // Or equivalently: 
            // sourceWrapper.SelectMany(Id, False);
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: Lazy<IEnumerable<T1>, IEnumerable<T2>> => IEnumerable<Lazy<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Multiply<T1, T2>(
            this Lazy<IEnumerable<T1>, IEnumerable<T2>> bifunctor) =>
                from value1 in bifunctor.Value1
                from value2 in bifunctor.Value2
                select value1.Lazy(value2);
                // bifunctor.Value1.SelectMany(value1 => bifunctor.Value2, (value1, value2) => value1.Lazy(value2));

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit)) => unit.Enumerable();
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IApplicativeFunctor<IEnumerable<>>
    {
        // Apply: (IEnumerable<TSource -> TResult>, IEnumerable<TSource>) -> IEnumerable<TResult>
        public static IEnumerable<TResult> Apply<TSource, TResult>(
            this IEnumerable<Func<TSource, TResult>> selectorWrapper, IEnumerable<TSource> source) =>
                from selector in selectorWrapper
                from value in source
                select selector(value);
                // selectorWrapper.SelectMany(selector => source, (selector, value) => selector(value));

        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value)
        {
            yield return value;
        }
    }

    public static partial class EnumerableExtensions // IEnumerable<> : IFunctor<IEnumerable<>>
    {
        // Select: (TSource -> TResult) -> (IEnumerable<TSource> -> IEnumerable<TResult>).
        public static Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                from value in source
                from result in value.Enumerable()
                select result;
                // source.SelectMany(value => value.Enumerable, False);
    }
#endif

    public static partial class EnumerableExtensions
    {
        internal static void MonoidLaws()
        {
            IEnumerable<int> enumerable = new int[] { 0, 1, 2, 3, 4 };
            // Left unit preservation: Unit(f).Multiply() == f.
            Unit(enumerable).Multiply().ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Right unit preservation: f == f.Select(Unit).Multiply().
            enumerable.Select(Unit).Multiply().ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Associativity preservation: f.Wrap().Multiply().Wrap().Multiply() == f.Wrap().Wrap().Multiply().Multiply().
            enumerable.Enumerable().Multiply().Enumerable().Multiply().ForEach(result => Trace.WriteLine(result));
            // 0 1 2 3 4
            enumerable.Enumerable().Enumerable().Multiply().Multiply().ForEach(result => Trace.WriteLine(result));
            // 0 1 2 3 4
        }

        internal static void MonadLaws()
        {
            IEnumerable<int> enumerable = new int[] { 0, 1, 2, 3, 4 };
            Func<int, IEnumerable<char>> selector = int32 => new string('*', int32);
            Func<int, IEnumerable<double>> selector1 = int32 => new double[] { int32 / 2D, Math.Sqrt(int32) };
            Func<double, IEnumerable<string>> selector2 =
                @double => new string[] { @double.ToString("0.0"), @double.ToString("0.00") };
            const int Value = 5;

            // Left unit: value.Wrap().SelectMany(selector) == selector(value).
            (from value in Value.Enumerable() from result in selector(value) select result).ForEach(
                result => Trace.WriteLine(result)); // * * * * *
            selector(Value).ForEach(result => Trace.WriteLine(result)); // * * * * *
            // Eight unit: f == f.SelectMany(Wrap).
            (from value in enumerable from result in value.Enumerable() select result).ForEach(
                result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Associativity f.SelectMany(selector1).SelectMany(selector2) == f.SelectMany(value => selector1(value).SelectMany(selector2)).
            (from value in enumerable from result1 in selector1(value) from result2 in selector2(result1) select result2)
                .ForEach(result => Trace.WriteLine(result));
            // 0.0 0.00 0.0 0.00
            // 0.5 0.50 1.0 1.00
            // 1.0 1.00 1.4 1.41
            // 1.5 1.50 1.7 1.73
            // 2.0 2.00 2.0 2.00
            (from value in enumerable
             from result in (from result1 in selector1(value) from result2 in selector2(result1) select result2)
             select result).ForEach(result => Trace.WriteLine(result));
            // 0.0 0.00 0.0 0.00
            // 0.5 0.50 1.0 1.00
            // 1.0 1.00 1.4 1.41
            // 1.5 1.50 1.7 1.73
            // 2.0 2.00 2.0 2.00
        }

        public static Func<TSource, IEnumerable<TResult>> o<TSource, TMiddle, TResult>(
            // After.
            this Func<TMiddle, IEnumerable<TResult>> selector2,
            Func<TSource, IEnumerable<TMiddle>> selector1) => value => selector1(value).Select(selector2).Multiply();

        internal static void LawsWithKleisliComposition()
        {
            Func<bool, IEnumerable<int>> selector1 =
                boolean => boolean ? new int[] { 0, 1, 2, 3, 4 } : new int[] { 5, 6, 7, 8, 9 };
            Func<int, IEnumerable<double>> selector2 = int32 => new double[] { int32 / 2D, Math.Sqrt(int32) };
            Func<double, IEnumerable<string>> selector3 =
                @double => new string[] { @double.ToString("0.0"), @double.ToString("0.00") };

            // Left unit: Unit.o(selector1) == selector.
            selector1(true).ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            new Func<int, IEnumerable<int>>(Unit).o(selector1)(true).ForEach(result => Trace.WriteLine(result));
            // 0 1 2 3 4
            // Right unit: selector.o(Unit) == selector.
            selector1.o(new Func<bool, IEnumerable<bool>>(Unit))(false).ForEach(result => Trace.WriteLine(result));
            // 5 6 7 8 9
            selector1(false).ForEach(result => Trace.WriteLine(result)); // 5 6 7 8 9
            // Associativity selector3.o(selector2).o(selector1) == selector3.o(selector2.o(selector1)).
            selector3.o(selector2).o(selector1)(true).ForEach(result => Trace.WriteLine(result));
            // 0.0 0.00 0.0 0.00
            // 0.5 0.50 1.0 1.00
            // 1.0 1.00 1.4 1.41
            // 1.5 1.50 1.7 1.73
            // 2.0 2.00 2.0 2.00
            selector3.o(selector2.o(selector1))(true).ForEach(result => Trace.WriteLine(result));
            // 0.0 0.00 0.0 0.00
            // 0.5 0.50 1.0 1.00
            // 1.0 1.00 1.4 1.41
            // 1.5 1.50 1.7 1.73
            // 2.0 2.00 2.0 2.00
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<T> : IMonad<Lazy<>>
    {
        // Multiply: Lazy<Lazy<TSource> -> Lazy<TSource>
        public static Lazy<TSource> Multiply<TSource>(this Lazy<Lazy<TSource>> sourceWrapper)
            => sourceWrapper.SelectMany(Id, False);

        // Unit: TSource -> Lazy<TSource>
        public static Lazy<TSource> Unit<TSource>(TSource value) => Lazy(value);

        public static Lazy<TResult> SelectMany<TSource, TSelector, TResult>(
                this Lazy<TSource> source,
                Func<TSource, Lazy<TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector)
            => new Lazy<TResult>(() => resultSelector(source.Value, selector(source.Value).Value));
    }

    #endregion

    #region Func<>

    public static partial class FuncExtensions // Func<T> : IMonad<Func<>>
    {
        // Multiply: Func<Func<T> -> Func<T>
        public static Func<TSource> Multiply<TSource>(this Func<Func<TSource>> source) => source.SelectMany(Id, False);

        // Unit: Unit -> Func<Unit>
        public static Func<TSource> Unit<TSource>(TSource value) => Func(value);

        public static Func<TResult> SelectMany<TSource, TSelector, TResult>(
            this Func<TSource> source,
            Func<TSource, Func<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => 
                () =>
                {
                    TSource value = source();
                    return resultSelector(value, selector(value)());
                };
    }

    public static partial class FuncExtensions
    {
        internal static async void Download()
        {
            using (WebClient webClient = new WebClient())
            {
                Func<Task> query = from url in new Func<string>(Console.ReadLine)
                                   from downloadTask in
                                       new Func<Task<string>>(() => webClient.DownloadStringTaskAsync(new Uri(url)))
                                   from consoleTask in new Func<Task>(async () => Console.WriteLine(await downloadTask))
                                   select consoleTask;
                await query();
            }
        }
    }

    #endregion

    #region Func<T,>

    public static partial class FuncExtensions // Func<T, TResult> : IMonad<Func<T,>>
    {
        // Multiply: Func<T, Func<T, TSource> -> Func<T, TSource>
        public static Func<T, TSource> Multiply<T, TSource>(this Func<T, Func<T, TSource>> source)
            => source.SelectMany(Id, False);

        // Unit: TSource -> Func<T, TSource>
        public static Func<T, TSource> Unit<T, TSource>(TSource value) => Func<T, TSource>(value);

        public static Func<T, TResult> SelectMany<T, TSource, TSelector, TResult>(
            this Func<T, TSource> source,
            Func<TSource, Func<T, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => 
                value =>
                {
                    TSource result = source(value);
                    return resultSelector(result, selector(result)(value));
                };
    }

    #endregion

    #region Optional<>

    public static partial class OptionalExtensions // Optional<T> : IMonad<Optional<>>
    {
        // Multiply: Optional<Optional<TSource> -> Optional<TSource>
        public static Optional<TSource> Multiply<TSource>(this Optional<Optional<TSource>> source)
            => source.SelectMany(Id, False);

        // Unit: TSource -> Optional<TSource>
        public static Optional<TSource> Unit<TSource>(TSource value) => Optional(value);

        public static Optional<TResult> SelectMany<TSource, TSelector, TResult>(
            this Optional<TSource> source,
            Func<TSource, Optional<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => 
                new Optional<TResult>(() =>
                {
                    if (source.HasValue)
                    {
                        Optional<TSelector> result = selector(source.Value);
                        if (result.HasValue)
                        {
                            return true.Tuple(resultSelector(source.Value, result.Value));
                        }
                    }
                    return false.Tuple(default(TResult));
                });
    }

    #endregion

    #region Tuple<>

    public static partial class TupleExtensions // Tuple<T> : IMonad<Tuple<>>
    {
        // Multiply: Tuple<Tuple<TSource> -> Tuple<TSource>
        public static Tuple<TSource> Multiply<TSource>(this Tuple<Tuple<TSource>> source) => source.SelectMany(Id, False);

        // Unit: TSource -> Tuple<TSource>
        public static Tuple<TSource> Unit<TSource>(TSource value) => Tuple(value);

        public static Tuple<TResult> SelectMany<TSource, TSelector, TResult>(
            this Tuple<TSource> source,
            Func<TSource, Tuple<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => 
                new Tuple<TResult>(resultSelector(source.Item1, selector(source.Item1).Item1)); // Immediate execution.
    }

    #endregion

    #region Tuple<T,>

    public static partial class TupleExtensions // Tuple<T, TResult> : IMonad<Tuple<T,>>
    {
        // Multiply: Tuple<T, Tuple<T, TSource> -> Tuple<T, TSource>
        public static Tuple<T, TSource> Multiply<T, TSource>(this Tuple<T, Tuple<T, TSource>> source)
            => source.SelectMany(Id, False);

        // Unit: TSource -> Tuple<T, TSource>
        public static Tuple<T, TSource> Unit<T, TSource>(TSource value) => Tuple<T, TSource>(value);

        public static Tuple<T, TResult> SelectMany<T, TSource, TSelector, TResult>(
            this Tuple<T, TSource> source,
            Func<TSource, Tuple<T, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => 
                source.Item1.Tuple(resultSelector(source.Item2, selector(source.Item2).Item2)); // Immediate execution.
    }

    public static partial class TupleExtensions
    {
        internal static void MonoidLaws()
        {
            Tuple<string, int> tuple = "a".Tuple(1);
            // Left unit preservation: Unit(f).Multiply() == f.
            Trace.WriteLine(Unit<string, Tuple<string, int>>(tuple).Multiply()); // (, 1)
            // Right unit preservation: f == f.Select(Unit).Multiply().
            Trace.WriteLine(tuple.Select(Unit<string, int>).Multiply()); // (a, 1)
            // Associativity preservation: f.Wrap().Multiply().Wrap().Multiply() == f.Wrap().Wrap().Multiply().Multiply().
            Trace.WriteLine(
                tuple.Tuple<string, Tuple<string, int>>().Multiply().Tuple<string, Tuple<string, int>>().Multiply());
            // (, 1)
            Trace.WriteLine(
                tuple.Tuple<string, Tuple<string, int>>()
                    .Tuple<string, Tuple<string, Tuple<string, int>>>()
                    .Multiply()
                    .Multiply()); // (, 1)
        }

        internal static void MonadLaws()
        {
            Tuple<string, int> enumerable = "a".Tuple(1);
            Func<int, Tuple<string, char>> selector = int32 => "b".Tuple('@');
            Func<int, Tuple<string, double>> selector1 = int32 => "c".Tuple(Math.Sqrt(int32));
            Func<double, Tuple<string, string>> selector2 = @double => "d".Tuple(@double.ToString("0.00"));
            const int Value = 5;

            // Left unit: value.Wrap().SelectMany(selector) == selector(value).
            Trace.WriteLine(from value in Value.Tuple<string, int>() from result in selector(value) select result);
            // (, @)
            Trace.WriteLine(selector(Value)); // (b, @)
            // Eight unit: f == f.SelectMany(Wrap).
            Trace.WriteLine(from value in enumerable from result in value.Tuple<string, int>() select result); // (a, 1)
            // Associativity f.SelectMany(selector1).SelectMany(selector2) == f.SelectMany(value => selector1(value).SelectMany(selector2)).
            Trace.WriteLine(
                from value in enumerable
                from result1 in selector1(value)
                from result2 in selector2(result1)
                select result2); // (a, 1.00)
            Trace.WriteLine(
                from value in enumerable
                from result in (from result1 in selector1(value) from result2 in selector2(result1) select result2)
                select result); // (a, 1.00)
        }
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<T> : IMonad<Task<>>
    {
        // μ: Task<Task<T> => Task<T>
        public static Task<TResult> Multiply<TResult>(this Task<Task<TResult>> source) => source.SelectMany(Id, False);

        // Unit: TSource -> Task<TSource>
        public static Task<TSource> Unit<TSource>(TSource value) => Task(value);

        public static async Task<TResult> SelectMany<TSource, TSelector, TResult>(
                this Task<TSource> source,
                Func<TSource, Task<TSelector>> selector,
                Func<TSource, TSelector, TResult> resultSelector)
            => resultSelector(await source, await selector(await source));
    }

    public static partial class TaskExtensions
    {
        internal static async void Download(string url)
        {
            Task<string> query = from response in new HttpClient().GetAsync(url)
                                     // Return Task<HttpResponseMessage>.
                                 from html in response.Content.ReadAsStringAsync()
                                     // Return Task<string>.
                                 select html;
            string result = await query;
        }
    }

    #endregion

    public static partial class TaskExtensions
    {
        public static async Task<TResult> SelectMany<TSelector, TResult>(
            this Task source,
            Func<Unit, Task<TSelector>> selector,
            Func<Unit, TSelector, TResult> resultSelector)
        {
            await source;
            return resultSelector(default(Unit), await selector(default(Unit)));
        }

        // Select: (Unit -> TResult) -> (Task -> Task<TResult>)
        public static Task<TResult> Select<TResult>(this Task source, Func<Unit, TResult> selector)
            => source.SelectMany(value => selector(value).Task(), False);
    }

    public static partial class QueryableExtensions
    {
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
                this IQueryable<TSource> source,
                Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector,
                Expression<Func<TSource, TCollection, TResult>> resultSelector)
            =>
            source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(
                        typeof(TSource),
                        typeof(TCollection),
                        typeof(TResult)),
                    new Expression[]
                            { source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector) }));
    }
}
