namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        // can be equivalently converted to:
        // IEnumerable<TResult> Select<TSource, TResult>(Func<TSource, TResult> selector, IEnumerable<TSource> source);

        // Other members.
    }

    public interface IEnumerable<T> : IFunctor<IEnumerable<>>, IEnumerable
    {
        // Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(Func<TSource, TResult> selector);
        // can be equivalently converted to:
        // IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector);

        // Other members.
    }
#endif

    #region IEnumerable<>

    public static partial class EnumerableExtensions // IEnumerable<T> : IFunctor<IEnumerable<>>
    {
        // Functor Select: (TSource -> TResult) -> (IEnumerable<TSource> -> IEnumerable<TResult>)
        public static Func<IEnumerable<TSource>, IEnumerable<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // 1. Uncurry to Select: (TSource -> TResult, IEnumerable<TSource>) -> IEnumerable<TResult>.
        // 2. Swap 2 parameters to Select: (IEnumerable<TSource>, TSource -> TResult) -> IEnumerable<TResult>.
        // 3. Define as LINQ extension method.
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
            // Map int to string.
            Func<int, string> selector = Convert.ToString;
            // Map IEnumerable<int> to IEnumerable<string>.
            IEnumerable<string> query = from value in source
                                        select selector(value); // Define query.
            query.ForEach(result => Trace.WriteLine(result)); // Execute query.
        }

        internal static void FunctorLaws()
        {
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };
            Func<int, double> selector1 = int32 => Math.Sqrt(int32);
            Func<double, string> selector2 = @double => @double.ToString("0.00");

            // Associativity preservation: source.Select(selector2.o(selector1)) == source.Select(selector1).Select(selector2).
            (from value in source
             select selector2.o(selector1)(value))
                .ForEach(result => Trace.WriteLine(result));  // 0.00 1.00 1.41 1.73 2.00
            (from value in source
             select selector1(value) into value
             select selector2(value))
                .ForEach(result => Trace.WriteLine(result));  // 0.00 1.00 1.41 1.73 2.00
            // Identity preservation: source.Select(Id) == Id(source).
            (from value in source
             select Id(value)).ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
            Id(source).ForEach(result => Trace.WriteLine(result)); // 0 1 2 3 4
        }
    }

    #endregion

    #region Lazy<>

    public static partial class LazyExtensions // Lazy<T> : IFunctor<Lazy<>>
    {
        // Functor Select: (TSource -> TResult) -> (Lazy<TSource> -> Lazy<TResult>)
        public static Func<Lazy<TSource>, Lazy<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // LINQ Select: (Lazy<TSource>, TSource -> TResult) -> Lazy<TResult>
        public static Lazy<TResult> Select<TSource, TResult>(
            this Lazy<TSource> source, Func<TSource, TResult> selector) =>
                new Lazy<TResult>(() => selector(source.Value));

        internal static void Map()
        {
            Lazy<int> source = new Lazy<int>(() => 1);
            // Map int to string.
            Func<int, string> selector = Convert.ToString;
            // Map Lazy<int> to Lazy<string>.
            Lazy<string> query = from value in source
                                 select selector(value); // Define query.
            string result = query.Value; // Execute query.
        }
    }

    public static partial class LazyExtensions // Lazy<T> : IFunctor<Lazy<>>
    {
#if DEMO
        public static Lazy<TResult> Select<TSource, TResult>(
            this Lazy<TSource> source, Func<TSource, TResult> selector) =>
                new Lazy<TResult>(() => default(TResult));
#endif
    }

    public static partial class LazyExtensions // Lazy<T> : IFunctor<Lazy<>>
    {
        internal static void FunctorLaws()
        {
            Lazy<int> lazy = new Lazy<int>(() => 1);
            Func<int, string> selector1 = Convert.ToString;
            Func<string, double> selector2 = Convert.ToDouble;

            // Associativity preservation: TFunctor<T>.Select(f2.o(f1)) == TFunctor<T>.Select(f1).Select(f2)
            Trace.WriteLine(lazy.Select(selector2.o(selector1)).Value); // 0
            Trace.WriteLine(lazy.Select(selector1).Select(selector2).Value); // 0
            // Identity preservation: TFunctor<T>.Select(Id) == Id(TFunctor<T>)
            Trace.WriteLine(lazy.Select(Id).Value); // 0
            Trace.WriteLine(Id(lazy).Value); // 1
        }
    }

    #endregion

    #region Func<>

    public static partial class FuncExtensions // Func<T> : IFunctor<Func<>>
    {
        // Functor Select: (TSource -> TResult) -> (Func<TSource> -> Func<TResult>)
        public static Func<Func<TSource>, Func<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // LINQ Select: (Func<TSource>, TSource -> TResult) -> Func<TResult>
        public static Func<TResult> Select<TSource, TResult>(
            this Func<TSource> source, Func<TSource, TResult> selector) =>
                () => selector(source());

        internal static void Map()
        {
            Func<int> source = () => 1;
            // Map int to string.
            Func<int, string> selector = Convert.ToString;
            // Map Func<int> to Func<string>.
            Func<string> query = from value in source
                                 select selector(value); // Define query.
            string result = query(); // Execute query.
        }
    }

    #endregion

    #region Func<T,>

    public static partial class FuncExtensions // Func<T, TResult> : IFunctor<Func<T,>>
    {
        // Functor Select: (TSource -> TResult) -> (Func<T, TSource> -> Func<T, TResult>)
        public static Func<Func<T, TSource>, Func<T, TResult>> Select<T, TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // LINQ Select: (Func<T, TSource>, TSource -> TResult) -> Func<T, TResult>
        public static Func<T, TResult> Select<T, TSource, TResult>(
            this Func<T, TSource> source, Func<TSource, TResult> selector) =>
                value => selector(source(value)); // selector.o(source);
    }

    public static partial class FuncExtensions
    {
        internal static void Map<T>(T input)
        {
            Func<T, string> source = value => value.ToString();
            // Map string to bool.
            Func<string, bool> selector = string.IsNullOrWhiteSpace;
            // Map Func<T, string> to Func<T, bool>.
            Func<T, bool> query = from value in source
                                  select selector(value); // Define query.
            bool result = query(input); // Execute query.

            // Equivalent to:
            Func<T, string> function1 = value => value.ToString();
            Func<string, bool> function2 = string.IsNullOrWhiteSpace;
            Func<T, bool> composition = function2.o(function1);
            result = composition(input);
        }
    }

    public static partial class FuncExtensions // Func<T1, T2, TResult> : IFunctor<Func<T1, T2,>>
    {
        // Functor Select: (TSource -> TResult) -> (Func<T1, T2, TSource> -> Func<T1, T2, TResult>)
        public static Func<Func<T1, T2, TSource>, Func<T1, T2, TResult>> Select<T1, T2, TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // LINQ Select: (Func<T1, T2, TSource>, TSource -> TResult) -> Func<T1, T2, TResult>
        public static Func<T1, T2, TResult> Select<T1, T2, TSource, TResult>(
            this Func<T1, T2, TSource> source, Func<TSource, TResult> selector) =>
                (value1, value2) => selector(source(value1, value2)); // selector.o(source);
    }

    #endregion

    #region Optional<>

    public static partial class OptionalExtensions // Optional<T> : IFunctor<Optional<>>
    {
        // Functor Select: (TSource -> TResult) -> (Optional<TSource> -> Optional<TResult>)
        public static Func<Optional<TSource>, Optional<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector);

        // LINQ Select: (Optional<TSource>, TSource -> TResult) -> Optional<TResult>
        public static Optional<TResult> Select<TSource, TResult>(
            this Optional<TSource> source, Func<TSource, TResult> selector) =>
                new Optional<TResult>(() => source.HasValue
                    ? true.Tuple(selector(source.Value)) : false.Tuple(default(TResult)));

#if DEMO
        internal static void Optional()
        {
            int int32 = 1;
            Func<int, string> function = Convert.ToString;

            Nullable<int> nullableInt32 = new Nullable<int>(int32);
            Nullable<Func<int, string>> nullableFunction = new Nullable<Func<int, string>>(function); // Cannot be compiled.
            Nullable<string> nullableString = new Nullable<string>(); // Cannot be compiled.

            Optional<int> optionalInt32 = new Optional<int>(() => true.Tuple(int32));
            Optional<Func<int, string>> optionalFunction = new Optional<Func<int, string>>(() => true.Tuple(function));
            Optional<string> optionalString = new Optional<string>(); // Equivalent to: new Optional<string>(() => false.Tuple(default(string)));
        }
#endif

        internal static void Map()
        {
            Optional<int> source1 = new Optional<int>(() => true.Tuple(1));
            // Map int to string.
            Func<int, string> selector = Convert.ToString;
            // Map Optional<int> to Optional<string>.
            Optional<string> query1 = from value in source1
                                      select selector(value); // Define query.
            string result1 = query1.Value; // Execute query.

            Optional<int> source2 = new Optional<int>();
            // Map Optional<int> to Optional<string>.
            Optional<string> query2 = from value in source2
                                      select selector(value); // Define query.
            bool result2 = query2.HasValue; // Execute query.
        }
    }

    #endregion

    #region Nullable<>

    public static partial class NullableExtensions // Nullable<T> : IFunctor<Nullable<>>
    {
        // Functor Select: (TSource -> TResult) -> (Nullable<TSource> -> Nullable<TResult>)
        public static Func<TSource?, TResult?> Select2<TSource, TResult>(
            Func<TSource, TResult> selector) where TSource : struct where TResult : struct => source =>
                Select(source, selector); // Immediate execution.

        // LINQ Select: (Nullable<TSource>, TSource -> TResult) -> Nullable<TResult>
        public static TResult? Select<TSource, TResult>(
            this TSource? source, Func<TSource, TResult> selector) where TSource : struct where TResult : struct =>
                source.HasValue ? selector(source.Value) : default(TResult?); // Immediate execution.

        internal static void Map()
        {
            long? source1 = 1L;
            // Map int to string.
            Func<long, TimeSpan> selector = TimeSpan.FromTicks;
            // Map Nullable<int> to Nullable<TimeSpan>.
            TimeSpan? query1 = from value in source1
                               select selector(value); // Define and execute query.
            TimeSpan result1 = query1.Value; // Query result.

            long? source2 = null;
            // Map Nullable<int> to Nullable<TimeSpan>.
            TimeSpan? query2 = from value in source2
                               select selector(value); // Define and execute query.
            bool result2 = query2.HasValue; // Query result.
        }
    }

    #endregion

    #region Tuple<>

    public static partial class TupleExtensions // Tuple<T> : IFunctor<Tuple<>>
    {
        // Functor Select: (TSource -> TResult) -> (Tuple<TSource> -> Tuple<TResult>)
        public static Func<Tuple<TSource>, Tuple<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate execution.

        // LINQ Select: (Tuple<TSource>, TSource -> TResult) -> Tuple<TResult>
        public static Tuple<TResult> Select<TSource, TResult>(
            this Tuple<TSource> source, Func<TSource, TResult> selector) =>
                new Tuple<TResult>(selector(source.Item1)); // Immediate execution.
    }

    public static partial class TupleExtensions // Tuple<T> : IFunctor<Tuple<>>
    {
        internal static void Map()
        {
            Tuple<int> source = new Tuple<int>(1);
            // Map int to string.
            Func<int, string> selector = int32 =>
                {
                    Trace.WriteLine($"{nameof(selector)} is called with {int32}.");
                    return Convert.ToString(int32);
                };
            // Map Tuple<int> to Tuple<string>.
            Tuple<string> query = from value in source // Define and execute query.
                                  select selector(value); // selector is called with 1.
            string result = query.Item1; // Query result.
        }
    }

    #endregion

    #region Tuple<T,>

    public static partial class TupleExtensions // Tuple<T, T2> : IFunctor<Tuple<T,>>
    {
        public static Tuple<T1, T2> Tuple<T1, T2>(this T1 item1, T2 item2) => new Tuple<T1, T2>(item1, item2);
    }

    public static partial class TupleExtensions // Tuple<T, T2> : IFunctor<Tuple<T,>>
    {
        // Functor Select: (TSource -> TResult) -> (Tuple<T, TSource> -> Tuple<T, TResult>)
        public static Func<Tuple<T, TSource>, Tuple<T, TResult>> Select<T, TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate execution.

        // LINQ Select: (Tuple<T, TSource>, TSource -> TResult) -> Tuple<T, TResult>
        public static Tuple<T, TResult> Select<T, TSource, TResult>(
            this Tuple<T, TSource> source, Func<TSource, TResult> selector) =>
                source.Item1.Tuple(selector(source.Item2)); // Immediate execution.

        internal static void Map<T>(T item1)
        {
            Tuple<T, int> source = new Tuple<T, int>(item1, 1);
            // Map int to string.
            Func<int, string> selector = int32 =>
            {
                Trace.WriteLine($"{nameof(selector)} is called with {int32}.");
                return Convert.ToString(int32);
            };
            // Map Tuple<T, int> to Tuple<T, string>.
            Tuple<T, string> query = from value in source // Define and execute query.
                                     select selector(value); // selector is called with 1.
            string result = query.Item2; // Query result.
        }
    }

    #endregion

    #region Task<>

    public static partial class TaskExtensions // Task<T> : IFunctor<Task<>>
    {
        // Functor Select: (TSource -> TResult) -> (Task<TSource> -> Task<TResult>)
        public static Func<Task<TSource>, Task<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate, impure.

        // LINQ Select: (Task<TSource>, TSource -> TResult) -> Task<TResult>
        public static async Task<TResult> Select<TSource, TResult>(
            this Task<TSource> source, Func<TSource, TResult> selector) =>
                selector(await source); // Immediate, impure.

        internal static async void Map()
        {
            Task<int> source = System.Threading.Tasks.Task.FromResult(1);
            // Map int to string.
            Func<int, string> selector = Convert.ToString;
            // Map Task<int> to Task<string>.
            Task<string> query = from value in source
                                 select selector(value); // Define and execute query.
            string result = await query; // Query result.
        }
    }

    #endregion

    public static partial class QueryableExtensions
    {
        // Functor Select: Expression<TSource -> TResult> -> (IQueryable<TSource> -> IQueryable<TResult>)
        public static Func<IQueryable<TSource>, IQueryable<TResult>> Select<TSource, TResult>(
            Expression<Func<TSource, TResult>> selector) => source =>
                Select(source, selector);

        // LINQ Select: (IQueryable<TSource>, Expression<TSource -> TResult>) -> IQueryable<TResult>
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
