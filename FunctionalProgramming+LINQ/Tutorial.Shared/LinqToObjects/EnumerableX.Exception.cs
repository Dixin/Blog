namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    using static Retry;

    public class ExceptionDetection : ITransientErrorDetectionStrategy
    {
        private readonly Func<Exception, bool> isTransient;

        public ExceptionDetection(Func<Exception, bool> isTransient = null) => 
            this.isTransient = isTransient ?? (_ => true);

        public bool IsTransient(Exception exception) => this.isTransient(exception);
    }

    public static partial class Retry
    {
        public static TResult Execute<TResult>(
            Func<TResult> func,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null)
        {
            RetryPolicy retryPolicy = new RetryPolicy(
                new ExceptionDetection(isTransient), retryStrategy ?? RetryStrategy.DefaultFixed);
            if (retryingHandler != null)
            {
                retryPolicy.Retrying += retryingHandler;
            }

            return retryPolicy.ExecuteAction(func);
        }
    }

    public static partial class Retry
    {
        public static FixedInterval FixedInterval(
            int? retryCount = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = null,
            [CallerMemberName] string name = null) => 
                new FixedInterval(
                    name,
                    retryCount ?? RetryStrategy.DefaultClientRetryCount,
                    retryInterval ?? RetryStrategy.DefaultRetryInterval,
                    firstFastRetry ?? RetryStrategy.DefaultFirstFastRetry);

        public static void Execute(
            Action action,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null)
        {
            RetryPolicy retryPolicy = new RetryPolicy(
                new ExceptionDetection(isTransient), retryStrategy ?? RetryStrategy.DefaultFixed);
            if (retryingHandler != null)
            {
                retryPolicy.Retrying += retryingHandler;
            }

            retryPolicy.ExecuteAction(action);
        }
    }

    public static partial class EnumerableX
    {
        public static IEnumerable<TSource> Catch<TSource, TException>(
            this IEnumerable<TSource> source, Func<TException, bool> handler = null)
            where TException : Exception
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (true)
                {
                    TSource value;
                    try
                    {
                        if (iterator.MoveNext())
                        {
                            value = iterator.Current;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (TException exception) when (handler?.Invoke(exception) ?? true)
                    {
                        break;
                    }
                    yield return value;
                }
            }
        }

        public static IEnumerable<TSource> Catch<TSource, TException>(
            this IEnumerable<TSource> source, Action<TException> handler = null) where TException : Exception =>
                source.Catch<TSource, TException>(exception =>
                {
                    handler?.Invoke(exception);
                    return true;
                });

        public static IEnumerable<TSource> Catch<TSource>(
            this IEnumerable<TSource> source, Func<Exception, bool> handler = null) => 
                source.Catch<TSource, Exception>(handler);

        public static IEnumerable<TSource> Catch<TSource>(
            this IEnumerable<TSource> source, Action<Exception> handler = null) => 
                source.Catch<TSource, Exception>(handler);

        public static IEnumerable<TResult> TrySelect<TSource, TResult, TException>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector,
            Func<TException, bool?> @catch = null) where TException : Exception
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                int index = -1;
                while (true)
                {
                    TResult result;
                    if (!iterator.MoveNext())
                    {
                        break;
                    }

                    index = checked(index + 1);
                    try
                    {
                        result = selector(iterator.Current, index);
                    }
                    catch (TException exception)
                    {
                        switch (@catch?.Invoke(exception))
                        {
                            case false:
                                break;
                            case true:
                                throw;
                        }
                        continue;
                    }

                    yield return result;
                }
            }
        }

        public static IEnumerable<TResult> TrySelect<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector,
            Func<Exception, bool?> @catch = null) => 
                source.TrySelect<TSource, TResult, Exception>(selector, @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult, TException>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            Func<TException, bool?> @catch = null) where TException : Exception => 
                source.TrySelect(
                    (value, index) => Execute(() => selector(value, index), retryStrategy, isTransient, retryingHandler),
                    @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult, TException>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = true,
            Func<TException, bool?> @catch = null,
            [CallerMemberName] string retryStrategyName = null) where TException : Exception => 
                source.RetrySelect(
                    selector,
                    FixedInterval(retryCount, retryInterval, firstFastRetry, retryStrategyName),
                    isTransient,
                    retryingHandler,
                    @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            Func<Exception, bool?> @catch = null) => 
                source.RetrySelect<TSource, TResult, Exception>(
                    selector, retryStrategy, isTransient, retryingHandler, @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = true,
            Func<Exception, bool?> @catch = null,
            [CallerMemberName] string retryStrategyName = null) => 
                source.RetrySelect(
                    selector,
                    FixedInterval(retryCount, retryInterval, firstFastRetry, retryStrategyName),
                    isTransient,
                    retryingHandler,
                    @catch);

        public static IEnumerable<TResult> TrySelect<TSource, TResult, TException>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            Func<TException, bool?> @catch = null) where TException : Exception
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (true)
                {
                    TResult result;
                    if (!iterator.MoveNext())
                    {
                        break;
                    }

                    try
                    {
                        result = selector(iterator.Current);
                    }
                    catch (TException exception)
                    {
                        switch (@catch?.Invoke(exception))
                        {
                            case false:
                                break;
                            case true:
                                throw;
                        }
                        continue;
                    }

                    yield return result;
                }
            }
        }

        public static IEnumerable<TResult> TrySelect<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            Func<Exception, bool?> @catch = null) => 
                source.TrySelect<TSource, TResult, Exception>(selector, @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult, TException>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            Func<TException, bool?> @catch = null) where TException : Exception => 
                source.TrySelect(
                    value => Execute(() => selector(value), retryStrategy, isTransient, retryingHandler),
                    @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult, TException>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = true,
            Func<TException, bool?> @catch = null,
            [CallerMemberName] string retryStrategyName = null) where TException : Exception => 
                source.RetrySelect(
                    selector,
                    FixedInterval(retryCount, retryInterval, firstFastRetry, retryStrategyName),
                    isTransient,
                    retryingHandler,
                    @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            Func<Exception, bool?> @catch = null) => 
                source.RetrySelect<TSource, TResult, Exception>(
                    selector, retryStrategy, isTransient, retryingHandler, @catch);

        public static IEnumerable<TResult> RetrySelect<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = true,
            Func<Exception, bool?> @catch = null,
            [CallerMemberName] string retryStrategyName = null) => 
                source.RetrySelect(
                    selector,
                    FixedInterval(retryCount, retryInterval, firstFastRetry, retryStrategyName),
                    isTransient,
                    retryingHandler,
                    @catch);

        public static IEnumerable<TSource> Retry<TSource, TException>(
            this IEnumerable<TSource> source,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            Func<TException, bool> @catch = null) where TException : Exception
        {
            List<TSource> results = new List<TSource>();
            try
            {
                Execute(
                    () =>
                    {
                        foreach (TSource value in source.Skip(results.Count))
                        {
                            results.Add(value);
                        }
                    },
                    retryStrategy,
                    isTransient,
                    retryingHandler);
            }
            catch (TException exception) when (@catch?.Invoke(exception) ?? true) { }

            return results.Hide();
        }

        public static IEnumerable<TSource> Retry<TSource, TException>(
            this IEnumerable<TSource> source,
            int retryCount,
            Func<TException, bool> @catch = null) where TException : Exception
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            int count = 0;
            TException lastException = null;
            for (int @try = 0; @try <= retryCount; @try++)
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    for (int index = 0; index < count; index++) // Skip values already evaluated.
                    {
                        try
                        {
                            if (!iterator.MoveNext())
                            {
                                yield break; // End of source.
                            }
                        }
                        catch (TException exception) when (@catch?.Invoke(exception) ?? true)
                        {
                            lastException = exception;
                            break; // Next retry.
                        }
                    }

                    while (true)
                    {
                        TSource value;
                        try
                        {
                            if (!iterator.MoveNext())
                            {
                                yield break; // End of source.
                            }

                            value = iterator.Current;
                        }
                        catch (TException exception) when (@catch?.Invoke(exception) ?? true)
                        {
                            lastException = exception;
                            break; // Next retry.
                        }

                        count = checked(count + 1);
                        yield return value;
                    }
                }
            }

            // yield break is never reached.
            if (@catch == null && lastException != null)
            {
                throw lastException; // Notifies caller evaluation is not done.
            }
        }

        public static IEnumerable<TSource> Retry<TSource>(
            this IEnumerable<TSource> source,
            int retryCount,
            Func<Exception, bool> @catch = null)
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            return source.Retry<TSource, Exception>(retryCount, @catch);
        }
    }
}
