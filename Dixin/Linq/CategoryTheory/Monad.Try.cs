namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Globalization;

    public delegate TryResult<T> Try<T>();

    public struct TryResult<T>
    {
        public TryResult(T value)
        {
            this.Value = value;
            this.Exception = null;
        }

        public TryResult(Exception exception)
        {
            this.Value = default(T);
            this.Exception = exception;
        }

        public T Value { get; }

        public Exception Exception { get; }

        public bool HasException => this.Exception != null;

        public static implicit operator TryResult<T>(T value) => new TryResult<T>(value);
    }

    public static partial class TryExtensions
    {
        public static Try<TResult> SelectMany<TSource, TSelector, TResult>(
            this Try<TSource> source,
            Func<TSource, Try<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) => () =>
            {
                try
                {
                    TryResult<TSource> value = source();
                    if (value.HasException)
                    {
                        return new TryResult<TResult>(value.Exception);
                    }
                    TryResult<TSelector> result = selector(value.Value)();
                    if (result.HasException)
                    {
                        return new TryResult<TResult>(result.Exception);
                    }
                    return resultSelector(value.Value, result.Value);
                }
                catch (Exception exception)
                {
                    return new TryResult<TResult>(exception);
                }
            };

        // Wrap: TSource -> Try<TSource>
        public static Try<TSource> Try<TSource>(this TSource value) => () => value;
    }

    public static partial class TryExtensions
    {
        public static TryResult<T> Throw<T>(this Exception exception) => new TryResult<T>(exception);

        public static Try<T> Try<T>(Func<TryResult<T>> function) =>
            () =>
            {
                try
                {
                    return function();
                }
                catch (Exception exception)
                {
                    return new TryResult<T>(exception);
                }
            };

        public static Try<T> Try<T>(Func<T> function) => Try(() => new TryResult<T>(function()));

        public static TryResult<T> Catch<T, TException>(
            this TryResult<T> result, Func<TException, TryResult<T>> handler, Func<TException, bool> when = null)
            where TException : Exception
        {
            TException exception;
            return result.HasException && (exception = result.Exception as TException) != null 
                && (when == null || when(exception)) ? handler(exception) : result;
        }

        public static TryResult<T> Catch<T>(
            this TryResult<T> result, Func<Exception, TryResult<T>> handler, Func<Exception, bool> when = null) =>
                Catch<T, Exception>(result, handler, when);

        public static TryResult<TResult> Finally<T, TResult>(
            this TryResult<T> result, Func<TryResult<T>, TResult> finnally) => finnally(result);
    }

    public static partial class TryExtensions
    {
        internal static TryResult<int> StrictFactorial(int? value)
        {
            if (value == null)
            {
                return Throw<int>(new ArgumentNullException(nameof(value)));
            }
            if (value <= 0)
            {
                return Throw<int>(new ArgumentOutOfRangeException(nameof(value), value, "Argument should be positive."));
            }
            if (value == 1)
            {
                return 1;
            }
            return value.Value * StrictFactorial(value - 1).Value;
        }

        internal static TryResult<string> Factorial(string value)
        {
            Func<string, int?> stringToNullableInt32 =
                @string => string.IsNullOrEmpty(value) ? default(int?) : Convert.ToInt32(@string);
            Try<int> query = from int32 in Try(() => stringToNullableInt32(value))
                             from result in Try(() => StrictFactorial(int32))
                             select result; // Define query.
            return query() // Execute query.
                .Catch<int, ArgumentNullException>(exception => 0)
                .Catch<int, ArgumentOutOfRangeException>(
                    when: exception => object.Equals(exception.ActualValue, 0),
                    handler: exception => 0)
                .Finally(result => result.HasException
                    ? result.Exception.Message : result.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
