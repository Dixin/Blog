namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;

    public static partial class EnumerableX
    {
        #region List

        public static IEnumerable<TSource> Insert<TSource>(this IEnumerable<TSource> source, int index, TSource value)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            IEnumerable<TSource> InsertGenerator()
            {
                int currentIndex = 0;
                foreach (TSource sourceValue in source)
                {
                    if (currentIndex == index)
                    {
                        yield return value; // Deferred execution.
                    }
                    yield return sourceValue; // Deferred execution.
                    currentIndex = checked(currentIndex + 1);
                }
                if (index == currentIndex)
                {
                    yield return value; // Deferred execution.
                }
                else if (index > currentIndex)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        $"{nameof(index)} must be within the bounds of {nameof(source)}.");
                }
            }
            return InsertGenerator();
        }

        public static IEnumerable<TSource> RemoveAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            IEnumerable<TSource> RemoveAtGenerator()
            {
                int currentIndex = 0;
                foreach (TSource value in source)
                {
                    if (currentIndex != index)
                    {
                        yield return value; // Deferred execution.
                    }
                    currentIndex = checked(currentIndex + 1);
                }
                if (index >= currentIndex)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            return RemoveAtGenerator();
        }

        public static IEnumerable<TSource> Remove<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            bool isRemoved = false;
            foreach (TSource sourceValue in source)
            {
                if (!isRemoved && comparer.Equals(sourceValue, value))
                {
                    isRemoved = true;
                }
                else
                {
                    yield return sourceValue; // Deferred execution.
                }
            }
        }

        public static IEnumerable<TSource> RemoveAll<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            foreach (TSource sourceValue in source)
            {
                if (!comparer.Equals(sourceValue, value))
                {
                    yield return sourceValue; // Deferred execution.
                }
            }
        }

        public static int IndexOf<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            int startIndex = 0,
            int? count = null,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            source = source.Skip(startIndex);
            if (count != null)
            {
                source = source.Take(count.Value);
            }
            int index = checked(0 + startIndex);
            foreach (TSource sourceValue in source)
            {
                if (comparer.Equals(sourceValue, value))
                {
                    return index;
                }
                index = checked(index + 1);
            }
            return -1;
        }

        public static int LastIndexOf<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            int startIndex = 0,
            int? count = null,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            source = source.Skip(startIndex);
            if (count != null)
            {
                source = source.Take(count.Value);
            }
            int lastIndex = -1;
            int index = checked(0 + startIndex);
            foreach (TSource sourceValue in source)
            {
                if (comparer.Equals(sourceValue, value))
                {
                    lastIndex = index;
                }
                index = checked(index + 1);
            }
            return lastIndex;
        }

        #endregion
    }
}
