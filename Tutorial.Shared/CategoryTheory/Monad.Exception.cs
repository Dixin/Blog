namespace Tutorial.CategoryTheory
{
    using System;

    using Microsoft.FSharp.Core;

    public readonly struct Try<T>
    {
        private readonly Lazy<(T, Exception)> factory;

        public Try(Func<(T, Exception)> factory) =>
            this.factory = new Lazy<(T, Exception)>(() =>
            {
                try
                {
                    return factory();
                }
                catch (Exception exception)
                {
                    return (default, exception);
                }
            });

        public T Value
        {
            get
            {
                if (this.HasException)
                {
                    throw new InvalidOperationException($"{nameof(Try<T>)} object must have a value.");
                }
                return this.factory.Value.Item1;
            }
        }

        public Exception Exception => this.factory.Value.Item2;

        public bool HasException => this.Exception != null;

        public static implicit operator Try<T>(T value) => new Try<T>(() => (value, (Exception)null));
    }

    public static partial class TryExtensions
    {
        // SelectMany: (Try<TSource>, TSource -> Try<TSelector>, (TSource, TSelector) -> TResult) -> Try<TResult>
        public static Try<TResult> SelectMany<TSource, TSelector, TResult>(
            this Try<TSource> source,
            Func<TSource, Try<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                new Try<TResult>(() =>
                {
                    if (source.HasException)
                    {
                        return (default, source.Exception);
                    }
                    Try<TSelector> result = selector(source.Value);
                    if (result.HasException)
                    {
                        return (default, result.Exception);
                    }
                    return (resultSelector(source.Value, result.Value), (Exception)null);
                });

        // Wrap: TSource -> Try<TSource>
        public static Try<TSource> Try<TSource>(this TSource value) => value;

        // Select: (Try<TSource>, TSource -> TResult) -> Try<TResult>
        public static Try<TResult> Select<TSource, TResult>(
            this Try<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).Try(), (value, result) => result);
    }

    public static partial class TryExtensions
    {
        public static Try<T> Throw<T>(this Exception exception) => new Try<T>(() => (default, exception));
    }

    public static partial class TryExtensions
    {
        public static Try<T> Try<T>(Func<T> function) =>
            new Try<T>(() => (function(), (Exception)null));

        public static Try<Unit> Try(Action action) =>
            new Try<Unit>(() =>
            {
                action();
                return (default, (Exception)null);
            });

        public static Try<T> Catch<T, TException>(
            this Try<T> source, Func<TException, Try<T>> handler, Func<TException, bool> when = null)
            where TException : Exception => 
                new Try<T>(() =>
                {
                    if (source.HasException && source.Exception is TException exception && exception != null
                        && (when == null || when(exception)))
                    {
                        source = handler(exception);
                    }
                    return source.HasException ? (default, source.Exception) : (source.Value, (Exception)null);
                });

        public static Try<T> Catch<T>(
            this Try<T> source, Func<Exception, Try<T>> handler, Func<Exception, bool> when = null) =>
                Catch<T, Exception>(source, handler, when);

        public static TResult Finally<T, TResult>(
            this Try<T> source, Func<Try<T>, TResult> @finally) => @finally(source);

        public static void Finally<T>(
            this Try<T> source, Action<Try<T>> @finally) => @finally(source);
    }

    public static partial class TryExtensions
    {
        internal static Try<int> TryStrictFactorial(int? value)
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
            return value.Value * TryStrictFactorial(value - 1).Value;
        }

        internal static string Factorial(string value)
        {
            Func<string, int?> stringToNullableInt32 = @string =>
                string.IsNullOrEmpty(@string) ? default : Convert.ToInt32(@string);
            Try<int> query = from nullableInt32 in Try(() => stringToNullableInt32(value)) // Try<int32?>
                             from result in TryStrictFactorial(nullableInt32) // Try<int>.
                             from unit in Try(() => result.WriteLine()) // Try<Unit>.
                             select result; // Define query.
            return query
                .Catch(exception => // Catch all and rethrow.
                {
                    exception.WriteLine();
                    return Throw<int>(exception);
                })
                .Catch<int, ArgumentNullException>(exception => 1) // When argument is null, factorial is 1.
                .Catch<int, ArgumentOutOfRangeException>(
                    when: exception => object.Equals(exception.ActualValue, 0),
                    handler: exception => 1) // When argument is 0, factorial is 1.
                .Finally(result => result.HasException // Execute query.
                    ? result.Exception.Message : result.Value.ToString());
        }
    }
}
