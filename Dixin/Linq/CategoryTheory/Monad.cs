namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

#if DEMO
// Cannot be compiled.
    public partial interface IMonad<TMonad<>> : IFunctor<TMonad<>> where TMonad<> : IMonad<TMonad<>>
    {
        // From IFunctor<TMonad<>>:
        // Select: (TSource -> TResult) -> (TMonad<TSource> -> TMonad<TResult>)
        // Func<TMonad<TSource>, TMonad<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Multiply: TMonad<TMonad<TSource>> -> TMonad<TSource>
        TMonad<TSource> Multiply<TSource>(TMonad<TMonad<TSource>> sourceWrapper);
        
        // Unit: TSource -> TMonad<TSource>
        TMonad<TSource> Unit<TSource>(TSource value);
    }

    public partial interface IMonad<TMonad<>> where TMonad<> : IMonad<TMonad<>>
    {
        // SelectMany: (TMonad<TSource>, TSource -> TMonad<TSelector>, (TSource, TSelector) -> TResult) -> TMonad<TResult>
        TMonad<TResult> SelectMany<TSource, TSelector, TResult>(
            TMonad<TSource> source,
            Func<TSource, TMonad<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector);

        // Wrap: TSource -> IEnumerable<TSource>
        TMonad<TSource> Wrap<TSource>(TSource value);
    }

    // Cannot be compiled.
    public static partial class FuncExtensions
    {
        public static Func<TSource, TMonad<TResult>> o<TMonad<>, TSource, TMiddle, TResult>( // After.
            this Func<TMiddle, TMonad<TResult>> selector2, 
            Func<TSource, TMonad<TMiddle>> selector1) where TMonad<> : IMonad<TMonad<>> =>
                value => selector1(value).Select(selector2).Multiply();
    }

    // Cannot be compiled.
    public static partial class MonadExtensions // (SelectMany, Wrap) implements (Multiply, Unit).
    {
        // Multiply: (TMonad<T1>, TMonad<T2>) => TMonad<Tuple<T1, T2>>
        public static TMonad<Tuple<T1, T2>> Multiply<TMonad<>, T1, T2>(
            this TMonad<T1> source1, TMonad<T2> source2) where TMonad<> : IMonad<TMonad<>> =>
                from value1 in source1
                from value2 in source2
                select value1.Tuple(value2);
                // source1.SelectMany(value1 => source2 (value1, value2) => value1.Tuple(value2));

        // Unit: Unit -> TMonad<Unit>
        public static TMonad<Unit> Unit<TMonad<>>(
            Unit unit = default(Unit)) where TMonad<> : IMonad<TMonad<>> => unit.Wrap();
    }

    // Cannot be compiled.
    public static partial class MonadExtensions // (SelectMany, Wrap) implements (Apply, Wrap).
    {
        // Apply: (TMonad<TSource -> TResult>, TMonad<TSource>) -> TMonad<TResult>
        public static TMonad<TResult> Apply<TMonad<>, TSource, TResult>(
            this TMonad<Func<TSource, TResult>> selectorWrapper, 
            TMonad<TSource> source) where TMonad<> : IMonad<TMonad<>> =>
                from selector in selectorWrapper
                from value in source
                select selector(value);
                // selectorWrapper.SelectMany(selector => source, (selector, value) => selector(value));

        // Monad's Wrap is identical to applicative functor's Wrap.
    }

    // Cannot be compiled.
    public static class MonadExtensions // Monad (Multiply, Unit) implements monoidal functor (Multiply, Unit).
    {
        // Multiply: (TMonad<T1>, TMonad<T2>) => TMonad<Tuple<T1, T2>>
        public static TMonad<Tuple<T1, T2>> Multiply<TMonad<>, T1, T2>(
            this TMonad<T1> source1, TMonad<T2> source2) where TMonad<> : IMonad<TMonad<>> =>
                (from value1 in source1
                 select (from value2 in source2
                         select value1.Tuple(value2))).Multiply();
                // source1.Select(value1 => source2.Select(value2 => value1.Tuple(value2))).Multiply();

        // Unit: Unit -> TMonad<Unit>
        public static TMonad<Unit> Unit<TMonad<>>(Unit unit = default(Unit)) where TMonad<> : IMonad<TMonad<>> => 
            TMonad<Unit>.Unit<Unit>(unit);
    }

    // Cannot be compiled.
    public static partial class MonadExtensions // Monad (Multiply, Unit) implements applicative functor (Apply, Wrap).
    {
        // Apply: (TMonad<TSource -> TResult>, TMonad<TSource>) -> TMonad<TResult>
        public static TMonad<TResult> Apply<TMonad<>, TSource, TResult>(
            this TMonad<Func<TSource, TResult>> selectorWrapper, 
            TMonad<TSource> source)  where TMonad<> : IMonad<TMonad<>> =>
                (from selector in selectorWrapper
                 select (from value in source
                         select selector(value))).Multiply();
                // selectorWrapper.Select(selector => source.Select(value => selector(value))).Multiply();

        // Wrap: TSource -> TMonad<TSource>
        public static TMonad<TSource> Wrap<TMonad<>, TSource>(
            this TSource value) where TMonad<> : IMonad<TMonad<>> => TMonad<TSource>.Unit<TSource>(value);
    }
    
    // Cannot be compiled.
    public partial interface IMonad<TMonad<>> : IMonoidalFunctor<TMonad<>>, IApplicativeFunctor<TMonad<>>
    {
    }

    // Cannot be compiled.
    public static partial class MonadExtensions
    {
        internal static void Workflow<TMonad<>, T1, T2, T3, T4, TResult>( // Non generic TMonad can work too.
            Func<TMonad<T1>> operation1,
            Func<TMonad<T2>> operation2,
            Func<TMonad<T3>> operation3,
            Func<TMonad<T4>> operation4,
            Func<T1, T2, T3, T4, TResult> resultSelector) where TMonad<> : IMonad<TMonad<>>
        {
            TMonad<TResult> query = from /* T1 */ value1 in /* TMonad<T1> */ operation1()
                                    from /* T2 */ value2 in /* TMonad<T1> */ operation2()
                                    from /* T3 */ value3 in /* TMonad<T1> */ operation3()
                                    from /* T4 */ value4 in /* TMonad<T1> */ operation4()
                                    select /* TResult */ resultSelector(value1, value2, value3, value4); // Define query.
        }
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

        // Unit: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Unit<TSource>(TSource value)
        {
            yield return value;
        }
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonad<IEnumerable<>>
    {
        // SelectMany: (IEnumerable<TSource>, TSource -> IEnumerable<TSelector>, (TSource, TSelector) -> TResult) -> IEnumerable<TResult>
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

#if DEMO
        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value)
        {
            yield return value;
        }
    }

    public static partial class EnumerableExtensions // (Select, Multiply, Unit) implements (SelectMany, Wrap).
    {
        // SelectMany: (IEnumerable<TSource>, TSource -> IEnumerable<TSelector>, (TSource, TSelector) -> TResult) -> IEnumerable<TResult>
        public static IEnumerable<TResult> SelectMany<TSource, TSelector, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                (from value in source
                 select (from result in selector(value)
                         select resultSelector(value, result))).Multiply();
                // Compiled to:
                // source.Select(value => selector(value).Select(result => resultSelector(value, result))).Multiply();

        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value) => Unit(value);
    }

    public static partial class EnumerableExtensions // (SelectMany, Wrap) implements (Select, Multiply, Unit).
    {
        // Select: (TSource -> TResult) -> (IEnumerable<TSource> -> IEnumerable<TResult>).
        public static Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                from value in source
                from result in value.Enumerable()
                select result;
                // source.SelectMany(Enumerable, (result, value) => value);

        // Multiply: IEnumerable<IEnumerable<TSource>> -> IEnumerable<TSource>
        public static IEnumerable<TSource> Multiply<TSource>(this IEnumerable<IEnumerable<TSource>> sourceWrapper) =>
            from source in sourceWrapper
            from value in source
            select value;
            // sourceWrapper.SelectMany(source => source, (source, value) => value);

        // Unit: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Unit<TSource>(TSource value) => value.Enumerable();
#endif
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

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: (IEnumerable<T1>, IEnumerable<T2>) => IEnumerable<Tuple<T1, T2>>
        public static IEnumerable<Tuple<T1, T2>> Multiply<T1, T2>(
            this IEnumerable<T1> source1, IEnumerable<T2> source2) =>
                from value1 in source1
                from value2 in source2
                select value1.Tuple(value2);
                // source1.SelectMany(value1 => source2 (value1, value2) => value1.Tuple(value2));

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
#endif

    public static partial class EnumerableExtensions
    {
        internal static void MonoidLaws()
        {
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };

            // Associativity preservation: source.Wrap().Multiply().Wrap().Multiply() == source.Wrap().Wrap().Multiply().Multiply().
            source.Enumerable().Multiply().Enumerable().Multiply().ForEach(result => Trace.WriteLine(result));
            // 0 1 2 3 4
            source.Enumerable().Enumerable().Multiply().Multiply().ForEach(result => Trace.WriteLine(result));
            // 0 1 2 3 4
            // Left unit preservation: Unit(source).Multiply() == f.
            Unit(source).Multiply().ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Right unit preservation: source == source.Select(Unit).Multiply().
            source.Select(Unit).Multiply().ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
        }

        internal static void Workflow<T1, T2, T3, T4>(
            Func<IEnumerable<T1>> source1,
            Func<IEnumerable<T2>> source2,
            Func<IEnumerable<T3>> source3,
            Func<T1, T2, T3, IEnumerable<T4>> source4)
        {
            IEnumerable<T4> query = from value1 in source1()
                                    from value2 in source2()
                                    from value3 in source3()
                                    from value4 in source4(value1, value2, value3)
                                    select value4; // Define query.
            query.ForEach(result => Trace.WriteLine(result)); // Execute query.
        }

        internal static void CompiledWorkflow<T1, T2, T3, T4>(
            Func<IEnumerable<T1>> source1,
            Func<IEnumerable<T2>> source2,
            Func<IEnumerable<T3>> source3,
            Func<T1, T2, T3, IEnumerable<T4>> source4)
        {
            IEnumerable<T4> query =
                source1()
                    .SelectMany(value1 => source2(), (value1, value2) => new { Value1 = value1, Value2 = value2 })
                    .SelectMany(result2 => source3(), (result2, value3) => new { Result2 = result2, Value3 = value3 })
                    .SelectMany(
                        result3 => source4(result3.Result2.Value1, result3.Result2.Value2, result3.Value3),
                        (result3, value4) => value4); // Define query.
            query.ForEach(result => Trace.WriteLine(result)); // Execute query.
        }

        internal static void MonadLaws()
        {
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };
            Func<int, IEnumerable<char>> selector = int32 => new string('*', int32);
            Func<int, IEnumerable<double>> selector1 = int32 => new double[] { int32 / 2D, Math.Sqrt(int32) };
            Func<double, IEnumerable<string>> selector2 =
                @double => new string[] { @double.ToString("0.0"), @double.ToString("0.00") };
            const int Value = 5;

            // Associativity: source.SelectMany(selector1).SelectMany(selector2) == source.SelectMany(value => selector1(value).SelectMany(selector2)).
            (from value in source
             from result1 in selector1(value)
             from result2 in selector2(result1)
             select result2).ForEach(result => Trace.WriteLine(result));
                // 0.0 0.00 0.0 0.00
                // 0.5 0.50 1.0 1.00
                // 1.0 1.00 1.4 1.41
                // 1.5 1.50 1.7 1.73
                // 2.0 2.00 2.0 2.00
            (from value in source
             from result in (from result1 in selector1(value)
                             from result2 in selector2(result1)
                             select result2)
             select result).ForEach(result => Trace.WriteLine(result));
                // 0.0 0.00 0.0 0.00
                // 0.5 0.50 1.0 1.00
                // 1.0 1.00 1.4 1.41
                // 1.5 1.50 1.7 1.73
                // 2.0 2.00 2.0 2.00
            // Left unit: value.Wrap().SelectMany(selector) == selector(value).
            (from value in Value.Enumerable()
             from result in selector(value)
             select result).ForEach(
                result => Trace.WriteLine(result)); // * * * * *
            selector(Value).ForEach(result => Trace.WriteLine(result)); // * * * * *
            // Right unit: source == source.SelectMany(Wrap).
            (from value in source
             from result in value.Enumerable()
             select result).ForEach(
                result => Trace.WriteLine(result)); // 0 1 2 3 4
        }

        public static Func<TSource, IEnumerable<TResult>> o<TSource, TMiddle, TResult>( // After.
            this Func<TMiddle, IEnumerable<TResult>> selector2,
            Func<TSource, IEnumerable<TMiddle>> selector1) => 
                value => selector1(value).SelectMany(selector2, (result1, result2) => result2);
                // Equivalent to:
                // value => selector1(value).Select(selector2).Multiply();

        internal static void KleisliComposition()
        {
            Func<bool, IEnumerable<int>> selector1 =
                boolean => boolean ? new int[] { 0, 1, 2, 3, 4 } : new int[] { 5, 6, 7, 8, 9 };
            Func<int, IEnumerable<double>> selector2 = int32 => new double[] { int32 / 2D, Math.Sqrt(int32) };
            Func<double, IEnumerable<string>> selector3 =
                @double => new string[] { @double.ToString("0.0"), @double.ToString("0.00") };

            // Associativity: selector3.o(selector2).o(selector1) == selector3.o(selector2.o(selector1)).
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
            // Left unit: Unit.o(selector) == selector.
            Func<int, IEnumerable<int>> leftUnit = Enumerable;
            leftUnit.o(selector1)(true).ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            selector1(true).ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Right unit: selector == selector.o(Unit).
            selector1(false).ForEach(result => Trace.WriteLine(result)); // 5 6 7 8 9
            Func<bool, IEnumerable<bool>> rightUnit = Enumerable;
            selector1.o(rightUnit)(false).ForEach(result => Trace.WriteLine(result)); // 5 6 7 8 9
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<T> : IMonad<Lazy<>>
    {
        // Multiply: Lazy<Lazy<TSource> -> Lazy<TSource>
        public static Lazy<TSource> Multiply<TSource>(this Lazy<Lazy<TSource>> sourceWrapper) =>
            sourceWrapper.SelectMany(source => source, (source, value) => value);

        // Unit: TSource -> Lazy<TSource>
        public static Lazy<TSource> Unit<TSource>(TSource value) => Lazy(value);

        // SelectMany: (Lazy<TSource>, TSource -> Lazy<TSelector>, (TSource, TSelector) -> TResult) -> Lazy<TResult>
        public static Lazy<TResult> SelectMany<TSource, TSelector, TResult>(
            this Lazy<TSource> source,
            Func<TSource, Lazy<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => 
                new Lazy<TResult>(() => resultSelector(source.Value, selector(source.Value).Value));
    }

    public static partial class LazyExtensions
    {
        internal static void Workflow()
        {
            Lazy<string> query = from filePath in new Lazy<string>(Console.ReadLine)
                                 from encodingName in new Lazy<string>(Console.ReadLine)
                                 from encoding in new Lazy<Encoding>(() => Encoding.GetEncoding(encodingName))
                                 from fileContent in new Lazy<string>(() => File.ReadAllText(filePath, encoding))
                                 select fileContent; // Define query.
            string result = query.Value; // Execute query.
        }
    }

    #endregion

    #region Func<>

    public static partial class FuncExtensions // Func<T> : IMonad<Func<>>
    {
        // Multiply: Func<Func<T> -> Func<T>
        public static Func<TSource> Multiply<TSource>(this Func<Func<TSource>> sourceWrapper) => 
            sourceWrapper.SelectMany(source => source, (source, value) => value);

        // Unit: Unit -> Func<Unit>
        public static Func<TSource> Unit<TSource>(TSource value) => Func(value);

        // SelectMany: (Func<TSource>, TSource -> Func<TSelector>, (TSource, TSelector) -> TResult) -> Func<TResult>
        public static Func<TResult> SelectMany<TSource, TSelector, TResult>(
            this Func<TSource> source,
            Func<TSource, Func<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => () =>
            {
                TSource value = source();
                return resultSelector(value, selector(value)());
            };
    }

    public static partial class FuncExtensions
    {
        internal static void Workflow()
        {
            Func<string> query = from filePath in new Func<string>(Console.ReadLine)
                                 from encodingName in new Func<string>(Console.ReadLine)
                                 from encoding in new Func<Encoding>(() => Encoding.GetEncoding(encodingName))
                                 from fileContent in new Func<string>(() => File.ReadAllText(filePath, encoding))
                                 select fileContent; // Define query.
            string result = query(); // Execute query.
        }
    }

    #endregion

    #region Func<T,>

    public static partial class FuncExtensions // Func<T, TResult> : IMonad<Func<T,>>
    {
        // Multiply: Func<T, Func<T, TSource> -> Func<T, TSource>
        public static Func<T, TSource> Multiply<T, TSource>(this Func<T, Func<T, TSource>> sourceWrapper) =>
            sourceWrapper.SelectMany(source => source, (source, value) => value);

        // Unit: TSource -> Func<T, TSource>
        public static Func<T, TSource> Unit<T, TSource>(TSource value) => Func<T, TSource>(value);

        // SelectMany: (Func<T, TSource>, TSource -> Func<T, TSelector>, (TSource, TSelector) -> TResult) -> Func<T, TResult>
        public static Func<T, TResult> SelectMany<T, TSource, TSelector, TResult>(
            this Func<T, TSource> source,
            Func<TSource, Func<T, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => value =>
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
        public static Optional<TSource> Multiply<TSource>(this Optional<Optional<TSource>> sourceWrapper) =>
            sourceWrapper.SelectMany(source => source, (source, value) => value);

        // Unit: TSource -> Optional<TSource>
        public static Optional<TSource> Unit<TSource>(TSource value) => Optional(value);

        // SelectMany: (Optional<TSource>, TSource -> Optional<TSelector>, (TSource, TSelector) -> TResult) -> Optional<TResult>
        public static Optional<TResult> SelectMany<TSource, TSelector, TResult>(
            this Optional<TSource> source,
            Func<TSource, Optional<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => new Optional<TResult>(() =>
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

    public static partial class OptionalExtensions
    {
        internal static void Workflow()
        {
            string input;
            Optional<string> query =
                from filePath in new Optional<string>(() => string.IsNullOrWhiteSpace(input = Console.ReadLine())
                    ? false.Tuple(default(string)) : true.Tuple(input))
                from encodingName in new Optional<string>(() => string.IsNullOrWhiteSpace(input = Console.ReadLine())
                    ? false.Tuple(default(string)) : true.Tuple(input))
                from encoding in new Optional<Encoding>(() =>
                    {
                        try
                        {
                            return true.Tuple(Encoding.GetEncoding(encodingName));
                        }
                        catch (ArgumentException)
                        {
                            return false.Tuple(default(Encoding));
                        }
                    })
                from fileContent in new Optional<string>(() => File.Exists(filePath)
                    ? true.Tuple(File.ReadAllText(filePath, encoding)) : false.Tuple(default(string)))
                select fileContent; // Define query.
            if (query.HasValue) // Execute query.
            {
                string result = query.Value;
            }
        }
    }

    #endregion

    #region Tuple<>

    public static partial class TupleExtensions // Tuple<T> : IMonad<Tuple<>>
    {
        // Multiply: Tuple<Tuple<TSource> -> Tuple<TSource>
        public static Tuple<TSource> Multiply<TSource>(this Tuple<Tuple<TSource>> sourceWrapper) =>
            sourceWrapper.SelectMany(source => source, (source, value) => value); // Immediate execution.

        // Unit: TSource -> Tuple<TSource>
        public static Tuple<TSource> Unit<TSource>(TSource value) => Tuple(value);

        // SelectMany: (Tuple<TSource>, TSource -> Tuple<TSelector>, (TSource, TSelector) -> TResult) -> Tuple<TResult>
        public static Tuple<TResult> SelectMany<TSource, TSelector, TResult>(
            this Tuple<TSource> source,
            Func<TSource, Tuple<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                new Tuple<TResult>(resultSelector(source.Item1, selector(source.Item1).Item1)); // Immediate execution.
    }

    public static partial class TupleExtensions
    {
        internal static void Workflow()
        {
            Tuple<string> query = from filePath in new Tuple<string>(Console.ReadLine())
                                  from encodingName in new Tuple<string>(Console.ReadLine())
                                  from encoding in new Tuple<Encoding>(Encoding.GetEncoding(encodingName))
                                  from fileContent in new Tuple<string>(File.ReadAllText(filePath, encoding))
                                  select fileContent; // Define and execute query.
            string result = query.Item1; // Query result.
        }
    }

    #endregion

    #region Tuple<T,>

    public static partial class TupleExtensions // Tuple<T, TResult> : IMonad<Tuple<T,>>
    {
        // Multiply: Tuple<T, Tuple<T, TSource> -> Tuple<T, TSource>
        public static Tuple<T, TSource> Multiply<T, TSource>(this Tuple<T, Tuple<T, TSource>> sourceWrapper) =>
            sourceWrapper.SelectMany(source => source, (source, value) => value); // Immediate execution.

        // Unit: TSource -> Tuple<T, TSource>
        public static Tuple<T, TSource> Unit<T, TSource>(TSource value) => Tuple<T, TSource>(value);

        // SelectMany: (Tuple<T, TSource>, TSource -> Tuple<T, TSelector>, (TSource, TSelector) -> TResult) -> Tuple<T, TResult>
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
            Tuple<string, int> source = "a".Tuple(1);

            // Associativity preservation: source.Wrap().Multiply().Wrap().Multiply() == source.Wrap().Wrap().Multiply().Multiply().
            Trace.WriteLine(source
                .Tuple<string, Tuple<string, int>>()
                .Multiply()
                .Tuple<string, Tuple<string, int>>()
                .Multiply()); // (, 1)
            Trace.WriteLine(source
                .Tuple<string, Tuple<string, int>>()
                .Tuple<string, Tuple<string, Tuple<string, int>>>()
                .Multiply()
                .Multiply()); // (, 1)
            // Left unit preservation: Unit(f).Multiply() == source.
            Trace.WriteLine(Unit<string, Tuple<string, int>>(source).Multiply()); // (, 1)
            // Right unit preservation: source == source.Select(Unit).Multiply().
            Trace.WriteLine(source.Select(Unit<string, int>).Multiply()); // (a, 1)
        }

        internal static void MonadLaws()
        {
            Tuple<string, int> source = "a".Tuple(1);
            Func<int, Tuple<string, char>> selector = int32 => "b".Tuple('@');
            Func<int, Tuple<string, double>> selector1 = int32 => "c".Tuple(Math.Sqrt(int32));
            Func<double, Tuple<string, string>> selector2 = @double => "d".Tuple(@double.ToString("0.00"));
            const int Value = 5;

            // Associativity: source.SelectMany(selector1).SelectMany(selector2) == source.SelectMany(value => selector1(value).SelectMany(selector2)).
            Trace.WriteLine(
                from value in source
                from result1 in selector1(value)
                from result2 in selector2(result1)
                select result2); // (a, 1.00)
            Trace.WriteLine(
                from value in source
                from result in (from result1 in selector1(value) from result2 in selector2(result1) select result2)
                select result); // (a, 1.00)
            // Left unit: value.Wrap().SelectMany(selector) == selector(value).
            Trace.WriteLine(
                from value in Value.Tuple<string, int>()
                from result in selector(value)
                select result); // (, @)
            Trace.WriteLine(selector(Value)); // (b, @)
            // Right unit: source == source.SelectMany(Wrap).
            Trace.WriteLine(
                from value in source
                from result in value.Tuple<string, int>()
                select result); // (a, 1)
        }
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<T> : IMonad<Task<>>
    {
        // Multiply: Task<Task<T> -> Task<T>
        public static Task<TResult> Multiply<TResult>(this Task<Task<TResult>> sourceWrapper) =>
            sourceWrapper.SelectMany(source => source, (source, value) => value); // Immediate execution, impure.

        // Unit: TSource -> Task<TSource>
        public static Task<TSource> Unit<TSource>(TSource value) => Task(value);

        // SelectMany: (Task<TSource>, TSource -> Task<TSelector>, (TSource, TSelector) -> TResult) -> Task<TResult>
        public static async Task<TResult> SelectMany<TSource, TSelector, TResult>(
            this Task<TSource> source,
            Func<TSource, Task<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                resultSelector(await source, await selector(await source)); // Immediate execution, impure.
    }

    public static partial class TaskExtensions
    {
        internal static async Task WorkflowAsync(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                Task<string> query = from response in httpClient.GetAsync(url) // Return Task<HttpResponseMessage>.
                                     from stream in response.Content.ReadAsStreamAsync() // Return Task<Stream>.
                                     from text in new StreamReader(stream).ReadToEndAsync() // Return Task<string>.
                                     select text; // Define and execute query.
                string result = await query; // Query result.
            }
        }
    }

    #endregion

    public static partial class TaskExtensions
    {
        // SelectMany: (Task, TSource -> Task<TSelector>, (TSource, TSelector) -> TResult) -> Task<TResult>
        public static async Task<TResult> SelectMany<TSelector, TResult>(
            this Task source,
            Func<Unit, Task<TSelector>> selector,
            Func<Unit, TSelector, TResult> resultSelector)
        {
            await source;
            return resultSelector(default(Unit), await selector(default(Unit)));
        }

        // SelectMany: (Task<TSource>, TSource -> Task, (TSource, Unit) -> TResult) -> Task<TResult>
        public static async Task<TResult> SelectMany<TSource, TResult>(
            this Task<TSource> source,
            Func<TSource, Task> selector,
            Func<TSource, Unit, TResult> resultSelector)
        {
            await selector(await source);
            return resultSelector(await source, default(Unit));
        }

        // Select: (Unit -> TResult) -> (Task -> Task<TResult>)
        public static Task<TResult> Select<TResult>(this Task source, Func<Unit, TResult> selector) =>
            source.SelectMany(value => selector(value).Task(), (value, result) => result);
    }

    public static partial class QueryableExtensions
    {
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector,
            Expression<Func<TSource, TCollection, TResult>> resultSelector) =>
                source.Provider.CreateQuery<TResult>(
                    Expression.Call(
                        null,
                        ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(
                            typeof(TSource),
                            typeof(TCollection),
                            typeof(TResult)),
                        new Expression[]
                            {
                                source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector)
                            }));
    }
}
