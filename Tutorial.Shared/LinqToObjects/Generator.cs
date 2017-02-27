namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;

    internal static partial class IteratorPattern
    {
        internal static IEnumerable<TSource> FromValue<TSource>(TSource value) =>
            new Sequence<TSource, bool>(
                data: false, // bool isValueIterated = false;
                iteratorFactory: isValueIterated => new Iterator<TSource>(
                    moveNext: () =>
                    {
                        while (!isValueIterated)
                        {
                            isValueIterated = true;
                            return true;
                        }
                        return false;
                    },
                    getCurrent: () => value));
    }

    internal static partial class IteratorPattern
    {
        internal static void ForEachFromValue<TSource>(TSource value)
        {
            foreach (TSource result in FromValue(value))
            {
            }
        }

        internal static void CompiledForEachFromValue<TSource>(TSource value)
        {
            using (IEnumerator<TSource> iterator = FromValue(value).GetEnumerator())
            {
                // bool isValueIterated = false;
                while (iterator.MoveNext()) // moveNext: while (!isValueIterated)
                {
                    // moveNext: isValueIterated = true;
                    TSource result = iterator.Current; // getCurrent: TSource result = value;
                }
            }

            // Virtual control flow when iterating the returned sequence:
            // bool isValueIterated = false;
            // while (!isValueIterated)
            // {
            //    isValueIterated = true;
            //    TSource result = value;
            // }
        }

        internal static IEnumerable<TSource> Repeat<TSource>(TSource value, int count) =>
            new Sequence<TSource, int>(
                data: 0, // int index = 0;
                iteratorFactory: index => new Iterator<TSource>(
                    moveNext: () => index++ < count,
                    getCurrent: () => value));

        internal static void CompiledForEachRepeat<TSource>(TSource value, int count)
        {
            using (IEnumerator<TSource> iterator = Repeat(value, count).GetEnumerator())
            {
                // int index = 0;
                while (iterator.MoveNext()) // moveNext: while (index++ < count)
                {
                    TSource result = iterator.Current; // getCurrent: TSource result = value;
                }
            }

            // Virtual control flow when iterating the returned sequence:
            // int index = 0;
            // while (index++ < count)
            // {
            //    TSource result = value; 
            // }
        }

        internal static IEnumerable<TResult> Select<TSource, TResult>(
                IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
            new Sequence<TResult, IEnumerator<TSource>>(
                iteratorFactory: sourceIterator => new Iterator<TResult>(
                    start: () => sourceIterator = source.GetEnumerator(),
                    moveNext: () => sourceIterator.MoveNext(),
                    getCurrent: () => selector(sourceIterator.Current),
                    dispose: () => sourceIterator?.Dispose()));

        internal static void CompiledForEachSelect<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            using (IEnumerator<TResult> iterator = Select(source, selector).GetEnumerator())
            {
                // IEnumerator<TSource> sourceIterator = null;
                // start: sourceIterator = source.GetEnumerator();
                while (iterator.MoveNext()) // moveNext: while (sourceIterator.MoveNext())
                {
                    TResult result = iterator.Current; // getCurrent: TResult result = selector(sourceIterator.Current);
                }
            } // dispose: sourceIterator?.Dispose();

            // Virtual control flow when iterating the returned sequence:
            // IEnumerator<TSource> sourceIterator = null;
            // try
            // {
            //    sourceIterator = source.GetEnumerator();
            //    while (sourceIterator.MoveNext())
            //    {
            //        TResult result = selector(sourceIterator.Current);
            //    }
            // }
            // finally
            // {
            //    sourceIterator?.Dispose();
            // }
        }

        internal static IEnumerable<TSource> Where<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                new Sequence<TSource, IEnumerator<TSource>>(
                    iteratorFactory: sourceIterator => new Iterator<TSource>(
                        start: () => sourceIterator = source.GetEnumerator(),
                        moveNext: () =>
                        {
                            while (sourceIterator.MoveNext())
                            {
                                if (predicate(sourceIterator.Current))
                                {
                                    return true;
                                }
                            }
                            return false;
                        },
                        getCurrent: () => sourceIterator.Current,
                        dispose: () => sourceIterator?.Dispose()));

        internal static void CompiledForEachWhere<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (IEnumerator<TSource> iterator = Where(source, predicate).GetEnumerator())
            {
                // IEnumerator<TSource> sourceIterator = null;
                // start: sourceIterator = source.GetEnumerator();
                while (iterator.MoveNext()) // moveNext: while (sourceIterator.MoveNext())
                {
                    // moveNext: if (predicate(sourceIterator.Current))
                    TSource result = iterator.Current; // getCurrent: TResult result = sourceIterator.Current;
                }
            } // dispose: sourceIterator?.Dispose();

            // Virtual control flow when iterating the returned sequence:
            // IEnumerator<TSource> sourceIterator = null;
            // try
            // {
            //    sourceIterator = source.GetEnumerator();
            //    while (sourceIterator.MoveNext())
            //    {
            //        if (predicate(sourceIterator.Current))
            //        {
            //            TResult result = selector(sourceIterator.Current);
            //        }
            //    }
            // }
            // finally
            // {
            //    sourceIterator?.Dispose();
            // }
        }

        internal static IEnumerable<TSource> FromValueGenerator<TSource>(TSource value)
        {
            // Virtual control flow when iterating the returned sequence:
            // bool isValueIterated = false;
            // while (!isValueIterated)
            // {
            //    isValueIterated = true;
            //    TSource result = value;
            // }

            bool isValueIterated = false;
            while (!isValueIterated) // moveNext.
            {
                isValueIterated = true; // moveNext.
                yield return value; // getCurrent.
            }
        }

        internal static IEnumerable<TSource> RepeatGenerator<TSource>(TSource value, int count)
        {
            // Virtual control flow when iterating the returned sequence:
            // int index = 0;
            // while (index++ < count)
            // {
            //    TSource result = value; 
            // }

            int index = 0;
            while (index++ < count) // moveNext.
            {
                yield return value; // getCurrent.
            }
        }

        internal static IEnumerable<TResult> SelectGenerator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            // Virtual control flow when iterating the returned sequence:
            // IEnumerator<TSource> sourceIterator = null;
            // try
            // {
            //    sourceIterator = source.GetEnumerator();
            //    while (sourceIterator.MoveNext())
            //    {
            //        TResult result = selector(sourceIterator.Current);
            //    }
            // }
            // finally
            // {
            //    sourceIterator?.Dispose();
            // }

            IEnumerator<TSource> sourceIterator = null;
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    yield return selector(sourceIterator.Current); // getCurrent.
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }
        }

        internal static IEnumerable<TSource> WhereGenerator<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            // Virtual control flow when iterating the returned sequence:
            // IEnumerator<TSource> sourceIterator = null;
            // try
            // {
            //    sourceIterator = source.GetEnumerator();
            //    while (sourceIterator.MoveNext())
            //    {
            //        if (predicate(sourceIterator.Current))
            //        {
            //            TResult result = selector(sourceIterator.Current);
            //        }
            //    }
            // }
            // finally
            // {
            //    sourceIterator?.Dispose();
            // }

            IEnumerator<TSource> sourceIterator = null;
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    if (predicate(sourceIterator.Current)) // moveNext.
                    {
                        yield return sourceIterator.Current; // getCurrent.
                    }
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }
        }

    }

    internal static partial class Generator
    {
        internal static IEnumerable<TSource> FromValueGenerator<TSource>(TSource value)
        {
            yield return value;
        }

        internal static IEnumerable<TSource> RepeatGenerator<TSource>(TSource value, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return value;
            }
        }

        internal static IEnumerable<TResult> SelectGenerator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }

        internal static IEnumerable<TSource> WhereGenerator<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        internal static IEnumerable<TSource> CompiledFromValueGenerator<TSource>(TSource value) =>
            new Generator<TSource, bool>(
                data: false, // bool isValueIterated = false;
                iteratorFactory: isValueIterated => new Iterator<TSource>(
                    moveNext: () =>
                    {
                        while (!isValueIterated)
                        {
                            isValueIterated = true;
                            return true;
                        }
                        return false;
                    },
                    getCurrent: () => value));

        internal static IEnumerable<TSource> CompiledRepeatGenerator<TSource>(TSource value, int count) =>
            new Generator<TSource, int>(
                data: 0, // int index = 0;
                iteratorFactory: index => new Iterator<TSource>(
                    moveNext: () => index++ < count,
                    getCurrent: () => value));

        internal static IEnumerable<TResult> CompiledSelectGenerator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                new Generator<TResult, IEnumerator<TSource>>(
                    data: null, // IEnumerator<TSource> sourceIterator = null;
                    iteratorFactory: sourceIterator => new Iterator<TResult>(
                        start: () => sourceIterator = source.GetEnumerator(),
                        moveNext: () => sourceIterator.MoveNext(),
                        getCurrent: () => selector(sourceIterator.Current),
                        dispose: () => sourceIterator?.Dispose()));

        internal static IEnumerable<TSource> CompiledWhereGenerator<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                new Generator<TSource, IEnumerator<TSource>>(
                    data: null, // IEnumerator<TSource> sourceIterator = null;
                    iteratorFactory: sourceIterator => new Iterator<TSource>(
                        start: () => sourceIterator = source.GetEnumerator(),
                        moveNext: () =>
                        {
                            while (sourceIterator.MoveNext())
                            {
                                if (predicate(sourceIterator.Current))
                                {
                                    return true;
                                }
                            }
                            return false;
                        },
                        getCurrent: () => sourceIterator.Current,
                        dispose: () => sourceIterator?.Dispose()));

        internal static IEnumerator<TSource> FromValueIterator<TSource>(TSource value)
        {
            yield return value;
        }

        internal static IEnumerator<TSource> RepeatIterator<TSource>(TSource value, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return value;
            }
        }

        internal static IEnumerator<TResult> SelectIterator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }

        internal static IEnumerator<TSource> WhereIterator<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        internal static IEnumerator<TSource> CompiledFromValueIterator<TSource>(TSource value)
        {
            bool isValueIterated = false;
            return new Iterator<TSource>(
                moveNext: () =>
                {
                    while (!isValueIterated)
                    {
                        isValueIterated = true;
                        return true;
                    }
                    return false;
                },
                getCurrent: () => value).Start();
        }

        internal static IEnumerator<TSource> CompiledRepeatIterator<TSource>(TSource value, int count)
        {
            int index = 0;
            return new Iterator<TSource>(
                moveNext: () => index++ < count,
                getCurrent: () => value).Start();
        }

        internal static IEnumerator<TResult> CompiledSelectIterator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            IEnumerator<TSource> sourceIterator = null;
            return new Iterator<TResult>(
                start: () => sourceIterator = source.GetEnumerator(),
                moveNext: () => sourceIterator.MoveNext(),
                getCurrent: () => selector(sourceIterator.Current),
                dispose: () => sourceIterator?.Dispose()).Start();
        }

        internal static IEnumerator<TSource> CompiledWhereIterator<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            IEnumerator<TSource> sourceIterator = null;
            return new Iterator<TSource>(
                start: () => sourceIterator = source.GetEnumerator(),
                moveNext: () =>
                {
                    while (sourceIterator.MoveNext())
                    {
                        if (predicate(sourceIterator.Current))
                        {
                            return true;
                        }
                    }
                    return false;
                },
                getCurrent: () => sourceIterator.Current,
                dispose: () => sourceIterator?.Dispose()).Start();
        }
    }
}
