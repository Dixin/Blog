namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

#if DEMO
    public interface IMonoidalFunctor<TMonoidalFunctor<>> : IFunctor<TMonoidalFunctor<>>
    {
        // Select: (TSource -> TResult) -> (TMonoidalFunctor<TSource> -> TMonoidalFunctor<TResult>)
        // Func<TMonoidalFunctor<TSource>, TMonoidalFunctor<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Multiply: Lazy<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> -> TMonoidalFunctor<Lazy<T1, T2>>
        TMonoidalFunctor<Lazy<T1, T2>> Multiply<T1, T2>(
            Lazy<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> bifunctor);

        // Unit: Unit -> TMonoidalFunctor<Unit>
        TMonoidalFunctor<Unit> Unit(Unit unit);
    }

    public interface IApplicativeFunctor<TApplicativeFunctor<>> : IFunctor<TApplicativeFunctor<>>
    {
        // Select: (TSource -> TResult) -> (TApplicativeFunctor<TSource> -> TApplicativeFunctor<TResult>)
        // Func<TApplicativeFunctor<TSource>, TApplicativeFunctor<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Apply: (TApplicativeFunctor<TSource -> TResult>, TApplicativeFunctor<TSource> -> TApplicativeFunctor<TResult>
        TApplicativeFunctor<TResult> Apply<TSource, TResult>(
            TApplicativeFunctor<Func<TSource, TResult>> selectorWrapper, TApplicativeFunctor<TSource> source);

        // Wrap: TSource -> TApplicativeFunctor<TSource>
        TApplicativeFunctor<TSource> Wrap<TSource>(TSource value);
    }
#endif

    #region IEnumerable<>

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: Lazy<IEnumerable<T1>, IEnumerable<T2>> => IEnumerable<Lazy<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Multiply<T1, T2>(
            this Lazy<IEnumerable<T1>, IEnumerable<T2>> bifunctor)
        {
            foreach (T1 value1 in bifunctor.Value1)
            {
                foreach (T2 value2 in bifunctor.Value2)
                {
                    yield return value1.Lazy(value2);
                }
            }
        }

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit))
        {
            yield return unit;
        }
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IApplicativeFunctor<IEnumerable<>>
    {
        // Apply: (IEnumerable<TSource -> TResult>, IEnumerable<TSource>) -> IEnumerable<TResult>
        public static IEnumerable<TResult> Apply<TSource, TResult>(
            this IEnumerable<Func<TSource, TResult>> selectorWrapper, IEnumerable<TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value) => Unit().Select(unit => value);
    }

#if DEMO
    public static partial class EnumerableExtensions // IEnumerable<T> : IApplicativeFunctor<IEnumerable<>>
    {
        // Apply: (IEnumerable<TSource -> TResult>, IEnumerable<TSource>) -> IEnumerable<TResult>
        public static IEnumerable<TResult> Apply<TSource, TResult>(
            this IEnumerable<Func<TSource, TResult>> selectorWrapper, IEnumerable<TSource> source)
        {
            foreach (Func<TSource, TResult> selector in selectorWrapper)
            {
                foreach (TSource value in source)
                {
                    yield return selector(value);
                }
            }
        }

        // Wrap: TSource -> IEnumerable<TSource>
        public static IEnumerable<TSource> Enumerable<TSource>(this TSource value)
        {
            yield return value;
        }
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: Lazy<IEnumerable<T1>, IEnumerable<T2>> -> IEnumerable<Lazy<T1, T2>>
        public static IEnumerable<Lazy<T1, T2>> Multiply<T1, T2>(
            this Lazy<IEnumerable<T1>, IEnumerable<T2>> bifunctor) =>
                 new Func<T1, T2, Lazy<T1, T2>>(LazyExtensions.Lazy).Curry().Enumerable()
                    .Apply(bifunctor.Value1)
                    .Apply(bifunctor.Value2);

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit)) => unit.Enumerable();
    }
#endif

    public static partial class EnumerableExtensions
    {
        internal static void MonoidalFunctorLaws()
        {
            IEnumerable<int> enumerable = new int[] { 0, 1, 2, 3, 4 };
            IEnumerable<Unit> unit = Unit();
            IEnumerable<int> enumerable1 = new int[] { 0, 1 };
            IEnumerable<char> enumerable2 = new char[] { '@', '#' };
            IEnumerable<bool> enumerable3 = new bool[] { true, false };

            // Left unit preservation: unit.Lazy(f).Multiply().Select(LeftUnitor) == f.
            unit.Lazy(enumerable).Multiply().Select(LazyExtensions.LeftUnitor)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Right unit preservation: f == f.Lazy(unit).Multiply().Select(RightUnitor).
            enumerable.Lazy(unit).Multiply().Select(LazyExtensions.RightUnitor)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Associativity preservation: f1.Lazy(f2).Multiply().Lazy(f3).Multiply().Select(Associator) == f1.Lazy(f2.Lazy(f3).Multiply()).Multiply().
            enumerable1.Lazy(enumerable2).Multiply().Lazy(enumerable3).Multiply().Select(LazyExtensions.Associator)
                .ForEach(result => Trace.WriteLine(result));
            // (0, (@, True)) (0, (@, False)) (0, (#, True)) (0, (#, False)) (1, (@, True)) (1, (@, False)) (1, (#, True)) (1, (#, False))
            enumerable1.Lazy(enumerable2.Lazy(enumerable3).Multiply()).Multiply()
                .ForEach(result => Trace.WriteLine(result));
            // (0, (@, True)) (0, (@, False)) (0, (#, True)) (0, (#, False)) (1, (@, True)) (1, (@, False)) (1, (#, True)) (1, (#, False))
        }

        internal static void ApplicativeFunctorLaws()
        {
            IEnumerable<int> enumerable = new int[] { 0, 1, 2, 3, 4 };
            IEnumerable<Func<int, double>> selectorWrapper1 =
                new Func<int, double>[] { int32 => int32 / 2D, int32 => Math.Sqrt(int32) };
            IEnumerable<Func<double, string>> selectorWrapper2 =
                new Func<double, string>[] { @double => @double.ToString("0.0"), @double => @double.ToString("0.00") };
            Func<int, double> selector = int32 => Math.Sqrt(int32);
            const int Value = 5;
            
            // Identity: Id.Wrap().Apply(f) == f.
            new Func<int, int>(Functions.Id).Enumerable().Apply(enumerable)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Composition: o.Wrap().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(f) == selectorWrapper2.Apply(selectorWrapper1.Apply(f)).
            Functions<int, double, string>.o.Enumerable()
                .Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(enumerable)
                .ForEach(result => Trace.WriteLine(result));
            // 0.0  0.5  1.0  1.5  2.0
            // 0.0  1.0  1.4  1.7  2.0 
            // 0.00 0.50 1.00 1.50 2.00
            // 0.00 1.00 1.41 1.73 2.00
            selectorWrapper2.Apply(selectorWrapper1.Apply(enumerable))
                .ForEach(result => Trace.WriteLine(result));
            // 0.0  0.5  1.0  1.5  2.0
            // 0.0  1.0  1.4  1.7  2.0 
            // 0.00 0.50 1.00 1.50 2.00
            // 0.00 1.00 1.41 1.73 2.00
            // Homomorphism: selector.Wrap().Apply(value.Wrap()) == selector(value).Wrap().
            selector.Enumerable().Apply(Value.Enumerable())
                .ForEach(result => Trace.WriteLine(result)); // 2.23606797749979
            selector(Value).Enumerable()
                .ForEach(result => Trace.WriteLine(result)); // 2.23606797749979
            // Interchange: selectorWrapper.Apply(value.Wrap()) == (selector => selector(value)).Wrap().Apply(selectorWrapper).
            selectorWrapper1.Apply(Value.Enumerable())
                .ForEach(result => Trace.WriteLine(result)); // 2.5 2.23606797749979
            new Func<Func<int, double>, double>(selectorFunction => selectorFunction(Value)).Enumerable().Apply(selectorWrapper1)
                .ForEach(result => Trace.WriteLine(result)); // 2.5 2.23606797749979
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<T> : IMonoidalFunctor<Lazy<>>
    {
        // Multiply: Lazy<Lazy<T1>, Lazy<T2>> => Lazy<Lazy<T1, T2>>
        public static Lazy<Lazy<T1, T2>> Multiply<T1, T2>(this Lazy<Lazy<T1>, Lazy<T2>> bifunctor) =>
            new Lazy<Lazy<T1, T2>>(() => bifunctor.Value1.Value.Lazy(bifunctor.Value2.Value));

        // Unit: Unit -> Lazy<Unit>
        public static Lazy<Unit> Unit(Unit unit = default(Unit)) => new Lazy<Unit>(() => unit);
    }

    public static partial class LazyExtensions // Lazy<T> : IApplicativeFunctor<Lazy<>>
    {
        // Apply: (Lazy<TSource -> TResult>, Lazy<TSource>) -> Lazy<TResult>
        public static Lazy<TResult> Apply<TSource, TResult>(
            this Lazy<Func<TSource, TResult>> selectorWrapper, Lazy<TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Lazy<TSource>
        public static Lazy<T> Lazy<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Func<>

    public static partial class FuncExtensions // Func<T> : IMonoidalFunctor<Func<>>
    {
        // Multiply: Lazy<Func<T1>, Func<T2>> => Func<Lazy<T1, T2>>
        public static Func<Lazy<T1, T2>> Multiply<T1, T2>(this Lazy<Func<T1>, Func<T2>> bifunctor) =>
            () => bifunctor.Value1().Lazy(bifunctor.Value2());

        // Unit: Unit -> Func<Unit>
        public static Func<Unit> Unit(Unit unit = default(Unit)) => () => unit;
    }

    public static partial class FuncExtensions // Func<T> : IApplicativeFunctor<Func<>>
    {
        // Apply: (Func<TSource -> TResult>, Func<TSource>) -> Func<TResult>
        public static Func<TResult> Apply<TSource, TResult>(
            this Func<Func<TSource, TResult>> selectorWrapper, Func<TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Func<TSource>
        public static Func<T> Func<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Func<T,>

    public static partial class FuncExtensions // Func<T, TResult> : IMonoidalFunctor<Func<T,>>
    {
        // Multiply: Lazy<Func<T, T1>, Func<T, T2>> => Func<T, Lazy<T1, T2>>
        public static Func<T, Lazy<T1, T2>> Multiply<T, T1, T2>(this Lazy<Func<T, T1>, Func<T, T2>> bifunctor) =>
            value => bifunctor.Value1(value).Lazy(bifunctor.Value2(value));

        // Unit: Unit -> Func<T, Unit>
        public static Func<T, Unit> Unit<T>(Unit unit = default(Unit)) => _ => unit;
    }

    public static partial class FuncExtensions // Func<T, TResult> : IApplicativeFunctor<Func<T,>>
    {
        // Apply: (Func<T, TSource -> TResult>, Func<T, TSource>) -> Func<T, TResult>
        public static Func<T, TResult> Apply<T, TSource, TResult>(
            this Func<T, Func<TSource, TResult>> selectorWrapper, Func<T, TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Func<T, TSource>
        public static Func<T, TSource> Func<T, TSource>(this TSource value) => Unit<T>().Select(unit => value);
    }

    #endregion

    #region Optional<>

    public static partial class OptionalExtensions // Optional<T> : IMonoidalFunctor<Optional<>>
    {
        // Multiply: Lazy<Optional<T1>, Optional<T2>> => Optional<Lazy<T1, T2>>
        public static Optional<Lazy<T1, T2>> Multiply<T1, T2>(this Lazy<Optional<T1>, Optional<T2>> bifunctor) =>
            new Optional<Lazy<T1, T2>>(() => bifunctor.Value1.HasValue && bifunctor.Value2.HasValue
                ? true.Tuple(bifunctor.Value1.Value.Lazy(bifunctor.Value2.Value))
                : false.Tuple(default(Lazy<T1, T2>)));

        // Unit: Unit -> Optional<Unit>
        public static Optional<Unit> Unit(Unit unit = default(Unit)) =>
            new Optional<Unit>(() => true.Tuple(unit));
    }

    public static partial class OptionalExtensions // Optional<T> : IApplicativeFunctor<Optional<>>
    {
        // Apply: (Optional<TSource -> TResult>, Optional<TSource>) -> Optional<TResult>
        public static Optional<TResult> Apply<TSource, TResult>(
            this Optional<Func<TSource, TResult>> selectorWrapper, Optional<TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Optional<TSource>
        public static Optional<T> Optional<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Tuple<>

    public static partial class TupleExtensions // Tuple<T> : IMonoidalFunctor<Tuple<>>
    {
        // Multiply: Lazy<Tuple<T1>, Tuple<T2>> => Tuple<Lazy<T1, T2>>
        public static Tuple<Lazy<T1, T2>> Multiply<T1, T2>(this Lazy<Tuple<T1>, Tuple<T2>> bifunctor) =>
            new Tuple<Lazy<T1, T2>>(bifunctor.Value1.Item1.Lazy(bifunctor.Value2.Item1));

        // Unit: Unit -> Tuple<Unit>
        public static Tuple<Unit> Unit(Unit unit = default(Unit)) => new Tuple<Unit>(unit);
    }

    public static partial class TupleExtensions // Tuple<T> : IApplicativeFunctor<Tuple<>>
    {
        // Apply: (Tuple<TSource -> TResult>, Tuple<TSource>) -> Tuple<TResult>
        public static Tuple<TResult> Apply<TSource, TResult>(
            this Tuple<Func<TSource, TResult>> selectorWrapper, Tuple<TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Tuple<TSource>
        public static Tuple<T> Tuple<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Tuple<T,>

    public static partial class TupleExtensions // Tuple<T, TResult> : IMonoidalFunctor<Tuple<T,>>
    {
        // Multiply: Lazy<Tuple<T, T1>, Tuple<T, T2>> => Tuple<T, Lazy<T1, T2>>
        public static Tuple<T, Lazy<T1, T2>> Multiply<T, T1, T2>(this Lazy<Tuple<T, T1>, Tuple<T, T2>> bifunctor) =>
            bifunctor.Value1.Item1.Tuple(bifunctor.Value1.Item2.Lazy(bifunctor.Value2.Item2));

        // Unit: Unit -> Tuple<Unit>
        public static Tuple<T, Unit> Unit<T>(Unit unit = default(Unit)) => default(T).Tuple(unit);
    }

    public static partial class TupleExtensions // Func<T, TResult> : IApplicativeFunctor<Func<T,>>
    {
        // Apply: (Tuple<T, TSource -> TResult>, Tuple<T, TSource>) -> Tuple<T, TResult>
        public static Tuple<T, TResult> Apply<T, TSource, TResult>(
            this Tuple<T, Func<TSource, TResult>> selectorWrapper, Tuple<T, TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Tuple<T, TSource>
        public static Tuple<T, TSource> Tuple<T, TSource>(this TSource value) => Unit<T>().Select(unit => value);
    }

    public static partial class TupleExtensions
    {
        internal static void MonoidalFunctorLaws()
        {
            Tuple<string, int> tuple = "a".Tuple(1);
            Tuple<string, Unit> unit = Unit<string>();
            Tuple<string, int> tuple1 = "b".Tuple(2);
            Tuple<string, char> tuple2 = "c".Tuple('@');
            Tuple<string, bool> tuple3 = "d".Tuple(true);

            // Left unit preservation: unit.Lazy(f).Multiply().Select(LeftUnitor) == f.
            Trace.WriteLine(unit.Lazy(tuple).Multiply().Select(LazyExtensions.LeftUnitor)); // (, 1)
            // Right unit preservation: f == f.Lazy(unit).Multiply().Select(RightUnitor).
            Trace.WriteLine(tuple.Lazy(unit).Multiply().Select(LazyExtensions.RightUnitor)); // (a, 1)
            // Associativity preservation: f1.Lazy(f2).Multiply().Lazy(f3).Multiply().Select(Associator) == f1.Lazy(f2.Lazy(f3).Multiply()).Multiply().
            Trace.WriteLine(tuple1.Lazy(tuple2).Multiply().Lazy(tuple3).Multiply().Select(LazyExtensions.Associator)); // (b, (2, (@, True)))
            Trace.WriteLine(tuple1.Lazy(tuple2.Lazy(tuple3).Multiply()).Multiply()); // (b, (2, (@, True)))
        }

        internal static void ApplicativeFunctorLaws()
        {
            Tuple<string, int> enumerable = "a".Tuple(1);
            Tuple<string, Func<int, double>> selectorWrapper1 = 
                "b".Tuple(new Func<int, double>(int32 => Math.Sqrt(int32)));
            Tuple<string, Func<double, string>> selectorWrapper2 = 
                "c".Tuple(new Func<double, string>(@double => @double.ToString("0.00")));
            Func<int, double> selector = int32 => Math.Sqrt(int32);
            const int Value = 5;

            // Identity: Id.Wrap().Apply(f) == f.
            Trace.WriteLine(new Func<int, int>(Functions.Id).Tuple<string, Func<int, int>>().Apply(enumerable)); // (, 1)
            // Composition: o.Wrap().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(f) == selectorWrapper2.Apply(selectorWrapper1.Apply(f)).
            Trace.WriteLine(Functions<int, double, string>.o.Tuple<string, Func<Func<double, string>, Func<Func<int, double>, Func<int, string>>>>()
                .Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(enumerable)); // (, 1.00)
            Trace.WriteLine(selectorWrapper2.Apply(selectorWrapper1.Apply(enumerable))); // (c, 1.00)
            // Homomorphism: selector.Wrap().Apply(value.Wrap()) == selector(value).Wrap().
            Trace.WriteLine(selector.Tuple<string, Func<int, double>>().Apply(Value.Tuple<string, int>())); // (, 2.23606797749979)
            Trace.WriteLine(selector(Value).Tuple<string, double>()); // (, 2.23606797749979)
            // Interchange: selectorWrapper.Apply(value.Wrap()) == (selector => selector(value)).Wrap().Apply(selectorWrapper).
            Trace.WriteLine(selectorWrapper1.Apply(Value.Tuple<string, int>())); // (b, 2.23606797749979)
            Trace.WriteLine(new Func<Func<int, double>, double>(selectorFunction => selectorFunction(Value))
                .Tuple<string, Func<Func<int, double>, double>>().Apply(selectorWrapper1)); // (, 2.23606797749979)
        }
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<T> : IMonoidalFunctor<Task<>>
    {
        // Multiply: Lazy<Task<T1>, Task<T2>> => Task<Lazy<T1, T2>>
        public static async Task<Lazy<T1, T2>> Multiply<T1, T2>(this Lazy<Task<T1>, Task<T2>> bifunctor) =>
            (await bifunctor.Value1).Lazy(await bifunctor.Value2);

        // Unit: Unit -> Task<Unit>
        public static Task<Unit> Unit(Unit unit = default(Unit)) => System.Threading.Tasks.Task.FromResult(unit);
    }

    // Impure.
    public static partial class TaskExtensions // Task<T> : IApplicativeFunctor<Task<>>
    {
        // Apply: (Task<TSource -> TResult>, Task<TSource>) -> Task<TResult>
        public static Task<TResult> Apply<TSource, TResult>(
            this Task<Func<TSource, TResult>> selectorWrapper, Task<TSource> source) =>
                selectorWrapper.Lazy(source).Multiply().Select(product => product.Value1(product.Value2));

        // Wrap: TSource -> Task<TSource>
        public static Task<T> Task<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion
}
