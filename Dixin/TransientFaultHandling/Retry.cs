namespace Dixin.TransientFaultHandling
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Dixin.Common;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    public static partial class Retry
    {
        public static TResult Execute<TResult>(
            Func<TResult> func,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null)
        {
            func.NotNull(nameof(func));

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
        public static TResult Execute<TResult>(
            Func<TResult> func,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = null,
            [CallerMemberName] string name = null)
        {
            func.NotNull(nameof(func));

            return Execute(
                func,
                FixedInterval(retryCount, retryInterval, firstFastRetry, name),
                isTransient,
                retryingHandler);
        }

        public static FixedInterval FixedInterval(
            int? retryCount = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = null,
            [CallerMemberName] string name = null) => new FixedInterval(
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
            action.NotNull(nameof(action));

            RetryPolicy retryPolicy = new RetryPolicy(
                new ExceptionDetection(isTransient), retryStrategy ?? RetryStrategy.DefaultFixed);
            if (retryingHandler != null)
            {
                retryPolicy.Retrying += retryingHandler;
            }

            retryPolicy.ExecuteAction(action);
        }

        public static void Execute(
            Action action,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = null,
            [CallerMemberName] string name = null)
        {
            action.NotNull(nameof(action));

            Execute(
                action,
                FixedInterval(retryCount, retryInterval, firstFastRetry, name),
                isTransient,
                retryingHandler);
        }

        public static Task<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> func,
            RetryStrategy retryStrategy = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null)
        {
            func.NotNull(nameof(func));

            RetryPolicy retryPolicy = new RetryPolicy(
                new ExceptionDetection(isTransient), retryStrategy ?? RetryStrategy.DefaultFixed);
            if (retryingHandler != null)
            {
                retryPolicy.Retrying += retryingHandler;
            }

            return retryPolicy.ExecuteAsync(func);
        }

        public static Task<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> func,
            int? retryCount = null,
            Func<Exception, bool> isTransient = null,
            EventHandler<RetryingEventArgs> retryingHandler = null,
            TimeSpan? retryInterval = null,
            bool? firstFastRetry = null,
            [CallerMemberName] string name = null)
        {
            func.NotNull(nameof(func));

            return ExecuteAsync(
                func,
                FixedInterval(retryCount, retryInterval, firstFastRetry, name),
                isTransient,
                retryingHandler);
        }
    }
}
