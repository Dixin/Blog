namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal static partial class EnumerableExtensions
    {
        #region Generation

        internal static IEnumerable<TResult> Defer<TResult>(Func<IEnumerable<TResult>> enumerableFactory)
        {
            foreach (TResult value in enumerableFactory())
            {
                yield return value;
            }
        }

        internal static IEnumerable<TResult> Create<TResult>(Func<IEnumerator<TResult>> getEnumerator)
        {
            using (IEnumerator<TResult> iterator = getEnumerator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Current;
                }
            }
        }

        internal static IEnumerable<T> Create<T>(Action<IYielder<T>> create) => EnumerableEx.Create(create);

        internal static IEnumerable<TResult> Return<TResult>(TResult value)
        {
            yield return value;
        }

        internal static IEnumerable<TResult> Repeat<TResult>(TResult value)
        {
            while (true)
            {
                yield return value;
            }
        }

        internal static IEnumerable<TSource> Repeat<TSource>(this IEnumerable<TSource> source, int? count = null)
        {
            if (count == null)
            {
                while (true)
                {
                    foreach (TSource value in source)
                    {
                        yield return value;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                foreach (TSource value in source)
                {
                    yield return value;
                }
            }
        }

        #endregion

        #region Filtering

        internal static IEnumerable<TSource> IgnoreElements<TSource>(this IEnumerable<TSource> source)
        {
            foreach (TSource _ in source)
            {
            } // Eager evaluation.

            yield break;
        }

        #endregion

        #region Mapping

        internal static IEnumerable<TOther> SelectMany<TSource, TOther>
            (this IEnumerable<TSource> source, IEnumerable<TOther> other) => source.SelectMany(_ => other);

        internal static IEnumerable<TSource> Scan<TSource>(
            this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }

                TSource accumulate = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return accumulate = func(accumulate, iterator.Current);
                }
            }
        }

        internal static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(
            this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) =>
                source.Select(value => seed = func(seed, value));

        #endregion

        #region Concatenation

        internal static IEnumerable<TSource> Concat<TSource>
            (this IEnumerable<IEnumerable<TSource>> sources) => sources.SelectMany(source => source);

        internal static IEnumerable<TSource> Concat<TSource>
            (params IEnumerable<TSource>[] sources) => sources.Concat();

        internal static IEnumerable<TSource> StartWith<TSource>
            (this IEnumerable<TSource> source, params TSource[] values) => values.Concat(source);

        #endregion

        #region Set

        internal static IEnumerable<TSource> Distinct<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            HashSet<TKey> hashSet = new HashSet<TKey>(comparer);
            return source.Where(value => hashSet.Add(keySelector(value)));
        }

        #endregion

        #region Partitioning

        internal static IEnumerable<TSource> TakeLast_<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (count <= 0)
            {
                yield break;
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
                yield return lastValues.Dequeue();
            }
        }

        internal static IEnumerable<TSource> SkipLast_<TSource>(this IEnumerable<TSource> source, int count)
        {
            Queue<TSource> lastValues = new Queue<TSource>();
            foreach (TSource value in source)
            {
                lastValues.Enqueue(value);
                if (lastValues.Count > count) // Can be lazy, eager, or between.
                {
                    yield return lastValues.Dequeue();
                }
            }
        }

        #endregion

        #region Conversion

        internal static IEnumerable<TSource> Hide<TSource>(this IEnumerable<TSource> source)
        {
            foreach (TSource value in source)
            {
                yield return value;
            }
        }

        #endregion

        #region Exception

        internal static IEnumerable<TResult> Throw<TResult>(Exception exception)
        {
            $"throw {exception}, thread id: {Thread.CurrentThread.ManagedThreadId}".WriteLine();
            throw exception;
#pragma warning disable 162
            yield break;
#pragma warning restore 162
        }

#if DEMO
        internal static IEnumerable<TSource> CatchWithYield<TSource, TException>(
            this IEnumerable<TSource> source, Func<TException, IEnumerable<TSource>> handler)
            where TException : Exception
        {
            try
            {
                foreach (TSource value in source)
                {
                    yield return value;
                }
            }
            catch (TException exception)
            {
                foreach (TSource value in handler(exception) ?? Empty<TSource>())
                {
                    yield return value;
                }
            }
        }
#endif

        internal static IEnumerable<TSource> CatchWithYield<TSource, TException>(
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

        internal static IEnumerable<TSource> Catch<TSource, TException>(
            this IEnumerable<TSource> source, Func<TException, IEnumerable<TSource>> handler)
            where TException : Exception
        {
            TException firstException = null;
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (true)
                {
                    TSource value;
                    try // Only MoveNext and Current are in try-catch.
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

                    yield return value; // yield is out of try-catch.
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
        internal static IEnumerable<TSource> CatchWithYield<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
        {
            Exception lastException = null;
            foreach (IEnumerable<TSource> source in sources)
            {
                lastException = null;
                try
                {
                    foreach (TSource value in source)
                    {
                        yield return value;
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

        internal static IEnumerable<TSource> Catch<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
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

        internal static IEnumerable<TSource> Catch2<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
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
                        try // Only MoveNext and Current are in try-catch.
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

                        yield return value;
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

        internal static IEnumerable<TSource> Catch<TSource>(params IEnumerable<TSource>[] sources) => sources.Catch();

        internal static IEnumerable<TSource> Catch<TSource>
            (this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                new IEnumerable<TSource>[] { first, second }.Catch();

        internal static IEnumerable<TSource> Finally<TSource>(this IEnumerable<TSource> source, Action finalAction)
        {
            try
            {
                foreach (TSource value in source)
                {
                    yield return value;
                }
            }
            finally
            {
                finalAction();
            }
        }

        internal static IEnumerable<TSource> Retry<TSource>
            (this IEnumerable<TSource> source, int? retryCount = null) => Return(source).Repeat(retryCount).Catch();

#if DEMO
        internal static IEnumerable<TSource> OnErrorResumeNextWithYield<TSource>(
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
                catch
                {
                }
            }
        }
#endif

        internal static IEnumerable<TSource> OnErrorResumeNext<TSource>(
            this IEnumerable<IEnumerable<TSource>> sources) => Create<TSource>(async yield =>
        {
            foreach (IEnumerable<TSource> source in sources)
            {
                try
                {
                    foreach (TSource value in source)
                    {
                        await yield.Return(value); // yield return value.
                    }
                }
                catch
                {
                }
            }
        });

        internal static IEnumerable<TSource> OnErrorResumeNext<TSource>
            (params IEnumerable<TSource>[] sources) => sources.OnErrorResumeNext();

        internal static IEnumerable<TSource> OnErrorResumeNext<TSource>
            (this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
                new IEnumerable<TSource>[] { first, second }.OnErrorResumeNext();

        #endregion

        #region Imperative

        internal static IEnumerable<TSource> Using<TSource, TResource>(
            Func<TResource> resourceFactory, Func<TResource, IEnumerable<TSource>> enumerableFactory)
            where TResource : IDisposable
        {
            using (TResource resource = resourceFactory())
            {
                foreach (TSource value in enumerableFactory(resource))
                {
                    yield return value;
                }
            }
        }

        internal static IEnumerable<TResult> If<TResult>
            (Func<bool> condition, IEnumerable<TResult> thenSource, IEnumerable<TResult> elseSource = null) =>
                Defer(() => condition() ? thenSource : elseSource ?? Enumerable.Empty<TResult>());

        internal static IEnumerable<TResult> Case<TValue, TResult>(
            Func<TValue> selector,
            IDictionary<TValue, IEnumerable<TResult>> sources,
            IEnumerable<TResult> defaultSource = null) => Defer(() =>
        {
            if (!sources.TryGetValue(selector(), out IEnumerable<TResult> result))
            {
                result = defaultSource ?? Enumerable.Empty<TResult>();
            }

            return result;
        });

        internal static IEnumerable<TResult> While<TResult>(Func<bool> condition, IEnumerable<TResult> source)
        {
            while (condition())
            {
                foreach (TResult value in source)
                {
                    yield return value;
                }
            }
        }

        internal static IEnumerable<TResult> DoWhile<TResult>
            (this IEnumerable<TResult> source, Func<bool> condition) => source.Concat(While(condition, source));

        internal static IEnumerable<TResult> Generate<TState, TResult>(
            TState initialState,
            Func<TState, bool> condition,
            Func<TState, TState> iterate,
            Func<TState, TResult> resultSelector)
        {
            for (TState state = initialState; condition(state); state = iterate(state))
            {
                yield return resultSelector(state);
            }
        }

        internal static IEnumerable<TResult> For<TSource, TResult>
            (IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> resultSelector) =>
                source.SelectMany(resultSelector);

        #endregion

        #region Iteration

        internal static IEnumerable<TSource> Do<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
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

                    onNext?.Invoke(value);
                    yield return value;
                }

                onCompleted?.Invoke();
            }
        }

        internal static void ForEach<TSource>(/* this */ IEnumerable<TSource> source, Action<TSource> onNext)
        {
            foreach (TSource value in source)
            {
                onNext(value);
            }
        }

        internal static void ForEach<TSource>(/* this */ IEnumerable<TSource> source, Action<TSource, int> onNext)
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

        internal static bool IsEmpty<TSource>(this IEnumerable<TSource> source) => !source.Any();

        #endregion
    }
}
