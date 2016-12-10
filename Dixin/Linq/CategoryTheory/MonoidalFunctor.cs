namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    using static Dixin.Linq.CategoryTheory.DotNetCategory;

#if DEMO
    // Cannot be compiled.
    public interface IMonoidalFunctor<TMonoidalFunctor<>> : IFunctor<TMonoidalFunctor<>>
         where TMonoidalFunctor<> : IMonoidalFunctor<TMonoidalFunctor<>>
    {
        // From IFunctor<TMonoidalFunctor<>>:
        // Select: (TSource -> TResult) -> (TMonoidalFunctor<TSource> -> TMonoidalFunctor<TResult>)
        // Func<TMonoidalFunctor<TSource>, TMonoidalFunctor<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Multiply: TMonoidalFunctor<T1> x TMonoidalFunctor<T2> -> TMonoidalFunctor<T1 x T2>
        // Multiply: Tuple<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> -> TMonoidalFunctor<Tuple<T1, T2>>
        TMonoidalFunctor<Tuple<T1, T2>> Multiply<T1, T2>(
            Tuple<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> bifunctor);

        // Unit: Unit -> TMonoidalFunctor<Unit>
        TMonoidalFunctor<Unit> Unit(Unit unit);
    }

    // Cannot be compiled.
    public interface IMonoidalFunctor<TMonoidalFunctor<>> : IFunctor<TMonoidalFunctor<>>
         where TMonoidalFunctor<> : IMonoidalFunctor<TMonoidalFunctor<>>
    {
        // Multiply: TMonoidalFunctor<T1> x TMonoidalFunctor<T2> -> TMonoidalFunctor<T1 x T2>
        // Multiply: (TMonoidalFunctor<T1>, TMonoidalFunctor<T2>) -> TMonoidalFunctor<Tuple<T1, T2>>
        TMonoidalFunctor<Tuple<T1, T2>> Multiply<T1, T2>(
            TMonoidalFunctor<T1> source1, TMonoidalFunctor<T2> source2);

        // Unit: Unit -> TMonoidalFunctor<Unit>
        TMonoidalFunctor<Unit> Unit(Unit unit);
    }

    // Cannot be compiled.
    public interface IApplicativeFunctor<TApplicativeFunctor<>> : IFunctor<TApplicativeFunctor<>>
         where TApplicativeFunctor<> : IApplicativeFunctor<TApplicativeFunctor<>>
    {
        // From: IFunctor<TApplicativeFunctor<>>:
        // Select: (TSource -> TResult) -> (TApplicativeFunctor<TSource> -> TApplicativeFunctor<TResult>)
        // Func<TApplicativeFunctor<TSource>, TApplicativeFunctor<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Apply: (TApplicativeFunctor<TSource -> TResult>, TApplicativeFunctor<TSource> -> TApplicativeFunctor<TResult>
        TApplicativeFunctor<TResult> Apply<TSource, TResult>(
            TApplicativeFunctor<Func<TSource, TResult>> selectorWrapper, TApplicativeFunctor<TSource> source);

        // Wrap: TSource -> TApplicativeFunctor<TSource>
        TApplicativeFunctor<TSource> Wrap<TSource>(TSource value);
    }

    // Cannot be compiled.
    public static class MonoidalFunctorExtensions // (Multiply, Unit) implements (Apply, Wrap).
    {
        // Apply: (TMonoidalFunctor<TSource -> TResult>, TMonoidalFunctor<TSource>) -> TMonoidalFunctor<TResult>
        public static TMonoidalFunctor<TResult> Apply<TMonoidalFunctor<>, TSource, TResult>(
            this TMonoidalFunctor<Func<TSource, TResult>> selectorWrapper, TMonoidalFunctor<TSource> source) 
            where TMonoidalFunctor<> : IMonoidalFunctor<TMonoidalFunctor<>> =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2));

        // Wrap: TSource -> TMonoidalFunctor<TSource>
        public static TMonoidalFunctor<TSource> Wrap<TMonoidalFunctor<>, TSource>(this TSource value) 
            where TMonoidalFunctor<> : IMonoidalFunctor<TMonoidalFunctor<>> => 
                TMonoidalFunctor<TSource>.Unit().Select(unit => value);
    }

    // Cannot be compiled.
    public static class ApplicativeFunctorExtensions // (Apply, Wrap) implements (Multiply, Unit).
    {
        // Multiply: TApplicativeFunctor<T1> x TApplicativeFunctor<T2> -> TApplicativeFunctor<T1 x T2>
        // Multiply: (TApplicativeFunctor<T1>, TApplicativeFunctor<T2>) -> TApplicativeFunctor<Tuple<T1, T2>>
        public static TApplicativeFunctor<Tuple<T1, T2>> Multiply<TApplicativeFunctor<>, T1, T2>(
            this TApplicativeFunctor<T1> source1, TApplicativeFunctor<T2> source2) 
            where TApplicativeFunctor<> : IApplicativeFunctor<TApplicativeFunctor<>> =>
                new Func<T1, T2, Tuple<T1, T2>>(Tuple.Create).Curry().Wrap().Apply(source1).Apply(source2);

        // Unit: Unit -> TApplicativeFunctor<Unit>
        public static TApplicativeFunctor<Unit> Unit<TApplicativeFunctor<>>(Unit unit = default(Unit))
            where TApplicativeFunctor<> : IApplicativeFunctor<TApplicativeFunctor<>> => unit.Wrap();
    }
#endif

    #region IEnumerable<>

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: IEnumerable<T1> x IEnumerable<T2> -> IEnumerable<T1 x T2>
        // Multiply: Tuple<IEnumerable<T1>, IEnumerable<T2>> -> IEnumerable<Tuple<T1, T2>>
        // Multiply: (IEnumerable<T1>, IEnumerable<T2>) -> IEnumerable<Tuple<T1, T2>>
        public static IEnumerable<Tuple<T1, T2>> Multiply<T1, T2>(
            this IEnumerable<T1> source1, IEnumerable<T2> source2) // Implicit tuple.
        {
            foreach (T1 value1 in source1)
            {
                foreach (T2 value2 in source2)
                {
                    yield return value1.Tuple(value2);
                }
            }
        }

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit))
        {
            yield return unit;
        }
    }

    public static partial class EnumerableExtensions
    {
        internal static void Selector1Arity(IEnumerable<int> xs)
        {
            Func<int, bool> selector = x => x > 0;
            // Apply selector with xs.
            IEnumerable<bool> applyWithXs = xs.Select(selector);
        }

        internal static void SelectorNArity(IEnumerable<int> xs, IEnumerable<long> ys, IEnumerable<double> zs)
        {
            Func<int, long, double, bool> selector = (x, y, z) => x + y + z > 0;
            // Curry selector.
            Func<int, Func<long, Func<double, bool>>> curriedSelector = 
                selector.Curry(); // 1 arity: x => (y => z => x + y + z > 0)
            // Partially apply selector with xs.
            IEnumerable<Func<long, Func<double, bool>>> applyWithXs = xs.Select(curriedSelector);
            // Partially apply selector with ys.
            IEnumerable<Tuple<Func<long, Func<double, bool>>, long>> multiplyWithYs = applyWithXs.Multiply(ys);
            IEnumerable<Func<double, bool>> applyWithYs = multiplyWithYs.Select(product =>
            {
                Func<long, Func<double, bool>> partialAppliedSelector = product.Item1;
                long y = product.Item2;
                return partialAppliedSelector(y);
            });
            // Partially apply selector with zs.
            IEnumerable<Tuple<Func<double, bool>, double>> multiplyWithZs = applyWithYs.Multiply(zs);
            IEnumerable<bool> applyWithZs = multiplyWithZs.Select(product =>
            {
                Func<double, bool> partialAppliedSelector = product.Item1;
                double z = product.Item2;
                return partialAppliedSelector(z);
            });
        }

        internal static void Apply(IEnumerable<int> xs, IEnumerable<long> ys, IEnumerable<double> zs)
        {
            Func<int, long, double, bool> selector = (x, y, z) => x + y + z > 0;
            // Partially apply selector with xs.
            IEnumerable<Func<long, Func<double, bool>>> applyWithXs = xs.Select(selector.Curry());
            // Partially apply selector with ys.
            IEnumerable<Func<double, bool>> applyWithYs = applyWithXs.Apply(ys);
            // Partially apply selector with zs.
            IEnumerable<bool> applyWithZs = applyWithYs.Apply(zs);
        }
    }

    public static partial class EnumerableExtensions // IEnumerable<T> : IApplicativeFunctor<IEnumerable<>>
    {
        // Apply: (IEnumerable<TSource -> TResult>, IEnumerable<TSource>) -> IEnumerable<TResult>
        public static IEnumerable<TResult> Apply<TSource, TResult>(
            this IEnumerable<Func<TSource, TResult>> selectorWrapper, IEnumerable<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2));

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
        // Multiply: IEnumerable<T1> x IEnumerable<T2> -> IEnumerable<T1 x T2>
        // Multiply: (IEnumerable<T1>, IEnumerable<T2>) -> IEnumerable<Tuple<T1, T2>>
        public static IEnumerable<Tuple<T1, T2>> Multiply<T1, T2>(
            this IEnumerable<T1> source1, IEnumerable<T2> source2) =>
                new Func<T1, T2, Tuple<T1, T2>>(Tuple.Create).Curry().Enumerable().Apply(source1).Apply(source2);

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit)) => unit.Enumerable();
    }
#endif

    public static partial class EnumerableExtensions
    {
        internal static void MonoidalFunctorLaws()
        {
            IEnumerable<Unit> unit = Unit();
            IEnumerable<int> source1 = new int[] { 0, 1 };
            IEnumerable<char> source2 = new char[] { '@', '#' };
            IEnumerable<bool> source3 = new bool[] { true, false };
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };

            // Associativity preservation: source1.Multiply(source2).Multiply(source3).Select(Associator) == source1.Multiply(source2.Multiply(source3)).
            source1.Multiply(source2).Multiply(source3).Select(Associator)
                .ForEach(result => Trace.WriteLine(result));
                // (0, (@, True)) (0, (@, False)) (0, (#, True)) (0, (#, False))
                // (1, (@, True)) (1, (@, False)) (1, (#, True)) (1, (#, False))
            source1.Multiply(source2.Multiply(source3))
                .ForEach(result => Trace.WriteLine(result));
                // (0, (@, True)) (0, (@, False)) (0, (#, True)) (0, (#, False))
                // (1, (@, True)) (1, (@, False)) (1, (#, True)) (1, (#, False))
            // Left unit preservation: unit.Multiply(source).Select(LeftUnitor) == source.
            unit.Multiply(source).Select(LeftUnitor)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Right unit preservation: source == source.Multiply(unit).Select(RightUnitor).
            source.Multiply(unit).Select(RightUnitor)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
        }

        internal static void ApplicativeLaws()
        {
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };
            Func<int, double> selector = int32 => Math.Sqrt(int32);
            IEnumerable<Func<int, double>> selectorWrapper1 =
                new Func<int, double>[] { int32 => int32 / 2D, int32 => Math.Sqrt(int32) };
            IEnumerable<Func<double, string>> selectorWrapper2 =
                new Func<double, string>[] { @double => @double.ToString("0.0"), @double => @double.ToString("0.00") };
            Func<Func<double, string>, Func<Func<int, double>, Func<int, string>>> o =
                new Func<Func<double, string>, Func<int, double>, Func<int, string>>(Linq.FuncExtensions.o).Curry();
            int value = 5;

            // Functor preservation: source.Select(selector) == selector.Wrap().Apply(source).
            source.Select(selector)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 1.4142135623731 1.73205080756888 2
            selector.Enumerable().Apply(source)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 1.4142135623731 1.73205080756888 2
            // Identity preservation: Id.Wrap().Apply(source) == source.
            new Func<int, int>(Functions.Id).Enumerable().Apply(source)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Composition preservation: o.Wrap().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source) == selectorWrapper2.Apply(selectorWrapper1.Apply(source)).
            o.Enumerable().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source)
                .ForEach(result => Trace.WriteLine(result));
                // 0.0  0.5  1.0  1.5  2.0
                // 0.0  1.0  1.4  1.7  2.0 
                // 0.00 0.50 1.00 1.50 2.00
                // 0.00 1.00 1.41 1.73 2.00
            selectorWrapper2.Apply(selectorWrapper1.Apply(source))
                .ForEach(result => Trace.WriteLine(result));
                // 0.0  0.5  1.0  1.5  2.0
                // 0.0  1.0  1.4  1.7  2.0 
                // 0.00 0.50 1.00 1.50 2.00
                // 0.00 1.00 1.41 1.73 2.00
            // Homomorphism: selector.Wrap().Apply(value.Wrap()) == selector(value).Wrap().
            selector.Enumerable().Apply(value.Enumerable())
                .ForEach(result => Trace.WriteLine(result)); // 2.23606797749979
            selector(value).Enumerable()
                .ForEach(result => Trace.WriteLine(result)); // 2.23606797749979
            // Interchange: selectorWrapper.Apply(value.Wrap()) == (selector => selector(value)).Wrap().Apply(selectorWrapper).
            selectorWrapper1.Apply(value.Enumerable())
                .ForEach(result => Trace.WriteLine(result)); // 2.5 2.23606797749979
            new Func<Func<int, double>, double>(function => function(value)).Enumerable().Apply(selectorWrapper1)
                .ForEach(result => Trace.WriteLine(result)); // 2.5 2.23606797749979
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<T> : IMonoidalFunctor<Lazy<>>
    {
        // Multiply: Lazy<T1> x Lazy<T2> -> Lazy<T1 x T2>
        // Multiply: (Lazy<T1>, Lazy<T2>) -> Lazy<Tuple<T1, T2>>
        public static Lazy<Tuple<T1, T2>> Multiply<T1, T2>(this Lazy<T1> source1, Lazy<T2> source2) =>
            new Lazy<Tuple<T1, T2>>(() => source1.Value.Tuple(source2.Value));

        // Unit: Unit -> Lazy<Unit>
        public static Lazy<Unit> Unit(Unit unit = default(Unit)) => new Lazy<Unit>(() => unit);
    }

    public static partial class LazyExtensions // Lazy<T> : IApplicativeFunctor<Lazy<>>
    {
        // Apply: (Lazy<TSource -> TResult>, Lazy<TSource>) -> Lazy<TResult>
        public static Lazy<TResult> Apply<TSource, TResult>(
            this Lazy<Func<TSource, TResult>> selectorWrapper, Lazy<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2));

        // Wrap: TSource -> Lazy<TSource>
        public static Lazy<T> Lazy<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Func<>

    public static partial class FuncExtensions // Func<T> : IMonoidalFunctor<Func<>>
    {
        // Multiply: Func<T1> x Func<T2> -> Func<T1 x T2>
        // Multiply: (Func<T1>, Func<T2>) -> Func<Tuple<T1, T2>>
        public static Func<Tuple<T1, T2>> Multiply<T1, T2>(this Func<T1> source1, Func<T2> source2) =>
            () => source1().Tuple(source2());

        // Unit: Unit -> Func<Unit>
        public static Func<Unit> Unit(Unit unit = default(Unit)) => () => unit;
    }

    public static partial class FuncExtensions // Func<T> : IApplicativeFunctor<Func<>>
    {
        // Apply: (Func<TSource -> TResult>, Func<TSource>) -> Func<TResult>
        public static Func<TResult> Apply<TSource, TResult>(
            this Func<Func<TSource, TResult>> selectorWrapper, Func<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2));

        // Wrap: TSource -> Func<TSource>
        public static Func<T> Func<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Func<T,>

    public static partial class FuncExtensions // Func<T, TResult> : IMonoidalFunctor<Func<T,>>
    {
        // Multiply: Func<T, T1> x Func<T, T2> -> Func<T, T1 x T2>
        // Multiply: (Func<T, T1>, Func<T, T2>) -> Func<T, Tuple<T1, T2>>
        public static Func<T, Tuple<T1, T2>> Multiply<T, T1, T2>(this Func<T, T1> source1, Func<T, T2> source2) =>
            value => source1(value).Tuple(source2(value));

        // Unit: Unit -> Func<T, Unit>
        public static Func<T, Unit> Unit<T>(Unit unit = default(Unit)) => _ => unit;
    }

    public static partial class FuncExtensions // Func<T, TResult> : IApplicativeFunctor<Func<T,>>
    {
        // Apply: (Func<T, TSource -> TResult>, Func<T, TSource>) -> Func<T, TResult>
        public static Func<T, TResult> Apply<T, TSource, TResult>(
            this Func<T, Func<TSource, TResult>> selectorWrapper, Func<T, TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2));

        // Wrap: TSource -> Func<T, TSource>
        public static Func<T, TSource> Func<T, TSource>(this TSource value) => Unit<T>().Select(unit => value);
    }

    #endregion

    #region Optional<>

    public static partial class OptionalExtensions // Optional<T> : IMonoidalFunctor<Optional<>>
    {
        // Multiply: Optional<T1> x Optional<T2> -> Optional<T1 x T2>
        // Multiply: (Optional<T1>, Optional<T2>) -> Optional<Tuple<T1, T2>>
        public static Optional<Tuple<T1, T2>> Multiply<T1, T2>(this Optional<T1> source1, Optional<T2> source2) =>
            new Optional<Tuple<T1, T2>>(() => source1.HasValue && source2.HasValue
                ? true.Tuple(source1.Value.Tuple(source2.Value))
                : false.Tuple(default(T1).Tuple(default(T2))));

        // Unit: Unit -> Optional<Unit>
        public static Optional<Unit> Unit(Unit unit = default(Unit)) =>
            new Optional<Unit>(() => true.Tuple(unit));
    }

    public static partial class OptionalExtensions // Optional<T> : IApplicativeFunctor<Optional<>>
    {
        // Apply: (Optional<TSource -> TResult>, Optional<TSource>) -> Optional<TResult>
        public static Optional<TResult> Apply<TSource, TResult>(
            this Optional<Func<TSource, TResult>> selectorWrapper, Optional<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2));

        // Wrap: TSource -> Optional<TSource>
        public static Optional<T> Optional<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Tuple<>

    public static partial class TupleExtensions // Tuple<T> : IMonoidalFunctor<Tuple<>>
    {
        // Multiply: Tuple<T1> x Tuple<T2> -> Tuple<T1 x T2>
        // Multiply: (Tuple<T1>, Tuple<T2>) -> Tuple<Tuple<T1, T2>>
        public static Tuple<Tuple<T1, T2>> Multiply<T1, T2>(this Tuple<T1> source1, Tuple<T2> source2) =>
            new Tuple<Tuple<T1, T2>>(source1.Item1.Tuple(source2.Item1)); // Immediate execution.

        // Unit: Unit -> Tuple<Unit>
        public static Tuple<Unit> Unit(Unit unit = default(Unit)) => new Tuple<Unit>(unit);
    }

    public static partial class TupleExtensions // Tuple<T> : IApplicativeFunctor<Tuple<>>
    {
        // Apply: (Tuple<TSource -> TResult>, Tuple<TSource>) -> Tuple<TResult>
        public static Tuple<TResult> Apply<TSource, TResult>(
            this Tuple<Func<TSource, TResult>> selectorWrapper, Tuple<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2)); // Immediate execution.

        // Wrap: TSource -> Tuple<TSource>
        public static Tuple<T> Tuple<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region Tuple<T,>

    public static partial class TupleExtensions // Tuple<T1, T2 : IMonoidalFunctor<Tuple<T,>>
    {
        // Multiply: Tuple<T, T1> x Tuple<T, T2> -> Tuple<T, T1 x T2>
        // Multiply: (Tuple<T, T1>, Tuple<T, T2>) -> Tuple<T, Tuple<T1, T2>>
        public static Tuple<T, Tuple<T1, T2>> Multiply<T, T1, T2>(this Tuple<T, T1> source1, Tuple<T, T2> source2) =>
            source1.Item1.Tuple(source1.Item2.Tuple(source2.Item2)); // Immediate execution.

        // Unit: Unit -> Tuple<Unit>
        public static Tuple<T, Unit> Unit<T>(Unit unit = default(Unit)) => default(T).Tuple(unit);
    }

    public static partial class TupleExtensions // Func<T, TResult> : IApplicativeFunctor<Func<T,>>
    {
        // Apply: (Tuple<T, TSource -> TResult>, Tuple<T, TSource>) -> Tuple<T, TResult>
        public static Tuple<T, TResult> Apply<T, TSource, TResult>(
            this Tuple<T, Func<TSource, TResult>> selectorWrapper, Tuple<T, TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2)); // Immediate execution.

        // Wrap: TSource -> Tuple<T, TSource>
        public static Tuple<T, TSource> Tuple<T, TSource>(this TSource value) => Unit<T>().Select(unit => value);
    }

    public static partial class TupleExtensions
    {
        internal static void MonoidalFunctorLaws()
        {
            Tuple<string, int> source = "a".Tuple(1);
            Tuple<string, Unit> unit = Unit<string>();
            Tuple<string, int> source1 = "b".Tuple(2);
            Tuple<string, char> source2 = "c".Tuple('@');
            Tuple<string, bool> source3 = "d".Tuple(true);

            // Associativity preservation: source1.Multiply(source2).Multiply(source3).Select(Associator) == source1.Multiply(source2.Multiply(source3)).
            Trace.WriteLine(source1.Multiply(source2).Multiply(source3).Select(Associator)); // (b, (2, (@, True)))
            Trace.WriteLine(source1.Multiply(source2.Multiply(source3))); // (b, (2, (@, True)))
            // Left unit preservation: unit.Multiply(source).Select(LeftUnitor) == source.
            Trace.WriteLine(unit.Multiply(source).Select(LeftUnitor)); // (, 1)
            // Right unit preservation: source == source.Multiply(unit).Select(RightUnitor).
            Trace.WriteLine(source.Multiply(unit).Select(RightUnitor)); // (a, 1)
        }

        internal static void ApplicativeLaws()
        {
            Tuple<string, int> source = "a".Tuple(1);
            Func<int, double> selector = int32 => Math.Sqrt(int32);
            Tuple<string, Func<int, double>> selectorWrapper1 = 
                "b".Tuple(new Func<int, double>(int32 => Math.Sqrt(int32)));
            Tuple<string, Func<double, string>> selectorWrapper2 =
                "c".Tuple(new Func<double, string>(@double => @double.ToString("0.00")));
            Func<Func<double, string>, Func<Func<int, double>, Func<int, string>>> o = 
                new Func<Func<double, string>, Func<int, double>, Func<int, string>>(Linq.FuncExtensions.o).Curry();
            int value = 5;

            // Functor preservation: source.Select(selector) == selector.Wrap().Apply(source).
            Trace.WriteLine(source.Select(selector)); // (a, 1)
            Trace.WriteLine(selector.Tuple<string, Func<int, double>>().Apply(source)); // (, 1)
            // Identity preservation: Id.Wrap().Apply(source) == source.
            Trace.WriteLine(new Func<int, int>(Functions.Id).Tuple<string, Func<int, int>>().Apply(source)); // (, 1)
            // Composition preservation: o.Curry().Wrap().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source) == selectorWrapper2.Apply(selectorWrapper1.Apply(source)).
            Trace.WriteLine(o.Tuple<string, Func<Func<double, string>, Func<Func<int, double>, Func<int, string>>>>()
                .Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source)); // (, 1.00)
            Trace.WriteLine(selectorWrapper2.Apply(selectorWrapper1.Apply(source))); // (c, 1.00)
            // Homomorphism: selector.Wrap().Apply(value.Wrap()) == selector(value).Wrap().
            Trace.WriteLine(selector.Tuple<string, Func<int, double>>().Apply(value.Tuple<string, int>())); // (, 2.23606797749979)
            Trace.WriteLine(selector(value).Tuple<string, double>()); // (, 2.23606797749979)
            // Interchange: selectorWrapper.Apply(value.Wrap()) == (selector => selector(value)).Wrap().Apply(selectorWrapper).
            Trace.WriteLine(selectorWrapper1.Apply(value.Tuple<string, int>())); // (b, 2.23606797749979)
            Trace.WriteLine(new Func<Func<int, double>, double>(function => function(value))
                .Tuple<string, Func<Func<int, double>, double>>().Apply(selectorWrapper1)); // (, 2.23606797749979)
        }
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<T> : IMonoidalFunctor<Task<>>
    {
        // Multiply: Task<T1> x Task<T2> -> Task<T1 x T2>
        // Multiply: (Task<T1>, Task<T2>) -> Task<Tuple<T1, T2>>
        public static async Task<Tuple<T1, T2>> Multiply<T1, T2>(this Task<T1> source1, Task<T2> source2) =>
            (await source1).Tuple(await source2); // Immediate execution, impure.

        // Unit: Unit -> Task<Unit>
        public static Task<Unit> Unit(Unit unit = default(Unit)) => System.Threading.Tasks.Task.FromResult(unit);
    }

    public static partial class TaskExtensions // Task<T> : IApplicativeFunctor<Task<>>
    {
        // Apply: (Task<TSource -> TResult>, Task<TSource>) -> Task<TResult>
        public static Task<TResult> Apply<TSource, TResult>(
            this Task<Func<TSource, TResult>> selectorWrapper, Task<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2)); // Immediate execution, impure.

        // Wrap: TSource -> Task<TSource>
        public static Task<T> Task<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion
}
