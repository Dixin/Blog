namespace System.Linq
{
    using System.Collections.Generic;

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TSource> Slice<TSource>(this IEnumerable<TSource> source, Range range)
        {
            if (source == null)
            {
                // ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                throw new ArgumentNullException(nameof(source));
            }

            return SilceIterator(source, range);
        }

        private static IEnumerable<TSource> SilceIterator<TSource>(IEnumerable<TSource> source, Range range)
        {
            Index start = range.Start;
            Index end = range.End;

            if (source is IList<TSource> list)
            {
                int count = list.Count;
                int firstIndex = start.FromEnd ? count - start.Value : start.Value;
                int lastIndex = (end.FromEnd ? count - end.Value : end.Value) - 1;
                if (firstIndex >= 0 && lastIndex < count)
                {
                    for (int currentIndex = firstIndex; currentIndex <= lastIndex; currentIndex++)
                    {
                        yield return list[currentIndex];
                    }
                }
                yield break;
            }

            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                int currentIndex = -1;
                if (start.FromEnd)
                {
                    if (e.MoveNext())
                    {
                        Queue<TSource> queue = new Queue<TSource>();
                        queue.Enqueue(e.Current);
                        currentIndex++;

                        int takeLastCount = start.Value;
                        while (e.MoveNext())
                        {
                            if (queue.Count == takeLastCount)
                            {
                                queue.Dequeue();
                            }

                            queue.Enqueue(e.Current);
                            currentIndex++;
                        }

                        if (queue.Count < takeLastCount)
                        {
                            yield break;
                        }

                        int firstIndex = currentIndex + 1 - takeLastCount;
                        int lastIndex = end.FromEnd ? currentIndex - end.Value : end.Value - 1;
                        for (int index = firstIndex; index <= lastIndex; index++)
                        {
                            yield return queue.Dequeue();
                        }
                    }
                }
                else
                {
                    int firstIndex = start.Value;
                    if (!e.MoveNext())
                    {
                        yield break;
                    }

                    currentIndex++;
                    while (currentIndex < firstIndex && e.MoveNext())
                    {
                        currentIndex++;
                    }

                    if (currentIndex != firstIndex)
                    {
                        yield break;
                    }

                    if (end.FromEnd)
                    {
                        int skipLastCount = end.Value;
                        if (skipLastCount > 0)
                        {
                            Queue<TSource> queue = new Queue<TSource>();
                            do
                            {
                                if (queue.Count == skipLastCount)
                                {
                                    yield return queue.Dequeue();
                                }

                                queue.Enqueue(e.Current);
                                currentIndex++;
                            }
                            while (e.MoveNext());
                        }
                        else
                        {
                            do
                            {
                                yield return e.Current;
                                currentIndex++;
                            }
                            while (e.MoveNext());
                        }
                    }
                    else
                    {
                        int lastIndex = end.Value - 1;
                        if (lastIndex <= firstIndex - 1)
                        {
                            yield break;
                        }

                        yield return e.Current;
                        while (currentIndex < lastIndex && e.MoveNext())
                        {
                            currentIndex++;
                            yield return e.Current;
                        }
                    }
                }
            }
        }
    }
}