namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    using static Tutorial.CategoryTheory.DotNetCategory;

#if DEMO
    // Cannot be compiled.
    public interface IMonoidalFunctor<TMonoidalFunctor<>> : IFunctor<TMonoidalFunctor<>>
         where TMonoidalFunctor<> : IMonoidalFunctor<TMonoidalFunctor<>>
    {
        // From IFunctor<TMonoidalFunctor<>>:
        // Select: (TSource -> TResult) -> (TMonoidalFunctor<TSource> -> TMonoidalFunctor<TResult>)
        // Func<TMonoidalFunctor<TSource>, TMonoidalFunctor<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Multiply: TMonoidalFunctor<T1> x TMonoidalFunctor<T2> -> TMonoidalFunctor<T1 x T2>
        // Multiply: ValueTuple<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> -> TMonoidalFunctor<ValueTuple<T1, T2>>
        TMonoidalFunctor<ValueTuple<T1, T2>> Multiply<T1, T2>(
            ValueTuple<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> bifunctor);

        // Unit: Unit -> TMonoidalFunctor<Unit>
        TMonoidalFunctor<Unit> Unit(Unit unit);
    }

    // Cannot be compiled.
    public interface IMonoidalFunctor<TMonoidalFunctor<>> : IFunctor<TMonoidalFunctor<>>
         where TMonoidalFunctor<> : IMonoidalFunctor<TMonoidalFunctor<>>
    {
        // Multiply: TMonoidalFunctor<T1> x TMonoidalFunctor<T2> -> TMonoidalFunctor<T1 x T2>
        // Multiply: (TMonoidalFunctor<T1>, TMonoidalFunctor<T2>) -> TMonoidalFunctor<(T1, T2)>
        TMonoidalFunctor<(T1, T2)> Multiply<T1, T2>(
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
        // Multiply: (TApplicativeFunctor<T1>, TApplicativeFunctor<T2>) -> TApplicativeFunctor<(T1, T2)>
        public static TApplicativeFunctor<(T1, T2)> Multiply<TApplicativeFunctor<>, T1, T2>(
            this TApplicativeFunctor<T1> source1, TApplicativeFunctor<T2> source2) 
            where TApplicativeFunctor<> : IApplicativeFunctor<TApplicativeFunctor<>> =>
                new Func<T1, T2, (T1, T2)>(ValueTuple.Create).Curry().Wrap().Apply(source1).Apply(source2);

        // Unit: Unit -> TApplicativeFunctor<Unit>
        public static TApplicativeFunctor<Unit> Unit<TApplicativeFunctor<>>(Unit unit = default(Unit))
            where TApplicativeFunctor<> : IApplicativeFunctor<TApplicativeFunctor<>> => unit.Wrap();
    }
#endif

    #region IEnumerable<>

    public static partial class EnumerableExtensions // IEnumerable<T> : IMonoidalFunctor<IEnumerable<>>
    {
        // Multiply: IEnumerable<T1> x IEnumerable<T2> -> IEnumerable<T1 x T2>
        // Multiply: ValueTuple<IEnumerable<T1>, IEnumerable<T2>> -> IEnumerable<ValueTuple<T1, T2>>
        // Multiply: (IEnumerable<T1>, IEnumerable<T2>) -> IEnumerable<(T1, T2)>
        public static IEnumerable<(T1, T2)> Multiply<T1, T2>(
            this IEnumerable<T1> source1, IEnumerable<T2> source2) // Implicit tuple.
        {
            foreach (T1 value1 in source1)
            {
                foreach (T2 value2 in source2)
                {
                    yield return (value1, value2);
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
            IEnumerable<(Func<long, Func<double, bool>>, long)> multiplyWithYs = applyWithXs.Multiply(ys);
            IEnumerable<Func<double, bool>> applyWithYs = multiplyWithYs.Select(product =>
            {
                Func<long, Func<double, bool>> partialAppliedSelector = product.Item1;
                long y = product.Item2;
                return partialAppliedSelector(y);
            });
            // Partially apply selector with zs.
            IEnumerable<(Func<double, bool>, double)> multiplyWithZs = applyWithYs.Multiply(zs);
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
        // Multiply: (IEnumerable<T1>, IEnumerable<T2>) -> IEnumerable<(T1, T2)>
        public static IEnumerable<(T1, T2)> Multiply<T1, T2>(
            this IEnumerable<T1> source1, IEnumerable<T2> source2) =>
                new Func<T1, T2, (T1, T2)>(ValueTuple.Create).Curry().Enumerable().Apply(source1).Apply(source2);

        // Unit: Unit -> IEnumerable<Unit>
        public static IEnumerable<Unit> Unit(Unit unit = default(Unit)) => unit.Enumerable();
    }
#endif

    public static partial class EnumerableExtensions
    {
        // using static Tutorial.CategoryTheory.DotNetCategory;
        internal static void MonoidalFunctorLaws()
        {
            IEnumerable<Unit> unit = Unit();
            IEnumerable<int> source1 = new int[] { 0, 1 };
            IEnumerable<char> source2 = new char[] { '@', '#' };
            IEnumerable<bool> source3 = new bool[] { true, false };
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };

            // Associativity preservation: source1.Multiply(source2).Multiply(source3).Select(Associator) == source1.Multiply(source2.Multiply(source3)).
            source1.Multiply(source2).Multiply(source3).Select(Associator).WriteLines();
                // (0, (@, True)) (0, (@, False)) (0, (#, True)) (0, (#, False))
                // (1, (@, True)) (1, (@, False)) (1, (#, True)) (1, (#, False))
            source1.Multiply(source2.Multiply(source3)).WriteLines();
                // (0, (@, True)) (0, (@, False)) (0, (#, True)) (0, (#, False))
                // (1, (@, True)) (1, (@, False)) (1, (#, True)) (1, (#, False))
            // Left unit preservation: unit.Multiply(source).Select(LeftUnitor) == source.
            unit.Multiply(source).Select(LeftUnitor).WriteLines(); // 0 1 2 3 4
            // Right unit preservation: source == source.Multiply(unit).Select(RightUnitor).
            source.Multiply(unit).Select(RightUnitor).WriteLines(); // 0 1 2 3 4
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
                new Func<Func<double, string>, Func<int, double>, Func<int, string>>(Tutorial.FuncExtensions.o).Curry();
            int value = 5;

            // Functor preservation: source.Select(selector) == selector.Wrap().Apply(source).
            source.Select(selector).WriteLines(); // 0 1 1.4142135623731 1.73205080756888 2
            selector.Enumerable().Apply(source).WriteLines(); // 0 1 1.4142135623731 1.73205080756888 2
            // Identity preservation: Id.Wrap().Apply(source) == source.
            new Func<int, int>(Functions.Id).Enumerable().Apply(source).WriteLines(); // 0 1 2 3 4
            // Composition preservation: o.Wrap().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source) == selectorWrapper2.Apply(selectorWrapper1.Apply(source)).
            o.Enumerable().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source).WriteLines();
                // 0.0  0.5  1.0  1.5  2.0
                // 0.0  1.0  1.4  1.7  2.0 
                // 0.00 0.50 1.00 1.50 2.00
                // 0.00 1.00 1.41 1.73 2.00
            selectorWrapper2.Apply(selectorWrapper1.Apply(source)).WriteLines();
                // 0.0  0.5  1.0  1.5  2.0
                // 0.0  1.0  1.4  1.7  2.0 
                // 0.00 0.50 1.00 1.50 2.00
                // 0.00 1.00 1.41 1.73 2.00
            // Homomorphism: selector.Wrap().Apply(value.Wrap()) == selector(value).Wrap().
            selector.Enumerable().Apply(value.Enumerable()).WriteLines(); // 2.23606797749979
            selector(value).Enumerable().WriteLines(); // 2.23606797749979
            // Interchange: selectorWrapper.Apply(value.Wrap()) == (selector => selector(value)).Wrap().Apply(selectorWrapper).
            selectorWrapper1.Apply(value.Enumerable()).WriteLines(); // 2.5 2.23606797749979
            new Func<Func<int, double>, double>(function => function(value)).Enumerable().Apply(selectorWrapper1)
                .WriteLines(); // 2.5 2.23606797749979
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<T> : IMonoidalFunctor<Lazy<>>
    {
        // Multiply: Lazy<T1> x Lazy<T2> -> Lazy<T1 x T2>
        // Multiply: (Lazy<T1>, Lazy<T2>) -> Lazy<(T1, T2)>
        public static Lazy<(T1, T2)> Multiply<T1, T2>(this Lazy<T1> source1, Lazy<T2> source2) =>
            new Lazy<(T1, T2)>(() => (source1.Value, source2.Value));

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
        // Multiply: (Func<T1>, Func<T2>) -> Func<(T1, T2)>
        public static Func<(T1, T2)> Multiply<T1, T2>(this Func<T1> source1, Func<T2> source2) =>
            () => (source1(), source2());

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
        // Multiply: (Func<T, T1>, Func<T, T2>) -> Func<T, (T1, T2)>
        public static Func<T, (T1, T2)> Multiply<T, T1, T2>(this Func<T, T1> source1, Func<T, T2> source2) =>
            value => (source1(value), source2(value));

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
        // Multiply: (Optional<T1>, Optional<T2>) -> Optional<(T1, T2)>
        public static Optional<(T1, T2)> Multiply<T1, T2>(this Optional<T1> source1, Optional<T2> source2) =>
            new Optional<(T1, T2)>(() => source1.HasValue && source2.HasValue
                ? (true, (source1.Value, source2.Value))
                : (false, (default(T1), default(T2))));

        // Unit: Unit -> Optional<Unit>
        public static Optional<Unit> Unit(Unit unit = default(Unit)) =>
            new Optional<Unit>(() => (true, unit));
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

    #region ValueTuple<>

    public static partial class ValueTupleExtensions // ValueTuple<T> : IMonoidalFunctor<ValueTuple<>>
    {
        // Multiply: ValueTuple<T1> x ValueTuple<T2> -> ValueTuple<T1 x T2>
        // Multiply: (ValueTuple<T1>, ValueTuple<T2>) -> ValueTuple<(T1, T2)>
        public static ValueTuple<(T1, T2)> Multiply<T1, T2>(this ValueTuple<T1> source1, ValueTuple<T2> source2) =>
            new ValueTuple<(T1, T2)>((source1.Item1, source2.Item1)); // Immediate execution.

        // Unit: Unit -> ValueTuple<Unit>
        public static ValueTuple<Unit> Unit(Unit unit = default(Unit)) => new ValueTuple<Unit>(unit);
    }

    public static partial class ValueTupleExtensions // ValueTuple<T> : IApplicativeFunctor<ValueTuple<>>
    {
        // Apply: (ValueTuple<TSource -> TResult>, ValueTuple<TSource>) -> ValueTuple<TResult>
        public static ValueTuple<TResult> Apply<TSource, TResult>(
            this ValueTuple<Func<TSource, TResult>> selectorWrapper, ValueTuple<TSource> source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2)); // Immediate execution.

        // Wrap: TSource -> ValueTuple<TSource>
        public static ValueTuple<T> ValueTuple<T>(this T value) => Unit().Select(unit => value);
    }

    #endregion

    #region ValueTuple<T,>

    public static partial class ValueTupleExtensions // ValueTuple<T1, T2 : IMonoidalFunctor<ValueTuple<T,>>
    {
        // Multiply: ValueTuple<T, T1> x ValueTuple<T, T2> -> ValueTuple<T, T1 x T2>
        // Multiply: (ValueTuple<T, T1>, ValueTuple<T, T2>) -> ValueTuple<T, (T1, T2)>
        public static (T, (T1, T2)) Multiply<T, T1, T2>(this (T, T1) source1, (T, T2) source2) =>
            (source1.Item1, (source1.Item2, source2.Item2)); // Immediate execution.

        // Unit: Unit -> ValueTuple<Unit>
        public static (T, Unit) Unit<T>(Unit unit = default(Unit)) => (default(T), unit);
    }

    public static partial class ValueTupleExtensions // ValueTuple<T, TResult> : IApplicativeFunctor<ValueTuple<T,>>
    {
        // Apply: (ValueTuple<T, TSource -> TResult>, ValueTuple<T, TSource>) -> ValueTuple<T, TResult>
        public static (T, TResult) Apply<T, TSource, TResult>(
            this (T, Func<TSource, TResult>) selectorWrapper, (T, TSource) source) =>
                selectorWrapper.Multiply(source).Select(product => product.Item1(product.Item2)); // Immediate execution.

        // Wrap: TSource -> ValueTuple<T, TSource>
        public static (T, TSource) ValueTuple<T, TSource>(this TSource value) => Unit<T>().Select(unit => value);
    }

    public static partial class ValueTupleExtensions
    {
        internal static void MonoidalFunctorLaws()
        {
            (string, int) source = ("a", 1);
            (string, Unit) unit = Unit<string>();
            (string, int) source1 = ("b", 2);
            (string, char) source2 = ("c", '@');
            (string, bool) source3 = ("d", true);

            // Associativity preservation: source1.Multiply(source2).Multiply(source3).Select(Associator) == source1.Multiply(source2.Multiply(source3)).
            source1.Multiply(source2).Multiply(source3).Select(Associator).WriteLine(); // (b, (2, (@, True)))
            source1.Multiply(source2.Multiply(source3)).WriteLine(); // (b, (2, (@, True)))
            // Left unit preservation: unit.Multiply(source).Select(LeftUnitor) == source.
            unit.Multiply(source).Select(LeftUnitor).WriteLine(); // (, 1)
            // Right unit preservation: source == source.Multiply(unit).Select(RightUnitor).
            source.Multiply(unit).Select(RightUnitor).WriteLine(); // (a, 1)
        }

        internal static void ApplicativeLaws()
        {
            (string, int) source = ("a", 1);
            Func<int, double> selector = int32 => Math.Sqrt(int32);
            (string, Func<int, double>) selectorWrapper1 = 
                ("b", new Func<int, double>(int32 => Math.Sqrt(int32)));
            (string, Func<double, string>) selectorWrapper2 =
                ("c", new Func<double, string>(@double => @double.ToString("0.00")));
            Func<Func<double, string>, Func<Func<int, double>, Func<int, string>>> o = 
                new Func<Func<double, string>, Func<int, double>, Func<int, string>>(Tutorial.FuncExtensions.o).Curry();
            int value = 5;

            // Functor preservation: source.Select(selector) == selector.Wrap().Apply(source).
            source.Select(selector).WriteLine(); // (a, 1)
            selector.ValueTuple<string, Func<int, double>>().Apply(source).WriteLine(); // (, 1)
            // Identity preservation: Id.Wrap().Apply(source) == source.
            new Func<int, int>(Functions.Id).ValueTuple<string, Func<int, int>>().Apply(source).WriteLine(); // (, 1)
            // Composition preservation: o.Curry().Wrap().Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source) == selectorWrapper2.Apply(selectorWrapper1.Apply(source)).
            o.ValueTuple<string, Func<Func<double, string>, Func<Func<int, double>, Func<int, string>>>>()
                .Apply(selectorWrapper2).Apply(selectorWrapper1).Apply(source).WriteLine(); // (, 1.00)
            selectorWrapper2.Apply(selectorWrapper1.Apply(source)).WriteLine(); // (c, 1.00)
            // Homomorphism: selector.Wrap().Apply(value.Wrap()) == selector(value).Wrap().
            selector.ValueTuple<string, Func<int, double>>().Apply(value.ValueTuple<string, int>()).WriteLine(); // (, 2.23606797749979)
            selector(value).ValueTuple<string, double>().WriteLine(); // (, 2.23606797749979)
            // Interchange: selectorWrapper.Apply(value.Wrap()) == (selector => selector(value)).Wrap().Apply(selectorWrapper).
            selectorWrapper1.Apply(value.ValueTuple<string, int>()).WriteLine(); // (b, 2.23606797749979)
            new Func<Func<int, double>, double>(function => function(value))
                .ValueTuple<string, Func<Func<int, double>, double>>().Apply(selectorWrapper1).WriteLine(); // (, 2.23606797749979)
        }
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<T> : IMonoidalFunctor<Task<>>
    {
        // Multiply: Task<T1> x Task<T2> -> Task<T1 x T2>
        // Multiply: (Task<T1>, Task<T2>) -> Task<(T1, T2)>
        public static async Task<(T1, T2)> Multiply<T1, T2>(this Task<T1> source1, Task<T2> source2) =>
            ((await source1), (await source2)); // Immediate execution, impure.

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
