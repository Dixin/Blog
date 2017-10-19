namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal static partial class EnumerableExtensions
    {
        #region Generation

        public static IEnumerable<TResult> Defer<TResult>(Func<IEnumerable<TResult>> enumerableFactory)
        {
            foreach (TResult value in enumerableFactory())
            {
                yield return value; // Deferred execution.
            }
        }

        public static IEnumerable<TResult> Create<TResult>(Func<IEnumerator<TResult>> getEnumerator)
        {
            using (IEnumerator<TResult> iterator = getEnumerator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Current; // Deferred execution.
                }
            }
        }

        public static IEnumerable<T> Create<T>(Action<IYielder<T>> create) => EnumerableEx.Create(create);

        public static IEnumerable<TResult> Return<TResult>(TResult value)
        {
            yield return value; // Deferred execution.
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult value)
        {
            while (true)
            {
                yield return value; // Deferred execution.
            }
        }

        public static IEnumerable<TSource> Repeat<TSource>(this IEnumerable<TSource> source, int? count = null)
        {
            if (count == null)
            {
                while (true)
                {
                    foreach (TSource value in source)
                    {
                        yield return value; // Deferred execution.
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                foreach (TSource value in source)
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        #endregion

        #region Filtering

        public static IEnumerable<TSource> IgnoreElements<TSource>(this IEnumerable<TSource> source)
        {
            foreach (TSource value in source) { } // Eager evaluation.
            yield break; // Deferred execution.
        }

        #endregion

        #region Mapping

        public static IEnumerable<TOther> SelectMany<TSource, TOther>(
            this IEnumerable<TSource> source, IEnumerable<TOther> other) => source.SelectMany(value => other);

        public static IEnumerable<TSource> Scan<TSource>(
            this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break; // Deferred execution.
                }
                TSource accumulate = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return accumulate = func(accumulate, iterator.Current); // Deferred execution.
                }
            }
        }

        public static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(
            this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) =>
                source.Select(value => seed = func(seed, value));

        #endregion

        #region Concatenation

        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<IEnumerable<TSource>> sources) => sources.SelectMany(source => source);

        public static IEnumerable<TSource> Concat<TSource>(
            params IEnumerable<TSource>[] sources) => sources.Concat();

        public static IEnumerable<TSource> StartWith<TSource>(
            this IEnumerable<TSource> source, params TSource[] values) => values.Concat(source);

        #endregion

        #region Set

        public static IEnumerable<TSource> Distinct<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            HashSet<TKey> hashSet = new HashSet<TKey>(comparer);
            foreach (TSource value in source)
            {
                if (hashSet.Add(keySelector(value)))
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        #endregion

        #region Partitioning

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            IEnumerable<TSource> TakeLastGGenerator()
            {
                if (count <= 0)
                {
                    yield break; // Deferred execution.
                }
                Queue<TSource> lastValues = new Queue<TSource>(count);
                foreach (TSource value in source)
                {
                    if (lastValues.Count >= count)
                    {
                        lastValues.Dequeue();
                    }

                    lastValues.Enqueue(value);
                } // Eager evaluation.
                while (lastValues.Count > 0)
                {
                    yield return lastValues.Dequeue(); // Deferred execution.
                }
            }
            return TakeLastGGenerator();
        }

        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            IEnumerable<TSource> SkipLastGenerator()
            {
                Queue<TSource> lastValues = new Queue<TSource>();
                foreach (TSource value in source)
                {
                    lastValues.Enqueue(value);
                    if (lastValues.Count > count) // Can be lazy, eager, or between.
                    {
                        yield return lastValues.Dequeue(); // Deferred execution.
                    }
                }
            }
            return SkipLastGenerator();
        }

        #endregion

        #region Conversion

        public static IEnumerable<TSource> Hide<TSource>(this IEnumerable<TSource> source)
        {
            foreach (TSource value in source)
            {
                yield return value; // Deferred execution.
            }
        }

        #endregion

        #region Buffering

        public static IEnumerable<TResult> Share<TSource, TResult>(
            this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector) => 
                Create(() => selector(source.Share()).GetEnumerator());

        #endregion

        #region Exception

        public static IEnumerable<TResult> Throw<TResult>(Exception exception)
        {
            $"throw {exception}, thread id: {Thread.CurrentThread.ManagedThreadId}".WriteLine();
            throw exception;
#pragma warning disable 162
            yield break;
#pragma warning restore 162
        }

#if DEMO
        // Cannot be compiled.
        public static IEnumerable<TSource> CatchWithYield<TSource, TException>(
            this IEnumerable<TSource> source, Func<TException, IEnumerable<TSource>> handler)
            where TException : Exception
        {
            try
            {
                foreach (TSource value in source)
                {
                    yield return value; // Deferred execution.
                }
            }
            catch (TException exception)
            {
                foreach (TSource value in handler(exception) ?? Empty<TSource>())
                {
                    yield return value; // Deferred execution.
                }
            }
        }
#endif

        public static IEnumerable<TSource> CatchWithYield<TSource, TException>(
            this IEnumerable<TSource> source, Func<TException, IEnumerable<TSource>> handler)
            where TException : Exception => Create<TSource>(async yield =>
        {
            try
            {
                foreach (TSource value in source)
                {
                    await yield.Return(value); // yield return value;
                }
            }
            catch (TException exception)
            {
                foreach (TSource value in handler(exception) ?? Empty<TSource>())
                {
                    await yield.Return(value); // yield return value;
                }
            }
        });

        public static IEnumerable<TSource> Catch<TSource, TException>(
            this IEnumerable<TSource> source, Func<TException, IEnumerable<TSource>> handler)
            where TException : Exception
        {
            TException firstException = null;
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (true)
                {
                    TSource value;
                    try // Only MoveNext and Current are inside try-catch.
                    {
                        if (iterator.MoveNext())
                        {
                            value = iterator.Current;
                        }
                        else
                        {
                            break; // Stops while loop at the end of iteration.
                        }
                    }
                    catch (TException exception)
                    {
                        firstException = exception;
                        break; // Stops while loop if TException is thrown.
                    }
                    yield return value; // Deferred execution, outside try-catch.
                }
            }
            if (firstException != null)
            {
                foreach (TSource value in handler(firstException) ?? Empty<TSource>())
                {
                    yield return value;
                }
            }
        }

#if DEMO
        // Cannot be compiled.
        public static IEnumerable<TSource> CatchWithYield<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
        {
            Exception lastException = null;
            foreach (IEnumerable<TSource> source in sources)
            {
                lastException = null;
                try
                {
                    foreach (TSource value in source)
                    {
                        yield return value; // Deferred execution.
                    }
                    break; // Stops if no exception from current sequence.
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    // Continue with next sequence if there is exception.
                }
            }
            if (lastException != null)
            {
                throw lastException;
            }
        }
#endif

        public static IEnumerable<TSource> Catch<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
             => Create<TSource>(async yield =>
        {
            Exception lastException = null;
            foreach (IEnumerable<TSource> source in sources)
            {
                lastException = null;
                try
                {
                    foreach (TSource value in source)
                    {
                        await yield.Return(value); // yield return value;
                    }
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    continue;
                }

                break;
            }
            if (lastException != null)
            {
                throw lastException;
            }
        });

        public static IEnumerable<TSource> Catch2<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
        {
            Exception lastException = null;
            foreach (IEnumerable<TSource> source in sources)
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    while (true)
                    {
                        lastException = null;
                        TSource value;
                        try // Only MoveNext and Current are inside try-catch.
                        {
                            if (iterator.MoveNext())
                            {
                                value = iterator.Current;
                            }
                            else
                            {
                                break; // Stops while loop at the end of iteration.
                            }
                        }
                        catch (Exception exception)
                        {
                            lastException = exception;
                            break; // Stops while loop if TException is thrown.
                        }
                        yield return value;  // Deferred execution, outside try-catch.
                    }
                }
                if (lastException == null)
                {
                    break; // If no exception, stops evaluating next source; otherwise, continue.
                }
            }
            if (lastException != null)
            {
                throw lastException;
            }
        }

        public static IEnumerable<TSource> Catch<TSource>(params IEnumerable<TSource>[] sources) => sources.Catch();

        public static IEnumerable<TSource> Catch<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                new IEnumerable<TSource>[] { first, second }.Catch();

        public static IEnumerable<TSource> Finally<TSource>(this IEnumerable<TSource> source, Action finalAction)
        {
            try
            {
                foreach (TSource value in source)
                {
                    yield return value; // Deferred execution.
                }
            }
            finally
            {
                finalAction();
            }
        }

        public static IEnumerable<TSource> Retry<TSource>(
            this IEnumerable<TSource> source, int? retryCount = null) => 
                Return(source).Repeat(retryCount).Catch();

#if DEMO
        // Cannot be compiled.
        public static IEnumerable<TSource> OnErrorResumeNextWithYield<TSource>(
            this IEnumerable<IEnumerable<TSource>> sources)
        {
            foreach (IEnumerable<TSource> source in sources)
            {
                try
                {
                    foreach (TSource value in source)
                    {
                        yield return value;
                    }
                }
                catch { }
            }
        }
#endif

        public static IEnumerable<TSource> OnErrorResumeNext<TSource>(IEnumerable<IEnumerable<TSource>> sources)
        {
            foreach (IEnumerable<TSource> source in sources)
            {
                using (IEnumerator<TSource> iterator = source.GetEnumerator())
                {
                    while (true)
                    {
                        TSource value = default;
                        try
                        {
                            if (!iterator.MoveNext())
                            {
                                break;
                            }
                            value = iterator.Current;
                        }
                        catch
                        {
                            break;
                        }
                        yield return value; // Deferred execution.
                    }
                }
            }
        }

        public static IEnumerable<TSource> OnErrorResumeNext<TSource>(
            params IEnumerable<TSource>[] sources) => sources.OnErrorResumeNext();

        public static IEnumerable<TSource> OnErrorResumeNext<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                new IEnumerable<TSource>[] { first, second }.OnErrorResumeNext();

        #endregion

        #region Imperative

        public static IEnumerable<TSource> Using<TSource, TResource>(
            Func<TResource> resourceFactory, Func<TResource, IEnumerable<TSource>> enumerableFactory)
            where TResource : IDisposable
        {
            using (TResource resource = resourceFactory())
            {
                foreach (TSource value in enumerableFactory(resource))
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        public static IEnumerable<TResult> If<TResult>(
            Func<bool> condition, IEnumerable<TResult> thenSource, IEnumerable<TResult> elseSource = null) =>
                Defer(() => condition() ? thenSource : elseSource ?? Enumerable.Empty<TResult>());

        public static IEnumerable<TResult> Case<TValue, TResult>(
            Func<TValue> selector,
            IDictionary<TValue, IEnumerable<TResult>> sources,
            IEnumerable<TResult> defaultSource = null) => 
                Defer(() => sources.TryGetValue(selector(), out IEnumerable<TResult> result)
                    ? result
                    : (defaultSource ?? Enumerable.Empty<TResult>()));

        public static IEnumerable<TResult> While<TResult>(Func<bool> condition, IEnumerable<TResult> source)
        {
            while (condition())
            {
                foreach (TResult value in source)
                {
                    yield return value; // Deferred execution.
                }
            }
        }

        public static IEnumerable<TResult> DoWhile<TResult>(
            this IEnumerable<TResult> source, Func<bool> condition) => source.Concat(While(condition, source));

        public static IEnumerable<TResult> Generate<TState, TResult>(
            TState initialState,
            Func<TState, bool> condition,
            Func<TState, TState> iterate,
            Func<TState, TResult> resultSelector)
        {
            for (TState state = initialState; condition(state); state = iterate(state))
            {
                yield return resultSelector(state); // Deferred execution.
            }
        }
        
        public static IEnumerable<TResult> For<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> resultSelector) =>
                source.SelectMany(resultSelector);

        #endregion

        #region Iteration

        public static IEnumerable<TSource> Do<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (true)
                {
                    TSource value;
                    try
                    {
                        if (!iterator.MoveNext())
                        {
                            break;
                        }
                        value = iterator.Current;
                    }
                    catch (Exception exception)
                    {
                        onError?.Invoke(exception);
                        throw;
                    }
                    onNext(value);
                    yield return value; // Deferred execution.
                }
                onCompleted?.Invoke();
            }
        }

        public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, IObserver<TSource> observer) =>
            Do(source, observer.OnNext, observer.OnError, observer.OnCompleted);

        public static void ForEach<TSource>(/* this */ IEnumerable<TSource> source, Action<TSource> onNext)
        {
            foreach (TSource value in source)
            {
                onNext(value);
            }
        }

        public static void ForEach<TSource>(/* this */ IEnumerable<TSource> source, Action<TSource, int> onNext)
        {
            int index = 0;
            foreach (TSource value in source)
            {
                onNext(value, index);
                index = checked(index + 1); // Not checked in the source code.
            }
        }

        #endregion

        #region Quantifier

        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source) => !source.Any();

        #endregion
    }
}

#if DEMO
namespace System
{
    public interface IObserver<in T>
    {
        void OnCompleted();

        void OnError(Exception error);

        void OnNext(T value);
    }
}
#endif