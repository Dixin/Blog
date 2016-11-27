namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using static Dixin.Linq.CategoryTheory.Functions;

#if DEMO
    public interface IFunctor<TFunctor<>> where TFunctor<> : IFunctor<TFunctor<>>
    {
        // Select: (TSource -> TResult) -> (TFunctor<TSource> -> TFunctor<TResult>)
        Func<TFunctor<TSource>, TFunctor<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);
    }

    public interface IEnumerable<T> : IFunctor<IEnumerable<>>, IEnumerable
    {
        // Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);

        // Other members.
    }

    public interface IEnumerable<T> : IFunctor<IEnumerable<>>, IEnumerable
    {
        // Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);
        // is equivalent to:
        // IEnumerable<TResult> Select<TSource, TResult>(Func<TSource, TResult> selector, IEnumerable<TSource> source);

        // Other members.
    }

    public interface IEnumerable<T> : IFunctor<IEnumerable<>>, IEnumerable
    {
        // Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);
        // is equivalent to:
        // IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector);

        // Other members.
    }
#endif

    #region IEnumerable<>

    public static partial class EnumerableExtensions // IEnumerable<> : IFunctor<IEnumerable<>>
    {
        // Select: (TSource -> TResult) -> (IEnumerable<TSource> -> IEnumerable<TResult>)
        public static Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // 1. Uncurry to Select: (TSource -> TResult, IEnumerable<TSource>) -> IEnumerable<TResult>.
        // 2. Swap 2 parameters to Select: (IEnumerable<TSource>, TSource -> TResult) -> IEnumerable<TResult>.
        // 3. Define as extension method.
        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }
    }

    public partial class EnumerableExtensions
    {
        internal static void Map()
        {
            IEnumerable<int> source = System.Linq.Enumerable.Range(0, 5);
            // Map from int to string.
            Func<int, string> selector = Convert.ToString;
            // Map from TFunctor<int> to TFunctor<string>.
            IEnumerable<string> query = from value in source
                                        select selector(value);
        }

        internal static void FunctorLaws()
        {
            IEnumerable<int> enumerable = new int[] { 0, 1, 2, 3, 4 };
            Func<int, double> selector1 = int32 => Math.Sqrt(int32);
            Func<double, string> selector2 = @double => @double.ToString("0.00");

            // Identity preservation: f.Select(Id) == Id(f).
            (from value in enumerable
             select Id(value))
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            Id(enumerable)
                .ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            // Associativity preservation: f.Select(selector2.o(selector1)) == f.Select(selector1).Select(selector2).
            (from value in enumerable
             select selector2.o(selector1)(value))
                .ForEach(result => Trace.WriteLine(result));  // 0.00 1.00 1.41 1.73 2.00
            (from value in enumerable
             select selector1(value) into value
             select selector2(value))
                .ForEach(result => Trace.WriteLine(result));  // 0.00 1.00 1.41 1.73 2.00
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<> : IFunctor<Lazy<>>
    {
        // Select: (TSource -> TResult) -> (Lazy<TSource> -> Lazy<TResult>)
        public static Func<Lazy<TSource>, Lazy<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // Select: (Lazy<TSource>, TSource -> TResult) -> Lazy<TResult>
        public static Lazy<TResult> Select<TSource, TResult>(
            this Lazy<TSource> source, Func<TSource, TResult> selector) =>
                new Lazy<TResult>(() => selector(source.Value));
    }

    public static partial class LazyExtensions // Lazy<> : IFunctor<Lazy<>>
    {
#if DEMO
        public static Lazy<TResult> Select<TSource, TResult>(
            this Lazy<TSource> source, Func<TSource, TResult> selector) =>
                new Lazy<TResult>(() => default(TResult));
#endif
    }

    public static partial class LazyExtensions // Lazy<> : IFunctor<Lazy<>>
    {
        internal static void FunctorLaws()
        {
            Lazy<int> lazy = new Lazy<int>(() => 1);
            Func<int, long> f1 = x => x + 1;
            Func<long, string> f2 = x => x.ToString(CultureInfo.InvariantCulture);

            // Identity preservation: TFunctor<T>.Select(Id) == Id(TFunctor<T>)
            Trace.WriteLine(lazy.Select(Id).Value); // 0
            Trace.WriteLine(Id(lazy).Value); // 1
            // Associativity preservation: TFunctor<T>.Select(f2.o(f1)) == TFunctor<T>.Select(f1).Select(f2)
            Trace.WriteLine(lazy.Select(f2.o(f1)).Value); // null (default(string))
            Trace.WriteLine(lazy.Select(f1).Select(f2).Value); // null (default(string))
        }
    }

    #endregion

    #region Func<>

    public static partial class FuncExtensions // Func<> : IFunctor<Func<>>
    {
        // Select: (TSource -> TResult) -> (Func<TSource> -> Func<TResult>)
        public static Func<Func<TSource>, Func<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // Select: (Func<TSource>, TSource -> TResult) -> Func<TResult>
        public static Func<TResult> Select<TSource, TResult>(
            this Func<TSource> source, Func<TSource, TResult> selector) =>
                () => selector(source());
    }

    #endregion

    #region Func<T,>

    public static partial class FuncExtensions // Func<T, TResult> : IFunctor<Func<T,>>
    {
        // Select: (TSource -> TResult) -> (Func<T, TSource> -> Func<T, TResult>)
        public static Func<Func<T, TSource>, Func<T, TResult>> Select<T, TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // Select: (Func<T, TSource>, TSource -> TResult) -> Func<T, TResult>
        public static Func<T, TResult> Select<T, TSource, TResult>(
            this Func<T, TSource> source, Func<TSource, TResult> selector) => value =>
                selector(source(value)); // selector.o(source);
    }

    public static partial class FuncExtensions
    {
        internal static void Map<T>()
        {
            Func<T, string> source = value => value.ToString();
            // Map string to bool.
            Func<string, bool> selector = string.IsNullOrWhiteSpace;
            // Map Func<T, string> to Func<T, bool>.
            Func<T, bool> query = from value in source
                                  select selector(value);

            // Equivalent to:
            Func<T, string> function1 = value => value.ToString();
            Func<string, bool> function2 = string.IsNullOrWhiteSpace;
            Func<T, bool> composition = function2.o(function1);
        }
    }

    #endregion

    #region Optional<>

    public static partial class OptionalExtensions // Optional<> : IFunctor<Optional<>>
    {
        // Select: (TSource -> TResult) -> (Optional<TSource> -> Optional<TResult>)
        public static Func<Optional<TSource>, Optional<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // Select: (Optional<TSource>, TSource -> TResult) -> Optional<TResult>
        public static Optional<TResult> Select<TSource, TResult>(
            this Optional<TSource> source, Func<TSource, TResult> selector) =>
                new Optional<TResult>(() => source.HasValue
                    ? true.Tuple(selector(source.Value))
                    : false.Tuple(default(TResult)));
    }

    #endregion

    #region Nullable<>

    public static partial class NullableExtensions // Nullable<> : IFunctor<Nullable<>>
    {
        // Select: (TSource -> TResult) -> (Nullable<TSource> -> Nullable<TResult>)
        public static Func<TSource?, TResult?> Select2<TSource, TResult>(
            Func<TSource, TResult> selector) where TSource : struct where TResult : struct => source =>
                Select(source, selector); // Immediate.

        // Select: (Nullable<TSource>, TSource -> TResult) -> Nullable<TResult>
        public static TResult? Select<TSource, TResult>(
            this TSource? source, Func<TSource, TResult> selector) where TSource : struct where TResult : struct =>
                source.HasValue ? selector(source.Value) : default(TResult?); // Immediate.
    }

    #endregion

    #region Tuple<>

    public static partial class TupleExtensions // Tuple<> : IFunctor<Tuple<>>
    {
        // Select: (TSource -> TResult) -> (Tuple<TSource> -> Tuple<TResult>)
        public static Func<Tuple<TSource>, Tuple<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate.

        // Select: (Tuple<TSource>, TSource -> TResult) -> Tuple<TResult>
        public static Tuple<TResult> Select<TSource, TResult>(
            this Tuple<TSource> source, Func<TSource, TResult> selector) =>
                new Tuple<TResult>(selector(source.Item1)); // Immediate.
    }

    #endregion

    #region Tuple<T,>

    public static partial class TupleExtensions // Tuple<T, T2> : IFunctor<Tuple<T,>>
    {
        public static Tuple<T1, T2> Tuple<T1, T2>(this T1 item1, T2 item2) => new Tuple<T1, T2>(item1, item2);
    }

    public static partial class TupleExtensions // Tuple<T, T2> : IFunctor<Tuple<T,>>
    {
        // Select: (TSource -> TResult) -> (Tuple<T, TSource> -> Tuple<T, TResult>)
        public static Func<Tuple<T, TSource>, Tuple<T, TResult>> Select<T, TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate.

        // Select: (Tuple<T, TSource>, TSource -> TResult) -> Tuple<T, TResult>
        public static Tuple<T, TResult> Select<T, TSource, TResult>(
            this Tuple<T, TSource> source, Func<TSource, TResult> selector) =>
                source.Item1.Tuple(selector(source.Item2)); // Immediate.
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<> : IFunctor<Task<>>
    {
        // Select: (TSource -> TResult) -> (Task<TSource> -> Task<TResult>)
        public static Func<Task<TSource>, Task<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate, impure.

        // Select: (Task<TSource>, TSource -> TResult) -> Task<TResult>
        public static async Task<TResult> Select<TSource, TResult>(
            this Task<TSource> source, Func<TSource, TResult> selector) =>
                selector(await source); // Immediate, impure.
    }

    #endregion 

    public static partial class QueryableExtensions
    {
        // Select: Expression<TSource -> TResult> -> (IQueryable<TSource> -> IQueryable<TResult>)
        public static Func<IQueryable<TSource>, IQueryable<TResult>> Select<TSource, TResult>(
            Expression<Func<TSource, TResult>> selector) => source =>
                Select(source, selector);

        // Select: (IQueryable<TSource>, Expression<TSource -> TResult>) -> IQueryable<TResult>
        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) =>
                source.Provider.CreateQuery<TResult>(Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)),
                    new Expression[] { source.Expression, Expression.Quote(selector) }));
    }
}

#if DEMO
namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);
    }
}
#endif
