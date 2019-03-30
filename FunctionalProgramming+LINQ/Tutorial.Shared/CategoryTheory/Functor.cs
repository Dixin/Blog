namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using static Tutorial.CategoryTheory.Functions;

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

    public static partial class FunctorExtensions
    {
        // Cannot be compiled.
        internal static void Map<TFunctor<>, TSource, TResult>( // Non generic TFunctor can work too.
            TFunctor<TSource> functor, Func<TSource, TResult> selector) where TFunctor<> : IFunctor<TFunctor<>>
        {
            TFunctor<TResult> query = from /* TSource */ value in /* TFunctor<TSource> */ functor
                                      select /* TResult */ selector(value); // Define query.
        }
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
            query.WriteLines(); // Execute query.
        }

        // using static Tutorial.CategoryTheory.Functions;
        internal static void FunctorLaws()
        {
            IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4 };
            Func<int, double> selector1 = int32 => Math.Sqrt(int32);
            Func<double, string> selector2 = @double => @double.ToString("0.00");

            // Associativity preservation: source.Select(selector2.o(selector1)) == source.Select(selector1).Select(selector2).
            (from value in source
             select selector2.o(selector1)(value)).WriteLines();  // 0.00 1.00 1.41 1.73 2.00
            (from value in source
             select selector1(value) into value
             select selector2(value)).WriteLines();  // 0.00 1.00 1.41 1.73 2.00
            // Identity preservation: source.Select(Id) == Id(source).
            (from value in source
             select Id(value)).WriteLines(); // 0 1 2 3 4
            Id(source).WriteLines(); // 0 1 2 3 4
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
                new Lazy<TResult>(() => default);
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
            lazy.Select(selector2.o(selector1)).Value.WriteLine(); // 0
            lazy.Select(selector1).Select(selector2).Value.WriteLine(); // 0
            // Identity preservation: TFunctor<T>.Select(Id) == Id(TFunctor<T>)
            lazy.Select(Id).Value.WriteLine(); // 0
            Id(lazy).Value.WriteLine(); // 1
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
            Func<T, string> function1 = source;
            Func<string, bool> function2 = selector;
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
#if DEMO
        internal static void Optional()
        {
            int int32 = 1;
            Func<int, string> function = Convert.ToString;

            Nullable<int> nullableInt32 = new Nullable<int>(int32);
            Nullable<Func<int, string>> nullableFunction = new Nullable<Func<int, string>>(function); // Cannot be compiled.
            Nullable<string> nullableString = new Nullable<string>(); // Cannot be compiled.

            Optional<int> optionalInt32 = new Optional<int>(() => (true, int32));
            Optional<Func<int, string>> optionalFunction = new Optional<Func<int, string>>(() => true, function));
            Optional<string> optionalString = new Optional<string>(); // Equivalent to: new Optional<string>(() => false, default);
        }
#endif
    }

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
                    ? (true, selector(source.Value)) : (false, default));

        internal static void Map()
        {
            Optional<int> source1 = new Optional<int>(() => (true, 1));
            // Map int to string.
            Func<int, string> selector = Convert.ToString;
            // Map Optional<int> to Optional<string>.
            Optional<string> query1 = from value in source1
                                      select selector(value); // Define query.
            if (query1.HasValue) // Execute query.
            {
                string result1 = query1.Value;
            }

            Optional<int> source2 = new Optional<int>();
            // Map Optional<int> to Optional<string>.
            Optional<string> query2 = from value in source2
                                      select selector(value); // Define query.
            if (query2.HasValue) // Execute query.
            {
                string result2 = query2.Value;
            }
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
                source.HasValue ? selector(source.Value) : default; // Immediate execution.

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

    #region ValueTuple<>

    public static partial class ValueTupleExtensions // ValueTuple<T> : IFunctor<ValueTuple<>>
    {
        // Functor Select: (TSource -> TResult) -> (ValueTuple<TSource> -> ValueTuple<TResult>)
        public static Func<ValueTuple<TSource>, ValueTuple<TResult>> Select<TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate execution.

        // LINQ Select: (ValueTuple<TSource>, TSource -> TResult) -> ValueTuple<TResult>
        public static ValueTuple<TResult> Select<TSource, TResult>(
            this ValueTuple<TSource> source, Func<TSource, TResult> selector) =>
                new ValueTuple<TResult>(selector(source.Item1)); // Immediate execution.
    }

    public static partial class ValueTupleExtensions // ValueTuple<T> : IFunctor<ValueTuple<>>
    {
        internal static void Map()
        {
            ValueTuple<int> source = new ValueTuple<int>(1);
            // Map int to string.
            Func<int, string> selector = int32 =>
                {
                    $"{nameof(selector)} is called with {int32}.".WriteLine();
                    return Convert.ToString(int32);
                };
            // Map ValueTuple<int> to ValueTuple<string>.
            ValueTuple<string> query = from value in source // Define and execute query.
                                       select selector(value); // selector is called with 1.
            string result = query.Item1; // Query result.
        }
    }

    #endregion

    #region ValueTuple<T,>

    public static partial class ValueTupleExtensions // ValueTuple<T, T2> : IFunctor<ValueTuple<T,>>
    {
        // Functor Select: (TSource -> TResult) -> (ValueTuple<T, TSource> -> ValueTuple<T, TResult>)
        public static Func<(T, TSource), (T, TResult)> Select<T, TSource, TResult>(
            Func<TSource, TResult> selector) => source =>
                Select(source, selector); // Immediate execution.

        // LINQ Select: (ValueTuple<T, TSource>, TSource -> TResult) -> ValueTuple<T, TResult>
        public static (T, TResult) Select<T, TSource, TResult>(
            this(T, TSource) source, Func<TSource, TResult> selector) =>
                (source.Item1, selector(source.Item2)); // Immediate execution.

        internal static void Map<T>(T item1)
        {
            (T, int) source = (item1, 1);
            // Map int to string.
            Func<int, string> selector = int32 =>
            {
                $"{nameof(selector)} is called with {int32}.".WriteLine();
                return Convert.ToString(int32);
            };
            // Map ValueTuple<T, int> to ValueTuple<T, string>.
            (T, string) query = from value in source // Define and execute query.
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
                Select(source, selector); // Immediate execution, impure.

        // LINQ Select: (Task<TSource>, TSource -> TResult) -> Task<TResult>
        public static async Task<TResult> Select<TSource, TResult>(
            this Task<TSource> source, Func<TSource, TResult> selector) =>
                selector(await source); // Immediate execution, impure.

        internal static async Task MapAsync()
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
