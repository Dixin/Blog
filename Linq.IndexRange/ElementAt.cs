namespace System.Linq
{
    using System;
    using System.Collections.Generic;

    public static partial class EnumerableExtensions
    {
        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, Index index)
        {
            if (source == null)
            {
                // ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                throw new ArgumentNullException(nameof(source));
            }

            if (!index.FromEnd)
            {
                return source.ElementAt(index.Value);
            }

            int indexFromEnd = index.Value;
            if (indexFromEnd > 0)
            {
                if (source is IList<TSource> list)
                {
                    return list[list.Count - indexFromEnd];
                }

                using (IEnumerator<TSource> e = source.GetEnumerator())
                {
                    if (e.MoveNext())
                    {
                        Queue<TSource> queue = new Queue<TSource>();
                        queue.Enqueue(e.Current);
                        while (e.MoveNext())
                        {
                            if (queue.Count == indexFromEnd)
                            {
                                queue.Dequeue();
                            }

                            queue.Enqueue(e.Current);
                        }

                        if (queue.Count == indexFromEnd)
                        {
                            return queue.Dequeue();
                        }
                    }
                }
            }

            // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
            throw new ArgumentOutOfRangeException(nameof(index));
            return default!;
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, Index index)
        {
            if (source == null)
            {
                // ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                throw new ArgumentNullException(nameof(source));

            }

            if (!index.FromEnd)
            {
                return source.ElementAtOrDefault(index.Value);
            }

            int indexFromEnd = index.Value;
            if (indexFromEnd > 0)
            {
                if (source is IList<TSource> list)
                {
                    int count = list.Count;
                    if (count >= indexFromEnd)
                    {
                        return list[count - indexFromEnd];
                    }
                }

                using (IEnumerator<TSource> e = source.GetEnumerator())
                {
                    if (e.MoveNext())
                    {
                        Queue<TSource> queue = new Queue<TSource>();
                        queue.Enqueue(e.Current);
                        while (e.MoveNext())
                        {
                            if (queue.Count == indexFromEnd)
                            {
                                queue.Dequeue();
                            }

                            queue.Enqueue(e.Current);
                        }

                        if (queue.Count == indexFromEnd)
                        {
                            return queue.Dequeue();
                        }
                    }
                }
            }

            return default!;
        }
    }
}